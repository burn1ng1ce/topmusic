using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace BurningICE.WebBrowserEx
{
    [System.Runtime.InteropServices.ComVisible(true)]
    [InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    public interface IExternalInterface
    {
        string exec(string commandName, string param);
    }
}
