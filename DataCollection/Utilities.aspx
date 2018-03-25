<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Utilities.aspx.cs" Inherits="DataCollection.Utilities" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Utilities :: BlackChair</title>
    <link href="Styler.css" rel="stylesheet" />

</head>
<body>
    <form id="form1" runat="server">
          <center>
            <asp:HyperLink ID="HyperLink1" NavigateUrl="~/AddQuestions.aspx" runat="server">Go back</asp:HyperLink>
        </center>
           <br />
    
    <section id="PNRSearch" runat="server">
    <h1>Search by PNR</h1>
        <br />
        <asp:TextBox ID="tb_PNR" CssClass="tb" runat="server" placeholder="PNR number" autofocus></asp:TextBox><br />
        <asp:Button ID="btn_PNRSearch" CssClass="btn" runat="server" Text="Search" OnClick="btn_PNRSearch_Click"/><br />
        <asp:Label ID="lbl_PNRlabel" runat="server" Visible="false" CssClass="lbl" Text="" Font-Bold="true"></asp:Label>
    </section>

        <asp:GridView ID="gv_HardResults" runat="server">
            <EmptyDataTemplate>
                No Hard Typed Paper found for this PNR
            </EmptyDataTemplate>
        </asp:GridView>
        <asp:GridView ID="gv_Reader" runat="server">
            <EmptyDataTemplate>
                No Paper found for this PNR
            </EmptyDataTemplate>
        </asp:GridView>

<center><small style="font-size: .8em;margin-top: 10px;">BlackChair</small>   </center>
    </form>
</body>
</html>
