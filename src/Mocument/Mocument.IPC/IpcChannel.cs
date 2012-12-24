using System;
using System.Text;
using MemoryMappedFileManager;

namespace Mocument.IPC
{
  
    public static class IpcCommunicator
    {
        private static IpcChannel _channel;
        public static void SendMessage(string message)
        {
            if (_channel == null)
            {
                if (string.IsNullOrEmpty(Path))
                {
                    throw new Exception("IpcCommunicator Path is empty");
                }
                if (ChannelSide == IpcChannelSide.None)
                {
                    throw new Exception("IpcCommunicator ChannelSide is NONE");
                }
                _channel = IpcChannel.Create(Path, ChannelSide);
            }
            _channel.SendMessage(message);
        }
        public static string Path { get; set; }
        public static IpcChannelSide ChannelSide { get; set; }


    }
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
            _communicator = new MemoryMappedFileCommunicator(ipcChannel, 4096)
                                {
                                    ReadPosition = ReadPos,
                                    WritePosition = WritePos
                                };
            
            _communicator.DataReceived += CommunicatorDataReceived;
            _communicator.StartReader();
        }

        public event EventHandler<IpcDataRecievedEventArgs> DataReceived;

        public void SendMessage(string message)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("Channel is disposed");
            }
            _communicator.Write(message);
        }

        private void CommunicatorDataReceived(object sender, MemoryMappedDataReceivedEventArgs e)
        {
            if (_disposed)
            {
                return;
            }
            string receivedMessage = Encoding.UTF8.GetString(e.Data);
            EventHandler<IpcDataRecievedEventArgs> handler = DataReceived;
            if (handler != null) handler(this, new IpcDataRecievedEventArgs(receivedMessage));
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