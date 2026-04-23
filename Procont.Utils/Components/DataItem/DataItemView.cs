using FontAwesome.Sharp;
using Procont.Utils.Components.DataItem.Models;
using Procont.Utils.Components.Sidebar.Models;
using Procont.Utils.Core.Theming;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Procont.Utils.Components.DataItem
{
    [ToolboxItem(true)]
    [Description("Scrollable ItemGroup container for DataItemControl rows.")]
    [DefaultEvent("ItemClicked")]
    [DefaultProperty("Items")]
    public class DataItemView : ScrollableControl, ISupportInitialize
    {
        private readonly List<DataItemModel> _models = new List<DataItemModel>();
        private readonly List<DataItemControl> _items = new List<DataItemControl>();
        private readonly List<Control> _separators = new List<Control>();

        private bool _initializing = false;
        private DataItemControl _activeItem = null;
        private bool _showSeparators = true;
        private DataItemSize _defaultSize = DataItemSize.Default;
        private DataItemVariant _defaultVariant = DataItemVariant.Default;

        // ══════════════════════════════════════════════════════════════
        // EVENTOS
        // ══════════════════════════════════════════════════════════════

        [Category("DataItemView")]
        public event EventHandler<DataItemControl> ItemClicked;

        [Category("DataItemView")]
        public event EventHandler<DataItemControl> ActionClicked;

        // ══════════════════════════════════════════════════════════════
        // PROPIEDADES
        // ══════════════════════════════════════════════════════════════

        [Category("DataItemView")]
        [DefaultValue(true)]
        public bool ShowSeparators
        {
            get => _showSeparators;
            set { _showSeparators = value; RebuildPanel(); }
        }

        [Category("DataItemView")]
        [DefaultValue(DataItemSize.Default)]
        public DataItemSize DefaultSize
        {
            get => _defaultSize;
            set => _defaultSize = value;
        }

        [Category("DataItemView")]
        [DefaultValue(DataItemVariant.Default)]
        public DataItemVariant DefaultVariant
        {
            get => _defaultVariant;
            set => _defaultVariant = value;
        }

        [Category("DataItemView")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public List<DataItemModel> Items => _models;

        [Browsable(false)]
        public DataItemControl ActiveItem => _activeItem;

        [Browsable(false)] public override Color BackColor { get => base.BackColor; set => base.BackColor = value; }
        [Browsable(false)] public override Color ForeColor { get => base.ForeColor; set => base.ForeColor = value; }

        // ══════════════════════════════════════════════════════════════
        // CONSTRUCTOR
        // ══════════════════════════════════════════════════════════════

        public DataItemView()
        {
            base.BackColor = ProcontTheme.SurfaceDark;
            AutoScroll = true;
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint, true);
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
        // FLUENT CODE API
        // ══════════════════════════════════════════════════════════════

        public DataItemBuilder AddItem(
            string title,
            string key = "",
            string description = "",
            IconChar icon = IconChar.None,
            DataItemMediaVariant mediaVariant = DataItemMediaVariant.None)
        {
            var model = new DataItemModel
            {
                Title = title,
                Key = string.IsNullOrEmpty(key) ? title : key,
                Description = description,
                Icon = icon,
                MediaVariant = mediaVariant,
                Size = _defaultSize,
                Variant = _defaultVariant
            };
            var ctrl = BuildControl(model);
            _items.Add(ctrl);
            if (!_initializing) RebuildPanel();
            return new DataItemBuilder(ctrl, this);
        }

        public void AddSeparator(string label = "")
        {
            var sep = BuildSeparator(label);
            _separators.Add(sep);
            _items.Add(null);
            if (!_initializing) RebuildPanel();
        }

        // ══════════════════════════════════════════════════════════════
        // MODEL REBUILD
        // ══════════════════════════════════════════════════════════════

        public void RebuildFromModels()
        {
            if (_initializing) return;
            ClearAll();
            foreach (var m in _models)
            {
                if (m == null) continue;
                _items.Add(BuildControl(m));
            }
            RebuildPanel();
        }

        // ══════════════════════════════════════════════════════════════
        // GENERIC DATA SOURCE
        // ══════════════════════════════════════════════════════════════

        public void SetDataSource<T>(
            IEnumerable<T> source,
            Func<T, string> display,
            Func<T, string> key = null,
            Func<T, string> description = null,
            Func<T, IconChar> icon = null,
            Func<T, DataItemMediaVariant> mediaVariant = null,
            Func<T, SidebarBadge> badge = null,
            Func<T, string> actionLabel = null)
        {
            ClearAll();
            foreach (var item in source)
            {
                if (item == null) continue;
                IconChar ic = icon != null ? icon(item) : IconChar.None;
                DataItemMediaVariant mv = mediaVariant != null
                    ? mediaVariant(item)
                    : (ic != IconChar.None ? DataItemMediaVariant.Icon : DataItemMediaVariant.None);

                var model = new DataItemModel
                {
                    Title = display(item),
                    Key = key?.Invoke(item) ?? display(item),
                    Description = description?.Invoke(item) ?? "",
                    Icon = ic,
                    MediaVariant = mv,
                    Size = _defaultSize,
                    Variant = _defaultVariant,
                    Badge = badge?.Invoke(item) ?? SidebarBadge.None,
                    ActionLabel = actionLabel?.Invoke(item) ?? ""
                };
                _items.Add(BuildControl(model));
            }
            RebuildPanel();
        }

        // ══════════════════════════════════════════════════════════════
        // SELECTION
        // ══════════════════════════════════════════════════════════════

        public bool SelectItem(string key)
        {
            foreach (var item in _items)
            {
                if (item == null) continue;
                if (item.Key == key) { ActivateItem(item); return true; }
            }
            return false;
        }

        public void ClearSelection()
        {
            if (_activeItem != null) { _activeItem.IsActive = false; _activeItem = null; }
        }

        public DataItemControl GetItem(string key)
        {
            foreach (var item in _items)
                if (item != null && item.Key == key) return item;
            return null;
        }

        public void RebuildActions()
        {
            for (int i = 0; i < _models.Count && i < _items.Count; i++)
            {
                if (_models[i] == null || _items[i] == null) continue;
                if (_models[i].Actions.Count > 0)
                    _items[i].BuildActionsFromModel(_models[i].Actions);
            }
        }

        // ══════════════════════════════════════════════════════════════
        // BUILD HELPERS
        // ══════════════════════════════════════════════════════════════

        private DataItemControl BuildControl(DataItemModel m)
        {
            var ctrl = new DataItemControl
            {
                Key = m.Key,
                Title = m.Title,
                Description = m.Description,
                Icon = m.Icon,
                MediaVariant = m.MediaVariant,
                Variant = m.Variant,
                Size = m.Size,
                Badge = m.Badge,
                ActionLabel = m.ActionLabel,
                AvatarInitials = m.AvatarInitials,
                MediaImage = m.MediaImage
            };

            if (m.Actions != null && m.Actions.Count > 0)
                ctrl.BuildActionsFromModel(m.Actions);

            ctrl.ItemClicked += (s, e) => ActivateItem(ctrl);
            ctrl.ActionClicked += (s, e) => ActionClicked?.Invoke(this, ctrl);
            return ctrl;
        }

        private Control BuildSeparator(string label)
        {
            bool hasLabel = !string.IsNullOrEmpty(label);
            var sep = new Panel
            {
                Height = hasLabel ? 26 : 1,
                Dock = DockStyle.Top,
                BackColor = hasLabel ? ProcontTheme.SurfaceDark : ProcontTheme.BorderDefault
            };

            if (hasLabel)
            {
                string captured = label;
                sep.Paint += (s, pe) =>
                {
                    pe.Graphics.Clear(ProcontTheme.SurfaceDark);
                    using (var b = new SolidBrush(ProcontTheme.TextSubdued))
                    using (var fmt = new StringFormat { LineAlignment = StringAlignment.Center })
                        pe.Graphics.DrawString(captured, ProcontTheme.FontSmall, b,
                            new RectangleF(12, 0, sep.Width - 24f, sep.Height), fmt);
                    using (var pen = new Pen(ProcontTheme.BorderDefault, 1))
                        pe.Graphics.DrawLine(pen, 0, sep.Height - 1, sep.Width, sep.Height - 1);
                };
            }
            return sep;
        }

        private void ActivateItem(DataItemControl ctrl)
        {
            if (_activeItem != null && _activeItem != ctrl)
                _activeItem.IsActive = false;
            _activeItem = ctrl;
            ctrl.IsActive = true;
            ItemClicked?.Invoke(this, ctrl);
        }

        private void ClearAll()
        {
            Controls.Clear();
            _items.Clear();
            _separators.Clear();
            _activeItem = null;
        }

        private void RebuildPanel()
        {
            Controls.Clear();
            int sepIdx = 0;
            for (int i = _items.Count - 1; i >= 0; i--)
            {
                var item = _items[i];
                if (item == null)
                {
                    int realSepIdx = CountNullsUpTo(i);
                    if (realSepIdx < _separators.Count)
                        Controls.Add(_separators[realSepIdx]);
                }
                else
                {
                    Controls.Add(item);
                    if (_showSeparators && i > 0)
                    {
                        var line = new Panel
                        {
                            Height = 1,
                            Dock = DockStyle.Top,
                            BackColor = ProcontTheme.BorderDefault
                        };
                        Controls.Add(line);
                    }
                }
            }
        }

        private int CountNullsUpTo(int index)
        {
            int count = 0;
            for (int i = 0; i < index; i++)
                if (_items[i] == null) count++;
            return count;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(ProcontTheme.SurfaceDark);
        }
    }

    // ══════════════════════════════════════════════════════════════════
    // FLUENT BUILDER
    // ══════════════════════════════════════════════════════════════════

    public sealed class DataItemBuilder
    {
        private readonly DataItemControl _ctrl;
        private readonly DataItemView _view;

        internal DataItemBuilder(DataItemControl ctrl, DataItemView view)
        {
            _ctrl = ctrl;
            _view = view;
        }

        public DataItemBuilder WithDescription(string text)
        { _ctrl.Description = text; return this; }

        public DataItemBuilder WithBadge(SidebarBadge badge)
        { _ctrl.Badge = badge; return this; }

        public DataItemBuilder WithActionLabel(string label)
        { _ctrl.ActionLabel = label; return this; }

        public DataItemBuilder WithVariant(DataItemVariant v)
        { _ctrl.Variant = v; return this; }

        public DataItemBuilder WithSize(DataItemSize s)
        { _ctrl.Size = s; return this; }

        public DataItemBuilder WithAvatarInitials(string initials)
        { _ctrl.MediaVariant = DataItemMediaVariant.Avatar; _ctrl.AvatarInitials = initials; return this; }

        public DataItemBuilder WithImage(Image img)
        { _ctrl.MediaVariant = DataItemMediaVariant.Image; _ctrl.MediaImage = img; return this; }

        /// <summary>
        /// Agrega un ActionButton al área de acciones.
        /// isSplit=false → ícono simple.
        /// isSplit=true  → [Label | ▼] con dropdown; usar configure() para añadir opciones.
        /// </summary>
        public DataItemBuilder WithActionButton(
            string label,
            IconChar icon = IconChar.None,
            bool isSplit = false,
            Action<ActionButton> configure = null)
        {
            var btn = _ctrl.AddActionButton(label, label, icon, isSplit);
            configure?.Invoke(btn);
            return this;
        }

        /// <summary>
        /// Agrega un ícono de acción simple (sin texto, sin dropdown).
        /// </summary>
        public DataItemBuilder WithIconAction(
            string key,
            IconChar icon,
            EventHandler handler = null,
            string tooltip = "")
        {
            var btn = _ctrl.AddActionButton(key, "", icon, isSplit: false);
            if (handler != null) btn.PrimaryClicked += handler;
            if (!string.IsNullOrEmpty(tooltip))
                new ToolTip().SetToolTip(btn, tooltip);
            return this;
        }

        public DataItemBuilder OnClicked(EventHandler handler)
        { _ctrl.ItemClicked += handler; return this; }

        public DataItemBuilder OnActionClicked(EventHandler handler)
        { _ctrl.ActionClicked += handler; return this; }

        public DataItemControl Build() => _ctrl;
    }
}