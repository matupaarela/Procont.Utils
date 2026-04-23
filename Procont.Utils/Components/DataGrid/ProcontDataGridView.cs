using Procont.Utils.Core.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Windows.Forms;

namespace Procont.Utils.Components.DataGrid
{
    /// <summary>
    /// DataGridView con dos capacidades integradas:
    ///
    /// ── 1. GROUPED COLUMN HEADERS ───────────────────────────────────
    ///   Añade un nivel de encabezado agrupador por encima de los headers normales.
    ///
    ///   grid.ColumnGroups.Add(new DataGridColumnGroup
    ///   {
    ///       Title           = "Período Actual",
    ///       ColumnNamesText = "colEnero, colFebrero, colMarzo"
    ///   });
    ///   grid.ApplyColumnGroups();
    ///
    /// ── 2. FOOTER ROWS (integrado) ──────────────────────────────────
    ///   Las filas de footer se pintan dentro del propio control.
    ///   No hace falta añadir ningún control hermano.
    ///
    ///   grid.FooterRows.Add(new DataGridFooterRow
    ///   {
    ///       Cells =
    ///       {
    ///           new DataGridFooterCell { ColumnName = "Importe", Formula = FooterFormula.Sum, Format = "N2" },
    ///           new DataGridFooterCell { ColumnName = "Label",   Text = "TOTAL" }
    ///       }
    ///   });
    ///   grid.RebuildFooter();
    ///
    /// ── NOTAS ───────────────────────────────────────────────────────
    ///   • El componente NO aplica estilos propios; el aspecto visual
    ///     queda completamente en manos del consumidor.
    ///   • ApplyColumnGroups() se llama automáticamente al añadir/quitar
    ///     columnas cuando ya hay grupos configurados.
    ///   • RebuildFooter() debe llamarse tras modificar FooterRows,
    ///     o se aplica automáticamente en OnHandleCreated si ya hay filas.
    ///   • Invalidate() recalcula las fórmulas del footer.
    /// </summary>
    [ToolboxItem(true)]
    [Description("DataGridView con cabeceras agrupadas y filas de footer integradas.")]
    public class ProcontDataGridView : System.Windows.Forms.DataGridView
    {
        // ── Mensajes Windows ───────────────────────────────────────────
        private const int WmPaint = 0x000F;
        private const int WM_VSCROLL = 0x0115;
        private const int WM_HSCROLL = 0x0114;

        // ── Column groups ──────────────────────────────────────────────
        private const int DefaultGroupHeaderHeight = 24;
        private readonly List<DataGridColumnGroup> _columnGroups = new List<DataGridColumnGroup>();
        private bool _groupsApplied = false;
        private int _baseHeaderHeight = DefaultGroupHeaderHeight;

        // ── Footer ─────────────────────────────────────────────────────
        private readonly List<DataGridFooterRow> _footerRows = new List<DataGridFooterRow>();

        // ── Theming override (opcional) ────────────────────────────────
        private Color _groupHeaderBackColor = Color.Empty;
        private Color _columnHeaderBackColor = Color.Empty;

        // ══════════════════════════════════════════════════════════════
        // PROPIEDADES
        // ══════════════════════════════════════════════════════════════

