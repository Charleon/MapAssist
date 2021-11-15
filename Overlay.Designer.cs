
using MapAssist.Drawing;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace MapAssist
{
    partial class Overlay
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

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
            _overlayDrawer = new Drawing.OverlayDrawer(_configuration);

            ComponentResourceManager resources = new ComponentResourceManager(typeof(Overlay));
            
            this.SuspendLayout();

            // 
            // frmOverlay
            // 
            AutoScaleDimensions = new SizeF(6F, 13F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1767, 996);
  
            Controls.Add(_overlayDrawer);
            FormBorderStyle = FormBorderStyle.None;
            Icon = (Icon)(resources.GetObject("$this.Icon"));
            Name = "Overlay";
            TransparencyKey = Color.Black;
            BackColor = Color.Black;
            WindowState = FormWindowState.Maximized;
            Load += new EventHandler(_overlayDrawer.Load);
            Load += new EventHandler(Overlay_Load);
            FormClosing += new FormClosingEventHandler(Overlay_FormClosing);
            FormClosing += new FormClosingEventHandler(_overlayDrawer.FormClosing);
            //((ISupportInitialize)(mapOverlay)).EndInit();
           
            ResumeLayout(false);
        }

        #endregion

        //private PictureBox mapOverlay;
        private Drawing.OverlayDrawer _overlayDrawer;
    }
}
