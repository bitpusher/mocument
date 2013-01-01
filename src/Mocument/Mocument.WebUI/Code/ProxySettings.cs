using System;
using System.Linq;


namespace Mocument.WebUI.Code
{

    public static class ProxySettings
    {
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