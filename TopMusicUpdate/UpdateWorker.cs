using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using Ionic.Zip;

namespace TopMusicUpdate
{
    public class UpdateWorker
    {
        private const string UPDATE_URL = "http://rpcbeta.networkbench.com/liaoxj/updates/";
        private const int UPDATE_INTERVAL = 600000;
        private const int HEARTBEAT_INTERVAL = 15000;
        private static Logger logger = new Logger(typeof(UpdateWorker));
        private long lastUpdate = 0L;   // 单位: 毫秒
        private volatile bool shutdownNow = false;

        public void Heartbeat()
        {
            for ( ; !shutdownNow; )
            {
                try
                {
                    CheckTopMusicHeartbeat();
                    if (shutdownNow)
                    {
                        break;
                    }

                    long timestamp = DateTime.Now.Ticks / 10000L;
                    if (lastUpdate == 0L || timestamp - lastUpdate > UPDATE_INTERVAL)
                    {
                        CheckUpdate();
                        this.lastUpdate = timestamp;
                    }
                }
                catch(Exception ex)
                {
                    logger.Error("error to check heartbeat: " + ex.Message, ex);
                }
                Thread.Sleep(HEARTBEAT_INTERVAL);
            }
        }

        /**
         * 检查主程序TopMusic.exe的心跳，若进程异常终止，主动启动主程序
         */
        private void CheckTopMusicHeartbeat()
        {
            string startupPath = Application.StartupPath + (Application.StartupPath.EndsWith("\\") ? "" : "\\");
            // update.undone被主程序重命名为update.restart，表明主程序已主动退出等待更新并重启
            string restartFile = startupPath + "updates\\TopMusic\\update.restart";
            bool updateFilesCopied = true;

            if (File.Exists(restartFile))
            { 
                // apply update for TopMusic.exe
                string[] updateFiles = Directory.GetFiles(startupPath + "updates\\TopMusic");
                foreach (string updateFile in updateFiles)
                {
                    string fileName = updateFile;
                    int delimiter = fileName.LastIndexOf('\\');
                    if (delimiter >= 0)
                    {
                        fileName = fileName.Substring(delimiter + 1);
                    }

                    if (!fileName.Equals("update.restart"))
                    { 
                        // copy update files
                        try
                        {
                            string destFile = startupPath + fileName;
                            string dir = Path.GetDirectoryName(destFile);
                            if (!Directory.Exists(dir))
                            {
                                Directory.CreateDirectory(dir);
                            }
                            File.Copy(updateFile, destFile, true);
                        }
                        catch (Exception ex)
                        {
                            updateFilesCopied = false;
                            logger.Error("failed to copy file " + fileName + " to update: " + ex.Message, ex);
                        }
                    }
                }

                if (updateFilesCopied)
                {
                    File.Move(restartFile, startupPath + "updates\\TopMusic\\update.done");
                    // update successfully
                    logger.Info("TopMusic.exe updated successfully.");
                }
            }

            Process[] processes = Process.GetProcessesByName("TopMusic");
            if (processes == null || processes.Length == 0)
            {
                logger.Info("starting TopMusic.exe");
                Process p = Process.Start(startupPath + "TopMusic.exe", "tray");
                logger.Info("TopMusic.exe started with pid=" + p.Id);
            }

            // 检查升级程序自身是否需要更新，文件update.undone文件，若存在此文件，表明需要升级，将其重命名为update.restart，并退出等待Updater程序copy升级文件（Updater拷贝完升级文件后会主动启动主程序）
            string undoneFile = startupPath + "updates\\TopMusicUpdate\\update.undone";
            if (updateFilesCopied && File.Exists(undoneFile))   // updateFilesCopied为false表明主程序暂未完全退出，先不重启
            {
                logger.Info("restart TopMusicUpdate for update");
                File.Move(undoneFile, startupPath + "updates\\TopMusicUpdate\\update.restart");
                this.shutdownNow = true;
            }
        }

