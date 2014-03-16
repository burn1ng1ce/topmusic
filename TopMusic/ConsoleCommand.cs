using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace TopMusic
{
    class ConsoleCommand : BurningICE.WebBrowserEx.IExternalCommand
    {
        private TopMusic parent;
        public ConsoleCommand(TopMusic parent)
        {
            this.parent = parent;
        }

        public string ExecuteCommand(string text)
        {
            if (this.parent != null)
            {
                this.parent.LogConsole(text);
            }
            
            return text;
        }
    }
}
