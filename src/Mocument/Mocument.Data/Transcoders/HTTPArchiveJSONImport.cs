using Fiddler;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Mocument.Data.Transcoders
{
    public static class HttpArchiveJsonImport
    {
        private static byte[] _getBodyArrayFromContent(Hashtable htContent, string sHeaders)
        {
            byte[] writeData = new byte[0];
            if (htContent != null)
            {
                if (!htContent.ContainsKey("text"))
                {
                    return writeData;
                }
                if (htContent.ContainsKey("encoding") && ("base64" == ((string)htContent["encoding"])))
                {
                    return Convert.FromBase64String((string)htContent["text"]);
                }
                Encoding encoding = Encoding.UTF8;
                if (htContent.ContainsKey("mimeType") && (((string)htContent["mimeType"]).IndexOf("charset") > -1))
                {
                    Match match = new Regex("charset\\s?=\\s?[\"]?(?<TokenValue>[^\";]*)").Match((string)htContent["mimeType"]);
                    if (match.Success && (match.Groups["TokenValue"] != null))
                    {
                        try
                        {
                            encoding = Encoding.GetEncoding(match.Groups["TokenValue"].Value);
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
                writeData = encoding.GetBytes((string)htContent["text"]);
                if (sHeaders.Contains("Content-Encoding") && sHeaders.Contains("gzip"))
                {
                    writeData = Utilities.GzipCompress(writeData);
                }
                if (sHeaders.Contains("Content-Encoding") && sHeaders.Contains("deflate"))
                {
                    writeData = Utilities.DeflaterCompress(writeData);
                }
                if (sHeaders.Contains("Transfer-Encoding") && sHeaders.Contains("chunked"))
                {
                    writeData = Utilities.doChunk(writeData, 2);
                }
            }
            return writeData;
        }

        private static string _getHeaderStringFromArrayList(ArrayList alHeaders)
        {
            StringBuilder builder = new StringBuilder();
            foreach (Hashtable hashtable in alHeaders)
            {
                builder.AppendFormat("{0}: {1}\r\n", hashtable["name"], hashtable["value"]);
            }
            return builder.ToString();
        }

        private static byte[] _getRequestFromEntry(Hashtable htRequest)
        {
            string b = (string)htRequest["method"];
            string str2 = htRequest["httpVersion"] as string;
            if (string.IsNullOrEmpty(str2))
            {
                str2 = "HTTP/0.0";
            }
            string sString = (string)htRequest["url"];
            string str4 = _getHeaderStringFromArrayList((ArrayList)htRequest["headers"]);
            string str5 = string.Empty;
            if (htRequest.ContainsKey("postData"))
            {
                Hashtable hashtable = htRequest["postData"] as Hashtable;
                if (hashtable != null)
                {
                    if (hashtable.ContainsKey("text"))
                    {
                        str5 = (string)hashtable["text"];
                    }
                    else if (hashtable.ContainsKey("params"))
                    {
                        str5 = _getStringFromParams((ArrayList)hashtable["params"]);
                    }
                }
            }
            if (string.Equals("CONNECT", b, StringComparison.OrdinalIgnoreCase))
            {
                sString = Utilities.TrimBeforeLast(sString, '/');
            }
            string s = string.Format("{0} {1} {2}\r\n{3}\r\n{4}", new object[] { b, sString, str2, str4, str5 });
            return CONFIG.oHeaderEncoding.GetBytes(s);
        }

        private static byte[] _getResponseFromEntry(Hashtable htResponse)
        {
            string str = ((double)htResponse["status"]).ToString();
            string str2 = (string)htResponse["statusText"];
            string str3 = htResponse["httpVersion"] as string;
            if (string.IsNullOrEmpty(str3))
            {
                str3 = "HTTP/0.0";
            }
            string sHeaders = _getHeaderStringFromArrayList((ArrayList)htResponse["headers"]);
            byte[] src = _getBodyArrayFromContent((Hashtable)htResponse["content"], sHeaders);
            string s = string.Format("{0} {1} {2}\r\n{3}\r\n", new object[] { str3, str, str2, sHeaders });
            byte[] bytes = CONFIG.oHeaderEncoding.GetBytes(s);
            byte[] dst = new byte[bytes.Length + src.Length];
            Buffer.BlockCopy(bytes, 0, dst, 0, bytes.Length);
            Buffer.BlockCopy(src, 0, dst, bytes.Length, src.Length);
            return dst;
        }

        private static Session _getSessionFromEntry(Hashtable htEntry)
        {
            DateTime now;
            Hashtable htRequest = (Hashtable)htEntry["request"];
            byte[] arrRequest = _getRequestFromEntry(htRequest);
            Hashtable htResponse = (Hashtable)htEntry["response"];
            byte[] arrResponse = _getResponseFromEntry(htResponse);
            if ((arrRequest == null) || (arrResponse == null))
            {
                throw new Exception("Failed to get session from entry");
            }
            SessionFlags responseStreamed = SessionFlags.ResponseStreamed;
            Session session = new Session(arrRequest, arrResponse, responseStreamed);
            int num = getTotalSize(htResponse);
            if (num > 0)
            {
                session["X-TRANSFER-SIZE"] = num.ToString();
            }
            if (htEntry.ContainsKey("comment"))
            {
                string str = (string)htEntry["comment"];
                if (!string.IsNullOrEmpty(str))
                {
                    session["ui-comments"] = str;
                }
            }
            if (!DateTime.TryParse((string)htEntry["startedDateTime"], out now))
            {
                now = DateTime.Now;
            }
            if (htEntry.ContainsKey("timings"))
            {
                Hashtable htTimers = (Hashtable)htEntry["timings"];
                session.Timers.DNSTime = getMilliseconds(htTimers, "dns");
                session.Timers.TCPConnectTime = getMilliseconds(htTimers, "connect");
                session.Timers.HTTPSHandshakeTime = getMilliseconds(htTimers, "ssl");
                session.Timers.ClientConnected = session.Timers.ClientBeginRequest = session.Timers.ClientDoneRequest = now;
                session.Timers.ServerConnected = session.Timers.FiddlerBeginRequest = now.AddMilliseconds((double)(((getMilliseconds(htTimers, "blocked") + session.Timers.DNSTime) + session.Timers.TCPConnectTime) + session.Timers.HTTPSHandshakeTime));
                session.Timers.ServerGotRequest = session.Timers.FiddlerBeginRequest.AddMilliseconds((double)getMilliseconds(htTimers, "send"));
                session.Timers.ServerBeginResponse = now.AddMilliseconds((double)getMilliseconds(htTimers, "wait"));
                session.Timers.ServerDoneResponse = session.Timers.ServerBeginResponse.AddMilliseconds((double)getMilliseconds(htTimers, "receive"));
                session.Timers.ClientBeginResponse = session.Timers.ClientDoneResponse = session.Timers.ServerDoneResponse;
            }
            return session;
        }

        private static string _getStringFromParams(ArrayList alParams)
        {
            StringBuilder builder = new StringBuilder();
            bool flag = true;
            foreach (Hashtable hashtable in alParams)
            {
                if (flag)
                {
                    flag = false;
                }
                else
                {
                    builder.Append("&");
                }
                builder.AppendFormat("{0}={1}", HttpUtility.UrlEncode((string)hashtable["name"], Encoding.UTF8), HttpUtility.UrlEncode((string)hashtable["value"], Encoding.UTF8));
            }
            return builder.ToString();
        }

        private static int getMilliseconds(Hashtable htTimers, string sMeasure)
        {
            double? nullable = htTimers[sMeasure] as double?;
            if (!nullable.HasValue)
            {
                return 0;
            }
            int num = (int)Math.Round(nullable.Value);
            return Math.Max(0, num);
        }

        private static int getTotalSize(Hashtable htMessage)
        {
            int num = -1;
            double? nullable = htMessage["headersSize"] as double?;
            if (nullable.HasValue)
            {
                num = (int)Math.Round(nullable.Value);
            }
            int num2 = -1;
            nullable = htMessage["bodySize"] as double?;
            if (nullable.HasValue)
            {
                num2 = (int)Math.Round(nullable.Value);
            }
            if ((num >= 0) && (num2 >= 0))
            {
                return (num + num2);
            }
            return -1;
        }

        public static List<Session> LoadStream(string json
            //,   EventHandler<ProgressCallbackEventArgs> evtProgressNotifications
            )
        {
            List<Session> listSessions = new List<Session>();
            Hashtable hashtable = Json.JsonDecode(json) as Hashtable;
            if (hashtable == null)
            {
                throw new Exception("This file is not properly formatted HAR JSON");
 
            }
            Hashtable hashtable2 = hashtable["log"] as Hashtable;
            if (hashtable == null)
            {
                throw new Exception("This file is not properly formatted HAR JSON");
            }
            //if (evtProgressNotifications != null)
            //{
            //    evtProgressNotifications(null, new ProgressCallbackEventArgs(0f, "Found HTTPArchive v" + hashtable2["version"] + "..."));
            //}
            var list = (ArrayList)hashtable2["entries"];
            int num = 0;
            int count = list.Count;
            foreach (Hashtable hashtable3 in list)
            {
                try
                {
                    Session item = _getSessionFromEntry(hashtable3);
                    if (item != null)
                    {
                        num++;
                        listSessions.Add(item);
                        //if (evtProgressNotifications != null)
                        //{
                        //    evtProgressNotifications(null, new ProgressCallbackEventArgs((float)(num / count), "Imported " + num.ToString() + " sessions."));
                        //}
                    }
                }
                catch (Exception exception)
                {
                    //if (evtProgressNotifications != null)
                    //{
                    //    evtProgressNotifications(null, new ProgressCallbackEventArgs((float)(num / count), "Skipping malformed session." + exception.Message));
                    //}
                    throw new Exception("Skipping malformed session." + exception.Message);
                }
            }
            return listSessions;
        }
    }
}

