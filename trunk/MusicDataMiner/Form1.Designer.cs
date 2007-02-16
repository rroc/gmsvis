using System;
using System.IO;

namespace MusicDataminer
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
            this.splitLayout = new System.Windows.Forms.SplitContainer();
            this.clearLogButton = new System.Windows.Forms.Button();
            this.titleLabel = new System.Windows.Forms.Label();
            this.infoLabel = new System.Windows.Forms.Label();
            this.stylesLabel = new System.Windows.Forms.Label();
            this.styleCheckBoxList = new System.Windows.Forms.CheckedListBox();
            this.startQueryButton = new System.Windows.Forms.Button();
            this.outputBox = new System.Windows.Forms.TextBox();
            this.splitLayout.Panel1.SuspendLayout();
            this.splitLayout.Panel2.SuspendLayout();
            this.splitLayout.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitLayout
            // 
            this.splitLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitLayout.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitLayout.Location = new System.Drawing.Point(0, 0);
            this.splitLayout.Name = "splitLayout";
            // 
            // splitLayout.Panel1
            // 
            this.splitLayout.Panel1.Controls.Add(this.clearLogButton);
            this.splitLayout.Panel1.Controls.Add(this.titleLabel);
            this.splitLayout.Panel1.Controls.Add(this.infoLabel);
            this.splitLayout.Panel1.Controls.Add(this.stylesLabel);
            this.splitLayout.Panel1.Controls.Add(this.styleCheckBoxList);
            this.splitLayout.Panel1.Controls.Add(this.startQueryButton);
            // 
            // splitLayout.Panel2
            // 
            this.splitLayout.Panel2.Controls.Add(this.outputBox);
            this.splitLayout.Size = new System.Drawing.Size(1051, 454);
            this.splitLayout.SplitterDistance = 219;
            this.splitLayout.TabIndex = 0;
            // 
            // clearLogButton
            // 
            this.clearLogButton.Location = new System.Drawing.Point(46, 419);
            this.clearLogButton.Name = "clearLogButton";
            this.clearLogButton.Size = new System.Drawing.Size(120, 23);
            this.clearLogButton.TabIndex = 5;
            this.clearLogButton.Text = "Erase Selected";
            this.clearLogButton.UseVisualStyleBackColor = true;
            this.clearLogButton.Click += new System.EventHandler(this.clearLogButton_Click);
            // 
            // titleLabel
            // 
            this.titleLabel.AutoSize = true;
            this.titleLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.titleLabel.Location = new System.Drawing.Point(20, 9);
            this.titleLabel.Name = "titleLabel";
            this.titleLabel.Size = new System.Drawing.Size(123, 17);
            this.titleLabel.TabIndex = 4;
            this.titleLabel.Text = "MusicDataMiner";
            // 
            // infoLabel
            // 
            this.infoLabel.AutoSize = true;
            this.infoLabel.Location = new System.Drawing.Point(20, 36);
            this.infoLabel.Name = "infoLabel";
            this.infoLabel.Size = new System.Drawing.Size(144, 102);
            this.infoLabel.TabIndex = 3;
            this.infoLabel.Text = "Browses through \r\npreprocessed freedb \r\n-information and \r\nmatches that with \r\nth" +
                "e online \r\nMusicBrainz-data.";
            // 
            // stylesLabel
            // 
            this.stylesLabel.AutoSize = true;
            this.stylesLabel.Location = new System.Drawing.Point(43, 148);
            this.stylesLabel.Name = "stylesLabel";
            this.stylesLabel.Size = new System.Drawing.Size(121, 17);
            this.stylesLabel.TabIndex = 2;
            this.stylesLabel.Text = "Styles to Process:";
            // 
            // styleCheckBoxList
            // 
            this.styleCheckBoxList.CheckOnClick = true;
            this.styleCheckBoxList.FormattingEnabled = true;
            this.styleCheckBoxList.Items.AddRange(new object[] {
            "blues",
            "classical",
            "country",
            "data",
            "folk",
            "jazz",
            "misc",
            "newage",
            "reggae",
            "rock",
            "soundtrack"});
            this.styleCheckBoxList.Location = new System.Drawing.Point(46, 168);
            this.styleCheckBoxList.Name = "styleCheckBoxList";
            this.styleCheckBoxList.Size = new System.Drawing.Size(120, 191);
            this.styleCheckBoxList.Sorted = true;
            this.styleCheckBoxList.TabIndex = 1;
            // 
            // startQueryButton
            // 
            this.startQueryButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.startQueryButton.Location = new System.Drawing.Point(46, 382);
            this.startQueryButton.Name = "startQueryButton";
            this.startQueryButton.Size = new System.Drawing.Size(120, 23);
            this.startQueryButton.TabIndex = 0;
            this.startQueryButton.Text = "Start Queries";
            this.startQueryButton.UseVisualStyleBackColor = true;
            this.startQueryButton.Click += new System.EventHandler(this.startQueryButton_Click);
            // 
            // outputBox
            // 
            this.outputBox.BackColor = System.Drawing.SystemColors.Window;
            this.outputBox.CausesValidation = false;
            this.outputBox.Font = new System.Drawing.Font("Courier New", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.outputBox.Location = new System.Drawing.Point(0, 0);
            this.outputBox.Multiline = true;
            this.outputBox.Name = "outputBox";
            this.outputBox.ReadOnly = true;
            this.outputBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.outputBox.Size = new System.Drawing.Size(828, 454);
            this.outputBox.TabIndex = 0;
            this.outputBox.TextChanged += new System.EventHandler(this.outputBox_TextChanged);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1051, 454);
            this.Controls.Add(this.splitLayout);
            this.Name = "Form1";
            this.Text = "Music Dataminer";
            this.splitLayout.Panel1.ResumeLayout(false);
            this.splitLayout.Panel1.PerformLayout();
            this.splitLayout.Panel2.ResumeLayout(false);
            this.splitLayout.Panel2.PerformLayout();
            this.splitLayout.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitLayout;
        private System.Windows.Forms.Label infoLabel;
        private System.Windows.Forms.Label stylesLabel;
        private System.Windows.Forms.Label titleLabel;

        public System.Windows.Forms.TextBox outputBox;
        public System.Windows.Forms.Button startQueryButton;
        public System.Windows.Forms.CheckedListBox styleCheckBoxList;
        private System.Windows.Forms.Button clearLogButton;
    }
}

