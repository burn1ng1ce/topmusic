using System;
using System.Collections.Generic;
using System.Text;

namespace TopMusic
{
    public interface RemoteCommandStopCallback
    {
        void OnCommandStopped(RemoteCommand command);
    }
}
