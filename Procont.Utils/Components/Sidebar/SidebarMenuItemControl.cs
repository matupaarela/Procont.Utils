using FontAwesome.Sharp;
using Procont.Utils.Components.Sidebar.Models;
using Procont.Utils.Sidebar;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Procont.Utils.Components.Sidebar
{
    /// <summary>
    /// Ítem hoja del sidebar.
    /// - Key   : identificador lógico (no cambia aunque el texto cambie).
    /// - Icon  : ícono FontAwesome.Sharp (IconChar.*).
    /// - Badge : insignia informativa opcional (None / New / Beta).
    /// </summary>
    [ToolboxItem(false)]
    [DesignTimeVisible(false)]
    public class SidebarMenuItemControl : Control
    {
        // ── Backing fields ─────────────────────────────────────────────
        private string _itemText = "Ítem";
        private string _key = "";
        private IconChar _iconChar = IconChar.None;
        private SidebarBadge _badge = SidebarBadge.None;
        private bool _isActive = false;
        private bool _isHovered = false;
        private readonly int _level;

        // ── Propiedades ────────────────────────────────────────────────

        [Category("Sidebar")]
        [Description("Texto visible del ítem.")]
        [DefaultValue("Ítem")]
        public string ItemText
        {
            get => _itemText;
            set { _itemText = value; Invalidate(); }
        }

        [Category("Sidebar")]
        [Description("Clave única para identificar este ítem sin depender del texto.")]
        [DefaultValue("")]
        public string Key
        {
            get => _key;
            set => _key = value;
        }

        [Category("Sidebar")]
        [Description("Ícono FontAwesome del ítem.")]
        [DefaultValue(IconChar.None)]
        public IconChar Icon
        {
            get => _iconChar;
            set { _iconChar = value; Invalidate(); }
        }

        [Category("Sidebar")]
        [Description("Insignia informativa: None, New (verde) o Beta (ámbar).")]
        [DefaultValue(SidebarBadge.None)]
        public SidebarBadge Badge
        {
            get => _badge;
            set { _badge = value; Invalidate(); }
        }

        [Category("Sidebar")]
        [Description("Indica si este ítem está seleccionado actualmente.")]
        [DefaultValue(false)]
        public bool IsActive
        {
            get => _isActive;
            set { _isActive = value; Invalidate(); }
        }

        // ── Ocultar propiedades heredadas irrelevantes ─────────────────
        [Browsable(false)] public override string Text { get => base.Text; set => base.Text = value; }
        [Browsable(false)] public override Color BackColor { get => base.BackColor; set => base.BackColor = value; }
        [Browsable(false)] public override Color ForeColor { get => base.ForeColor; set => base.ForeColor = value; }

        // ── Propiedades resueltas al construir el árbol ────────────────

        [Browsable(false)]
        public string BreadcrumbPath { get; internal set; } = "";

        [Browsable(false)]
        public IconChar ResolvedIcon { get; internal set; } = IconChar.None;

        // ── Evento ────────────────────────────────────────────────────
        public event EventHandler ItemSelected;

        // ── Constructores ─────────────────────────────────────────────
        public SidebarMenuItemControl() : this("Ítem", "", IconChar.None, 1) { }

        public SidebarMenuItemControl(string text, string key = "", IconChar icon = IconChar.None, int level = 1)
        {
            _itemText = text;
            _key = key;
            _iconChar = icon;
            _level = level;

            Height = SidebarTheme.ItemHeight;
            Dock = DockStyle.Top;
            Cursor = Cursors.Hand;

            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint, true);
        }

        // ── Hover / Click ─────────────────────────────────────────────
        protected override void OnMouseEnter(EventArgs e) { base.OnMouseEnter(e); _isHovered = true; Invalidate(); }
        protected override void OnMouseLeave(EventArgs e) { base.OnMouseLeave(e); _isHovered = false; Invalidate(); }
        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            IsActive = true;
            ItemSelected?.Invoke(this, EventArgs.Empty);
        }

        // ── Pintura ───────────────────────────────────────────────────
        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            Color bg = _isActive ? SidebarTheme.BackgroundActive :
                       _isHovered ? SidebarTheme.BackgroundHover :
                                    SidebarTheme.BackgroundDark;
            g.Clear(bg);

            // Barra lateral izquierda (ítem activo)
            if (_isActive)
            {
                using (var b = new SolidBrush(SidebarTheme.TextAccent))
                    g.FillRectangle(b, 0, 5, 3, Height - 10);
            }

            Color textColor = _isActive ? SidebarTheme.TextAccent : SidebarTheme.TextPrimary;
            Color dotColor = _isActive ? SidebarTheme.TextAccent : SidebarTheme.TextSubdued;

            int baseX = 14 + (_level * 18);

            if (_level > 0)
            {
                int connectorX = 14 + ((_level - 1) * 18) + 7;

                using (var pen = new Pen(SidebarTheme.BorderColor, 1))
                {
                    // Líneas verticales de los niveles ancestro
                    for (int lvl = 1; lvl < _level; lvl++)
                    {
                        int lx = 14 + (lvl * 18) - 11;
                        g.DrawLine(pen, lx, 0, lx, Height);
                    }

                    // Línea vertical del nivel actual (igual que GroupHeader.PaintLevelN)
                    g.DrawLine(pen, connectorX, 0, connectorX, Height);

                    // Guión horizontal conector
                    g.DrawLine(pen, connectorX, Height / 2, baseX, Height / 2);
                }
            }


            // Ícono
            if (_iconChar != IconChar.None)
            {
                int iconSize = 14, iconX = baseX - 2, iconY = (Height - iconSize) / 2;
                using (var bmp = _iconChar.ToBitmap(dotColor, iconSize))
                    g.DrawImage(bmp, iconX, iconY, iconSize, iconSize);
            }

            // Badge — reservar espacio a la derecha
            int badgeWidth = SidebarTheme.GetBadgeWidth(g, _badge);
            int badgeReserve = badgeWidth > 0 ? badgeWidth + 6 : 0;

            // Texto
            int textOffsetX = _iconChar != IconChar.None ? baseX + 16 : baseX;

            using (var brush = new SolidBrush(textColor))
            {
                var rect = new Rectangle(textOffsetX, 0,
                    Width - textOffsetX - 8 - badgeReserve, Height);
                var fmt = new StringFormat
                {
                    Alignment = StringAlignment.Near,
                    LineAlignment = StringAlignment.Center,
                    Trimming = StringTrimming.EllipsisCharacter
                };
                g.DrawString(_itemText, SidebarTheme.FontMenuItem, brush, rect, fmt);
            }

            // Badge pill
            if (_badge != SidebarBadge.None)
                SidebarTheme.DrawBadge(g, _badge, Width - 6, Height / 2);
        }
    }
}