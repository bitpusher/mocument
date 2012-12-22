using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using Mocument.Data;
using Mocument.Web.Code;

namespace Mocument.Web.Manage
{
    public partial class MyTapes : System.Web.UI.Page
    {
         
        protected void Page_Load(object sender, EventArgs e)
        {
            IpLabel.Text = TapeLibraryDataSource.GetIpAddress();
            AddIPTextBox.Text = TapeLibraryDataSource.GetIpAddress();
        }

   

        protected void AddButtonClick(object sender, EventArgs e)
        {
            try
            {
                using (var context = new TapesEntities())
                {
                    var t = new Tape
                                {
                                    Id = Membership.GetUser().UserName.ToLower() + "." + AddNameTextBox.Text,
                                    Description = AddDescrTextBox.Text,
                                    OpenForRecording = AddOpenCheckBox.Checked,
                                    AllowedIpAddress = AddIPTextBox.Text
                                };
                    context.Tapes.AddObject(t);

                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {

                ErrorLabel.Text = ex.InnerException.Message;
            }
            Page.DataBind();
        }

        protected void EntityDataSource1_Updated(object sender, EntityDataSourceChangedEventArgs e)
        {
            RefreshProxy();
        }

        protected void EntityDataSource1_Inserted(object sender, EntityDataSourceChangedEventArgs e)
        {
            RefreshProxy();
        }

        protected void EntityDataSource1_Deleted(object sender, EntityDataSourceChangedEventArgs e)
        {
            RefreshProxy();
        }

        private static void RefreshProxy()
        {
            new WebClient().DownloadString("http://localhost:81/refresh/");
        }
    }
}