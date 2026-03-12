using FontAwesome.Sharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;


namespace Procont.Utils.Sidebar
{
    /// <summary>
    /// Grupo colapsable del sidebar con soporte recursivo de N niveles.
    /// - Key      : identificador lógico único.
    /// - GroupIcon: ícono FontAwesome.Sharp (IconChar.*).
    /// </summary>
    [ToolboxItem(false)]
    [DesignTimeVisible(false)]
    public class SidebarMenuGroupControl : Panel
    {
        // ── Controles internos ────────────────────────────────────────
        private readonly GroupHeader _header;
        private readonly Panel _childrenPanel;

        // ── Estado ────────────────────────────────────────────────────
        private bool _expanded = false;
        private readonly int _indentLevel;
        private readonly List<Control> _children = new List<Control>();

        // ── Eventos ───────────────────────────────────────────────────
        public event EventHandler<SidebarMenuItemControl> ItemSelected;
        internal event EventHandler LayoutChanged;

        // ── Propiedades ───────────────────────────────────────────────

        [Category("Sidebar")]
        [Description("Texto del encabezado del grupo.")]
        [DefaultValue("Grupo")]
        public string GroupTitle
        {
            get => _header.Title;
            set { _header.Title = value; _header.Invalidate(); }
        }

        [Category("Sidebar")]
        [Description("Clave única para identificar este grupo sin depender del texto.")]
        [DefaultValue("")]
        public string Key { get; set; } = "";

        [Category("Sidebar")]
        [Description("Ícono FontAwesome del grupo. Requiere NuGet FontAwesome.Sharp.")]
        [DefaultValue(IconChar.None)]
        public IconChar GroupIcon
        {
            get => _header.Icon;
            set { _header.Icon = value; _header.Invalidate(); }
        }

        [Category("Sidebar")]
        [Description("Indica si el grupo está expandido al iniciar.")]
        [DefaultValue(false)]
        public bool Expanded
        {
            get => _expanded;
            set { _expanded = value; UpdateLayout(); }
        }

        // ── Ocultar propiedades heredadas irrelevantes ─────────────────
        [Browsable(false)] public override Color BackColor { get => base.BackColor; set => base.BackColor = value; }
        [Browsable(false)] public new Padding Padding { get => base.Padding; set => base.Padding = value; }

        // ── Constructores ─────────────────────────────────────────────
        public SidebarMenuGroupControl() : this(0) { }

        public SidebarMenuGroupControl(int indentLevel)
        {
            _indentLevel = indentLevel;
            Dock = DockStyle.Top;
            BackColor = SidebarTheme.BackgroundDark;
            Padding = Padding.Empty;
            Margin = Padding.Empty;

            if (_indentLevel == 0)
            {
                Paint += (s, e) =>
                {
                    using (var pen = new Pen(SidebarTheme.BorderColor, 1))
                        e.Graphics.DrawLine(pen, 0, Height - 1, Width, Height - 1);
                };
            }

            _header = new GroupHeader(_indentLevel) { Dock = DockStyle.Top };
            _header.ToggleRequested += (s, e) => Toggle();

            _childrenPanel = new Panel
            {
                Dock = DockStyle.Top,
                BackColor = SidebarTheme.BackgroundDark,
                Height = 0,
                Visible = false
            };

            Controls.Add(_childrenPanel);
            Controls.Add(_header);
            UpdateLayout();
        }

        // ── API pública ────────────────────────────────────────────────

        /// <summary>
        /// Agrega un ítem hoja a este grupo.
        /// </summary>
        /// <param name="text">Texto visible.</param>
        /// <param name="key">Clave de identificación. Si se omite, se usa el texto.</param>
        /// <param name="icon">Ícono FontAwesome (opcional).</param>
        public SidebarMenuItemControl AddItem(string text, string key = "", IconChar icon = IconChar.None)
        {
            var resolvedKey = string.IsNullOrEmpty(key) ? text : key;
            var item = new SidebarMenuItemControl(text, resolvedKey, icon, _indentLevel + 1);
            item.Height = GetItemHeight();
            item.ItemSelected += (s, e) => ItemSelected?.Invoke(this, item);
            _children.Add(item);
            RebuildChildrenPanel();
            return item;
        }

        /// <summary>
        /// Agrega un sub-grupo ya construido (usado internamente por SidebarControl.RebuildFromModels).
        /// </summary>
        internal void AddSubGroupControl(SidebarMenuGroupControl sub)
        {
            sub.ItemSelected += (s, item) => ItemSelected?.Invoke(this, item);
            sub.LayoutChanged += (s, e) =>
            {
                UpdateLayout();
                // Burbujear hacia arriba para que SidebarControl recalcule el container
                LayoutChanged?.Invoke(this, EventArgs.Empty);
            };
            _children.Add(sub);
            RebuildChildrenPanel();
        }

