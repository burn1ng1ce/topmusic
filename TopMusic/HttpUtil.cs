using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace TopMusic
{
    public class HttpUtil
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(typeof(HttpUtil));
        private const int BUFFER_SIZE = 4096;
        private static Encoding DEFAULT_ENCODING = Encoding.Default;


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
    }
}
