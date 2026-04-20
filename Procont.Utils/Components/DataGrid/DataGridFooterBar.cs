using Procont.Utils.Core.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Procont.Utils.Components.DataGrid
{
    /// <summary>
    /// Footer panel externo para un DataGridView estándar (no ProcontDataGridView).
    ///
    /// Para <see cref="ProcontDataGridView"/> se recomienda usar
    /// <see cref="ProcontDataGridView.FooterRows"/> + <see cref="ProcontDataGridView.RebuildFooter()"/>
    /// que pinta el footer dentro del propio control sin controles adicionales.
    ///
    /// ── SETUP ───────────────────────────────────────────────────────
    ///   // 1. Crear con la factory del grid
    ///   var footer = grid.CreateFooterBar();
    ///
    ///   // 2. Colocar ANTES del grid en el container (Dock.Bottom primero)
    ///   myPanel.Controls.Add(footer);   // DockStyle.Bottom asignado internamente
    ///   myPanel.Controls.Add(grid);     // el grid encima
    ///
    ///   // 3. Configurar filas y reconstruir
    ///   footer.Rows.Add(new DataGridFooterRow { ... });
    ///   footer.Rebuild();
    ///
    /// NOTA: Funciona con cualquier DataGridView estándar, no solo ProcontDataGridView.
    /// </summary>
    [ToolboxItem(false)]
    [DesignTimeVisible(false)]
    public class DataGridFooterBar : Control
    {
        private readonly System.Windows.Forms.DataGridView _grid;
        private readonly List<DataGridFooterRow> _rows = new List<DataGridFooterRow>();

        [Browsable(false)]
        public List<DataGridFooterRow> Rows => _rows;

        // ── Constructor ───────────────────────────────────────────────

        public DataGridFooterBar(System.Windows.Forms.DataGridView grid)
        {
            _grid = grid ?? throw new ArgumentNullException(nameof(grid));

            Height = 0;
            Dock = DockStyle.Bottom;

            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint, true);

            // Sincronizar con cambios en el grid
            _grid.ColumnWidthChanged += (s, e) => Invalidate();
            _grid.ColumnDisplayIndexChanged += (s, e) => Invalidate();
            _grid.ColumnStateChanged += (s, e) => Invalidate(); // cubre ColumnVisible
            _grid.Scroll += (s, e) =>
            {
                if (e.ScrollOrientation == ScrollOrientation.HorizontalScroll)
                    Invalidate();
            };
            _grid.DataSourceChanged += (s, e) => Invalidate();
            _grid.RowsAdded += (s, e) => Invalidate();
            _grid.RowsRemoved += (s, e) => Invalidate();
            _grid.CellValueChanged += (s, e) => Invalidate();
        }

        // ── API pública ───────────────────────────────────────────────

        /// <summary>Recalcula la altura del footer y fuerza repintado.</summary>
        public void Rebuild()
        {
            int h = 0;
            foreach (var row in _rows) h += row.Height;
            Height = h;
            Invalidate();
        }

        // ── Pintado ───────────────────────────────────────────────────

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SetHighQuality();
            g.Clear(SystemColors.ControlDark);

            if (_rows.Count == 0) return;

            using (var pen = new Pen(SystemColors.ControlDarkDark, 1))
                g.DrawLine(pen, 0, 0, Width, 0);

            int rowY = 1;
            foreach (var row in _rows)
            {
                DrawRow(g, row, rowY);
                rowY += row.Height;
            }
        }

        private void DrawRow(Graphics g, DataGridFooterRow row, int y)
        {
            Color bg = row.BackColor == Color.Empty
                ? SystemColors.ControlDark
                : row.BackColor;

            using (var fill = new SolidBrush(bg))
                g.FillRectangle(fill, 0, y, Width, row.Height - 1);

            using (var pen = new Pen(SystemColors.ControlDarkDark, 1))
                g.DrawLine(pen, 0, y + row.Height - 1, Width, y + row.Height - 1);

            foreach (var cell in row.Cells)
            {
                if (string.IsNullOrEmpty(cell.ColumnName)) continue;

                var col = _grid.Columns[cell.ColumnName];
                if (col == null || !col.Visible) continue;

                var colRect = _grid.GetColumnDisplayRectangle(col.Index, false);
                if (colRect.IsEmpty) continue;

                // Traducción robusta de coordenadas: grid client → screen → footer client
                // Funciona independientemente del parent hierarchy
                var screenPt = _grid.PointToScreen(new Point(colRect.Left, 0));
                int cellX = PointToClient(screenPt).X;

                var cellRect = new Rectangle(cellX, y, colRect.Width - 1, row.Height - 1);
                string text = ComputeValue(cell, col);

                var font = cell.Bold ? new Font(Font, FontStyle.Bold) : Font;
                Color fg = cell.ForeColor == Color.Empty
                    ? SystemColors.ControlText
                    : cell.ForeColor;

                using (var fmt = BuildStringFormat(cell.Alignment))
                using (var brush = new SolidBrush(fg))
                    g.DrawString(text, font, brush,
                        new RectangleF(cellX + 4, y, colRect.Width - 8, row.Height - 1),
                        fmt);

                if (cell.Bold) font.Dispose();

                using (var pen = new Pen(SystemColors.ControlDarkDark, 1))
                    g.DrawLine(pen, cellRect.Right, y, cellRect.Right, y + row.Height - 1);
            }
        }

        // ── Cálculo de valores ────────────────────────────────────────

        private string ComputeValue(DataGridFooterCell cell, DataGridViewColumn col)
        {
            if (cell.Formula == FooterFormula.Custom)
                return cell.Text ?? "";

            double result = 0;
            int count = 0;

            foreach (DataGridViewRow row in _grid.Rows)
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
                    case FooterFormula.Min: result = count == 1 ? d : Math.Min(result, d); break;
                    case FooterFormula.Max: result = count == 1 ? d : Math.Max(result, d); break;
                }
            }

            if (cell.Formula == FooterFormula.Average && count > 0) result /= count;
            if (cell.Formula == FooterFormula.Count) result = count;

            string fmt = string.IsNullOrEmpty(cell.Format)
                ? result.ToString()
                : result.ToString(cell.Format);

            return string.IsNullOrEmpty(cell.Text) ? fmt : $"{cell.Text} {fmt}";
        }

        // ── StringFormat desde ContentAlignment ───────────────────────

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
                case ContentAlignment.BottomLeft: fmt.Alignment = StringAlignment.Near; break;
                case ContentAlignment.TopCenter:
                case ContentAlignment.MiddleCenter:
                case ContentAlignment.BottomCenter: fmt.Alignment = StringAlignment.Center; break;
                default: fmt.Alignment = StringAlignment.Far; break;
            }
            switch (a)
            {
                case ContentAlignment.TopLeft:
                case ContentAlignment.TopCenter:
                case ContentAlignment.TopRight: fmt.LineAlignment = StringAlignment.Near; break;
                case ContentAlignment.BottomLeft:
                case ContentAlignment.BottomCenter:
                case ContentAlignment.BottomRight: fmt.LineAlignment = StringAlignment.Far; break;
                default: fmt.LineAlignment = StringAlignment.Center; break;
            }
            return fmt;
        }
    }
}