        private void CheckUpdate()
        {
            string config = HttpUtil.Get(UPDATE_URL + "config.xml");
            if (config != null && config.Length > 0)
            {
                try
                {
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(config);
                    XmlNodeList updateNodes = doc.SelectNodes("/updates/update");

                    string latestTopMusicVersion = null;
                    string latestTopMusicUpdaterVersion = null;

                    for (int i = 0; i < updateNodes.Count; i++)
                    {
                        XmlNode node = updateNodes.Item(i);
                        if (node.NodeType != XmlNodeType.Element)
                        {
                            continue;
                        }

                        XmlElement updateNode = (XmlElement)node;
                        XmlNodeList propNodes = updateNode.ChildNodes;
                        string name = null;
                        string version = null;
                        string timestamp = null;

                        for (int j = 0; j < propNodes.Count; ++j)
                        {
                            XmlNode propNode = propNodes.Item(j);
                            if (propNode.NodeType == XmlNodeType.Element)
                            {
                                XmlElement prop = (XmlElement)propNode;
                                string pname = prop.LocalName;
                                string pvalue = prop.InnerText;
                                if (pvalue != null)
                                {
                                    pvalue = pvalue.Trim();
                                    if (pname == "name")
                                    {
                                        name = pvalue.ToLower();
                                    }
                                    else if (pname == "version")
                                    {
                                        version = pvalue;
                                    }
                                    else if (pname == "timestamp")
                                    {
                                        timestamp = pvalue;
                                    }
                                }
                            }
                        }

                        if (name == "topmusic")
                        { 
                            if(version != null && version.Length > 0)
                            {
                                if (latestTopMusicVersion == null || version.CompareTo(latestTopMusicVersion) > 0)
                                { 
                                    // highest version
                                    latestTopMusicVersion = version;
                                }
                            }
                        }
                        else if (name == "topmusicupdate")
                        {
                            if (version != null && version.Length > 0)
                            {
                                if (latestTopMusicUpdaterVersion == null || version.CompareTo(latestTopMusicUpdaterVersion) > 0)
                                {
                                    // highest version
                                    latestTopMusicUpdaterVersion = version;
                                }
                            }
                        }
                    }

                    if (latestTopMusicVersion != null)
                    {                        
                        string exeFilePath = Application.StartupPath + (Application.StartupPath.EndsWith("\\") ? "" : "\\") + "TopMusic.exe";
                        FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(exeFilePath);
                        if (latestTopMusicVersion.CompareTo(fileVersionInfo.FileVersion) > 0)
                        {
                            TryUpdate("TopMusic", latestTopMusicVersion);
                        }
                    }

                    if (latestTopMusicUpdaterVersion != null)
                    {
                        string exeFilePath = Application.StartupPath + (Application.StartupPath.EndsWith("\\") ? "" : "\\") + "TopMusicUpdate.exe";
                        FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(exeFilePath);
                        if (latestTopMusicUpdaterVersion.CompareTo(fileVersionInfo.FileVersion) > 0)
                        {
                            TryUpdate("TopMusicUpdate", latestTopMusicUpdaterVersion);
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("failed to load updates/config.xml：" + ex.Message, ex);
                }
            }
        }

        private bool TryUpdate(string assemplyName, string version) {
            string startupPath = Application.StartupPath + (Application.StartupPath.EndsWith("\\") ? "" : "\\");

            // 检查标识文件，若处于restart或undone状态，则无需重新下载
            if (File.Exists(startupPath + "updates\\" + assemplyName + "\\update.restart")
                || File.Exists(startupPath + "updates\\" + assemplyName + "\\update.undone"))
            {
                return false;
            }

            string updateFileName = assemplyName + "_update_" + version + ".zip";
            string outputFile = startupPath + "updates\\" + updateFileName;
            string dir = Path.GetDirectoryName(outputFile);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            logger.Info("downloading " + updateFileName + " for update...");
            HttpUtil.Download(UPDATE_URL + updateFileName, outputFile);

            if (File.Exists(outputFile))
            {
                string md5 = HttpUtil.Get(UPDATE_URL + updateFileName + ".md5");
                if (md5 != null && md5.Length > 0)
                {
                    // check md5
                    MD5 md5Hasher = MD5.Create();
                    StringBuilder md5Actual = new StringBuilder();
                    FileStream fs = File.OpenRead(outputFile);
                    byte[] hashBytes = md5Hasher.ComputeHash(fs);
                    foreach (byte b in hashBytes)
                    {
                        md5Actual.Append(b.ToString("x2"));
                    }
                    fs.Close();
                    if (!md5.Trim().ToLower().Equals(md5Actual.ToString().ToLower()))
                    {
                        logger.Error("md5 not matches for " + updateFileName + ", expected: " + md5 + ", actual: " + md5Actual.ToString());
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                logger.Error("failed to download " + updateFileName);
                return false;
            }

            // try update
            // upzip
            ZipFile zipFile = ZipFile.Read(outputFile);
            string extractDir = startupPath + "updates\\" + assemplyName;
            if (Directory.Exists(extractDir))
            {
                Directory.Delete(extractDir, true);
            }

            zipFile.ExtractAll(extractDir);
            zipFile.Dispose();
            File.Delete(outputFile);
            // 创建文件标识并等待主程序退出后再升级
            string undoneFile = startupPath + "updates\\" + assemplyName + "\\update.undone";
            FileStream undoneFileStream = File.Create(undoneFile);
            undoneFileStream.Close();

            return true;
        }
    
        
    }
}
