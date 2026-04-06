using FontAwesome.Sharp;
using Procont.Utils.Components.Sidebar;
using Procont.Utils.Components.Sidebar.Models;
using Procont.Utils.Core.Theming;
using System.Windows.Forms;

namespace Procont.DevSandbox.Pages
{
    partial class SidebarPage
    {
        /// <summary> 
        /// Variable del diseñador necesaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Limpiar los recursos que se estén usando.
        /// </summary>
        /// <param name="disposing">true si los recursos administrados se deben desechar; false en caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de componentes

        /// <summary> 
        /// Método necesario para admitir el Diseñador. No se puede modificar
        /// el contenido de este método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            BackColor = ProcontTheme.SurfaceDark;
            Dock = DockStyle.Fill;

            var sidebar = new SidebarControl { Dock = DockStyle.Left };
            sidebar.SetCompanyInfo("PROCONT", "Procont Business S.A.C.", "20602111602", "DevSandbox");

            var grp = sidebar.AddGroup("VENTAS", icon: IconChar.DollarSign, expanded: true);
            grp.AddItem("Comprobantes", icon: IconChar.FileInvoice);
            grp.AddItem("Clientes", icon: IconChar.Users);
            grp.AddItem("Reportes", icon: IconChar.ChartBar, badge: SidebarBadge.New);

            var grp2 = sidebar.AddGroup("COMPRAS", icon: IconChar.ShoppingCart);
            grp2.AddItem("Proveedores", icon: IconChar.Truck);
            grp2.AddItem("Órdenes", icon: IconChar.ClipboardList, badge: SidebarBadge.Beta);

            sidebar.AddRootItem("FINANCIERO", icon: IconChar.Hourglass1, badge: SidebarBadge.Beta);

            // Info panel
            var infoPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ProcontTheme.SurfaceDark,
                Padding = new Padding(24)
            };

            var lbl = new Label
            {
                Text = "Sidebar — seleccioná un ítem",
                ForeColor = ProcontTheme.TextSubdued,
                Font = ProcontTheme.FontBase,
                AutoSize = true,
                Location = new System.Drawing.Point(24, 24)
            };
            infoPanel.Controls.Add(lbl);

            sidebar.ItemSelected += (s, item) =>
            {
                if (item == null) { lbl.Text = "Dashboard seleccionado"; return; }
                lbl.Text = $"Seleccionado: {item.BreadcrumbPath}\nKey: {item.Key}";
            };

            Controls.Add(infoPanel);
            Controls.Add(sidebar);

        }

        #endregion
    }
}
