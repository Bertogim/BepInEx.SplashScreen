﻿using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace BepInEx.SplashScreen
{
    partial class SplashScreen
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        private void InitializeComponent()
        {
            int fixedWidth = 768;
            float scale = (float)fixedWidth / 768;
            int scaledHeight = (int)(472 * scale);

            int labelHeight = 30;
            int progressHeight = 10;
            int totalHeight = scaledHeight + labelHeight + progressHeight;


            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SplashScreen));
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.labelBot = new System.Windows.Forms.Label();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();

            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();

            // 
            // pictureBox1
            // 
            this.pictureBox1.Dock = DockStyle.None;
            this.pictureBox1.Size = new Size(fixedWidth, scaledHeight);
            this.pictureBox1.Location = new Point(0, 0);
            this.pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.BackColor = Color.Black;

            // 
            // labelTop
            // 
            this.labelBot.AutoSize = false;
            this.labelBot.BackColor = Color.FromArgb(160, 0, 0, 0); // semi-transparent
            this.labelBot.ForeColor = Color.White;
            this.labelBot.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            this.labelBot.Dock = DockStyle.Bottom;
            this.labelBot.TextAlign = ContentAlignment.MiddleCenter;
            this.labelBot.Size = new Size(fixedWidth, labelHeight);
            this.labelBot.Location = new Point(0, scaledHeight);
            this.labelBot.Text = "Initializing...";

            // 
            // progressBar1
            // 
            this.progressBar1.Dock = DockStyle.Bottom;
            this.progressBar1.Size = new Size(fixedWidth, progressHeight);
            this.progressBar1.Location = new Point(0, scaledHeight + labelHeight);
            this.progressBar1.Style = ProgressBarStyle.Continuous;
            this.progressBar1.ForeColor = Color.LimeGreen;
            this.progressBar1.Value = 0;

            // 
            // checkedListBox1
            // 
            this.checkedListBox1 = new System.Windows.Forms.CheckedListBox();
            this.checkedListBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.checkedListBox1.CausesValidation = false;
            this.checkedListBox1.FormattingEnabled = true;
            this.checkedListBox1.Items.AddRange(new object[] {
    "Initialize environment and BepInEx",
    "Load and apply patchers",
    "Load and apply plugins",
    "Start the game"});
            this.checkedListBox1.SelectionMode = System.Windows.Forms.SelectionMode.None;
            this.checkedListBox1.ThreeDCheckBoxes = true;
            this.checkedListBox1.UseTabStops = false;
            this.checkedListBox1.Visible = false;

            //this.MouseDown += Form_MouseDown;
            //this.MouseMove += Form_MouseMove;
            //this.MouseUp += Form_MouseUp;

            //this.pictureBox1.MouseDown += Form_MouseDown;
            //this.pictureBox1.MouseMove += Form_MouseMove;
            //this.pictureBox1.MouseUp += Form_MouseUp;

            // 
            // SplashScreen
            // 
            this.AutoScaleMode = AutoScaleMode.None;
            this.ClientSize = new Size(fixedWidth, totalHeight); //(768, 472); //(512, 328);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.labelBot);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.checkedListBox1);
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Name = "LoadingScreen";
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Text = "The game is loading...";
            this.ShowInTaskbar = false;
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label labelBot;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.CheckedListBox checkedListBox1;

        private bool dragging = false;
        private Point dragCursorPoint;
        private Point dragFormPoint;

        protected override bool ShowWithoutActivation => true;

    }
}
