<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="ManageTapes.aspx.cs" Inherits="Mocument.Web.Manage.ManageTapes" %>

<%@ Import Namespace="Mocument.Web.Code" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
  
    <asp:ObjectDataSource ID="ObjectDataSource1" runat="server" DeleteMethod="Delete"
        InsertMethod="Insert" SelectMethod="SelectForCurrentUser" TypeName="Mocument.Web.Code.TapeLibraryDataSource"
        UpdateMethod="Update">
        <DeleteParameters>
            <asp:Parameter Name="Id" Type="String" />
        </DeleteParameters>
        <InsertParameters>
            <asp:Parameter Name="Id" Type="String" />
            <asp:Parameter Name="Description" Type="String" />
            <asp:Parameter Name="OpenForRecording" Type="Boolean" />
            <asp:Parameter Name="AllowedIpAddress" Type="String" />
        </InsertParameters>
        <UpdateParameters>
            <asp:Parameter Name="Id" Type="String" />
            <asp:Parameter Name="Description" Type="String" />
            <asp:Parameter Name="OpenForRecording" Type="Boolean" />
            <asp:Parameter Name="AllowedIpAddress" Type="String" />
        </UpdateParameters>
    </asp:ObjectDataSource>
    Your IP:
    <asp:Label ID="IpLabel" runat="server"></asp:Label>
    <br />
    <asp:GridView ID="GridView1" runat="server" AllowPaging="True" AutoGenerateColumns="False"
        DataKeyNames="Id" DataSourceID="ObjectDataSource1" 
        ShowHeaderWhenEmpty="True">
        <Columns>
            <asp:CommandField ShowDeleteButton="True" ShowEditButton="True" />
            <asp:TemplateField HeaderText="Id" SortExpression="Id">
                <EditItemTemplate>
                    <asp:TextBox ID="TextBox1" runat="server" Text='<%# Bind("Id") %>' Enabled="False"></asp:TextBox>
                </EditItemTemplate>
                <ItemTemplate>
                    <asp:Label ID="Label1" runat="server" Text='<%# Bind("Id") %>'></asp:Label>
                </ItemTemplate>
                <FooterTemplate>
                </FooterTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="Description" SortExpression="Description">
                <EditItemTemplate>
                    <asp:TextBox ID="TextBox2" runat="server" Text='<%# Bind("Description") %>'></asp:TextBox>
                </EditItemTemplate>
                <ItemTemplate>
                    <asp:Label ID="Label2" runat="server" Text='<%# Bind("Description") %>'></asp:Label>
                </ItemTemplate>
                <FooterTemplate>
                </FooterTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="OpenForRecording" SortExpression="OpenForRecording">
                <EditItemTemplate>
                    <asp:CheckBox ID="CheckBox1" runat="server" Checked='<%# Bind("OpenForRecording") %>' />
                </EditItemTemplate>
                <ItemTemplate>
                    <asp:CheckBox ID="CheckBox1" runat="server" Checked='<%# Bind("OpenForRecording") %>'
                        Enabled="false" />
                </ItemTemplate>
                <FooterTemplate>
                </FooterTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="AllowedIpAddress" SortExpression="AllowedIpAddress">
                <EditItemTemplate>
                    <asp:TextBox ID="TextBox3" runat="server" Text='<%# Bind("AllowedIpAddress") %>'></asp:TextBox>
                </EditItemTemplate>
                <ItemTemplate>
                    <asp:Label ID="Label3" runat="server" Text='<%# Bind("AllowedIpAddress") %>'></asp:Label>
                </ItemTemplate>
                <FooterTemplate>
                </FooterTemplate>
            </asp:TemplateField>
        </Columns>
    </asp:GridView>
    <br />
    <p>
        ID: <asp:Label ID="Label4" runat="server" Text='<%# GetUserName() %>'></asp:Label><asp:TextBox
            ID="AddIdTextBox" runat="server" Text=''></asp:TextBox>
        <br />
        DESCRIPTION: <asp:TextBox ID="AddDescriptionTextBox" runat="server" Text=''></asp:TextBox>

        <br />
        OPEN FOR RECORDING: <asp:CheckBox ID="AddOpenCheckbox" runat="server" Checked='true' Enabled="true" />
        <br />
        ALLOWED IP: <asp:TextBox ID="AddIPTextBox" runat="server" Text='<%# TapeLibraryDataSource.GetIpAddress() %>'></asp:TextBox>
        <br />
        <asp:Button ID="AddTapeButton" runat="server" OnClick="AddTapeButton_Click" Text="Add Tape" />
    </p>
</asp:Content>
