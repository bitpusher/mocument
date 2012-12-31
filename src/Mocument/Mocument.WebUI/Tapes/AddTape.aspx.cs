﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using Mocument.DataAccess.SQLite;
using Mocument.Model;

namespace Mocument.WebUI.Tapes
{
    public partial class AddTape : System.Web.UI.Page
    {
        public string GetUserIP()
        {
            string ipList = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if (!string.IsNullOrEmpty(ipList))
            {
                return ipList.Split(',')[0];
            }

            return Request.ServerVariables["REMOTE_ADDR"];
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            IpTextBox.Text = GetUserIP();
        }

        protected void CancelButton_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/tapes/mytapes.aspx");
        }

        protected void AddButton_Click(object sender, EventArgs e)
        {

            var user = Membership.GetUser().UserName;
            var tape = new Tape()
                           {
                               Id = user + "." + NameTextBox.Text,
                               Description = DescTextBox.Text,
                               AllowedIpAddress = IpTextBox.Text,
                               OpenForRecording = LockedCheckBox.Checked
                           };
            var store = new Store("mocument");
            store.Insert(tape);
            Response.Redirect("~/tapes/mytapes.aspx");
        }
    }
}