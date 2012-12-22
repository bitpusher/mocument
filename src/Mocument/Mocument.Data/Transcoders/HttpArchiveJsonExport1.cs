using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;
using Fiddler;
using Newtonsoft.Json;

namespace Mocument.Data.Transcoders
{
    public class HttpArchiveJsonExport1
    {
        private static readonly int MaxBinaryBodyLength =
            FiddlerApplication.Prefs.GetInt32Pref("fiddler.importexport.HTTPArchiveJSON.MaxBinaryBodyLength", 0x8000);

        private static Hashtable GetBodyInfo(Session oS)
        {
            int num;
            int num2;
            var hashtable = new Hashtable();
            GetDecompressedSize(oS, out num, out num2);
            hashtable.Add("size", num);
            hashtable.Add("compression", num2);
            hashtable.Add("mimeType", oS.oResponse["Content-Type"]);
            string mImeType = oS.oResponse.MIMEType;
            bool isMimeTypeTextEquivalent = Utility.IsMimeTypeTextEquivalent(mImeType);
            if (((isMimeTypeTextEquivalent && ("text/plain" == mImeType)) && (oS.responseBodyBytes.Length > 3)) &&
                ((((oS.responseBodyBytes[0] == 0x43) && (oS.responseBodyBytes[1] == 0x57)) &&
                  (oS.responseBodyBytes[2] == 0x53)) ||
                 (((oS.responseBodyBytes[0] == 70) && (oS.responseBodyBytes[1] == 0x4c)) &&
                  (oS.responseBodyBytes[2] == 0x56))))
            {
                isMimeTypeTextEquivalent = false;
            }
            if (isMimeTypeTextEquivalent)
            {
                hashtable.Add("text", oS.GetResponseBodyAsString());
                return hashtable;
            }
            if (oS.responseBodyBytes.Length < MaxBinaryBodyLength)
            {
                hashtable.Add("encoding", "base64");
                hashtable.Add("text", Convert.ToBase64String(oS.responseBodyBytes));
                return hashtable;
            }
            hashtable.Add("comment",
                          "Body length exceeded fiddler.importexport.HTTPArchiveJSON.MaxBinaryBodyLength, so body was omitted.");
            return hashtable;
        }

        private static ArrayList GetCookies(HTTPHeaders oHeaders)
        {
            var list = new ArrayList();
            if (oHeaders is HTTPRequestHeaders)
            {
                string str = oHeaders["Cookie"];
                if (!string.IsNullOrEmpty(str))
                {
                    foreach (string str2 in str.Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries))
                    {
                        string sString = str2.Trim();
                        if (sString.Length >= 1)
                        {
                            var hashtable = new Hashtable
                                                {
                                                    {"name", Utilities.TrimAfter(sString, '=')},
                                                    {"value", Utilities.TrimBefore(sString, '=')}
                                                };
                            list.Add(hashtable);
                        }
                    }
                }
                return list;
            }
            foreach (HTTPHeaderItem item in oHeaders)
            {
                if (item.Name == "Set-Cookie")
                {
                    var hashtable2 = new Hashtable();
                    string str4 = item.Value;
                    string str5 = Utilities.TrimAfter(str4, ';');
                    hashtable2.Add("name", Utilities.TrimAfter(str5, '='));
                    hashtable2.Add("value", Utilities.TrimBefore(str5, '='));
                    string str6 = Utilities.TrimBefore(str4, ';');
                    if (!string.IsNullOrEmpty(str6))
                    {
                        DateTime time;
                        if (str6.IndexOf("httpOnly", StringComparison.OrdinalIgnoreCase) > -1)
                        {
                            hashtable2.Add("httpOnly", "true");
                        }
                        if (str6.IndexOf("secure", StringComparison.OrdinalIgnoreCase) > -1)
                        {
                            hashtable2.Add("_secure", "true");
                        }
                        var regex = new Regex("expires\\s?=\\s?[\"]?(?<TokenValue>[^\";]*)");
                        Match match = regex.Match(str6);
                        if ((match.Success && (match.Groups["TokenValue"] != null)) &&
                            DateTime.TryParse(match.Groups["TokenValue"].Value, out time))
                        {
                            hashtable2.Add("expires", time.ToString("o"));
                        }
                        regex = new Regex("domain\\s?=\\s?[\"]?(?<TokenValue>[^\";]*)");
                        match = regex.Match(str6);
                        if (match.Success && (match.Groups["TokenValue"] != null))
                        {
                            hashtable2.Add("domain", match.Groups["TokenValue"].Value);
                        }
                        match = new Regex("path\\s?=\\s?[\"]?(?<TokenValue>[^\";]*)").Match(str6);
                        if (match.Success && (match.Groups["TokenValue"] != null))
                        {
                            hashtable2.Add("path", match.Groups["TokenValue"].Value);
                        }
                    }
                    list.Add(hashtable2);
                }
            }
            return list;
        }

