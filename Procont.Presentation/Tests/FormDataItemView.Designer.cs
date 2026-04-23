namespace Procont.Presentation.Tests
{
    partial class FormDataItemView
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormDataItemView));
            this.dataItemView1 = new Procont.Utils.Components.DataItem.DataItemView();
            ((System.ComponentModel.ISupportInitialize)(this.dataItemView1)).BeginInit();
            this.SuspendLayout();
            // 
            // dataItemView1
            // 
            this.dataItemView1.AutoScroll = true;
            this.dataItemView1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(3)))), ((int)(((byte)(39)))), ((int)(((byte)(59)))));
            this.dataItemView1.Items.Add(((Procont.Utils.Components.DataItem.Models.DataItemModel)(resources.GetObject("dataItemView1.Items"))));
            this.dataItemView1.Location = new System.Drawing.Point(31, 77);
            this.dataItemView1.Name = "dataItemView1";
            this.dataItemView1.Size = new System.Drawing.Size(713, 273);
            this.dataItemView1.TabIndex = 0;
            this.dataItemView1.Text = "dataItemView1";
            // 
            // FormDataItemView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.dataItemView1);
            this.Name = "FormDataItemView";
            this.Text = "FormDataItemView";
            this.Load += new System.EventHandler(this.FormDataItemView_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataItemView1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Utils.Components.DataItem.DataItemView dataItemView1;
    }
}