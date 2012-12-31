<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
         CodeBehind="MyTapes.aspx.cs" Inherits="Mocument.WebUI.Tapes.MyTapes" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <style type="text/css">
        
        .NameCell {
            background-color: #f5f5f5;
            border-right: 1px solid black;
            font-variant: small-caps;
            font-weight: bold;
            margin: 5px;
            padding: 5px;
            text-align: right;
            vertical-align: top;
            width: 5%;
        }

        .HeaderCell {
            background-color: #f5f5f5;
            font-variant: small-caps;
            font-weight: bold;
            margin: 5px;
            padding: 5px;
            text-align: center;
            vertical-align: bottom;
            width: 5%;
        }

        .subtable {
            border: 1px solid black;
            font-size: smaller;
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <p>
        <asp:Button ID="AddButton" runat="server" OnClick="AddButton_Click" Text="ADD NEW TAPE" />
    </p>
    <p>
        <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="False" DataKeyNames="Id"
                      DataSourceID="ObjectDataSource1" AllowPaging="True" 
                      onselectedindexchanged="GridView1_SelectedIndexChanged">
            <Columns>
                <asp:CommandField ShowDeleteButton="True" ShowEditButton="True" 
                                  ShowSelectButton="True" />
                <asp:BoundField DataField="Id" HeaderText="Id" SortExpression="Id" />
                <asp:BoundField DataField="Description" HeaderText="Description" 
                                SortExpression="Description" />
                <asp:CheckBoxField DataField="OpenForRecording" HeaderText="OpenForRecording" SortExpression="OpenForRecording" />
                <asp:BoundField DataField="AllowedIpAddress" HeaderText="AllowedIpAddress" SortExpression="AllowedIpAddress" />
                <asp:BoundField DataField="Comment" HeaderText="Comment" 
                                SortExpression="Comment" />
            </Columns>
        </asp:GridView>
    </p>
    <asp:Panel ID="Panel1" runat="server" GroupingText="Details" ScrollBars="Auto">
    </asp:Panel>
    <p>
    </p>
    <p>
        &nbsp;</p>
    <p>
        &nbsp;</p>

    <p>
        <asp:ObjectDataSource ID="ObjectDataSource1" runat="server" SelectMethod="ListTapesForUser"
                              TypeName="Mocument.WebUI.Code.ContextDataSource" DataObjectTypeName="Mocument.Model.Tape"
                              DeleteMethod="Delete" InsertMethod="Insert" UpdateMethod="Update">
        </asp:ObjectDataSource>
    </p>
</asp:Content>