        private static void GetDecompressedSize(Session oSession, out int iExpandedSize, out int iCompressionSavings)
        {
            int length = oSession.responseBodyBytes.Length;
            var arrBody = (byte[]) oSession.responseBodyBytes.Clone();
            try
            {
                Utilities.utilDecodeHTTPBody(oSession.oResponse.headers, ref arrBody);
            }
// ReSharper disable EmptyGeneralCatchClause
            catch
// ReSharper restore EmptyGeneralCatchClause
            {
            }
            iExpandedSize = arrBody.Length;
            iCompressionSavings = iExpandedSize - length;
        }

        private static ArrayList GetHeadersAsArrayList(HTTPHeaders oHeaders)
        {
            var list = new ArrayList();
            foreach (HTTPHeaderItem item in oHeaders)
            {
                var hashtable = new Hashtable(2) {{"name", item.Name}, {"value", item.Value}};
                list.Add(hashtable);
            }
            return list;
        }

        private static int GetMilliseconds(Hashtable htTimers, string sMeasure)
        {
            int num;
            object obj2 = htTimers[sMeasure];
            if (obj2 == null)
            {
                return 0;
            }
            if (obj2 is int)
            {
                num = (int) obj2;
            }
            else
            {
                var a = (double) obj2;
                num = (int) Math.Round(a);
            }
            return Math.Max(0, num);
        }

