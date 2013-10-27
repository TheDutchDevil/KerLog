<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Home.aspx.cs" Inherits="KerLogVisualizer.Home" %>
<%@ Register Src="~/Controls/Header.ascx" TagPrefix="uc1" TagName="Header" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Kerlog</title>
</head>
<body>
    <form id="home" runat="server" oninit="home_Init">
        <uc1:Header runat="server" ID="Header" />
        <div id ="Content" runat="server">
            <asp:Image runat="server" id="imgAscendGraph" ImageUrl=""/>
        </div>
    </form>
</body>
</html>
