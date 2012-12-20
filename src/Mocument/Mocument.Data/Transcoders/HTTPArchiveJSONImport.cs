using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Fiddler;
using Newtonsoft.Json;

namespace Mocument.Data.Transcoders
{
    internal class HttpArchiveJsonImport
    {
        private static byte[] GetBodyArrayFromContent(Hashtable htContent, string sHeaders)
        {
            var writeData = new byte[0];
            if (htContent != null)
            {
                if (!htContent.ContainsKey("text"))
                {
                    return writeData;
                }
                if (htContent.ContainsKey("encoding") && ("base64" == ((string) htContent["encoding"])))
                {
                    return Convert.FromBase64String((string) htContent["text"]);
                }
                Encoding encoding = Encoding.UTF8;
                if (htContent.ContainsKey("mimeType") &&
                    (((string) htContent["mimeType"]).IndexOf("charset", StringComparison.Ordinal) > -1))
                {
                    Match match =
                        new Regex("charset\\s?=\\s?[\"]?(?<TokenValue>[^\";]*)").Match((string) htContent["mimeType"]);
                    if (match.Success && (match.Groups["TokenValue"] != null))
                    {
                        try
                        {
                            encoding = Encoding.GetEncoding(match.Groups["TokenValue"].Value);
                        }
// ReSharper disable RedundantCatchClause
                        catch
                        {
                            throw;
                        }
// ReSharper restore RedundantCatchClause
                    }
                }
                writeData = encoding.GetBytes((string) htContent["text"]);
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

        public static string GetHeaderStringFromArrayList(ArrayList alHeaders)
        {
            var builder = new StringBuilder();
            foreach (Hashtable hashtable in alHeaders)
            {
                builder.AppendFormat("{0}: {1}\r\n", hashtable["name"], hashtable["value"]);
            }
            return builder.ToString();
        }

        private static byte[] GetRequestFromEntry(Hashtable htRequest)
        {
            var b = (string) htRequest["method"];
            var str2 = htRequest["httpVersion"] as string;
            if (string.IsNullOrEmpty(str2))
            {
                str2 = "HTTP/0.0";
            }
            var sString = (string) htRequest["url"];
            string str4 = GetHeaderStringFromArrayList((ArrayList) htRequest["headers"]);
            string str5 = string.Empty;
            if (htRequest.ContainsKey("postData"))
            {
                var hashtable = htRequest["postData"] as Hashtable;
                if (hashtable != null)
                {
                    if (hashtable.ContainsKey("text"))
                    {
                        str5 = (string) hashtable["text"];
                    }
                    else if (hashtable.ContainsKey("params"))
                    {
                        str5 = GetStringFromParams((ArrayList) hashtable["params"]);
                    }
                }
            }
            if (string.Equals("CONNECT", b, StringComparison.OrdinalIgnoreCase))
            {
                sString = Utilities.TrimBeforeLast(sString, '/');
            }
            string s = string.Format("{0} {1} {2}\r\n{3}\r\n{4}", new object[] {b, sString, str2, str4, str5});
            return CONFIG.oHeaderEncoding.GetBytes(s);
        }

        private static byte[] GetResponseFromEntry(Hashtable htResponse)
        {
            string str = ((double) htResponse["status"]).ToString(CultureInfo.InvariantCulture);
            var str2 = (string) htResponse["statusText"];
            var str3 = htResponse["httpVersion"] as string;
            if (string.IsNullOrEmpty(str3))
            {
                str3 = "HTTP/0.0";
            }
            string sHeaders = GetHeaderStringFromArrayList((ArrayList) htResponse["headers"]);
            byte[] src = GetBodyArrayFromContent((Hashtable) htResponse["content"], sHeaders);
            string s = string.Format("{0} {1} {2}\r\n{3}\r\n", new object[] {str3, str, str2, sHeaders});
            byte[] bytes = CONFIG.oHeaderEncoding.GetBytes(s);
            var dst = new byte[bytes.Length + src.Length];
            Buffer.BlockCopy(bytes, 0, dst, 0, bytes.Length);
            Buffer.BlockCopy(src, 0, dst, bytes.Length, src.Length);
            return dst;
        }

        private static Session GetSessionFromEntry(Hashtable htEntry)
        {
            DateTime now;
            var htRequest = (Hashtable) htEntry["request"];
            byte[] arrRequest = GetRequestFromEntry(htRequest);
            var htResponse = (Hashtable) htEntry["response"];
            byte[] arrResponse = GetResponseFromEntry(htResponse);
            if ((arrRequest == null) || (arrResponse == null))
            {
                throw new Exception("Failed to get session from entry");
            }
            const SessionFlags responseStreamed = SessionFlags.ResponseStreamed;
            var session = new Session(arrRequest, arrResponse, responseStreamed);
            int num = GetTotalSize(htResponse);
            if (num > 0)
            {
                session["X-TRANSFER-SIZE"] = num.ToString(CultureInfo.InvariantCulture);
            }
            if (htEntry.ContainsKey("comment"))
            {
                var str = (string) htEntry["comment"];
                if (!string.IsNullOrEmpty(str))
                {
                    session["ui-comments"] = str;
                }
            }
            if (!DateTime.TryParse((string) htEntry["startedDateTime"], out now))
            {
                now = DateTime.Now;
            }
            if (htEntry.ContainsKey("timings"))
            {
                var htTimers = (Hashtable) htEntry["timings"];
                session.Timers.DNSTime = GetMilliseconds(htTimers, "dns");
                session.Timers.TCPConnectTime = GetMilliseconds(htTimers, "connect");
                session.Timers.HTTPSHandshakeTime = GetMilliseconds(htTimers, "ssl");
                session.Timers.ClientConnected =
                    session.Timers.ClientBeginRequest = session.Timers.ClientDoneRequest = now;
                session.Timers.ServerConnected =
                    session.Timers.FiddlerBeginRequest =
                    now.AddMilliseconds(((GetMilliseconds(htTimers, "blocked") + session.Timers.DNSTime) +
                                         session.Timers.TCPConnectTime) + session.Timers.HTTPSHandshakeTime);
                session.Timers.ServerGotRequest =
                    session.Timers.FiddlerBeginRequest.AddMilliseconds(GetMilliseconds(htTimers, "send"));
                session.Timers.ServerBeginResponse = now.AddMilliseconds(GetMilliseconds(htTimers, "wait"));
                session.Timers.ServerDoneResponse =
                    session.Timers.ServerBeginResponse.AddMilliseconds(GetMilliseconds(htTimers, "receive"));
                session.Timers.ClientBeginResponse =
                    session.Timers.ClientDoneResponse = session.Timers.ServerDoneResponse;
            }
            return session;
        }

        private static string GetStringFromParams(ArrayList alParams)
        {
            var builder = new StringBuilder();
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
                builder.AppendFormat("{0}={1}", HttpUtility.UrlEncode((string) hashtable["name"], Encoding.UTF8),
                                     HttpUtility.UrlEncode((string) hashtable["value"], Encoding.UTF8));
            }
            return builder.ToString();
        }

        private static int GetMilliseconds(Hashtable htTimers, string sMeasure)
        {
            var nullable = htTimers[sMeasure] as double?;
            if (!nullable.HasValue)
            {
                return 0;
            }
            var num = (int) Math.Round(nullable.Value);
            return Math.Max(0, num);
        }

        private static int GetTotalSize(Hashtable htMessage)
        {
            int num = -1;
            var nullable = htMessage["headersSize"] as double?;
            if (nullable.HasValue)
            {
                num = (int) Math.Round(nullable.Value);
            }
            int num2 = -1;
            nullable = htMessage["bodySize"] as double?;
            if (nullable.HasValue)
            {
                num2 = (int) Math.Round(nullable.Value);
            }
            if ((num >= 0) && (num2 >= 0))
            {
                return (num + num2);
            }
            return -1;
        }

        public static bool LoadStream(StreamReader oSr, List<Session> listSessions)
        {
            var hashtable = JsonConvert.DeserializeObject<Hashtable>(oSr.ReadToEnd());
            if (hashtable == null)
            {
                throw new Exception("This file is not properly formatted HAR JSON");
            }
            var hashtable2 = hashtable["log"] as Hashtable;
            if (hashtable == null)
            {
                throw new Exception("This file is not properly formatted HAR JSON");
            }

            if (hashtable2 != null)
            {
                var list = (ArrayList) hashtable2["entries"];
                foreach (Hashtable hashtable3 in list)
                {
                    try
                    {
                        Session item = GetSessionFromEntry(hashtable3);
                        if (item != null)
                        {
                            listSessions.Add(item);
                        }
                    }
// ReSharper disable RedundantCatchClause
                    catch
                    {
                        // malformed session
                        throw;
                    }
// ReSharper restore RedundantCatchClause
                }
            }
            return true;
        }
    }
}