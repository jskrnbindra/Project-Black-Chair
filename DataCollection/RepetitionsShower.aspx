
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="RepetitionsShower.aspx.cs" Inherits="DataCollection.RepetitionsShower" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Statement + Repeated Papers :: BlackChair</title>
    <link href="Styler.css" rel="stylesheet" />
    <style>
        .RepeatedPapersPanel
        {
            width:500px;
            margin:0 auto;
            padding: 40px;
            box-shadow: 1px 2px 2px 3px rgba(0,0,0,0.1);
            border-radius: 5px;
            background: #fff;
        }
        .DynamicLinks{
            display:block;
            text-decoration: none;
            padding: 5px;
        }
        .SamePaperLink
        {
            background-color:lavender;
        }
        .SamePaperLink::after
        {
            content:'same paper';
            display:block;
            color:black;
            font-size: .6em;
        }
        
        .DynamicLinks:hover
        {
            text-decoration: underline;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
    <asp:Panel runat="server" ID="pnl_LinksHolder" CssClass="RepeatedPapersPanel">
        <h1>This question was repeated in the following papers:</h1><br /><br />
    </asp:Panel>
<center><small style="font-size: .8em;margin-top: 10px;">BlackChair</small>   </center>
    </form>
</body>
</html>
