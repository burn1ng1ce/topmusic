using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.ComponentModel;
using BurningICE.WebBrowserEx;

namespace TopMusic
{
    public class RemoteCommandExecutor
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(typeof(RemoteCommandExecutor));
        private string dispatcherUrl;
        private ICollection<TopMusicSite> topMusicSites;
        private WebBrowserEx webBrowser;
        private Queue<RemoteCommand> waitingCommands;
        private List<RemoteCommand> runningCommands;
        private string rasEntryName;
        private Timer timer;
        private int playedCount = 0;
        private bool reconnecting = false;

        [Description("Fires when command played")]
        public delegate void CommandPlayedHandler(RemoteCommand command);
        [Description("Fires when command played")]
        public event CommandPlayedHandler OnCommandPlayed;

        public RemoteCommandExecutor(string dispatcherUrl, string rasEntryName, ICollection<TopMusicSite> topMusicSites, WebBrowserEx webBrowser)
        {
            this.dispatcherUrl = dispatcherUrl;
            this.rasEntryName = rasEntryName;
            this.topMusicSites = topMusicSites;
            this.webBrowser = webBrowser;

            this.waitingCommands = new Queue<RemoteCommand>();
            this.runningCommands = new List<RemoteCommand>();
            this.timer = new Timer();
            this.timer.Interval = 10000;
            this.timer.Tick += new EventHandler(timer_Tick);
        }

        public bool CanReconnectRas
        {
            get { return this.rasEntryName != null && this.rasEntryName.Length > 0; }
        }

        public string RasEntryName
        {
            get { return this.rasEntryName; }
            set { this.rasEntryName = value; }
        }

        public int PlayedCount
        {
            get { return this.playedCount; }
        }

        public bool IsReconnecting
        {
            get { return this.reconnecting; }
        }

        public void Start()
        {
            this.timer.Start();
        }

        public void Pause()
        {
            this.timer.Stop();
        }

        public void Resume()
        {
            this.timer.Start();
        }

        public void Stop()
        {
            this.timer.Stop();
            this.timer.Dispose();
        }


        public RemoteCommand GetCurrentCommand(string scope)
        {
            if (this.runningCommands == null)
                return null;

            foreach (RemoteCommand command in this.runningCommands)
            {
                if (command.Scope == scope)
                {
                    return command;
                }
            }

            return null;
        }

        public int RunningCommandCount
        {
            get { return this.runningCommands == null ? 0 : this.runningCommands.Count; }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if (this.waitingCommands.Count == 0)
            { 
                // no more, request from server
                List<RemoteCommand> newCommands = GetRemoteCommands();
                if (newCommands != null)
                {
                    foreach(RemoteCommand command in newCommands) 
                    {
                        this.waitingCommands.Enqueue(command);
                    }
                }
            }

            List<RemoteCommand> runnableCommands = new List<RemoteCommand>();
            /*
            bool shouldReconnect = (this.waitingCommands.Count > 0 && this.waitingCommands.Peek().Type == RemoteCommandType.RECONNECT);
            if (shouldReconnect)
            {
                // 需要断网重连切换IP，等待所有其他指令执行完成再断网
                if (this.runningCommands.Count == 0)
                { 
                    
                }
            }
            else
            { 
            
            }
            */
            // 暂时实现：等待所有指令执行完成，再执行下一批指令
            if (this.runningCommands.Count == 0)
            {
                for (; this.waitingCommands.Count > 0; ) 
                {
                    runnableCommands.Add(this.waitingCommands.Dequeue());
                }
                this.waitingCommands.Clear();
            }

            if (runnableCommands.Count > 0)
            {
                foreach (RemoteCommand command in runnableCommands)
                {
                    if(command.Type != RemoteCommandType.RECONNECT)
                        this.runningCommands.Add(command);

                    logger.Debug("running command: " + command.Type + (command.Type != RemoteCommandType.RECONNECT ? ", site: " + ((AbstractTopMusicRemoteCommand)command).Site : ""));
                    try
                    {
                        if (command.Type == RemoteCommandType.RECONNECT)
                        {
                            this.reconnecting = true;
                        }
                        command.Run();

                        this.reconnecting = false;
                    }
                    catch (Exception ex)
                    {
                        logger.Error("failed to run command: " + command.Type + ", " + ex.Message, ex);
                    }

                    if (command.Type != RemoteCommandType.RECONNECT)
                    {
                        ++this.playedCount;
                        if (OnCommandPlayed != null) 
                        {
                            OnCommandPlayed(command);
                        }
                    }
                }
            }

            // 检查Command的执行时间，决定是否停止
            long now = DateTime.Now.Ticks;
            IList<RemoteCommand> stoppedCommands = new List<RemoteCommand>();
            foreach (AbstractTopMusicRemoteCommand command in this.runningCommands)
            {
                if (now - command.RunTime > command.Timeout * 10000000)
                {
                    logger.Debug("stopping command: " + command.Type + ", site: " + command.Site);
                    command.Stop();
                    stoppedCommands.Add(command);
                }
            }

            foreach(RemoteCommand stoppedCommand in stoppedCommands)
            {
                this.runningCommands.Remove(stoppedCommand);
            }
            
            Application.DoEvents();
        }

