using System;
using System.Linq;
using System.Web.Configuration;


namespace Mocument.WebUI.Code
{

    public static class ProxySettings
    {
    static ProxySettings()
    {
        int port = int.Parse(WebConfigurationManager.AppSettings["proxyPort"]);
        ProxySettings.Port = port;
        bool lockDown = bool.Parse(WebConfigurationManager.AppSettings["proxySecured"]);
        ProxySettings.LockDown = lockDown;

    }
        public static int Port { get; set; }
        public static bool LockDown { get; set; }
        public static string MungTapeId(object id)
        {
            var i = id.ToString().Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            var username = i[0];
            i.RemoveAt(0);
            var d = string.Join(".", i);
            return username + "/" + d;
        }
        public static string GetTapeId(object id)
        {
            var i = id.ToString().Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries).ToList();
 
            i.RemoveAt(0);
            return string.Join(".", i);
        }
    }
}