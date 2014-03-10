<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="_Default" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Strict//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Northwind Traders Online</title>
    <link href="Reset.css" rel="stylesheet" type="text/css" media="all" />
    <link href="styles.css" rel="stylesheet" type="text/css" media="all" />
</head>
<body>
    <div id="container">
        <div id="header">
            <h1>northwind traders online</h1>
            <p class="topLinks">This Week&#39;s Sale | Shipping Information | About Us</p>
        </div>
        <div id="left">
            <h3>Categories</h3>
            <ul>
                <li class="<%= ImageOptimizations.MakeCssClassName("~/App_Sprites/categories/windowsLogo.png") %>"><a href="#" class="categories">Operating Systems</a></li>
                <li class="<%= ImageOptimizations.MakeCssClassName("~/App_Sprites/categories/dynamics.png") %>"><a href="#" class="categories">Business Software</a></li>
                <li class="<%= ImageOptimizations.MakeCssClassName("~/App_Sprites/categories/exchange.png") %>"><a href="#" class="categories">Server Software</a></li>
                <li class="<%= ImageOptimizations.MakeCssClassName("~/App_Sprites/categories/azureLogo.png") %>"><a href="#" class="categories">Cloud Services</a></li>
                <li class="<%= ImageOptimizations.MakeCssClassName("~/App_Sprites/categories/dotNet.png") %>"><a href="#" class="categories">Programming</a></li>
                <li class="<%= ImageOptimizations.MakeCssClassName("~/App_Sprites/categories/expression.png") %>"><a href="#" class="categories">Design</a></li>
            </ul>
            <h3>Top Sellers</h3>
            <ul>
                <li class="<%= ImageOptimizations.MakeCssClassName("~/App_Sprites/categories/windowsLogo.png") %>"><a href="#" class="categories">Windows 7</a></li>
                <li class="<%= ImageOptimizations.MakeCssClassName("~/App_Sprites/categories/xbox.png") %>"><a href="#" class="categories">XBOX 360</a></li>
                <li class="<%= ImageOptimizations.MakeCssClassName("~/App_Sprites/categories/office.png") %>"><a href="#" class="categories">Office 2010</a></li>
                <li class="<%= ImageOptimizations.MakeCssClassName("~/App_Sprites/categories/visualStudio.png") %>"><a href="#" class="categories">Visual Studio 2010</a></li>
            </ul>
        </div>
        <div id="right">
            <div id="content">
                <p>&nbsp;</p>
                <p><img src="Images/visualStudio.png" alt="Visual Studio 2010" /></p>
                <p class="descriptionText">
                    Visual Studio 2010 has arrived, and brings a whole suite of advancements to the
                    venerable IDE, including a new user interface, functional programming with F#, .NET 4.0, support for
                    Windows Phone 7 and XBOX, and advanced refactoring abilities.
                </p>
                <h2>Popular Now</h2>
                <table class="mainTable">
                    <tr>
                        <td class="tableCell">
                            <a href="#">
                                <asp:ImageSprite runat="server" ImageUrl="~/App_Sprites/popular/windowsLogo.png" GenerateEmptyAlternateText="true" />
                                <br />
                                Windows 7
                            </a>
                        </td>
                        <td class="tableCell">
                            <a href="#">
                                <asp:ImageSprite runat="server" ImageUrl="~/App_Sprites/popular/xbox.png" GenerateEmptyAlternateText="true" />
                                <br />
                                XBOX 360
                            </a>
                        </td>
                        <td class="tableCell">
                            <a href="#">
                                <asp:ImageSprite runat="server" ImageUrl="~/App_Sprites/popular/office.png" GenerateEmptyAlternateText="true" />
                                <br />
                                Office 2010 Home
                            </a>
                        </td>
                        <td class="tableCell">
                            <a href="#">
                                <asp:ImageSprite runat="server" ImageUrl="~/App_Sprites/popular/office.png" GenerateEmptyAlternateText="true" />
                                <br />
                                Office 2010 Pro
                            </a>
                        </td>
                    </tr>
                    <tr>
                        <td class="tableCell">
                            <a href="#">
                                <asp:ImageSprite runat="server" ImageUrl="~/App_Sprites/popular/mesh.png" GenerateEmptyAlternateText="true" />
                                <br />
                                Windows Live Mesh
                            </a>
                        </td>
                        <td class="tableCell">
                            <a href="#">
                                <asp:ImageSprite runat="server" ImageUrl="~/App_Sprites/popular/exchange.png" GenerateEmptyAlternateText="true" />
                                <br />
                                Exchange 2010
                            </a>
                        </td>
                        <td class="tableCell">
                            <a href="#">
                                <asp:ImageSprite runat="server" ImageUrl="~/App_Sprites/popular/azureLogo.png" GenerateEmptyAlternateText="true" />
                                <br />
                                Azure Subscriptions
                            </a>
                        </td>
                        <td class="tableCell">
                            <a href="#">
                                <asp:ImageSprite runat="server" ImageUrl="~/App_Sprites/popular/visualStudio.png" GenerateEmptyAlternateText="true" />
                                <br />
                                Visual Studio 2010
                            </a>
                        </td>
                    </tr>
                </table>
                <h2>New In Stock</h2>
                <p><img class="bottomImage" src="Images/newXbox.png" alt="The new XBOX 360" /></p>
            </div>
        </div>
    </div>
    <!--This links the proper CSS file for the icons on the left pane. -->
    <asp:ImageSpriteCssLink runat="server" ImageUrl="~/App_Sprites/categories" />
</body>
</html>