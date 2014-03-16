using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using BurningICE.WebBrowserEx;

namespace TopMusic
{
    /**
     * 通过网页播放
     */
    public class TopMusicPlayRemoteCommand : AbstractTopMusicRemoteCommand
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(typeof(TopMusicPlayRemoteCommand));
        private WebBrowserEx webBrowser;
        private State state;
        private int lastTime;
        private long runTime;
        public TopMusicPlayRemoteCommand() : base()
        { 
        
        }

        public TopMusicPlayRemoteCommand(TopMusicSite topMusicSite, string site, string song, string artist, int timeout, int lastTime, WebBrowserEx webBrowser)
                                            : base(topMusicSite, site, song, artist, timeout)
        {
            this.webBrowser = webBrowser;
            this.lastTime = lastTime;
        }

        public override string Type
        {
            get { return "topmusicplay"; }
        }

        public override State State
        {
            get { return this.state; }
        }

        public override long RunTime
        {
            get { return this.runTime; }
        }

        public int LastTime
        {
            get { return this.lastTime; }
            set { this.lastTime = value; }
        }
        public override void Run()
        {
            if (this.TopMusicSite != null && this.TopMusicSite.Home != null)
            {
                this.state = State.RUNNING;
                this.runTime = DateTime.Now.Ticks;
                this.webBrowser.NewWindow(this.TopMusicSite.Home);
            }
            else
            {
                this.state = State.FAILED;
            }
        }


        public override void Stop()
        {
            ICollection<WebBrowser> windows = this.webBrowser.GetWebBrowserWindows();
            if (windows != null)
            {
                foreach (WebBrowser win in windows)
                {
                    try
                    {
                        if (win != null && win.Url != null)
                        {
                            string url = win.Url.ToString();
                            string uri = url;
                            int pend = uri.IndexOf('?');
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

                            if (host != null && this.TopMusicSite != null && host.Contains(this.TopMusicSite.Host)
                                || (this.TopMusicSite.Player != null && this.TopMusicSite.Player.Length > 0 && url.Contains(this.TopMusicSite.Player)))
                            {
                                this.webBrowser.CloseWindow(win);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error("error to stop command: " + ex.Message, ex);
                    }
                }
            }

            this.state = State.COMPLETED;
        }

        public override string ToJson()
        {
            StringBuilder sb = new StringBuilder("{");
            sb.Append("\"type\":\"").Append(this.Type).Append("\",");
            sb.Append("\"site\":\"").Append(this.Site).Append("\",");
            sb.Append("\"song\":\"").Append(this.Song).Append("\",");
            sb.Append("\"artist\":\"").Append(this.Artist).Append("\",");
            sb.Append("\"timeout\":\"").Append(this.Timeout).Append("\",");
            sb.Append("\"state\":\"").Append(this.State).Append("\"");
            sb.Append("}");

            return sb.ToString();
        }
    }
}
