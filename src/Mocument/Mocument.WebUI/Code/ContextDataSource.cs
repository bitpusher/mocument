using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using Mocument.DataAccess;
using Mocument.Model;

namespace Mocument.WebUI.Code
{
    public class ContextDataSource
    {

        private MembershipUser _user;
        public ContextDataSource()
        {
            _user = Membership.GetUser();
        }

        public List<Tape> ListTapes()
        {
            return Context.ListTapes();
        }
    }
}