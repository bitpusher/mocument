using System;
using System.Threading;
using System.Web;
using System.Web.Configuration;
using Mocument.IPC;
using Mocument.WebUI.Code;

namespace Mocument.WebUI
{
    public class Global : HttpApplication
    {
        private void Application_Start(object sender, EventArgs e)
        {
            string ipcPath = WebConfigurationManager.AppSettings["ipcPath"];
            string ipcChannel = WebConfigurationManager.AppSettings["ipcChannel"];

            IpcFactory.ChannelSide = (IpcChannelSide)Enum.Parse(typeof(IpcChannelSide), ipcChannel, true);
            IpcFactory.Path = ipcPath;

            string libraryPath = WebConfigurationManager.AppSettings["contextPath"];
            DataAccess.Context.Open(libraryPath);

            int port = int.Parse(WebConfigurationManager.AppSettings["proxyPort"]);
            ProxySettings.Port = port;
            bool lockDown = bool.Parse(WebConfigurationManager.AppSettings["proxyLockDown"]);
            ProxySettings.LockDown = lockDown;





            var ipc = IpcFactory.Create();
            ipc.SendMessage("WebUI Started");

        }

        private void Application_End(object sender, EventArgs e)
        {
            DataAccess.Context.Close();
        }

        private void Application_Error(object sender, EventArgs e)
        {
            // Code that runs when an unhandled error occurs
        }

        private void Session_Start(object sender, EventArgs e)
        {
            // Code that runs when a new session is started
        }

        private void Session_End(object sender, EventArgs e)
        {
            // Code that runs when a session ends. 
            // Note: The Session_End event is raised only when the sessionstate mode
            // is set to InProc in the Web.config file. If session mode is set to StateServer 
            // or SQLServer, the event is not raised.
        }
    }
}