        private static Hashtable GetPostData(Session oS)
        {
            var hashtable = new Hashtable();
            string sString = oS.oRequest["Content-Type"];
            hashtable.Add("mimeType", Utilities.TrimAfter(sString, ';'));
            if (sString.StartsWith("application/x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase))
            {
                hashtable.Add("params", GetQueryString("http://fake/path?" + oS.GetRequestBodyAsString()));
                return hashtable;
            }
            hashtable.Add("text", oS.GetRequestBodyAsString());
            return hashtable;
        }

        private static ArrayList GetQueryString(string sUri)
        {
            var list = new ArrayList();
            try
            {
                var uri = new Uri(sUri);
                NameValueCollection values = HttpUtility.ParseQueryString(uri.Query);
                foreach (string str in values.AllKeys)
                {
// ReSharper disable PossibleNullReferenceException
                    foreach (string str2 in values.GetValues(str))
// ReSharper restore PossibleNullReferenceException
                    {
                        var hashtable = new Hashtable {{"name", str}, {"value", str2}};
                        list.Add(hashtable);
                    }
                }
            }
            catch
            {
                return new ArrayList();
            }
            return list;
        }

        private static Hashtable GetRequest(Session oS)
        {
            var hashtable = new Hashtable
                                {
                                    {"method", oS.oRequest.headers.HTTPMethod},
                                    {"url", oS.fullUrl},
                                    {"httpVersion", oS.oRequest.headers.HTTPVersion},
                                    {"headersSize", oS.oRequest.headers.ByteCount() + 2},
                                    {"bodySize", oS.requestBodyBytes.Length},
                                    {"headers", GetHeadersAsArrayList(oS.oRequest.headers)},
                                    {"cookies", GetCookies(oS.oRequest.headers)},
                                    {"queryString", GetQueryString(oS.fullUrl)}
                                };
            if ((oS.requestBodyBytes != null) && (oS.requestBodyBytes.Length > 0))
            {
                hashtable.Add("postData", GetPostData(oS));
            }
            return hashtable;
        }

        private static Hashtable GetResponse(Session oS)
        {
            var hashtable = new Hashtable
                                {
                                    {"status", oS.responseCode},
                                    {"statusText", Utilities.TrimBefore(oS.oResponse.headers.HTTPResponseStatus, ' ')},
                                    {"httpVersion", oS.oResponse.headers.HTTPVersion},
                                    {"headersSize", oS.oResponse.headers.ByteCount() + 2},
                                    {"redirectURL", oS.oResponse["Location"]},
                                    {"bodySize", oS.responseBodyBytes.Length},
                                    {"headers", GetHeadersAsArrayList(oS.oResponse.headers)},
                                    {"cookies", GetCookies(oS.oResponse.headers)},
                                    {"content", GetBodyInfo(oS)}
                                };
            return hashtable;
        }

        private static Hashtable GetTimings(SessionTimers oTimers)
        {
            var hashtable = new Hashtable
                                {
                                    {"blocked", -1},
                                    {"dns", oTimers.DNSTime},
                                    {"connect", oTimers.TCPConnectTime + oTimers.HTTPSHandshakeTime},
                                    {"ssl", oTimers.HTTPSHandshakeTime}
                                };
            TimeSpan span = oTimers.ServerGotRequest - oTimers.FiddlerBeginRequest;
            hashtable.Add("send", Math.Max(0.0, Math.Round(span.TotalMilliseconds)));
            TimeSpan span2 = oTimers.ServerBeginResponse - oTimers.ServerGotRequest;
            hashtable.Add("wait", Math.Max(0.0, Math.Round(span2.TotalMilliseconds)));
            TimeSpan span3 = oTimers.ServerDoneResponse - oTimers.ServerBeginResponse;
            hashtable.Add("receive", Math.Max(0.0, Math.Round(span3.TotalMilliseconds)));
            return hashtable;
        }

        private static int GetTotalTime(Hashtable htTimers)
        {
            int num = 0;
            num += GetMilliseconds(htTimers, "blocked");
            num += GetMilliseconds(htTimers, "dns");
            num += GetMilliseconds(htTimers, "connect");
            num += GetMilliseconds(htTimers, "send");
            num += GetMilliseconds(htTimers, "wait");
            return (num + GetMilliseconds(htTimers, "receive"));
        }

        public static string Serialize(List<Session> oSessions)
        {
            var hashtable = new Hashtable
                                {
                                    {"version", "1.2"},
                                    {"pages", null},
                                    {"comment", "exported @ " + DateTime.Now.ToString(CultureInfo.InvariantCulture)}
                                };
            var hashtable2 = new Hashtable {{"name", "Mocument Server"}, {"comment", "http://mocument.it"}};
            hashtable.Add("creator", hashtable2);
            var list = new ArrayList();
            foreach (Session session in oSessions)
            {
                try
                {
                    if (session.state < SessionStates.Done)
                    {
                        continue;
                    }
                    var hashtable3 = new Hashtable
                                         {
                                             {"startedDateTime", session.Timers.ClientBeginRequest.ToString("o")},
                                             {"request", GetRequest(session)},
                                             {"response", GetResponse(session)},
                                             {"cache", new Hashtable()}
                                         };
                    Hashtable htTimers = GetTimings(session.Timers);
                    hashtable3.Add("time", GetTotalTime(htTimers));
                    hashtable3.Add("timings", htTimers);
                    string str = session["ui-comments"];
                    if (!string.IsNullOrEmpty(str))
                    {
                        hashtable3.Add("comment", session["ui-comments"]);
                    }

                    if (!string.IsNullOrEmpty(str) && !session.isFlagSet(SessionFlags.SentToGateway))
                    {
                        hashtable3.Add("serverIPAddress", session.m_hostIP);
                    }
                    hashtable3.Add("connection", session.clientPort.ToString(CultureInfo.InvariantCulture));
                    list.Add(hashtable3);
                }
                catch (Exception exception)
                {
                    FiddlerApplication.ReportException(exception, "Failed to Export Session");
                }
            }
            hashtable.Add("entries", list);
            var json = new Hashtable {{"log", hashtable}};
            return JsonConvert.SerializeObject(json, Formatting.Indented);
        }
    }
}