        [Category("ProcontDataGrid")]
        [Description("Grupos de columnas. Usar ColumnNamesText para listar columnas.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public List<DataGridColumnGroup> ColumnGroups => _columnGroups;

        [Category("ProcontDataGrid")]
        [Description("Filas de footer con totales/promedios. Llamar RebuildFooter() tras configurar.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public List<DataGridFooterRow> FooterRows => _footerRows;

        [Category("ProcontDataGrid — Theming")]
        [Description("Fondo del header de grupo. Empty = SystemColors.ControlDark.")]
        public Color GroupHeaderBackColor
        {
            get => _groupHeaderBackColor;
            set { _groupHeaderBackColor = value; Invalidate(); }
        }

        [Category("ProcontDataGrid — Theming")]
        [Description("Fondo del header de columna (mitad inferior). Empty = SystemColors.Control.")]
        public Color ColumnHeaderBackColor
        {
            get => _columnHeaderBackColor;
            set { _columnHeaderBackColor = value; Invalidate(); }
        }

        private Color ResolvedGroupHeaderBg =>
            _groupHeaderBackColor == Color.Empty ? SystemColors.ControlDark : _groupHeaderBackColor;

        private Color ResolvedColumnHeaderBg =>
            _columnHeaderBackColor == Color.Empty ? SystemColors.Control : _columnHeaderBackColor;

        private int FooterTotalHeight
        {
            get { int h = 0; foreach (var r in _footerRows) h += r.Height; return h; }
        }

        // ══════════════════════════════════════════════════════════════
        // CONSTRUCTOR
        // ══════════════════════════════════════════════════════════════

        public ProcontDataGridView()
        {
            // OBLIGATORIO: sin esto, asignar ColumnHeadersHeight lanza excepción
            ColumnHeadersHeightSizeMode =
                DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

            // OBLIGATORIO: sin esto, visual styles anulan OnCellPainting en headers
            EnableHeadersVisualStyles = false;

            // Activar double buffer real vía reflexión (DataGridView sella la prop pública)
            typeof(Control)
                .GetProperty("DoubleBuffered",
                    BindingFlags.Instance | BindingFlags.NonPublic)
                ?.SetValue(this, true);

            // Re-pintar footer cuando el DGV hace scroll vertical
            Scroll += OnInternalScroll;
        }

        // ── WS_EX_COMPOSITED: toda la pintura se hace fuera de pantalla ───
        // Elimina el BitBlt de scroll que causaba el flicker del footer.
        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= 0x02000000; // WS_EX_COMPOSITED
                return cp;
            }
        }

        // ══════════════════════════════════════════════════════════════
        // INIT — auto-apply desde el diseñador
        // ══════════════════════════════════════════════════════════════

        /// <summary>
        /// Aplica automáticamente el footer y los grupos al crearse el handle,
        /// resolviendo el caso del diseñador donde RebuildFooter() no se llama
        /// explícitamente (solo se serializan las colecciones FooterRows / ColumnGroups).
        /// </summary>
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            if (_columnGroups.Count > 0 && !_groupsApplied)
                ApplyColumnGroups();

            if (_footerRows.Count > 0)
                RebuildFooter();
        }

        // ══════════════════════════════════════════════════════════════
        // COLUMN GROUPS — setup
        // ══════════════════════════════════════════════════════════════

        /// <summary>
        /// Dobla la altura del header para acomodar la fila de grupo.
        /// Llamar tras agregar o modificar ColumnGroups.
        /// También se llama automáticamente al agregar/quitar columnas
        /// cuando ya hay grupos configurados, y desde OnHandleCreated.
        /// </summary>
        public void ApplyColumnGroups()
        {
            if (ColumnHeadersHeightSizeMode != DataGridViewColumnHeadersHeightSizeMode.DisableResizing)
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

            if (_columnGroups.Count == 0)
            {
                if (_groupsApplied)
                {
                    ColumnHeadersHeight = _baseHeaderHeight;
                    _groupsApplied = false;
                    Invalidate();
                }
                return;
            }

            if (!_groupsApplied)
                _baseHeaderHeight = Math.Max(DefaultGroupHeaderHeight, ColumnHeadersHeight);

            int targetH = _baseHeaderHeight * 2;
            if (ColumnHeadersHeight != targetH)
                ColumnHeadersHeight = targetH;

            _groupsApplied = true;
            Invalidate();
        }

        protected override void OnColumnAdded(DataGridViewColumnEventArgs e)
        {
            base.OnColumnAdded(e);
            if (_columnGroups.Count > 0) ApplyColumnGroups();
        }

        protected override void OnColumnRemoved(DataGridViewColumnEventArgs e)
        {
            base.OnColumnRemoved(e);
            if (_columnGroups.Count > 0) ApplyColumnGroups();
        }

        // ══════════════════════════════════════════════════════════════
        // FOOTER — setup
        // ══════════════════════════════════════════════════════════════

        /// <summary>
        /// Reserva espacio (Padding.Bottom) para el footer y fuerza repintado.
        /// Llamar tras modificar FooterRows.
        /// También se aplica automáticamente desde OnHandleCreated si ya hay filas.
        /// </summary>
        public void RebuildFooter()
        {
            int fh = FooterTotalHeight;
            var p = Padding;
            if (p.Bottom != fh)
                Padding = new Padding(p.Left, p.Top, p.Right, fh);
            Invalidate();
        }

        /// <summary>
        /// [Compatibilidad] Crea un DataGridFooterBar externo pre-conectado.
        /// Para uso integrado (recomendado) usar FooterRows + RebuildFooter().
        /// </summary>
        public DataGridFooterBar CreateFooterBar() => new DataGridFooterBar(this);

        // ══════════════════════════════════════════════════════════════
        // FOOTER — scroll repaint
        // ══════════════════════════════════════════════════════════════

        private void OnInternalScroll(object sender, ScrollEventArgs e)
        {
            InvalidateFooterStrip();
        }

        private void InvalidateFooterStrip()
        {
            int fh = FooterTotalHeight;
            if (fh <= 0) return;
            Invalidate(new Rectangle(0, ClientSize.Height - fh, ClientSize.Width, fh));
        }

        // ══════════════════════════════════════════════════════════════
        // WndProc — footer pintado + scroll repaint
        // ══════════════════════════════════════════════════════════════

        protected override void WndProc(ref Message m)
        {
            // Interceptar scroll: forzar repaint del footer DESPUÉS del scroll del DGV
            if ((m.Msg == WM_VSCROLL || m.Msg == WM_HSCROLL) && FooterTotalHeight > 0)
            {
                base.WndProc(ref m);
                InvalidateFooterStrip();
                Update(); // repaint inmediato para evitar frame con footer desplazado
                return;
            }

            base.WndProc(ref m);

            if (m.Msg != WmPaint) return;
            int fh = FooterTotalHeight;
            if (fh <= 0 || _footerRows.Count == 0) return;

            // Graphics.FromHwnd obtiene el DC actual después del EndPaint del base.
            // Con WS_EX_COMPOSITED esto se hace en el buffer off-screen, sin flicker.
            using (var g = Graphics.FromHwnd(Handle))
            {
                g.SetHighQuality();
                int footerTop = ClientSize.Height - fh;

                using (var pen = new Pen(SystemColors.ControlDark, 1))
                    g.DrawLine(pen, 0, footerTop, ClientSize.Width, footerTop);

                int rowY = footerTop + 1;
                foreach (var row in _footerRows)
                {
                    PaintFooterRow(g, row, rowY);
                    rowY += row.Height;
                }
            }
        }

        private void PaintFooterRow(Graphics g, DataGridFooterRow row, int y)
        {
            g.FillRectangle(SystemBrushes.ControlDark,
                new Rectangle(0, y, ClientSize.Width, row.Height - 1));

            if (row.BackColor != Color.Empty)
                using (var fill = new SolidBrush(row.BackColor))
                    g.FillRectangle(fill, 0, y, ClientSize.Width, row.Height - 1);

            using (var pen = new Pen(SystemColors.ControlDarkDark, 1))
                g.DrawLine(pen, 0, y + row.Height - 1, ClientSize.Width, y + row.Height - 1);

            foreach (var cell in row.Cells)
            {
                if (string.IsNullOrEmpty(cell.ColumnName)) continue;

                var col = Columns[cell.ColumnName];
                if (col == null || !col.Visible) continue;

                var colRect = GetColumnDisplayRectangle(col.Index, false);
                if (colRect.IsEmpty) continue;

                var cellRect = new Rectangle(colRect.X, y, colRect.Width - 1, row.Height - 1);
                string text = ComputeFooterCellValue(cell, col);

                var font = cell.Bold ? new Font(Font, FontStyle.Bold) : Font;
                Color fg = cell.ForeColor == Color.Empty ? SystemColors.ControlText : cell.ForeColor;

                using (var fmt = BuildStringFormat(cell.Alignment))
                using (var brush = new SolidBrush(fg))
                    g.DrawString(text, font, brush,
                        new RectangleF(cellRect.X + 4, cellRect.Y, cellRect.Width - 8, cellRect.Height),
                        fmt);

                if (cell.Bold) font.Dispose();

                using (var pen = new Pen(SystemColors.ControlDarkDark, 1))
                    g.DrawLine(pen, cellRect.Right, y, cellRect.Right, y + row.Height - 1);
            }
        }

        // ══════════════════════════════════════════════════════════════
        // FOOTER — cálculo de valores
        // ══════════════════════════════════════════════════════════════

        private string ComputeFooterCellValue(DataGridFooterCell cell, DataGridViewColumn col)
        {
            if (cell.Formula == FooterFormula.Custom)
                return cell.Text ?? "";

            double result = 0;
            int count = 0;

            foreach (DataGridViewRow row in Rows)
            {
                if (row.IsNewRow) continue;
                var v = row.Cells[col.Index].Value;
                if (v == null || v == DBNull.Value) continue;
                if (!double.TryParse(v.ToString(), out double d)) continue;

                count++;
                switch (cell.Formula)
                {
                    case FooterFormula.Sum: result += d; break;
                    case FooterFormula.Average: result += d; break;
                    case FooterFormula.Count: result = count; break;
                    case FooterFormula.Min: result = count == 1 ? d : Math.Min(result, d); break;
                    case FooterFormula.Max: result = count == 1 ? d : Math.Max(result, d); break;
                }
            }

            if (cell.Formula == FooterFormula.Average && count > 0) result /= count;
            if (cell.Formula == FooterFormula.Count) result = count;

            string formatted = string.IsNullOrEmpty(cell.Format)
                ? result.ToString()
                : result.ToString(cell.Format);

            return string.IsNullOrEmpty(cell.Text) ? formatted : $"{cell.Text} {formatted}";
        }

        // ══════════════════════════════════════════════════════════════
        // COLUMN GROUPS — pintado
        // ══════════════════════════════════════════════════════════════

        protected override void OnCellPainting(DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex != -1 || e.ColumnIndex < 0 || !_groupsApplied)
            {
                base.OnCellPainting(e);
                return;
            }

            e.Handled = true;
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int fullH = e.CellBounds.Height;
            int groupH = fullH / 2;
            int colH = fullH - groupH;
            int bx = e.CellBounds.X;
            int by = e.CellBounds.Y;
            int bw = e.CellBounds.Width;

            // ── Mitad inferior: nombre de columna ────────────────────
            PaintColumnNameCell(g,
                new Rectangle(bx, by + groupH, bw, colH),
                e.Value?.ToString() ?? "",
                e.ColumnIndex);

            // ── Mitad superior: zona de grupo ────────────────────────
            var topRect = new Rectangle(bx, by, bw, groupH);
            var group = FindGroupForColumn(e.ColumnIndex);

            if (group != null)
            {
                Color groupBg = group.BackColor == Color.Empty
                    ? ResolvedGroupHeaderBg
                    : group.BackColor;

                using (var fill = new SolidBrush(groupBg))
                    g.FillRectangle(fill, topRect);

                if (!IsLastInGroup(e.ColumnIndex, group))
                {
                    using (var pen = new Pen(Color.FromArgb(60, SystemColors.ControlDarkDark), 1))
                        g.DrawLine(pen, topRect.Right - 1, topRect.Y + 3,
                                        topRect.Right - 1, topRect.Bottom - 3);
                }

                if (IsLastInGroup(e.ColumnIndex, group))
                {
                    var spanRect = ComputeGroupSpanRect(group, by, groupH);
                    if (!spanRect.IsEmpty)
                    {
                        var savedClip = g.Clip.Clone();
                        g.ResetClip();

                        using (var fill = new SolidBrush(groupBg))
                            g.FillRectangle(fill, spanRect);

                        using (var brush = new SolidBrush(SystemColors.ControlText))
                        using (var fmt = new StringFormat
                        {
                            Alignment = StringAlignment.Center,
                            LineAlignment = StringAlignment.Center,
                            Trimming = StringTrimming.EllipsisCharacter,
                            FormatFlags = StringFormatFlags.NoWrap
                        })
                            g.DrawString(group.Title, Font, brush, spanRect, fmt);

                        using (var pen = new Pen(SystemColors.ControlDarkDark, 1))
                            g.DrawLine(pen, spanRect.X, spanRect.Bottom - 1,
                                            spanRect.Right, spanRect.Bottom - 1);

                        using (var pen = new Pen(SystemColors.ControlDarkDark, 1))
                            g.DrawLine(pen, spanRect.Right - 1, spanRect.Y,
                                            spanRect.Right - 1, spanRect.Bottom);

                        g.Clip = savedClip;
                    }
                }
            }
            else
            {
                using (var fill = new SolidBrush(ResolvedGroupHeaderBg))
                    g.FillRectangle(fill, topRect);

                using (var pen = new Pen(SystemColors.ControlDarkDark, 1))
                    g.DrawLine(pen, topRect.Right - 1, topRect.Y,
                                    topRect.Right - 1, topRect.Bottom);
            }
        }

