using FontAwesome.Sharp;
using Procont.Utils.Sidebar.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;

namespace Procont.Utils.Sidebar
{
    /// <summary>
    /// Sidebar de navegación estilo XAML para WinForms .NET 4.7.
    ///
    /// ── USO DESDE EL DISEÑADOR ────────────────────────────────────────
    ///   1. Arrastra SidebarControl al Form.
    ///   2. Properties → Sidebar → Groups → [...].
    ///   3. "Add ▼" ofrece dos tipos:
    ///        • SidebarRootItemModel  → ítem raíz directo (sin hijos)
    ///        • SidebarGroupModel     → grupo colapsable con hijos
    ///   4. VS serializa todo en InitializeComponent() automáticamente.
    ///
    /// ── USO DESDE CÓDIGO ──────────────────────────────────────────────
    ///   AddRootItem(text, key, icon)  →  ítem raíz por código (Form.Load).
    ///   AddGroup(title, key, icon)    →  grupo colapsable por código.
    /// </summary>
    [ToolboxItem(true)]
    [Description("Sidebar de navegación con grupos colapsables y soporte de N niveles.")]
    [DefaultProperty("Groups")]
    [DefaultEvent("ItemSelected")]
    public class SidebarControl : UserControl, ISupportInitialize
    {
        // ── Controles internos ────────────────────────────────────────
        private readonly SidebarHeaderControl _header;
        private readonly DoubleBufferedPanel _scrollPanel;
        private readonly DoubleBufferedPanel _menuContainer;

        // ── Modelo unificado (diseñador) ──────────────────────────────
        private readonly List<SidebarNodeModel> _nodeModels = new List<SidebarNodeModel>();

        // ── Controles generados en tiempo de ejecución ────────────────
        private readonly List<Control> _nodeControls = new List<Control>();
        private readonly List<SidebarMenuGroupControl> _groupControls = new List<SidebarMenuGroupControl>();
        private DashboardItemControl _dashControl = null;
        private RootItemControl _activeRootItem = null;
        private SidebarMenuItemControl _activeItem = null;
        private bool _initializing = false;

        // ── Backing fields ─────────────────────────────────────────────
        private bool _showHeader = true;
        private bool _showDashboard = true;

        // ══════════════════════════════════════════════════════════════
        // EVENTO PÚBLICO
        // ══════════════════════════════════════════════════════════════

        [Category("Sidebar")]
        [Description("Se dispara cuando el usuario hace clic en un ítem del menú.")]
        public event EventHandler<SidebarMenuItemControl> ItemSelected;

        // ══════════════════════════════════════════════════════════════
        // PROPIEDADES DEL ÍTEM SELECCIONADO
        // ══════════════════════════════════════════════════════════════

        [Browsable(false)]
        public string SelectedBreadcrumb => _activeItem?.BreadcrumbPath ?? string.Empty;

        [Browsable(false)]
        public IconChar SelectedIcon => _activeItem?.ResolvedIcon ?? IconChar.None;

        // ══════════════════════════════════════════════════════════════
        // PROPIEDADES DEL DISEÑADOR
        // ══════════════════════════════════════════════════════════════

