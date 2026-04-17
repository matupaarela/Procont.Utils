using FontAwesome.Sharp;
using Procont.Utils.Core.Extensions;
using Procont.Utils.Core.Theming;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Procont.Utils.Components.ComboSearch
{
    /// <summary>
    /// Dropdown flotante del ComboSearchBox.
    /// Contiene el input de búsqueda, la lista de ítems,
    /// el empty state y el botón de acción sticky.
    ///
    /// ── ESTRUCTURA VISUAL ────────────────────────────────────────────
    ///   ┌─ ToolStripDropDown ─────────────────────────────────┐
    ///   │  ┌─ SearchBox (TextBox con lupa) ─────────────────┐ │
    ///   │  └─────────────────────────────────────────────── │
    ///   │  ┌─ ScrollPanel (máx N ítems) ──────────────────┐ │
    ///   │  │  ComboSearchItemControl × N                   │ │
    ///   │  └─────────────────────────────────────────────── │
    ///   │  ┌─ EmptyStatePanel (si no hay resultados) ─────┐ │
    ///   │  └─────────────────────────────────────────────── │
    ///   │  ┌─ ActionButton (sticky) ──────────────────────┐ │
    ///   │  └─────────────────────────────────────────────── │
    ///   └─────────────────────────────────────────────────── ┘
    /// </summary>
    internal class ComboSearchDropdown : ToolStripDropDown
    {
        // ── Controles internos ────────────────────────────────────────
        private readonly Panel _borderPanel;
        private readonly Panel _root;
        private readonly SearchInputPanel _searchPanel;
        private readonly Panel _scrollPanel;
        private readonly EmptyStatePanel _emptyPanel;
        private readonly IconButton _actionButton;
        private readonly ToolStripControlHost _host;
        private readonly Timer _debounceTimer;

        // ── Estado ────────────────────────────────────────────────────
        private readonly List<ComboSearchItemControl> _itemControls
            = new List<ComboSearchItemControl>();
        private int _selectedIndex = -1;

        // ── Datasource y funciones de extracción ──────────────────────
        private IList _dataSource;
        private Func<object, string> _getDisplay;
        private Func<object, object> _getValue;
        private Func<object, string> _getSubtitle;
        private Func<object, IconChar> _getIcon;

        // ══════════════════════════════════════════════════════════════
        // CONFIGURACIÓN (expuesta desde ComboSearchBox)
        // ══════════════════════════════════════════════════════════════

        public string ActionLabel { get; set; } = "+ Nuevo";
        public IconChar ActionIcon { get; set; } = IconChar.Plus;
        public string EmptyStateText { get; set; } = "No hay ítems. Puedes agregarlo desde el botón {action}.";
        public int MaxVisible { get; set; } = ComboSearchTheme.MaxVisibleItems;
        public ComboSearchMode SearchMode { get; set; } = ComboSearchMode.Contains;
        public string SearchPlaceholder
        {
            get => _searchPanel.Placeholder;
            set => _searchPanel.Placeholder = value;
        }
        public int SearchDelay
        {
            get => _debounceTimer.Interval;
            set => _debounceTimer.Interval = Math.Max(0, value);
        }

        // ══════════════════════════════════════════════════════════════
        // EVENTOS (consumidos por ComboSearchBox)
        // ══════════════════════════════════════════════════════════════

        /// <summary>El usuario confirmó un ítem (Enter o clic).</summary>
        public event EventHandler<ComboSearchItemControl> ItemCommitted;

        /// <summary>El usuario hizo clic en el action button.</summary>
        public event EventHandler<ComboActionEventArgs> ActionClicked;

        /// <summary>La selección navegada cambió (↑↓).</summary>
        public event EventHandler IndexChanged;

        // ══════════════════════════════════════════════════════════════
        // ÍNDICE SELECCIONADO (navegación)
        // ══════════════════════════════════════════════════════════════

        public int SelectedIndex
        {
            get => _selectedIndex;
            private set
            {
                if (_selectedIndex == value) return;
                if (_selectedIndex >= 0 && _selectedIndex < _itemControls.Count)
                    _itemControls[_selectedIndex].IsSelected = false;

                _selectedIndex = value;

                if (_selectedIndex >= 0 && _selectedIndex < _itemControls.Count)
                {
                    _itemControls[_selectedIndex].IsSelected = true;
                    EnsureVisible(_itemControls[_selectedIndex]);
                }

                IndexChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        // ══════════════════════════════════════════════════════════════
        // CONSTRUCTOR
        // ══════════════════════════════════════════════════════════════

        public ComboSearchDropdown()
        {
            AutoSize = false;
            Padding = Padding.Empty;
            Margin = Padding.Empty;
            BackColor = ComboSearchTheme.DropdownBackground;
            DropShadowEnabled = false;

            // ── Debounce timer ─────────────────────────────────────────
            _debounceTimer = new Timer { Interval = 200 };
            _debounceTimer.Tick += (s, e) =>
            {
                _debounceTimer.Stop();
                RebuildItems(_searchPanel.Text);
            };

            // ── Panel raíz ─────────────────────────────────────────────
            _root = new Panel
            {
                Margin = Padding.Empty,
                Padding = Padding.Empty,
                BackColor = ComboSearchTheme.DropdownBackground,
                Dock = DockStyle.Fill,
            };

            // ── Wrapper con borde de 1 px ──────────────────────────────────
            _borderPanel = new Panel
            {
                Margin = Padding.Empty,
                Padding = new Padding(1),    // ← el gap que muestra el borde
                BackColor = SystemColors.ControlDark
            };
            _borderPanel.Controls.Add(_root);

            // ── Input de búsqueda (parte superior) ─────────────────────
            _searchPanel = new SearchInputPanel { Dock = DockStyle.Top };
            _searchPanel.TextChanged += (s, e) =>
            {
                _debounceTimer.Stop();
                _debounceTimer.Start();
            };
            _searchPanel.KeyDown += SearchPanel_KeyDown;

            // ── Panel scroll de ítems ──────────────────────────────────
            _scrollPanel = new Panel
            {
                Dock = DockStyle.Top,
                AutoScroll = true,
                BackColor = ComboSearchTheme.DropdownBackground,
                Height = 0
            };
            _scrollPanel.HorizontalScroll.Maximum = 0;
            _scrollPanel.HorizontalScroll.Visible = false;
            _scrollPanel.AutoScrollMinSize = new Size(0, 0);

            // ── Empty state ────────────────────────────────────────────
            _emptyPanel = new EmptyStatePanel { Dock = DockStyle.Top, Visible = false };

            // ── Action button sticky ───────────────────────────────────
            _actionButton = new IconButton { Dock = DockStyle.Bottom, TextImageRelation = TextImageRelation.ImageBeforeText };
            _actionButton.Click += (s, e) =>
            {
                var args = new ComboActionEventArgs(_searchPanel.Text);
                ActionClicked?.Invoke(this, args);
                Close();
            };

            _root.Controls.Add(_scrollPanel);
            _root.Controls.Add(_emptyPanel);
            _root.Controls.Add(_searchPanel);
            _root.Controls.Add(_actionButton);

            _host = new ToolStripControlHost(_borderPanel)
            {
                Margin = Padding.Empty,
                Padding = Padding.Empty,
                AutoSize = false
            };
            Items.Add(_host);
        }

        // ══════════════════════════════════════════════════════════════
        // API PÚBLICA — llamada desde ComboSearchBox
        // ══════════════════════════════════════════════════════════════

        /// <summary>
        /// Abre el dropdown bajo <paramref name="owner"/> con el datasource dado.
        /// </summary>
        public void Open(
            IList dataSource,
            Func<object, string> getDisplay,
            Func<object, object> getValue,
            Func<object, string> getSubtitle,
            Func<object, IconChar> getIcon,
            Control owner)
        {
            _dataSource = dataSource;
            _getDisplay = getDisplay;
            _getValue = getValue;
            _getSubtitle = getSubtitle;
            _getIcon = getIcon;

            _searchPanel.Clear();
            _selectedIndex = -1;

            int w = owner.Width;
            SetWidth(w);
            RebuildItems("");

            // Posicionamiento con flip vertical
            var screenPos = owner.PointToScreen(new Point(0, owner.Height + 2));
            var screen = Screen.FromControl(owner).WorkingArea;
            if (screenPos.Y + Height > screen.Bottom)
                screenPos = owner.PointToScreen(new Point(0, -Height - 2));

            Show(screenPos);

            // Dar foco al input de búsqueda después de mostrarse
            BeginInvoke(new Action(() => _searchPanel.FocusInput()));
        }

        /// <summary>
        /// Actualiza etiqueta e ícono del action button.
        /// </summary>
        public void UpdateActionButton(string label, IconChar icon)
        {
            ActionLabel = label;
            ActionIcon = icon;
            _actionButton.Text = label;
            _actionButton.IconChar = icon;
            _actionButton.IconSize = ComboSearchTheme.IconSize;
            _actionButton.TextAlign = ContentAlignment.MiddleLeft;
            _actionButton.ImageAlign = ContentAlignment.MiddleRight;
            _actionButton.Invalidate();
        }

        public void MoveUp()
        {
            if (_itemControls.Count == 0) return;
            SelectedIndex = _selectedIndex <= 0
                ? _itemControls.Count - 1
                : _selectedIndex - 1;
        }

        public void MoveDown()
        {
            if (_itemControls.Count == 0) return;
            SelectedIndex = _selectedIndex >= _itemControls.Count - 1
                ? 0
                : _selectedIndex + 1;
        }

        public ComboSearchItemControl GetSelectedControl() =>
            (_selectedIndex >= 0 && _selectedIndex < _itemControls.Count)
                ? _itemControls[_selectedIndex]
                : null;

        // ══════════════════════════════════════════════════════════════
        // FILTRADO Y RECONSTRUCCIÓN
        // ══════════════════════════════════════════════════════════════

        private void RebuildItems(string filter)
        {
            _scrollPanel.Controls.Clear();
            _itemControls.Clear();
            _selectedIndex = -1;

            if (_dataSource == null || _dataSource.Count == 0)
            {
                ShowEmpty(EmptyStateText.Replace("{action}", ActionLabel));
                return;
            }

            // Filtrar
            var filtered = new List<(object item, string display, string subtitle, IconChar icon, object value)>();
            bool hasFilter = !string.IsNullOrEmpty(filter);

            foreach (var raw in _dataSource)
            {
                if (raw == null) continue;
                string display = _getDisplay(raw);
                if (hasFilter)
                {
                    bool match = SearchMode == ComboSearchMode.Contains
                        ? display.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0
                        : display.StartsWith(filter, StringComparison.OrdinalIgnoreCase);
                    if (!match) continue;
                }
                filtered.Add((raw, display, _getSubtitle(raw), _getIcon(raw), _getValue(raw)));
            }

            if (filtered.Count == 0)
            {
                string msg = EmptyStateText.Replace("{action}", ActionLabel);
                ShowEmpty(msg);
                return;
            }

            _emptyPanel.Visible = false;
            _scrollPanel.Visible = true;

            // Construir controles (Dock.Top = LIFO → agregar en reversa)
            for (int i = 0; i < filtered.Count; i++)
            {
                var d = filtered[i];
                var ctrl = new ComboSearchItemControl(
                    d.item, d.display, d.subtitle, d.icon, d.value, i);
                _itemControls.Add(ctrl);
            }

            for (int i = _itemControls.Count - 1; i >= 0; i--)
            {
                var ctrl = _itemControls[i];
                int idx = i;
                ctrl.ItemClicked += (s, e) =>
                {
                    SelectedIndex = idx;
                    ItemCommitted?.Invoke(this, ctrl);
                    Close();
                };
                _scrollPanel.Controls.Add(ctrl);
            }

            RecalcHeight(filtered.Count);
        }

        private void ShowEmpty(string message)
        {
            _emptyPanel.SetMessage(message);
            _emptyPanel.Visible = true;
            _scrollPanel.Visible = false;
            RecalcHeight(0);
        }

        // ══════════════════════════════════════════════════════════════
        // TECLADO — manejado en el SearchInputPanel
        // ══════════════════════════════════════════════════════════════

        private void SearchPanel_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Down:
                    MoveDown();
                    e.Handled = true;
                    break;

                case Keys.Up:
                    MoveUp();
                    e.Handled = true;
                    break;

                case Keys.Enter:
                    var sel = GetSelectedControl();
                    if (sel != null)
                    {
                        ItemCommitted?.Invoke(this, sel);
                        Close();
                    }
                    e.Handled = true;
                    break;

                case Keys.Escape:
                    Close();
                    e.Handled = true;
                    break;
            }
        }

        // ══════════════════════════════════════════════════════════════
        // LAYOUT
        // ══════════════════════════════════════════════════════════════

        private void SetWidth(int w)
        {
            //_root.Width = w;
            //_searchPanel.Width = w;
            //_scrollPanel.Width = w;
            //_emptyPanel.Width = w;
            //_actionButton.Width = w;

            _borderPanel.Width = w;
            _host.Width = w;
            Width = w;
        }

        private void RecalcHeight(int itemCount)
        {
            //int searchH = ComboSearchTheme.SearchInputHeight;
            //int visible = Math.Min(itemCount, MaxVisible);
            //int scrollH = visible * ComboSearchTheme.ItemHeight;
            //int emptyH = _emptyPanel.Visible ? ComboSearchTheme.EmptyStateHeight : 0;
            //int actionH = ComboSearchTheme.ActionHeight;

            //_scrollPanel.Height = scrollH;
            //_emptyPanel.Height = emptyH;

            //int totalH = searchH + scrollH + emptyH + actionH;
            //_root.Height = totalH;
            //_host.Height = totalH;
            //Height = totalH;

            int searchH = ComboSearchTheme.SearchInputHeight;
            int visible = Math.Min(itemCount, MaxVisible);
            int scrollH = visible * ComboSearchTheme.ItemHeight;
            int emptyH = _emptyPanel.Visible ? ComboSearchTheme.EmptyStateHeight : 0;
            int actionH = ComboSearchTheme.ActionHeight;

            _scrollPanel.Height = scrollH;
            _emptyPanel.Height = emptyH;

            int totalH = searchH + scrollH + emptyH + actionH;
            _root.Height = totalH;
            _borderPanel.Height = totalH + 2;   // + 2 por el 1px de borde arriba/abajo
            _host.Height = totalH + 2;
            Height = totalH + 2;
        }

        private void EnsureVisible(Control ctrl)
        {
            if (ctrl == null) return;
            int top = ctrl.Top;
            int bot = ctrl.Bottom;
            int scrollTop = -_scrollPanel.AutoScrollPosition.Y;
            int scrollBot = scrollTop + _scrollPanel.Height;
            if (top < scrollTop)
                _scrollPanel.AutoScrollPosition = new Point(0, top);
            else if (bot > scrollBot)
                _scrollPanel.AutoScrollPosition = new Point(0, bot - _scrollPanel.Height);
        }

        // ══════════════════════════════════════════════════════════════
        // CONTROLES INTERNOS PRIVADOS
        // ══════════════════════════════════════════════════════════════

        /// <summary>
        /// Panel con TextBox de búsqueda + ícono de lupa.
        /// </summary>
        private class SearchInputPanel : Panel
        {
            private readonly TextBox _tb;
            private string _placeholder = "Buscar...";
            private bool _focused = false;

            public string Text => _tb.Text;
            public new event KeyEventHandler KeyDown;
            public new event EventHandler TextChanged;

            public string Placeholder
            {
                get => _placeholder;
                set { _placeholder = value ?? ""; _tb.Invalidate(); Invalidate(); }
            }

            public SearchInputPanel()
            {
                Height = ComboSearchTheme.SearchInputHeight;
                BackColor = ComboSearchTheme.SearchBackground;
                Padding = Padding.Empty;

                SetStyle(ControlStyles.OptimizedDoubleBuffer |
                         ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.UserPaint, true);

                _tb = new TextBox
                {
                    BorderStyle = BorderStyle.None,
                    BackColor = ComboSearchTheme.SearchBackground,
                    ForeColor = ComboSearchTheme.InputText,
                    Font = ComboSearchTheme.FontInput,
                    AutoSize = false
                };

                _tb.GotFocus += (s, e) => { _focused = true; Invalidate(); };
                _tb.LostFocus += (s, e) => { _focused = false; Invalidate(); };
                _tb.TextChanged += (s, e) => TextChanged?.Invoke(this, e);
                _tb.KeyDown += (s, e) => KeyDown?.Invoke(s, e);

                Controls.Add(_tb);
                PositionInput();
            }

            public void Clear() => _tb.Text = "";
            public void FocusInput() => _tb.Focus();

            protected override void OnResize(EventArgs e)
            {
                base.OnResize(e);
                PositionInput();
            }

            private void PositionInput()
            {
                if (_tb == null) return;
                int iconW = 16 + 8; // ícono + gap
                int padH = ComboSearchTheme.PaddingH;
                int padV = (Height - _tb.PreferredHeight) / 2;
                _tb.SetBounds(
                    padH + iconW,
                    padV,
                    Width - padH * 2 - iconW,
                    _tb.PreferredHeight);
                Invalidate();
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                var g = e.Graphics;
                g.SetHighQuality();
                g.Clear(BackColor);

                // Línea divisoria inferior
                using (var pen = new Pen(ComboSearchTheme.DropdownBorder, 1))
                    g.DrawLine(pen, 8, Height - 1, Width - 8, Height - 1);

                // Ícono lupa
                int iconSize = 14;
                int iconX = ComboSearchTheme.PaddingH;
                int iconY = (Height - iconSize) / 2;
                try
                {
                    using (var bmp = IconChar.Search.ToBitmap(
                        _focused ? ComboSearchTheme.InputBorderFocus : ComboSearchTheme.ChevronColor,
                        iconSize))
                        g.DrawImage(bmp, iconX, iconY, iconSize, iconSize);
                }
                catch { /* FontAwesome puede no estar disponible en design-time */ }

                // Placeholder cuando el TextBox está vacío
                if (string.IsNullOrEmpty(_tb.Text))
                {
                    using (var b = new SolidBrush(ComboSearchTheme.InputPlaceholder))
                    using (var fmt = new StringFormat { LineAlignment = StringAlignment.Center })
                        g.DrawString(_placeholder, ComboSearchTheme.FontInput, b,
                            new RectangleF(_tb.Left, 0, _tb.Width, Height), fmt);
                }
            }
        }

        /// <summary>Panel de estado vacío con mensaje CTA.</summary>
        private class EmptyStatePanel : Panel
        {
            private string _message = "";

            public EmptyStatePanel()
            {
                BackColor = ComboSearchTheme.DropdownBackground;
                SetStyle(ControlStyles.OptimizedDoubleBuffer |
                         ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.UserPaint, true);
            }

            public void SetMessage(string msg) { _message = msg ?? ""; Invalidate(); }

            protected override void OnPaint(PaintEventArgs e)
            {
                var g = e.Graphics;
                g.SetHighQuality();
                g.Clear(BackColor);

                if (string.IsNullOrEmpty(_message)) return;

                using (var b = new SolidBrush(ComboSearchTheme.EmptyText))
                using (var fmt = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center,
                    Trimming = StringTrimming.EllipsisCharacter
                })
                    g.DrawString(_message, ComboSearchTheme.FontEmpty, b,
                        new RectangleF(12, 0, Width - 24, Height), fmt);
            }
        }

        /// <summary>Botón de acción sticky, siempre visible al fondo.</summary>
        private class ActionButton : Control
        {
            private bool _hovered = false;

            public string Label { get; set; } = "+ Nuevo";
            public IconChar Icon { get; set; } = IconChar.Plus;
            public event EventHandler Clicked;

            public ActionButton()
            {
                Height = ComboSearchTheme.ActionHeight;
                Cursor = Cursors.Hand;
                BackColor = ComboSearchTheme.ActionBackground;
                SetStyle(ControlStyles.OptimizedDoubleBuffer |
                         ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.UserPaint, true);
            }

            protected override void OnMouseEnter(EventArgs e) { base.OnMouseEnter(e); _hovered = true; Invalidate(); }
            protected override void OnMouseLeave(EventArgs e) { base.OnMouseLeave(e); _hovered = false; Invalidate(); }
            protected override void OnClick(EventArgs e) { base.OnClick(e); Clicked?.Invoke(this, EventArgs.Empty); }

            protected override void OnPaint(PaintEventArgs e)
            {
                var g = e.Graphics;
                g.SetHighQuality();
                g.Clear(_hovered ? ComboSearchTheme.ActionBackgroundHover : ComboSearchTheme.ActionBackground);

                using (var pen = new Pen(ComboSearchTheme.ActionBorderTop, 1))
                    g.DrawLine(pen, 0, 0, Width, 0);

                int x = ComboSearchTheme.PaddingH;
                if (Icon != IconChar.None)
                {
                    int is_ = ComboSearchTheme.IconSize;
                    int iy = (Height - is_) / 2;
                    try
                    {
                        using (var bmp = Icon.ToBitmap(ComboSearchTheme.ActionIcon, is_))
                            g.DrawImage(bmp, x, iy, is_, is_);
                    }
                    catch { }
                    x += is_ + 8;
                }

                using (var b = new SolidBrush(ComboSearchTheme.ActionText))
                using (var fmt = new StringFormat { LineAlignment = StringAlignment.Center })
                    g.DrawString(Label, ComboSearchTheme.FontAction, b,
                        new RectangleF(x, 0, Width - x - ComboSearchTheme.PaddingH, Height), fmt);
            }
        }

        // ══════════════════════════════════════════════════════════════
        // DISPOSE
        // ══════════════════════════════════════════════════════════════

        protected override void Dispose(bool disposing)
        {
            if (disposing) _debounceTimer?.Dispose();
            base.Dispose(disposing);
        }
    }
}