        /// <summary>
        /// Agrega un sub-grupo anidado.
        /// </summary>
        /// <param name="title">Texto visible.</param>
        /// <param name="key">Clave de identificación. Si se omite, se usa el título.</param>
        /// <param name="icon">Ícono FontAwesome (opcional).</param>
        /// <param name="expanded">Expandido por defecto.</param>
        public SidebarMenuGroupControl AddSubGroup(string title, string key = "", IconChar icon = IconChar.None, bool expanded = false)
        {
            var sub = new SidebarMenuGroupControl(_indentLevel + 1)
            {
                GroupTitle = title,
                Key = string.IsNullOrEmpty(key) ? title : key,
                GroupIcon = icon,
                Expanded = expanded
            };
            sub.ItemSelected += (s, item) => ItemSelected?.Invoke(this, item);
            sub.LayoutChanged += (s, e) => UpdateLayout();
            _children.Add(sub);
            RebuildChildrenPanel();
            return sub;
        }

        // ══════════════════════════════════════════════════════════════
        // VISIBILIDAD POR KEY
        // ══════════════════════════════════════════════════════════════

        /// <summary>
        /// Altura que tendría el grupo si estuviera visible en su estado actual.
        /// Se usa para restaurar la altura al volver a mostrar.
        /// </summary>
        internal int GetNaturalHeight()
        {
            return _header.Height
                + (_expanded ? CalculateChildrenHeight() : 0)
                + (_indentLevel == 0 ? 1 : 0);
        }

