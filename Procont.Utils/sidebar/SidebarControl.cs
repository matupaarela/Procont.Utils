using FontAwesome.Sharp;
using Procont.Utils.Sidebar.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Design;
using System.Linq;
using System.Windows.Forms;

namespace Procont.Utils.Sidebar
{
    /// <summary>
    /// Sidebar de navegación estilo XAML para WinForms .NET 4.7.
    ///
    /// ── USO DESDE EL DISEÑADOR ────────────────────────────────────────
    ///   1. Arrastra SidebarControl al Form.
    ///   2. En Properties → Sidebar → Groups → haz clic en [...]
    ///   3. Agrega SidebarGroupModel, configura GroupTitle / GroupIcon / Key.
    ///   4. Dentro de cada grupo, edita Items y SubGroups de la misma forma.
    ///   5. VS serializa todo en InitializeComponent() automáticamente.
    ///
    /// ── USO DESDE CÓDIGO ──────────────────────────────────────────────
    ///   También puedes llamar AddGroup() / AddItem() en Form.Load.
    ///   Ambos métodos conviven: primero se renderizan los Groups del diseñador,
    ///   luego los que agregues por código.
    /// </summary>
    [ToolboxItem(true)]
    [Description("Sidebar de navegación con grupos colapsables y soporte de N niveles.")]
    [DefaultProperty("Groups")]
    [DefaultEvent("ItemSelected")]
    public class SidebarControl : UserControl, ISupportInitialize
    {
        // ── Controles internos ────────────────────────────────────────
        private readonly SidebarHeaderControl _header;
        private readonly Panel _scrollPanel;
        private readonly Panel _menuContainer;

        // ── Modelo (diseñador) + grupos por código ────────────────────
        private readonly List<SidebarGroupModel> _groupModels = new List<SidebarGroupModel>();
        private readonly List<SidebarMenuGroupControl> _groupControls = new List<SidebarMenuGroupControl>();
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
        // PROPIEDADES DEL DISEÑADOR
        // ══════════════════════════════════════════════════════════════

        /// <summary>
        /// Colección de grupos del sidebar.
        /// Editable desde Properties → Sidebar → Groups → [...]
        /// </summary>
        [Category("Sidebar")]
        [Description("Grupos del menú. Haz clic en [...] para agregar/editar desde el diseñador.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Editor(typeof(CollectionEditor), typeof(UITypeEditor))]
        public List<SidebarGroupModel> Groups => _groupModels;

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
                if (_menuContainer == null || _menuContainer.Controls.Count == 0) return;
                var dash = _menuContainer.Controls[_menuContainer.Controls.Count - 1];
                dash.Visible = value;
                dash.Height = value ? SidebarTheme.GroupHeight : 0;
                UpdateContainerHeight();
            }
        }

        // ── Empresa (reenviadas al header) ────────────────────────────

        [Category("Sidebar — Empresa")]
        [Description("Nombre corto de la empresa.")]
        [DefaultValue("EMPRESA S.A.")]
        public string CompanyName
        {
            get => _header.CompanyName;
            set { _header.CompanyName = value; }
        }

        [Category("Sidebar — Empresa")]
        [Description("Razón social completa.")]
        [DefaultValue("Razón social completa")]
        public string CompanySubtitle
        {
            get => _header.CompanySubtitle;
            set { _header.CompanySubtitle = value; }
        }

        [Category("Sidebar — Empresa")]
        [Description("Número de RUC.")]
        [DefaultValue("00000000000")]
        public string Ruc
        {
            get => _header.Ruc;
            set { _header.Ruc = value; }
        }

        [Category("Sidebar — Empresa")]
        [Description("Módulo o área (Ventas, Compras, etc.).")]
        [DefaultValue("Módulo")]
        public string Module
        {
            get => _header.Module;
            set { _header.Module = value; }
        }

        [Category("Sidebar — Empresa")]
        [Description("Logo de la empresa.")]
        [DefaultValue(null)]
        public Image CompanyLogo
        {
            get => _header.Logo;
            set { _header.Logo = value; }
        }

        // ── Ocultar propiedades heredadas irrelevantes ─────────────────
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

            _scrollPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = SidebarTheme.BackgroundDark,
                AutoScroll = true
            };
            _scrollPanel.HorizontalScroll.Maximum = 0;
            _scrollPanel.HorizontalScroll.Visible = false;
            _scrollPanel.AutoScrollMinSize = new Size(0, 0);

