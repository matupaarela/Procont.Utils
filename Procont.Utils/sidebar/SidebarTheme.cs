using Procont.Utils.sidebar.Models;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Procont.Utils.Sidebar
{
    /// <summary>
    /// Colores y fuentes centralizados del Sidebar.
    /// Cambia aquí para re-tematizar toda la barra.
    /// </summary>
    public static class SidebarTheme
    {
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

        // ── Badge ──────────────────────────────────────────────────────
        /// <summary>Color de fondo de la insignia «NEW» (verde).</summary>
        public static readonly Color BadgeNewBackground = Color.FromArgb(22, 163, 74);
        /// <summary>Color de fondo de la insignia «BETA» (ámbar).</summary>
        public static readonly Color BadgeBetaBackground = Color.FromArgb(202, 107, 0);

        // ── Fonts ──────────────────────────────────────────────────────
        public static readonly Font FontGroupTitle = new Font("Segoe UI", 8.5f, FontStyle.Bold);
        public static readonly Font FontMenuItem = new Font("Segoe UI", 8.5f, FontStyle.Regular);
        public static readonly Font FontHeader = new Font("Segoe UI", 8f, FontStyle.Regular);
        public static readonly Font FontHeaderBold = new Font("Segoe UI", 10f, FontStyle.Bold);
        public static readonly Font FontHeaderSmall = new Font("Segoe UI", 7.5f, FontStyle.Regular);
        public static readonly Font FontBadge = new Font("Segoe UI", 6.5f, FontStyle.Bold);

        // ── Sizes ──────────────────────────────────────────────────────
        public const int SidebarWidth = 260;
        public const int GroupHeight = 42;
        public const int ItemHeight = 36;
        public const int HeaderHeight = 110;
        public const int IconSize = 18;
        public const int IndentSubItem = 38;

        // ══════════════════════════════════════════════════════════════
        // BADGE HELPER
        // ══════════════════════════════════════════════════════════════

        /// <summary>
        /// Calcula el ancho en píxeles que ocupará la pill de la insignia
        /// (incluye padding interno). Devuelve 0 si badge == None.
        /// Úsalo para reservar espacio antes de dibujar el texto del nodo.
        /// </summary>
        public static int GetBadgeWidth(Graphics g, SidebarBadge badge)
        {
            if (badge == SidebarBadge.None) return 0;
            string text = badge == SidebarBadge.New ? "NEW" : "BETA";
            return (int)g.MeasureString(text, FontBadge).Width + 10;
        }

        /// <summary>
        /// Dibuja la pill de la insignia centrada verticalmente.
        /// </summary>
        /// <param name="g">Contexto gráfico del control.</param>
        /// <param name="badge">Tipo de insignia a dibujar.</param>
        /// <param name="rightEdge">Coordenada X del borde derecho de la pill.</param>
        /// <param name="centerY">Coordenada Y del centro vertical.</param>
        public static void DrawBadge(Graphics g, SidebarBadge badge, int rightEdge, int centerY)
        {
            if (badge == SidebarBadge.None) return;

            string text = badge == SidebarBadge.New ? "NEW" : "BETA";
            Color bg = badge == SidebarBadge.New ? BadgeNewBackground : BadgeBetaBackground;

            var textSize = g.MeasureString(text, FontBadge);
            int pw = (int)textSize.Width + 10;
            int ph = 13;
            int px = rightEdge - pw;
            int py = centerY - ph / 2;

            var rect = new Rectangle(px, py, pw, ph);

            using (var path = CreateRoundedPath(rect, 4))
            using (var fill = new SolidBrush(bg))
                g.FillPath(fill, path);

            using (var tb = new SolidBrush(Color.White))
            using (var fmt = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            })
                g.DrawString(text, FontBadge, tb, new RectangleF(px, py, pw, ph), fmt);
        }

        // ── Rounded-rect helper (privado) ──────────────────────────────
        private static GraphicsPath CreateRoundedPath(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            int d = radius * 2;
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}