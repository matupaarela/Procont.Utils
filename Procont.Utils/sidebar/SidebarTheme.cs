using System.Drawing;

namespace Procont.Utils.Sidebar
{
    /// <summary>
    /// Colores y fuentes centralizados del Sidebar.
    /// Cambia aquí para re-tematizar toda la barra.
    /// </summary>
    public static class SidebarTheme
    {
		//// ── Backgrounds ────────────────────────────────────────────────
		//public static readonly Color BackgroundDark = Color.FromArgb(10, 22, 40);
		//public static readonly Color BackgroundActive = Color.FromArgb(20, 40, 70);
		//public static readonly Color BackgroundHover = Color.FromArgb(15, 30, 55);
		//public static readonly Color BackgroundHeader = Color.FromArgb(6, 14, 26);

		//// ── Text ───────────────────────────────────────────────────────
		//public static readonly Color TextPrimary = Color.FromArgb(230, 230, 230);
		//public static readonly Color TextAccent = Color.FromArgb(245, 168, 30);
		//public static readonly Color TextSubdued = Color.FromArgb(160, 175, 195);

		//// ── Borders ────────────────────────────────────────────────────
		//public static readonly Color BorderColor = Color.FromArgb(25, 45, 75);

		// ── Backgrounds ────────────────────────────────────────────────
		public static readonly Color BackgroundDark = Color.FromArgb(3, 39, 59);
		public static readonly Color BackgroundActive = Color.FromArgb(10, 22, 40);
		public static readonly Color BackgroundHover = Color.FromArgb(15, 30, 55);
		public static readonly Color BackgroundHeader = Color.FromArgb(6, 39, 59);

		// ── Text ───────────────────────────────────────────────────────
		public static readonly Color TextPrimary = Color.FromArgb(230, 230, 230);
		public static readonly Color TextAccent = Color.FromArgb(245, 168, 30);
		public static readonly Color TextSubdued = Color.FromArgb(160, 175, 195);

		// ── Borders ────────────────────────────────────────────────────
		public static readonly Color BorderColor = Color.FromArgb(25, 45, 75);

		// ── Fonts ──────────────────────────────────────────────────────
		public static readonly Font FontGroupTitle = new Font("Segoe UI", 8.5f, FontStyle.Bold);
        public static readonly Font FontMenuItem = new Font("Segoe UI", 8.5f, FontStyle.Regular);
        public static readonly Font FontHeader = new Font("Segoe UI", 8f, FontStyle.Regular);
        public static readonly Font FontHeaderBold = new Font("Segoe UI", 10f, FontStyle.Bold);
        public static readonly Font FontHeaderSmall = new Font("Segoe UI", 7.5f, FontStyle.Regular);

        // ── Sizes ──────────────────────────────────────────────────────
        public const int SidebarWidth = 260;
        public const int GroupHeight = 42;
        public const int ItemHeight = 36;
        public const int HeaderHeight = 110;
        public const int IconSize = 18;
        public const int IndentSubItem = 38;
    }
}