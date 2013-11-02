<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Header.ascx.cs" Inherits="KerLogVisualizer.Views.ActiveFlightAscendProfile" %>

    <link href="..\CSS/Header.css" rel="stylesheet" type="text/css" />

        <div id="Header" class="headerBackground" >
            <div id="HeaderBackgroundImage" class="headerImageBackground">
                <div id="HeadText" class="headerTitleTextBackground">
                    <span class="headerTitleText">Kerlog Visualizer</span>
                </div>
            </div>
        </div>
        <div id="Menu">
            <asp:Table id="spCommand" runat="server" CssClass="commandTable">
            </asp:Table>
        </div>