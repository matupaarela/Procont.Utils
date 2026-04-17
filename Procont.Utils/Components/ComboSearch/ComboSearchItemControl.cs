using FontAwesome.Sharp;
using Procont.Utils.Core.Extensions;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Procont.Utils.Components.ComboSearch
{
    /// <summary>
    /// Control de un ítem individual dentro del dropdown del ComboSearchBox.
    /// Soporta texto principal, subtítulo opcional, ícono FontAwesome
    /// y modo multi-selección con checkbox integrado.
    /// </summary>
    [ToolboxItem(false)]
    [DesignTimeVisible(false)]
    internal class ComboSearchItemControl : Control
    {
        // ── Estado ────────────────────────────────────────────────────
        private bool _isHovered = false;
        private bool _isSelected = false;
        private bool _isChecked = false;
        private bool _multiSelect = false;

        // ── Datos del ítem ────────────────────────────────────────────
        public object DataItem { get; }
        public string DisplayText { get; }
        public string Subtitle { get; }
        public IconChar Icon { get; }
        public object Value { get; }
        public int ItemIndex { get; }

        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; Invalidate(); }
        }

        /// <summary>
        /// Activa el modo multi-selección: muestra un checkbox a la izquierda
        /// de cada ítem y el clic alterna el estado marcado sin cerrar el dropdown.
        /// </summary>
        public bool MultiSelect
        {
            get => _multiSelect;
            set { _multiSelect = value; Invalidate(); }
        }

        /// <summary>Estado del checkbox (solo relevante cuando MultiSelect = true).</summary>
        public bool IsChecked
        {
            get => _isChecked;
            set { _isChecked = value; Invalidate(); }
        }

        // ── Evento ────────────────────────────────────────────────────
        public event EventHandler ItemClicked;

        // ── Constructor ───────────────────────────────────────────────
        public ComboSearchItemControl(
            object dataItem,
            string displayText,
            string subtitle,
            IconChar icon,
            object value,
            int itemIndex)
        {
            DataItem = dataItem;
            DisplayText = displayText ?? "";
            Subtitle = subtitle ?? "";
            Icon = icon;
            Value = value;
            ItemIndex = itemIndex;

            Height = ComboSearchTheme.ItemHeight;
            Dock = DockStyle.Top;
            Cursor = Cursors.Hand;

            SetStyle(
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint, true);
        }

        // ── Mouse ─────────────────────────────────────────────────────
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
            ItemClicked?.Invoke(this, EventArgs.Empty);
        }

        // ── Pintura ───────────────────────────────────────────────────
        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SetHighQuality();

            // ── Fondo ─────────────────────────────────────────────────
            Color bg = _isSelected && !_multiSelect ? ComboSearchTheme.ItemSelected :
                       _isHovered ? ComboSearchTheme.ItemHover :
                                                       ComboSearchTheme.ItemBackground;
            g.Clear(bg);

            // Barra lateral izquierda (solo selección simple)
            if (_isSelected && !_multiSelect)
            {
                using (var b = new SolidBrush(ComboSearchTheme.ActionText))
                    g.FillRectangle(b, 0, 5, 3, Height - 10);
            }

            int x = ComboSearchTheme.PaddingH;

            // ── Checkbox (multi-select) ────────────────────────────────
            if (_multiSelect)
            {
                const int cbSize = 14;
                int cbY = (Height - cbSize) / 2;
                DrawCheckbox(g, x, cbY, cbSize);
                x += cbSize + 8;
            }

            // ── Ícono ──────────────────────────────────────────────────
            if (Icon != IconChar.None)
            {
                int iconSize = ComboSearchTheme.IconSize;
                int iconY = (Height - iconSize) / 2;
                Color iconColor = (_isSelected && !_multiSelect)
                    ? ComboSearchTheme.ItemIconActive
                    : ComboSearchTheme.ItemIconColor;
                using (var bmp = Icon.ToBitmap(iconColor, iconSize))
                    g.DrawImage(bmp, x, iconY, iconSize, iconSize);
                x += iconSize + 8;
            }

            // ── Texto principal + subtítulo ────────────────────────────
            int textW = Width - x - ComboSearchTheme.PaddingH;
            bool hasSubtitle = !string.IsNullOrEmpty(Subtitle);
            Color textColor = ComboSearchTheme.ItemText;

            if (hasSubtitle)
            {
                int mainY = Height / 2 - 10;
                int subY = Height / 2 + 1;

                using (var b = new SolidBrush(textColor))
                    g.DrawString(DisplayText, ComboSearchTheme.FontItem, b,
                        new RectangleF(x, mainY, textW, 14),
                        ComboSearchRenderer.FmtLeftTop);

                using (var b = new SolidBrush(ComboSearchTheme.ItemSubtitle))
                    g.DrawString(Subtitle, ComboSearchTheme.FontSubtitle, b,
                        new RectangleF(x, subY, textW, 12),
                        ComboSearchRenderer.FmtLeftTop);
            }
            else
            {
                using (var b = new SolidBrush(textColor))
                    g.DrawString(DisplayText, ComboSearchTheme.FontItem, b,
                        new RectangleF(x, 0, textW, Height),
                        ComboSearchRenderer.FmtLeft);
            }
        }

        // ── Checkbox rendering ────────────────────────────────────────
        /// <summary>
        /// Dibuja un checkbox a la posición (x, y) con el tamaño indicado.
        /// Marcado: fondo highlight + tilde blanco.
        /// Desmarcado: fondo blanco + borde gris.
        /// </summary>
        private void DrawCheckbox(Graphics g, int x, int y, int size)
        {
            var rect = new Rectangle(x, y, size, size);

            if (_isChecked)
            {
                // Fondo sólido (usa el mismo color que ítems seleccionados)
                g.FillRectangle(ComboSearchRenderer.BrushSelected, rect);

                // Tilde blanca — dos segmentos formando √
                using (var pen = new Pen(Color.White, 1.5f))
                {
                    pen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
                    pen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
                    // Segmento corto: esquina inferior-izquierda → centro-bajo
                    g.DrawLine(pen, x + 2, y + size / 2,
                                   x + size / 2 - 1, y + size - 3);
                    // Segmento largo: centro-bajo → esquina superior-derecha
                    g.DrawLine(pen, x + size / 2 - 1, y + size - 3,
                                   x + size - 2, y + 2);
                }
            }
            else
            {
                // Fondo blanco + borde gris
                using (var fill = new SolidBrush(ComboSearchTheme.ItemBackground))
                    g.FillRectangle(fill, rect);
                g.DrawRectangle(ComboSearchRenderer.PenSeparator, rect);
            }
        }
    }
}