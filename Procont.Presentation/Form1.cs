using FontAwesome.Sharp;
using Procont.Utils.Core.Models;
using System;
using System.Collections.Generic;
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