        private void PaintColumnNameCell(Graphics g, Rectangle rect, string text, int colIndex)
        {
            using (var fill = new SolidBrush(ResolvedColumnHeaderBg))
                g.FillRectangle(fill, rect);

            using (var pen = new Pen(SystemColors.ControlDarkDark, 1))
            {
                g.DrawLine(pen, rect.Right - 1, rect.Y, rect.Right - 1, rect.Bottom);
                g.DrawLine(pen, rect.X, rect.Bottom - 1, rect.Right, rect.Bottom - 1);
            }

            var col = Columns[colIndex];
            bool sorted = col.HeaderCell.SortGlyphDirection != SortOrder.None;
            float rightPad = sorted ? 20 : 6;

            using (var b = new SolidBrush(SystemColors.ControlText))
            using (var fmt = new StringFormat
            {
                Alignment = sorted ? StringAlignment.Near : StringAlignment.Center,
                LineAlignment = StringAlignment.Center,
                Trimming = StringTrimming.EllipsisCharacter,
                FormatFlags = StringFormatFlags.NoWrap
            })
                g.DrawString(text, Font, b,
                    new RectangleF(rect.X + 4, rect.Y, rect.Width - rightPad, rect.Height),
                    fmt);

            if (sorted)
            {
                int ax = rect.Right - 14;
                int ay = rect.Y + rect.Height / 2 - 3;
                bool asc = col.HeaderCell.SortGlyphDirection == SortOrder.Ascending;
                using (var pen = new Pen(SystemColors.GrayText, 1.5f)
                { StartCap = LineCap.Round, EndCap = LineCap.Round })
                {
                    if (asc)
                    {
                        g.DrawLine(pen, ax, ay + 5, ax + 4, ay);
                        g.DrawLine(pen, ax + 4, ay, ax + 8, ay + 5);
                    }
                    else
                    {
                        g.DrawLine(pen, ax, ay, ax + 4, ay + 5);
                        g.DrawLine(pen, ax + 4, ay + 5, ax + 8, ay);
                    }
                }
            }
        }

