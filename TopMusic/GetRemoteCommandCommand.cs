using System;
using System.Collections.Generic;
using System.Text;

namespace TopMusic
{
    public class GetRemoteCommandCommand : BurningICE.WebBrowserEx.IExternalCommand
    {
        private RemoteCommandExecutor remoteCommandExecutor;

        public GetRemoteCommandCommand(RemoteCommandExecutor remoteCommandExecutor) {
            this.remoteCommandExecutor = remoteCommandExecutor;
        }

        public string ExecuteCommand(string param)
        {
            if(param == null)
                return null;

            RemoteCommand remoteCommand = this.remoteCommandExecutor.GetCurrentCommand(param);
            return remoteCommand.ToJson();
        }
    }
}
