using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;

namespace TopMusicUpdate
{
    public class HttpUtil
    {
        private const int BUFFER_SIZE = 4096;
        private static Encoding DEFAULT_ENCODING = Encoding.Default;
        private static Logger logger = new Logger(typeof(HttpUtil));

        public static string Get(string url)
        {
            return Get(url, DEFAULT_ENCODING);
        }

        public static string Get(string url, Encoding encoding)
        {
            string responseText = null;
            try {
                HttpWebRequest req = (HttpWebRequest)WebRequest.CreateDefault(new Uri(url));
                req.Method = "GET";
                req.Timeout = 10000;

                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
                
                if (resp.StatusCode == HttpStatusCode.OK)
                {
                    System.IO.Stream streamIn = resp.GetResponseStream();
                    System.IO.MemoryStream streamOut = new System.IO.MemoryStream();
                    byte[] bbuf = new byte[BUFFER_SIZE];
                    int k;
                    for( ; (k = streamIn.Read(bbuf, 0, BUFFER_SIZE)) != 0; )
                    {
                        streamOut.Write(bbuf, 0, k);
                    }

                    streamIn.Close();
                    streamOut.Close();

                    if (encoding == null)
                    {
                        encoding = DEFAULT_ENCODING;
                    }
                    responseText = encoding.GetString(streamOut.ToArray());
                }

                resp.Close();
            }
            catch (WebException ex)
            {
                if (ex.Response == null || ((HttpWebResponse)ex.Response).StatusCode != HttpStatusCode.NotFound)
                {
                    logger.Error("WebException while get from " + url + ", exception status: " + ex.Status + ":" + ex.Message);
                }
            }
            catch (Exception ex) {
                logger.Error("Exception while get from " + url + ": " + ex.Message, ex);
            }

            return responseText;
        }

        public static void Download(string url, string outputFile)
        {
            System.IO.FileStream streamOut = null;
            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.CreateDefault(new Uri(url));
                req.Method = "GET";
                req.Timeout = 120000;

                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();

                if (resp.StatusCode == HttpStatusCode.OK)
                {
                    System.IO.Stream streamIn = resp.GetResponseStream();

                    // create parent dir if necessary
                    string dir = Path.GetDirectoryName(outputFile);
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }

                    streamOut = System.IO.File.Open(outputFile, System.IO.FileMode.Create, System.IO.FileAccess.Write);
                    byte[] bbuf = new byte[BUFFER_SIZE];
                    int k;
                    for (; (k = streamIn.Read(bbuf, 0, BUFFER_SIZE)) != 0; )
                    {
                        streamOut.Write(bbuf, 0, k);
                    }

                    streamIn.Close();
                }

                resp.Close();
            }
            catch (WebException ex)
            {
                if (ex.Response == null || ((HttpWebResponse)ex.Response).StatusCode != HttpStatusCode.NotFound)
                {
                    logger.Error("WebException while get from " + url + ", exception status: " + ex.Status + ":" + ex.Message);
                }
            }
            catch (Exception ex)
            {
                logger.Error("Exception while get from " + url + ": " + ex.Message, ex);
            }
            finally
            {
                if (streamOut != null)
                {
                    try
                    {
                        streamOut.Close();
                    }
                    catch
                    { }
                }
            }
        }
    }
}
