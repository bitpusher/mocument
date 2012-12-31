using System.Collections.Concurrent;
using System.Threading;
using Fiddler;
using Mocument.DataAccess.SQLite;
using Mocument.Model;
using Mocument.Transcoders;
using Salient.HTTPArchiveModel;

namespace Mocument.ReverseProxyServer
{
    public class Server
    {
        private static readonly ConcurrentDictionary<Session, SessionInfo> RecordCache =
            new ConcurrentDictionary<Session, SessionInfo>();

        private readonly string _connectionStringName;
        private readonly bool _secured;
        private readonly int _port;
        private readonly Store _store;

        public Server(string connectionStringName, int port, bool secured)
        {
            _connectionStringName = connectionStringName;
            _port = port;
            _secured = secured;
            _store = new Store(_connectionStringName);
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


        private void ProcessEndResponse(Session oS)
        {

            {
                if (RecordCache.ContainsKey(oS))
                {
                    if (oS.state != SessionStates.Done)
                    {
                        // dirty: #TODO: report and discard
                        return;
                    }

                    SessionInfo info;
                    RecordCache.TryRemove(oS, out info);
                    string tapeId = info.UserId + "." + info.TapeId;
                    Tape tape = _store.Select(tapeId);
                    if (tape == null)
                    {
                        tape = new Tape
                                   {
                                       Id = tapeId
                                   };
                        _store.Insert(tape);
                    }
                    Entry entry = HttpArchiveTranscoder.Export(oS);
                    var matched =_store.MatchEntry(tapeId, entry);
                    if (matched==null)
                    {
                        tape.log.entries.Add(entry);
                        _store.Update(tape);    
                    }
                    
                }
            }
        }

        private void ProcessBeginRequest(Session oS)
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

        private void PlaybackSession(Session oS, SessionInfo info)
        {
            try
            {
                string tapeId = info.UserId + "." + info.TapeId;
                Tape tape = _store.Select(tapeId);
                if (tape == null)
                {
                    oS.utilCreateResponseAndBypassServer();
                    oS.responseCode = 404;
                    oS.utilSetResponseBody("Tape not found");
                    return;
                }


                // time to find matching session
                Entry entry = HttpArchiveTranscoder.Export(oS, true);

                Entry matchedEntry = _store.MatchEntry(tapeId, entry);


                if (matchedEntry == null)
                {
                    oS.utilCreateResponseAndBypassServer();
                    oS.responseCode = 404;
                    oS.utilSetResponseBody("Matching entry not found");
                }
                else
                {
                    Session matchedSession = HttpArchiveTranscoder.Import(matchedEntry);
                    oS.utilCreateResponseAndBypassServer();
                    // #TODO: figger me out
                    // odd, fiddler is compressing respose when it is not compressed from server
                    //oS.responseBodyBytes = matchedSession.responseBodyBytes;
                   
                    oS.utilSetResponseBody(matchedEntry.response.content.text);
                    oS.oResponse.headers = (HTTPResponseHeaders) matchedSession.oResponse.headers.Clone();

                    // #TODO: figger me out
                    oS.oResponse.headers["Content-Length"] = matchedEntry.response.content.text.Length.ToString();
                    // #TODO: figger me out
                    oS.oResponse.headers.Remove("Content-Encoding");
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

        private void RecordSession(Session oS, SessionInfo info)
        {
            try
            {
                if (_secured)
                {
                    Tape tape = _store.Select(info.UserId + "." + info.TapeId);
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
                        oS.responseCode = 412;
                        oS.utilSetResponseBody("Tape is not open for recording");
                        return;
                    }
                    string ip = GetClientIp(oS);
                    if (ip != tape.AllowedIpAddress)
                    {
                        oS.utilCreateResponseAndBypassServer();
                        oS.responseCode = 403;
                        oS.utilSetResponseBody("IP " + GetClientIp(oS) + " not allowed to record.");
                        return;
                    }
                }
                oS.bBufferResponse = true;
                RecordCache.TryAdd(oS, info);
            }
            catch
            {
                oS.utilCreateResponseAndBypassServer();
                oS.responseCode = 500;
                oS.utilSetResponseBody("Exception occurred");
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
    }
}