        [Category("Sidebar")]
        [Description("Elementos raíz del menú. Add ▼ permite elegir entre ítem directo o grupo colapsable.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Editor(typeof(SidebarRootCollectionEditor), typeof(UITypeEditor))]
        public List<SidebarNodeModel> Groups => _nodeModels;

        // ── Visibilidad ───────────────────────────────────────────────

        [Category("Sidebar — Visibilidad")]
        [Description("Muestra u oculta el encabezado con logo y nombre de empresa.")]
        [DefaultValue(true)]
        public bool ShowHeader
        {
            get => _showHeader;
            set
            {
                _showHeader = value;
                _header.Visible = value;
                _header.Height = value ? SidebarTheme.HeaderHeight : 0;
            }
        }

        [Category("Sidebar — Visibilidad")]
        [Description("Muestra u oculta el ítem Dashboard.")]
        [DefaultValue(true)]
        public bool ShowDashboard
        {
            get => _showDashboard;
            set
            {
                _showDashboard = value;
                if (_dashControl == null) return;
                _dashControl.Visible = value;
                _dashControl.Height = value ? SidebarTheme.GroupHeight : 0;
                UpdateContainerHeight();
            }
        }

        // ── Dashboard configurable ────────────────────────────────────

        [Category("Sidebar — Dashboard")]
        [Description("Texto del ítem Dashboard.")]
        [DefaultValue("DASHBOARD")]
        public string DashboardTitle
        {
            get => _dashControl?.Title ?? "DASHBOARD";
            set { if (_dashControl != null) { _dashControl.Title = value; _dashControl.Invalidate(); } }
        }

        [Category("Sidebar — Dashboard")]
        [Description("Ícono del ítem Dashboard (IconChar.*).")]
        [DefaultValue(IconChar.ChartLine)]
        public IconChar DashboardIcon
        {
            get => _dashControl?.Icon ?? IconChar.ChartLine;
            set { if (_dashControl != null) { _dashControl.Icon = value; _dashControl.Invalidate(); } }
        }

        [Category("Sidebar — Dashboard")]
        [Description("Key del ítem Dashboard, útil para SelectItem(key).")]
        [DefaultValue("__dashboard__")]
        public string DashboardKey
        {
            get => _dashControl?.Key ?? "__dashboard__";
            set { if (_dashControl != null) _dashControl.Key = value; }
        }

        // ── Empresa ───────────────────────────────────────────────────

        [Category("Sidebar — Empresa")]
        [Description("Nombre corto de la empresa.")]
        [DefaultValue("EMPRESA S.A.")]
        public string CompanyName { get => _header.CompanyName; set { _header.CompanyName = value; } }

        [Category("Sidebar — Empresa")]
        [Description("Razón social completa.")]
        [DefaultValue("Razón social completa")]
        public string CompanySubtitle { get => _header.CompanySubtitle; set { _header.CompanySubtitle = value; } }

        [Category("Sidebar — Empresa")]
        [Description("Número de RUC.")]
        [DefaultValue("00000000000")]
        public string Ruc { get => _header.Ruc; set { _header.Ruc = value; } }

        [Category("Sidebar — Empresa")]
        [Description("Módulo o área.")]
        [DefaultValue("Módulo")]
        public string Module { get => _header.Module; set { _header.Module = value; } }

        [Category("Sidebar — Empresa")]
        [Description("Logo de la empresa.")]
        [DefaultValue(null)]
        public Image CompanyLogo { get => _header.Logo; set { _header.Logo = value; } }

        // ── Ocultar heredadas ─────────────────────────────────────────
        [Browsable(false)] public override Color BackColor { get => base.BackColor; set => base.BackColor = value; }
        [Browsable(false)] public override Color ForeColor { get => base.ForeColor; set => base.ForeColor = value; }
        [Browsable(false)] public override Font Font { get => base.Font; set => base.Font = value; }
        [Browsable(false)] public override string Text { get => base.Text; set => base.Text = value; }

        // ══════════════════════════════════════════════════════════════
        // CONSTRUCTOR
        // ══════════════════════════════════════════════════════════════
        public SidebarControl()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint, true);

            base.BackColor = SidebarTheme.BackgroundDark;
            base.Width = SidebarTheme.SidebarWidth;
            Dock = DockStyle.Left;
            Padding = Padding.Empty;

            _header = new SidebarHeaderControl();

            _scrollPanel = new DoubleBufferedPanel
            {
                Dock = DockStyle.Fill,
                BackColor = SidebarTheme.BackgroundDark,
                AutoScroll = true
            };
            _scrollPanel.HorizontalScroll.Maximum = 0;
            _scrollPanel.HorizontalScroll.Visible = false;
            _scrollPanel.AutoScrollMinSize = new Size(0, 0);

            _menuContainer = new DoubleBufferedPanel
            {
                Dock = DockStyle.Top,
                BackColor = SidebarTheme.BackgroundDark,
                Height = 0
            };

            _scrollPanel.Controls.Add(_menuContainer);
            AddDashboardItem();

            Controls.Add(_scrollPanel);
            Controls.Add(_header);
        }

