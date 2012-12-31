using Mocument.Model;

namespace Mocument.DataAccess.File.IPC
{
    public class IPCMessage
    {
        public IPCMessageType Type { get; set; }
        public Tape Tape { get; set; }
        public bool Accepted { get; set; }
    }
}