<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="MyTapes.aspx.cs" Inherits="Mocument.Web.Manage.MyTapes" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <asp:EntityDataSource ID="EntityDataSource1" runat="server" ConnectionString="name=TapesEntities"
        DefaultContainerName="TapesEntities" EnableDelete="True" EnableFlattening="False"
        EnableInsert="True" EnableUpdate="True" EntitySetName="Tapes" 
        ondeleted="EntityDataSource1_Deleted" oninserted="EntityDataSource1_Inserted" onupdated="EntityDataSource1_Updated" 
         >
    </asp:EntityDataSource>
    <p>   Your IP:
    <asp:Label ID="IpLabel" runat="server"></asp:Label></p>

    <asp:GridView ID="GridView1" runat="server" AllowPaging="True" AllowSorting="True"
        AutoGenerateColumns="False" DataKeyNames="Id" DataSourceID="EntityDataSource1">
        <Columns>
            <asp:CommandField ShowDeleteButton="True" ShowEditButton="True" />
            <asp:TemplateField HeaderText="Id" SortExpression="Id">
                <EditItemTemplate>
                    <asp:Label ID="Label1" runat="server" Text='<%# string.Format("{0}",Eval("Id").ToString().Split(new string[]{"."},StringSplitOptions.RemoveEmptyEntries)[1]) %>'></asp:Label>
                </EditItemTemplate>
                <ItemTemplate>
                    <asp:Label ID="Label1" runat="server" Text='<%# string.Format("{0}",Eval("Id").ToString().Split(new string[]{"."},StringSplitOptions.RemoveEmptyEntries)[1]) %>'></asp:Label>
                </ItemTemplate>
            </asp:TemplateField>
            <asp:BoundField DataField="Description" HeaderText="Description" SortExpression="Description" />
            <asp:CheckBoxField DataField="OpenForRecording" HeaderText="OpenForRecording" SortExpression="OpenForRecording" />
            <asp:BoundField DataField="AllowedIpAddress" HeaderText="AllowedIpAddress" SortExpression="AllowedIpAddress" />
        </Columns>
        <EmptyDataTemplate>
            User has no tapes.
        </EmptyDataTemplate>
    </asp:GridView>
    <br />
    <asp:Panel ID="Panel1" runat="server" GroupingText="Create New Tape">
        <br />
   <br />
    <table>
        <tr>
            <td>
                Name:
            </td>
            <td>
                <asp:TextBox ID="AddNameTextBox" runat="server"></asp:TextBox>
                <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" 
                    ControlToValidate="AddNameTextBox" ErrorMessage="Tape Name Required" 
                    ForeColor="Red"></asp:RequiredFieldValidator>
            </td>
            <td>
                &nbsp;
            </td>
        </tr>
        <tr>
            <td>
                Description:
            </td>
            <td>
                <asp:TextBox ID="AddDescrTextBox" runat="server" TextMode="MultiLine"></asp:TextBox>
            </td>
            <td>
                &nbsp;
            </td>
        </tr>
        <tr>
            <td>
                Open for recording:
            </td>
            <td>
                <asp:CheckBox ID="AddOpenCheckBox" runat="server" Checked="True" />
            </td>
            <td>
                &nbsp;
            </td>
        </tr>
        <tr>
            <td>
                Allowed IP:
            </td>
            <td>
                <asp:TextBox ID="AddIPTextBox" runat="server"></asp:TextBox>
            </td>
            <td>
                &nbsp;
            </td>
        </tr>
        <tr>
            <td colspan="2">
                <asp:Label ID="ErrorLabel" runat="server" EnableViewState="False" 
                    ForeColor="Red"></asp:Label>
            </td>
            <td>
                <asp:Button ID="AddButton" runat="server" OnClick="AddButtonClick" Text="Add" />
            </td>
        </tr>
    </table>
    </asp:Panel>

</asp:Content>