        // ══════════════════════════════════════════════════════════════
        // ISupportInitialize
        // ══════════════════════════════════════════════════════════════
        public void BeginInit() => _initializing = true;

        public void EndInit()
        {
            _initializing = false;
            RebuildFromModels();
        }

        // ══════════════════════════════════════════════════════════════
        // RECONSTRUCCIÓN DESDE MODELOS
        // ══════════════════════════════════════════════════════════════

        public void RebuildFromModels()
        {
            if (_initializing) return;

            foreach (var ctrl in _nodeControls)
                _menuContainer.Controls.Remove(ctrl);
            _nodeControls.Clear();
            _groupControls.Clear();

            foreach (var node in _nodeModels)
            {
                if (node == null) continue;

                if (node is SidebarRootItemModel rootModel)
                {
                    var ctrl = BuildRootItemControl(rootModel);
                    _nodeControls.Add(ctrl);
                }
                else if (node is SidebarGroupModel groupModel)
                {
                    var ctrl = BuildGroupControl(groupModel, indentLevel: 0);
                    if (ctrl == null) continue;
                    _nodeControls.Add(ctrl);
                    _groupControls.Add(ctrl);
                }
                else if (node is SidebarSeparatorModel setModel)
                {
                    var ctrl = new SidebarMenuSeparatorControl(setModel.Label);
                    _nodeControls.Add(ctrl);
                }
            }

            RebuildMenuContainer();
        }

        // ── Construir control de ítem raíz ────────────────────────────
        private RootItemControl BuildRootItemControl(SidebarRootItemModel model)
        {
            var ctrl = new RootItemControl
            {
                Title = model.ItemText,
                Key = string.IsNullOrEmpty(model.Key) ? model.ItemText : model.Key,
                Icon = model.Icon,
                Badge = model.Badge          // ← propagar badge
            };
            ctrl.RootItemSelected += OnRootItemSelected;
            return ctrl;
        }

        // ── Construir grupo desde modelo ──────────────────────────────
        private SidebarMenuGroupControl BuildGroupControl(
            SidebarGroupModel model,
            int indentLevel,
            List<string> parentTitles = null,
            List<IconChar> parentIcons = null)
        {
            if (model == null) return null;

            parentTitles = parentTitles ?? new List<string>();
            parentIcons = parentIcons ?? new List<IconChar>();

            var ctrl = new SidebarMenuGroupControl(indentLevel)
            {
                GroupTitle = model.GroupTitle,
                Key = string.IsNullOrEmpty(model.Key) ? model.GroupTitle : model.Key,
                GroupIcon = model.Icon,
                Expanded = model.Expanded,
                Badge = model.Badge     // ← propagar badge al grupo
            };

            var myTitles = new List<string>(parentTitles) { model.GroupTitle };
            var myIcons = new List<IconChar>(parentIcons) { model.Icon };

            foreach (var node in model.Children)
            {
                if (node == null) continue;

                if (node is SidebarItemModel itemModel)
                {
                    var key = string.IsNullOrEmpty(itemModel.Key) ? itemModel.ItemText : itemModel.Key;
                    var item = ctrl.AddItem(itemModel.ItemText, key, itemModel.Icon, itemModel.Badge); // ← badge

                    var parts = new List<string>(myTitles) { itemModel.ItemText };
                    item.BreadcrumbPath = string.Join(" · ", parts);
                    item.ResolvedIcon = ResolveIcon(itemModel.Icon, myIcons);
                }
                else if (node is SidebarGroupModel subModel)
                {
                    var subCtrl = BuildGroupControl(subModel, indentLevel + 1, myTitles, myIcons);
                    if (subCtrl != null)
                        ctrl.AddSubGroupControl(subCtrl);
                }
                else if (node is SidebarSeparatorModel sepModel)
                {
                    ctrl.AddSeparator(sepModel.Label);
                }
            }

            ctrl.ItemSelected += OnGroupItemSelected;
            ctrl.LayoutChanged += (s, e) => UpdateContainerHeight();
            ctrl.GroupExpanded += OnGroupExpanded;
            return ctrl;
        }

