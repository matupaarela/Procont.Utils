using FontAwesome.Sharp;
using Procont.Utils.Core.Extensions;
using Procont.Utils.Core.Theming;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Windows.Forms;

namespace Procont.Utils.Components.ComboSearch
{
    /// <summary>
    /// ComboBox custom con búsqueda incremental dentro del dropdown,
    /// binding Display/Value, estado vacío y botón de acción configurable.
    ///
    /// ── USO MÍNIMO ────────────────────────────────────────────────────
    ///   comboSearch.DataSource    = listaClientes;
    ///   comboSearch.DisplayMember = "Nombre";
    ///   comboSearch.ValueMember   = "Id";
    ///   comboSearch.SelectionCommitted += (s, e) => Console.WriteLine(e.Value);
    ///
    /// ── CON BindableItem ─────────────────────────────────────────────
    ///   comboSearch.DataSource = BindableItem.From(clientes, c => c.Nombre, c => c.Id);
    ///   // DisplayMember y ValueMember se infieren automáticamente.
    ///
    /// ── BOTÓN DE ACCIÓN ──────────────────────────────────────────────
    ///   comboSearch.ActionLabel = "+ Nuevo cliente";
    ///   comboSearch.ActionIcon  = IconChar.UserPlus;
    ///   comboSearch.ActionButtonClicked += (s, e) => AbrirFormulario(e.SearchText);
    /// </summary>
    [ToolboxItem(true)]
    [Description("ComboBox con búsqueda en el dropdown, binding Display/Value y botón de acción.")]
    [DefaultEvent("SelectionCommitted")]
    [DefaultProperty("DataSource")]
    public class ComboSearchBox : Control, IThemeable
    {
        // ══════════════════════════════════════════════════════════════
        // CONTROLES INTERNOS
        // ══════════════════════════════════════════════════════════════

        private readonly ComboSearchDropdown _dropdown;

        // ══════════════════════════════════════════════════════════════
        // ESTADO
        // ══════════════════════════════════════════════════════════════

        private bool _isDropdownOpen = false;
        private bool _isHovered = false;
        private bool _hasSelection = false;
        private object _selectedItem = null;
        private object _selectedValue = null;
        private string _displayText = "";

        // ── Datasource ────────────────────────────────────────────────
        private IList _dataSource;
        private string _displayMember = "";
        private string _valueMember = "";
        private PropertyInfo _displayProp;
        private PropertyInfo _valueProp;

        // ══════════════════════════════════════════════════════════════
        // PROPIEDADES — Datos
        // ══════════════════════════════════════════════════════════════

        [Category("Datos")]
        [Description("Lista de objetos a mostrar. Soporta List<T>, List<BindableItem>, etc.")]
        [DefaultValue(null)]
        public IList DataSource
        {
            get => _dataSource;
            set { _dataSource = value; ResolveMembers(); }
        }

        [Category("Datos")]
        [Description("Nombre de la propiedad a mostrar como texto. Vacío = ToString().")]
        [DefaultValue("")]
        public string DisplayMember
        {
            get => _displayMember;
            set { _displayMember = value ?? ""; ResolveMembers(); }
        }

        [Category("Datos")]
        [Description("Nombre de la propiedad a usar como valor. Vacío = objeto completo.")]
        [DefaultValue("")]
        public string ValueMember
        {
            get => _valueMember;
            set { _valueMember = value ?? ""; ResolveMembers(); }
        }

        // ── Selección (solo lectura) ───────────────────────────────────

        [Browsable(false)]
        public object SelectedItem => _selectedItem;

        [Browsable(false)]
        public object SelectedValue => _selectedValue;

        [Browsable(false)]
        public int SelectedIndex { get; private set; } = -1;

        // ══════════════════════════════════════════════════════════════
        // PROPIEDADES — Apariencia / UX
        // ══════════════════════════════════════════════════════════════

        [Category("ComboSearch")]
        [Description("Texto gris cuando no hay selección.")]
        [DefaultValue("Seleccionar...")]
        public string PlaceholderText
        {
            get => _placeholder;
            set { _placeholder = value ?? ""; Invalidate(); }
        }
        private string _placeholder = "Seleccionar...";

        [Category("ComboSearch")]
        [Description("Texto del placeholder dentro del input de búsqueda del dropdown.")]
        [DefaultValue("Buscar...")]
        public string SearchPlaceholder
        {
            get => _dropdown.SearchPlaceholder;
            set => _dropdown.SearchPlaceholder = value;
        }

        [Category("ComboSearch")]
        [Description("Máximo de ítems visibles sin scroll.")]
        [DefaultValue(10)]
        public int MaxDropdownItems
        {
            get => _dropdown.MaxVisible;
            set => _dropdown.MaxVisible = Math.Max(1, value);
        }

