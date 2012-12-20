using System;
using Fiddler;

namespace Mocument.Server
{
    
    public class SessionInfo
    {
        
        public SessionInfo(Session oS)
        {
        
            if (oS.hostname.StartsWith("record.", StringComparison.OrdinalIgnoreCase))
            {
                Type = SessionType.Record;
            }
            if (oS.hostname.StartsWith("playback.", StringComparison.OrdinalIgnoreCase))
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
                        UserId = pathSegments[0];
                        TapeId = pathSegments[1];
                        Host = pathSegments[2];
                        PathAndQuery = "/" + string.Join("/", pathSegments, 3, pathSegments.Length - 3);
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