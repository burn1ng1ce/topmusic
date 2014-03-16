using System;
using System.Collections.Generic;
using System.Text;

namespace TopMusic
{
    public class TopMusicSite
    {
        private string site;    // 站点名称
        private string host;    // 站点一级域名（或域名关键字），退出任务时将根据host决定关闭哪些页面
        private string home;    // 音乐首页
        private string player;  // 播放器页面（用于判断播放器，暂时不需要）

        public string Site {
            get { return this.site; }
            set { this.site = value; }
        }

        public string Host {
            get { return this.host; }
            set { this.host = value; }
        }

        public string Home
        {
            get { return this.home; }
            set { this.home = value; }
        }

        public string Player
        {
            get { return this.player; }
            set { this.player = value; }
        }

        public TopMusicSite()
        { 
        
        }

        public TopMusicSite(string site, string host, string home, string player)
        {
            this.site = site;
            this.host = host;
            this.home = home;
            this.player = player;
        }
    }
}
