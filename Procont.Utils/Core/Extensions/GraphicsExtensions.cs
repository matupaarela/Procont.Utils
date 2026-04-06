using Procont.Utils.Components.Sidebar.Models;
using Procont.Utils.Core.Theming;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Procont.Utils.Core.Extensions
{
    /// <summary>
    /// Métodos de extensión sobre <see cref="Graphics"/> reutilizables
    /// por todos los componentes de Procont.Utils.
    ///
    /// Consolida las utilidades de dibujo que antes estaban duplicadas
    /// (DrawRoundedRect en SidebarHeaderControl, DrawBadge en SidebarTheme, etc.)
    /// </summary>
    public static class GraphicsExtensions
    {
        // ══════════════════════════════════════════════════════════════
        // ROUNDED RECTANGLES
        // ══════════════════════════════════════════════════════════════

        /// <summary>Rellena un rectángulo con esquinas redondeadas.</summary>
        public static void FillRoundedRect(
            this Graphics g, Brush brush, Rectangle rect, int radius)
        {
            using (var path = BuildRoundedPath(rect, radius))
                g.FillPath(brush, path);
        }

        /// <summary>Dibuja el borde de un rectángulo con esquinas redondeadas.</summary>
        public static void DrawRoundedRect(
            this Graphics g, Pen pen, Rectangle rect, int radius)
        {
            using (var path = BuildRoundedPath(rect, radius))
                g.DrawPath(pen, path);
        }

        /// <summary>Rellena Y dibuja borde de un rectángulo redondeado en una sola pasada.</summary>
        public static void FillAndDrawRoundedRect(
            this Graphics g, Brush fill, Pen pen, Rectangle rect, int radius)
        {
            using (var path = BuildRoundedPath(rect, radius))
            {
                g.FillPath(fill, path);
                g.DrawPath(pen, path);
            }
        }

        /// <summary>Construye un <see cref="GraphicsPath"/> redondeado. Caller debe disponer.</summary>
        public static GraphicsPath BuildRoundedPath(Rectangle rect, int radius)
        {
            int d = radius * 2;
            var path = new GraphicsPath();
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        // ══════════════════════════════════════════════════════════════
        // BADGES (SidebarBadge) — movido desde SidebarTheme
        // ══════════════════════════════════════════════════════════════

        /// <summary>
        /// Calcula el ancho en px de la pill de insignia (incluye padding).
        /// Retorna 0 si <paramref name="badge"/> es <c>None</c>.
        /// </summary>
        public static int MeasureBadgeWidth(this Graphics g, SidebarBadge badge)
        {
            if (badge == SidebarBadge.None) return 0;
            string text = badge == SidebarBadge.New ? "NEW" : "BETA";
            return (int)g.MeasureString(text, ProcontTheme.FontBadge).Width + 10;
        }

        /// <summary>
        /// Dibuja la pill de insignia centrada verticalmente en el punto dado.
        /// </summary>
        /// <param name="g">Contexto gráfico.</param>
        /// <param name="badge">Tipo de insignia.</param>
        /// <param name="rightEdge">Coordenada X del borde derecho de la pill.</param>
        /// <param name="centerY">Centro vertical.</param>
        public static void DrawBadge(
            this Graphics g, SidebarBadge badge, int rightEdge, int centerY)
        {
            if (badge == SidebarBadge.None) return;

            string text = badge == SidebarBadge.New ? "NEW" : "BETA";
            Color bg = badge == SidebarBadge.New
                ? ProcontTheme.SemanticNew
                : ProcontTheme.SemanticBeta;

            var textSize = g.MeasureString(text, ProcontTheme.FontBadge);
            int pw = (int)textSize.Width + 10;
            int ph = 13;
            int px = rightEdge - pw;
            int py = centerY - ph / 2;

            var rect = new Rectangle(px, py, pw, ph);

            using (var fill = new SolidBrush(bg))
                g.FillRoundedRect(fill, rect, ProcontTheme.RadiusSmall);

            using (var tb = new SolidBrush(ProcontTheme.TextOnColor))
            using (var fmt = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            })
                g.DrawString(text, ProcontTheme.FontBadge, tb,
                    new RectangleF(px, py, pw, ph), fmt);
        }

        // ══════════════════════════════════════════════════════════════
        // UTILITY
        // ══════════════════════════════════════════════════════════════

        /// <summary>
        /// Activa antialiasing y compositing de alta calidad en el contexto gráfico.
        /// Llama esto al inicio de cada OnPaint para consistencia visual.
        /// </summary>
        public static void SetHighQuality(this Graphics g)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.CompositingQuality = CompositingQuality.HighQuality;
        }
    }
}