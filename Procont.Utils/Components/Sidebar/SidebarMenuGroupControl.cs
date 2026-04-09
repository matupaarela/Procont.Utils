using FontAwesome.Sharp;
using Procont.Utils.Components.Sidebar.Models;
using Procont.Utils.Sidebar;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Procont.Utils.Components.Sidebar
{
    [ToolboxItem(false)]
    [DesignTimeVisible(false)]
    public class SidebarMenuGroupControl : Panel
    {
        private readonly GroupHeader _header;
        private readonly Panel _childrenPanel;

        private bool _expanded = false;
        private SidebarBadge _badge = SidebarBadge.None;
        private readonly int _indentLevel;
        private readonly List<Control> _children = new List<Control>();

        public event EventHandler<SidebarMenuItemControl> ItemSelected;
        internal event EventHandler LayoutChanged;
        internal event EventHandler GroupExpanded;

        // ← NUEVO: notifica al padre cuando un ítem hijo entra/sale de hover
        internal event Action<bool> ChildHoverChanged;

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
        [Description("Ícono FontAwesome del grupo.")]
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

        [Category("Sidebar")]
        [Description("Insignia informativa: None, New (verde) o Beta (ámbar).")]
        [DefaultValue(SidebarBadge.None)]
        public SidebarBadge Badge
        {
            get => _badge;
            set { _badge = value; _header.Badge = value; _header.Invalidate(); }
        }

        [Browsable(false)] public override Color BackColor { get => base.BackColor; set => base.BackColor = value; }
        [Browsable(false)] public new Padding Padding { get => base.Padding; set => base.Padding = value; }

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

        public SidebarMenuSeparatorControl AddSeparator(string label = "")
        {
            var sep = new SidebarMenuSeparatorControl(label);
            _children.Add(sep);
            RebuildChildrenPanel();
            return sep;
        }

        public SidebarMenuItemControl AddItem(
            string text,
            string key = "",
            IconChar icon = IconChar.None,
            SidebarBadge badge = SidebarBadge.None)
        {
            var resolvedKey = string.IsNullOrEmpty(key) ? text : key;
            var item = new SidebarMenuItemControl(text, resolvedKey, icon, _indentLevel + 1)
            {
                Badge = badge
            };
            item.Height = GetItemHeight();
            item.ItemSelected += (s, e) => ItemSelected?.Invoke(this, item);

            // ← Propagar hover del ítem al header de este grupo
            item.MouseEnter += (s, e) =>
            {
                _header.HasHoveredChild = true;
                _header.Invalidate();
                ChildHoverChanged?.Invoke(true);
            };
            item.MouseLeave += (s, e) =>
            {
                _header.HasHoveredChild = false;
                _header.Invalidate();
                ChildHoverChanged?.Invoke(false);
            };

            _children.Add(item);
            RebuildChildrenPanel();
            return item;
        }

        internal void AddSubGroupControl(SidebarMenuGroupControl sub)
        {
            sub.ItemSelected += (s, item) => ItemSelected?.Invoke(this, item);
            sub.LayoutChanged += (s, e) =>
            {
                UpdateLayout();
                LayoutChanged?.Invoke(this, EventArgs.Empty);
            };

            // ← Propagar hover de ítems del sub-grupo al header de este grupo
            sub.ChildHoverChanged += hovered =>
            {
                _header.HasHoveredChild = hovered;
                _header.Invalidate();
            };

            _children.Add(sub);
            RebuildChildrenPanel();
        }

        public SidebarMenuGroupControl AddSubGroup(
            string title,
            string key = "",
            IconChar icon = IconChar.None,
            bool expanded = false,
            SidebarBadge badge = SidebarBadge.None)
        {
            var sub = new SidebarMenuGroupControl(_indentLevel + 1)
            {
                GroupTitle = title,
                Key = string.IsNullOrEmpty(key) ? title : key,
                GroupIcon = icon,
                Expanded = expanded,
                Badge = badge
            };
            sub.ItemSelected += (s, item) => ItemSelected?.Invoke(this, item);
            sub.LayoutChanged += (s, e) => UpdateLayout();

            // ← Propagar hover
            sub.ChildHoverChanged += hovered =>
            {
                _header.HasHoveredChild = hovered;
                _header.Invalidate();
            };

            _children.Add(sub);
            RebuildChildrenPanel();
            return sub;
        }

        internal int GetNaturalHeight()
        {
            return _header.Height
                + (_expanded ? CalculateChildrenHeight() : 0)
                + (_indentLevel == 0 ? 1 : 0);
        }

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

                    if (subGrp.SetChildVisible(key, visible, recalculate))
                        return true;
                }
            }
            return false;
        }

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

        internal bool TrySelectItem(string key, out SidebarMenuItemControl found)
        {
            foreach (var child in _children)
            {
                if (child is SidebarMenuItemControl item && item.Key == key)
                {
                    item.IsActive = true;
                    found = item;
                    if (!_expanded) { _expanded = true; UpdateLayout(); }
                    return true;
                }

                if (child is SidebarMenuGroupControl sub && sub.TrySelectItem(key, out found))
                {
                    if (!_expanded) { _expanded = true; UpdateLayout(); }
                    return true;
                }
            }

            found = null;
            return false;
        }

        internal void DeactivateAllItems()
        {
            foreach (var child in _children)
            {
                if (child is SidebarMenuItemControl item) item.IsActive = false;
                else if (child is SidebarMenuGroupControl sub) sub.DeactivateAllItems();
            }
        }

        internal void CollapseAll()
        {
            _expanded = false;
            foreach (var child in _children)
                if (child is SidebarMenuGroupControl sub)
                    sub.CollapseAll();
            UpdateLayout();
        }

        public void Toggle()
        {
            _expanded = !_expanded;
            _header.IsExpanded = _expanded;
            UpdateLayout();
            LayoutChanged?.Invoke(this, EventArgs.Empty);
            if (_expanded && _indentLevel == 0)
                GroupExpanded?.Invoke(this, EventArgs.Empty);
        }

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
                total += child.Height;
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
            public SidebarBadge Badge = SidebarBadge.None;

            // ← NUEVO: true cuando algún ítem hijo está en hover
            public bool HasHoveredChild = false;

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

                // ← Sin estado "active" en headers; solo hover (directo o desde hijo)
                g.Clear(_hovered || HasHoveredChild
                    ? SidebarTheme.BackgroundHover
                    : SidebarTheme.BackgroundDark);

                if (_level == 0) PaintLevel0(g);
                else PaintLevelN(g);
            }

            private void PaintLevel0(Graphics g)
            {
                if (Icon != IconChar.None)
                {
                    int iconSize = 16, iconY = (Height - iconSize) / 2;
                    using (var bmp = Icon.ToBitmap(SidebarTheme.TextAccent, iconSize))
                        g.DrawImage(bmp, 12, iconY, iconSize, iconSize);
                }

                int badgeWidth = SidebarTheme.GetBadgeWidth(g, Badge);
                int rightReserve = 22 + (badgeWidth > 0 ? badgeWidth + 6 : 0);

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
                        new RectangleF(tx, 0, Width - tx - rightReserve, Height), fmt);
                }

                if (Badge != SidebarBadge.None)
                    SidebarTheme.DrawBadge(g, Badge, Width - 22, Height / 2);

                DrawChevron(g, IsExpanded, SidebarTheme.TextSubdued, large: true);
            }

            private void PaintLevelN(Graphics g)
            {
                int baseX = 14 + (_level * 18);

                using (var pen = new Pen(SidebarTheme.BorderColor, 1))
                {
                    int lineX = 14 + ((_level - 1) * 18) + 7;
                    g.DrawLine(pen, lineX, 0, lineX, Height);
                    g.DrawLine(pen, lineX, Height / 2, baseX, Height / 2);
                }

                if (Icon != IconChar.None)
                {
                    int iconSize = 13, iconY = (Height - iconSize) / 2;
                    // ← Color del ícono: accent si expandido, subdued si no
                    using (var bmp = Icon.ToBitmap(
                        IsExpanded ? SidebarTheme.TextAccent : SidebarTheme.TextSubdued, iconSize))
                        g.DrawImage(bmp, baseX - 2, iconY, iconSize, iconSize);
                }

                int badgeWidth = SidebarTheme.GetBadgeWidth(g, Badge);
                int rightReserve = 22 + (badgeWidth > 0 ? badgeWidth + 6 : 0);

                // ← Texto: accent si expandido (sin cambiar fondo), subdued si no
                Color tc = IsExpanded ? SidebarTheme.TextAccent : Color.FromArgb(190, 200, 215);
                using (var b = new SolidBrush(tc))
                {
                    int tx = baseX + (Icon != IconChar.None ? 16 : 0);
                    var fmt = new StringFormat
                    {
                        Alignment = StringAlignment.Near,
                        LineAlignment = StringAlignment.Center,
                        Trimming = StringTrimming.EllipsisCharacter
                    };
                    g.DrawString(Title, SidebarTheme.FontMenuItem, b,
                        new RectangleF(tx, 0, Width - tx - rightReserve, Height), fmt);
                }

                if (Badge != SidebarBadge.None)
                    SidebarTheme.DrawBadge(g, Badge, Width - 22, Height / 2);

                DrawChevron(g, IsExpanded, SidebarTheme.TextSubdued, large: false);
            }

            private void DrawChevron(Graphics g, bool expanded, Color color, bool large)
            {
                int cx = Width - 14;
                int cy = Height / 2;
                int half = large ? 4 : 3;

                using (var pen = new Pen(color, 1.5f))
                {
                    pen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
                    pen.EndCap = System.Drawing.Drawing2D.LineCap.Round;

                    if (expanded)
                    {
                        g.DrawLine(pen, cx - half, cy - 1, cx, cy + half - 1);
                        g.DrawLine(pen, cx, cy + half - 1, cx + half, cy - 1);
                    }
                    else
                    {
                        g.DrawLine(pen, cx - 1, cy + half, cx + half - 1, cy);
                        g.DrawLine(pen, cx + half - 1, cy, cx - 1, cy - half);
                    }
                }
            }
        }
    }
}