using System;

namespace Mocument.IPC
{
    public class IpcDataRecievedEventArgs : EventArgs
    {
        public IpcDataRecievedEventArgs(string message)
        {
            Message = message;
        }

        public string Message { get; set; }
    }
}