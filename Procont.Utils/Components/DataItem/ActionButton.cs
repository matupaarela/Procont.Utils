using FontAwesome.Sharp;
using Procont.Utils.Core.Extensions;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Procont.Utils.Components.DataItem
{
    /// <summary>
    /// Botón de acción: simple o split [Label | ▼].
    ///
    /// ── SIMPLE ───────────────────────────────────────────────────────
    ///   var btn = new ActionButton { ButtonText = "Copiar" };
    ///   btn.PrimaryClicked += (s, e) => Copiar();
    ///
    /// ── SPLIT ────────────────────────────────────────────────────────
    ///   var btn = new ActionButton { ButtonText = "Publicar", IsSplit = true };
    ///   btn.PrimaryClicked += (s, e) => Publicar();
    ///   btn.AddOption("Descargar",          IconChar.Download,   (s, e) => Descargar());
    ///   btn.AddOption("Añadir al proyecto", IconChar.FolderPlus, (s, e) => Aniadir());
    ///   btn.AddSeparator();
    ///   btn.AddOption("Eliminar",           IconChar.Trash,      (s, e) => Eliminar());
    ///
    /// Usa SystemColors; hereda automáticamente el tema del sistema.
    /// </summary>
    public sealed class ActionButton : Control
    {
        // ── Medidas fijas ─────────────────────────────────────────────
        private const int FixedH = 26;
        private const int PadL = 10;
        private const int PadR = 10;
        private const int ArrowW = 22;
        private const int IconSz = 13;
        private const int Radius = 4;

        // ── Estado ────────────────────────────────────────────────────
        private bool _isSplit = false;
        private bool _hoverMain = false;
        private bool _hoverArrow = false;
        private bool _menuOpen = false;

        // ── Datos ─────────────────────────────────────────────────────
        private string _text = "";
        private IconChar _icon = IconChar.None;
        private readonly ContextMenuStrip _menu;

        public event EventHandler PrimaryClicked;

        // ══════════════════════════════════════════════════════════════
        // PROPIEDADES
        // ══════════════════════════════════════════════════════════════

        public bool IsSplit
        {
            get => _isSplit;
            set { _isSplit = value; RecalcWidth(); Invalidate(); }
        }

        public string ButtonText
        {
            get => _text;
            set { _text = value ?? ""; RecalcWidth(); Invalidate(); }
        }

        public IconChar ButtonIcon
        {
            get => _icon;
            set { _icon = value; RecalcWidth(); Invalidate(); }
        }

        public bool HasOptions => _menu.Items.Count > 0;

        // ══════════════════════════════════════════════════════════════
        // CONSTRUCTOR
        // ══════════════════════════════════════════════════════════════

        public ActionButton()
        {
            _menu = new ContextMenuStrip();
            _menu.Opening += (s, e) => { _menuOpen = true; Invalidate(); };
            _menu.Closed += (s, e) => { _menuOpen = false; Invalidate(); };

            Cursor = Cursors.Hand;
            //BackColor = Color.Transparent;

            SetStyle(
                ControlStyles.SupportsTransparentBackColor |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.ResizeRedraw, true);

            RecalcWidth();
        }

        // ══════════════════════════════════════════════════════════════
        // API PÚBLICA
        // ══════════════════════════════════════════════════════════════

        public void AddOption(string text, IconChar icon = IconChar.None, EventHandler handler = null)
        {
            var item = new ToolStripMenuItem(text);
            if (icon != IconChar.None)
                try { item.Image = icon.ToBitmap(SystemColors.MenuText, 14); } catch { }
            if (handler != null) item.Click += handler;
            _menu.Items.Add(item);
        }

        public void AddSeparator() => _menu.Items.Add(new ToolStripSeparator());

        // ══════════════════════════════════════════════════════════════
        // MOUSE
        // ══════════════════════════════════════════════════════════════

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            bool onArrow = _isSplit && e.X >= Width - ArrowW;
            if (onArrow == _hoverArrow) return;
            _hoverArrow = onArrow;
            _hoverMain = !onArrow;
            Invalidate();
        }

        protected override void OnMouseEnter(EventArgs e)
        { base.OnMouseEnter(e); _hoverMain = true; Invalidate(); }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _hoverMain = false; _hoverArrow = false;
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button != MouseButtons.Left) return;

            if (_isSplit && e.X >= Width - ArrowW)
            {
                if (HasOptions) _menu.Show(this, new Point(0, Height));
                else PrimaryClicked?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                PrimaryClicked?.Invoke(this, EventArgs.Empty);
            }
        }

        // ══════════════════════════════════════════════════════════════
        // PAINT
        // ══════════════════════════════════════════════════════════════

        protected override void OnPaintBackground(PaintEventArgs e) { }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SetHighQuality();

            var bounds = new Rectangle(0, 0, Width - 1, Height - 1);

            if (_isSplit)
            {
                int split = Width - ArrowW - 1;

                // Zona izquierda (label)
                var leftRect = new Rectangle(0, 0, split, Height - 1);
                var rightRect = new Rectangle(split, 0, ArrowW, Height - 1);

                Color bgL = _hoverMain ? SystemColors.ControlLight : SystemColors.Control;
                Color bgR = (_hoverArrow || _menuOpen)
                            ? SystemColors.ControlLight : SystemColors.Control;

                using (var path = LeftRounded(leftRect, Radius))
                using (var fill = new SolidBrush(bgL))
                    g.FillPath(fill, path);

                using (var path = RightRounded(rightRect, Radius))
                using (var fill = new SolidBrush(bgR))
                    g.FillPath(fill, path);

                // Un solo borde que rodea los dos
                using (var pen = new Pen(SystemColors.ControlDark, 1f))
                    g.DrawRoundedRect(pen, bounds, Radius);

                // Divisor vertical
                using (var pen = new Pen(SystemColors.ControlDark, 1f))
                    g.DrawLine(pen, split, 4, split, Height - 5);

                // Chevron
                PaintChevron(g, Width - ArrowW / 2 - 1, Height / 2, 3);
            }
            else
            {
                Color bg = _hoverMain ? SystemColors.ControlLight : SystemColors.Control;
                using (var fill = new SolidBrush(bg))
                    g.FillRoundedRect(fill, bounds, Radius);
                using (var pen = new Pen(SystemColors.ControlDark, 1f))
                    g.DrawRoundedRect(pen, bounds, Radius);
            }

            // Ícono
            int x = PadL;
            if (_icon != IconChar.None)
            {
                try
                {
                    using (var bmp = _icon.ToBitmap(SystemColors.ControlText, IconSz))
                        g.DrawImage(bmp, x, (Height - IconSz) / 2, IconSz, IconSz);
                }
                catch { }
                x += IconSz + 5;
            }

            // Texto
            int maxW = (_isSplit ? Width - ArrowW - 1 : Width) - x - PadR;
            if (maxW > 0 && !string.IsNullOrEmpty(_text))
            {
                using (var b = new SolidBrush(SystemColors.ControlText))
                using (var fmt = new StringFormat
                {
                    LineAlignment = StringAlignment.Center,
                    Alignment = StringAlignment.Near,
                    Trimming = StringTrimming.EllipsisCharacter
                })
                    g.DrawString(_text, SystemFonts.DefaultFont, b,
                        new RectangleF(x, 0, maxW, Height), fmt);
            }
        }

        private void PaintChevron(Graphics g, int cx, int cy, int half)
        {
            Color c = (_hoverArrow || _menuOpen) ? SystemColors.ControlText : SystemColors.GrayText;
            using (var pen = new Pen(c, 1.5f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
            {
                if (_menuOpen)
                {
                    g.DrawLine(pen, cx - half, cy + 2, cx, cy - 2);
                    g.DrawLine(pen, cx, cy - 2, cx + half, cy + 2);
                }
                else
                {
                    g.DrawLine(pen, cx - half, cy - 2, cx, cy + 2);
                    g.DrawLine(pen, cx, cy + 2, cx + half, cy - 2);
                }
            }
        }

        // ══════════════════════════════════════════════════════════════
        // SIZING
        // ══════════════════════════════════════════════════════════════

        private void RecalcWidth()
        {
            int iconW = _icon != IconChar.None ? IconSz + 5 : 0;
            int textW = string.IsNullOrEmpty(_text) ? 0
                : TextRenderer.MeasureText(_text, SystemFonts.DefaultFont).Width;
            int label = PadL + iconW + textW + PadR;
            Width = _isSplit ? label + ArrowW : Math.Max(label, 28);
            Height = FixedH;
        }

        protected override void SetBoundsCore(int x, int y, int w, int h, BoundsSpecified s)
            => base.SetBoundsCore(x, y, w, FixedH, s);

        // ══════════════════════════════════════════════════════════════
        // PATHS REDONDEADOS PARCIALES
        // ══════════════════════════════════════════════════════════════

        // Esquinas redondeadas solo en el lado izquierdo
        private static GraphicsPath LeftRounded(Rectangle r, int rad)
        {
            int d = rad * 2;
            var p = new GraphicsPath();
            p.AddArc(r.X, r.Y, d, d, 180, 90);                  // sup-izq
            p.AddLine(r.X + rad, r.Y, r.Right, r.Y);             // borde superior
            p.AddLine(r.Right, r.Y, r.Right, r.Bottom);          // borde derecho (recto)
            p.AddLine(r.Right, r.Bottom, r.X + rad, r.Bottom);   // borde inferior
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);           // inf-izq
            p.CloseFigure();
            return p;
        }

        // Esquinas redondeadas solo en el lado derecho
        private static GraphicsPath RightRounded(Rectangle r, int rad)
        {
            int d = rad * 2;
            var p = new GraphicsPath();
            p.AddLine(r.X, r.Y, r.Right - rad, r.Y);             // borde superior
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);           // sup-der
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);    // inf-der
            p.AddLine(r.Right - rad, r.Bottom, r.X, r.Bottom);   // borde inferior
            p.CloseFigure();
            return p;
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