using Mocument.IPC;

namespace Mocument.WebUI.Code
{
    public static class ProxySettings
    {
        public static int Port { get; set; }
        public static bool LockDown { get; set; }
    }
    public static class IpcFactory
    {
        public static string Path { get; set; }
        public static IpcChannelSide ChannelSide { get; set; }

        public static IpcChannel Create()
        {
            return IpcChannel.Create(Path, ChannelSide);
        }
    }
}