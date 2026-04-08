using FontAwesome.Sharp;
using Procont.Utils.Components.ComboSearch;
using Procont.Utils.Core.Models;
using Procont.Utils.Core.Theming;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Procont.DevSandbox.Pages
{
    partial class ComboSearchPage
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            BackColor = ProcontTheme.SurfaceDark;
            Dock = DockStyle.Fill;
            Padding = new Padding(32);

            // ── Layout principal ───────────────────────────────────────
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 6,
                BackColor = ProcontTheme.SurfaceDark,
                Padding = Padding.Empty
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 320));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (int i = 0; i < 6; i++)
                layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 80));

            // ── Helper: etiqueta de sección ───────────────────────────
            Label SectionLabel(string text, int row)
            {
                var lbl = new Label
                {
                    Text = text,
                    ForeColor = ProcontTheme.TextSubdued,
                    Font = ProcontTheme.FontSmall,
                    AutoSize = true,
                    Dock = DockStyle.Bottom,
                    Padding = new Padding(0, 0, 0, 4)
                };
                layout.Controls.Add(lbl, 0, row);
                return lbl;
            }

            // ── Log panel (columna derecha) ───────────────────────────
            var logBox = new RichTextBox
            {
                Dock = DockStyle.Fill,
                BackColor = ProcontTheme.SurfaceActive,
                ForeColor = ProcontTheme.TextPrimary,
                Font = ProcontTheme.FontSmall,
                BorderStyle = BorderStyle.None,
                ReadOnly = true,
                Margin = new Padding(16, 0, 0, 0)
            };
            layout.Controls.Add(logBox, 1, 0);
            layout.SetRowSpan(logBox, 6);

            void Log(string msg)
            {
                logBox.AppendText(msg + "\n");
                logBox.ScrollToCaret();
            }

            // ══════════════════════════════════════════════════════════
            // CASO 1 — List<BindableItem> con íconos
            // ══════════════════════════════════════════════════════════
            SectionLabel("1. BindableItem con íconos", 0);

            var combo1 = new ComboSearchBox
            {
                Width = 280,
                Anchor = AnchorStyles.Left | AnchorStyles.Top,
                PlaceholderText = "Buscar cliente...",
                ActionLabel = "+ Nuevo cliente",
                ActionIcon = IconChar.UserPlus,
                EmptyStateText = "No encontrado. Usá {action}",
                Margin = new Padding(0, 36, 0, 0)
            };

            combo1.DataSource = new List<BindableItem>
            {
                new BindableItem("Acme Corp",          1, IconChar.Building),
                new BindableItem("Beta Solutions",     2, IconChar.Laptop),
                new BindableItem("Contoso Ltd",        3, IconChar.Briefcase),
                new BindableItem("Delta Industries",   4, IconChar.Industry),
                new BindableItem("Echo Ventures",      5, IconChar.Rocket),
                new BindableItem("Foxtrot & Asociados",6, IconChar.Handshake),
                new BindableItem("Global Traders",     7, IconChar.Globe),
                new BindableItem("Horizon Capital",    8, IconChar.ChartLine),
                new BindableItem("Innovatech SA",      9, IconChar.Lightbulb),
                new BindableItem("Jupiter Holdings",  10, IconChar.Star),
                new BindableItem("Kappa Group",       11, IconChar.Users),
            };

            combo1.SelectionCommitted += (s, e) => Log($"[combo1] Committed: {e.DisplayText} → value={e.Value}");
            combo1.SelectionCleared += (s, e) => Log($"[combo1] Cleared");
            combo1.ActionButtonClicked += (s, e) => Log($"[combo1] Action clicked. SearchText='{e.SearchText}'");

            layout.Controls.Add(combo1, 0, 0);

            // ══════════════════════════════════════════════════════════
            // CASO 2 — Tipo personalizado con DisplayMember / ValueMember
            // ══════════════════════════════════════════════════════════
            SectionLabel("2. Tipo personalizado (DisplayMember / ValueMember)", 1);

            var combo2 = new ComboSearchBox
            {
                Width = 280,
                Anchor = AnchorStyles.Left | AnchorStyles.Top,
                PlaceholderText = "Buscar producto...",
                DisplayMember = "Nombre",
                ValueMember = "Codigo",
                ActionLabel = "+ Agregar producto",
                ActionIcon = IconChar.PlusCircle,
                Margin = new Padding(0, 36, 0, 0)
            };

            combo2.DataSource = new List<Producto>
            {
                new Producto { Nombre = "Laptop HP 14",       Codigo = "LP-001", Subtitle = "S/ 2,800" },
                new Producto { Nombre = "Mouse Logitech M705",Codigo = "MS-002", Subtitle = "S/ 120"   },
                new Producto { Nombre = "Monitor LG 24\"",    Codigo = "MN-003", Subtitle = "S/ 750"   },
                new Producto { Nombre = "Teclado Mecánico",   Codigo = "TC-004", Subtitle = "S/ 280"   },
                new Producto { Nombre = "Webcam Logitech C920",Codigo="WC-005", Subtitle = "S/ 340"   },
            };

            combo2.SelectionCommitted += (s, e) => Log($"[combo2] {e.DisplayText} → codigo={e.Value}");
            combo2.ActionButtonClicked += (s, e) => Log($"[combo2] Action: nuevo producto '{e.SearchText}'");

            layout.Controls.Add(combo2, 0, 1);

            // ══════════════════════════════════════════════════════════
            // CASO 3 — SearchMode StartsWith
            // ══════════════════════════════════════════════════════════
            SectionLabel("3. SearchMode = StartsWith", 2);

            var combo3 = new ComboSearchBox
            {
                Width = 280,
                Anchor = AnchorStyles.Left | AnchorStyles.Top,
                PlaceholderText = "Buscar país (StartsWith)...",
                SearchMode = ComboSearchMode.StartsWith,
                ActionLabel = "Ver todos los países",
                ActionIcon = IconChar.Globe,
                Margin = new Padding(0, 36, 0, 0)
            };

            combo3.DataSource = new List<BindableItem>
            {
                new BindableItem("Argentina",  "AR"),
                new BindableItem("Bolivia",    "BO"),
                new BindableItem("Brasil",     "BR"),
                new BindableItem("Chile",      "CL"),
                new BindableItem("Colombia",   "CO"),
                new BindableItem("Ecuador",    "EC"),
                new BindableItem("Paraguay",   "PY"),
                new BindableItem("Perú",       "PE"),
                new BindableItem("Uruguay",    "UY"),
                new BindableItem("Venezuela",  "VE"),
            };

            combo3.SelectionCommitted += (s, e) => Log($"[combo3] País: {e.DisplayText} ({e.Value})");
            combo3.ActionButtonClicked += (s, e) => Log($"[combo3] Ver todos. Filtro='{e.SearchText}'");

            layout.Controls.Add(combo3, 0, 2);

            Controls.Add(layout);
        }

        // ── Modelo de prueba ───────────────────────────────────────────
        private class Producto
        {
            public string Nombre { get; set; }
            public string Codigo { get; set; }
            public string Subtitle { get; set; }   // se detecta automáticamente
        }
    }
}