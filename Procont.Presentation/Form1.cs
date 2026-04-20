using FontAwesome.Sharp;
using Procont.Utils.Components.DataGrid;
using Procont.Utils.Core.Models;
using Procont.Utils.Core.Theming;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Procont.Presentation
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void sidebarControl1_ItemSelected(object sender, Utils.Components.Sidebar.SidebarMenuItemControl item)
        {
            if (item == null) return;

            // 1. Breadcrumb
            label1.Text = sidebarControl1.SelectedBreadcrumb;
            // → "COMPROBANTES SEE · GUÍAS DE REMISIÓN · REMITENTE"

            // 2. Ícono resuelto
            IconChar icono = sidebarControl1.SelectedIcon;
            iconPictureBox1.IconChar = icono;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            CboTest.DataSource = new List<BindableItem>
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

            SetupDgv();
        }


        private void SetupDgv()
        {
            var grid = new ProcontDataGridView
            {
                AutoGenerateColumns = false,
                Dock = DockStyle.Fill
            };

            // ── Columnas ──────────────────────────────────────────────────────
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Descripcion", HeaderText = "Descripción", Width = 200 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Enero", HeaderText = "Enero", Width = 90 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Febrero", HeaderText = "Febrero", Width = 90 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Marzo", HeaderText = "Marzo", Width = 90 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Total", HeaderText = "Total", Width = 100 });

            // ── Grupo de columnas ─────────────────────────────────────────────
            // Usar ColumnNamesText (string CSV), NO la sintaxis { "a", "b" } del designer
            grid.ColumnGroups.Add(new DataGridColumnGroup
            {
                Title = "Período anual",
                //BackColor = Color.FromArgb(15, 245, 168, 30),  // tinte ámbar sutil
                ColumnNamesText = "Enero, Febrero, Marzo"      // ← nombres exactos de columna
            });
            grid.ApplyColumnGroups();  // explícito tras agregar grupos en Load

            // ── Footer integrado ──────────────────────────────────────────────
            grid.FooterRows.Add(new DataGridFooterRow
            {
                Cells = {
                    new DataGridFooterCell
                    {
                        ColumnName = "Descripcion",
                        Formula    = FooterFormula.Custom,
                        Text       = "TOTAL",
                        Alignment  = ContentAlignment.MiddleLeft
                    },
                    new DataGridFooterCell
                    {
                        ColumnName = "Enero",
                        Formula    = FooterFormula.Sum,
                        Format     = "N2"
                    },
                    new DataGridFooterCell
                    {
                        ColumnName = "Febrero",
                        Formula    = FooterFormula.Sum,
                        Format     = "N2"
                    },
                    new DataGridFooterCell
                    {
                        ColumnName = "Marzo",
                        Formula    = FooterFormula.Sum,
                        Format     = "N2"
                    },
                    new DataGridFooterCell
                    {
                        ColumnName = "Total",
                        Formula    = FooterFormula.Sum,
                        Format     = "N2",
                        ForeColor  = ProcontTheme.TextAccent
                    }
                }
            });
            grid.RebuildFooter();  // ← reserva Padding.Bottom y activa pintado

            // ── Datos de prueba ───────────────────────────────────────────────
            grid.Rows.Add("Producto A", 1200.50, 1350.75, 980.00, 3531.25);
            grid.Rows.Add("Producto B", 850.00, 920.00, 1100.50, 2870.50);
            grid.Rows.Add("Producto C", 600.25, 700.00, 650.75, 1951.00);

            // ── Agregar al panel ──────────────────────────────────────────────
            // El footer se pinta DENTRO del grid — no necesitas añadir ningún control extra
            flowLayoutPanel1.Controls.Add(grid);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            sidebarControl1.SetVisible("sHidden", false);
        }

        private void CboTest_MultiSelectionChanged(object sender, Utils.Components.ComboSearch.MultiSelectionChangedEventArgs e)
        {
            TbResultTest.Text = $"Selected {e.Count} items:\r\n" + string.Join("\r\n", e.SelectedDisplayTexts);
        }
    }
}
