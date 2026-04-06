using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Procont.Utils.Sidebar
{
    [ToolboxItem(false)]
    [DesignTimeVisible(false)]
    public class SidebarMenuSeparatorControl : Control
    {
        private string _label = "";

        public string Label
        {
            get => _label;
            set { _label = value ?? ""; Height = GetNaturalHeight(); Invalidate(); }
        }

        public SidebarMenuSeparatorControl(string label = "")
        {
            _label = label ?? "";
            Dock = DockStyle.Top;
            Height = GetNaturalHeight();
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint, true);
        }

        private int GetNaturalHeight() =>
            string.IsNullOrEmpty(_label) ? 13 : 22;

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.Clear(SidebarTheme.BackgroundDark);

            if (string.IsNullOrEmpty(_label))
            {
                int y = Height / 2;
                using (var pen = new Pen(SidebarTheme.BorderColor, 1))
                    g.DrawLine(pen, 14, y, Width - 14, y);
            }
            else
            {
                var textSize = g.MeasureString(_label, SidebarTheme.FontBadge);
                int textW = (int)textSize.Width + 2;
                int cx = Width / 2;
                int y = Height / 2;

                using (var pen = new Pen(SidebarTheme.BorderColor, 1))
                {
                    g.DrawLine(pen, 14, y, cx - textW / 2 - 5, y);
                    g.DrawLine(pen, cx + textW / 2 + 5, y, Width - 14, y);
                }

                using (var b = new SolidBrush(SidebarTheme.TextSubdued))
                {
                    var fmt = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center
                    };
                    g.DrawString(_label, SidebarTheme.FontBadge, b,
                        new RectangleF(cx - textW / 2, 0, textW, Height), fmt);
                }
            }
        }
    }
}