        private static IconChar ResolveIcon(IconChar itemIcon, List<IconChar> ancestorIcons)
        {
            if (itemIcon != IconChar.None) return itemIcon;
            for (int i = ancestorIcons.Count - 1; i >= 0; i--)
                if (ancestorIcons[i] != IconChar.None) return ancestorIcons[i];
            return IconChar.None;
        }

        // ══════════════════════════════════════════════════════════════
        // SELECCIÓN DE ÍTEM
        // ══════════════════════════════════════════════════════════════

        private void ClearActiveRootItem()
        {
            if (_activeRootItem == null) return;
            _activeRootItem.IsActive = false;
            _activeRootItem = null;
        }

        private void OnRootItemSelected(object sender, EventArgs e)
        {
            var ctrl = sender as RootItemControl;
            if (ctrl == null) return;

            if (_activeItem != null) { _activeItem.IsActive = false; _activeItem = null; }
            ClearActiveRootItem();

            foreach (var grp in _groupControls)
            {
                grp.DeactivateAllItems();
                grp.CollapseAll();
            }
            UpdateContainerHeight();

            ctrl.IsActive = true;
            _activeRootItem = ctrl;

            var synthetic = new SidebarMenuItemControl(ctrl.Title, ctrl.Key, ctrl.Icon, level: 0)
            {
                BreadcrumbPath = ctrl.Title,
                ResolvedIcon = ctrl.Icon,
                IsActive = true
            };
            _activeItem = synthetic;
            ItemSelected?.Invoke(this, synthetic);
        }

        private void OnGroupItemSelected(object sender, SidebarMenuItemControl item)
        {
            if (_activeItem != null && _activeItem != item)
                _activeItem.IsActive = false;

            ClearActiveRootItem();
            _activeItem = item;
            ItemSelected?.Invoke(this, item);
        }

        private void OnGroupExpanded(object sender, EventArgs e)
        {
            var openedGroup = sender as SidebarMenuGroupControl;
            foreach (var grp in _groupControls)
            {
                if (grp == openedGroup) continue;
                grp.DeactivateAllItems();
                grp.CollapseAll();
            }
            UpdateContainerHeight();
        }

        public bool SelectItem(string key)
        {
            if (_activeItem != null) { _activeItem.IsActive = false; _activeItem = null; }
            ClearActiveRootItem();

            foreach (var ctrl in _nodeControls)
            {
                if (ctrl is RootItemControl root && root.Key == key)
                {
                    OnRootItemSelected(root, EventArgs.Empty);
                    return true;
                }
            }

            SidebarMenuItemControl found = null;
            SidebarMenuGroupControl ownerGroup = null;

            foreach (var grp in _groupControls)
            {
                if (grp.TrySelectItem(key, out found))
                {
                    ownerGroup = grp;
                    break;
                }
            }

            if (found == null) return false;

            _activeItem = found;

            foreach (var grp in _groupControls)
            {
                if (grp == ownerGroup) continue;
                grp.DeactivateAllItems();
                grp.CollapseAll();
            }

            UpdateContainerHeight();
            ItemSelected?.Invoke(this, found);
            return true;
        }

        // ══════════════════════════════════════════════════════════════
        // API PÚBLICA — agregar por código
        // ══════════════════════════════════════════════════════════════

