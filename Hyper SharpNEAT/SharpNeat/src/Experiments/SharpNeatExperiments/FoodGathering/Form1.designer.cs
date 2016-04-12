namespace SharpNeatLib.Experiments
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
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.seeNetworkToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadNetworkToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.playWithNetworkToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.adHocAnalysisToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(292, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.seeNetworkToolStripMenuItem,
            this.loadNetworkToolStripMenuItem,
            this.playWithNetworkToolStripMenuItem,
            this.adHocAnalysisToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(35, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // seeNetworkToolStripMenuItem
            // 
            this.seeNetworkToolStripMenuItem.Name = "seeNetworkToolStripMenuItem";
            this.seeNetworkToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
            this.seeNetworkToolStripMenuItem.Text = "See Network";
            this.seeNetworkToolStripMenuItem.Click += new System.EventHandler(this.seeNetworkToolStripMenuItem_Click);
            // 
            // loadNetworkToolStripMenuItem
            // 
            this.loadNetworkToolStripMenuItem.Name = "loadNetworkToolStripMenuItem";
            this.loadNetworkToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
            this.loadNetworkToolStripMenuItem.Text = "Load Network";
            this.loadNetworkToolStripMenuItem.Click += new System.EventHandler(this.loadNetworkToolStripMenuItem_Click);
            // 
            // playWithNetworkToolStripMenuItem
            // 
            this.playWithNetworkToolStripMenuItem.Name = "playWithNetworkToolStripMenuItem";
            this.playWithNetworkToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
            this.playWithNetworkToolStripMenuItem.Text = "Play with Network";
            this.playWithNetworkToolStripMenuItem.Click += new System.EventHandler(this.playWithNetworkToolStripMenuItem_Click);
            // 
            // adHocAnalysisToolStripMenuItem
            // 
            this.adHocAnalysisToolStripMenuItem.Name = "adHocAnalysisToolStripMenuItem";
            this.adHocAnalysisToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
            this.adHocAnalysisToolStripMenuItem.Text = "AdHoc Analysis";
            this.adHocAnalysisToolStripMenuItem.Click += new System.EventHandler(this.adHocAnalysisToolStripMenuItem_Click);
            // 
            // GNGViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 266);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "GNGViewer";
            this.Text = "Form1";
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.Form1_Paint);
            this.DoubleClick += new System.EventHandler(this.Form1_DoubleClick);
            this.Click += new System.EventHandler(this.Form1_Click);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.Form1_KeyPress);
            this.Load += new System.EventHandler(this.GNGViewer_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem seeNetworkToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadNetworkToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem playWithNetworkToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem adHocAnalysisToolStripMenuItem;
    }
}

