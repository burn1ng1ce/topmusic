using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using log4net;

namespace TopMusic
{
    static class Program
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(typeof(Program));
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // 单进程运行
            Process[] processes = Process.GetProcessesByName("TopMusic");
            if (processes != null && processes.Length > 1)
            {
                logger.Info("TopMusic process exists, pid: " + processes[0].Id + ", current pid=" + Process.GetCurrentProcess().Id);
                return;
            }

            bool systrayMode = (args.Length > 0 && "tray".Equals(args[0]));
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.ThreadException += Application_ThreadException;
            Application.Run(new TopMusic(systrayMode));
        }

        static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            logger.Error("application will exit by uncaught error: " + (e.Exception == null ? null : e.Exception.Message), e.Exception);
        }
    }
}
