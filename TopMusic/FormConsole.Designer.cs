namespace TopMusic
{
    partial class FormConsole
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
            this.panelCmd = new System.Windows.Forms.Panel();
            this.panelCmdLine = new System.Windows.Forms.Panel();
            this.txtCmdLine = new System.Windows.Forms.TextBox();
            this.panelCmdLabel = new System.Windows.Forms.Panel();
            this.labelCmdLabel = new System.Windows.Forms.Label();
            this.panelConsole = new System.Windows.Forms.Panel();
            this.txtConsole = new System.Windows.Forms.TextBox();
            this.panelCmd.SuspendLayout();
            this.panelCmdLine.SuspendLayout();
            this.panelCmdLabel.SuspendLayout();
            this.panelConsole.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelCmd
            // 
            this.panelCmd.Controls.Add(this.panelCmdLine);
            this.panelCmd.Controls.Add(this.panelCmdLabel);
            this.panelCmd.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelCmd.Location = new System.Drawing.Point(0, 537);
            this.panelCmd.Name = "panelCmd";
            this.panelCmd.Size = new System.Drawing.Size(784, 24);
            this.panelCmd.TabIndex = 0;
            // 
            // panelCmdLine
            // 
            this.panelCmdLine.Controls.Add(this.txtCmdLine);
            this.panelCmdLine.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelCmdLine.Location = new System.Drawing.Point(32, 0);
            this.panelCmdLine.Name = "panelCmdLine";
            this.panelCmdLine.Size = new System.Drawing.Size(752, 24);
            this.panelCmdLine.TabIndex = 1;
            // 
            // txtCmdLine
            // 
            this.txtCmdLine.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtCmdLine.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtCmdLine.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtCmdLine.Location = new System.Drawing.Point(0, 0);
            this.txtCmdLine.Multiline = true;
            this.txtCmdLine.Name = "txtCmdLine";
            this.txtCmdLine.Size = new System.Drawing.Size(752, 24);
            this.txtCmdLine.TabIndex = 0;
            this.txtCmdLine.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtCmdLine_KeyDown);
            this.txtCmdLine.KeyUp += new System.Windows.Forms.KeyEventHandler(this.txtCmdLine_KeyUp);
            // 
            // panelCmdLabel
            // 
            this.panelCmdLabel.Controls.Add(this.labelCmdLabel);
            this.panelCmdLabel.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelCmdLabel.Location = new System.Drawing.Point(0, 0);
            this.panelCmdLabel.Name = "panelCmdLabel";
            this.panelCmdLabel.Size = new System.Drawing.Size(32, 24);
            this.panelCmdLabel.TabIndex = 0;
            // 
            // labelCmdLabel
            // 
            this.labelCmdLabel.AutoSize = true;
            this.labelCmdLabel.Location = new System.Drawing.Point(6, 6);
            this.labelCmdLabel.Name = "labelCmdLabel";
            this.labelCmdLabel.Size = new System.Drawing.Size(11, 12);
            this.labelCmdLabel.TabIndex = 0;
            this.labelCmdLabel.Text = ">";
            // 
            // panelConsole
            // 
            this.panelConsole.Controls.Add(this.txtConsole);
            this.panelConsole.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelConsole.Location = new System.Drawing.Point(0, 0);
            this.panelConsole.Name = "panelConsole";
            this.panelConsole.Size = new System.Drawing.Size(784, 537);
            this.panelConsole.TabIndex = 1;
            // 
            // txtConsole
            // 
            this.txtConsole.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtConsole.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtConsole.Location = new System.Drawing.Point(0, 0);
            this.txtConsole.Multiline = true;
            this.txtConsole.Name = "txtConsole";
            this.txtConsole.ReadOnly = true;
            this.txtConsole.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtConsole.Size = new System.Drawing.Size(784, 537);
            this.txtConsole.TabIndex = 0;
            // 
            // FormConsole
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(784, 561);
            this.Controls.Add(this.panelConsole);
            this.Controls.Add(this.panelCmd);
            this.Name = "FormConsole";
            this.Text = "Console";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormConsole_FormClosing);
            this.panelCmd.ResumeLayout(false);
            this.panelCmdLine.ResumeLayout(false);
            this.panelCmdLine.PerformLayout();
            this.panelCmdLabel.ResumeLayout(false);
            this.panelCmdLabel.PerformLayout();
            this.panelConsole.ResumeLayout(false);
            this.panelConsole.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelCmd;
        private System.Windows.Forms.Panel panelConsole;
        private System.Windows.Forms.Panel panelCmdLine;
        private System.Windows.Forms.Panel panelCmdLabel;
        private System.Windows.Forms.Label labelCmdLabel;
        private System.Windows.Forms.TextBox txtCmdLine;
        private System.Windows.Forms.TextBox txtConsole;
    }
}