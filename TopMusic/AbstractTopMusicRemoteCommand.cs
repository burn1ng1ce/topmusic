using System;
using System.Collections.Generic;
using System.Text;

namespace TopMusic
{
    public abstract class AbstractTopMusicRemoteCommand : RemoteCommand
    {
        private TopMusicSite topMusicSite;
        private string site;	// 音乐基地名称
        private string song;	// 音乐名称
        private string artist;	// 演唱者
        private int timeout;   // 执行时间(秒)

        public AbstractTopMusicRemoteCommand()
        {

	    }

        public AbstractTopMusicRemoteCommand(TopMusicSite topMusicSite, string site, string song, string artist, int timeout)
        {
            this.topMusicSite = topMusicSite;
		    this.site = site;
		    this.song = song;
		    this.artist = artist;
            this.timeout = timeout;
	    }

        public abstract string Type
        {
            get;
        }

        public abstract State State
        {
            get;
        }

        public abstract long RunTime
        {
            get;
        }

        public string Scope
        {
            get { return this.site; }
        }

        public abstract void Run();

        public abstract void Stop();

        public abstract string ToJson();

        public TopMusicSite TopMusicSite
        {
            get { return this.topMusicSite; }
            set { this.topMusicSite = value; }
        }

        public string Site
        {
            get { return this.site; }
            set { this.site = value; }
        }

        public string Song
        {
            get { return this.song; }
            set { this.song = value; }
        }

        public string Artist
        {
            get { return this.artist; }
            set { this.artist = value; }
        }

        public int Timeout
        {
            get { return this.timeout; }
            set { this.timeout = value; }
        }
    }
}
