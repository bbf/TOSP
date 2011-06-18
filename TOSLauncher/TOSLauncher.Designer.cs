namespace TOSP
{
    partial class TOSLauncher
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
            this.pathDialog = new System.Windows.Forms.OpenFileDialog();
            this.launchBtn = new System.Windows.Forms.Button();
            this.pathBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.pathBrowse = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // pathDialog
            // 
            this.pathDialog.FileName = "TerrariaServer.exe";
            this.pathDialog.Filter = "Terraria Server|TerrariaServer.exe";
            // 
            // launchBtn
            // 
            this.launchBtn.Location = new System.Drawing.Point(520, 12);
            this.launchBtn.Name = "launchBtn";
            this.launchBtn.Size = new System.Drawing.Size(136, 36);
            this.launchBtn.TabIndex = 0;
            this.launchBtn.Text = "Launch / Focus";
            this.launchBtn.UseVisualStyleBackColor = true;
            this.launchBtn.Click += new System.EventHandler(this.launchBtn_Click);
            // 
            // pathBox
            // 
            this.pathBox.Location = new System.Drawing.Point(95, 21);
            this.pathBox.Name = "pathBox";
            this.pathBox.ReadOnly = true;
            this.pathBox.Size = new System.Drawing.Size(326, 20);
            this.pathBox.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(18, 24);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(71, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Terraria Path:";
            // 
            // pathBrowse
            // 
            this.pathBrowse.Location = new System.Drawing.Point(427, 19);
            this.pathBrowse.Name = "pathBrowse";
            this.pathBrowse.Size = new System.Drawing.Size(75, 23);
            this.pathBrowse.TabIndex = 3;
            this.pathBrowse.Text = "Browse...";
            this.pathBrowse.UseVisualStyleBackColor = true;
            this.pathBrowse.Click += new System.EventHandler(this.pathBrowse_Click);
            // 
            // TOSLauncher
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(673, 62);
            this.Controls.Add(this.pathBrowse);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.pathBox);
            this.Controls.Add(this.launchBtn);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "TOSLauncher";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Terraria OpenSource Plugin Launcher";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.TOSLauncher_FormClosing);
            this.Load += new System.EventHandler(this.TOSLauncher_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.OpenFileDialog pathDialog;
        private System.Windows.Forms.Button launchBtn;
        private System.Windows.Forms.TextBox pathBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button pathBrowse;
    }
}