            _menuContainer = new Panel
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
        // ISupportInitialize — el diseñador llama Begin/EndInit
        // al deserializar InitializeComponent()
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

        /// <summary>
        /// Reconstruye todos los controles visuales a partir de la colección Groups.
        /// Se llama automáticamente al terminar la deserialización del diseñador,
        /// y también puedes llamarla manualmente si modificas Groups por código.
        /// </summary>
        public void RebuildFromModels()
        {
            if (_initializing) return;

            // Limpiar controles anteriores generados desde modelos
            foreach (var ctrl in _groupControls)
            {
                ctrl.ItemSelected -= OnGroupItemSelected;
                _menuContainer.Controls.Remove(ctrl);
            }
            _groupControls.Clear();

            // Reconstruir desde _groupModels
            foreach (var model in _groupModels)
            {
                if (model == null) continue;
                var ctrl = BuildGroupControl(model, indentLevel: 0);
                if (ctrl == null) continue;
                _groupControls.Add(ctrl);
            }

            RebuildMenuContainer();
        }

        // Crea un SidebarMenuGroupControl desde un SidebarGroupModel (recursivo)
        private SidebarMenuGroupControl BuildGroupControl(SidebarGroupModel model, int indentLevel)
        {
            var ctrl = new SidebarMenuGroupControl(indentLevel)
            {
                GroupTitle = model.GroupTitle,
                Key = string.IsNullOrEmpty(model.Key) ? model.GroupTitle : model.Key,
                GroupIcon = model.Icon,
                Expanded = model.Expanded
            };

            // Iterar Children en orden de visualización.
            // Cada nodo puede ser SidebarItemModel o SidebarGroupModel — el orden se respeta.
            foreach (var node in model.Children)
            {
                if (node is SidebarItemModel itemModel)
                {
                    var key = string.IsNullOrEmpty(itemModel.Key) ? itemModel.ItemText : itemModel.Key;
                    ctrl.AddItem(itemModel.ItemText, key, itemModel.Icon);
                }
                else if (node is SidebarGroupModel subModel)
                {
                    var subCtrl = BuildGroupControl(subModel, indentLevel + 1);
                    if (subCtrl != null)
                        ctrl.AddSubGroupControl(subCtrl);
                }
            }

            ctrl.ItemSelected += OnGroupItemSelected;
            ctrl.LayoutChanged += (s, e) => UpdateContainerHeight();
            return ctrl;
        }

        private void OnGroupItemSelected(object sender, SidebarMenuItemControl item)
        {
            if (_activeItem != null && _activeItem != item)
                _activeItem.IsActive = false;
            _activeItem = item;
            ItemSelected?.Invoke(this, item);
        }

        // ══════════════════════════════════════════════════════════════
        // API PÚBLICA — agregar por código (complementa al diseñador)
        // ══════════════════════════════════════════════════════════════

        /// <summary>Agrega un grupo por código en tiempo de ejecución.</summary>
        public SidebarMenuGroupControl AddGroup(
            string title,
            string key = "",
            IconChar icon = IconChar.None,
            bool expanded = false)
        {
            var group = new SidebarMenuGroupControl(0)
            {
                GroupTitle = title,
                Key = string.IsNullOrEmpty(key) ? title : key,
                GroupIcon = icon,
                Expanded = expanded
            };
            group.ItemSelected += OnGroupItemSelected;
            group.LayoutChanged += (s, e) => UpdateContainerHeight();
            _groupControls.Add(group);
            RebuildMenuContainer();
            return group;
        }

        // ══════════════════════════════════════════════════════════════
        // VISIBILIDAD POR ROL
        // ══════════════════════════════════════════════════════════════

        /// <summary>
        /// Muestra u oculta un ítem o grupo por su Key.
        /// Útil para controlar el acceso según el rol del usuario.
        /// </summary>
        /// <param name="key">Key del ítem o grupo a afectar.</param>
        /// <param name="visible">true = visible, false = oculto.</param>
        public void SetVisible(string key, bool visible)
        {
            // Buscar en grupos raíz
            foreach (var grp in _groupControls)
                if (SetVisibleInGroup(grp, key, visible)) return;
        }

        /// <summary>
        /// Aplica visibilidad a varios keys de una vez.
        /// </summary>
        public void SetVisible(bool visible, params string[] keys)
        {
            foreach (var key in keys)
                SetVisible(key, visible);
        }