        // ══════════════════════════════════════════════════════════════
        // HELPERS — group lookup
        // ══════════════════════════════════════════════════════════════

        private DataGridColumnGroup FindGroupForColumn(int colIndex)
        {
            if (colIndex < 0 || colIndex >= Columns.Count) return null;
            string name = Columns[colIndex].Name;
            foreach (var grp in _columnGroups)
                if (grp.ColumnNames.Contains(name)) return grp;
            return null;
        }

        private bool IsLastInGroup(int colIndex, DataGridColumnGroup group)
        {
            int maxDi = int.MinValue, lastIdx = -1;
            foreach (var name in group.ColumnNames)
            {
                var col = Columns[name];
                if (col == null || !col.Visible) continue;
                if (col.DisplayIndex > maxDi) { maxDi = col.DisplayIndex; lastIdx = col.Index; }
            }
            return lastIdx == colIndex;
        }

        private Rectangle ComputeGroupSpanRect(DataGridColumnGroup group, int y, int height)
        {
            int minX = int.MaxValue, maxRight = int.MinValue;
            foreach (var name in group.ColumnNames)
            {
                var col = Columns[name];
                if (col == null || !col.Visible) continue;
                var r = GetColumnDisplayRectangle(col.Index, false);
                if (r.IsEmpty) continue;
                if (r.X < minX) minX = r.X;
                if (r.Right > maxRight) maxRight = r.Right;
            }
            return minX == int.MaxValue
                ? Rectangle.Empty
                : new Rectangle(minX, y, maxRight - minX, height);
        }

