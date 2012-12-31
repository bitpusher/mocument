using System;
using System.Text;
using MemoryMappedFileManager;
using Newtonsoft.Json;

namespace Mocument.DataAccess.File.IPC
{
    public abstract class IpcChannel : IDisposable
    {
        #region Factory

        public static IpcChannel Create(string ipcPath, IpcChannelSide ipcChannelSide)
        {
            IpcChannel i = null;
            switch (ipcChannelSide)
            {
                case IpcChannelSide.High:
                    i = new IpcChannelHigh(ipcPath);
                    break;
                case IpcChannelSide.Low:
                    i = new IpcChannelLow(ipcPath);
                    break;
            }
            return i;
        }

        #endregion

        private MemoryMappedFileCommunicator _communicator;

        protected IpcChannel(string ipcPath)
        {
            StartIPC(ipcPath);
        }

        protected abstract int WritePos { get; }
        protected abstract int ReadPos { get; }


        private void StartIPC(string ipcChannel)
        {
            _communicator = new MemoryMappedFileCommunicator(ipcChannel, 1024 * 1024)
                                {
                                    ReadPosition = ReadPos,
                                    WritePosition = WritePos
                                };

            _communicator.DataReceived += CommunicatorDataReceived;
            _communicator.StartReader();
        }

        public event EventHandler<IpcDataRecievedEventArgs> DataReceived;

        public void SendMessage(IPCMessage message)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("Channel is disposed");
            }
            string json = JsonConvert.SerializeObject(message);
            _communicator.Write(json);
        }

        private void CommunicatorDataReceived(object sender, MemoryMappedDataReceivedEventArgs e)
        {
            if (_disposed)
            {
                return;
            }
            string receivedMessage = Encoding.UTF8.GetString(e.Data);
            var m = JsonConvert.DeserializeObject<IPCMessage>(receivedMessage);
            var args = new IpcDataRecievedEventArgs(m);
            EventHandler<IpcDataRecievedEventArgs> handler = DataReceived;
            if (handler != null) handler(this, args);
        }

        #region IDisposable

        private bool _disposed;

        public void Dispose()
        {
            if (_communicator != null)
            {
                try
                {
                    _communicator.CloseReader();
                    _communicator.Dispose();
                    _communicator = null;
                }
                catch
                {
                }
            }

            _disposed = true;
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}