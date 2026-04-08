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
        public static Color InputBackground => ProcontTheme.SurfaceInput;
        public static Color InputBorder => ProcontTheme.BorderDefault;
        public static Color InputBorderFocus => ProcontTheme.BorderFocus;
        public static Color InputText => ProcontTheme.TextPrimary;
        public static Color InputPlaceholder => ProcontTheme.TextPlaceholder;
        public static Color ChevronColor => ProcontTheme.TextSubdued;

        // ── Search input (dentro del dropdown) ────────────────────────
        public static Color SearchBackground => ProcontTheme.SurfaceInput;

        // ── Dropdown ──────────────────────────────────────────────────
        public static Color DropdownBackground => ProcontTheme.SurfacePopup;
        public static Color DropdownBorder => ProcontTheme.BorderDefault;
        public static Color ItemBackground => ProcontTheme.SurfacePopup;
        public static Color ItemHover => ProcontTheme.SurfaceHover;
        public static Color ItemSelected => ProcontTheme.SurfaceActive;
        public static Color ItemText => ProcontTheme.TextPrimary;
        public static Color ItemSubtitle => ProcontTheme.TextSubdued;
        public static Color ItemIconColor => ProcontTheme.TextSubdued;
        public static Color ItemIconActive => ProcontTheme.TextAccent;

        // ── Empty state ───────────────────────────────────────────────
        public static Color EmptyText => ProcontTheme.TextSubdued;
        public static Color EmptyAccent => ProcontTheme.TextAccent;

        // ── Action button (sticky) ────────────────────────────────────
        public static Color ActionBackground => ProcontTheme.SurfaceAction;
        public static Color ActionBackgroundHover => ProcontTheme.SurfaceActive;
        public static Color ActionBorderTop => ProcontTheme.BorderDefault;
        public static Color ActionText => ProcontTheme.TextAccent;
        public static Color ActionIcon => ProcontTheme.TextAccent;

        // ── Fonts ─────────────────────────────────────────────────────
        public static Font FontInput => ProcontTheme.FontBase;
        public static Font FontItem => ProcontTheme.FontBase;
        public static Font FontSubtitle => ProcontTheme.FontSmall;
        public static Font FontEmpty => ProcontTheme.FontBase;
        public static Font FontAction => ProcontTheme.FontBold;

        // ── Metrics ───────────────────────────────────────────────────
        public const int InputHeight = ProcontTheme.HeightInput;    // 36 — control principal
        public const int SearchInputHeight = 38;                          // input dentro del dropdown
        public const int ItemHeight = ProcontTheme.HeightItem;     // 34
        public const int ActionHeight = ProcontTheme.HeightAction;   // 38
        public const int EmptyStateHeight = 72;
        public const int BorderRadius = ProcontTheme.RadiusSmall;    // 4
        public const int DropdownRadius = ProcontTheme.RadiusMedium;   // 8
        public const int MaxVisibleItems = 10;
        public const int IconSize = ProcontTheme.IconSizeBase;   // 16
        public const int PaddingH = 12;
    }
}