        // ══════════════════════════════════════════════════════════════
        // HELPERS — StringFormat
        // ══════════════════════════════════════════════════════════════

        private static StringFormat BuildStringFormat(ContentAlignment a)
        {
            var fmt = new StringFormat
            {
                Trimming = StringTrimming.EllipsisCharacter,
                FormatFlags = StringFormatFlags.NoWrap
            };

            switch (a)
            {
                case ContentAlignment.TopLeft:
                case ContentAlignment.MiddleLeft:
                case ContentAlignment.BottomLeft:
                    fmt.Alignment = StringAlignment.Near; break;
                case ContentAlignment.TopCenter:
                case ContentAlignment.MiddleCenter:
                case ContentAlignment.BottomCenter:
                    fmt.Alignment = StringAlignment.Center; break;
                default:
                    fmt.Alignment = StringAlignment.Far; break;
            }

            switch (a)
            {
                case ContentAlignment.TopLeft:
                case ContentAlignment.TopCenter:
                case ContentAlignment.TopRight:
                    fmt.LineAlignment = StringAlignment.Near; break;
                case ContentAlignment.BottomLeft:
                case ContentAlignment.BottomCenter:
                case ContentAlignment.BottomRight:
                    fmt.LineAlignment = StringAlignment.Far; break;
                default:
                    fmt.LineAlignment = StringAlignment.Center; break;
            }

            return fmt;
        }
    }
}