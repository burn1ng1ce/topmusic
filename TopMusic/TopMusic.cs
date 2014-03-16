using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Xml;
using System.Threading;
using System.IO;
using System.Diagnostics;
using mshtml;

namespace TopMusic
{
    public enum KeyModifiers //组合键枚举
    {
        None = 0,
        Alt = 1,
        Control = 2,
        Shift = 4,
        Windows = 8
    }

    public partial class TopMusic : Form
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(typeof(TopMusic));
        private const int WM_HOTKEY = 0x0312;
        private const int WM_ACTIVATE = 0x0006;
        private const int BM_CLICK = 0xF5;

        private const int HOTKEY_CTRL_R = 230;
        private const int HOTKEY_CTRL_N = 231;
        private const int HOTKEY_F12 = 232;

        private static Encoding GBK = Encoding.GetEncoding(936);
        private const string TOP_MUSIC_SERVER_URL = "http://rpcbeta.networkbench.com/liaoxj/";
        //private const string TOP_MUSIC_SERVER_URL = "http://localhost/topmusic/";
        private const int GC_INTERVAL = 1800000000;
        private string HOMEPAGE = "about:blank";
        private string USER_AGENT = null;

        private bool isFormLoaded;
        private TextBox txtLocation;
        private string lastLocation;
        private ToolBarButton btnHome;
        private ToolBarButton btnBack;
        private ToolBarButton btnForward;
        private ToolBarButton btnGo;
        private ToolBarButton btnConfig;
        private ToolBarButton btnExit;
        private ToolBar tBar1;
        private ToolBar tBar2;
        private ImageList imgList;
        private FormScriptError scriptErrorForm;
        private FormConsole formConsole;
        private Config config;
        private ICollection<TopMusicSite> topMusicSites;
        private string scriptOnCloseWindow;
        private RemoteCommandExecutor remoteCommandExecutor;
        private bool systrayMode;
        private bool userExit = false;
        private bool appExit = false;
        private long lastGCTime;

        [DllImport("user32.dll", EntryPoint = "RegisterHotKey")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);
        [DllImport("user32.dll", EntryPoint = "UnregisterHotKey")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        [DllImport("user32.dll", EntryPoint = "GetClassName", CharSet = CharSet.Auto)]
        public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);
        [DllImport("user32.dll", EntryPoint = "GetWindowText", CharSet = CharSet.Auto)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpWindowText, int nMaxCount);
        [DllImport("user32.dll", EntryPoint = "GetDlgItemText", CharSet = CharSet.Auto)]
        public static extern int GetDlgItemText(IntPtr hWnd, int nIDDlgItem, StringBuilder lpDlgItemText, int nMaxCount);
        [DllImport("user32.dll", EntryPoint = "SendDlgItemMessage")]
        public static extern IntPtr SendDlgItemMessage(IntPtr hWnd, int IDDlgItem, int uMsg, int wParam, int lParam);
        [DllImport("winmm.dll", EntryPoint = "waveOutSetVolume")]
        public static extern int waveOutSetVolume(IntPtr hWnd, uint dwVolume);
        [DllImport("psapi.dll")]
        static extern int EmptyWorkingSet(IntPtr hwProc);

        protected internal BurningICE.WebBrowserEx.WebBrowserEx WebBrowser
        {
            get { return this.webBrowser; }
        }

        protected internal string ScriptOnCloseWindow
        {
            get { return this.scriptOnCloseWindow; }
            set { this.scriptOnCloseWindow = value; }
        }

