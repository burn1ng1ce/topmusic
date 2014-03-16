using System;
using System.Collections.Generic;
using System.Text;

namespace TopMusic
{
    class NavigateCommand : BurningICE.WebBrowserEx.IExternalCommand
    {
        private TopMusic parent;
        public NavigateCommand(TopMusic parent)
        {
            this.parent = parent;
        }

        public string ExecuteCommand(string url)
        {
            if (this.parent != null)
            {
                this.parent.Navigate(url);
            }
            
            return null;
        }
    }
}
