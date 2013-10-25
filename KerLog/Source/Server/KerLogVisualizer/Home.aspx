<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Home.aspx.cs" Inherits="KerLogVisualizer.Home" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Kerlog</title>
    <link href="CSS/Header.css" rel="stylesheet" type="text/css" />
</head>
<body>
    <form id="home" runat="server">
        <div id="Header" class="headerBackground" >
            <div id="HeaderBackgroundImage" class="headerImageBackground">
                <div id="HeadText" class="headerTitleTextBackground">
                    <span class="headerTitleText">Kerlog Visualizer</span>
                </div>
            </div>
        </div>
        <div id="Menu">

        </div>
        <div id ="Content">

            <asp:MultiView ID="mvContent" runat="server">
            </asp:MultiView>

        </div>
    </form>
</body>
</html>
