using System;
using System.Collections.Generic;
using System.Text;

namespace TopMusic
{
    public class RemoveCommand : BurningICE.WebBrowserEx.IExternalCommand
    {
        private System.Collections.Hashtable parameters;
        public RemoveCommand(System.Collections.Hashtable parameters)
        {
            this.parameters = parameters;
        }

        public string ExecuteCommand(string param)
        {
            string oldValue = (string)this.parameters[param];
            this.parameters.Remove(param);
            return oldValue;
        }
    }
}