        public TopMusic(bool systrayMode)
        {
            this.isFormLoaded = false;
            InitializeComponent();

            InitializeComponentInternal();
            this.LoadImage();

            this.webBrowser.UpdateLocation += new BurningICE.WebBrowserEx.WebBrowserEx.UpdateLocationHandler(this.webBrowser_UpdateLocation);
            this.webBrowser.ShowScriptErrors += new BurningICE.WebBrowserEx.WebBrowserEx.ShowScriptErrorsHandler(this.webBrowser_ShowScriptErrors);
            this.webBrowser.OnNewWindowCreated += new BurningICE.WebBrowserEx.WebBrowserEx.NewWindowCreatedHandler(this.webBrowser_OnNewWindowCreated);
            this.webBrowser.OnBeginNavigate += new BurningICE.WebBrowserEx.WebBrowserEx.BeginNavigateHandler(this.webBrowser_OnBeginNavigate);
            this.webBrowser.OnDocumentComplete += new BurningICE.WebBrowserEx.WebBrowserEx.DocumentCompleteHandler(this.webBrowser_OnDocumentComplete);
            this.webBrowser.OnDocumentWindowFocus += new BurningICE.WebBrowserEx.WebBrowserEx.DocumentWindowFocusHandler(this.webBrowser_OnDocumentWindowFocus);
            this.webBrowser.OnFileDownload += new BurningICE.WebBrowserEx.WebBrowserEx.FileDownloadHandler(this.webBrowser_OnFileDownload);
            this.webBrowser.OnWindowClosing += new BurningICE.WebBrowserEx.WebBrowserEx.WindowClosingHandler(this.webBrowser_OnWindowClosing);
            this.webBrowser.OnWindowClosed += new BurningICE.WebBrowserEx.WebBrowserEx.WindowClosedHandler(this.webBrowser_OnWindowClosed);

            this.webBrowser.RegisterCommand("console", new ConsoleCommand(this));
            this.webBrowser.RegisterCommand("navigate", new NavigateCommand(this));
            this.webBrowser.RegisterCommand("closeWindow", new CloseWindowCommand(this));
            this.webBrowser.RegisterCommand("onCloseWindow", new OnCloseWindowCommand(this));
            System.Collections.Hashtable parameters = new System.Collections.Hashtable();
            this.webBrowser.RegisterCommand("get", new GetCommand(parameters));
            this.webBrowser.RegisterCommand("put", new PutCommand(parameters));
            this.webBrowser.RegisterCommand("remove", new RemoveCommand(parameters));
            this.webBrowser.RegisterCommand("httpget", new HttpGetCommand());

            this.systrayMode = systrayMode;
            logger.Info("TopMusic started " + (systrayMode ? "in systray mode." : "."));
        }