        public SidebarMenuGroupControl AddGroup(
            string title,
            string key = "",
            IconChar icon = IconChar.None,
            bool expanded = false,
            SidebarBadge badge = SidebarBadge.None)
        {
            var group = new SidebarMenuGroupControl(0)
            {
                GroupTitle = title,
                Key = string.IsNullOrEmpty(key) ? title : key,
                GroupIcon = icon,
                Expanded = expanded,
                Badge = badge
            };
            group.ItemSelected += OnGroupItemSelected;
            group.LayoutChanged += (s, e) => UpdateContainerHeight();
            group.GroupExpanded += OnGroupExpanded;
            _groupControls.Add(group);
            _nodeControls.Add(group);
            RebuildMenuContainer();
            return group;
        }

        public RootItemControl AddRootItem(
            string text,
            string key = "",
            IconChar icon = IconChar.None,
            SidebarBadge badge = SidebarBadge.None)
        {
            var ctrl = new RootItemControl
            {
                Title = text,
                Key = string.IsNullOrEmpty(key) ? text : key,
                Icon = icon,
                Badge = badge
            };
            ctrl.RootItemSelected += OnRootItemSelected;
            _nodeControls.Add(ctrl);
            RebuildMenuContainer();
            return ctrl;
        }

        // ══════════════════════════════════════════════════════════════
        // VISIBILIDAD POR ROL
        // ══════════════════════════════════════════════════════════════

        public void SetVisible(string key, bool visible)
        {
            foreach (var grp in _groupControls)
                if (SetVisibleInGroup(grp, key, visible)) return;
        }

        public void SetVisible(bool visible, params string[] keys)
        {
            foreach (var k in keys) SetVisible(k, visible);
        }

        public void ApplyRol(params string[] allowedKeys)
        {
            foreach (var grp in _groupControls) SetGroupVisibility(grp, false);
            foreach (var key in allowedKeys) SetVisible(key, true);
        }

        private bool SetVisibleInGroup(SidebarMenuGroupControl grp, string key, bool visible)
        {
            if (grp.Key == key)
            {
                grp.Visible = visible;
                grp.Height = visible ? grp.GetNaturalHeight() : 0;
                UpdateContainerHeight();
                return true;
            }
            return grp.SetChildVisible(key, visible, () => UpdateContainerHeight());
        }

        private void SetGroupVisibility(SidebarMenuGroupControl grp, bool visible)
        {
            grp.Visible = visible;
            grp.Height = visible ? grp.GetNaturalHeight() : 0;
            grp.SetAllChildrenVisible(visible, () => UpdateContainerHeight());
        }

        public void SetCompanyInfo(string name, string subtitle, string ruc, string module, Image logo = null)
        {
            CompanyName = name;
            CompanySubtitle = subtitle;
            Ruc = ruc;
            Module = module;
            if (logo != null) CompanyLogo = logo;
        }

        // ══════════════════════════════════════════════════════════════
        // LAYOUT INTERNO
        // ══════════════════════════════════════════════════════════════

        private void AddDashboardItem()
        {
            _dashControl = new DashboardItemControl();
            _dashControl.RootItemSelected += (s, e) =>
            {
                if (_activeItem != null) { _activeItem.IsActive = false; _activeItem = null; }
                ClearActiveRootItem();
                ItemSelected?.Invoke(this, null);
            };
            _menuContainer.Controls.Add(_dashControl);
            UpdateContainerHeight();
        }

        private void RebuildMenuContainer()
        {
            _menuContainer.Controls.Clear();

            for (int i = _nodeControls.Count - 1; i >= 0; i--)
                _menuContainer.Controls.Add(_nodeControls[i]);

            if (_dashControl != null)
                _menuContainer.Controls.Add(_dashControl);

            UpdateContainerHeight();
        }

        private void UpdateContainerHeight()
        {
            if (_menuContainer == null) return;

            int total = _showDashboard ? SidebarTheme.GroupHeight : 0;
            foreach (var ctrl in _nodeControls)
                if (ctrl.Visible) total += ctrl.Height;

            _menuContainer.Height = total + 10;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            UpdateContainerHeight();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (_menuContainer == null) return;
            _menuContainer.Width = Width;
        }

