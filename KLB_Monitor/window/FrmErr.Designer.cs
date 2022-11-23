namespace KLB_Monitor.window
{
    partial class FrmErr
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
            this.tipLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // tipLabel
            // 
            this.tipLabel.BackColor = System.Drawing.Color.Transparent;
            this.tipLabel.Font = new System.Drawing.Font("宋体", 36F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.tipLabel.ForeColor = System.Drawing.Color.White;
            this.tipLabel.Location = new System.Drawing.Point(8, 60);
            this.tipLabel.Name = "tipLabel";
            this.tipLabel.Size = new System.Drawing.Size(780, 50);
            this.tipLabel.TabIndex = 0;
            this.tipLabel.Text = "XXX";
            this.tipLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.tipLabel.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.tipLabel_MouseDoubleClick);
            // 
            // FrmErr
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = global::KLB_Monitor.Properties.Resources.修复中;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(800, 386);
            this.Controls.Add(this.tipLabel);
            this.DoubleBuffered = true;
            this.Name = "FrmErr";
            this.Text = "预警提示";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FrmErr_FormClosing);
            this.Load += new System.EventHandler(this.FrmErr_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private Label tipLabel;
    }
}