        private List<RemoteCommand> GetRemoteCommands()
        {
            logger.Debug("get remote commands...");

            List<RemoteCommand> remoteCommands = null;
            string dispatcherUrl = this.dispatcherUrl;
            if (this.CanReconnectRas) 
            {
                dispatcherUrl += "?rasReconnect=true";
            }

            string remoteCommandsXml = HttpUtil.Get(dispatcherUrl, Encoding.UTF8);
            if (remoteCommandsXml != null && remoteCommandsXml.Length > 0)
            {
                remoteCommands = new List<RemoteCommand>();
                try
                {
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(remoteCommandsXml);
                    XmlNodeList commandNodes = doc.SelectNodes("/commands/command");
                    for (int i = 0; i < commandNodes.Count; i++)
                    {
                        XmlNode node = commandNodes.Item(i);
                        if (node.NodeType != XmlNodeType.Element)
                        {
                            continue;
                        }

                        XmlElement commandNode = (XmlElement)node;
                        string commandType = commandNode.GetAttribute("type");
                        string siteName = null;
                        string song = null;
                        string artist = null;
                        int timeout = 90;
                        int lastTime = 60;

                        
                        XmlNodeList propNodes = commandNode.ChildNodes;
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
                                    if (name == "site")
                                    {
                                        siteName = value;
                                    }
                                    else if (name == "song")
                                    {
                                        song = value;
                                    }
                                    else if (name == "artist")
                                    {
                                        artist = value;
                                    }
                                    else if (name == "lastTime")
                                    {
                                        lastTime = int.Parse(value);
                                    }
                                    else if (name == "timeout")
                                    {
                                        timeout = int.Parse(value);
                                    }
                                }
                            }
                        }

                        if (commandType == RemoteCommandType.RECONNECT)
                        {
                            remoteCommands.Add(new RasReconnectRemoteCommand(this.rasEntryName));
                        }
                        else if (commandType == RemoteCommandType.TOPMUSIC_PLAY)
                        {
                            remoteCommands.Add(new TopMusicPlayRemoteCommand(GetTopMusicSite(siteName), siteName, song, artist, timeout, lastTime, webBrowser));
                        }
                        
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("failed to load topmusic-sites.xml：" + ex.Message, ex);
                }
            }

            logger.Debug("finished get remoted commands with " + (remoteCommands == null ? 0 : remoteCommands.Count) + " items.");
            return remoteCommands;
        }

        private TopMusicSite GetTopMusicSite(string site)
        {
            if (this.topMusicSites == null)
                return null;

            foreach (TopMusicSite topMusicSite in this.topMusicSites)
            {
                if (site == topMusicSite.Site)
                {
                    return topMusicSite;
                }
            }

            return null;
        }
    }
}
