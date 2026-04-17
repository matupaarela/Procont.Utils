using Procont.Utils.Core.Extensions;
using Procont.Utils.Core.Theming;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Procont.Utils.Components.DataGrid
{
    /// <summary>
    /// Sticky footer panel for a <see cref="ProcontDataGridView"/>.
    ///
    /// Docks just below the grid and paints <see cref="DataGridFooterRow"/> cells
    /// aligned to the grid's column widths, including horizontal scroll tracking.
    ///
    /// ── SETUP ────────────────────────────────────────────────────────
    /// The simplest way is to let ProcontDataGridView create it for you:
    ///
    ///   var footer = grid.CreateFooterBar();
    ///   footer.Dock = DockStyle.Bottom;
    ///   myPanel.Controls.Add(footer);
    ///   myPanel.Controls.Add(grid);   // DGV must be added AFTER footer
    ///
    /// Then add rows:
    ///   footer.Rows.Add(new DataGridFooterRow
    ///   {
    ///       Cells =
    ///       {
    ///           new DataGridFooterCell { ColumnName = "Total", Formula = FooterFormula.Sum, Format = "N2" },
    ///           new DataGridFooterCell { ColumnName = "Label", Text = "TOTAL", Alignment = ContentAlignment.MiddleLeft }
    ///       }
    ///   });
    ///   footer.Rebuild();
    ///
    /// ── AUTO-CALCULATION ────────────────────────────────────────────
    /// When Formula != Custom, the footer reads cell values from the bound
    /// DataGridView rows at paint time (lazy — recalculated each Invalidate).
    /// Call footer.Invalidate() after the grid's DataSource changes.
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

            base.BackColor = ProcontTheme.SurfaceDark;
            Height = 0;

            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint, true);

            // Sync when grid layout changes
            _grid.ColumnWidthChanged += (s, e) => Invalidate();
            _grid.ColumnDisplayIndexChanged += (s, e) => Invalidate();
            //_grid.ColumnVisibleChanged += (s, e) => Invalidate();
            _grid.Scroll += (s, e) => { if (e.ScrollOrientation == ScrollOrientation.HorizontalScroll) Invalidate(); };
            _grid.DataSourceChanged += (s, e) => Invalidate();
            _grid.RowsAdded += (s, e) => Invalidate();
            _grid.RowsRemoved += (s, e) => Invalidate();
            _grid.CellValueChanged += (s, e) => Invalidate();
        }

        // ── Public API ────────────────────────────────────────────────

        /// <summary>Recalculates this bar's height from the sum of row heights and repaints.</summary>
        public void Rebuild()
        {
            int h = 0;
            foreach (var row in _rows) h += row.Height;
            Height = h;
            Invalidate();
        }

        // ══════════════════════════════════════════════════════════════
        // PAINT
        // ══════════════════════════════════════════════════════════════

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SetHighQuality();
            g.Clear(ProcontTheme.SurfaceDark);

            if (_grid == null || _rows.Count == 0) return;

            // Top border
            using (var pen = new Pen(ProcontTheme.BorderDefault, 1))
                g.DrawLine(pen, 0, 0, Width, 0);

            int rowY = 1;
            foreach (var row in _rows)
            {
                DrawFooterRow(g, row, rowY);
                rowY += row.Height;
            }
        }

        private void DrawFooterRow(Graphics g, DataGridFooterRow row, int y)
        {
            var rowRect = new Rectangle(0, y, Width, row.Height - 1);

            // Background
            Color bg = row.BackColor == Color.Transparent
                ? ProcontTheme.SurfaceActive
                : row.BackColor;
            using (var fill = new SolidBrush(bg))
                g.FillRectangle(fill, rowRect);

            // Bottom separator
            using (var pen = new Pen(ProcontTheme.BorderDefault, 1))
                g.DrawLine(pen, 0, y + row.Height - 1, Width, y + row.Height - 1);

            // Row-header offset mirrors the grid
            int rhw = _grid.RowHeadersVisible ? _grid.RowHeadersWidth : 0;

            // Cells
            foreach (var cell in row.Cells)
            {
                var col = _grid.Columns[cell.ColumnName];
                if (col == null || !col.Visible) continue;

                // GetColumnDisplayRectangle gives coords relative to the grid's client area.
                // We subtract the grid's left edge relative to our shared parent, then add rhw offset.
                var colRect = _grid.GetColumnDisplayRectangle(col.Index, false);
                if (colRect.IsEmpty) continue; // scrolled out of view

                // colRect is already in screen-relative coords of the grid.
                // We need to translate to our own coords.
                // Both controls share the same parent, so:
                int cellX = _grid.Left + colRect.Left;
                // But we might not be at the same Left as the grid; adjust:
                cellX -= Left;
                var cellRect = new Rectangle(cellX, y, colRect.Width - 1, row.Height - 1);

                // Compute value
                string valueText = ComputeCellValue(cell, col);

                // Font
                var font = cell.Bold ? ProcontTheme.FontBold : ProcontTheme.FontBase;

                // ForeColor
                Color fg = cell.ForeColor == Color.Transparent
                    ? ProcontTheme.TextPrimary
                    : cell.ForeColor;

                // Draw text
                var fmt = AlignmentToStringFormat(cell.Alignment);
                var pad = new RectangleF(cellRect.X + 4, cellRect.Y, cellRect.Width - 8, cellRect.Height);
                using (var b = new SolidBrush(fg))
                    g.DrawString(valueText, font, b, pad, fmt);
                fmt.Dispose();

                // Right border
                using (var pen = new Pen(ProcontTheme.BorderDefault, 1))
                    g.DrawLine(pen, cellRect.Right, y, cellRect.Right, y + row.Height - 1);
            }
        }

        // ── Value computation ─────────────────────────────────────────

        private string ComputeCellValue(DataGridFooterCell cell, DataGridViewColumn col)
        {
            if (cell.Formula == FooterFormula.Custom)
                return cell.Text;

            // Gather numeric values from grid rows
            var values = new List<double>();
            foreach (DataGridViewRow row in _grid.Rows)
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
                        double sum = 0;
                        foreach (var v in values) sum += v;
                        result = sum / values.Count;
                    }
                    break;
                case FooterFormula.Count:
                    result = values.Count;
                    break;
                case FooterFormula.Min:
                    result = values.Count > 0 ? double.MaxValue : 0;
                    foreach (var v in values) if (v < result) result = v;
                    break;
                case FooterFormula.Max:
                    result = values.Count > 0 ? double.MinValue : 0;
                    foreach (var v in values) if (v > result) result = v;
                    break;
            }

            string formatted = string.IsNullOrEmpty(cell.Format)
                ? result.ToString()
                : result.ToString(cell.Format);

            return string.IsNullOrEmpty(cell.Text)
                ? formatted
                : $"{cell.Text} {formatted}";
        }

        // ── StringFormat from ContentAlignment ────────────────────────

        private static StringFormat AlignmentToStringFormat(ContentAlignment a)
        {
            var fmt = new StringFormat
            {
                LineAlignment = StringAlignment.Center,
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