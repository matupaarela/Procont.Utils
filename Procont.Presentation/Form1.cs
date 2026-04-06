using FontAwesome.Sharp;
using System;
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

        }

        private void button1_Click(object sender, EventArgs e)
        {
            sidebarControl1.SetVisible("sHidden", false);
        }
    }
}