        [Category("ComboSearch — Modo de búsqueda")]
        [Description("Contains: busca en cualquier parte del texto. StartsWith: solo al inicio.")]
        [DefaultValue(ComboSearchMode.Contains)]
        public ComboSearchMode SearchMode
        {
            get => _dropdown.SearchMode;
            set => _dropdown.SearchMode = value;
        }

        [Category("ComboSearch — Modo de búsqueda")]
        [Description("Milisegundos de espera tras el último carácter antes de filtrar.")]
        [DefaultValue(200)]
        public int SearchDelay
        {
            get => _dropdown.SearchDelay;
            set => _dropdown.SearchDelay = value;
        }

        // ══════════════════════════════════════════════════════════════
        // PROPIEDADES — Action button
        // ══════════════════════════════════════════════════════════════

        [Category("ComboSearch — Action Button")]
        [Description("Etiqueta del botón de acción sticky al fondo del dropdown.")]
        [DefaultValue("+ Nuevo")]
        public string ActionLabel
        {
            get => _dropdown.ActionLabel;
            set => _dropdown.UpdateActionButton(value, _dropdown.ActionIcon);
        }

        [Category("ComboSearch — Action Button")]
        [Description("Ícono FontAwesome del botón de acción.")]
        [DefaultValue(IconChar.Plus)]
        public IconChar ActionIcon
        {
            get => _dropdown.ActionIcon;
            set => _dropdown.UpdateActionButton(_dropdown.ActionLabel, value);
        }

        [Category("ComboSearch — Action Button")]
        [Description("Mensaje del empty state. Use {action} para insertar el label del botón.")]
        [DefaultValue("No se encontraron resultados.")]
        public string EmptyStateText
        {
            get => _dropdown.EmptyStateText;
            set => _dropdown.EmptyStateText = value;
        }

        // ══════════════════════════════════════════════════════════════
        // EVENTOS
        // ══════════════════════════════════════════════════════════════

        [Category("ComboSearch")]
        [Description("Mientras el usuario navega por los ítems (↑↓ o hover).")]
        public event EventHandler SelectedIndexChanged;

        [Category("ComboSearch")]
        [Description("El usuario confirmó una selección (Enter o clic).")]
        public event EventHandler<ComboSelectionEventArgs> SelectionCommitted;

        [Category("ComboSearch")]
        [Description("El usuario borró la selección (clic en X o Esc sin selección).")]
        public event EventHandler SelectionCleared;

        [Category("ComboSearch")]
        [Description("El usuario hizo clic en el botón de acción.")]
        public event EventHandler<ComboActionEventArgs> ActionButtonClicked;

        [Category("ComboSearch")]
        public event EventHandler DropdownOpened;

        [Category("ComboSearch")]
        public event EventHandler DropdownClosed;

        // ══════════════════════════════════════════════════════════════
        // CONSTRUCTOR
        // ══════════════════════════════════════════════════════════════

        public ComboSearchBox()
        {
            Height = ComboSearchTheme.InputHeight;
            MinimumSize = new Size(80, ComboSearchTheme.InputHeight);
            MaximumSize = new Size(int.MaxValue, ComboSearchTheme.InputHeight);
            Cursor = Cursors.Hand;

            SetStyle(
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.ResizeRedraw, true);

            // ── Dropdown ──────────────────────────────────────────────
            _dropdown = new ComboSearchDropdown();

            _dropdown.ItemCommitted += Dropdown_ItemCommitted;
            _dropdown.ActionClicked += Dropdown_ActionClicked;
            _dropdown.IndexChanged += (s, e) =>
            {
                var sel = _dropdown.GetSelectedControl();
                if (sel != null) SelectedIndex = sel.ItemIndex;
                SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
            };
            _dropdown.Closed += (s, e) =>
            {
                _isDropdownOpen = false;
                Invalidate();
                DropdownClosed?.Invoke(this, EventArgs.Empty);
            };
        }

        // ══════════════════════════════════════════════════════════════
        // MOUSE / TECLADO en el control principal
        // ══════════════════════════════════════════════════════════════

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            _isHovered = true;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _isHovered = false;
            Invalidate();
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);

            // Clic en la zona X → limpiar selección
            var me = e as MouseEventArgs;
            if (me != null && _hasSelection)
            {
                int xZoneRight = Width - 26;
                int xZoneLeft = xZoneRight - 20;
                if (me.X >= xZoneLeft && me.X <= xZoneRight)
                {
                    ClearSelection();
                    return;
                }
            }

