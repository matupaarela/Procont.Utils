using FontAwesome.Sharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Procont.Utils.Components.ComboSearch
{
    /// <summary>
    /// Lista virtual de ítems del dropdown.
    ///
    /// ── POR QUÉ VIRTUAL ──────────────────────────────────────────────
    /// La versión anterior creaba un <see cref="Control"/> por cada ítem.
    /// Con datasources grandes (200+ items) eso crea 200 HWND handles,
    /// 200 message loops y 200 OnPaint calls. Este panel dibuja todos
    /// los ítems en un solo OnPaint haciendo hit-testing por coordenada Y.
    ///
    /// ── RESULTADO ────────────────────────────────────────────────────
    /// - 1 HWND en vez de N.
    /// - OnPaint dibuja solo los ítems dentro del clip rectangle.
    /// - SetItems es O(n) sin crear objetos de UI.
    /// - Hover y selección son O(1) por hit-test.
    /// </summary>
    internal sealed class VirtualItemsPanel : Panel
    {
        // ── Modelo de ítem (struct para evitar heap pressure) ─────────
        internal readonly struct ItemEntry
        {
            public readonly object DataItem;
            public readonly string DisplayText;
            public readonly string Subtitle;
            public readonly IconChar Icon;
            public readonly object Value;
            public readonly int Index;

            public ItemEntry(object data, string display, string subtitle,
                             IconChar icon, object value, int index)
            {
                DataItem = data;
                DisplayText = display;
                Subtitle = subtitle;
                Icon = icon;
                Value = value;
                Index = index;
            }
        }

        // ── Estado ────────────────────────────────────────────────────
        private List<ItemEntry> _items = new List<ItemEntry>();
        private int _hovered = -1;
        private int _selected = -1;
        private int _itemHeight;

        // ── Caché de íconos (bitmap por IconChar+Color+Size) ──────────
        // Evita llamar Icon.ToBitmap() en cada frame para el mismo ícono.
        private readonly Dictionary<(IconChar, Color, int), Bitmap> _iconCache
            = new Dictionary<(IconChar, Color, int), Bitmap>();

        // ── Eventos ───────────────────────────────────────────────────
        public event EventHandler<ItemEntry> ItemClicked;
        public event EventHandler<ItemEntry> ItemHovered;

        // ── Propiedades ───────────────────────────────────────────────
        public int SelectedIndex
        {
            get => _selected;
            set
            {
                if (_selected == value) return;
                int prev = _selected;
                _selected = value;
                InvalidateRow(prev);
                InvalidateRow(_selected);
                EnsureVisible(_selected);
            }
        }

        public ItemEntry? SelectedItem =>
            _selected >= 0 && _selected < _items.Count
                ? _items[_selected]
                : (ItemEntry?)null;

        // ── Constructor ───────────────────────────────────────────────
        public VirtualItemsPanel()
        {
            _itemHeight = ComboSearchTheme.ItemHeight;

            // Double buffer máximo: UserPaint + OptimizedDoubleBuffer + WS_EX_COMPOSITED
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw, true);

            BackColor = ComboSearchTheme.DropdownBackground;
            AutoScroll = true;
            HorizontalScroll.Maximum = 0;
            HorizontalScroll.Visible = false;
            AutoScrollMinSize = new Size(0, 0);
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= 0x02000000; // WS_EX_COMPOSITED — compositing a nivel de SO
                return cp;
            }
        }

        // ══════════════════════════════════════════════════════════════
        // API PÚBLICA
        // ══════════════════════════════════════════════════════════════

        /// <summary>
        /// Reemplaza la lista de ítems y ajusta la altura virtual.
        /// No crea ningún Control adicional.
        /// </summary>
        public void SetItems(List<ItemEntry> items)
        {
            _items = items ?? new List<ItemEntry>();
            _hovered = -1;
            _selected = -1;

            // Altura virtual = N ítems × altura por ítem
            int totalH = _items.Count * _itemHeight;
            AutoScrollMinSize = new Size(0, totalH);
            // Volver al tope
            AutoScrollPosition = new Point(0, 0);

            Invalidate();
        }

        public void MoveUp()
        {
            if (_items.Count == 0) return;
            SelectedIndex = _selected <= 0 ? _items.Count - 1 : _selected - 1;
        }

        public void MoveDown()
        {
            if (_items.Count == 0) return;
            SelectedIndex = _selected >= _items.Count - 1 ? 0 : _selected + 1;
        }

        public void ClearSelection()
        {
            int prev = _selected;
            _selected = -1;
            InvalidateRow(prev);
        }

        // ══════════════════════════════════════════════════════════════
        // PINTURA — owner-draw completo
        // ══════════════════════════════════════════════════════════════

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            var clip = e.ClipRectangle;
            int scrollY = -AutoScrollPosition.Y;

            // Calcular rango de ítems visibles en el clip (evitar dibujar off-screen)
            int firstVisible = Math.Max(0, (clip.Top + scrollY) / _itemHeight);
            int lastVisible = Math.Min(_items.Count - 1,
                               (clip.Bottom + scrollY) / _itemHeight);

            for (int i = firstVisible; i <= lastVisible; i++)
            {
                var item = _items[i];
                int y = i * _itemHeight - scrollY;
                var row = new Rectangle(0, y, Width, _itemHeight);

                PaintRow(g, item, row, i == _selected, i == _hovered);
            }
        }

        private void PaintRow(Graphics g, in ItemEntry item,
                               Rectangle row, bool selected, bool hovered)
        {
            // Fondo
            var brush = selected ? ComboSearchRenderer.BrushSelected
                      : hovered ? ComboSearchRenderer.BrushHover
                                 : ComboSearchRenderer.BrushBackground;
            g.FillRectangle(brush, row);

            // Barra lateral izquierda (seleccionado)
            if (selected)
                g.DrawLine(ComboSearchRenderer.PenActiveBar,
                    1, row.Y + 5, 1, row.Bottom - 5);

            int x = ComboSearchTheme.PaddingH;
            bool hasIcon = item.Icon != IconChar.None;
            bool hasSubtitle = !string.IsNullOrEmpty(item.Subtitle);

            // Ícono (desde caché)
            if (hasIcon)
            {
                int is_ = ComboSearchTheme.IconSize;
                int iy = row.Y + (row.Height - is_) / 2;
                Color col = selected
                    ? ComboSearchTheme.ItemIconActive
                    : ComboSearchTheme.ItemIconColor;
                var bmp = GetCachedIcon(item.Icon, col, is_);
                if (bmp != null) g.DrawImage(bmp, x, iy, is_, is_);
                x += is_ + 8;
            }

            int textW = Width - x - ComboSearchTheme.PaddingH;

            if (hasSubtitle)
            {
                // Dos líneas
                int mainY = row.Y + row.Height / 2 - 10;
                int subY = row.Y + row.Height / 2 + 1;

                g.DrawString(item.DisplayText, ComboSearchTheme.FontItem,
                    ComboSearchRenderer.BrushText,
                    new RectangleF(x, mainY, textW, 14),
                    ComboSearchRenderer.FmtLeftTop);

                g.DrawString(item.Subtitle, ComboSearchTheme.FontSubtitle,
                    ComboSearchRenderer.BrushSubtitle,
                    new RectangleF(x, subY, textW, 12),
                    ComboSearchRenderer.FmtLeftTop);
            }
            else
            {
                g.DrawString(item.DisplayText, ComboSearchTheme.FontItem,
                    ComboSearchRenderer.BrushText,
                    new RectangleF(x, row.Y, textW, row.Height),
                    ComboSearchRenderer.FmtLeft);
            }
        }

        // ══════════════════════════════════════════════════════════════
        // MOUSE — hit-test por coordenada Y
        // ══════════════════════════════════════════════════════════════

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            int idx = HitTest(e.Y);
            if (idx == _hovered) return;
            int prev = _hovered;
            _hovered = idx;
            InvalidateRow(prev);
            InvalidateRow(_hovered);
            if (_hovered >= 0) ItemHovered?.Invoke(this, _items[_hovered]);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            int prev = _hovered;
            _hovered = -1;
            InvalidateRow(prev);
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            if (e.Button != MouseButtons.Left) return;
            int idx = HitTest(e.Y);
            if (idx < 0 || idx >= _items.Count) return;
            _selected = idx;
            Invalidate();
            ItemClicked?.Invoke(this, _items[idx]);
        }

        // ══════════════════════════════════════════════════════════════
        // HELPERS PRIVADOS
        // ══════════════════════════════════════════════════════════════

        /// <summary>
        /// Retorna el índice del ítem bajo la coordenada Y del panel
        /// (ya compensando el scroll interno).
        /// </summary>
        private int HitTest(int panelY)
        {
            int scrollY = -AutoScrollPosition.Y;
            int virtualY = panelY + scrollY;
            int idx = virtualY / _itemHeight;
            return (idx >= 0 && idx < _items.Count) ? idx : -1;
        }

        /// <summary>
        /// Invalida solo la franja de un ítem, evitando repintar todo el panel.
        /// </summary>
        private void InvalidateRow(int index)
        {
            if (index < 0 || index >= _items.Count) return;
            int scrollY = -AutoScrollPosition.Y;
            int y = index * _itemHeight - scrollY;
            Invalidate(new Rectangle(0, y, Width, _itemHeight));
        }

        /// <summary>
        /// Hace scroll para que el ítem en <paramref name="index"/> sea visible.
        /// </summary>
        private void EnsureVisible(int index)
        {
            if (index < 0 || index >= _items.Count) return;
            int itemTop = index * _itemHeight;
            int itemBottom = itemTop + _itemHeight;
            int scrollY = -AutoScrollPosition.Y;
            int viewBottom = scrollY + Height;

            if (itemTop < scrollY)
                AutoScrollPosition = new Point(0, itemTop);
            else if (itemBottom > viewBottom)
                AutoScrollPosition = new Point(0, itemBottom - Height);
        }

        // ══════════════════════════════════════════════════════════════
        // CACHÉ DE ÍCONOS
        // ══════════════════════════════════════════════════════════════

        private Bitmap GetCachedIcon(IconChar icon, Color color, int size)
        {
            var key = (icon, color, size);
            if (!_iconCache.TryGetValue(key, out var bmp))
            {
                try { bmp = icon.ToBitmap(color, size); }
                catch { bmp = null; }
                _iconCache[key] = bmp;
            }
            return bmp;
        }

        // ══════════════════════════════════════════════════════════════
        // DISPOSE
        // ══════════════════════════════════════════════════════════════

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var bmp in _iconCache.Values)
                    bmp?.Dispose();
                _iconCache.Clear();
            }
            base.Dispose(disposing);
        }
    }
}