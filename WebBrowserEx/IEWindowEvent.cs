using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace BurningICE.WebBrowserEx
{
    public class IEWindowEvent
    {
        private WebBrowserEx sender;
        private WebBrowser webBrowser;
        private mshtml.IHTMLDocument2 document;

        public IEWindowEvent(WebBrowserEx sender, WebBrowser webBrowser, mshtml.IHTMLDocument2 document)
        {
            this.sender = sender;
            this.webBrowser = webBrowser;
            this.document = document;
        }

        public void onfocus()
        {
            this.sender.IE_onfocus(this.webBrowser, this.document);
        }

        public void onerror(string description, string url, int line)
        {
            this.sender.IE_onerror(description, url, line, this.webBrowser, this.document);
        }
    }
}
