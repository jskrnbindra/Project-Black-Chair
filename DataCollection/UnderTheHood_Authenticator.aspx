<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="UnderTheHood_Authenticator.aspx.cs" Inherits="DataCollection.UnderTheHood_Authenticator" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>BlackChair - Admin Authentication</title>
    <style>
        .Invisible
        {
            opacity:0;
        }
        .tb
        {
            width: 250px;
            height: 30px;
            font-size: 1em;
            padding: 5px;
        }
        body
        {
            background: #fbfbfb;
        }
    </style>
</head>

<body>
    <form id="form1" runat="server">
    <div>
        <center>
<img src="Icons/itslocked.jpg" />
        <asp:Panel ID="pnl_Logger" runat="server" DefaultButton="btn_logger">
        <asp:TextBox ID="tb_PassKey" TextMode="Password" CssClass="tb" runat="server" Placeholder="Enter the PassKey to unlock" autofocus></asp:TextBox>
        <asp:Button ID="btn_logger" runat="server" Height="0" Width="0" OnClick="btn_logger_Click" CssClass="Invisible" />
            <br /><br /><asp:Label runat="server" ID="lbl_LoginMsg"></asp:Label>
            </asp:Panel>
            </center>
    </div>
    </form>
</body>
</html>