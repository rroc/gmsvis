namespace Treemap
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
            this.treemapPanel = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // treemapPanel
            // 
            this.treemapPanel.BackColor = System.Drawing.Color.Thistle;
            this.treemapPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treemapPanel.Location = new System.Drawing.Point(0, 0);
            this.treemapPanel.Name = "treemapPanel";
            this.treemapPanel.Size = new System.Drawing.Size(637, 440);
            this.treemapPanel.TabIndex = 0;
            this.treemapPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.treemapPanel_Paint);
            this.treemapPanel.SizeChanged += new System.EventHandler(this.treemapPanel_SizeChanged);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(637, 440);
            this.Controls.Add(this.treemapPanel);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel treemapPanel;

    }
}

