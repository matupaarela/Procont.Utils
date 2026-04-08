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
    /// Soporta texto principal, subtítulo opcional e ícono FontAwesome.
    /// </summary>
    [ToolboxItem(false)]
    [DesignTimeVisible(false)]
    internal class ComboSearchItemControl : Control
    {
        // ── Estado ────────────────────────────────────────────────────
        private bool _isHovered = false;
        private bool _isSelected = false;

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

            // Fondo
            Color bg = _isSelected ? ComboSearchTheme.ItemSelected :
                       _isHovered ? ComboSearchTheme.ItemHover :
                                     ComboSearchTheme.ItemBackground;
            g.Clear(bg);

            // Barra lateral izquierda cuando está seleccionado
            if (_isSelected)
            {
                using (var b = new SolidBrush(ComboSearchTheme.ActionText))
                    g.FillRectangle(b, 0, 5, 3, Height - 10);
            }

            int x = ComboSearchTheme.PaddingH;

            // Ícono
            if (Icon != IconChar.None)
            {
                int iconSize = ComboSearchTheme.IconSize;
                int iconY = (Height - iconSize) / 2;
                Color iconColor = _isSelected
                    ? ComboSearchTheme.ItemIconActive
                    : ComboSearchTheme.ItemIconColor;
                using (var bmp = Icon.ToBitmap(iconColor, iconSize))
                    g.DrawImage(bmp, x, iconY, iconSize, iconSize);
                x += iconSize + 8;
            }

            // Texto principal + subtítulo
            bool hasSubtitle = !string.IsNullOrEmpty(Subtitle);
            Color textColor = ComboSearchTheme.ItemText;

            if (hasSubtitle)
            {
                // Dos líneas
                int mainY = Height / 2 - 10;
                int subY = Height / 2 + 1;

                using (var b = new SolidBrush(textColor))
                    g.DrawString(DisplayText, ComboSearchTheme.FontItem, b,
                        new RectangleF(x, mainY, Width - x - ComboSearchTheme.PaddingH, 14),
                        new StringFormat { Trimming = StringTrimming.EllipsisCharacter });

                using (var b = new SolidBrush(ComboSearchTheme.ItemSubtitle))
                    g.DrawString(Subtitle, ComboSearchTheme.FontSubtitle, b,
                        new RectangleF(x, subY, Width - x - ComboSearchTheme.PaddingH, 12),
                        new StringFormat { Trimming = StringTrimming.EllipsisCharacter });
            }
            else
            {
                // Una sola línea centrada
                using (var b = new SolidBrush(textColor))
                using (var fmt = new StringFormat
                {
                    LineAlignment = StringAlignment.Center,
                    Trimming = StringTrimming.EllipsisCharacter
                })
                    g.DrawString(DisplayText, ComboSearchTheme.FontItem, b,
                        new RectangleF(x, 0, Width - x - ComboSearchTheme.PaddingH, Height), fmt);
            }
        }
    }
}