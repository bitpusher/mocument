using Mocument.DataAccess.File.IPC;

namespace Mocument.DataAccess.File.IPC
{
    public class IpcChannelHigh : IpcChannel
    {
        private const int READ_POS = 0;
        private const int WRITE_POS = 1024 * 512;

        internal IpcChannelHigh(string ipcPath)
            : base(ipcPath)
        {
        }

        protected override int ReadPos
        {
            get { return READ_POS; }
        }

        protected override int WritePos
        {
            get { return WRITE_POS; }
        }
    }
}