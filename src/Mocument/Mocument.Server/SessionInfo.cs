using System;
using Fiddler;

namespace Mocument.Server
{

    public class SessionInfo
    {

        public SessionInfo(Session oS)
        {
            if (oS.PathAndQuery.StartsWith("/refresh/", StringComparison.OrdinalIgnoreCase) && (oS.clientIP == "::1" || oS.clientIP == "127.0.0.1"))
            {
                Type = SessionType.Refresh;
                return;
            }
            if (oS.PathAndQuery.StartsWith("/record/", StringComparison.OrdinalIgnoreCase))
            {
                Type = SessionType.Record;
            }
            if (oS.PathAndQuery.StartsWith("/play/", StringComparison.OrdinalIgnoreCase))
            {
                Type = SessionType.Playback;
            }
            if (Type != SessionType.None)
            {
                string path = oS.PathAndQuery;

                switch (path)
                {
                    //#TODO: remove this
                    case "/blank.html":
                        //do nothing
                        break;
                    default:
                        string[] pathSegments = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                        UserId = pathSegments[1];
                        TapeId = pathSegments[2];
                        Host = pathSegments[3];
                        PathAndQuery = "/" + string.Join("/", pathSegments, 4, pathSegments.Length - 4);
                        break;
                }
            }
        }
        public SessionType Type;
        public string UserId;
        public string TapeId;
        public string Host;
        public string PathAndQuery;
    }
}