        private void InitializeComponentInternal()
        {
            this.SuspendLayout();
            this.panelAddress.SuspendLayout();
            // 
            // imgList
            // 
            this.imgList = new ImageList();
            this.imgList.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this.imgList.ImageSize = new System.Drawing.Size(16, 16);
            this.imgList.TransparentColor = System.Drawing.Color.Transparent;

            // init tBar1
            this.btnHome = new ToolBarButton();
            this.btnHome.ImageIndex = 0;
            this.btnHome.Name = "btnHome";
            this.btnHome.Tag = "Home";
            this.btnHome.ToolTipText = "首页";

            this.btnBack = new ToolBarButton();
            this.btnBack.ImageIndex = 1;
            this.btnBack.Name = "btnBack";
            this.btnBack.Tag = "GoBack";
            this.btnBack.ToolTipText = "后退";

            this.btnForward = new ToolBarButton();
            this.btnForward.ImageIndex = 2;
            this.btnForward.Name = "btnForward";
            this.btnForward.Tag = "GoForward";
            this.btnForward.ToolTipText = "前进";

            this.tBar1 = new System.Windows.Forms.ToolBar();
            this.tBar1.Anchor = AnchorStyles.None;
            this.tBar1.Appearance = ToolBarAppearance.Flat;
            this.tBar1.AllowDrop = false;
            this.tBar1.Divider = false;
            this.tBar1.BorderStyle = BorderStyle.None;
            this.tBar1.ButtonSize = new System.Drawing.Size(24, 20);
            this.tBar1.Location = new Point(0, 6);
            this.tBar1.Dock = DockStyle.None;
            this.tBar1.ShowToolTips = true;
            this.tBar1.ImageList = this.imgList;
            this.tBar1.TabIndex = 1;
            this.tBar1.TabStop = true;
            this.tBar1.Wrappable = false;
            this.tBar1.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler(this.tBar_ButtonClick);

            this.tBar1.Buttons.AddRange(new ToolBarButton[] {
                this.btnHome,
                this.btnBack,
                this.btnForward
            });

            this.tBar1.Size = new Size(this.tBar1.Buttons.Count * this.tBar1.ButtonSize.Width, 20);

            this.panelAddress.Controls.Add(tBar1);

            // init tbar2
            this.btnGo = new ToolBarButton();
            this.btnGo.Name = "btnGo";
            this.btnGo.Tag = "Go";
            this.btnGo.ToolTipText = "前往/刷新";
            this.btnGo.ImageIndex = 3;

            this.btnConfig = new ToolBarButton();
            this.btnConfig.Name = "btnConfig";
            this.btnConfig.Tag = "Config";
            this.btnConfig.ToolTipText = "设置";
            this.btnConfig.ImageIndex = 4;

            this.btnExit = new ToolBarButton();
            this.btnExit.Name = "btnExit";
            this.btnExit.Tag = "Exit";
            this.btnExit.ToolTipText = "退出";
            this.btnExit.ImageIndex = 5;

            this.tBar2 = new System.Windows.Forms.ToolBar();
            this.tBar2.Anchor = AnchorStyles.Top;
            this.tBar2.Appearance = ToolBarAppearance.Flat;
            this.tBar2.AllowDrop = false;
            this.tBar2.Divider = false;
            this.tBar2.BorderStyle = BorderStyle.None;
            this.tBar2.ButtonSize = new System.Drawing.Size(24, 20);
            this.tBar2.Dock = DockStyle.None;
            this.tBar2.ShowToolTips = true;
            this.tBar2.ImageList = this.imgList;
            this.tBar2.TabIndex = 2;
            this.tBar2.TabStop = true;
            this.tBar2.Wrappable = false;
            this.tBar2.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler(this.tBar_ButtonClick);
            this.tBar2.Buttons.AddRange(new ToolBarButton[] {
                this.btnGo,
                this.btnConfig,
                this.btnExit
            });

            this.tBar2.Size = new Size(this.tBar2.Buttons.Count * this.tBar2.ButtonSize.Width, 20);
            this.tBar2.Location = new Point(this.panelAddress.Width - this.tBar2.Width, 6);

            this.panelAddress.Controls.Add(tBar2);

            //init txtLocation
            this.txtLocation = new TextBox();
            this.txtLocation.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtLocation.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.txtLocation.Name = "txtLocation";
            this.txtLocation.Size = new System.Drawing.Size(this.panelAddress.Width - this.tBar1.Width - this.tBar2.Width, 22);
            this.txtLocation.Location = new System.Drawing.Point(this.tBar1.Width, 8);
            this.txtLocation.TabIndex = 0;
            this.txtLocation.Text = "about:blank";
            this.txtLocation.KeyUp += new System.Windows.Forms.KeyEventHandler(this.txtLocation_KeyUp);
            this.panelAddress.Controls.Add(this.txtLocation);

            this.panelAddress.ResumeLayout(false);

            this.ResumeLayout(false);
        }

        private void LoadImage()
        {
            if (this.imgList != null)
            {
                string[] images = new string[] { 
                    "Home.png",
                    "Back.png",
                    "Forward.png",
                    "Go.png",
                    "Settings.png",
                    "Exit.png"
                };
                foreach (string img in images)
                {
                    if (System.IO.File.Exists("ico/" + img))
                    {
                        if (img.ToLower().EndsWith(".ico"))
                        {
                            System.Drawing.Icon ico = new System.Drawing.Icon("ico/" + img);
                            this.imgList.Images.Add(ico);
                        }
                        else
                        {
                            System.Drawing.Image image = System.Drawing.Image.FromFile("ico/" + img);
                            this.imgList.Images.Add(image);
                        }
                    }
                }
            }
        }

        private void tBar_ButtonClick(object sender, System.Windows.Forms.ToolBarButtonClickEventArgs e)
        {
            //触发事件
            if (e.Button.Tag != null)
            {
                if (e.Button.Tag.Equals("Home"))
                {
                    this.webBrowser.Navigate(HOMEPAGE);
                }
                else if (e.Button.Tag.Equals("GoBack"))
                {
                    this.webBrowser.GoBack();
                }
                else if (e.Button.Tag.Equals("GoForward"))
                {
                    this.webBrowser.GoForward();
                }
                else if (e.Button.Tag.Equals("Go"))
                {
                    this.Go();
                }
                else if (e.Button.Tag.Equals("Config"))
                {
                    FormConfig formConfig = new FormConfig(this.config);
                    formConfig.ShowDialog(this);
                }
                else if (e.Button.Tag.Equals("Exit"))
                {
                    this.userExit = true;
                    // 用户主动退出， 同时退出Updater程序
                    Process[] processes = Process.GetProcessesByName("TopMusicUpdate");
                    if (processes != null && processes.Length > 0)
                    {
                        logger.Error("killing TopMusicUpdate process...");
                        foreach (Process p in processes)
                        {
                            try
                            {
                                p.Kill();
                            }
                            catch (Exception ex)
                            {
                                logger.Error("failed to kill TopMusicUpdate, pid=" + p.Id, ex);
                            }
                        }
                    }
                    logger.Info("Exitting TopMusic...");
                    Application.Exit();
                }
            }
        }

