using FontAwesome.Sharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Procont.Presentation
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void sidebarControl1_ItemSelected(object sender, Utils.Sidebar.SidebarMenuItemControl item)
        {
            if (item == null) return;

            // 1. Breadcrumb
            label1.Text = sidebarControl1.SelectedBreadcrumb;
            // → "COMPROBANTES SEE · GUÍAS DE REMISIÓN · REMITENTE"

            // 2. Ícono resuelto
            IconChar icono = sidebarControl1.SelectedIcon;
            iconPictureBox1.IconChar = icono;
        }
    }
}
