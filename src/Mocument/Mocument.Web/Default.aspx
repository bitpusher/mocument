<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true"
    CodeBehind="Default.aspx.cs" Inherits="Mocument.Web._Default" %>

<%@ Import Namespace="Mocument.Web.Code" %>
<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="HeadContent">
    <style type="text/css">
        .style1
        {
            font-size: xx-small;
        }
    </style>
</asp:Content>
<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="MainContent">
    <h2>
        Welcome to mocument.it!
    </h2>
    <p>
        I will fulfill all of your fantasies ....
    </p>
    <p class="style1">
        ... that is, if all you fantasize about is easy recording and playback of http traffic.
    </p>
    
    <p class="style1">
        <asp:EntityDataSource ID="EntityDataSource1" runat="server" 
            ConnectionString="name=TapesEntities" DefaultContainerName="TapesEntities" 
            EnableFlattening="False" EntitySetName="Tapes">
        </asp:EntityDataSource>
    </p>
    <p>
        <asp:GridView ID="GridView1" runat="server" DataSourceID="EntityDataSource1" 
            AutoGenerateColumns="False" DataKeyNames="Id">
            <Columns>
                <asp:TemplateField HeaderText="Id" SortExpression="Id">
                    <EditItemTemplate>
                        <asp:Label ID="Label1" runat="server" Text='<%# string.Format("{0}",string.Join("/",Eval("Id").ToString().Split(new string[]{"."},StringSplitOptions.RemoveEmptyEntries))) %>'></asp:Label>
                    </EditItemTemplate>
                    <ItemTemplate>
                        <asp:Label ID="Label1" runat="server" Text='<%# string.Format("{0}",string.Join("/",Eval("Id").ToString().Split(new string[]{"."},StringSplitOptions.RemoveEmptyEntries))) %>'></asp:Label>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:BoundField DataField="Description" HeaderText="Description" 
                    SortExpression="Description" />
            </Columns>
        </asp:GridView>
    </p>
</asp:Content>
