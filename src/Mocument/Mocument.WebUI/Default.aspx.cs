using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using Mocument.WebUI.Code;
using Salient.HTTPArchiveModel;
using ContextDataSource = Mocument.WebUI.Code.ContextDataSource;
using Page = System.Web.UI.Page;

namespace Mocument.WebUI
{
    public partial class _Default : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected void GridView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            var id = GridView1.SelectedDataKey.Value as string;

            var ds = new ContextDataSource();
            List<Entry> entries = ds.ListEntries(id);

            foreach (Entry entry in entries)
            {
                Panel1.Controls.Add(new LiteralControl("<hr/>"));
                Table entryTable = EntryRenderer.BuildEntryTable(entry);
                Panel1.Controls.Add(entryTable);
            }
        }

        protected void GridView1_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            string name = e.CommandName;
            object arg = e.CommandArgument;
            switch (name.ToLower())
            {
                case "export":
                    Response.Redirect("http://localhost:" + ProxySettings.Port + "/export/" +
                                      ProxySettings.MungTapeId(arg));
                    break;
            }
        }
    }
}