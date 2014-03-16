using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using mshtml;
using System.ComponentModel.Design;
using System.Runtime.InteropServices; 

namespace BurningICE.WebBrowserEx
{
    public class WebBrowserEx : UserControl
    {
        private WebBrowser activeWindow;		//当前活动窗口
        private const int REFRESH_NORMAL = 0;
        private const int REFRESH_COMPLETELY = 3;
        private const int URLMON_OPTION_USERAGENT = 0x10000001;

        private const string DEFAULT_TITLE = "blank";
        private const int TAB_CLOSE_SIZE = 12;
        private const int TAB_ADD_SIZE = 12;
        private Hashtable hstIEWindows = new Hashtable();
        private Hashtable hstIEScriptErrors = new Hashtable();
        private System.Windows.Forms.TabControl tabIE;
        private System.Windows.Forms.Panel panelStatusBar;
        private System.Windows.Forms.Label labelStatusBar;
        private System.Windows.Forms.Timer timerStatusBar;
        private EventHandler panelStatusBarEventHandler;
        private ExternalInterface externalInterface;

        [DllImport("urlmon.dll", CharSet = CharSet.Ansi)]
        private static extern int UrlMkSetSessionOption(int dwOption, string pBuffer, int dwBufferLength, int dwReserved);

        private Guid cmdGuid = new Guid("ED016940-BD5B-11CF-BA4E-00C04FD70816");
        private enum MiscCommandTarget
        {
            Find = 1,
            ViewSource,
            Options
        }

        public WebBrowserEx()
        {
            InitializeComponentInternal();
            this.externalInterface = new ExternalInterface();
        }

        #region Properties
        public WebBrowser ActiveWindow
        {
            get
            {
                return this.activeWindow;
            }
        }
        #endregion

        #region Published events
        [Description("Fires when navigate completed")]
        public delegate void BeginNavigateHandler(object sender, string url, WebBrowserNavigatingEventArgs e);
        [Description("Fires when navigate completed")]
        public event BeginNavigateHandler OnBeginNavigate;
        [Description("Fires when navigate completed")]
        public delegate void NavigateCompleteHandler(object sender, string url, WebBrowserNavigatedEventArgs e);
        [Description("Fires when navigate completed")]
        public event NavigateCompleteHandler OnNavigateComplete;
        [Description("Fires when document completed")]
        public delegate void DocumentCompleteHandler(object sender, string url, WebBrowserDocumentCompletedEventArgs e);
        [Description("Fires when document completed")]
        public event DocumentCompleteHandler OnDocumentComplete;
        [Description("Fires when document window focused")]
        public delegate void DocumentWindowFocusHandler(object sender, IHTMLDocument2 document);
        [Description("Fires when document window focused")]
        public event DocumentWindowFocusHandler OnDocumentWindowFocus;
        [Description("Fires when script error occured")]
        public delegate void DocumentScriptErrorHandler(object sender, string description, string url, int line, IHTMLDocument2 document);
        [Description("Fires when script error occured")]
        public event DocumentScriptErrorHandler OnDocumentScriptError;

        public delegate void ShowScriptErrorsHandler(System.Collections.Generic.List<ScriptError> scriptErrors);
        public event ShowScriptErrorsHandler ShowScriptErrors;

        public delegate void UpdateLocationHandler(string locationURL);
        public event UpdateLocationHandler UpdateLocation;

        public delegate void NewWindowCreatedHandler(object sender);
        public event NewWindowCreatedHandler OnNewWindowCreated;

        [Description("Fires when window closing")]
        public delegate void WindowClosingHandler(object sender);
        [Description("Fires when window closing")]
        public event WindowClosingHandler OnWindowClosing;
        [Description("Fires when window closed")]
        public delegate void WindowClosedHandler(object sender);
        [Description("Fires when window closed")]
        public event WindowClosedHandler OnWindowClosed;

        [Description("Fires before file download")]
        public event FileDownloadHandler OnFileDownload;
        [Description("Fires before file download")]
        public delegate void FileDownloadHandler(object sender, bool activeDocument, ref bool cancel);

