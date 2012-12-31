using System.Collections.Generic;
using System.Web.Security;
using Mocument.DataAccess.SQLite;
using Mocument.Model;
using Salient.HTTPArchiveModel;

namespace Mocument.WebUI.Code
{
    public class ContextDataSource
    {
        private readonly SQLiteStore _store;
        private readonly MembershipUser _user;

        public ContextDataSource()
        {
            _user = Membership.GetUser();
            _store = new SQLiteStore("mocument");
        }

        public void Update(Tape tape)
        {
            _store.Update(tape);
        }

        public void Delete(Tape tape)
        {
            _store.Delete(tape.Id);
        }

        public void Insert(Tape tape)
        {
            _store.Insert(tape);
        }

        public Tape Select(string id)
        {
            Tape value = _store.Select(id);
            return value;
        }

        public List<Tape> ListTapes()
        {
            return _store.List();
        }

        public List<Tape> ListTapesForUser()
        {
            return _store.List(t => t.Id.StartsWith(_user.UserName.ToLower() + "."));
        }

        public List<Entry> ListEntries(string id)
        {
            List<Entry>  result=new List<Entry>();
            if(!string.IsNullOrEmpty(id))
            {
                var tape = Select(id);
                result = tape.log.entries;
            }
            return result;
        }
    }
}