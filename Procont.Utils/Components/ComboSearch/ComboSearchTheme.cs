using Procont.Utils.Core.Theming;
using System.Drawing;

namespace Procont.Utils.Components.ComboSearch
{
    /// <summary>
    /// Alias semánticos del ComboSearch sobre <see cref="ProcontTheme"/>.
    /// </summary>
    internal static class ComboSearchTheme
    {
        // ── Input box (control principal) ─────────────────────────────
        public static Color InputBackground => SystemColors.Window;
        public static Color InputBorder => SystemColors.ControlDark;
        public static Color InputBorderFocus => SystemColors.GrayText;
        public static Color InputText => SystemColors.WindowText;
        public static Color InputPlaceholder => SystemColors.GrayText;
        public static Color ChevronColor => SystemColors.ControlDark;

        // ── Search input (dentro del dropdown) ────────────────────────
        public static Color SearchBackground => SystemColors.Window;

        // ── Dropdown ──────────────────────────────────────────────────
        public static Color DropdownBackground => Color.White;
        public static Color DropdownBorder => Color.Gray;
        public static Color ItemBackground => SystemColors.Window;
        public static Color ItemHover => Color.FromArgb(
            50, SystemColors.Highlight.R,
                SystemColors.Highlight.G,
                SystemColors.Highlight.B);
        public static Color ItemSelected => SystemColors.Highlight;
        public static Color ItemText => SystemColors.WindowText;
        public static Color ItemSubtitle => SystemColors.GrayText;
        public static Color ItemIconColor => SystemColors.GrayText;
        public static Color ItemIconActive => SystemColors.HighlightText;

        // ── Empty state ───────────────────────────────────────────────
        public static Color EmptyText => SystemColors.GrayText;
        public static Color EmptyAccent => SystemColors.Highlight;

        // ── Action button (sticky) ────────────────────────────────────
        public static Color ActionBackground => SystemColors.Control;
        public static Color ActionBackgroundHover => SystemColors.ControlLight;
        public static Color ActionBorderTop => SystemColors.ControlDark;
        public static Color ActionText => SystemColors.ControlText;
        public static Color ActionIcon => SystemColors.ControlText;

        // ── Fonts (campos estáticos para no allocar en cada llamada) ──
        public static readonly Font FontInput = SystemFonts.DefaultFont;
        public static readonly Font FontItem = SystemFonts.DefaultFont;
        public static readonly Font FontSubtitle = new Font(
            SystemFonts.DefaultFont.FontFamily,
            SystemFonts.DefaultFont.SizeInPoints - 0.5f,
            FontStyle.Regular);
        public static readonly Font FontEmpty = SystemFonts.DefaultFont;
        public static readonly Font FontAction = new Font(
            SystemFonts.DefaultFont,
            FontStyle.Bold);
        // ── Metrics ───────────────────────────────────────────────────
        public const int InputHeight = 23;    // 36 — control principal
        public const int SearchInputHeight = 38;                          // input dentro del dropdown
        public const int ItemHeight = ProcontTheme.HeightItem;     // 34
        public const int ActionHeight = ProcontTheme.HeightAction;   // 38
        public const int EmptyStateHeight = 72;
        public const int BorderRadius = ProcontTheme.RadiusSmall;    // 4
        public const int DropdownRadius = ProcontTheme.RadiusMedium;   // 8
        public const int MaxVisibleItems = 10;
        public const int IconSize = ProcontTheme.IconSizeBase;   // 16
        public const int PaddingH = 3;
    }
}