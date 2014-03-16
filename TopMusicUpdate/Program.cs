using System;
using System.Threading;
using System.Diagnostics;

namespace TopMusicUpdate
{
    static class Program
    {
        private static Logger logger = new Logger(typeof(Program));
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        static void Main()
        {
            // 只允许单进程运行
            Process[] ps = Process.GetProcesses();
            
            Process[] processes = Process.GetProcessesByName("TopMusicUpdate");
            if (processes != null && processes.Length > 1)
            {
                logger.Info("TopMusicUpdate process exists, pid: " + processes[0].Id + ", current pid=" + Process.GetCurrentProcess().Id);
                return;
            }

            UpdateWorker updateWorker = new UpdateWorker();
            Thread workerThread = new Thread(updateWorker.Heartbeat);
            //workerThread.IsBackground = true;
            // Start the worker thread.
            workerThread.Start();

            logger.Info("topmusic-updater started.");

            while (!workerThread.IsAlive)
            {
                Thread.Sleep(1);
            }

            workerThread.Join();
            logger.Info("topmusic-updater exited.");
        }
    }
}