        /// <summary>
        /// Oculta TODOS los ítems y grupos, luego muestra solo los keys indicados.
        /// Ideal para aplicar un perfil de rol completo de una sola vez.
        /// </summary>
        public void ApplyRol(params string[] allowedKeys)
        {
            // Ocultar todo
            foreach (var grp in _groupControls)
                SetGroupVisibility(grp, false);

            // Mostrar solo los permitidos
            foreach (var key in allowedKeys)
                SetVisible(key, true);
        }

        // ── Helpers de visibilidad ────────────────────────────────────

        /// <summary>
        /// Busca recursivamente por Key y aplica la visibilidad.
        /// Devuelve true si encontró el nodo.
        /// </summary>
        private bool SetVisibleInGroup(SidebarMenuGroupControl grp, string key, bool visible)
        {
            // ¿Es el grupo mismo?
            if (grp.Key == key)
            {
                grp.Visible = visible;
                grp.Height = visible ? grp.GetNaturalHeight() : 0;
                UpdateContainerHeight();
                return true;
            }

            // Buscar en los hijos del grupo
            return grp.SetChildVisible(key, visible, () => UpdateContainerHeight());
        }

        private void SetGroupVisibility(SidebarMenuGroupControl grp, bool visible)
        {
            grp.Visible = visible;
            grp.Height = visible ? grp.GetNaturalHeight() : 0;
            grp.SetAllChildrenVisible(visible, () => UpdateContainerHeight());
        }

        /// <summary>Configura la cabecera por código.</summary>
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
            var dash = new DashboardItemControl();
            dash.Click += (s, e) => ItemSelected?.Invoke(this, null);
            _menuContainer.Controls.Add(dash);
            UpdateContainerHeight();
        }

        private void RebuildMenuContainer()
        {
            // Preservar el DashboardItem (siempre el último agregado, primero visualmente)
            Control dashItem = null;
            if (_menuContainer.Controls.Count > 0)
                dashItem = _menuContainer.Controls[_menuContainer.Controls.Count - 1];

            _menuContainer.Controls.Clear();

            // Apilar en orden inverso (Dock.Top invierte)
            for (int i = _groupControls.Count - 1; i >= 0; i--)
                _menuContainer.Controls.Add(_groupControls[i]);

            if (dashItem != null)
                _menuContainer.Controls.Add(dashItem);

            UpdateContainerHeight();
        }

        private void UpdateContainerHeight()
        {
            if (_menuContainer == null) return;
            int total = SidebarTheme.GroupHeight; // dashboard
            foreach (var g in _groupControls)
                total += GetGroupTotalHeight(g);
            _menuContainer.Height = total + 10;
        }

        /// <summary>
        /// Calcula la altura real de un grupo incluyendo todos sus hijos expandidos,
        /// de forma recursiva. SidebarMenuGroupControl.Height ya acumula hijos,
        /// pero lo llamamos explícitamente para forzar la lectura más reciente.
        /// </summary>
        private static int GetGroupTotalHeight(SidebarMenuGroupControl group)
        {
            // El control ya gestiona su propia Height recursivamente en UpdateLayout().
            // Simplemente la leemos — es la fuente de verdad.
            return group.Height;
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (_menuContainer == null) return;
            _menuContainer.Width = Width;
        }

        // ══════════════════════════════════════════════════════════════
        // Dashboard integrado
        // ══════════════════════════════════════════════════════════════
        private class DashboardItemControl : Control
        {
            private bool _hovered = false, _active = false;

            public DashboardItemControl()
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
            protected override void OnClick(EventArgs e) { base.OnClick(e); _active = true; Invalidate(); }

            protected override void OnPaint(PaintEventArgs e)
            {
                var g = e.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.Clear(_active ? SidebarTheme.BackgroundActive :
                        _hovered ? SidebarTheme.BackgroundHover :
                                   SidebarTheme.BackgroundDark);

                using (var pen = new Pen(SidebarTheme.BorderColor, 1))
                    g.DrawLine(pen, 0, Height - 1, Width, Height - 1);

                // Ícono FontAwesome
                int iconSize = 16, iconY = (Height - iconSize) / 2;
                using (var bmp = IconChar.ChartLine.ToBitmap(SidebarTheme.TextAccent, iconSize))
                    g.DrawImage(bmp, 12, iconY, iconSize, iconSize);

                using (var b = new SolidBrush(SidebarTheme.TextAccent))
                {
                    var fmt = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center };
                    g.DrawString("DASHBOARD", SidebarTheme.FontGroupTitle, b,
                        new RectangleF(36, 0, Width - 52, Height), fmt);
                }
            }
        }
    }
}