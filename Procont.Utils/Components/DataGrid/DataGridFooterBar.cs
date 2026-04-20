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
    /// Footer panel EXTERNO para un DataGridView estándar.
    ///
    /// Para <see cref="ProcontDataGridView"/>, usar directamente
    /// <see cref="ProcontDataGridView.FooterRows"/> + <see cref="ProcontDataGridView.RebuildFooter()"/>
    /// que integra el footer dentro del propio control sin controles adicionales.
    ///
    /// ── SETUP (uso externo / DataGridView estándar) ──────────────────
    ///   var footer = grid.CreateFooterBar();   // o new DataGridFooterBar(grid)
    ///   footer.Dock = DockStyle.Bottom;
    ///   myPanel.Controls.Add(footer);          // agregar ANTES que el grid
    ///   myPanel.Controls.Add(grid);
    ///
    ///   footer.Rows.Add(new DataGridFooterRow { ... });
    ///   footer.Rebuild();
    /// </summary>
    [ToolboxItem(false)]
    [DesignTimeVisible(false)]
    public class DataGridFooterBar : Control
    {
        private readonly System.Windows.Forms.DataGridView _grid;
        private readonly List<DataGridFooterRow> _rows = new List<DataGridFooterRow>();

        [Browsable(false)]
        public List<DataGridFooterRow> Rows => _rows;

        public DataGridFooterBar(System.Windows.Forms.DataGridView grid)
        {
            _grid = grid ?? throw new ArgumentNullException(nameof(grid));

            base.BackColor = ProcontTheme.SurfaceDark;
            Height = 0;

            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint, true);

            _grid.ColumnWidthChanged += (s, e) => Invalidate();
            _grid.ColumnDisplayIndexChanged += (s, e) => Invalidate();
            _grid.ColumnStateChanged += (s, e) => Invalidate();  // cubre ColumnVisible
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

        public void Rebuild()
        {
            int h = 0;
            foreach (var row in _rows) h += row.Height;
            Height = h;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SetHighQuality();
            g.Clear(ProcontTheme.SurfaceDark);

            if (_rows.Count == 0) return;

            using (var pen = new Pen(ProcontTheme.BorderDefault, 1))
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
            Color bg = row.BackColor == Color.Transparent
                ? ProcontTheme.SurfaceActive
                : row.BackColor;

            using (var fill = new SolidBrush(bg))
                g.FillRectangle(fill, 0, y, Width, row.Height - 1);

            using (var pen = new Pen(ProcontTheme.BorderDefault, 1))
                g.DrawLine(pen, 0, y + row.Height - 1, Width, y + row.Height - 1);

            foreach (var cell in row.Cells)
            {
                if (string.IsNullOrEmpty(cell.ColumnName)) continue;
                var col = _grid.Columns[cell.ColumnName];
                if (col == null || !col.Visible) continue;

                var colRect = _grid.GetColumnDisplayRectangle(col.Index, false);
                if (colRect.IsEmpty) continue;

                // Traducir coords del grid a coords de este control
                int cellX = _grid.Left + colRect.Left - Left;
                var cellRect = new Rectangle(cellX, y, colRect.Width - 1, row.Height - 1);

                string text = ComputeValue(cell, col);
                var font = cell.Bold ? ProcontTheme.FontBold : ProcontTheme.FontBase;
                Color fg = cell.ForeColor == Color.Transparent
                    ? ProcontTheme.TextPrimary
                    : cell.ForeColor;

                using (var fmt = BuildStringFormat(cell.Alignment))
                using (var b = new SolidBrush(fg))
                    g.DrawString(text, font, b,
                        new RectangleF(cellRect.X + 4, cellRect.Y, cellRect.Width - 8, cellRect.Height),
                        fmt);

                using (var pen = new Pen(ProcontTheme.BorderDefault, 1))
                    g.DrawLine(pen, cellRect.Right, y, cellRect.Right, y + row.Height - 1);
            }
        }

        private string ComputeValue(DataGridFooterCell cell, DataGridViewColumn col)
        {
            if (cell.Formula == FooterFormula.Custom)
                return cell.Text ?? "";

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
                    foreach (var v in values) result += v; break;
                case FooterFormula.Average:
                    if (values.Count > 0)
                    {
                        foreach (var v in values) result += v;
                        result /= values.Count;
                    }
                    break;
                case FooterFormula.Count: result = values.Count; break;
                case FooterFormula.Min:
                    if (values.Count > 0) { result = double.MaxValue; foreach (var v in values) if (v < result) result = v; }
                    break;
                case FooterFormula.Max:
                    if (values.Count > 0) { result = double.MinValue; foreach (var v in values) if (v > result) result = v; }
                    break;
            }

            string fmt2 = string.IsNullOrEmpty(cell.Format) ? result.ToString() : result.ToString(cell.Format);
            return string.IsNullOrEmpty(cell.Text) ? fmt2 : $"{cell.Text} {fmt2}";
        }

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