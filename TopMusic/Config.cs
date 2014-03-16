using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace TopMusic
{
    public class Config
    {
        private string rasEntryName;

        public string RasEntryName
        {
            get { return this.rasEntryName; }
            set { this.rasEntryName = value; }
        }

        public Config()
        {
            this.LoadConfig();
        }

        private void LoadConfig()
        {
            string configFile = System.Windows.Forms.Application.StartupPath;
            if (!configFile.EndsWith("\\"))
                configFile += "\\";
            configFile += "topmusic.conf.xml";

            if (!System.IO.File.Exists(configFile))
            {
                return;
            }

            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(configFile);
                XmlElement root = doc.DocumentElement;
                XmlNodeList nodes = root.ChildNodes;
                for (int i = 0; i < nodes.Count; i++)
                {
                    XmlNode node = nodes.Item(i);
                    if (node.NodeType != XmlNodeType.Element)
                    {
                        continue;
                    }

                    XmlElement element = (XmlElement)node;
                    string name = element.LocalName;
                    string value = element.InnerText;
                    if (value != null)
                    {
                        value = value.Trim();
                        if (name == "rasEntryName")
                        {
                            this.rasEntryName = value;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("加载配置文件失败：" + configFile + "\r\n" + ex.Message);
            }
        }

        public void Save()
        {
            string configFile = System.Windows.Forms.Application.StartupPath;
            if (!configFile.EndsWith("\\"))
                configFile += "\\";
            configFile += "topmusic.conf.xml";

            System.IO.StreamWriter writer = new System.IO.StreamWriter(configFile, false, Encoding.UTF8);
            writer.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            writer.WriteLine("<config>");
            writer.WriteLine("<rasEntryName>" + StringValueOf(this.rasEntryName) + "</rasEntryName>");
            writer.WriteLine("</config>");
            writer.Flush();
            writer.Close();
        }

        private static DateTime parseDate(string value)
        {
            return DateTime.ParseExact(value, "yyyy-MM-dd", null);
        }

        private static string StringValueOf(string value)
        {
            return value == null ? "" : value;
        }

        private static string StringValueOf(int value)
        {
            return value.ToString();
        }

        private static string StringValueOf(bool value)
        {
            return value ? "true" : "false";
        }

        private static string StringValueOf(DateTime value)
        {
            return value.ToString("yyyy-MM-dd");
        }
    }
}
