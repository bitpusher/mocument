using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Security;
using Mocument.Data;

namespace Mocument.Web.Code
{
    public class TapeLibraryDataSource
    {
        private readonly TapeLibrary _library;
        private readonly MembershipUser _user;

        public TapeLibraryDataSource()
        {
            _user = Membership.GetUser();
            _library = new TapeLibrary();
        }

        public static string GetIpAddress()
        {
            HttpContext context = HttpContext.Current;

            string ipAddress = context.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if (!string.IsNullOrEmpty(ipAddress))
            {
                string[] addresses = ipAddress.Split(',');
                if (addresses.Length != 0)
                {
                    return addresses[0];
                }
            }

            return context.Request.ServerVariables["REMOTE_ADDR"];
        }

        public static string GetUrls(string id)
        {
            string server = HttpContext.Current.Request.ServerVariables["SERVER_NAME"].ToLower();
            if (server.StartsWith("www."))
            {
                server = server.Substring(4);
            }
            string[] segments = id.Split(new[] {'.'}, StringSplitOptions.RemoveEmptyEntries);
            return "http://record." + server + ":81/" + segments[0] + "/" + segments[1] + "/" + "<br/>" +
                   "http://playback." + server + ":81/" + segments[0] + "/" + segments[1] + "/";
        }

        public List<Tape> Select()
        {
            return _library.Select();
        }

        public List<Tape> SelectMatches(string pattern)
        {
            return _library.SelectMatches(pattern);
        }

        public List<Tape> SelectForCurrentUser()
        {
            return SelectMatches(_user.UserName + "\\..*");
        }

// ReSharper disable InconsistentNaming
        public void Delete(string Id)
// ReSharper restore InconsistentNaming
        {
            _library.Delete(Id);
        }

        public void Update(Tape t)
        {
            _library.AddOrUpdate(t);
        }

// ReSharper disable InconsistentNaming
        public void Select(string Id)
// ReSharper restore InconsistentNaming
        {
            _library.Select(Id);
        }
    }
}