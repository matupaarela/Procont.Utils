using FontAwesome.Sharp;
using Procont.Utils.Components.DataItem.Models;
using Procont.Utils.Components.Sidebar.Models;
using Procont.Utils.Core.Extensions;
using Procont.Utils.Core.Theming;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Procont.Utils.Components.DataItem
{
    /// <summary>
    /// Versatile item row — WinForms port of shadcn/ui's Item component.
    ///
    /// Layout (left → right):
    ///   [ Media (optional) ] [ Title / Description ] [ Badge ] [ ActionLabel ] [ ActionsPanel ]
    ///
    /// ── STANDALONE USE ───────────────────────────────────────────────
    ///   var item = new DataItemControl
    ///   {
    ///       Title        = "Security Alert",
    ///       Description  = "New login from unknown device.",
    ///       MediaVariant = DataItemMediaVariant.Icon,
    ///       Icon         = IconChar.ShieldAlt,
    ///   };
    ///   // Agregar un split button desde código:
    ///   item.AddActionButton("review", "Review", IconChar.Eye, isSplit: true);
    ///   item.GetAction("review").PrimaryClicked += (s, e) => OpenReview();
    ///   item.GetAction("review").AddOption("Dismiss", IconChar.Times, (s,e) => Dismiss());
    ///
    /// ── VÍA DataItemView + diseñador ─────────────────────────────────
    ///   Properties → Actions → [...] → "Add" → configurar Key/Label/Icon/IsSplit.
    ///   En Form.Load:
    ///   dataItemView1.GetItem("alert").GetAction("review").PrimaryClicked += ...
    ///
    /// ── COMPLEX ACTIONS (embedded controls) ──────────────────────────
    ///   item.ActionsPanel.Controls.Add(myButton);
    /// </summary>
    [ToolboxItem(true)]
    [Description("Versatile item row with media, title, description and actions (shadcn Item port).")]
    [DefaultEvent("ItemClicked")]
    [DefaultProperty("Title")]
    public class DataItemControl : Control
    {
        // ── Backing fields ─────────────────────────────────────────────
        private string _key = "";
        private string _title = "Item";
        private string _description = "";
        private IconChar _icon = IconChar.None;
        private DataItemMediaVariant _mediaVariant = DataItemMediaVariant.None;
        private DataItemVariant _variant = DataItemVariant.Default;
        private DataItemSize _size = DataItemSize.Default;
        private SidebarBadge _badge = SidebarBadge.None;
        private string _actionLabel = "";
        private string _avatarInitials = "";
        private Image _mediaImage = null;
        private bool _isActive = false;
        private bool _isHovered = false;
        private bool _actionHovered = false;
        private Rectangle _cachedActionBounds = Rectangle.Empty;

        // ── Actions panel ──────────────────────────────────────────────
        private readonly Panel _actionsPanel;

        // ── Diccionario de botones construidos por Key ─────────────────
        private readonly Dictionary<string, SplitActionButton> _actionButtons
            = new Dictionary<string, SplitActionButton>(StringComparer.OrdinalIgnoreCase);

        // ── Layout constants ───────────────────────────────────────────
        private const int PadH = 12;
        private const int MediaGap = 10;
        private const int ActionsGap = 8;

        // ══════════════════════════════════════════════════════════════
        // EVENTOS
        // ══════════════════════════════════════════════════════════════

        [Category("DataItem")]
        [Description("Fires when the user clicks anywhere on the item (except the action label).")]
        public event EventHandler ItemClicked;

        [Category("DataItem")]
        [Description("Fires when the user clicks the inline action label (legacy ActionLabel).")]
        public event EventHandler ActionClicked;

        // ══════════════════════════════════════════════════════════════
        // PROPIEDADES
        // ══════════════════════════════════════════════════════════════

        [Category("DataItem")]
        [DefaultValue("")]
        public string Key { get => _key; set => _key = value ?? ""; }

        [Category("DataItem")]
        [DefaultValue("Item")]
        public string Title
        { get => _title; set { _title = value ?? ""; Invalidate(); } }

        [Category("DataItem")]
        [DefaultValue("")]
        public string Description
        { get => _description; set { _description = value ?? ""; Invalidate(); } }

        [Category("DataItem")]
        [DefaultValue(IconChar.None)]
        public IconChar Icon
        { get => _icon; set { _icon = value; Invalidate(); } }

        [Category("DataItem")]
        [DefaultValue(DataItemMediaVariant.None)]
        public DataItemMediaVariant MediaVariant
        { get => _mediaVariant; set { _mediaVariant = value; Invalidate(); } }

        [Category("DataItem")]
        [DefaultValue(DataItemVariant.Default)]
        public DataItemVariant Variant
        { get => _variant; set { _variant = value; Invalidate(); } }

        [Category("DataItem")]
        [DefaultValue(DataItemSize.Default)]
        public DataItemSize Size
        { get => _size; set { _size = value; Height = GetNaturalHeight(); Invalidate(); } }

        [Category("DataItem")]
        [DefaultValue(SidebarBadge.None)]
        public SidebarBadge Badge
        { get => _badge; set { _badge = value; Invalidate(); } }

        [Category("DataItem")]
        [DefaultValue("")]
        public string ActionLabel
        { get => _actionLabel; set { _actionLabel = value ?? ""; Invalidate(); } }

        [Category("DataItem")]
        [DefaultValue("")]
        public string AvatarInitials
        { get => _avatarInitials; set { _avatarInitials = (value ?? "").ToUpper(); Invalidate(); } }

        [Category("DataItem")]
        [DefaultValue(null)]
        public Image MediaImage
        { get => _mediaImage; set { _mediaImage = value; Invalidate(); } }

        [Category("DataItem")]
        [DefaultValue(false)]
        public bool IsActive
        { get => _isActive; set { _isActive = value; Invalidate(); } }

        /// <summary>
        /// Panel docked al borde derecho para controles embebidos.
        /// Usar <see cref="AddActionButton"/> para agregar botones tipados,
        /// o acceder directamente para casos avanzados.
        /// </summary>
        [Browsable(false)]
        public Panel ActionsPanel => _actionsPanel;

        [Browsable(false)] public override string Text { get => base.Text; set => base.Text = value; }
        [Browsable(false)] public override Color BackColor { get => base.BackColor; set => base.BackColor = value; }
        [Browsable(false)] public override Color ForeColor { get => base.ForeColor; set => base.ForeColor = value; }

        // ══════════════════════════════════════════════════════════════
        // CONSTRUCTOR
        // ══════════════════════════════════════════════════════════════

        public DataItemControl()
        {
            _actionsPanel = new Panel
            {
                BackColor = Color.Transparent,
                Visible = false,
                AutoSize = false
            };
            _actionsPanel.ControlAdded += (s, e) => { _actionsPanel.Visible = true; LayoutActionsPanel(); };
            _actionsPanel.ControlRemoved += (s, e) => { _actionsPanel.Visible = _actionsPanel.Controls.Count > 0; LayoutActionsPanel(); };
            Controls.Add(_actionsPanel);

            Height = GetNaturalHeight();
            Dock = DockStyle.Top;
            Cursor = Cursors.Hand;

            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint, true);
        }

        // ══════════════════════════════════════════════════════════════
        // API DE ACCIONES — construcción y acceso
        // ══════════════════════════════════════════════════════════════

        /// <summary>
        /// Crea y registra un <see cref="SplitActionButton"/> en el área de acciones.
        /// Llamar en Form.Load (o desde <see cref="DataItemView.BuildActions"/>).
        /// </summary>
        /// <param name="key">Clave para recuperarlo con <see cref="GetAction"/>.</param>
        /// <param name="label">Texto visible. Vacío = solo ícono.</param>
        /// <param name="icon">Ícono del botón.</param>
        /// <param name="isSplit">true = [Label|▼], false = solo ícono.</param>
        public SplitActionButton AddActionButton(
            string key,
            string label = "",
            IconChar icon = IconChar.None,
            bool isSplit = false)
        {
            var btn = new SplitActionButton(label, icon);
            string resolvedKey = string.IsNullOrEmpty(key) ? label : key;

            if (_actionButtons.ContainsKey(resolvedKey))
                _actionButtons.Remove(resolvedKey);

            _actionButtons[resolvedKey] = btn;
            _actionsPanel.Controls.Add(btn);
            return btn;
        }

        /// <summary>
        /// Obtiene el <see cref="SplitActionButton"/> registrado con la clave indicada.
        /// Retorna null si la clave no existe.
        /// Usar para wiring de handlers después de RebuildFromModels():
        ///   ctrl.GetAction("edit").PrimaryClicked += (s,e) => Edit();
        /// </summary>
        public SplitActionButton GetAction(string key)
        {
            if (string.IsNullOrEmpty(key)) return null;
            _actionButtons.TryGetValue(key, out var btn);
            return btn;
        }

        /// <summary>
        /// Construye los botones de la colección <see cref="DataItemModel.Actions"/>.
        /// Llamado internamente por <see cref="DataItemView"/> tras RebuildFromModels().
        /// </summary>
        internal void BuildActionsFromModel(IList<DataItemActionModel> actions)
        {
            // Limpiar botones anteriores del modelo (no los que haya agregado el usuario a mano)
            foreach (var kv in _actionButtons)
                _actionsPanel.Controls.Remove(kv.Value);
            _actionButtons.Clear();

            if (actions == null) return;
            foreach (var a in actions)
            {
                if (a == null) continue;
                AddActionButton(
                    string.IsNullOrEmpty(a.Key) ? a.Label : a.Key,
                    a.IsSplit ? a.Label : "",  // ícono puro si no es split
                    a.Icon,
                    a.IsSplit);
            }
        }

        // ══════════════════════════════════════════════════════════════
        // SIZING HELPERS
        // ══════════════════════════════════════════════════════════════

        internal int GetNaturalHeight()
        {
            switch (_size)
            {
                case DataItemSize.Small: return 48;
                case DataItemSize.XSmall: return 36;
                default: return 64;
            }
        }

        private int GetMediaBoxSize()
        {
            switch (_size)
            {
                case DataItemSize.Small: return 30;
                case DataItemSize.XSmall: return 24;
                default: return 38;
            }
        }

        private int GetIconDrawSize()
        {
            switch (_size)
            {
                case DataItemSize.Small: return 15;
                case DataItemSize.XSmall: return 12;
                default: return 18;
            }
        }

        // ══════════════════════════════════════════════════════════════
        // ACTIONS PANEL LAYOUT
        // ══════════════════════════════════════════════════════════════

        private void LayoutActionsPanel()
        {
            if (!_actionsPanel.Visible) return;

            const int gap = 6;
            int x = 0;
            int maxH = 0;
            foreach (Control c in _actionsPanel.Controls)
            {
                c.Location = new Point(x, 0);
                x += c.Width + gap;
                if (c.Height > maxH) maxH = c.Height;
            }
            int totalW = x > gap ? x - gap : 0;
            _actionsPanel.Size = new Size(totalW, maxH > 0 ? maxH : Height);
            PositionActionsPanel();
            Invalidate();
        }

        private void PositionActionsPanel()
        {
            if (!_actionsPanel.Visible) return;
            int x = Width - PadH - _actionsPanel.Width;
            int y = (Height - _actionsPanel.Height) / 2;
            _actionsPanel.Location = new Point(x, y);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            PositionActionsPanel();
        }

        // ══════════════════════════════════════════════════════════════
        // MOUSE
        // ══════════════════════════════════════════════════════════════

        protected override void OnMouseEnter(EventArgs e)
        { base.OnMouseEnter(e); _isHovered = true; Invalidate(); }

        protected override void OnMouseLeave(EventArgs e)
        { base.OnMouseLeave(e); _isHovered = false; _actionHovered = false; Invalidate(); }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (string.IsNullOrEmpty(_actionLabel)) return;
            bool over = !_cachedActionBounds.IsEmpty && _cachedActionBounds.Contains(e.Location);
            if (over != _actionHovered)
            {
                _actionHovered = over;
                Cursor = over ? Cursors.Hand : Cursors.Default;
                Invalidate();
            }
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            var me = e as MouseEventArgs;
            if (me != null && !string.IsNullOrEmpty(_actionLabel)
                && !_cachedActionBounds.IsEmpty
                && _cachedActionBounds.Contains(me.Location))
            {
                ActionClicked?.Invoke(this, EventArgs.Empty);
                return;
            }
            ItemClicked?.Invoke(this, EventArgs.Empty);
        }

        // ══════════════════════════════════════════════════════════════
        // PAINT
        // ══════════════════════════════════════════════════════════════

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SetHighQuality();

            Color bg = _isActive ? ProcontTheme.SurfaceActive
                     : _isHovered ? ProcontTheme.SurfaceHover
                     : _variant == DataItemVariant.Muted ? ProcontTheme.SurfaceHover
                     : ProcontTheme.SurfaceDark;

            if (_variant == DataItemVariant.Outline)
            {
                g.Clear(ProcontTheme.SurfaceDark);
                var borderRect = new Rectangle(1, 1, Width - 2, Height - 2);
                if (_isActive || _isHovered || _variant == DataItemVariant.Muted)
                    using (var fill = new SolidBrush(bg))
                        g.FillRoundedRect(fill, borderRect, ProcontTheme.RadiusSmall);
                using (var pen = new Pen(ProcontTheme.BorderDefault, 1f))
                    g.DrawRoundedRect(pen, borderRect, ProcontTheme.RadiusSmall);
            }
            else
            {
                g.Clear(bg);
            }

            if (_isActive)
                using (var b = new SolidBrush(ProcontTheme.TextAccent))
                    g.FillRectangle(b, 0, 6, 3, Height - 12);

            int x = PadH + (_isActive ? 3 : 0);

            if (_mediaVariant != DataItemMediaVariant.None)
            {
                int boxSize = GetMediaBoxSize();
                int boxY = (Height - boxSize) / 2;
                DrawMedia(g, x, boxY, boxSize);
                x += boxSize + MediaGap;
            }

            int rightBound = Width - PadH;
            if (_actionsPanel.Visible)
                rightBound = _actionsPanel.Left - ActionsGap;

            // ── ActionLabel (legacy) ──────────────────────────────────
            _cachedActionBounds = Rectangle.Empty;
            if (!string.IsNullOrEmpty(_actionLabel))
            {
                var labelSize = g.MeasureString(_actionLabel, ProcontTheme.FontSmall);
                int lw = (int)labelSize.Width + 4;
                int lh = (int)labelSize.Height;
                int lx = rightBound - lw;
                int ly = (Height - lh) / 2;

                Color lc = _actionHovered ? ProcontTheme.TextPrimary : ProcontTheme.TextAccent;
                using (var b = new SolidBrush(lc))
                    g.DrawString(_actionLabel, ProcontTheme.FontSmall, b, new PointF(lx, ly));

                if (_actionHovered)
                    using (var pen = new Pen(lc, 1f))
                        g.DrawLine(pen, lx, ly + lh - 1, lx + lw, ly + lh - 1);

                _cachedActionBounds = new Rectangle(lx, ly, lw, lh);
                rightBound = lx - ActionsGap;
            }

            // ── Badge ─────────────────────────────────────────────────
            if (_badge != SidebarBadge.None)
            {
                int bw = g.MeasureBadgeWidth(_badge);
                g.DrawBadge(_badge, rightBound, Height / 2);
                rightBound -= bw + ActionsGap;
            }

            // ── Content ───────────────────────────────────────────────
            int contentW = Math.Max(0, rightBound - x - 4);
            bool hasDesc = !string.IsNullOrEmpty(_description);

            if (hasDesc)
            {
                int titleH = 15, descH = 12, gap2 = 2;
                int total = titleH + gap2 + descH;
                int vy = (Height - total) / 2;

                using (var b = new SolidBrush(ProcontTheme.TextPrimary))
                {
                    var fmt = new StringFormat { Trimming = StringTrimming.EllipsisCharacter, FormatFlags = StringFormatFlags.NoWrap };
                    g.DrawString(_title, ProcontTheme.FontBold, b,
                        new RectangleF(x, vy, contentW, titleH), fmt);
                }
                using (var b = new SolidBrush(ProcontTheme.TextSubdued))
                {
                    var fmt = new StringFormat { Trimming = StringTrimming.EllipsisCharacter, FormatFlags = StringFormatFlags.NoWrap };
                    g.DrawString(_description, ProcontTheme.FontSmall, b,
                        new RectangleF(x, vy + titleH + gap2, contentW, descH), fmt);
                }
            }
            else
            {
                using (var b = new SolidBrush(ProcontTheme.TextPrimary))
                {
                    var fmt = new StringFormat
                    {
                        LineAlignment = StringAlignment.Center,
                        Trimming = StringTrimming.EllipsisCharacter,
                        FormatFlags = StringFormatFlags.NoWrap
                    };
                    g.DrawString(_title, ProcontTheme.FontBold, b,
                        new RectangleF(x, 0, contentW, Height), fmt);
                }
            }
        }

        // ── Media rendering ───────────────────────────────────────────
        private void DrawMedia(Graphics g, int x, int y, int boxSize)
        {
            var boxRect = new Rectangle(x, y, boxSize, boxSize);

            switch (_mediaVariant)
            {
                case DataItemMediaVariant.Icon:
                    {
                        var tint = Color.FromArgb(45, ProcontTheme.TextAccent.R,
                                                  ProcontTheme.TextAccent.G, ProcontTheme.TextAccent.B);
                        using (var fill = new SolidBrush(tint))
                            g.FillRoundedRect(fill, boxRect, ProcontTheme.RadiusSmall);

                        if (_icon != IconChar.None)
                        {
                            int is_ = GetIconDrawSize();
                            using (var bmp = _icon.ToBitmap(ProcontTheme.TextAccent, is_))
                                g.DrawImage(bmp, x + (boxSize - is_) / 2, y + (boxSize - is_) / 2, is_, is_);
                        }
                        break;
                    }
                case DataItemMediaVariant.Avatar:
                    {
                        var tint = Color.FromArgb(55, ProcontTheme.TextAccent.R,
                                                  ProcontTheme.TextAccent.G, ProcontTheme.TextAccent.B);
                        using (var path = GraphicsExtensions.BuildRoundedPath(boxRect, boxSize / 2))
                        using (var fill = new SolidBrush(tint))
                            g.FillPath(fill, path);

                        if (!string.IsNullOrEmpty(_avatarInitials))
                        {
                            var f = _size == DataItemSize.Default ? ProcontTheme.FontBold : ProcontTheme.FontSmallBold;
                            string ini = _avatarInitials.Length > 2 ? _avatarInitials.Substring(0, 2) : _avatarInitials;
                            using (var b = new SolidBrush(ProcontTheme.TextAccent))
                            using (var fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                                g.DrawString(ini, f, b, boxRect, fmt);
                        }
                        break;
                    }
                case DataItemMediaVariant.Image:
                    {
                        if (_mediaImage != null)
                        {
                            using (var path = GraphicsExtensions.BuildRoundedPath(boxRect, ProcontTheme.RadiusSmall))
                            {
                                g.SetClip(path);
                                g.DrawImage(_mediaImage, boxRect);
                                g.ResetClip();
                            }
                        }
                        else
                        {
                            using (var fill = new SolidBrush(ProcontTheme.BorderDefault))
                                g.FillRoundedRect(fill, boxRect, ProcontTheme.RadiusSmall);
                        }
                        break;
                    }
            }
        }
    }
}