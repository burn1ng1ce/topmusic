using System;
using System.Collections.Generic;
using System.Text;

namespace TopMusic
{
    public class GetCommand : BurningICE.WebBrowserEx.IExternalCommand
    {
        private System.Collections.Hashtable parameters;
        public GetCommand(System.Collections.Hashtable parameters)
        {
            this.parameters = parameters;
        }

        public string ExecuteCommand(string param)
        {
            return (string)this.parameters[param];
        }
    }
}
