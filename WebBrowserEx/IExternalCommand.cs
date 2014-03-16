using System;
using System.Collections.Generic;
using System.Text;

namespace BurningICE.WebBrowserEx
{
    public interface IExternalCommand
    {
        string ExecuteCommand(string param);
    }
}