        /// <summary>
        /// Busca un nodo por Key entre los hijos directos e indirectos.
        /// Devuelve true si lo encontró y aplicó la visibilidad.
        /// </summary>
        internal bool SetChildVisible(string key, bool visible, Action recalculate)
        {
            foreach (var child in _children)
            {
                if (child is SidebarMenuItemControl item && item.Key == key)
                {
                    item.Visible = visible;
                    item.Height = visible ? GetItemHeight() : 0;
                    UpdateLayout();
                    LayoutChanged?.Invoke(this, EventArgs.Empty);
                    recalculate?.Invoke();
                    return true;
                }

                if (child is SidebarMenuGroupControl subGrp)
                {
                    if (subGrp.Key == key)
                    {
                        subGrp.Visible = visible;
                        subGrp.Height = visible ? subGrp.GetNaturalHeight() : 0;
                        UpdateLayout();
                        LayoutChanged?.Invoke(this, EventArgs.Empty);
                        recalculate?.Invoke();
                        return true;
                    }

                    // Buscar más profundo
                    if (subGrp.SetChildVisible(key, visible, recalculate))
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Aplica visibilidad a todos los hijos recursivamente.
        /// </summary>
        internal void SetAllChildrenVisible(bool visible, Action recalculate)
        {
            foreach (var child in _children)
            {
                if (child is SidebarMenuItemControl item)
                {
                    item.Visible = visible;
                    item.Height = visible ? GetItemHeight() : 0;
                }
                else if (child is SidebarMenuGroupControl subGrp)
                {
                    subGrp.Visible = visible;
                    subGrp.Height = visible ? subGrp.GetNaturalHeight() : 0;
                    subGrp.SetAllChildrenVisible(visible, null);
                }
            }
            UpdateLayout();
            LayoutChanged?.Invoke(this, EventArgs.Empty);
            recalculate?.Invoke();
        }

        // ── Toggle ────────────────────────────────────────────────────
        public void Toggle()
        {
            _expanded = !_expanded;
            _header.IsExpanded = _expanded;
            UpdateLayout();
            LayoutChanged?.Invoke(this, EventArgs.Empty);
        }

        // ── Layout ────────────────────────────────────────────────────
        private void UpdateLayout()
        {
            _header.IsExpanded = _expanded;
            _header.Height = GetHeaderHeight();
            int ch = CalculateChildrenHeight();
            if (_expanded && _children.Count > 0)
            {
                _childrenPanel.Height = ch;
                _childrenPanel.Visible = true;
            }
            else
            {
                _childrenPanel.Height = 0;
                _childrenPanel.Visible = false;
            }
            Height = _header.Height + (_expanded ? ch : 0) + (_indentLevel == 0 ? 1 : 0);
        }

        private int GetHeaderHeight() => _indentLevel == 0 ? SidebarTheme.GroupHeight : SidebarTheme.GroupHeight - 6;
        private int GetItemHeight() => _indentLevel == 0 ? SidebarTheme.ItemHeight : SidebarTheme.ItemHeight - 4;

        private int CalculateChildrenHeight()
        {
            int total = 0;
            foreach (var child in _children)
                total += (child is SidebarMenuGroupControl sub) ? sub.Height : GetItemHeight();
            return total;
        }

        private void RebuildChildrenPanel()
        {
            _childrenPanel.Controls.Clear();
            for (int i = _children.Count - 1; i >= 0; i--)
                _childrenPanel.Controls.Add(_children[i]);
            UpdateLayout();
        }

        // ══════════════════════════════════════════════════════════════
        // Encabezado interno
        // ══════════════════════════════════════════════════════════════
        private class GroupHeader : Control
        {
            public string Title = "Grupo";
            public IconChar Icon = IconChar.None;
            public bool IsExpanded = false;
            private bool _hovered = false;
            private readonly int _level;

            public event EventHandler ToggleRequested;

            public GroupHeader(int level)
            {
                _level = level;
                Height = level == 0 ? SidebarTheme.GroupHeight : SidebarTheme.GroupHeight - 6;
                Cursor = Cursors.Hand;
                SetStyle(ControlStyles.OptimizedDoubleBuffer |
                         ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.UserPaint, true);
            }

            protected override void OnMouseEnter(EventArgs e) { base.OnMouseEnter(e); _hovered = true; Invalidate(); }
            protected override void OnMouseLeave(EventArgs e) { base.OnMouseLeave(e); _hovered = false; Invalidate(); }
            protected override void OnClick(EventArgs e) { base.OnClick(e); ToggleRequested?.Invoke(this, EventArgs.Empty); }

            protected override void OnPaint(PaintEventArgs e)
            {
                var g = e.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.Clear(IsExpanded ? SidebarTheme.BackgroundActive :
                        _hovered ? SidebarTheme.BackgroundHover :
                                     SidebarTheme.BackgroundDark);
                if (_level == 0) PaintLevel0(g);
                else PaintLevelN(g);
            }

            private void PaintLevel0(Graphics g)
            {
                // Ícono FontAwesome
                if (Icon != IconChar.None)
                {
                    int iconSize = 16;
                    int iconY = (Height - iconSize) / 2;
                    using (var bmp = Icon.ToBitmap(SidebarTheme.TextAccent, iconSize))
                        g.DrawImage(bmp, 12, iconY, iconSize, iconSize);
                }

                // Título dorado mayúscula
                using (var b = new SolidBrush(SidebarTheme.TextAccent))
                {
                    var fmt = new StringFormat
                    {
                        Alignment = StringAlignment.Near,
                        LineAlignment = StringAlignment.Center,
                        Trimming = StringTrimming.EllipsisCharacter
                    };
                    int tx = Icon != IconChar.None ? 36 : 14;
                    g.DrawString(Title.ToUpper(), SidebarTheme.FontGroupTitle, b,
                        new RectangleF(tx, 0, Width - tx - 26, Height), fmt);
                }

                DrawChevron(g, IsExpanded, SidebarTheme.TextSubdued, large: true);
            }

            private void PaintLevelN(Graphics g)
            {
                int baseX = 14 + (_level * 18);

                // Líneas de jerarquía
                using (var pen = new Pen(SidebarTheme.BorderColor, 1))
                {
                    int lineX = 14 + ((_level - 1) * 18) + 7;
                    g.DrawLine(pen, lineX, 0, lineX, Height);
                    g.DrawLine(pen, lineX, Height / 2, baseX, Height / 2);
                }

                // Ícono o flecha
                if (Icon != IconChar.None)
                {
                    int iconSize = 13;
                    int iconY = (Height - iconSize) / 2;
                    using (var bmp = Icon.ToBitmap(IsExpanded ? SidebarTheme.TextAccent : SidebarTheme.TextSubdued, iconSize))
                        g.DrawImage(bmp, baseX + 2, iconY, iconSize, iconSize);
                }
                else
                {
                    DrawSubGroupArrow(g, baseX, IsExpanded);
                }

                // Título
                Color tc = IsExpanded ? SidebarTheme.TextPrimary : Color.FromArgb(190, 200, 215);
                using (var b = new SolidBrush(tc))
                {
                    int tx = baseX + (Icon != IconChar.None ? 20 : 18);
                    var fmt = new StringFormat
                    {
                        Alignment = StringAlignment.Near,
                        LineAlignment = StringAlignment.Center,
                        Trimming = StringTrimming.EllipsisCharacter
                    };
                    g.DrawString(Title, SidebarTheme.FontMenuItem, b,
                        new RectangleF(tx, 0, Width - tx - 22, Height), fmt);
                }

                DrawChevron(g, IsExpanded, SidebarTheme.BorderColor, large: false);
            }

            private void DrawSubGroupArrow(Graphics g, int x, bool expanded)
            {
                int cy = Height / 2;
                using (var pen = new Pen(expanded ? SidebarTheme.TextAccent : SidebarTheme.TextSubdued, 1.5f))
                {
                    pen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
                    pen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
                    if (expanded) { g.DrawLine(pen, x + 1, cy - 2, x + 5, cy + 2); g.DrawLine(pen, x + 5, cy + 2, x + 9, cy - 2); }
                    else { g.DrawLine(pen, x + 3, cy - 4, x + 7, cy); g.DrawLine(pen, x + 7, cy, x + 3, cy + 4); }
                }
            }

            private void DrawChevron(Graphics g, bool down, Color color, bool large)
            {
                int cx = Width - 16, cy = Height / 2, half = large ? 4 : 3;
                using (var pen = new Pen(color, 1.5f))
                {
                    pen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
                    pen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
                    if (down) { g.DrawLine(pen, cx - half, cy - 1, cx, cy + 2); g.DrawLine(pen, cx, cy + 2, cx + half, cy - 1); }
                    else { g.DrawLine(pen, cx - half, cy + 1, cx, cy - 2); g.DrawLine(pen, cx, cy - 2, cx + half, cy + 1); }
                }
            }
        }
    }
}