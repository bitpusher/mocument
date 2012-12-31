using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Mocument.WebUI.Tapes
{
    public partial class MyTapes : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void AddButton_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/tapes/AddTape.aspx");
        }

        protected void GridView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string id = GridView1.SelectedDataKey.Value as string;

            var ds = new Code.ContextDataSource();
            var entries = ds.ListEntries(id);

            foreach (var entry in entries)
            {
                Panel1.Controls.Add(new LiteralControl("<hr/>"));
                var entryTable = Code.EntryRenderer.BuildEntryTable(entry);
                Panel1.Controls.Add(entryTable);
            }
        }
    }
}