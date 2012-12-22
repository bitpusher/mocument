using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using Mocument.Web.Code;

namespace Mocument.Web.Manage
{
    public partial class ManageTapes : System.Web.UI.Page
    {
        public string GetUserName()
        {
            var user = Membership.GetUser();
            return user.UserName.ToLower() + ".";
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            IpLabel.Text = TapeLibraryDataSource.GetIpAddress();
            AddIPTextBox.Text = TapeLibraryDataSource.GetIpAddress();
        }

        protected void AddTapeButton_Click(object sender, EventArgs e)
        {
            var ds = new TapeLibraryDataSource();
            ds.Insert(GetUserName() + AddIdTextBox.Text.ToLower(), AddDescriptionTextBox.Text, AddOpenCheckbox.Checked, AddIPTextBox.Text);
            Page.DataBind();
        }
    }
}