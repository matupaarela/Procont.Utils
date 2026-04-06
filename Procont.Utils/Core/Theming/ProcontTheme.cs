using System.Drawing;

namespace Procont.Utils.Core.Theming
{
    /// <summary>
    /// Tema visual global de Procont.Utils.
    /// Todos los componentes referencian estas constantes; cambiar aquí
    /// re-tematiza toda la librería de una sola vez.
    ///
    /// ── CONVENCIÓN DE NOMBRES ─────────────────────────────────────────
    ///   Surface*   → fondos de controles y contenedores
    ///   Text*      → colores de texto
    ///   Border*    → colores de bordes y separadores
    ///   Semantic*  → colores con significado funcional (error, éxito, etc.)
    ///   Font*      → fuentes tipográficas
    ///   Height*    → alturas estándar de controles
    ///   Radius*    → radios de esquinas redondeadas
    /// </summary>
    public static class ProcontTheme
    {
        // ── Surfaces ───────────────────────────────────────────────────
        public static Color SurfaceDark = Color.FromArgb(3, 39, 59);
        public static Color SurfaceActive = Color.FromArgb(10, 22, 40);
        public static Color SurfaceHover = Color.FromArgb(15, 30, 55);
        public static Color SurfaceHeader = Color.FromArgb(6, 39, 59);

        /// <summary>Fondo para inputs de texto y campos editables.</summary>
        public static Color SurfaceInput = Color.FromArgb(8, 50, 72);

        /// <summary>Fondo para dropdowns y popups flotantes.</summary>
        public static Color SurfacePopup = Color.FromArgb(6, 44, 66);

        /// <summary>Fondo para el ítem de acción sticky en dropdowns.</summary>
        public static Color SurfaceAction = Color.FromArgb(4, 33, 52);

        // ── Text ───────────────────────────────────────────────────────
        public static Color TextPrimary = Color.FromArgb(230, 230, 230);
        public static Color TextAccent = Color.FromArgb(245, 168, 30);
        public static Color TextSubdued = Color.FromArgb(160, 175, 195);

        /// <summary>Texto gris para placeholder / hint en inputs vacíos.</summary>
        public static Color TextPlaceholder = Color.FromArgb(100, 120, 145);

        /// <summary>Texto blanco puro — usado sobre fondos de color (badges, botones).</summary>
        public static Color TextOnColor = Color.White;

        // ── Borders ────────────────────────────────────────────────────
        public static Color BorderDefault = Color.FromArgb(25, 45, 75);

        /// <summary>Borde resaltado cuando un control tiene el foco.</summary>
        public static Color BorderFocus = Color.FromArgb(245, 168, 30);

        // ── Semantic ───────────────────────────────────────────────────
        public static Color SemanticNew = Color.FromArgb(22, 163, 74);
        public static Color SemanticBeta = Color.FromArgb(202, 107, 0);
        public static Color SemanticDanger = Color.FromArgb(220, 38, 38);
        public static Color SemanticInfo = Color.FromArgb(37, 99, 235);

        // ── Typography ─────────────────────────────────────────────────
        public static readonly Font FontBase = new Font("Segoe UI", 8.5f, FontStyle.Regular);
        public static readonly Font FontBold = new Font("Segoe UI", 8.5f, FontStyle.Bold);
        public static readonly Font FontSmall = new Font("Segoe UI", 7.5f, FontStyle.Regular);
        public static readonly Font FontSmallBold = new Font("Segoe UI", 7.5f, FontStyle.Bold);
        public static readonly Font FontBadge = new Font("Segoe UI", 6.5f, FontStyle.Bold);
        public static readonly Font FontLarge = new Font("Segoe UI", 10f, FontStyle.Bold);

        // ── Control metrics ────────────────────────────────────────────
        public const int HeightInput = 36;
        public const int HeightItem = 34;
        public const int HeightGroup = 42;
        public const int HeightAction = 38;
        public const int HeightHeader = 110;

        public const int RadiusSmall = 4;
        public const int RadiusMedium = 8;
        public const int RadiusLarge = 12;

        public const int IconSizeSmall = 13;
        public const int IconSizeBase = 16;
        public const int IconSizeLarge = 18;

        public const int SidebarWidth = 260;
    }
}