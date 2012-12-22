using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Fiddler;
using Mocument.Data;
using Mocument.Data.Transcoders;

namespace Mocument.Server
{
    public class MocumentServer
    {
        private static TapeLibrary _library;

        private static readonly ConcurrentDictionary<Session, SessionInfo> RecordCache =
            new ConcurrentDictionary<Session, SessionInfo>();

        private readonly int _port;

        public MocumentServer(int port)
        {
            _library = new TapeLibrary();
            _port = port;
        }

        private static void ProcessEndResponse(Session oS)
        {
            lock (RecordCache)
            {
                if (RecordCache.ContainsKey(oS))
                {
                    if (oS.state != SessionStates.Done)
                    {
                        return;
                    }

                    SessionInfo info;
                    RecordCache.TryRemove(oS, out info);
                    string tapeId = info.UserId + "." + info.TapeId;
                    Tape tape = _library.Select(tapeId);
                    var sessions = new List<Session>();
                    if (!string.IsNullOrEmpty(tape.Content))
                    {
                        sessions = HttpArchiveJsonImport.LoadStream(tape.Content);
                    }
                    Session matchedSession = FindMatchingSession(oS, sessions);
                    if (matchedSession == null)
                    {
                        sessions.Add(oS);
                        tape.Content = HttpArchiveJsonExport.WriteStream(sessions);

                        _library.AddOrUpdate(tape);
                    }

                }
            }
        }

        private static void ProcessBeginRequest(Session oS)
        {
            var info = new SessionInfo(oS);

            oS.host = info.Host;
            oS.PathAndQuery = info.PathAndQuery;

            switch (info.Type)
            {
                case SessionType.Refresh:
                    
                    Console.WriteLine("refreshing context");
                    _library.ResetContext();
                    oS.utilCreateResponseAndBypassServer();
                    oS.responseCode = 200;
                    oS.utilSetResponseBody("Library Refreshed");

                    break;
                case SessionType.Record:
                    string tapeId = info.UserId + "." + info.TapeId;

                    Tape tape = _library.Select(tapeId);
                    if (tape == null)
                    {
                        oS.utilCreateResponseAndBypassServer();
                        oS.responseCode = 404;
                        oS.utilSetResponseBody("Tape not found");
                        return;
                    }
                    if (!tape.OpenForRecording)
                    {
                        oS.utilCreateResponseAndBypassServer();
                        oS.responseCode = 500;
                        oS.utilSetResponseBody("Tape is not open for recording");
                        return;
                    }
                    string ip = GetClientIp(oS);
                    if (ip != tape.AllowedIpAddress)
                    {
                        oS.utilCreateResponseAndBypassServer();
                        oS.responseCode = 500;
                        oS.utilSetResponseBody("IP " + GetClientIp(oS) + " not allowed to record. (only  " +
                                               tape.AllowedIpAddress + "is authorized)");
                        return;
                    }
                    RecordSession(oS, info);
                    break;

                case SessionType.Playback:
                    PlaybackSession(oS, info);
                    break;
                case SessionType.InvalidMimeType:
                    oS.utilCreateResponseAndBypassServer();
                    oS.responseCode = 500;
                    oS.utilSetResponseBody("Invalid MIME type");

                    break;
            }
        }

        private static string GetClientIp(Session oS)
        {
            string ip = oS.oRequest.pipeClient.Address.ToString();
            if (ip.StartsWith("::ffff:"))
            {
                ip = ip.Substring(7);
            }
            return ip;
        }

        private static void PlaybackSession(Session oS, SessionInfo info)
        {
            Console.WriteLine("entering playback");
            try
            {
                string tapeId = info.UserId + "." + info.TapeId;
                Tape tape = _library.Select(tapeId);
                if (tape == null)
                {
                    oS.utilCreateResponseAndBypassServer();
                    oS.responseCode = 404;
                    oS.utilSetResponseBody("Tape not found");
                    Console.WriteLine("exit playback");
                    return;
                }


                List<Session> sessions = string.IsNullOrEmpty(tape.Content) ? new List<Session>() : HttpArchiveJsonImport.LoadStream(tape.Content);

                Session matchedSession = FindMatchingSession(oS, sessions);


                if (matchedSession == null)
                {
                    oS.utilCreateResponseAndBypassServer();
                    oS.responseCode = 404;

                    oS.utilSetResponseBody("Matching session not found");
                }
                else
                {
                    oS.utilCreateResponseAndBypassServer();
                    oS.responseBodyBytes = matchedSession.responseBodyBytes;
                    oS.oResponse.headers = (HTTPResponseHeaders)matchedSession.oResponse.headers.Clone();
                }
            }
            catch
            {
                oS.utilCreateResponseAndBypassServer();
                oS.responseCode = 500;
                oS.utilSetResponseBody("Exception occurred");
                Console.WriteLine("exit playback");
                throw;
            }
            Console.WriteLine("exit playback");
        }

        private static Session FindMatchingSession(Session oS, IEnumerable<Session> sessions)
        {
            Session matchedSession = null;

            foreach (Session session in sessions)
            {
                if (oS.oRequest.headers.HTTPMethod != session.oRequest.headers.HTTPMethod)
                {
                    continue;
                }

                if (String.Compare(oS.fullUrl, session.fullUrl, StringComparison.OrdinalIgnoreCase) != 0)
                {
                    continue;
                }

                bool headerMismatch = false;

                foreach (HTTPHeaderItem headerItem in session.oRequest.headers)
                {
                    switch (headerItem.Name.ToLowerInvariant())
                    {
                        case "user-agent":
                        case "host":
                            // #TODO: allow user configuration of ignored headers. requires a tool page to show all headers collected
                            break;
                        default:

                            if (
                                !oS.oRequest.headers.ExistsAndContains(headerItem.Name.ToLowerInvariant(),
                                                                       headerItem.Value.ToLowerInvariant()))
                            {
                                headerMismatch = true;
                            }
                            break;
                    }
                }
                if (headerMismatch)
                {
                    continue;
                }


                if (!session.RequestBody.SequenceEqual(oS.RequestBody))
                {
                    continue;
                }


                matchedSession = session;

                break;
            }
            return matchedSession;
        }

        private static void RecordSession(Session oS, SessionInfo info)
        {
            try
            {
                oS.bBufferResponse = true;

                lock (RecordCache)
                {
                    RecordCache.TryAdd(oS, info);
                }
            }
            catch
            {
                oS.utilCreateResponseAndBypassServer();
                oS.responseCode = 500;
                oS.utilSetResponseBody("Exception occurred");
            }
        }


        public void Start()
        {
            FiddlerApplication.BeforeRequest += ProcessBeginRequest;


            FiddlerApplication.AfterSessionComplete += ProcessEndResponse;
            CONFIG.IgnoreServerCertErrors = true;
            FiddlerApplication.Prefs.SetBoolPref("fiddler.network.streaming.ForgetStreamedData", false);
            FiddlerApplication.Prefs.SetBoolPref("fiddler.network.streaming.abortifclientaborts", true);


            FiddlerApplication.Startup(_port, false, true, true);
        }

        public void Stop()
        {
            FiddlerApplication.Shutdown();
            Thread.Sleep(500);
        }
    }
}