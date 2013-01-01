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
    insert brief introduction here<br />
    tape list visible only to owner of tape. owner can share tape url<br />
    </asp:Content>
