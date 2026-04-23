using FontAwesome.Sharp;
using Procont.Utils.Core.Extensions;
using Procont.Utils.Core.Theming;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Procont.Utils.Components.DataItem
{
    /// <summary>
    /// Botón dividido estilo [Label | ▼]: el lado izquierdo dispara la acción primaria
    /// y el chevron de la derecha abre un ContextMenuStrip de opciones secundarias.
    ///
    /// ── USO DIRECTO ──────────────────────────────────────────────────
    ///   var btn = new SplitActionButton("Invite", IconChar.UserPlus);
    ///   btn.PrimaryClicked += (s, e) => Invite();
    ///   btn.AddOption("View Profile", IconChar.User,  (s, e) => OpenProfile());
    ///   btn.AddOption("Remove",       IconChar.Trash, (s, e) => Remove());
    ///   item.ActionsPanel.Controls.Add(btn);
    ///
    /// ── VÍA BUILDER ──────────────────────────────────────────────────
    ///   view.AddItem("user", "Alice Smith", "alice@example.com")
    ///       .WithSplitButton("Follow", IconChar.UserPlus, btn =>
    ///       {
    ///           btn.AddOption("Unfollow", IconChar.UserMinus, (s, e) => Unfollow());
    ///           btn.AddOption("Block",    IconChar.Ban,       (s, e) => Block());
    ///       });
    ///
    /// ── SOLO BOTÓN PRIMARIO (sin dropdown) ───────────────────────────
    ///   Si no se llama AddOption(), el chevron se oculta y el control
    ///   se comporta como un simple IconButton con texto.
    /// </summary>
    public sealed class SplitActionButton : Control
    {
        // ── Layout ────────────────────────────────────────────────────
        private const int ArrowZoneW = 22;
        private const int PadH = 10;
        private const int ControlH = 26;
        private const int IconSize = 13;

        // ── Estado ────────────────────────────────────────────────────
        private bool _hoverMain = false;
        private bool _hoverArrow = false;
        private bool _menuOpen = false;

        // ── Datos ─────────────────────────────────────────────────────
        private string _label;
        private IconChar _icon;
        private readonly ContextMenuStrip _menu;

        // ── Evento ────────────────────────────────────────────────────
        /// <summary>Se dispara al hacer clic en la parte izquierda (acción primaria).</summary>
        public event EventHandler PrimaryClicked;

        // ── Propiedades ───────────────────────────────────────────────

        public string ButtonLabel
        {
            get => _label;
            set { _label = value ?? ""; RecalcWidth(); Invalidate(); }
        }

        public IconChar ButtonIcon
        {
            get => _icon;
            set { _icon = value; RecalcWidth(); Invalidate(); }
        }

        /// <summary>true si hay al menos una opción en el menú (muestra la zona del chevron).</summary>
        public bool HasDropdown => _menu.Items.Count > 0;

        // ══════════════════════════════════════════════════════════════
        // CONSTRUCTOR
        // ══════════════════════════════════════════════════════════════

        public SplitActionButton(string label = "Action", IconChar icon = IconChar.None)
        {
            _label = label ?? "Action";
            _icon = icon;

            _menu = new ContextMenuStrip();
            _menu.Renderer = new ToolStripProfessionalRenderer(new DarkMenuColorTable());
            _menu.Opening += (s, e) => { _menuOpen = true; Invalidate(); };
            _menu.Closed += (s, e) => { _menuOpen = false; Invalidate(); };

            Cursor = Cursors.Hand;
            Height = ControlH;

            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.ResizeRedraw, true);

            RecalcWidth();
        }

        // ══════════════════════════════════════════════════════════════
        // API PÚBLICA
        // ══════════════════════════════════════════════════════════════

        /// <summary>Agrega una opción al menú desplegable.</summary>
        public void AddOption(string text, IconChar icon, EventHandler clickHandler = null)
        {
            var item = new ToolStripMenuItem(text);
            if (icon != IconChar.None)
            {
                try { item.Image = icon.ToBitmap(ProcontTheme.TextPrimary, 14); }
                catch { /* ignorar si FontAwesome falla */ }
            }
            if (clickHandler != null) item.Click += clickHandler;
            _menu.Items.Add(item);
            RecalcWidth(); // puede necesitar espacio extra para el arrow
            Invalidate();
        }

        /// <summary>Agrega un separador al menú.</summary>
        public void AddSeparator() => _menu.Items.Add(new ToolStripSeparator());

        // ══════════════════════════════════════════════════════════════
        // MOUSE
        // ══════════════════════════════════════════════════════════════

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            bool onArrow = HasDropdown && e.X >= Width - ArrowZoneW;
            if (onArrow == _hoverArrow && !onArrow == _hoverMain) return;
            _hoverArrow = onArrow;
            _hoverMain = !onArrow;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _hoverArrow = false;
            _hoverMain = false;
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button != MouseButtons.Left) return;

            if (HasDropdown && e.X >= Width - ArrowZoneW)
                _menu.Show(this, new Point(0, Height));
            else
                PrimaryClicked?.Invoke(this, EventArgs.Empty);
        }

        // ══════════════════════════════════════════════════════════════
        // PAINT
        // ══════════════════════════════════════════════════════════════

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SetHighQuality();

            var fullRect = new Rectangle(0, 0, Width - 1, Height - 1);
            int r = ProcontTheme.RadiusSmall;

            // ── Fondo base ────────────────────────────────────────────
            Color bg = (_menuOpen || _hoverMain || _hoverArrow)
                ? ProcontTheme.SurfaceHover
                : ProcontTheme.SurfaceActive;

            using (var fill = new SolidBrush(bg))
                g.FillRoundedRect(fill, fullRect, r);

            // ── Highlight extra sobre la zona del arrow ───────────────
            if (_hoverArrow && HasDropdown)
            {
                var arrowZone = new Rectangle(Width - ArrowZoneW - 1, 0, ArrowZoneW, Height - 1);
                using (var path = RightRoundedPath(arrowZone, r))
                using (var fill = new SolidBrush(Color.FromArgb(25, 255, 255, 255)))
                    g.FillPath(fill, path);
            }

            // ── Borde ─────────────────────────────────────────────────
            using (var pen = new Pen(ProcontTheme.BorderDefault, 1f))
                g.DrawRoundedRect(pen, fullRect, r);

            // ── Divisor entre main y arrow ────────────────────────────
            int effectiveWidth = HasDropdown ? Width - ArrowZoneW - 1 : Width - 1;

            if (HasDropdown)
            {
                using (var pen = new Pen(ProcontTheme.BorderDefault, 1f))
                    g.DrawLine(pen, effectiveWidth, 3, effectiveWidth, Height - 4);
            }

            // ── Ícono ─────────────────────────────────────────────────
            int x = PadH;
            if (_icon != IconChar.None)
            {
                int iy = (Height - IconSize) / 2;
                try
                {
                    using (var bmp = _icon.ToBitmap(ProcontTheme.TextPrimary, IconSize))
                        g.DrawImage(bmp, x, iy, IconSize, IconSize);
                }
                catch { }
                x += IconSize + 5;
            }

            // ── Label ─────────────────────────────────────────────────
            int textW = effectiveWidth - x - 4;
            if (textW > 0 && !string.IsNullOrEmpty(_label))
            {
                using (var b = new SolidBrush(ProcontTheme.TextPrimary))
                using (var fmt = new StringFormat
                {
                    LineAlignment = StringAlignment.Center,
                    Alignment = StringAlignment.Near,
                    Trimming = StringTrimming.EllipsisCharacter
                })
                    g.DrawString(_label, ProcontTheme.FontBase, b,
                        new RectangleF(x, 0, textW, Height), fmt);
            }

            // ── Chevron ───────────────────────────────────────────────
            if (HasDropdown)
            {
                int cx = Width - ArrowZoneW / 2 - 1;
                int cy = Height / 2;
                int half = 3;

                Color cc = _hoverArrow ? ProcontTheme.TextPrimary : ProcontTheme.TextSubdued;
                using (var pen = new Pen(cc, 1.5f)
                { StartCap = LineCap.Round, EndCap = LineCap.Round })
                {
                    if (_menuOpen)
                    {
                        g.DrawLine(pen, cx - half, cy + 1, cx, cy - half + 1);
                        g.DrawLine(pen, cx, cy - half + 1, cx + half, cy + 1);
                    }
                    else
                    {
                        g.DrawLine(pen, cx - half, cy - 1, cx, cy + half - 1);
                        g.DrawLine(pen, cx, cy + half - 1, cx + half, cy - 1);
                    }
                }
            }
        }

        // ══════════════════════════════════════════════════════════════
        // SIZING
        // ══════════════════════════════════════════════════════════════

        private void RecalcWidth()
        {
            int textW = string.IsNullOrEmpty(_label) ? 0
                : TextRenderer.MeasureText(_label, ProcontTheme.FontBase).Width;
            int iconW = _icon != IconChar.None ? IconSize + 5 : 0;
            int arrow = HasDropdown ? ArrowZoneW + 2 : 0;
            Width = PadH + iconW + textW + 6 + arrow;
            Height = ControlH;
        }

        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            // Altura siempre fija; ancho libre
            base.SetBoundsCore(x, y, width, ControlH, specified);
        }

        // ══════════════════════════════════════════════════════════════
        // HELPERS
        // ══════════════════════════════════════════════════════════════

        /// <summary>
        /// GraphicsPath con solo las esquinas derechas redondeadas (para el highlight del arrow).
        /// </summary>
        private static GraphicsPath RightRoundedPath(Rectangle rect, int r)
        {
            int d = r * 2;
            var path = new GraphicsPath();
            path.AddLine(rect.X, rect.Y, rect.Right - r, rect.Y);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddLine(rect.Right - r, rect.Bottom, rect.X, rect.Bottom);
            path.CloseFigure();
            return path;
        }

        // ── Tabla de colores del dropdown para que coincida con ProcontTheme ──

        private sealed class DarkMenuColorTable : ProfessionalColorTable
        {
            public override Color MenuItemSelected
                => ProcontTheme.SurfaceHover;
            public override Color MenuItemSelectedGradientBegin
                => ProcontTheme.SurfaceHover;
            public override Color MenuItemSelectedGradientEnd
                => ProcontTheme.SurfaceHover;
            public override Color MenuBorder
                => ProcontTheme.BorderDefault;
            public override Color ToolStripDropDownBackground
                => ProcontTheme.SurfacePopup;
            public override Color ImageMarginGradientBegin
                => ProcontTheme.SurfacePopup;
            public override Color ImageMarginGradientMiddle
                => ProcontTheme.SurfacePopup;
            public override Color ImageMarginGradientEnd
                => ProcontTheme.SurfacePopup;
            public override Color MenuItemBorder
                => ProcontTheme.BorderDefault;
        }

        // ══════════════════════════════════════════════════════════════
        // DISPOSE
        // ══════════════════════════════════════════════════════════════

        protected override void Dispose(bool disposing)
        {
            if (disposing) _menu?.Dispose();
            base.Dispose(disposing);
        }
    }
}