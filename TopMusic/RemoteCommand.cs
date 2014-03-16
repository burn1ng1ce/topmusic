using System;
using System.Collections.Generic;
using System.Text;

namespace TopMusic
{
    public enum State
    { 
        WAITING,
        RUNNING,
        COMPLETED,
        FAILED
    }

    /**
     * 远程指令
     */
    public interface RemoteCommand
    {
        string Type { get; }
        string Scope { get; }
        State State { get; }
        long RunTime { get; }
        void Run();
        string ToJson();
    }
}
