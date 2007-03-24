namespace GMS
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.groupingComboBox = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.helpButton = new System.Windows.Forms.ToolStripButton();
            this.treeMapComboBox = new System.Windows.Forms.ToolStripComboBox();
            this.mainSplitContainer = new System.Windows.Forms.SplitContainer();
            this.PCMapSplitContainer = new System.Windows.Forms.SplitContainer();
            this.glyphPanel = new System.Windows.Forms.Panel();
            this.glyphCheckBoxes = new System.Windows.Forms.CheckedListBox();
            this.glyph1picture = new System.Windows.Forms.PictureBox();
            this.pictureBox3 = new System.Windows.Forms.PictureBox();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.toolStrip1.SuspendLayout();
            this.mainSplitContainer.Panel1.SuspendLayout();
            this.mainSplitContainer.SuspendLayout();
            this.PCMapSplitContainer.Panel1.SuspendLayout();
            this.PCMapSplitContainer.Panel2.SuspendLayout();
            this.PCMapSplitContainer.SuspendLayout();
            this.glyphPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.glyph1picture)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel1,
            this.groupingComboBox,
            this.toolStripSeparator1,
            this.helpButton,
            this.treeMapComboBox});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(540, 25);
            this.toolStrip1.Size = new System.Drawing.Size(692, 25);
            this.toolStrip1.TabIndex = 1;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(94, 22);
            this.toolStripLabel1.Size = new System.Drawing.Size(129, 22);
            this.toolStripLabel1.Text = "Number of Groups";
            this.toolStripLabel1.ToolTipText = "Find similar groups (using K-Means Algorithm)";
            // 
            // groupingComboBox
            // 
            this.groupingComboBox.AutoSize = false;
            this.groupingComboBox.DropDownWidth = 50;
            this.groupingComboBox.Items.AddRange(new object[] {
            "2",
            "3",
            "4",
            "5",
            "6",
            "None"});
            this.groupingComboBox.Name = "groupingComboBox";
            this.groupingComboBox.Size = new System.Drawing.Size(65, 25);
            this.groupingComboBox.Sorted = true;
            this.groupingComboBox.Text = "None";
            this.groupingComboBox.ToolTipText = "Find similar groups (using K-Means Algorithm)";
            this.groupingComboBox.SelectedIndexChanged += new System.EventHandler(this.toolStripComboBox1_SelectedIndexChanged);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // helpButton
            // 
            this.helpButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.helpButton.Image = global::GMS.Properties.Resources.help1;
            this.helpButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.helpButton.Name = "helpButton";
            this.helpButton.Size = new System.Drawing.Size(23, 22);
            this.helpButton.Text = "Help";
            this.helpButton.Click += new System.EventHandler(this.helpButtonClicked);
            // 
            // treeMapComboBox
            // 
            this.treeMapComboBox.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.treeMapComboBox.Items.AddRange(new object[] {
            "Choose TreeMap",
            "Styles per Country",
            "Countries per Style",
            "Countries per Government"});
            this.treeMapComboBox.Name = "treeMapComboBox";
            this.treeMapComboBox.Size = new System.Drawing.Size(150, 25);
            this.treeMapComboBox.Text = "Choose TreeMap";
            this.treeMapComboBox.ToolTipText = "Opens a TreeMap according to the chosen option";
            this.treeMapComboBox.SelectedIndexChanged += new System.EventHandler(this.treeMapComboBox_SelectedIndexChanged);
            // 
            // mainSplitContainer
            // 
            this.mainSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainSplitContainer.Location = new System.Drawing.Point(0, 0);
            this.mainSplitContainer.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.mainSplitContainer.Name = "mainSplitContainer";
            // 
            // mainSplitContainer.Panel1
            // 
            this.mainSplitContainer.Panel1.Controls.Add(this.PCMapSplitContainer);
            this.mainSplitContainer.Size = new System.Drawing.Size(743, 359);
            this.mainSplitContainer.SplitterDistance = 540;
            this.mainSplitContainer.Size = new System.Drawing.Size(991, 442);
            this.mainSplitContainer.SplitterDistance = 692;
            this.mainSplitContainer.SplitterWidth = 5;
            this.mainSplitContainer.TabIndex = 2;
            // 
            // PCMapSplitContainer
            // 
            this.PCMapSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PCMapSplitContainer.Location = new System.Drawing.Point(0, 0);
            this.PCMapSplitContainer.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.PCMapSplitContainer.Name = "PCMapSplitContainer";
            this.PCMapSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // PCMapSplitContainer.Panel1
            // 
            this.PCMapSplitContainer.Panel1.Controls.Add(this.glyphPanel);
            this.PCMapSplitContainer.Panel1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            // 
            // PCMapSplitContainer.Panel2
            // 
            this.PCMapSplitContainer.Panel2.Controls.Add(this.panel1);
            this.PCMapSplitContainer.Panel2.Controls.Add(this.toolStrip1);
            this.PCMapSplitContainer.Size = new System.Drawing.Size(540, 359);
            this.PCMapSplitContainer.SplitterDistance = 200;
            this.PCMapSplitContainer.Size = new System.Drawing.Size(692, 442);
            this.PCMapSplitContainer.SplitterDistance = 246;
            this.PCMapSplitContainer.SplitterWidth = 5;
            this.PCMapSplitContainer.TabIndex = 0;
            // 
            // glyphPanel
            // 
            this.glyphPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.glyphPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(189)))), ((int)(((byte)(183)))), ((int)(((byte)(107)))));
            this.glyphPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.glyphPanel.Controls.Add(this.glyphCheckBoxes);
            this.glyphPanel.Controls.Add(this.glyph1picture);
            this.glyphPanel.Controls.Add(this.pictureBox3);
            this.glyphPanel.Controls.Add(this.pictureBox2);
            this.glyphPanel.Controls.Add(this.pictureBox1);
            this.glyphPanel.Location = new System.Drawing.Point(8, 110);
            this.glyphPanel.Margin = new System.Windows.Forms.Padding(2);
            this.glyphPanel.Location = new System.Drawing.Point(11, 135);
            this.glyphPanel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.glyphPanel.Name = "glyphPanel";
            this.glyphPanel.Size = new System.Drawing.Size(150, 100);
            this.glyphPanel.TabIndex = 0;
            // 
            // glyphCheckBoxes
            // 
            this.glyphCheckBoxes.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)));
            this.glyphCheckBoxes.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.glyphCheckBoxes.CheckOnClick = true;
            this.glyphCheckBoxes.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.glyphCheckBoxes.FormattingEnabled = true;
            this.glyphCheckBoxes.Items.AddRange(new object[] {
            "Median Age",
            "Albums",
            "Employment",
            "GDP"});
            this.glyphCheckBoxes.Location = new System.Drawing.Point(2, 2);
            this.glyphCheckBoxes.Margin = new System.Windows.Forms.Padding(2);
            this.glyphCheckBoxes.Location = new System.Drawing.Point(3, 2);
            this.glyphCheckBoxes.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.glyphCheckBoxes.Name = "glyphCheckBoxes";
            this.glyphCheckBoxes.Size = new System.Drawing.Size(119, 90);
            this.glyphCheckBoxes.TabIndex = 8;
            this.glyphCheckBoxes.SelectedIndexChanged += new System.EventHandler(this.glyphCheckBoxes_SelectedIndexChanged);
            // 
            // glyph1picture
            // 
            this.glyph1picture.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.glyph1picture.Image = global::GMS.Properties.Resources.coin;
            this.glyph1picture.Location = new System.Drawing.Point(94, 54);
            this.glyph1picture.Margin = new System.Windows.Forms.Padding(2);
            this.glyph1picture.Location = new System.Drawing.Point(125, 66);
            this.glyph1picture.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.glyph1picture.Name = "glyph1picture";
            this.glyph1picture.Size = new System.Drawing.Size(20, 20);
            this.glyph1picture.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.glyph1picture.TabIndex = 1;
            this.glyph1picture.TabStop = false;
            // 
            // pictureBox3
            // 
            this.pictureBox3.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.pictureBox3.Image = global::GMS.Properties.Resources.work;
            this.pictureBox3.Location = new System.Drawing.Point(94, 37);
            this.pictureBox3.Margin = new System.Windows.Forms.Padding(2);
            this.pictureBox3.Location = new System.Drawing.Point(125, 46);
            this.pictureBox3.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.pictureBox3.Name = "pictureBox3";
            this.pictureBox3.Size = new System.Drawing.Size(20, 20);
            this.pictureBox3.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox3.TabIndex = 7;
            this.pictureBox3.TabStop = false;
            // 
            // pictureBox2
            // 
            this.pictureBox2.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.pictureBox2.Image = global::GMS.Properties.Resources.age;
            this.pictureBox2.Location = new System.Drawing.Point(94, 2);
            this.pictureBox2.Margin = new System.Windows.Forms.Padding(2);
            this.pictureBox2.Location = new System.Drawing.Point(125, 2);
            this.pictureBox2.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(20, 20);
            this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox2.TabIndex = 5;
            this.pictureBox2.TabStop = false;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.pictureBox1.Image = global::GMS.Properties.Resources.cd;
            this.pictureBox1.Location = new System.Drawing.Point(94, 20);
            this.pictureBox1.Margin = new System.Windows.Forms.Padding(2);
            this.pictureBox1.Location = new System.Drawing.Point(125, 25);
            this.pictureBox1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(20, 20);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 3;
            this.pictureBox1.TabStop = false;
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.Location = new System.Drawing.Point(4, 34);
            this.panel1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(534, 127);
            this.panel1.Size = new System.Drawing.Size(684, 156);
            this.panel1.TabIndex = 2;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(991, 442);
            this.Controls.Add(this.mainSplitContainer);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "Form1";
            this.Text = "GeoMusic Visualization";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.Form1_Load);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.mainSplitContainer.Panel1.ResumeLayout(false);
            this.mainSplitContainer.ResumeLayout(false);
            this.PCMapSplitContainer.Panel1.ResumeLayout(false);
            this.PCMapSplitContainer.Panel2.ResumeLayout(false);
            this.PCMapSplitContainer.Panel2.PerformLayout();
            this.PCMapSplitContainer.ResumeLayout(false);
            this.glyphPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.glyph1picture)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripComboBox groupingComboBox;
        private System.Windows.Forms.SplitContainer mainSplitContainer;
        private System.Windows.Forms.SplitContainer PCMapSplitContainer;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel glyphPanel;
        private System.Windows.Forms.PictureBox pictureBox3;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.PictureBox glyph1picture;
        private System.Windows.Forms.CheckedListBox glyphCheckBoxes;
        private System.Windows.Forms.ToolStripButton helpButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripComboBox treeMapComboBox;
    }
}

