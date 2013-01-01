<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true"
    CodeBehind="Default.aspx.cs" Inherits="Mocument.WebUI._Default" %>
<%@ Import Namespace="Mocument.WebUI.Code" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="HeadContent">
        <style type="text/css">
        .style1
        {
            font-size: xx-small;
        }
    </style>
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
<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="MainContent">
    <h2>
        Welcome to Mocument.it!
    </h2>
        <p>
        I will fulfill all of your fantasies .... 
    </p>
    <p class="style1">
        ... that is, if all you fantasize about is easy recording, playback 
        and documentation of http traffic.
       
    </p>
    <br />
    Avaliable Tapes<br />
    <asp:GridView ID="GridView1" runat="server" AllowPaging="True" 
        AutoGenerateColumns="False" CellPadding="4" DataKeyNames="Id" 
        DataSourceID="ObjectDataSource1" ForeColor="#333333" GridLines="None" 
        onselectedindexchanged="GridView1_SelectedIndexChanged" 
        onrowcommand="GridView1_RowCommand">
        <AlternatingRowStyle BackColor="White" ForeColor="#284775" />
        <Columns>
            <asp:TemplateField ShowHeader="False">
                <ItemTemplate>
                    <asp:LinkButton ID="LinkButton1" runat="server" CausesValidation="False" 
                        CommandName="Select" Text="Details"></asp:LinkButton>&nbsp;<asp:LinkButton ID="LinkButton4" runat="server" CausesValidation="False" 
                            CommandName="Export" Text="Export" CommandArgument='<%# Eval("Id") %>'></asp:LinkButton>
                </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="Id" SortExpression="Id">
                <EditItemTemplate>
                    <asp:TextBox ID="TextBox1" runat="server" Text='<%# ProxySettings.MungTapeId(Eval("Id"))  %>'></asp:TextBox>
                </EditItemTemplate>
                <ItemTemplate>
                    <asp:Label ID="Label1" runat="server" Text='<%# ProxySettings.MungTapeId(Eval("Id"))  %>'></asp:Label>
                </ItemTemplate>
            </asp:TemplateField>
            <asp:BoundField DataField="Description" HeaderText="Description" 
                SortExpression="Description" />
        </Columns>
        <EditRowStyle BackColor="#999999" />
        <EmptyDataTemplate>
            Tape library is empty! Time to record.
        </EmptyDataTemplate>
        <FooterStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
        <HeaderStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
        <PagerStyle BackColor="#284775" ForeColor="White" HorizontalAlign="Center" />
        <RowStyle BackColor="#F7F6F3" ForeColor="#333333" />
        <SelectedRowStyle BackColor="#E2DED6" Font-Bold="True" ForeColor="#333333" />
        <SortedAscendingCellStyle BackColor="#E9E7E2" />
        <SortedAscendingHeaderStyle BackColor="#506C8C" />
        <SortedDescendingCellStyle BackColor="#FFFDF8" />
        <SortedDescendingHeaderStyle BackColor="#6F8DAE" />
    </asp:GridView>
     
    <asp:Panel ID="Panel1" runat="server" GroupingText="Details" ScrollBars="Auto">
    </asp:Panel>
     
    <asp:ObjectDataSource ID="ObjectDataSource1" runat="server" 
        SelectMethod="ListTapes" TypeName="Mocument.WebUI.Code.ContextDataSource">
    </asp:ObjectDataSource>
</asp:Content>
