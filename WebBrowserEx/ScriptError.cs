using System;
using System.Collections.Generic;
using System.Text;

namespace BurningICE.WebBrowserEx
{
    public class ScriptError
    {
        private string description;
        private String url;
        private int line;

        public ScriptError() { 
        
        }

        public ScriptError(string description, string url, int line) {
            this.description = description;
            this.url = url;
            this.line = line;
        }

        public string Description
        {
            get { return this.description; }
            set { this.description = value; }
        }

        public string Url
        {
            get { return this.url; }
            set { this.url = value; }
        }

        public int Line
        {
            get { return this.line; }
            set { this.line = value; }
        }
    }
}
