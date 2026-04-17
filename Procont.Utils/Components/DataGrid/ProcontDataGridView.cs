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
    /// DataGridView extendido con dos funcionalidades extra:
    ///
    /// 1. GROUPED COLUMN HEADERS
    ///    Pinta un nivel de encabezado de grupo por encima de los headers
    ///    de columna estándar. Soporta spans multi-columna.
    ///
    ///    grid.ColumnGroups.Add(new DataGridColumnGroup
    ///    {
    ///        Title = "Período Actual",
    ///        BackColor = Color.FromArgb(20, 245, 168, 30),
    ///        ColumnNames = { "colUnits", "colPrice", "colTotal" }
    ///    });
    ///    grid.ApplyColumnGroups();   // ← llamar una vez después de agregar grupos
    ///
    /// 2. FOOTER BAR (sticky aggregates)
    ///    var footer = grid.CreateFooterBar();
    ///    footer.Dock = DockStyle.Bottom;
    ///    myPanel.Controls.Add(footer);
    ///    myPanel.Controls.Add(grid);        // grid encima del footer
    ///    footer.Rows.Add(new DataGridFooterRow { ... });
    ///    footer.Rebuild();
    ///
    /// ── THEMING ─────────────────────────────────────────────────────
    /// Todos los colores provienen de ProcontTheme. Override HeaderBackColor /
    /// GroupHeaderBackColor para personalizar sin tocar ProcontTheme.
    /// </summary>
    [ToolboxItem(true)]
    [Description("DataGridView with grouped column headers and sticky footer rows.")]
    public class ProcontDataGridView : System.Windows.Forms.DataGridView
    {
        // ── Column groups ─────────────────────────────────────────────
        private readonly List<DataGridColumnGroup> _columnGroups = new List<DataGridColumnGroup>();
        private bool _groupsApplied = false;
        private const int GroupHeaderHeight = 24;

        // ── Visual config ──────────────────────────────────────────────
        private Color _headerBackColor = Color.Transparent;    // Transparent = use ProcontTheme
        private Color _groupHeaderBackColor = Color.Transparent;

        // ══════════════════════════════════════════════════════════════
        // PROPIEDADES PÚBLICAS
        // ══════════════════════════════════════════════════════════════

        [Category("ProcontDataGrid")]
        [Description("Column group definitions. Call ApplyColumnGroups() after adding.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public List<DataGridColumnGroup> ColumnGroups => _columnGroups;

        [Category("ProcontDataGrid — Theming")]
        [Description("Column header background. Transparent = ProcontTheme.SurfaceActive.")]
        public Color HeaderBackColor
        {
            get => _headerBackColor;
            set { _headerBackColor = value; Invalidate(); }
        }

        [Category("ProcontDataGrid — Theming")]
        [Description("Group header background. Transparent = ProcontTheme.SurfaceDark (with tint from group.BackColor).")]
        public Color GroupHeaderBackColor
        {
            get => _groupHeaderBackColor;
            set { _groupHeaderBackColor = value; Invalidate(); }
        }

        // ── Resolved colors ───────────────────────────────────────────
        private Color ResolvedHeaderBg =>
            _headerBackColor == Color.Transparent ? ProcontTheme.SurfaceActive : _headerBackColor;

        private Color ResolvedGroupHeaderBg =>
            _groupHeaderBackColor == Color.Transparent ? ProcontTheme.SurfaceHover : _groupHeaderBackColor;

        // ══════════════════════════════════════════════════════════════
        // CONSTRUCTOR
        // ══════════════════════════════════════════════════════════════

        public ProcontDataGridView()
        {
            // Base appearance
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
        // FOOTER FACTORY
        // ══════════════════════════════════════════════════════════════

        /// <summary>
        /// Creates a <see cref="DataGridFooterBar"/> pre-wired to this grid.
        /// Dock it to DockStyle.Bottom in the same parent container as this grid,
        /// adding it BEFORE adding this grid so docking order is correct.
        /// </summary>
        public DataGridFooterBar CreateFooterBar() => new DataGridFooterBar(this);

        // ══════════════════════════════════════════════════════════════
        // GROUPED HEADERS — setup
        // ══════════════════════════════════════════════════════════════

        /// <summary>
        /// Doubles the column header height to accommodate the group row.
        /// Call once after all ColumnGroups have been added.
        /// Safe to call multiple times (idempotent when groups are unchanged).
        /// </summary>
        public void ApplyColumnGroups()
        {
            if (_columnGroups.Count == 0)
            {
                // Remove group row if it was previously applied
                if (_groupsApplied)
                {
                    ColumnHeadersHeight = GroupHeaderHeight;
                    _groupsApplied = false;
                }
                return;
            }

            ColumnHeadersHeight = GroupHeaderHeight * 2;
            _groupsApplied = true;
            Invalidate();
        }

        // ══════════════════════════════════════════════════════════════
        // GROUPED HEADERS — painting
        // ══════════════════════════════════════════════════════════════

        protected override void OnCellPainting(DataGridViewCellPaintingEventArgs e)
        {
            // Only intercept column header cells when groups are configured
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

            // ── Bottom half: column name ───────────────────────────────
            var colRect = new Rectangle(bx, by + groupH, bw, colH);
            PaintColumnHeaderCell(g, colRect, e.Value?.ToString() ?? "", e.ColumnIndex);

            // ── Top half: group row ────────────────────────────────────
            var topRect = new Rectangle(bx, by, bw, groupH);
            var group = FindGroupForColumn(e.ColumnIndex);

            if (group != null)
            {
                Color groupBg = group.BackColor == Color.Transparent
                    ? ResolvedGroupHeaderBg
                    : Blend(ProcontTheme.SurfaceDark, group.BackColor, 0.85f);

                // Always fill this column's slice of the top row
                using (var fill = new SolidBrush(groupBg))
                    g.FillRectangle(fill, topRect);

                // Vertical right border (between columns in the same group, except the last)
                if (!IsLastColumnInGroup(e.ColumnIndex, group))
                {
                    using (var pen = new Pen(Color.FromArgb(30, ProcontTheme.BorderDefault), 1))
                        g.DrawLine(pen, topRect.Right - 1, topRect.Y + 3, topRect.Right - 1, topRect.Bottom - 3);
                }

                // Paint the group title ONCE — from the first visible column of the group
                if (IsFirstColumnInGroup(e.ColumnIndex, group))
                {
                    var spanRect = GetGroupSpanRect(group, by, groupH);
                    if (!spanRect.IsEmpty)
                    {
                        // Break out of cell clip to paint the full span
                        var saved = g.Clip.Clone();
                        g.ResetClip();

                        // Re-fill to cover any partial paints from prior columns
                        using (var fill = new SolidBrush(groupBg))
                            g.FillRectangle(fill, spanRect);

                        // Group title text
                        using (var b = new SolidBrush(ProcontTheme.TextAccent))
                        using (var fmt = new StringFormat
                        {
                            Alignment = StringAlignment.Center,
                            LineAlignment = StringAlignment.Center,
                            Trimming = StringTrimming.EllipsisCharacter,
                            FormatFlags = StringFormatFlags.NoWrap
                        })
                            g.DrawString(group.Title, ProcontTheme.FontBold, b, spanRect, fmt);

                        // Bottom border of group span
                        using (var pen = new Pen(ProcontTheme.BorderDefault, 1))
                            g.DrawLine(pen, spanRect.X, spanRect.Bottom - 1, spanRect.Right, spanRect.Bottom - 1);

                        // Right border of group span
                        using (var pen = new Pen(ProcontTheme.BorderDefault, 1))
                            g.DrawLine(pen, spanRect.Right - 1, spanRect.Y, spanRect.Right - 1, spanRect.Bottom);

                        g.Clip = saved;
                    }
                }
            }
            else
            {
                // Column not in any group — full-height header (no split)
                // Paint the top half as a plain header
                using (var fill = new SolidBrush(ResolvedGroupHeaderBg))
                    g.FillRectangle(fill, topRect);

                // Right border
                using (var pen = new Pen(ProcontTheme.BorderDefault, 1))
                    g.DrawLine(pen, topRect.Right - 1, topRect.Y, topRect.Right - 1, topRect.Bottom);
            }
        }

        // ── Paint column name cell (bottom half) ──────────────────────
        private void PaintColumnHeaderCell(Graphics g, Rectangle rect, string text, int colIndex)
        {
            // Background
            using (var fill = new SolidBrush(ResolvedHeaderBg))
                g.FillRectangle(fill, rect);

            // Right border
            using (var pen = new Pen(ProcontTheme.BorderDefault, 1))
            {
                g.DrawLine(pen, rect.Right - 1, rect.Y, rect.Right - 1, rect.Bottom);
                g.DrawLine(pen, rect.X, rect.Bottom - 1, rect.Right, rect.Bottom - 1);
            }

            // Text + sort indicator
            int rightPad = 18; // reserve space for sort arrow
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
            {
                var textRect = new RectangleF(rect.X + 4, rect.Y, rect.Width - rightPad, rect.Height);
                g.DrawString(text, ProcontTheme.FontBold, b, textRect, fmt);
            }

            // Sort arrow
            if (sorted)
            {
                int ax = rect.Right - 14;
                int ay = rect.Y + (rect.Height / 2) - 3;
                bool asc = col.HeaderCell.SortGlyphDirection == SortOrder.Ascending;
                using (var pen = new Pen(ProcontTheme.TextSubdued, 1.5f)
                { StartCap = System.Drawing.Drawing2D.LineCap.Round, EndCap = System.Drawing.Drawing2D.LineCap.Round })
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
        // GROUP LOOKUP HELPERS
        // ══════════════════════════════════════════════════════════════

        private DataGridColumnGroup FindGroupForColumn(int colIndex)
        {
            if (colIndex < 0 || colIndex >= Columns.Count) return null;
            string colName = Columns[colIndex].Name;

            foreach (var group in _columnGroups)
            {
                if (group.ColumnNames.Contains(colName))
                    return group;
            }
            return null;
        }

        private bool IsFirstColumnInGroup(int colIndex, DataGridColumnGroup group)
        {
            int minDi = int.MaxValue;
            int firstIdx = -1;

            foreach (string name in group.ColumnNames)
            {
                var col = Columns[name];
                if (col == null || !col.Visible) continue;
                if (col.DisplayIndex < minDi)
                {
                    minDi = col.DisplayIndex;
                    firstIdx = col.Index;
                }
            }
            return firstIdx == colIndex;
        }

        private bool IsLastColumnInGroup(int colIndex, DataGridColumnGroup group)
        {
            int maxDi = int.MinValue;
            int lastIdx = -1;

            foreach (string name in group.ColumnNames)
            {
                var col = Columns[name];
                if (col == null || !col.Visible) continue;
                if (col.DisplayIndex > maxDi)
                {
                    maxDi = col.DisplayIndex;
                    lastIdx = col.Index;
                }
            }
            return lastIdx == colIndex;
        }

        /// <summary>
        /// Returns the spanning rectangle (in DGV client coords) that covers all
        /// visible columns in the group, for painting the merged group header.
        /// </summary>
        private Rectangle GetGroupSpanRect(DataGridColumnGroup group, int y, int height)
        {
            int minX = int.MaxValue;
            int maxRight = int.MinValue;

            foreach (string name in group.ColumnNames)
            {
                var col = Columns[name];
                if (col == null || !col.Visible) continue;

                var r = GetColumnDisplayRectangle(col.Index, false);
                if (r.IsEmpty) continue; // scrolled out of view

                if (r.X < minX) minX = r.X;
                if (r.Right > maxRight) maxRight = r.Right;
            }

            if (minX == int.MaxValue) return Rectangle.Empty;
            return new Rectangle(minX, y, maxRight - minX, height);
        }

        // ── Color blend helper ─────────────────────────────────────────

        private static Color Blend(Color base_, Color over, float overAlpha)
        {
            float ba = 1f - overAlpha;
            return Color.FromArgb(
                (int)(base_.R * ba + over.R * overAlpha),
                (int)(base_.G * ba + over.G * overAlpha),
                (int)(base_.B * ba + over.B * overAlpha));
        }

        // ══════════════════════════════════════════════════════════════
        // ROW PAINTING — bottom border per row
        // ══════════════════════════════════════════════════════════════

        protected override void OnRowPostPaint(DataGridViewRowPostPaintEventArgs e)
        {
            base.OnRowPostPaint(e);
            using (var pen = new Pen(ProcontTheme.BorderDefault, 1))
                e.Graphics.DrawLine(pen,
                    e.RowBounds.Left, e.RowBounds.Bottom - 1,
                    e.RowBounds.Right, e.RowBounds.Bottom - 1);
        }
    }
}