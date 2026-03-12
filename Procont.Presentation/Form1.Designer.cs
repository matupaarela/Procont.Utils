namespace Procont.Presentation
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            Procont.Utils.Sidebar.Models.SidebarGroupModel sidebarGroupModel1 = new Procont.Utils.Sidebar.Models.SidebarGroupModel();
            Procont.Utils.Sidebar.Models.SidebarItemModel sidebarItemModel1 = new Procont.Utils.Sidebar.Models.SidebarItemModel();
            Procont.Utils.Sidebar.Models.SidebarItemModel sidebarItemModel2 = new Procont.Utils.Sidebar.Models.SidebarItemModel();
            Procont.Utils.Sidebar.Models.SidebarGroupModel sidebarGroupModel2 = new Procont.Utils.Sidebar.Models.SidebarGroupModel();
            Procont.Utils.Sidebar.Models.SidebarItemModel sidebarItemModel3 = new Procont.Utils.Sidebar.Models.SidebarItemModel();
            Procont.Utils.Sidebar.Models.SidebarItemModel sidebarItemModel4 = new Procont.Utils.Sidebar.Models.SidebarItemModel();
            Procont.Utils.Sidebar.Models.SidebarItemModel sidebarItemModel5 = new Procont.Utils.Sidebar.Models.SidebarItemModel();
            Procont.Utils.Sidebar.Models.SidebarItemModel sidebarItemModel6 = new Procont.Utils.Sidebar.Models.SidebarItemModel();
            Procont.Utils.Sidebar.Models.SidebarGroupModel sidebarGroupModel3 = new Procont.Utils.Sidebar.Models.SidebarGroupModel();
            Procont.Utils.Sidebar.Models.SidebarItemModel sidebarItemModel7 = new Procont.Utils.Sidebar.Models.SidebarItemModel();
            Procont.Utils.Sidebar.Models.SidebarItemModel sidebarItemModel8 = new Procont.Utils.Sidebar.Models.SidebarItemModel();
            this.sidebarControl1 = new Procont.Utils.Sidebar.SidebarControl();
            ((System.ComponentModel.ISupportInitialize)(this.sidebarControl1)).BeginInit();
            this.SuspendLayout();
            // 
            // sidebarControl1
            // 
            this.sidebarControl1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(3)))), ((int)(((byte)(39)))), ((int)(((byte)(59)))));
            this.sidebarControl1.Dock = System.Windows.Forms.DockStyle.Left;
            sidebarGroupModel2.Children.Add(sidebarItemModel3);
            sidebarGroupModel2.Children.Add(sidebarItemModel4);
            sidebarGroupModel2.Children.Add(sidebarItemModel5);
            sidebarGroupModel2.Children.Add(sidebarItemModel6);
            sidebarGroupModel1.Children.Add(sidebarItemModel1);
            sidebarGroupModel1.Children.Add(sidebarItemModel2);
            sidebarGroupModel1.Children.Add(sidebarGroupModel2);
            sidebarGroupModel3.Children.Add(sidebarItemModel7);
            sidebarGroupModel3.Children.Add(sidebarItemModel8);
            this.sidebarControl1.Groups.Add(sidebarGroupModel1);
            this.sidebarControl1.Groups.Add(sidebarGroupModel3);
            this.sidebarControl1.Location = new System.Drawing.Point(0, 0);
            this.sidebarControl1.Name = "sidebarControl1";
            this.sidebarControl1.Size = new System.Drawing.Size(260, 450);
            this.sidebarControl1.TabIndex = 0;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.sidebarControl1);
            this.Name = "Form1";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.sidebarControl1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Utils.Sidebar.SidebarControl sidebarControl1;
    }
}