<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true"
    CodeBehind="Default.aspx.cs" Inherits="Mocument.Web._Default" %>

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
</asp:Content>
