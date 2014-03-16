using System;
using System.Collections.Generic;
using System.Text;

namespace BurningICE.WebBrowserEx
{
    [System.Runtime.InteropServices.ComVisible(true)]
    public class ExternalInterface : IExternalInterface
    {
        private System.Collections.Hashtable hstCommands;

        public ExternalInterface() {
        
        }

        public string exec(string command, string param) {
            if (command == null || this.hstCommands == null)
                return null;

            IExternalCommand cmd = (IExternalCommand)this.hstCommands[command];
            if (cmd == null)
                return null;

            return cmd.ExecuteCommand(param);
        }

        public void RegisterCommand(string name, IExternalCommand command) {
            if (name != null && command != null)
            {
                if (this.hstCommands == null)
                {
                    this.hstCommands = new System.Collections.Hashtable();
                }

                this.hstCommands[name] = command;
            }
        }
    }
}
