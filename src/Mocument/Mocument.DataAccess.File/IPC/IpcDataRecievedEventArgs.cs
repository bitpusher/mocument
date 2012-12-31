using System;

namespace Mocument.DataAccess.File.IPC
{
    public class IpcDataRecievedEventArgs : EventArgs
    {
        public IpcDataRecievedEventArgs(IPCMessage message)
        {
            Message = message;
        }

        public IPCMessage Message { get; set; }
    }
}