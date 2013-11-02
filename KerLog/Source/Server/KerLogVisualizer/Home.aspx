<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Home.aspx.cs" Inherits="KerLogVisualizer.Home" %>

<%@ Register Assembly="GoogleChartsNGraphsControls" Namespace="GoogleChartsNGraphsControls" TagPrefix="cc1" %>
<%@ Register Src="~/Controls/Header.ascx" TagPrefix="uc1" TagName="Header" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Kerlog</title>
    <link href="..\CSS/Home.css" rel="stylesheet" type="text/css" />
</head>
<body class="body">
    <form id="home" runat="server" oninit="home_Init">
        <uc1:Header runat="server" ID="Header" />
        <div id ="Content" runat="server">
            <cc1:GVLineChart ID="GVLineChart1" CssClass="visualization" runat="server" />
        </div>
    </form>
</body>
</html>