        public void Pause()
        {
            if (this.remoteCommandExecutor != null)
                this.remoteCommandExecutor.Pause();
        }

        public void Resume()
        {
            if (this.remoteCommandExecutor != null)
                this.remoteCommandExecutor.Resume();
        }

        private void txtLocation_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            //回车浏览网页
            if (e.KeyCode == Keys.Enter && e.Modifiers == Keys.None)
            {
                Go();
            }
        }

        private void Go()
        {
            //Go or refresh
            if (this.lastLocation == null || !this.lastLocation.Equals(this.txtLocation.Text))
            {
                this.webBrowser.Navigate(this.txtLocation.Text);
            }
            else
            {
                this.webBrowser.Refresh2();
            }
        }

        private void FormTopMusic_Resize(object sender, EventArgs e)
        {
            if (this.isFormLoaded)
            {
                this.SuspendLayout();
                this.panelAddress.SuspendLayout();
                this.tBar1.Left = 0;
                this.tBar2.Left = this.panelAddress.Width - this.tBar2.Width;
                this.txtLocation.Width = this.panelAddress.Width - this.tBar1.Width - this.tBar2.Width;

                this.panelAddress.ResumeLayout(false);
                this.ResumeLayout(true);
            }

            if (this.WindowState == FormWindowState.Minimized)
            {
                this.systray.Visible = true;
                this.systray.ShowBalloonTip(500);
                //this.panelWebBrowser.Enabled = false;
                this.Hide();
            }
            else if(this.Visible)   // 判断当前窗口是否可见，注意Webbrowser控件可能偷焦点（导致窗口莫名其妙弹出！）
            {
                this.systray.Visible = false;
               //this.panelWebBrowser.Enabled = true;
            }

            Console.WriteLine("topmusic resize: state: " + this.WindowState + ", size: " + this.Size.Width + ", " + this.Size.Height + ", visible: " + this.Visible + ", webbrowser: " + this.webBrowser.Size.Width + ", " + this.webBrowser.Size.Height + ", visible: " + this.webBrowser.Visible);
        }

        private void FormTopMusic_Load(object sender, EventArgs e)
        {
            foreach (InputLanguage lang in InputLanguage.InstalledInputLanguages)
            {
                if (lang.LayoutName == "中文 (简体) - 美式键盘")
                {
                    InputLanguage.CurrentInputLanguage = lang;
                }
            }

            this.webBrowser.CreateNewIE();

            RegisterHotKey(this.Handle, HOTKEY_CTRL_R, (int)KeyModifiers.Control, (int)Keys.R);
            RegisterHotKey(this.Handle, HOTKEY_CTRL_N, (int)KeyModifiers.Control, (int)Keys.N);
            RegisterHotKey(this.Handle, HOTKEY_F12, (int)KeyModifiers.Control, (int)Keys.F12);

            this.LoadConfig();

            if (USER_AGENT != null && USER_AGENT.Length > 0)
            {
                this.webBrowser.SetUserAgent(USER_AGENT);
            }

            this.isFormLoaded = true;
            /*
            if (this.urlToOpen != null && this.urlToOpen.Length > 0)
            {
                this.webBrowser.Navigate(this.urlToOpen);
            }
            */

            waveOutSetVolume(IntPtr.Zero, 0);

            this.remoteCommandExecutor = new RemoteCommandExecutor(TOP_MUSIC_SERVER_URL + "getCommands", this.config.RasEntryName, this.topMusicSites, this.webBrowser);
            this.remoteCommandExecutor.OnCommandPlayed += remoteCommandExecutor_OnCommandPlayed;
            this.remoteCommandExecutor.Start();

            this.webBrowser.RegisterCommand("getRemoteCommand", new GetRemoteCommandCommand(this.remoteCommandExecutor));

            this.timerHeartbeat.Enabled = true;
            this.timerHeartbeat.Start();

            if (this.systrayMode)
            {
                this.systray.Visible = true;
                this.systray.ShowBalloonTip(500);
                //this.panelWebBrowser.Enabled = false;
                this.Hide();
            }
        }

        void remoteCommandExecutor_OnCommandPlayed(RemoteCommand command)
        {
            this.systray.BalloonTipText = "已播放: " + this.remoteCommandExecutor.PlayedCount;
        }

        private void TopMusic_Shown(object sender, EventArgs e)
        {
            if (this.config.RasEntryName == null)
            {
                // 未配置ADSL，显示配置页
                FormConfig formConfig = new FormConfig(this.config);
                formConfig.ShowDialog(this);
            }
        }

        private void LoadConfig()
        {
            this.config = new Config();

            // load topmusic-sites
            string topmusicSitesXml = HttpUtil.Get(TOP_MUSIC_SERVER_URL + "topmusic-sites.xml");
            if (topmusicSitesXml != null && topmusicSitesXml.Length > 0)
            {
                List<TopMusicSite> topMusicSites = new List<TopMusicSite>();
                try
                {
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(topmusicSitesXml);
                    XmlNodeList siteNodes = doc.SelectNodes("/sites/site");
                    for (int i = 0; i < siteNodes.Count; i++)
                    {
                        XmlNode node = siteNodes.Item(i);
                        if (node.NodeType != XmlNodeType.Element)
                        {
                            continue;
                        }

                        XmlElement siteNode = (XmlElement)node;
                        string siteName = null;
                        string siteHost = null;
                        string siteHome = null;
                        string player = null;

                        XmlNodeList propNodes = siteNode.ChildNodes;
                        for (int j = 0; j < propNodes.Count; ++j)
                        {
                            XmlNode propNode = propNodes.Item(j);
                            if (propNode.NodeType == XmlNodeType.Element)
                            {
                                XmlElement prop = (XmlElement)propNode;
                                string name = prop.LocalName;
                                string value = prop.InnerText;
                                if (value != null)
                                {
                                    value = value.Trim();
                                    if (name == "name")
                                    {
                                        siteName = value;
                                    }
                                    else if (name == "host")
                                    {
                                        siteHost = value;
                                    }
                                    else if (name == "home")
                                    {
                                        siteHome = value;
                                    }
                                    else if (name == "player")
                                    {
                                        player = value;
                                    }
                                }
                            }
                        }

                        topMusicSites.Add(new TopMusicSite(siteName, siteHost, siteHome, player));
                    }

                    this.topMusicSites = topMusicSites;
                }
                catch (Exception ex)
                {
                    logger.Error("failed to load topmusic-sites.xml：" + ex.Message, ex);
                }
            }
        }

        private void FormTopMusic_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.userExit || this.appExit)
            {
                UnregisterHotKey(this.Handle, HOTKEY_CTRL_R);
                UnregisterHotKey(this.Handle, HOTKEY_CTRL_N);
                UnregisterHotKey(this.Handle, HOTKEY_F12);

                this.remoteCommandExecutor.Stop();
                this.timerHeartbeat.Stop();

                this.systray.Visible = false;
            }
            else
            {
                e.Cancel = true;

                this.systray.Visible = true;
                this.systray.ShowBalloonTip(500);
                this.Hide();
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_HOTKEY)
            {
                int hk = m.WParam.ToInt32();
                switch (hk)
                {
                    case HOTKEY_CTRL_N:
                        //New IE
                        this.webBrowser.CreateNewIE();
                        break;
                    case HOTKEY_CTRL_R:
                        this.webBrowser.Refresh2(true);
                        break;
                    case HOTKEY_F12:
                        // showConsoleForm
                        showConsoleForm();
                        break;
                }
            }

            base.WndProc(ref m);
        }

        private void showConsoleForm() 
        {
            if (this.formConsole == null)
            {
                this.formConsole = new FormConsole(this);
                this.formConsole.Show();
            }
            else if (!this.formConsole.Visible) {
                this.formConsole.Visible = true;
            }
        }

        private void webBrowser_UpdateLocation(string locationURL)
        {
            this.lastLocation = this.txtLocation.Text;
            this.txtLocation.Text = locationURL;
        }

        private void webBrowser_ShowScriptErrors(System.Collections.Generic.List<BurningICE.WebBrowserEx.ScriptError> scriptErrors)
        {
            if (scriptErrors != null && scriptErrors.Count > 0)
            {
                if (this.scriptErrorForm == null)
                {
                    this.scriptErrorForm = new FormScriptError(this);
                }

                this.scriptErrorForm.ShowScriptErrors(scriptErrors);
            }
        }

        private void webBrowser_OnNewWindowCreated(object sender)
        {
            this.txtLocation.Focus();
            this.txtLocation.SelectionStart = 0;
            this.txtLocation.SelectionLength = this.txtLocation.Text.Length;
        }

        private void webBrowser_OnBeginNavigate(object sender, string url, WebBrowserNavigatingEventArgs e)
        {
            //if(this.skReady)
            //    this.windowsWaitingForScript.Add(sender);
            Console.WriteLine("begin navigate " + url);
            if (url == null || url == "javascript:false;")
            {
                Console.WriteLine("cancel " + url);
                e.Cancel = true;
            }
            /*
            if (this.Visible)
            {
                this.panelWebBrowser.Enabled = false;
            }
            */
        }

        private void webBrowser_OnDocumentComplete(object sender, string url, WebBrowserDocumentCompletedEventArgs e)
        {
            /*
            AxSHDocVw.AxWebBrowser window = (AxSHDocVw.AxWebBrowser)sender;
            this.execExternalScript(window, url);
            mshtml.HTMLDocument doc = (mshtml.HTMLDocument)window.Document;
            string innerHTML = doc.documentElement.innerHTML;
            */
            Console.WriteLine("document complete: " + url);
            try
            {
                WebBrowser window = (WebBrowser)sender;
                mshtml.HTMLDocument doc = (mshtml.HTMLDocument)window.Document.DomDocument;
                this.InjectScript(window, url, -1);

                int framesCount = doc.frames.length;
                for (int i = 0; i < framesCount; ++i)
                {
                    object indexRef = i;
                    try
                    {
                        /*
                        mshtml.IHTMLWindow2 frameWindow = (mshtml.IHTMLWindow2)doc.frames.item(ref indexRef);
                        string frameUrl = frameWindow.location.href;
                         */
                        InjectScript(window, url, i);
                    }
                    catch (Exception ex1)
                    {
                        logger.Error("error to inject script to frame#" + i + " of " + url + ": " + ex1.Message, ex1);
                    }
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine("error while on document complete for url " + url + ": " + ex.Message);
            }
            catch (Exception ex)
            {
                logger.Error("error while on document complete for url " + url + ": " + ex.Message, ex);
            }
            /*
            if (this.Visible)
            {
                this.panelWebBrowser.Enabled = true;
            }
            */
        }

        private void InjectScript(WebBrowser window, string url, int frameIndex) 
        {
            if (url == null || (!url.ToLower().StartsWith("http://") && !url.ToLower().StartsWith("https://")))
            {
                return;
            }
            
            string uri = url;
            int pend = uri.IndexOfAny(new char[] { '?', '&', '#' });
            if (pend != -1)
            {
                uri = uri.Substring(0, pend);
            }

            int p0 = uri.IndexOf("://");
            if (p0 != -1)
            {
                uri = uri.Substring(p0 + 3);
            }

            if (uri.EndsWith("/"))
            {
                uri = uri.Substring(0, uri.Length - 1);
            }

            p0 = uri.IndexOf('/');
            string host = (p0 == -1 ? uri : uri.Substring(0, p0));
            if (host.IndexOf(':') != -1)
            {
                // trunc port
                host = host.Substring(0, host.IndexOf(':'));
            }
            int pstart = uri.LastIndexOf('/');
            string uriFileName = (pstart == -1 ? "" : uri.Substring(pstart + 1));

            // 检查是否需要自动加载外部脚本
            TopMusicSite topMusicSite = GetTopMusicSiteConfig(host);
            if (topMusicSite != null)
            {
                // 加载脚本
                string externalScriptPath = TOP_MUSIC_SERVER_URL + "scripts/topmusic.js";
                string externalSiteScriptPath = TOP_MUSIC_SERVER_URL + "scripts/" + topMusicSite.Site + "/topmusic.js";
                this.InjectExternalScript(window, frameIndex, externalScriptPath, false);
                this.InjectExternalScript(window, frameIndex, externalSiteScriptPath, false);
                if (frameIndex == -1)
                {
                    string externalDocumentScriptPath = TOP_MUSIC_SERVER_URL + "scripts/" + topMusicSite.Site + "/" + host + "/" + uriFileName + ".js";
                    this.InjectExternalScript(window, frameIndex, externalDocumentScriptPath, false);
                }
            }
        }

        private void webBrowser_OnDocumentWindowFocus(object sender, mshtml.IHTMLDocument2 doc)
        {
            //this.Focus();
        }

        private void webBrowser_OnFileDownload(object sender, bool activeDocument, ref bool cancel)
        {
            if (!activeDocument)
            {
                Console.WriteLine("cancel downloading");
                cancel = true;
            }
        }

        private void webBrowser_OnWindowClosing(object sender)
        {
            if (this.scriptOnCloseWindow != null && this.scriptOnCloseWindow.Length > 0)
            {
                this.webBrowser.ExecuteScript(this.scriptOnCloseWindow, (WebBrowser)sender);
                // wait for script to execute
                Thread.Sleep(200);
            }
        }

        private void webBrowser_OnWindowClosed(object sender)
        {
            
        }

        // 获取当前站点的配置信息，若无相应配置信息，则返回null
        private TopMusicSite GetTopMusicSiteConfig(string host)
        {
            if (this.topMusicSites == null)
                return null;

            foreach(TopMusicSite topMusicSite in this.topMusicSites)
            {
                if (host.Contains(topMusicSite.Host))
                {
                    return topMusicSite;
                }
            }

            return null;
        }

        private void InjectExternalScript(WebBrowser window, int frameIndex, string src, bool cache)
        {
            if (!cache)
            {
                src += (src.IndexOf("?") == -1 ? "?t=" : "&t=") + System.DateTime.Now.Ticks;
            }
            /*
            string code = "var sxobj=document.createElement(\"script\");sxobj.type=\"text/javascript\";sxobj.src=\"" + src + "\";var headNodes = document.getElementsByTagName(\"HEAD\");var sxParentNode = (headNodes == undefined ? document.body : headNodes[0]);sxParentNode.appendChild(sxobj);";
             */
            try
            {
                mshtml.HTMLDocument doc = (mshtml.HTMLDocument)window.Document.DomDocument;
                string charset = doc.charset;
                Encoding encoding = Encoding.UTF8;
                if (charset != null)
                {
                    charset = charset.ToUpper();
                    if (charset == "GBK" || charset == "GB2312")
                    {
                        encoding = GBK;
                    }
                }
                string code = HttpUtil.Get(src, encoding);
                if (code != null)
                {
                    this.webBrowser.ExecuteScript(code, "javascript", window, frameIndex);
                }
            }
            catch
            {
                Console.WriteLine("error to exec script: " + src);
            }
        }

        private static string readFile(string path, Encoding encoding)
        {
            if (!System.IO.File.Exists(path))
                return "";

            System.IO.StreamReader reader = null;
            try
            {
                reader = new System.IO.StreamReader(path, encoding);
                string text = reader.ReadToEnd();
                return text;
            }
            catch
            {
                return "";
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }
        }

        public string LogConsole(string text)
        {
            if (this.formConsole != null && this.formConsole.Visible)
            {
                this.formConsole.LogConsole(text);
            }

            return text;
        }

        public void Navigate(string url)
        {
            this.webBrowser.NewWindow(url);
        }

        public void CloseWindow(string urlRegex)
        {
            Regex regex = new Regex(urlRegex, RegexOptions.Singleline);
            ICollection<WebBrowser> windows = this.webBrowser.GetWebBrowserWindows();
            if (windows != null)
            { 
                foreach(WebBrowser win in windows)
                {
                    string url = win.Url.ToString();
                    if (regex.IsMatch(url))
                    {
                        this.webBrowser.CloseWindow(win);
                    }
                }
            }
        }

        private void systray_DoubleClick(object sender, EventArgs e)
        {   
            NotifyIcon notifyIcon = (NotifyIcon)sender;
            notifyIcon.Visible = false;
            this.Show();
            this.WindowState = FormWindowState.Normal;
            //this.panelWebBrowser.Enabled = true;
            this.webBrowser.Visible = true;
        }

        private void TopMusic_Activated(object sender, EventArgs e)
        {
            if (!this.Visible)
            {
                // Webbrowser控件的BUG，当窗口可能被偷焦点（例如IE中执行windows.focus()脚本），此时状态不正常，需强行将窗口推至后台
                /*
                this.Show();
                this.webBrowser.Width = this.panelWebBrowser.Width;
                this.webBrowser.Height = this.panelWebBrowser.Height;
                this.webBrowser.Visible = true;
                 */
                Console.WriteLine("activated imnormally, hide the window.");
                this.Hide();
                this.systray.Visible = true;
            }
        }

        /**
         * 检查主程序TopMusic.exe的心跳，若进程异常终止，主动启动主程序
         */
        private void timerHeartbeat_Tick(object sender, EventArgs e)
        {
            string startupPath = Application.StartupPath + (Application.StartupPath.EndsWith("\\") ? "" : "\\");
            // update.undone被主程序重命名为update.restart，表明Updater程序已主动退出等待更新并重启
            string restartFile = startupPath + "updates\\TopMusicUpdate\\update.restart";
            bool updateFilesCopied = true;
            if (File.Exists(restartFile))
            {
                // apply update for TopMusic.exe
                string[] updateFiles = Directory.GetFiles(startupPath + "updates\\TopMusicUpdate");
                foreach (string updateFile in updateFiles)
                {
                    string fileName = updateFile;
                    int delimiter = fileName.LastIndexOf('\\');
                    if (delimiter >= 0)
                    {
                        fileName = fileName.Substring(delimiter + 1);
                    }

                    if (!fileName.Equals("update.restart"))
                    {
                        // copy update files
                        try
                        {
                            string destFile = startupPath + fileName;
                            string dir = Path.GetDirectoryName(destFile);
                            if (!Directory.Exists(dir))
                            {
                                Directory.CreateDirectory(dir);
                            }
                            File.Copy(updateFile, destFile, true);
                        }
                        catch (Exception ex)
                        {
                            updateFilesCopied = false;
                            logger.Error("failed to copy file " + fileName + " to update: " + ex.Message, ex);
                        }
                    }
                }

                if (updateFilesCopied)
                {
                    File.Move(restartFile, startupPath + "updates\\TopMusicUpdate\\update.done");
                    // update successfully
                    logger.Info("TopMusicUpdate.exe updated successfully.");
                }
            }

            Process[] processes = Process.GetProcessesByName("TopMusicUpdate");
            if (processes == null || processes.Length == 0)
            {
                logger.Info("starting TopMusicUpdate.exe");
                Process p = Process.Start(startupPath + "TopMusicUpdate.exe");
                logger.Info("TopMusicUpdate.exe started with pid=" + p.Id);
            }

            // 检查主程序是否需要更新，文件update.undone文件，若存在此文件，表明需要升级，将其重命名为update.restart，并退出等待Updater程序copy升级文件（Updater拷贝完升级文件后会主动启动主程序）
            if (!this.remoteCommandExecutor.IsReconnecting) // 如果正在重连网络，不能重启，否则重启后将无法联网
            { 
                string undoneFile = startupPath + "updates\\TopMusic\\update.undone";
                if (updateFilesCopied && File.Exists(undoneFile))   // updateFilesCopied为false表明Updater程序暂未完全退出，先不重启
                {
                    logger.Info("restart TopMusic for update");
                    this.appExit = true;
                    File.Move(undoneFile, startupPath + "updates\\TopMusic\\update.restart");
                    Application.Exit();
                }
            }

            // 回收内存清理内存泄露
            if (!this.remoteCommandExecutor.IsReconnecting && this.remoteCommandExecutor.RunningCommandCount == 0)
            {
                long now = DateTime.Now.Ticks;
                if (lastGCTime == 0L || now - lastGCTime > GC_INTERVAL)
                {
                    logger.Debug("gc...");
                    GC.Collect();
                    GC.WaitForPendingFinalizers();

                    EmptyWorkingSet(Process.GetCurrentProcess().Handle);
                    logger.Debug("finished gc.");
                    this.lastGCTime = now;
                }
            }
        }
    }
}