        #endregion
        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.activeWindow = null;
            }
            base.Dispose(disposing);
        }

        private void InitializeComponentInternal()
        {
            this.tabIE = new System.Windows.Forms.TabControl();
            this.SuspendLayout();
            this.tabIE.SuspendLayout();
            // 
            // tabIE
            // 
            this.tabIE.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabIE.DrawMode = System.Windows.Forms.TabDrawMode.OwnerDrawFixed;
            this.tabIE.ItemSize = new System.Drawing.Size(128, 17);
            this.tabIE.Location = new System.Drawing.Point(0, 0);
            this.tabIE.Name = "tabIE";
            this.tabIE.SelectedIndex = 0;
            this.tabIE.Size = new System.Drawing.Size(792, 513);
            this.tabIE.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.tabIE.TabIndex = 1;
            this.tabIE.DoubleClick += new System.EventHandler(this.tabIE_DoubleClick);
            this.tabIE.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.tabIE_DrawItem);
            this.tabIE.MouseDown += new System.Windows.Forms.MouseEventHandler(this.tabIE_MouseDown);
            this.tabIE.SelectedIndexChanged += new System.EventHandler(this.tabIE_SelectedIndexChanged);
            this.tabIE.Padding = new Point(TAB_CLOSE_SIZE, TAB_CLOSE_SIZE);
            this.Controls.Add(this.tabIE);

            this.panelStatusBar = new Panel();
            this.labelStatusBar = new Label();

            this.panelStatusBar.SuspendLayout();
            this.panelStatusBar.Dock = System.Windows.Forms.DockStyle.None;

            this.panelStatusBar.Name = "panelStatusBar";
            //this.panelStatusBar.Size = new System.Drawing.Size(200, 24);
            this.panelStatusBar.Width = 450;
            this.panelStatusBar.Height = 18;
            this.panelStatusBar.Location = new Point(2, this.Height - this.panelStatusBar.Height - 4);
            //this.panelStatusBar.Padding = new Padding(3, 4, 2, 0);
            this.panelStatusBar.BackColor = Color.FromArgb(223, 223, 223);
            this.panelStatusBar.BackgroundImageLayout = ImageLayout.None;
            this.panelStatusBar.TabIndex = 1000;
            this.panelStatusBar.Visible = false;
            this.panelStatusBarEventHandler = new EventHandler(this.panelStatusBar_DoubleClick);
            //this.panelStatusBar.DoubleClick += new System.EventHandler(this.panelStatusBar_DoubleClick);

            this.labelStatusBar.Width = this.panelStatusBar.Width - 20;
            this.labelStatusBar.Height = 18;
            this.labelStatusBar.Location = new Point(3, 3);
            this.labelStatusBar.Dock = DockStyle.None;
            this.labelStatusBar.ForeColor = Color.FromArgb(96, 96, 96);
            this.panelStatusBar.Controls.Add(this.labelStatusBar);

            this.Controls.Add(this.panelStatusBar);

            // 
            // WebBrowserEx
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Name = "WebBrowserEx";

            this.panelStatusBar.ResumeLayout(false);
            this.tabIE.ResumeLayout(false);
            this.ResumeLayout(false);

            this.timerStatusBar = new Timer();
            this.timerStatusBar.Interval = 2000;
            this.timerStatusBar.Tick += new EventHandler(this.timerStatusBar_Tick);

            this.MouseClick += new MouseEventHandler(this.WebBrowserEx_MouseClick);
            this.DoubleClick += new EventHandler(WebBrowserEx_DoubleClick);
            this.Resize += new System.EventHandler(this.WebBrowserEx_Resize);
        }

        private void WebBrowserEx_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && this.tabIE.TabCount > 0)
            {
                int x = e.X, y = e.Y;

                Rectangle rect = this.tabIE.GetTabRect(this.tabIE.TabCount - 1);
                rect.Offset(rect.Width + 1, 0);
                rect.Width = TAB_ADD_SIZE + 12;

                if (x > rect.X && x < rect.Right && y > rect.Y && y < rect.Bottom)
                {
                    this.CreateNewIE();
                }
            }
        }

        private void WebBrowserEx_DoubleClick(object sender, System.EventArgs e)
        {
            //双击新建空白窗体
            this.CreateNewIE();
        }

        #region Events of IE
        private void IE_NewWindow2(ref object ppDisp, ref bool Cancel)
        {
            //将页面显示到新窗口中
            try
            {
                WebBrowser newWindow = this.NewWindow();
                SHDocVw.WebBrowser axNewWIndow = (newWindow.ActiveXInstance as SHDocVw.WebBrowser);
                axNewWIndow.RegisterAsBrowser = true;
                ppDisp = axNewWIndow.Application;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception in IE_NewWindow2: " + ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        private void IE_BeforeNavigate2(object sender, WebBrowserNavigatingEventArgs e)
        {
            if (this.OnBeginNavigate != null) {
                this.OnBeginNavigate(sender, (string)e.Url.ToString(), e);
            }
        }

        void IE_TitleChange(string caption)
        {
            try
            {
                TabPage targetTabPage = (TabPage)this.hstIEWindows[this.activeWindow];
                if (targetTabPage != null)
                {
                    //限制标题的长度，避免Tab显示过长
                    if (caption == null || caption.Length == 0)
                    {
                        caption = "无标题";
                    }
                    else if (System.Text.Encoding.Default.GetByteCount(caption) > 14)
                    {
                        caption = caption.Substring(0, 4) + ".." + caption.Substring(caption.Length - 4);
                    }

                    targetTabPage.Text = caption;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception in IE_TitleChange: " + ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        private void IE_NavigateComplete(object sender, WebBrowserNavigatedEventArgs e)
        {
            WebBrowser window = (WebBrowser)sender;
            SHDocVw.WebBrowser axWindow = (window.ActiveXInstance as SHDocVw.WebBrowser);
            mshtml.IHTMLDocument2 doc = (axWindow.Document as mshtml.IHTMLDocument2);
            HTMLWindowEvents_Event pwin = (HTMLWindowEvents_Event)doc.parentWindow;
            
            /*
            bool navigateTopWin = true;
            
            mshtml.IHTMLDocument2 doc2 = e.pDisp.GetType().InvokeMember("Document",
                                                                        System.Reflection.BindingFlags.GetProperty,
                                                                        null,
                                                                        e.pDisp,
                                                                        null) as mshtml.IHTMLDocument2;
            if (doc2 != null && doc != doc2)
            {
                navigateTopWin = false;
                pwin = (HTMLWindowEvents_Event)doc2.parentWindow;
                doc = doc2;
            }
            */
            pwin.onerror += new HTMLWindowEvents_onerrorEventHandler(new IEWindowEvent(this, window, doc).onerror);
            pwin.onfocus += new HTMLWindowEvents_onfocusEventHandler(new IEWindowEvent(this, window, doc).onfocus);

            if(this.OnNavigateComplete != null)
                OnNavigateComplete(window, window.Url.ToString(), e);

            //更新Url
            /*
            if (navigateTopWin)
            {
            */
                TabPage tab = (TabPage)this.hstIEWindows[sender];
                if (tab != null)
                {
                    tab.Tag = axWindow.LocationURL;
                    if (tab == this.tabIE.SelectedTab)
                    {
                        if(this.UpdateLocation != null)
                            this.UpdateLocation(axWindow.LocationURL);
                    }
                }
            /* } */
            Application.DoEvents();
        }

        private void IE_DocumentComplete(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (OnDocumentComplete != null)
                OnDocumentComplete((WebBrowser)sender, e.Url == null ? null : e.Url.ToString(), e);
        }

        private void IE_ProgressChange(object sender, WebBrowserProgressChangedEventArgs e)
        {

        }

        private void IE_FileDownload(bool activeDocument, ref bool cancel)
        {
            if (OnFileDownload != null)
                OnFileDownload(this.activeWindow, activeDocument, ref cancel);
        }

        private void IE_StatusTextChange(object sender, System.EventArgs e)
        {
            if (this.timerStatusBar.Enabled)
            {
                this.timerStatusBar.Stop();
            }

            this.SuspendLayout();
            this.panelStatusBar.SuspendLayout();

            string statusText = ((WebBrowser)sender).StatusText;
            if (statusText == "页面发生脚本错误")
            {
                //this.panelStatusBar.Padding = new Padding(16, 3, 2, 2);
                this.labelStatusBar.Left = 16;
                this.panelStatusBar.BackgroundImage = Image.FromFile("ico/Warning.png");
                if (this.panelStatusBar.Tag == null)
                {
                    this.panelStatusBar.DoubleClick += panelStatusBarEventHandler;
                    this.panelStatusBar.Tag = "1";
                }
            }
            else
            {
                //this.panelStatusBar.Padding = new Padding(3, 3, 2, 2);
                this.labelStatusBar.Left = 3;
                this.panelStatusBar.BackgroundImage = null;
                this.timerStatusBar.Start();
                if (this.panelStatusBar.Tag != null)
                {
                    this.panelStatusBar.DoubleClick -= panelStatusBarEventHandler;
                    this.panelStatusBar.Tag = null;
                }
            }

            this.labelStatusBar.Text = statusText;
            this.panelStatusBar.Visible = true;
            this.panelStatusBar.BringToFront();

            this.panelStatusBar.ResumeLayout(true);
            this.ResumeLayout(false);
        }

        internal void IE_onerror(string description, string url, int line,
                                                        WebBrowser webBrowser, IHTMLDocument2 document)
        {
            System.Collections.Generic.List<ScriptError> scriptErrors = (System.Collections.Generic.List<ScriptError>)this.hstIEScriptErrors[webBrowser];
            if (scriptErrors == null)
            {
                scriptErrors = new System.Collections.Generic.List<ScriptError>(8);
                this.hstIEScriptErrors[webBrowser] = scriptErrors;
            }
            scriptErrors.Add(new ScriptError(description, url, line));
            ((activeWindow.ActiveXInstance as SHDocVw.WebBrowser).Document as mshtml.IHTMLDocument2).parentWindow.status = "页面发生脚本错误";
            ((IHTMLEventObj)document.parentWindow.@event).returnValue = true;
            if(this.OnDocumentScriptError != null)
                this.OnDocumentScriptError(webBrowser, description, url, line, document);
        }

        internal void IE_onfocus(WebBrowser webBrowser, IHTMLDocument2 document)
        {
            if(this.OnDocumentWindowFocus != null)
                this.OnDocumentWindowFocus(webBrowser, document);
        }

        private void tabIE_DoubleClick(object sender, System.EventArgs e)
        {
            CloseWindow();
        }

        private void panWorkArea_DoubleClick(object sender, System.EventArgs e)
        {
            //双击新建空白窗体
            this.CreateNewIE();
        }

        private void tabIE_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            //激活当前控件
            this.activeWindow = null;
            if (this.tabIE.SelectedTab != null)
            {
                foreach (WebBrowser key in this.hstIEWindows.Keys)
                {
                    if (hstIEWindows[key] == this.tabIE.SelectedTab)
                    {
                        this.activeWindow = key;

                        //同步Url
                        if (this.UpdateLocation != null)
                        {
                            this.UpdateLocation((string)this.tabIE.SelectedTab.Tag);
                        }
                        break;
                    }
                }
            }
        }

        private void tabIE_DrawItem(object sender, DrawItemEventArgs e)
        {
            try
            {
                Rectangle rect = this.tabIE.GetTabRect(e.Index);
                Color bgColor = (e.State == DrawItemState.Selected ? Color.White : SystemColors.Control);
                e.Graphics.FillRectangle(new SolidBrush(bgColor), rect);
                e.Graphics.DrawString(this.tabIE.TabPages[e.Index].Text,
                                        this.Font,
                                        SystemBrushes.ControlText,
                                        rect.X + 2, rect.Y + 2);
                rect.Offset(rect.Width - TAB_CLOSE_SIZE - 3, 2);
                rect.Width = TAB_CLOSE_SIZE;
                rect.Height = TAB_CLOSE_SIZE;
                if (e.State == DrawItemState.Selected)
                {
                    e.Graphics.DrawImage(Image.FromFile("ico/Close.png"), rect);
                }
                else
                {
                    Pen pen = new Pen(Color.FromArgb(157, 157, 157));
                    //"\"线   
                    Point p1 = new Point(rect.X + 3, rect.Y + 3);
                    Point p2 = new Point(rect.X + rect.Width - 3, rect.Y + rect.Height - 3);
                    e.Graphics.DrawLine(pen, p1, p2);

                    //"/"线   
                    Point p3 = new Point(rect.X + 3, rect.Y + rect.Height - 3);
                    Point p4 = new Point(rect.X + rect.Width - 3, rect.Y + 3);
                    e.Graphics.DrawLine(pen, p3, p4);
                }

                if (e.Index + 1 == this.tabIE.TabPages.Count) {
                    rect.Offset(TAB_CLOSE_SIZE + 10, 2);
                    rect.Width = TAB_ADD_SIZE;
                    rect.Height = TAB_ADD_SIZE;
                    e.Graphics.DrawImage(Image.FromFile("ico/Add.png"), rect);
                }

                e.Graphics.Dispose();
            }
            catch
            {

            }
        }

        private void tabIE_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                int x = e.X, y = e.Y;

                Rectangle rect = this.tabIE.GetTabRect(this.tabIE.SelectedIndex);

                rect.Offset(rect.Width - TAB_CLOSE_SIZE - 3, 2);
                rect.Width = TAB_CLOSE_SIZE;
                rect.Height = TAB_CLOSE_SIZE;

                if (x > rect.X && x < rect.Right && y > rect.Y && y < rect.Bottom)
                {
                    this.CloseWindow();
                }
            }
        }

        private void panelStatusBar_DoubleClick(object sender, System.EventArgs e)
        {
            System.Collections.Generic.List<ScriptError> scriptErrors = (System.Collections.Generic.List<ScriptError>)this.hstIEScriptErrors[this.activeWindow];
            if (scriptErrors != null && scriptErrors.Count > 0)
            {
                /*
                if (this.scriptErrorForm == null)
                {
                    this.scriptErrorForm = new FormScriptError(this);
                }

                this.scriptErrorForm.ShowScriptErrors(scriptErrors);
                */
                if(this.ShowScriptErrors != null)
                    this.ShowScriptErrors(scriptErrors);
            }
        }

        private void timerStatusBar_Tick(object sender, System.EventArgs e)
        {
            this.SuspendLayout();
            this.labelStatusBar.Text = "";
            this.panelStatusBar.SendToBack();
            this.panelStatusBar.Visible = false;
            this.ResumeLayout(false);
            this.timerStatusBar.Stop();
        }
        #endregion


        #region 方法
        public void RegisterCommand(string name, IExternalCommand command)
        {
            if (this.externalInterface == null)
            {
                this.externalInterface = new ExternalInterface();
            }

            this.externalInterface.RegisterCommand(name, command);
        }

        public void Navigate(string url)
        {
            this.Navigate(url, this.tabIE.SelectedTab);
        }

        public void Navigate(string url, TabPage target)
        {
            foreach (WebBrowser key in this.hstIEWindows.Keys)
            {
                if (hstIEWindows[key] == target)
                {
                    Navigate(url, key);
                    target.Tag = url;
                    break;
                }
            }
        }

        public void Navigate(string url, WebBrowser target)
        {
            this.Navigate(url, target, null);
        }

        public void Navigate(string url, WebBrowser target, string postData)
        {
            //默认http协议
            object _url = null;
            object _null = string.Empty;
            object headers = string.Empty;
            object bPostData = string.Empty;

            if (url == null)
                url = "about:blank";

            string protocol = null;
            int pi = url.IndexOf(':');
            if (pi != -1)
            {
                protocol = url.Substring(0, pi).ToLower();
            }

            if (url != null && url.IndexOf("://") < 0 && !"about".Equals(protocol) && !"javascript".Equals(protocol) && !"vbscript".Equals(protocol))
                url = "http://" + url;

            _url = (object)url;

            if (postData != null && postData.Length > 0)
            {
                headers = "Content-Type:application/x-www-form-urlencoded";
                bPostData = System.Text.Encoding.Default.GetBytes(postData);
            }

            if (target == null)
                return;

            try
            {
                if (target.IsBusy)
                {
                    target.Stop();
                }

                //导航页面
                if (postData != null && postData.Length > 0)
                {
                    (activeWindow.ActiveXInstance as SHDocVw.WebBrowser).Navigate2(ref _url, ref _null, ref _null, ref bPostData, ref headers);
                    //target.Navigate2( ref _url, ref _null, ref _null, ref _null, ref _null);
                }
                else 
                {
                    target.Navigate(url);
                }
                TabPage tab = (TabPage)this.hstIEWindows[target];
                if (tab != null)
                    tab.Tag = url;

                if (!"about:blank".Equals(url))
                {
                    target.Focus();
                }

                //clear script errors for current window
                System.Collections.Generic.List<ScriptError> scriptErrors = (System.Collections.Generic.List<ScriptError>)this.hstIEScriptErrors[target];
                if (scriptErrors != null)
                {
                    scriptErrors.Clear();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception in Navigate: " + ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        public WebBrowser NewWindow(string url)
        {
            WebBrowser newIE = NewWindow();
            //导航到制定Url
            this.Navigate(url, newIE);
            return newIE;
        }

        public WebBrowser NewWindow()
        {
            TabPage newTabPage = new TabPage();
            WebBrowser newIE = new WebBrowser();
            // WebBrowser newIE = new WebBrowser();

            try
            {
                this.SuspendLayout();
                this.tabIE.SuspendLayout();
                //初始化TabPage控件
                newTabPage.SuspendLayout();

                newTabPage.Location = new System.Drawing.Point(2, 21);
                newTabPage.Name = "tabPage1";
                newTabPage.Size = new System.Drawing.Size(this.tabIE.Width, this.tabIE.Height);
                newTabPage.TabIndex = 0;
                newTabPage.Text = DEFAULT_TITLE;

                //初始化IE控件
                // ((System.ComponentModel.ISupportInitialize)(newIE)).BeginInit();
                newIE.SuspendLayout();
                // newIE.ContainingControl = this;
                // newIE.Enabled = true;
                newIE.Location = new System.Drawing.Point(0, 0);

                //newIE.OcxState
                newIE.Size = new System.Drawing.Size(672, 324);
                newIE.Dock = System.Windows.Forms.DockStyle.Fill;
                newIE.TabIndex = 0;

                newTabPage.Controls.Add(newIE);
                this.tabIE.Controls.Add(newTabPage);

                // ((System.ComponentModel.ISupportInitialize)(newIE)).EndInit();
                newIE.ResumeLayout(false);
                newTabPage.ResumeLayout(false);
                this.tabIE.ResumeLayout(false);
                this.ResumeLayout(false);

                //将新建的窗口添加至哈希表
                //AXIE作为key，TabPage作为值
                this.hstIEWindows[newIE] = newTabPage;

                //切换到当前tab
                this.tabIE.SelectedTab = newTabPage;

                //设置活动窗口
                SHDocVw.WebBrowser axNewIE = (newIE.ActiveXInstance as SHDocVw.WebBrowser);
                axNewIE.Silent = true;

                this.activeWindow = newIE;

                if (this.OnNewWindowCreated != null)
                {
                    this.OnNewWindowCreated(newIE);
                }

                // attach events
                axNewIE.NewWindow2 += new SHDocVw.DWebBrowserEvents2_NewWindow2EventHandler(IE_NewWindow2);
                // newIE.NewWindow += new System.ComponentModel.CancelEventHandler(IE_NewWindow2);
                // newIE.DocumentTitleChanged += IE_TitleChange;
                axNewIE.TitleChange += new SHDocVw.DWebBrowserEvents2_TitleChangeEventHandler(IE_TitleChange);
                
                newIE.Navigating += new WebBrowserNavigatingEventHandler(IE_BeforeNavigate2);

                newIE.Navigated += new WebBrowserNavigatedEventHandler(IE_NavigateComplete);
                newIE.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(IE_DocumentComplete);

                newIE.ProgressChanged += new WebBrowserProgressChangedEventHandler(IE_ProgressChange);
                newIE.StatusTextChanged += IE_StatusTextChange;
                axNewIE.FileDownload += IE_FileDownload;

                if (this.externalInterface != null)
                {
                    newIE.ObjectForScripting = this.externalInterface;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception in NewWindow: " + ex.Message + "\r\n" + ex.StackTrace);
            }

            //返回刚刚创建的IE控件
            return newIE;
        }

        public void CloseWindow()
        {

            //关闭窗口

            TabPage tab = this.tabIE.SelectedTab;
            if (tab != null)
                CloseWindow(tab);
        }

        public void CloseWindow(TabPage tab)
        {
            try
            {
                bool removed = true;
                foreach (WebBrowser window in this.hstIEWindows.Keys)
                {
                    if (hstIEWindows[window] == tab)
                    {
                        if (this.tabIE.TabCount == 1)
                        {
                            //最后一个tab，不允许移除
                            this.Navigate("about:blank", tab);
                            removed = false;
                        }
                        else
                        {
                            this.hstIEWindows.Remove(window);
                            if (OnWindowClosing != null)
                            {
                                OnWindowClosing(window);
                            }

                            //先解除控件绑定
                            if (tab.Controls.Contains(window))
                                tab.Controls.Remove(window);
                                
                            window.Visible = false;
                            // window.ContainingControl = null;
                            //window.Stop();
                            window.Dispose();

                            if (OnWindowClosed != null)
                            {
                                OnWindowClosed(window);
                            }
                        }
                        break;
                    }
                }

                //tab.Dispose();
                if (removed)
                    this.tabIE.TabPages.Remove(tab);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception in CloseWindow: " + ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        public void CloseWindow(WebBrowser window)
        {
            try
            {
                if (this.hstIEWindows.ContainsKey(window))
                {
                    if (OnWindowClosing != null)
                    {
                        OnWindowClosing(window);
                    }

                    TabPage tab = (TabPage)this.hstIEWindows[window];
                    if (tab.Controls.Contains(window))
                        tab.Controls.Remove(window);
                    //tab.Dispose();
                    this.tabIE.TabPages.Remove(tab);
                }
                
                window.Visible = false;
                // window.ContainingControl = null;

                window.Dispose();

                this.hstIEWindows.Remove(window);

                if (OnWindowClosed != null)
                {
                    OnWindowClosed(window);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception in CloseWindow: " + ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        public void CreateNewIE()
        {
            this.NewWindow("about:blank");
        }
        public void CreateNewIE(string url)
        {
            if (url == null || url.Length == 0)
            {
                this.NewWindow("about:blank");
            }
            else
            {
                this.NewWindow(url);
            }
        }

        public void GoBack()
        {
            try
            {
                if (this.activeWindow != null)
                    this.activeWindow.GoBack();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception in GoBack: " + ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        public void GoForward()
        {
            try
            {
                if (this.activeWindow != null)
                    this.activeWindow.GoForward();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception in GoForward: " + ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        public void Refresh2()
        {
            Refresh2(false);
        }

        public void Refresh2(bool noCache)
        {
            object refreshLevel = noCache ? REFRESH_COMPLETELY : REFRESH_NORMAL;
            try
            {
                if (this.activeWindow == null)
                    return;

                if (this.activeWindow.Url != null && this.activeWindow.Url.ToString() != "")
                    this.activeWindow.Refresh(WebBrowserRefreshOption.Completely);

                //clear script errors for current window
                System.Collections.Generic.List<ScriptError> scriptErrors = (System.Collections.Generic.List<ScriptError>)this.hstIEScriptErrors[activeWindow];
                if (scriptErrors != null)
                {
                    scriptErrors.Clear();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception in Refresh2: " + ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        public void Stop()
        {
            if (this.activeWindow != null && !this.activeWindow.IsBusy)
                this.activeWindow.Stop();
        }

        public void CloseAll()
        {
            //关闭所有的窗口
            try
            {
                foreach (WebBrowser iewin in this.hstIEWindows.Keys)
                {
                    TabPage tab = (TabPage)this.hstIEWindows[iewin];
                    if (this.tabIE.TabCount > 1)
                    {
                        tab.Dispose();
                        this.tabIE.TabPages.Remove(tab);
                        iewin.Visible = false;
                        iewin.Dispose();
                        this.hstIEWindows.Remove(iewin);
                    }
                    else
                    {
                        //最后一个Tab，不允许移除
                        this.Navigate("about:blank");
                    }
                }
                //this.hstIEWindows.Clear();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception in CloseAll: " + ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        public void SetUserAgent(string userAgent)
        {
            if (userAgent != null && userAgent.Length > 0)
                UrlMkSetSessionOption(URLMON_OPTION_USERAGENT, userAgent, userAgent.Length, 0);
        }

        ////////定义ViewSource,Find,InternetOptions方法
        /*
        public void ViewSource()
        {
            IOleCommandTarget cmdt;
            Object o = new object();
            try
            {
                cmdt = (IOleCommandTarget)this.activeWindow.Document;
                cmdt.Exec(ref cmdGuid, (uint)MiscCommandTarget.ViewSource,
                    (uint)SHDocVw.OLECMDEXECOPT.OLECMDEXECOPT_DODEFAULT, ref o, ref o);
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.Message);
            }
        }

        public void Find()
        {
            IOleCommandTarget cmdt;
            Object o = new object();
            try
            {
                cmdt = (IOleCommandTarget)this.activeWindow.Document;
                cmdt.Exec(ref cmdGuid, (uint)MiscCommandTarget.Find,
                    (uint)SHDocVw.OLECMDEXECOPT.OLECMDEXECOPT_DODEFAULT, ref o, ref o);
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.Message);
            }
        }

        public void InternetOptions()
        {
            IOleCommandTarget cmdt;
            Object o = new object();
            try
            {
                cmdt = (IOleCommandTarget)this.activeWindow.Document;
                cmdt.Exec(ref cmdGuid, (uint)MiscCommandTarget.Options,
                    (uint)SHDocVw.OLECMDEXECOPT.OLECMDEXECOPT_DODEFAULT, ref o, ref o);
            }
            catch
            {
                // NOTE: Because of the way that this CMDID is handled in Internet Explorer,
                // this catch block will always fire, even though the dialog box
                // and its operations completed successfully. You can suppress this
                // error without causing any damage to your host.
            }
        }
        */

        public void ExecuteScript(string code) {
            ExecuteScript(code, "javascript");
        }

        public void ExecuteScript(string code, string language)
        {
            ExecuteScript(code, language, this.activeWindow);
        }

        public void ExecuteScript(string code, WebBrowser window)
        {
            ExecuteScript(code, "javascript", window);
        }
        public void ExecuteScript(string code, string language, WebBrowser window)
        {
            string frameName = null;

            if (code != null)
            {
                code = code.Trim();
                if (code.StartsWith("//@frame="))
                {
                    frameName = code.Substring(9, code.IndexOfAny(new char[] { '\r', '\n' }) - 9).Trim();
                }
            }

            ExecuteScript(code, language, window, frameName);
        }

        public void ExecuteScript(string code, string language, WebBrowser window, string frameName)
        {
            if (window == null)
            {
                window = this.activeWindow;
            }
            if (window != null && code != null && code.Length > 0)
            {
                try
                {
                    mshtml.IHTMLDocument2 doc = (mshtml.IHTMLDocument2)window.Document.DomDocument;
                    if (frameName == null)
                    {
                        doc.parentWindow.execScript(code, language);
                    }
                    else
                    {
                        int framesCount = doc.frames.length;
                        for (int i = 0; i < framesCount; ++i)
                        {
                            object indexRef = i;
                            mshtml.IHTMLWindow2 frameWindow = (mshtml.IHTMLWindow2)doc.frames.item(ref indexRef);
                            if (frameWindow.name == frameName)
                            {
                                frameWindow.execScript(code, language);
                                break;
                            }
                        }
                    }
                }
                catch (UnauthorizedAccessException e)
                {
                    Console.WriteLine("Exception when exec script: " + e.Message);
                }
                catch (System.Exception e)
                {
                    Console.WriteLine("Exception when exec script: " + e.Message + "\n script: \n" + (code.Length < 100 ? code : code.Substring(0, 100) + "..."));
                }
            }
        }

        public void ExecuteScript(string code, string language, WebBrowser window, int frameIndex)
        {
            if (window == null)
            {
                window = this.activeWindow;
            }
            if (window != null && code != null && code.Length > 0)
            {
                try
                {
                    mshtml.IHTMLDocument2 doc = (mshtml.IHTMLDocument2)window.Document.DomDocument;
                    if (frameIndex < 0)
                    {
                        doc.parentWindow.execScript(code, language);
                    }
                    else
                    {
                        if (frameIndex < doc.frames.length)
                        {
                            object indexRef = frameIndex;
                            mshtml.IHTMLWindow2 frameWindow = (mshtml.IHTMLWindow2)doc.frames.item(ref indexRef);
                            frameWindow.execScript(code, language);
                        }
                    }
                }
                catch (System.Exception e)
                {
                    Console.WriteLine("Exception when exec script: " + e.Message + "\n script: \n" + (code.Length < 100 ? code : code.Substring(0, 100) + "..."));
                }
            }
        }

        public ICollection<WebBrowser> GetWebBrowserWindows()
        {
            List<WebBrowser> windows = new List<WebBrowser>();
            foreach (WebBrowser win in this.hstIEWindows.Keys) {
                windows.Add(win);
            }

            return windows;
        }

        #endregion

        private void WebBrowserEx_Resize(object sender, EventArgs e)
        {
            this.SuspendLayout();
            this.panelStatusBar.SuspendLayout();

            this.panelStatusBar.Width = 450;
            this.panelStatusBar.Top = this.Height - this.panelStatusBar.Height - 4;
            this.labelStatusBar.Width = this.panelStatusBar.Width - 20;

            this.panelStatusBar.ResumeLayout(true);
            this.ResumeLayout(true);

            if (!this.Visible && this.Width > 0)
            {
                this.Visible = true;
                this.Enabled = true;
                this.tabIE.Visible = true;
                this.tabIE.Enabled = true;
                this.tabIE.Width = this.Width;
                this.tabIE.Height = this.Height;
            }
            else if (this.Visible)
            {
                this.tabIE.Width = this.Width;
                this.tabIE.Height = this.Height;
            }

            Console.WriteLine("webbrowser resize: visible: " + this.Visible + ", " + this.Size.Width + ", " + this.Size.Height + ", tab size: " + this.tabIE.Size.Width + ", " + this.tabIE.Size.Height + ", tab visible: " + this.tabIE.Visible + ", tabs: " + this.tabIE.TabPages.Count);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // WebBrowserEx
            // 
            this.Name = "WebBrowserEx";
            this.VisibleChanged += new System.EventHandler(this.WebBrowserEx_VisibleChanged);
            this.ResumeLayout(false);

        }

        private void WebBrowserEx_VisibleChanged(object sender, EventArgs e)
        {
            Console.WriteLine("webbrowser visible change to " + this.Visible + ", tabs visible: " + this.tabIE.Visible);
        }
    }
}
