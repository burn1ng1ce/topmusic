using System;
using System.Collections.Generic;
using System.Text;

namespace TopMusic
{
    public class PutCommand : BurningICE.WebBrowserEx.IExternalCommand
    {
        private System.Collections.Hashtable parameters;
        public PutCommand(System.Collections.Hashtable parameters)
        {
            this.parameters = parameters;
        }

        public string ExecuteCommand(string param)
        {
            int index = param.IndexOf('=');
            string name;
            string value;
            if (index == -1)
            {
                name = "__default";
                value = param;
            }
            else
            {
                name = param.Substring(0, index);
                value = param.Substring(index + 1);
            }

            this.parameters[name] = value;
            return null;
        }
    }
}