        // ══════════════════════════════════════════════════════════════
        // Panel con double-buffer
        // ══════════════════════════════════════════════════════════════
        private sealed class DoubleBufferedPanel : Panel
        {
            public DoubleBufferedPanel()
            {
                SetStyle(
                    ControlStyles.OptimizedDoubleBuffer |
                    ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.UserPaint, true);
                DoubleBuffered = true;
            }

            protected override CreateParams CreateParams
            {
                get
                {
                    CreateParams cp = base.CreateParams;
                    cp.ExStyle |= 0x02000000; // WS_EX_COMPOSITED
                    return cp;
                }
            }
        }

        // ══════════════════════════════════════════════════════════════
        // RootItemControl — ítem raíz nivel 0, sin hijos.
        // ══════════════════════════════════════════════════════════════
        public class RootItemControl : Control
        {
            private bool _hovered = false;
            private bool _active = false;

            public string Title { get; set; } = "Ítem";
            public IconChar Icon { get; set; } = IconChar.None;
            public string Key { get; set; } = "";
            public SidebarBadge Badge { get; set; } = SidebarBadge.None;

            public bool IsActive
            {
                get => _active;
                set { _active = value; Invalidate(); }
            }

            internal event EventHandler RootItemSelected;

            public RootItemControl()
            {
                Height = SidebarTheme.GroupHeight;
                Dock = DockStyle.Top;
                Cursor = Cursors.Hand;
                SetStyle(ControlStyles.OptimizedDoubleBuffer |
                         ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.UserPaint, true);
            }

            protected override void OnMouseEnter(EventArgs e) { base.OnMouseEnter(e); _hovered = true; Invalidate(); }
            protected override void OnMouseLeave(EventArgs e) { base.OnMouseLeave(e); _hovered = false; Invalidate(); }
            protected override void OnClick(EventArgs e)
            {
                base.OnClick(e);
                RootItemSelected?.Invoke(this, EventArgs.Empty);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                var g = e.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                g.Clear(_active ? SidebarTheme.BackgroundActive :
                        _hovered ? SidebarTheme.BackgroundHover :
                                   SidebarTheme.BackgroundDark);

                if (_active)
                {
                    using (var b = new SolidBrush(SidebarTheme.TextAccent))
                        g.FillRectangle(b, 0, 5, 3, Height - 10);
                }

                using (var pen = new Pen(SidebarTheme.BorderColor, 1))
                    g.DrawLine(pen, 0, Height - 1, Width, Height - 1);

                int textX = 14;
                if (Icon != IconChar.None)
                {
                    int iconSize = 16, iconY = (Height - iconSize) / 2;
                    using (var bmp = Icon.ToBitmap(SidebarTheme.TextAccent, iconSize))
                        g.DrawImage(bmp, 12, iconY, iconSize, iconSize);
                    textX = 36;
                }

                // Reservar espacio para badge
                int badgeWidth = SidebarTheme.GetBadgeWidth(g, Badge);
                int badgeReserve = badgeWidth > 0 ? badgeWidth + 8 : 8;

                using (var b = new SolidBrush(SidebarTheme.TextAccent))
                {
                    var fmt = new StringFormat
                    {
                        Alignment = StringAlignment.Near,
                        LineAlignment = StringAlignment.Center,
                        Trimming = StringTrimming.EllipsisCharacter
                    };
                    g.DrawString(Title.ToUpper(), SidebarTheme.FontGroupTitle, b,
                        new RectangleF(textX, 0, Width - textX - badgeReserve, Height), fmt);
                }

                // Badge pill
                if (Badge != SidebarBadge.None)
                    SidebarTheme.DrawBadge(g, Badge, Width - 8, Height / 2);
            }
        }

        // ── Dashboard hereda RootItemControl con valores fijos ────────
        private class DashboardItemControl : RootItemControl
        {
            public DashboardItemControl()
            {
                Title = "DASHBOARD";
                Icon = IconChar.ChartLine;
                Key = "__dashboard__";
            }
        }
    }

}