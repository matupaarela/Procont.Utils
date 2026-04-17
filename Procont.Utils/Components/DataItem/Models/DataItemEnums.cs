using System.ComponentModel;

namespace Procont.Utils.Components.DataItem.Models
{
    /// <summary>Visual style of a DataItemControl row.</summary>
    public enum DataItemVariant
    {
        [Description("Transparent background, no border.")]
        Default,

        [Description("Transparent background with a 1-px border.")]
        Outline,

        [Description("Subtle muted background for secondary content.")]
        Muted
    }

    /// <summary>Height preset of a DataItemControl.</summary>
    public enum DataItemSize
    {
        [Description("Standard height (64 px) with room for title + description.")]
        Default,

        [Description("Compact height (48 px).")]
        Small,

        [Description("Minimal height (36 px) — title only.")]
        XSmall
    }

    /// <summary>How the left media zone renders.</summary>
    public enum DataItemMediaVariant
    {
        [Description("No media zone.")]
        None,

        [Description("FontAwesome icon inside a tinted rounded square.")]
        Icon,

        [Description("Two-letter initials inside a circle.")]
        Avatar,

        [Description("Bitmap image clipped to a rounded square.")]
        Image
    }
}