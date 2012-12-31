namespace Mocument.DataAccess.File.IPC
{
    public class IpcChannelLow : IpcChannel
    {
        private const int READ_POS = 2000;
        private const int WRITE_POS = 0;

        internal IpcChannelLow(string ipcPath)
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