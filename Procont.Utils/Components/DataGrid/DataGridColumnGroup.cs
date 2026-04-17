using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;

namespace Procont.Utils.Components.DataGrid
{
    // ══════════════════════════════════════════════════════════════════
    // GROUPED COLUMN HEADERS
    // ══════════════════════════════════════════════════════════════════

    /// <summary>
    /// Defines a spanning group label painted above a set of columns
    /// in a <see cref="ProcontDataGridView"/>.
    ///
    /// ── USO ──────────────────────────────────────────────────────────
    ///   grid.ColumnGroups.Add(new DataGridColumnGroup
    ///   {
    ///       Title       = "Período actual",
    ///       BackColor   = Color.FromArgb(10, 245, 168, 30),
    ///       ColumnNames = { "colUnits", "colPrice", "colTotal" }
    ///   });
    ///
    /// After adding groups, call grid.ApplyColumnGroups() once to
    /// double the header height and trigger a repaint.
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class DataGridColumnGroup
    {
        [Category("ColumnGroup")]
        [Description("Text displayed in the merged group header cell.")]
        [DefaultValue("")]
        public string Title { get; set; } = "";

        [Category("ColumnGroup")]
        [Description("Background of the group header. Transparent = default header background.")]
        public Color BackColor { get; set; } = Color.Transparent;

        [Category("ColumnGroup")]
        [Description("Column Names (DataGridViewColumn.Name) that belong to this group.")]
        public List<string> ColumnNames { get; } = new List<string>();

        public override string ToString() =>
            string.IsNullOrEmpty(Title) ? "(grupo sin título)" : Title;
    }

    // ══════════════════════════════════════════════════════════════════
    // FOOTER ROWS
    // ══════════════════════════════════════════════════════════════════

    /// <summary>Aggregation formula for a footer cell.</summary>
    public enum FooterFormula
    {
        [Description("Show a fixed text value.")]
        Custom,

        [Description("Sum of numeric values in the column.")]
        Sum,

        [Description("Average of numeric values.")]
        Average,

        [Description("Count of non-empty cells.")]
        Count,

        [Description("Minimum numeric value.")]
        Min,

        [Description("Maximum numeric value.")]
        Max
    }

    /// <summary>
    /// One cell in a <see cref="DataGridFooterRow"/>.
    /// Maps to a DataGridView column by <see cref="ColumnName"/>.
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class DataGridFooterCell
    {
        [Category("FooterCell")]
        [Description("Column Name (DataGridViewColumn.Name) this cell maps to.")]
        [DefaultValue("")]
        public string ColumnName { get; set; } = "";

        [Category("FooterCell")]
        [Description("How the cell value is computed.")]
        [DefaultValue(FooterFormula.Custom)]
        public FooterFormula Formula { get; set; } = FooterFormula.Custom;

        [Category("FooterCell")]
        [Description("Custom text (used directly when Formula = Custom; also used as label prefix for formulas).")]
        [DefaultValue("")]
        public string Text { get; set; } = "";

        [Category("FooterCell")]
        [Description("Numeric format string (e.g. \"N2\", \"C2\"). Empty = ToString().")]
        [DefaultValue("")]
        public string Format { get; set; } = "";

        [Category("FooterCell")]
        [Description("Text alignment within the cell.")]
        [DefaultValue(ContentAlignment.MiddleRight)]
        public ContentAlignment Alignment { get; set; } = ContentAlignment.MiddleRight;

        [Category("FooterCell")]
        [Description("Override foreground color. Transparent = default.")]
        public Color ForeColor { get; set; } = Color.Transparent;

        [Category("FooterCell")]
        [Description("Render text in bold.")]
        [DefaultValue(true)]
        public bool Bold { get; set; } = true;
    }

    /// <summary>
    /// One footer row in a <see cref="DataGridFooterBar"/>.
    /// Add one or more rows to show aggregates (totals, averages, etc.).
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class DataGridFooterRow
    {
        [Category("FooterRow")]
        [Description("Row height in pixels.")]
        [DefaultValue(32)]
        public int Height { get; set; } = 32;

        [Category("FooterRow")]
        [Description("Row background. Transparent = SurfaceActive.")]
        public Color BackColor { get; set; } = Color.Transparent;

        [Category("FooterRow")]
        [Description("Cells in this row, one per column.")]
        public List<DataGridFooterCell> Cells { get; } = new List<DataGridFooterCell>();
    }
}