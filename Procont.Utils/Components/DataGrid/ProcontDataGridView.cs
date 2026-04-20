using Procont.Utils.Core.Extensions;
using Procont.Utils.Core.Theming;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Procont.Utils.Components.DataGrid
{
    /// <summary>
    /// DataGridView extendido con dos funcionalidades integradas:
    ///
    /// ── 1. GROUPED COLUMN HEADERS ───────────────────────────────────
    ///   Pinta un nivel de encabezado agrupador ENCIMA de los headers
    ///   estándar. Soporta spans multi-columna con color propio.
    ///
    ///   CÓDIGO:
    ///     grid.ColumnGroups.Add(new DataGridColumnGroup
    ///     {
    ///         Title           = "Período Actual",
    ///         BackColor       = Color.FromArgb(20, 245, 168, 30),
    ///         ColumnNamesText = "colUnits, colPrice, colTotal"
    ///     });
    ///     // ApplyColumnGroups() se llama solo; también se puede forzar.
    ///
    ///   DISEÑADOR:
    ///     Properties → ColumnGroups → [...] → Add → establecer Title y ColumnNamesText.
    ///
    /// ── 2. FOOTER ROWS (integrado) ──────────────────────────────────
    ///   Las filas de footer se pintan DENTRO del propio DataGridView
    ///   (vía WM_PAINT + Padding.Bottom). No necesitas añadir ningún
    ///   control extra al formulario.
    ///
    ///   CÓDIGO:
    ///     grid.FooterRows.Add(new DataGridFooterRow
    ///     {
    ///         Cells =
    ///         {
    ///             new DataGridFooterCell { ColumnName = "Importe", Formula = FooterFormula.Sum, Format = "N2" },
    ///             new DataGridFooterCell { ColumnName = "Descripcion", Text = "TOTAL" }
    ///         }
    ///     });
    ///     grid.RebuildFooter();   // ← llamar después de agregar filas
    ///
    ///   DISEÑADOR:
    ///     Properties → FooterRows → [...] → Add → configurar celdas.
    ///     Llamar RebuildFooter() en Form.Load.
    ///
    /// ── NOTAS DE INTEGRACIÓN ────────────────────────────────────────
    ///   • ColumnNamesText usa nombres separados por coma (NO List de strings).
    ///   • ApplyColumnGroups() se llama automáticamente al agregar columnas.
    ///   • RebuildFooter() debe llamarse manualmente tras configurar FooterRows.
    ///   • Invalidate() del grid actualiza los cálculos del footer.
    /// </summary>
    [ToolboxItem(true)]
    [Description("DataGridView con cabeceras agrupadas y filas de footer integradas.")]
    public class ProcontDataGridView : System.Windows.Forms.DataGridView
    {
        // ── Constantes ────────────────────────────────────────────────
        private const int GroupHeaderHeight = 24;
        private const int WM_PAINT = 0x000F;

        // ── Column groups ─────────────────────────────────────────────
        private readonly List<DataGridColumnGroup> _columnGroups = new List<DataGridColumnGroup>();
        private bool _groupsApplied = false;

        // ── Footer ────────────────────────────────────────────────────
        private readonly List<DataGridFooterRow> _footerRows = new List<DataGridFooterRow>();

        // ── Visual config ──────────────────────────────────────────────
        private Color _headerBackColor = Color.Transparent;
        private Color _groupHeaderBackColor = Color.Transparent;

        // ══════════════════════════════════════════════════════════════
        // PROPIEDADES PÚBLICAS
        // ══════════════════════════════════════════════════════════════

        [Category("ProcontDataGrid")]
        [Description("Grupos de columnas. Serializable desde el diseñador — usar ColumnNamesText.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public List<DataGridColumnGroup> ColumnGroups => _columnGroups;

        [Category("ProcontDataGrid")]
        [Description("Filas de footer con totales/promedios. Llamar RebuildFooter() tras configurar.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public List<DataGridFooterRow> FooterRows => _footerRows;

        [Category("ProcontDataGrid — Theming")]
        [Description("Fondo de header de columna. Transparent = ProcontTheme.SurfaceActive.")]
        public Color HeaderBackColor
        {
            get => _headerBackColor;
            set { _headerBackColor = value; Invalidate(); }
        }

        [Category("ProcontDataGrid — Theming")]
        [Description("Fondo del header de grupo. Transparent = ProcontTheme.SurfaceHover.")]
        public Color GroupHeaderBackColor
        {
            get => _groupHeaderBackColor;
            set { _groupHeaderBackColor = value; Invalidate(); }
        }

        // ── Colores resueltos ─────────────────────────────────────────
        private Color ResolvedHeaderBg =>
            _headerBackColor == Color.Transparent ? ProcontTheme.SurfaceActive : _headerBackColor;

        private Color ResolvedGroupHeaderBg =>
            _groupHeaderBackColor == Color.Transparent ? ProcontTheme.SurfaceHover : _groupHeaderBackColor;

        // ── Alto total de footer ──────────────────────────────────────
        private int FooterTotalHeight
        {
            get
            {
                int h = 0;
                foreach (var r in _footerRows) h += r.Height;
                return h;
            }
        }

        // ══════════════════════════════════════════════════════════════
        // CONSTRUCTOR
        // ══════════════════════════════════════════════════════════════

        public ProcontDataGridView()
        {
            // CRÍTICO: sin esto, setear ColumnHeadersHeight lanza excepción
            ColumnHeadersHeightSizeMode =
                DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

            // Theming base
            BackgroundColor = ProcontTheme.SurfaceDark;
            GridColor = ProcontTheme.BorderDefault;
            BorderStyle = BorderStyle.None;

            RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;

            ColumnHeadersDefaultCellStyle.BackColor = ProcontTheme.SurfaceActive;
            ColumnHeadersDefaultCellStyle.ForeColor = ProcontTheme.TextPrimary;
            ColumnHeadersDefaultCellStyle.Font = ProcontTheme.FontBold;
            ColumnHeadersDefaultCellStyle.SelectionBackColor = ProcontTheme.SurfaceActive;
            ColumnHeadersDefaultCellStyle.SelectionForeColor = ProcontTheme.TextPrimary;
            ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            RowsDefaultCellStyle.BackColor = ProcontTheme.SurfaceDark;
            RowsDefaultCellStyle.ForeColor = ProcontTheme.TextPrimary;
            RowsDefaultCellStyle.Font = ProcontTheme.FontBase;
            RowsDefaultCellStyle.SelectionBackColor = ProcontTheme.SurfaceHover;
            RowsDefaultCellStyle.SelectionForeColor = ProcontTheme.TextPrimary;

            AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(6, 44, 62);
            AlternatingRowsDefaultCellStyle.ForeColor = ProcontTheme.TextPrimary;
            AlternatingRowsDefaultCellStyle.SelectionBackColor = ProcontTheme.SurfaceHover;
            AlternatingRowsDefaultCellStyle.SelectionForeColor = ProcontTheme.TextPrimary;

            RowHeadersDefaultCellStyle.BackColor = ProcontTheme.SurfaceActive;
            RowHeadersDefaultCellStyle.ForeColor = ProcontTheme.TextSubdued;

            EnableHeadersVisualStyles = false;
            SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            RowHeadersWidth = 20;
            DoubleBuffered = true;
        }

        // ══════════════════════════════════════════════════════════════
        // COLUMN GROUPS — setup y aplicación automática
        // ══════════════════════════════════════════════════════════════

        /// <summary>
        /// Dobla la altura del header para acomodar la fila de grupo.
        /// Se llama automáticamente al agregar/quitar columnas.
        /// Puede llamarse explícitamente si se configuran grupos en Form.Load.
        /// </summary>
        public void ApplyColumnGroups()
        {
            // Garantizar que el modo permita cambiar la altura
            if (ColumnHeadersHeightSizeMode != DataGridViewColumnHeadersHeightSizeMode.DisableResizing)
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

            if (_columnGroups.Count == 0)
            {
                if (_groupsApplied)
                {
                    ColumnHeadersHeight = GroupHeaderHeight;
                    _groupsApplied = false;
                    Invalidate();
                }
                return;
            }

            int targetH = GroupHeaderHeight * 2;
            if (ColumnHeadersHeight != targetH)
                ColumnHeadersHeight = targetH;

            _groupsApplied = true;
            Invalidate();
        }

        // Auto-apply cuando cambia la estructura de columnas
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
        // FOOTER — configuración
        // ══════════════════════════════════════════════════════════════

        /// <summary>
        /// Recalcula el espacio reservado para el footer (Padding.Bottom)
        /// y fuerza un repintado. Llamar después de modificar FooterRows.
        /// </summary>
        public void RebuildFooter()
        {
            int fh = FooterTotalHeight;
            // Padding.Bottom reserva espacio vacío al fondo del DGV
            // → las filas de datos no invaden esa área
            // → WM_PAINT pinta el footer allí
            var p = Padding;
            if (p.Bottom != fh)
                Padding = new Padding(p.Left, p.Top, p.Right, fh);
            Invalidate();
        }

        // ══════════════════════════════════════════════════════════════
        // FOOTER — fábrica externa (compatibilidad hacia atrás)
        // ══════════════════════════════════════════════════════════════

        /// <summary>
        /// [Compatibilidad] Crea un DataGridFooterBar externo pre-conectado.
        /// Para el uso integrado (recomendado), usar FooterRows + RebuildFooter().
        /// </summary>
        public DataGridFooterBar CreateFooterBar() => new DataGridFooterBar(this);

        // ══════════════════════════════════════════════════════════════
        // WM_PAINT — pintado del footer
        // ══════════════════════════════════════════════════════════════

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (m.Msg != WM_PAINT) return;
            int fh = FooterTotalHeight;
            if (fh <= 0 || _footerRows.Count == 0) return;

            // Pintar sobre el área reservada por Padding.Bottom
            using (var g = Graphics.FromHwnd(Handle))
            {
                g.SetHighQuality();
                int footerTop = ClientSize.Height - fh;
                PaintFooterRows(g, footerTop);
            }
        }

        // ══════════════════════════════════════════════════════════════
        // FOOTER — pintado interno
        // ══════════════════════════════════════════════════════════════

        private void PaintFooterRows(Graphics g, int startY)
        {
            // Línea separadora superior
            using (var pen = new Pen(ProcontTheme.BorderDefault, 1))
                g.DrawLine(pen, 0, startY, Width, startY);

            int rowY = startY + 1;
            foreach (var row in _footerRows)
            {
                PaintFooterRow(g, row, rowY);
                rowY += row.Height;
            }
        }

        private void PaintFooterRow(Graphics g, DataGridFooterRow row, int y)
        {
            int rowH = row.Height;

            // Fondo
            Color bg = row.BackColor == Color.Transparent
                ? ProcontTheme.SurfaceActive
                : row.BackColor;
            using (var fill = new SolidBrush(bg))
                g.FillRectangle(fill, 0, y, Width, rowH - 1);

            // Línea inferior
            using (var pen = new Pen(ProcontTheme.BorderDefault, 1))
                g.DrawLine(pen, 0, y + rowH - 1, Width, y + rowH - 1);

            // Celdas
            foreach (var cell in row.Cells)
            {
                if (string.IsNullOrEmpty(cell.ColumnName)) continue;

                var col = Columns[cell.ColumnName];
                if (col == null || !col.Visible) continue;

                // GetColumnDisplayRectangle devuelve coords en client area del DGV,
                // ya incluye offset del row header y el scroll horizontal actual.
                var colRect = GetColumnDisplayRectangle(col.Index, false);
                if (colRect.IsEmpty) continue; // columna fuera de la vista

                var cellRect = new Rectangle(colRect.X, y, colRect.Width - 1, rowH - 1);

                // Calcular valor
                string text = ComputeFooterCellValue(cell, col);

                // Tipografía y color
                var font = cell.Bold ? ProcontTheme.FontBold : ProcontTheme.FontBase;
                Color fg = cell.ForeColor == Color.Transparent
                    ? ProcontTheme.TextPrimary
                    : cell.ForeColor;

                // Texto
                using (var fmt = BuildStringFormat(cell.Alignment))
                using (var brush = new SolidBrush(fg))
                    g.DrawString(text, font, brush,
                        new RectangleF(cellRect.X + 4, cellRect.Y, cellRect.Width - 8, cellRect.Height),
                        fmt);

                // Borde derecho de celda
                using (var pen = new Pen(ProcontTheme.BorderDefault, 1))
                    g.DrawLine(pen, cellRect.Right, y, cellRect.Right, y + rowH - 1);
            }
        }

        // ══════════════════════════════════════════════════════════════
        // FOOTER — cálculo de valores
        // ══════════════════════════════════════════════════════════════

        private string ComputeFooterCellValue(DataGridFooterCell cell, DataGridViewColumn col)
        {
            if (cell.Formula == FooterFormula.Custom)
                return cell.Text ?? "";

            var values = new List<double>();
            foreach (DataGridViewRow row in Rows)
            {
                if (row.IsNewRow) continue;
                var v = row.Cells[col.Index].Value;
                if (v == null || v == DBNull.Value) continue;
                if (double.TryParse(v.ToString(), out double d))
                    values.Add(d);
            }

            double result = 0;
            switch (cell.Formula)
            {
                case FooterFormula.Sum:
                    foreach (var v in values) result += v;
                    break;
                case FooterFormula.Average:
                    if (values.Count > 0)
                    {
                        foreach (var v in values) result += v;
                        result /= values.Count;
                    }
                    break;
                case FooterFormula.Count:
                    result = values.Count;
                    break;
                case FooterFormula.Min:
                    if (values.Count > 0)
                    {
                        result = double.MaxValue;
                        foreach (var v in values) if (v < result) result = v;
                    }
                    break;
                case FooterFormula.Max:
                    if (values.Count > 0)
                    {
                        result = double.MinValue;
                        foreach (var v in values) if (v > result) result = v;
                    }
                    break;
            }

            string formatted = string.IsNullOrEmpty(cell.Format)
                ? result.ToString()
                : result.ToString(cell.Format);

            return string.IsNullOrEmpty(cell.Text)
                ? formatted
                : $"{cell.Text} {formatted}";
        }

        // ══════════════════════════════════════════════════════════════
        // COLUMN GROUPS — pintado (OnCellPainting)
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

            // Mitad inferior — nombre de columna
            PaintColumnHeaderCell(g,
                new Rectangle(bx, by + groupH, bw, colH),
                e.Value?.ToString() ?? "",
                e.ColumnIndex);

            // Mitad superior — grupo
            var topRect = new Rectangle(bx, by, bw, groupH);
            var group = FindGroupForColumn(e.ColumnIndex);

            if (group != null)
            {
                Color groupBg = group.BackColor == Color.Transparent
                    ? ResolvedGroupHeaderBg
                    : BlendColor(ProcontTheme.SurfaceDark, group.BackColor, 0.85f);

                using (var fill = new SolidBrush(groupBg))
                    g.FillRectangle(fill, topRect);

                // Borde vertical entre columnas del mismo grupo (excepto la última)
                if (!IsLastColumnInGroup(e.ColumnIndex, group))
                    using (var pen = new Pen(Color.FromArgb(30, ProcontTheme.BorderDefault), 1))
                        g.DrawLine(pen, topRect.Right - 1, topRect.Y + 3, topRect.Right - 1, topRect.Bottom - 3);

                // Título del grupo: se pinta UNA sola vez desde la primera columna visible
                if (IsFirstColumnInGroup(e.ColumnIndex, group))
                {
                    var spanRect = ComputeGroupSpanRect(group, by, groupH);
                    if (!spanRect.IsEmpty)
                    {
                        var savedClip = g.Clip.Clone();
                        g.ResetClip(); // Necesario para pintar cruzando los clips de celdas

                        using (var fill = new SolidBrush(groupBg))
                            g.FillRectangle(fill, spanRect);

                        using (var b = new SolidBrush(ProcontTheme.TextAccent))
                        using (var fmt = new StringFormat
                        {
                            Alignment = StringAlignment.Center,
                            LineAlignment = StringAlignment.Center,
                            Trimming = StringTrimming.EllipsisCharacter,
                            FormatFlags = StringFormatFlags.NoWrap
                        })
                            g.DrawString(group.Title, ProcontTheme.FontBold, b, spanRect, fmt);

                        // Borde inferior del span
                        using (var pen = new Pen(ProcontTheme.BorderDefault, 1))
                            g.DrawLine(pen, spanRect.X, spanRect.Bottom - 1, spanRect.Right, spanRect.Bottom - 1);

                        // Borde derecho del span
                        using (var pen = new Pen(ProcontTheme.BorderDefault, 1))
                            g.DrawLine(pen, spanRect.Right - 1, spanRect.Y, spanRect.Right - 1, spanRect.Bottom);

                        g.Clip = savedClip;
                    }
                }
            }
            else
            {
                // Columna sin grupo — top plain
                using (var fill = new SolidBrush(ResolvedGroupHeaderBg))
                    g.FillRectangle(fill, topRect);

                using (var pen = new Pen(ProcontTheme.BorderDefault, 1))
                    g.DrawLine(pen, topRect.Right - 1, topRect.Y, topRect.Right - 1, topRect.Bottom);
            }
        }

        private void PaintColumnHeaderCell(Graphics g, Rectangle rect, string text, int colIndex)
        {
            using (var fill = new SolidBrush(ResolvedHeaderBg))
                g.FillRectangle(fill, rect);

            using (var pen = new Pen(ProcontTheme.BorderDefault, 1))
            {
                g.DrawLine(pen, rect.Right - 1, rect.Y, rect.Right - 1, rect.Bottom);
                g.DrawLine(pen, rect.X, rect.Bottom - 1, rect.Right, rect.Bottom - 1);
            }

            var col = Columns[colIndex];
            bool sorted = col.HeaderCell.SortGlyphDirection != SortOrder.None;

            using (var b = new SolidBrush(ProcontTheme.TextPrimary))
            using (var fmt = new StringFormat
            {
                Alignment = sorted ? StringAlignment.Near : StringAlignment.Center,
                LineAlignment = StringAlignment.Center,
                Trimming = StringTrimming.EllipsisCharacter,
                FormatFlags = StringFormatFlags.NoWrap
            })
                g.DrawString(text, ProcontTheme.FontBold, b,
                    new RectangleF(rect.X + 4, rect.Y, rect.Width - 20, rect.Height), fmt);

            if (sorted)
            {
                int ax = rect.Right - 14;
                int ay = rect.Y + rect.Height / 2 - 3;
                bool asc = col.HeaderCell.SortGlyphDirection == SortOrder.Ascending;
                using (var pen = new Pen(ProcontTheme.TextSubdued, 1.5f)
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
        // ROW PAINTING — borde inferior por fila
        // ══════════════════════════════════════════════════════════════

        protected override void OnRowPostPaint(DataGridViewRowPostPaintEventArgs e)
        {
            base.OnRowPostPaint(e);
            using (var pen = new Pen(ProcontTheme.BorderDefault, 1))
                e.Graphics.DrawLine(pen,
                    e.RowBounds.Left, e.RowBounds.Bottom - 1,
                    e.RowBounds.Right, e.RowBounds.Bottom - 1);
        }

        // ══════════════════════════════════════════════════════════════
        // HELPERS — column group lookup
        // ══════════════════════════════════════════════════════════════

        private DataGridColumnGroup FindGroupForColumn(int colIndex)
        {
            if (colIndex < 0 || colIndex >= Columns.Count) return null;
            string name = Columns[colIndex].Name;
            foreach (var g in _columnGroups)
                if (g.ColumnNames.Contains(name)) return g;
            return null;
        }

        private bool IsFirstColumnInGroup(int colIndex, DataGridColumnGroup group)
        {
            int minDi = int.MaxValue, firstIdx = -1;
            foreach (var name in group.ColumnNames)
            {
                var col = Columns[name];
                if (col == null || !col.Visible) continue;
                if (col.DisplayIndex < minDi) { minDi = col.DisplayIndex; firstIdx = col.Index; }
            }
            return firstIdx == colIndex;
        }

        private bool IsLastColumnInGroup(int colIndex, DataGridColumnGroup group)
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
        // HELPERS — utilidades
        // ══════════════════════════════════════════════════════════════

        private static Color BlendColor(Color base_, Color over, float alpha)
        {
            float ba = 1f - alpha;
            return Color.FromArgb(
                (int)(base_.R * ba + over.R * alpha),
                (int)(base_.G * ba + over.G * alpha),
                (int)(base_.B * ba + over.B * alpha));
        }

        private static StringFormat BuildStringFormat(ContentAlignment alignment)
        {
            var fmt = new StringFormat
            {
                Trimming = StringTrimming.EllipsisCharacter,
                FormatFlags = StringFormatFlags.NoWrap
            };

            switch (alignment)
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

            switch (alignment)
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