using Procont.Utils.Components.Sidebar.Models;
using Procont.Utils.Core.Extensions;
using Procont.Utils.Core.Theming;
using System.Drawing;

namespace Procont.Utils.Sidebar
{
    /// <summary>
    /// Alias semánticos del Sidebar sobre <see cref="ProcontTheme"/>.
    ///
    /// ── COMPATIBILIDAD ───────────────────────────────────────────────
    /// Todos los nombres públicos originales se mantienen para no romper
    /// el código existente. Internamente son referencias a ProcontTheme.
    ///
    /// ── NUEVO CÓDIGO ─────────────────────────────────────────────────
    /// Preferir usar ProcontTheme directamente en controles nuevos.
    /// SidebarTheme solo se justifica para tokens semánticos propios
    /// del sidebar que no tienen equivalente en el tema global.
    /// </summary>
    public static class SidebarTheme
    {
        // ── Backgrounds (alias → ProcontTheme) ────────────────────────
        public static Color BackgroundDark => ProcontTheme.SurfaceDark;
        public static Color BackgroundActive => ProcontTheme.SurfaceActive;
        public static Color BackgroundHover => ProcontTheme.SurfaceHover;
        public static Color BackgroundHeader => ProcontTheme.SurfaceHeader;

        // ── Text (alias → ProcontTheme) ────────────────────────────────
        public static Color TextPrimary => ProcontTheme.TextPrimary;
        public static Color TextAccent => ProcontTheme.TextAccent;
        public static Color TextSubdued => ProcontTheme.TextSubdued;

        // ── Borders (alias → ProcontTheme) ────────────────────────────
        public static Color BorderColor => ProcontTheme.BorderDefault;

        // ── Fonts (alias → ProcontTheme) ──────────────────────────────
        public static Font FontGroupTitle => ProcontTheme.FontBold;
        public static Font FontMenuItem => ProcontTheme.FontBase;
        public static Font FontHeader => ProcontTheme.FontBase;
        public static Font FontHeaderBold => ProcontTheme.FontLarge;
        public static Font FontHeaderSmall => ProcontTheme.FontSmall;
        public static Font FontBadge => ProcontTheme.FontBadge;

        // ── Sizes (alias → ProcontTheme) ──────────────────────────────
        public const int SidebarWidth = ProcontTheme.SidebarWidth;
        public const int GroupHeight = ProcontTheme.HeightGroup;
        public const int ItemHeight = ProcontTheme.HeightItem;
        public const int HeaderHeight = ProcontTheme.HeightHeader;
        public const int IconSize = ProcontTheme.IconSizeLarge;
        public const int IndentSubItem = 38;   // específico del sidebar, queda aquí

        // ══════════════════════════════════════════════════════════════
        // BADGE HELPERS — delegan a GraphicsExtensions
        // Mantenidos por compatibilidad con llamadas existentes.
        // ══════════════════════════════════════════════════════════════

        /// <summary>
        /// Calcula el ancho en px de la pill de insignia.
        /// Retorna 0 si badge == None.
        /// </summary>
        public static int GetBadgeWidth(Graphics g, SidebarBadge badge)
            => g.MeasureBadgeWidth(badge);

        /// <summary>
        /// Dibuja la pill de insignia centrada verticalmente.
        /// </summary>
        public static void DrawBadge(Graphics g, SidebarBadge badge, int rightEdge, int centerY)
            => g.DrawBadge(badge, rightEdge, centerY);
    }
}