using System;
using System.Collections.Generic;
using System.Text;

namespace TopMusic
{
    class CloseWindowCommand : BurningICE.WebBrowserEx.IExternalCommand
    {
        private TopMusic parent;
        public CloseWindowCommand(TopMusic parent)
        {
            this.parent = parent;
        }

        public string ExecuteCommand(string urlRegex)
        {
            if (this.parent != null)
            {
                this.parent.CloseWindow(urlRegex);
            }
            
            return null;
        }
    }
}
