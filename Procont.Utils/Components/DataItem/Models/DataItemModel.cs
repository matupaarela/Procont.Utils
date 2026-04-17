using FontAwesome.Sharp;
using Procont.Utils.Components.Sidebar.Models;
using System.ComponentModel;
using System.Drawing;

namespace Procont.Utils.Components.DataItem.Models
{
    /// <summary>
    /// Model for one row in a <see cref="DataItemView"/>.
    ///
    /// Maps to shadcn's Item composition:
    ///   ItemMedia → Icon / Avatar / Image
    ///   ItemContent → Title + Description
    ///   ItemActions → ActionLabel (inline link) or ActionsPanel (embedded controls)
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class DataItemModel
    {
        // ── Identity ───────────────────────────────────────────────────
        [Category("DataItem")]
        [Description("Unique key used in ItemClicked events.")]
        [DefaultValue("")]
        public string Key { get; set; } = "";

        // ── Content ────────────────────────────────────────────────────
        [Category("DataItem")]
        [Description("Primary text (title line).")]
        [DefaultValue("Item")]
        public string Title { get; set; } = "Item";

        [Category("DataItem")]
        [Description("Secondary text (description line). Empty = title centred vertically.")]
        [DefaultValue("")]
        public string Description { get; set; } = "";

        // ── Media ──────────────────────────────────────────────────────
        [Category("DataItem — Media")]
        [Description("Rendering mode of the left media zone.")]
        [DefaultValue(DataItemMediaVariant.None)]
        public DataItemMediaVariant MediaVariant { get; set; } = DataItemMediaVariant.None;

        [Category("DataItem — Media")]
        [Description("FontAwesome icon (used when MediaVariant = Icon).")]
        [DefaultValue(IconChar.None)]
        public IconChar Icon { get; set; } = IconChar.None;

        [Category("DataItem — Media")]
        [Description("1–2 upper-case initials (used when MediaVariant = Avatar).")]
        [DefaultValue("")]
        public string AvatarInitials { get; set; } = "";

        [Category("DataItem — Media")]
        [Description("Bitmap image (used when MediaVariant = Image).")]
        [DefaultValue(null)]
        public Image MediaImage { get; set; }

        // ── Style ──────────────────────────────────────────────────────
        [Category("DataItem — Style")]
        [Description("Visual variant of the row.")]
        [DefaultValue(DataItemVariant.Default)]
        public DataItemVariant Variant { get; set; } = DataItemVariant.Default;

        [Category("DataItem — Style")]
        [Description("Height preset.")]
        [DefaultValue(DataItemSize.Default)]
        public DataItemSize Size { get; set; } = DataItemSize.Default;

        [Category("DataItem — Style")]
        [Description("Optional badge pill (New / Beta).")]
        [DefaultValue(SidebarBadge.None)]
        public SidebarBadge Badge { get; set; } = SidebarBadge.None;

        // ── Action ─────────────────────────────────────────────────────
        [Category("DataItem — Action")]
        [Description("Text of the inline action link rendered on the right. Empty = no link.")]
        [DefaultValue("")]
        public string ActionLabel { get; set; } = "";

        public override string ToString() =>
            string.IsNullOrEmpty(Title) ? "(sin título)" : $"• {Title}";
    }
}