            if (_isDropdownOpen)
                _dropdown.Close();
            else
                OpenDropdown();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Space || e.KeyCode == Keys.Down)
            {
                OpenDropdown();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                ClearSelection();
                e.Handled = true;
            }
        }

        // ══════════════════════════════════════════════════════════════
        // LIFECYCLE
        // ══════════════════════════════════════════════════════════════

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            var form = FindForm();
            if (form != null)
            {
                form.Move += (s, _) => _dropdown.Close();
                form.Resize += (s, _) => _dropdown.Close();
            }
        }

        // ══════════════════════════════════════════════════════════════
        // PINTURA
        // ══════════════════════════════════════════════════════════════

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SetHighQuality();

            var rect = new Rectangle(0, 0, Width - 1, Height - 1);

            // Fondo
            Color bg = _isHovered || _isDropdownOpen
                ? ComboSearchTheme.InputBackground
                : ComboSearchTheme.InputBackground;

            using (var fill = new SolidBrush(bg))
                g.FillRoundedRect(fill, rect, ComboSearchTheme.BorderRadius);

            // Borde — accent cuando el dropdown está abierto
            Color borderColor = _isDropdownOpen
                ? ComboSearchTheme.InputBorderFocus
                : _isHovered
                    ? Color.FromArgb(50, 80, 110)
                    : ComboSearchTheme.InputBorder;

            float borderW = _isDropdownOpen ? 1.5f : 1f;
            using (var pen = new Pen(borderColor, borderW))
                g.DrawRoundedRect(pen, rect, ComboSearchTheme.BorderRadius);

            int padH = ComboSearchTheme.PaddingH;
            int chevronZone = 24;
            int clearZone = _hasSelection ? 22 : 0;
            int textWidth = Width - padH * 2 - chevronZone - clearZone;

            // Texto de selección o placeholder
            if (_hasSelection && !string.IsNullOrEmpty(_displayText))
            {
                using (var b = new SolidBrush(ComboSearchTheme.InputText))
                using (var fmt = new StringFormat
                {
                    LineAlignment = StringAlignment.Center,
                    Trimming = StringTrimming.EllipsisCharacter
                })
                    g.DrawString(_displayText, ComboSearchTheme.FontInput, b,
                        new RectangleF(padH, 0, textWidth, Height), fmt);
            }
            else
            {
                using (var b = new SolidBrush(ComboSearchTheme.InputPlaceholder))
                using (var fmt = new StringFormat
                {
                    LineAlignment = StringAlignment.Center,
                    Trimming = StringTrimming.EllipsisCharacter
                })
                    g.DrawString(_placeholder, ComboSearchTheme.FontInput, b,
                        new RectangleF(padH, 0, textWidth, Height), fmt);
            }

            // Botón X (limpiar selección)
            if (_hasSelection)
            {
                int cx = Width - chevronZone - clearZone / 2 - 2;
                int cy = Height / 2;
                using (var pen = new Pen(ComboSearchTheme.ChevronColor, 1.5f)
                { StartCap = LineCap.Round, EndCap = LineCap.Round })
                {
                    g.DrawLine(pen, cx - 4, cy - 4, cx + 4, cy + 4);
                    g.DrawLine(pen, cx + 4, cy - 4, cx - 4, cy + 4);
                }
            }

            // Chevron
            DrawChevron(g);
        }

        private void DrawChevron(Graphics g)
        {
            int cx = Width - 14;
            int cy = Height / 2;
            int half = 4;

            using (var pen = new Pen(
                _isDropdownOpen ? ComboSearchTheme.InputBorderFocus : ComboSearchTheme.ChevronColor,
                1.5f)
            { StartCap = LineCap.Round, EndCap = LineCap.Round })
            {
                if (_isDropdownOpen)
                {
                    g.DrawLine(pen, cx - half, cy + 2, cx, cy - half + 2);
                    g.DrawLine(pen, cx, cy - half + 2, cx + half, cy + 2);
                }
                else
                {
                    g.DrawLine(pen, cx - half, cy - 2, cx, cy + half - 2);
                    g.DrawLine(pen, cx, cy + half - 2, cx + half, cy - 2);
                }
            }
        }

        // ══════════════════════════════════════════════════════════════
        // LÓGICA DE DROPDOWN
        // ══════════════════════════════════════════════════════════════

        private void OpenDropdown()
        {
            if (_isDropdownOpen) return;
            //if (_dataSource == null) return;

            _isDropdownOpen = true;

            // Pasar el datasource y las funciones de extracción al dropdown
            _dropdown.Open(
                dataSource: _dataSource,
                getDisplay: GetDisplay,
                getValue: GetValue,
                getSubtitle: GetSubtitle,
                getIcon: GetIcon,
                owner: this);

            Invalidate();
            DropdownOpened?.Invoke(this, EventArgs.Empty);
        }

        // ══════════════════════════════════════════════════════════════
        // HANDLERS DEL DROPDOWN
        // ══════════════════════════════════════════════════════════════

        private void Dropdown_ItemCommitted(object sender, ComboSearchItemControl ctrl)
        {
            _hasSelection = true;
            _selectedItem = ctrl.DataItem;
            _selectedValue = ctrl.Value;
            _displayText = ctrl.DisplayText;
            SelectedIndex = ctrl.ItemIndex;

            Invalidate();

            SelectionCommitted?.Invoke(this, new ComboSelectionEventArgs(
                ctrl.DataItem, ctrl.Value, ctrl.DisplayText, ctrl.ItemIndex));
        }

        private void Dropdown_ActionClicked(object sender, ComboActionEventArgs e)
        {
            ActionButtonClicked?.Invoke(this, e);
        }

        // ══════════════════════════════════════════════════════════════
        // SELECCIÓN PROGRAMÁTICA
        // ══════════════════════════════════════════════════════════════

        /// <summary>
        /// Selecciona el ítem cuyo Value coincide con <paramref name="value"/>.
        /// </summary>
        public bool SetSelectedValue(object value)
        {
            if (_dataSource == null) return false;
            foreach (var item in _dataSource)
            {
                if (item == null) continue;
                object v = GetValue(item);
                if (Equals(v, value))
                {
                    _hasSelection = true;
                    _selectedItem = item;
                    _selectedValue = v;
                    _displayText = GetDisplay(item);
                    Invalidate();
                    return true;
                }
            }
            return false;
        }

        /// <summary>Limpia la selección actual.</summary>
        public void Clear() => ClearSelection();

        private void ClearSelection()
        {
            if (!_hasSelection) return;
            _hasSelection = false;
            _selectedItem = null;
            _selectedValue = null;
            _displayText = "";
            SelectedIndex = -1;
            Invalidate();
            SelectionCleared?.Invoke(this, EventArgs.Empty);
        }

        // ══════════════════════════════════════════════════════════════
        // BINDING — reflexión sobre DisplayMember / ValueMember
        // ══════════════════════════════════════════════════════════════

        private void ResolveMembers()
        {
            _displayProp = null;
            _valueProp = null;

            if (_dataSource == null || _dataSource.Count == 0) return;

            Type itemType = null;
            foreach (var item in _dataSource)
                if (item != null) { itemType = item.GetType(); break; }
            if (itemType == null) return;

            // Auto-inferencia para BindableItem
            if (typeof(Core.Models.BindableItem).IsAssignableFrom(itemType))
            {
                if (string.IsNullOrEmpty(_displayMember)) _displayMember = "Display";
                if (string.IsNullOrEmpty(_valueMember)) _valueMember = "Value";
            }

            const BindingFlags bf = BindingFlags.Public | BindingFlags.Instance;
            if (!string.IsNullOrEmpty(_displayMember))
                _displayProp = itemType.GetProperty(_displayMember, bf);
            if (!string.IsNullOrEmpty(_valueMember))
                _valueProp = itemType.GetProperty(_valueMember, bf);
        }

        private string GetDisplay(object item) =>
            _displayProp?.GetValue(item)?.ToString() ?? item.ToString();

        private object GetValue(object item) =>
            _valueProp?.GetValue(item) ?? item;

        private string GetSubtitle(object item)
        {
            if (item is Core.Models.BindableItem bi) return bi.Subtitle ?? "";
            var p = item.GetType().GetProperty("Subtitle", BindingFlags.Public | BindingFlags.Instance)
                 ?? item.GetType().GetProperty("Description", BindingFlags.Public | BindingFlags.Instance);
            return p?.GetValue(item)?.ToString() ?? "";
        }

        private IconChar GetIcon(object item)
        {
            if (item is Core.Models.BindableItem bi) return bi.Icon;
            var p = item.GetType().GetProperty("Icon", BindingFlags.Public | BindingFlags.Instance);
            if (p != null && p.PropertyType == typeof(IconChar))
                return (IconChar)(p.GetValue(item) ?? IconChar.None);
            return IconChar.None;
        }

        // ══════════════════════════════════════════════════════════════
        // IThemeable
        // ══════════════════════════════════════════════════════════════

        public void ApplyTheme() => Invalidate();

        // ══════════════════════════════════════════════════════════════
        // DISPOSE
        // ══════════════════════════════════════════════════════════════

        protected override void Dispose(bool disposing)
        {
            if (disposing) _dropdown?.Dispose();
            base.Dispose(disposing);
        }
    }

    /// <summary>Modo de búsqueda del ComboSearchBox.</summary>
    public enum ComboSearchMode
    {
        [Description("Busca en cualquier parte del texto del ítem.")]
        Contains,
        [Description("Busca solo al inicio del texto del ítem.")]
        StartsWith
    }
}