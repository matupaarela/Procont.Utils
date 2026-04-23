using FontAwesome.Sharp;
using Procont.Utils.Core.Extensions;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Procont.Utils.Components.DataItem
{
    /// <summary>
    /// Botón de acción unificado: puede funcionar como botón simple o como
    /// split button [Label | ▼] con dropdown de opciones secundarias.
    ///
    /// ── SIMPLE ───────────────────────────────────────────────────────
    ///   var btn = new ActionButton { ButtonText = "Edit", ButtonIcon = IconChar.Edit };
    ///   btn.PrimaryClicked += (s, e) => Edit();
    ///
    /// ── SPLIT ────────────────────────────────────────────────────────
    ///   var btn = new ActionButton { ButtonText = "Follow", IsSplit = true };
    ///   btn.PrimaryClicked += (s, e) => Follow();
    ///   btn.AddOption("Unfollow", IconChar.UserMinus, (s, e) => Unfollow());
    ///   btn.AddOption("Block",    IconChar.Ban,       (s, e) => Block());
    ///
    /// ── COLORES ──────────────────────────────────────────────────────
    /// Usa SystemColors para integrarse con el tema del sistema.
    /// El consumidor puede asignar ForeColor / BackColor para personalizar.
    /// </summary>
    public sealed class ActionButton : Control
    {
        // ── Layout ────────────────────────────────────────────────────
        private const int ArrowW = 20;
        private const int PadH = 8;
        private const int FixedH = 26;
        private const int IconSz = 13;
        private const int Radius = 4;
        private const int BtnGap = 6;

        // ── State ─────────────────────────────────────────────────────
        private bool _isSplit = false;
        private bool _hoverMain = false;
        private bool _hoverArrow = false;
        private bool _menuOpen = false;

        // ── Data ──────────────────────────────────────────────────────
        private string _text = "";
        private IconChar _icon = IconChar.None;

        private readonly ContextMenuStrip _menu;

        // ── Evento ────────────────────────────────────────────────────
        /// <summary>Clic en la parte principal del botón (no en el chevron).</summary>
        public event EventHandler PrimaryClicked;

        // ══════════════════════════════════════════════════════════════
        // PROPIEDADES
        // ══════════════════════════════════════════════════════════════

        public bool IsSplit
        {
            get => _isSplit;
            set { _isSplit = value; RecalcSize(); Invalidate(); }
        }

        public string ButtonText
        {
            get => _text;
            set { _text = value ?? ""; RecalcSize(); Invalidate(); }
        }

        public IconChar ButtonIcon
        {
            get => _icon;
            set { _icon = value; RecalcSize(); Invalidate(); }
        }

        public bool HasOptions => _menu.Items.Count > 0;

        /// <summary>
        /// Zona de flecha visible si IsSplit = true,
        /// independientemente de si hay opciones ya agregadas.
        /// </summary>
        private bool ShowArrow => _isSplit;

        // ══════════════════════════════════════════════════════════════
        // CONSTRUCTOR
        // ══════════════════════════════════════════════════════════════

        public ActionButton()
        {
            _menu = new ContextMenuStrip();
            _menu.Opening += (s, e) => { _menuOpen = true; Invalidate(); };
            _menu.Closed += (s, e) => { _menuOpen = false; Invalidate(); };

            Cursor = Cursors.Hand;

            // SupportsTransparentBackColor: los píxeles de las esquinas
            // redondeadas dejan ver al padre sin pintar nada encima.
            SetStyle(
                ControlStyles.SupportsTransparentBackColor |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.ResizeRedraw, true);

            BackColor = Color.Transparent;
            RecalcSize();
        }

        // ══════════════════════════════════════════════════════════════
        // API PÚBLICA
        // ══════════════════════════════════════════════════════════════

        /// <summary>Agrega una opción al menú desplegable.</summary>
        public void AddOption(string text, IconChar icon = IconChar.None,
                              EventHandler handler = null)
        {
            var item = new ToolStripMenuItem(text);
            if (icon != IconChar.None)
                try { item.Image = icon.ToBitmap(SystemColors.MenuText, 14); } catch { }
            if (handler != null)
                item.Click += handler;
            _menu.Items.Add(item);
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
            bool onArrow = ShowArrow && e.X >= Width - ArrowW;
            if (onArrow == _hoverArrow) return;
            _hoverArrow = onArrow;
            _hoverMain = !onArrow;
            Invalidate();
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            _hoverMain = true;
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

            if (ShowArrow && e.X >= Width - ArrowW)
            {
                // Zona del chevron: abrir dropdown si tiene opciones
                if (HasOptions)
                    _menu.Show(this, new Point(0, Height));
                else
                    PrimaryClicked?.Invoke(this, EventArgs.Empty); // fallback
            }
            else
            {
                PrimaryClicked?.Invoke(this, EventArgs.Empty);
            }
        }

        // ══════════════════════════════════════════════════════════════
        // PAINT
        // ══════════════════════════════════════════════════════════════

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // No pintar fondo: los píxeles fuera del rounded rect
            // muestran el padre a través de BackColor = Transparent.
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SetHighQuality();

            int arrowZone = ShowArrow ? ArrowW : 0;
            var bounds = new Rectangle(0, 0, Width - 1, Height - 1);

            // ── Fondo del botón ───────────────────────────────────────
            Color bg = (_hoverMain || _hoverArrow || _menuOpen)
                ? SystemColors.ControlLight
                : SystemColors.Control;

            using (var fill = new SolidBrush(bg))
                g.FillRoundedRect(fill, bounds, Radius);

            // ── Highlight extra en la zona del arrow ──────────────────
            if (_hoverArrow && arrowZone > 0)
            {
                var arrowRect = new Rectangle(Width - arrowZone - 1, 0, arrowZone, Height - 1);
                using (var path = BuildRightRoundedPath(arrowRect, Radius))
                using (var fill = new SolidBrush(SystemColors.ButtonHighlight))
                    g.FillPath(fill, path);
            }

            // ── Borde ─────────────────────────────────────────────────
            using (var pen = new Pen(SystemColors.ControlDark, 1f))
                g.DrawRoundedRect(pen, bounds, Radius);

            // ── Divisor vertical (solo split) ─────────────────────────
            if (arrowZone > 0)
            {
                int dx = Width - arrowZone - 1;
                using (var pen = new Pen(SystemColors.ControlDark, 1f))
                    g.DrawLine(pen, dx, 4, dx, Height - 5);
            }

            // ── Ícono ─────────────────────────────────────────────────
            int x = PadH;
            if (_icon != IconChar.None)
            {
                int iy = (Height - IconSz) / 2;
                try
                {
                    using (var bmp = _icon.ToBitmap(SystemColors.ControlText, IconSz))
                        g.DrawImage(bmp, x, iy, IconSz, IconSz);
                }
                catch { /* FontAwesome no disponible */ }
                x += IconSz + 5;
            }

            // ── Texto ─────────────────────────────────────────────────
            int mainW = Width - arrowZone - 1;
            int textW = mainW - x - 4;
            if (textW > 0 && !string.IsNullOrEmpty(_text))
            {
                using (var b = new SolidBrush(SystemColors.ControlText))
                using (var fmt = new StringFormat
                {
                    LineAlignment = StringAlignment.Center,
                    Alignment = StringAlignment.Near,
                    Trimming = StringTrimming.EllipsisCharacter
                })
                    g.DrawString(_text, SystemFonts.DefaultFont, b,
                        new RectangleF(x, 0, textW, Height), fmt);
            }

            // ── Chevron ───────────────────────────────────────────────
            if (arrowZone > 0)
            {
                int cx = Width - arrowZone / 2 - 1;
                int cy = Height / 2;
                int half = 3;
                Color cc = _hoverArrow
                    ? SystemColors.ControlText
                    : SystemColors.GrayText;

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

        private void RecalcSize()
        {
            int iconW = _icon != IconChar.None ? IconSz + 5 : 0;
            int textW = string.IsNullOrEmpty(_text) ? 0
                : TextRenderer.MeasureText(_text, SystemFonts.DefaultFont).Width;
            int contentW = Math.Max(iconW + textW, 8);
            int arrowW = _isSplit ? ArrowW + 1 : 0;
            Width = PadH + contentW + PadH + arrowW;
            Height = FixedH;
        }

        protected override void SetBoundsCore(
            int x, int y, int width, int height, BoundsSpecified specified)
        {
            // Altura fija; ancho calculado automáticamente
            base.SetBoundsCore(x, y, width, FixedH, specified);
        }

        // ── Helper: esquinas redondeadas solo en el lado derecho ──────

        private static GraphicsPath BuildRightRoundedPath(Rectangle rect, int r)
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