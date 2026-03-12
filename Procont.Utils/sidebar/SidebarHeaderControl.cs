using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Procont.Utils.Sidebar
{
    /// <summary>
    /// Encabezado del Sidebar: logo + nombre de empresa + RUC/módulo.
    /// Todas sus propiedades son editables desde el diseñador de Visual Studio.
    /// </summary>
    [ToolboxItem(false)]   // No aparece solo en toolbox; lo gestiona SidebarControl
    [DesignTimeVisible(false)]
    public class SidebarHeaderControl : Control
    {
        // ── Backing fields ─────────────────────────────────────────────
        private string _companyName = "EMPRESA S.A.";
        private string _companySubtitle = "Razón social completa";
        private string _ruc = "00000000000";
        private string _module = "Módulo";
        private Image _logo = null;

        // ── Propiedades con atributos de diseñador ────────────────────

        [Category("Empresa")]
        [Description("Nombre corto de la empresa (aparece en la cabecera).")]
        [DefaultValue("EMPRESA S.A.")]
        public string CompanyName
        {
            get => _companyName;
            set { _companyName = value; Invalidate(); }
        }

        [Category("Empresa")]
        [Description("Razón social completa.")]
        [DefaultValue("Razón social completa")]
        public string CompanySubtitle
        {
            get => _companySubtitle;
            set { _companySubtitle = value; Invalidate(); }
        }

        [Category("Empresa")]
        [Description("Número de RUC.")]
        [DefaultValue("00000000000")]
        public string Ruc
        {
            get => _ruc;
            set { _ruc = value; Invalidate(); }
        }

        [Category("Empresa")]
        [Description("Nombre del módulo o área (Ventas, Compras, etc.).")]
        [DefaultValue("Módulo")]
        public string Module
        {
            get => _module;
            set { _module = value; Invalidate(); }
        }

        [Category("Empresa")]
        [Description("Logo de la empresa. Si es null se muestra un ícono placeholder.")]
        [DefaultValue(null)]
        public Image Logo
        {
            get => _logo;
            set { _logo = value; Invalidate(); }
        }

        // ── Constructor ───────────────────────────────────────────────
        public SidebarHeaderControl()
        {
            Height = SidebarTheme.HeaderHeight;
            Dock = DockStyle.Top;
            BackColor = SidebarTheme.BackgroundHeader;

            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint, true);
        }

        // ── Pintura ───────────────────────────────────────────────────
        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.Clear(SidebarTheme.BackgroundHeader);

            int logoSize = 50, logoX = 14, logoY = 16;

            if (_logo != null)
            {
                g.DrawImage(_logo, new Rectangle(logoX, logoY, logoSize, logoSize));
            }
            else
            {
                using (var pen = new Pen(SidebarTheme.TextAccent, 1.5f))
                using (var fill = new SolidBrush(Color.FromArgb(30, 245, 168, 30)))
                {
                    DrawRoundedRect(g, fill, pen, new Rectangle(logoX, logoY, logoSize, logoSize), 8);
                    using (var b = new SolidBrush(SidebarTheme.TextAccent))
                    {
                        var fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                        g.DrawString("🛍", new Font("Segoe UI Emoji", 18), b,
                            new System.Drawing.RectangleF(logoX, logoY, logoSize, logoSize), fmt);
                    }
                }
            }

            // Nombre empresa
            string[] parts = _companyName.Split(' ');
            string line1 = parts.Length > 0 ? parts[0] : _companyName;
            string line2 = parts.Length > 1 ? string.Join(" ", parts, 1, parts.Length - 1) : "";
            int textX = logoX + logoSize + 10;

            using (var b = new SolidBrush(SidebarTheme.TextAccent))
            {
                g.DrawString(line1, SidebarTheme.FontHeaderBold, b, textX, logoY + 2);
                if (!string.IsNullOrEmpty(line2))
                    g.DrawString(line2, new Font("Segoe UI", 8f, FontStyle.Bold), b, textX, logoY + 18);
            }

            // Línea divisoria
            using (var pen = new Pen(SidebarTheme.BorderColor, 1))
                g.DrawLine(pen, 14, logoY + logoSize + 8, Width - 14, logoY + logoSize + 8);

            // Subtítulo y RUC
            int infoY = logoY + logoSize + 14;
            using (var b = new SolidBrush(SidebarTheme.TextSubdued))
            {
                var fmt = new StringFormat { Trimming = StringTrimming.EllipsisCharacter };
                g.DrawString(_companySubtitle, SidebarTheme.FontHeaderSmall, b,
                    new System.Drawing.RectangleF(14, infoY, Width - 28, 15), fmt);
                g.DrawString($"{_ruc} - {_module}", SidebarTheme.FontHeaderSmall, b,
                    new System.Drawing.RectangleF(14, infoY + 14, Width - 28, 15), fmt);
            }
        }

        private static void DrawRoundedRect(Graphics g, System.Drawing.Brush fill, System.Drawing.Pen pen, Rectangle rect, int radius)
        {
            var path = new System.Drawing.Drawing2D.GraphicsPath();
            path.AddArc(rect.X, rect.Y, radius * 2, radius * 2, 180, 90);
            path.AddArc(rect.Right - radius * 2, rect.Y, radius * 2, radius * 2, 270, 90);
            path.AddArc(rect.Right - radius * 2, rect.Bottom - radius * 2, radius * 2, radius * 2, 0, 90);
            path.AddArc(rect.X, rect.Bottom - radius * 2, radius * 2, radius * 2, 90, 90);
            path.CloseFigure();
            g.FillPath(fill, path);
            g.DrawPath(pen, path);
        }
    }
}