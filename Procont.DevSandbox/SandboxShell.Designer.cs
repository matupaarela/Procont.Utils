using Procont.Utils.Core.Theming;
using System.Drawing;
using System.Windows.Forms;

namespace Procont.DevSandbox
{
    partial class SandboxShell
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();

            // ── Ventana ────────────────────────────────────────────────
            Text = "Procont.Utils — DevSandbox";
            Size = new Size(1200, 720);
            MinimumSize = new Size(900, 600);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = ProcontTheme.SurfaceDark;
            ForeColor = ProcontTheme.TextPrimary;

            // ── Nav (panel izquierdo) ──────────────────────────────────
            var navPanel = new Panel
            {
                Dock = DockStyle.Left,
                Width = 180,
                BackColor = ProcontTheme.SurfaceActive,
                Padding = new Padding(0, 8, 0, 0)
            };

            var navTitle = new Label
            {
                Text = "COMPONENTES",
                Dock = DockStyle.Top,
                Height = 32,
                ForeColor = ProcontTheme.TextSubdued,
                Font = ProcontTheme.FontSmallBold,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
                Padding = new Padding(14, 0, 0, 0)
            };

            _nav = new ListBox
            {
                Dock = DockStyle.Fill,
                BackColor = ProcontTheme.SurfaceActive,
                ForeColor = ProcontTheme.TextPrimary,
                BorderStyle = BorderStyle.None,
                Font = ProcontTheme.FontBase,
                ItemHeight = 34,
                DrawMode = DrawMode.OwnerDrawFixed,
                SelectionMode = SelectionMode.One
            };
            _nav.DrawItem += Nav_DrawItem;
            _nav.SelectedIndexChanged += Nav_SelectedIndexChanged;

            navPanel.Controls.Add(_nav);
            navPanel.Controls.Add(navTitle);

            // ── Content area ───────────────────────────────────────────
            _contentArea = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ProcontTheme.SurfaceDark,
                Padding = new Padding(24)
            };

            // ── Splitter ───────────────────────────────────────────────
            var splitter = new Panel
            {
                Dock = DockStyle.Left,
                Width = 1,
                BackColor = ProcontTheme.BorderDefault
            };

            Controls.Add(_contentArea);
            Controls.Add(splitter);
            Controls.Add(navPanel);

            Load += SandboxShell_Load;
        }

        private void Nav_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;
            bool selected = (e.State & DrawItemState.Selected) != 0;

            var bg = selected ? ProcontTheme.SurfaceHover : ProcontTheme.SurfaceActive;
            e.Graphics.FillRectangle(new System.Drawing.SolidBrush(bg), e.Bounds);

            if (selected)
            {
                e.Graphics.FillRectangle(
                    new System.Drawing.SolidBrush(ProcontTheme.TextAccent),
                    new Rectangle(0, e.Bounds.Y + 6, 3, e.Bounds.Height - 12));
            }

            var text = _nav.Items[e.Index].ToString();
            using (var b = new System.Drawing.SolidBrush(
                selected ? ProcontTheme.TextAccent : ProcontTheme.TextPrimary))
            {
                var fmt = new System.Drawing.StringFormat
                {
                    LineAlignment = System.Drawing.StringAlignment.Center
                };
                e.Graphics.DrawString(text, ProcontTheme.FontBase, b,
                    new RectangleF(16, e.Bounds.Y, e.Bounds.Width - 16, e.Bounds.Height), fmt);
            }
        }
    }
}