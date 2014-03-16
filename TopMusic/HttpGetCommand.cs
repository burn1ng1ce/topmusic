using System;
using System.Collections.Generic;
using System.Text;

namespace TopMusic
{
    public class HttpGetCommand : BurningICE.WebBrowserEx.IExternalCommand
    {
        public string ExecuteCommand(string url)
        {
            if (url == null || url.Length == 0)
                return null;

            return HttpUtil.Get(url);
        }
    }
}
