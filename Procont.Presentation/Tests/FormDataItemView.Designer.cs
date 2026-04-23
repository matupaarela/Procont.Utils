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
            this.actionButton1 = new Procont.Utils.Components.DataItem.ActionButton();
            this.SuspendLayout();
            // 
            // actionButton1
            // 
            this.actionButton1.BackColor = System.Drawing.Color.Transparent;
            this.actionButton1.ButtonIcon = FontAwesome.Sharp.IconChar.Add;
            this.actionButton1.ButtonText = "asdasd";
            this.actionButton1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.actionButton1.IsSplit = true;
            this.actionButton1.Location = new System.Drawing.Point(187, 92);
            this.actionButton1.Name = "actionButton1";
            this.actionButton1.Size = new System.Drawing.Size(96, 26);
            this.actionButton1.TabIndex = 0;
            this.actionButton1.Text = "actionButton1";
            // 
            // FormDataItemView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.actionButton1);
            this.Name = "FormDataItemView";
            this.Text = "FormDataItemView";
            this.ResumeLayout(false);

        }

        #endregion

        private Utils.Components.DataItem.ActionButton actionButton1;
    }
}