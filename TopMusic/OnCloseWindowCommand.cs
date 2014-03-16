using System;
using System.Collections.Generic;
using System.Text;

namespace TopMusic
{
    class OnCloseWindowCommand : BurningICE.WebBrowserEx.IExternalCommand
    {
        private TopMusic parent;
        public OnCloseWindowCommand(TopMusic parent)
        {
            this.parent = parent;
        }

        public string ExecuteCommand(string script)
        {
            if (this.parent != null)
            {
                this.parent.ScriptOnCloseWindow = script;
            }
            
            return null;
        }
    }
}
