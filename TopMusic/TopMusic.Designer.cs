namespace TopMusic
{
    partial class TopMusic
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TopMusic));
            this.panelAddress = new System.Windows.Forms.Panel();
            this.systray = new System.Windows.Forms.NotifyIcon(this.components);
            this.panelWebBrowser = new System.Windows.Forms.Panel();
            this.webBrowser = new BurningICE.WebBrowserEx.WebBrowserEx();
            this.timerHeartbeat = new System.Windows.Forms.Timer(this.components);
            this.panelWebBrowser.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelAddress
            // 
            this.panelAddress.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(220)))), ((int)(((byte)(241)))));
            this.panelAddress.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelAddress.Location = new System.Drawing.Point(0, 0);
            this.panelAddress.Name = "panelAddress";
            this.panelAddress.Size = new System.Drawing.Size(784, 32);
            this.panelAddress.TabIndex = 0;
            // 
            // systray
            // 
            this.systray.BalloonTipText = "已播放: 0";
            this.systray.BalloonTipTitle = "Top Music";
            this.systray.Icon = ((System.Drawing.Icon)(resources.GetObject("systray.Icon")));
            this.systray.Text = "Top Music";
            this.systray.Click += new System.EventHandler(this.systray_DoubleClick);
            // 
            // panelWebBrowser
            // 
            this.panelWebBrowser.BackColor = System.Drawing.Color.White;
            this.panelWebBrowser.Controls.Add(this.webBrowser);
            this.panelWebBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelWebBrowser.ForeColor = System.Drawing.Color.Black;
            this.panelWebBrowser.Location = new System.Drawing.Point(0, 32);
            this.panelWebBrowser.Name = "panelWebBrowser";
            this.panelWebBrowser.Size = new System.Drawing.Size(784, 529);
            this.panelWebBrowser.TabIndex = 2;
            // 
            // webBrowser
            // 
            this.webBrowser.BackColor = System.Drawing.Color.White;
            this.webBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser.Location = new System.Drawing.Point(0, 0);
            this.webBrowser.Name = "webBrowser";
            this.webBrowser.Size = new System.Drawing.Size(784, 529);
            this.webBrowser.TabIndex = 1;
            // 
            // timerHeartbeat
            // 
            this.timerHeartbeat.Interval = 15000;
            this.timerHeartbeat.Tick += new System.EventHandler(this.timerHeartbeat_Tick);
            // 
            // TopMusic
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 561);
            this.Controls.Add(this.panelWebBrowser);
            this.Controls.Add(this.panelAddress);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "TopMusic";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Top Music";
            this.Activated += new System.EventHandler(this.TopMusic_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormTopMusic_FormClosing);
            this.Load += new System.EventHandler(this.FormTopMusic_Load);
            this.Shown += new System.EventHandler(this.TopMusic_Shown);
            this.Resize += new System.EventHandler(this.FormTopMusic_Resize);
            this.panelWebBrowser.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelAddress;
        private BurningICE.WebBrowserEx.WebBrowserEx webBrowser;
        private System.Windows.Forms.NotifyIcon systray;
        private System.Windows.Forms.Panel panelWebBrowser;
        private System.Windows.Forms.Timer timerHeartbeat;
    }
}

