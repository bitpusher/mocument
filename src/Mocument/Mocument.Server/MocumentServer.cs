using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Fiddler;
using Mocument.Data;

namespace Mocument.Server
{
    public class MocumentServer
    {
        private static TapeLibrary _library;
        private static readonly ConcurrentDictionary<Session, SessionInfo> RecordCache = new ConcurrentDictionary<Session, SessionInfo>();
        private readonly string _libraryPath;
        private readonly int _port;

        public MocumentServer(string libraryPath, int port)
        {
            _libraryPath = libraryPath;
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
                        // dirty: i think aborted connections cannot be saved?
                        return;
                    }

                    SessionInfo info;
                    RecordCache.TryRemove(oS, out info);
                    string tapeId = info.UserId + "." + info.TapeId;
                    Tape tape = _library.GetOrAddTape(tapeId);
                    List<Session> contents = _library.GetTapeContents(tape.Id);

                    contents.Add(oS);

                    _library.SetTapeContents(tape.Id, contents);
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
                case SessionType.Record:
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

        private static void PlaybackSession(Session oS, SessionInfo info)
        {
            try
            {
                string tapeId = info.UserId + "." + info.TapeId;
                Tape tape = _library.FindTape(tapeId);
                if (tape == null)
                {
                    oS.utilCreateResponseAndBypassServer();
                    oS.responseCode = 404;
                    oS.utilSetResponseBody("Tape not found");
                    return;
                }


                List<Session> sessions = _library.GetTapeContents(tapeId);
                // time to find matching session

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

                throw;
            }
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
            _library = new TapeLibrary(_libraryPath);
            _library.Load();
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