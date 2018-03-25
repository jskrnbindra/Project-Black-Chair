<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ViewAllQuestions.aspx.cs" Inherits="DataCollection.ViewAllQuestions" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>All questions :: BlackChair</title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
         <center>
            <asp:HyperLink ID="HyperLink1" NavigateUrl="~/AddQuestions.aspx" runat="server">Go back</asp:HyperLink>
        </center>
           <br />
        <br />
        <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="False" DataKeyNames="QuestionID" DataSourceID="SqlDataSource1" CellPadding="4" ForeColor="#333333" GridLines="None" AllowSorting="True">
            <AlternatingRowStyle BackColor="White"></AlternatingRowStyle>
            <Columns>
                <asp:BoundField DataField="Serial" HeaderText="Serial" ReadOnly="True" InsertVisible="False" SortExpression="Serial"></asp:BoundField>
                <asp:BoundField DataField="QuestionID" HeaderText="QuestionID" ReadOnly="True" SortExpression="QuestionID"></asp:BoundField>
                <asp:BoundField DataField="Number" HeaderText="Number" SortExpression="Number"></asp:BoundField>
                <asp:BoundField DataField="Part" HeaderText="Part" SortExpression="Part"></asp:BoundField>
                <asp:BoundField DataField="SubPart" HeaderText="SubPart" SortExpression="SubPart"></asp:BoundField>
                <asp:BoundField DataField="Statement" HeaderText="Statement" SortExpression="Statement"></asp:BoundField>
                <asp:BoundField DataField="Diagram" HeaderText="Diagram" SortExpression="Diagram"></asp:BoundField>
                <asp:BoundField DataField="Option1" HeaderText="Option1" SortExpression="Option1"></asp:BoundField>
                <asp:BoundField DataField="Option1File" HeaderText="Option1File" SortExpression="Option1File"></asp:BoundField>
                <asp:BoundField DataField="Option2" HeaderText="Option2" SortExpression="Option2"></asp:BoundField>
                <asp:BoundField DataField="Option2File" HeaderText="Option2File" SortExpression="Option2File"></asp:BoundField>
                <asp:BoundField DataField="Option3" HeaderText="Option3" SortExpression="Option3"></asp:BoundField>
                <asp:BoundField DataField="Option3File" HeaderText="Option3File" SortExpression="Option3File"></asp:BoundField>
                <asp:BoundField DataField="Option4" HeaderText="Option4" SortExpression="Option4"></asp:BoundField>
                <asp:BoundField DataField="Option4File" HeaderText="Option4File" SortExpression="Option4File"></asp:BoundField>
                <asp:BoundField DataField="Weightage" HeaderText="Weightage" SortExpression="Weightage"></asp:BoundField>
                <asp:BoundField DataField="PNR" HeaderText="PNR" SortExpression="PNR"></asp:BoundField>
                <asp:BoundField DataField="CourseCode" HeaderText="CourseCode" SortExpression="CourseCode"></asp:BoundField>
                <asp:BoundField DataField="CourseName" HeaderText="CourseName" SortExpression="CourseName"></asp:BoundField>
                <asp:BoundField DataField="MaxTimeAllowed" HeaderText="MaxTimeAllowed" SortExpression="MaxTimeAllowed"></asp:BoundField>
                <asp:BoundField DataField="PaperSet" HeaderText="PaperSet" SortExpression="PaperSet"></asp:BoundField>
                <asp:BoundField DataField="DriveLink" HeaderText="DriveLink" SortExpression="DriveLink"></asp:BoundField>
                <asp:BoundField DataField="TimeStamps" HeaderText="TimeStamps" SortExpression="TimeStamps"></asp:BoundField>
            </Columns>
            <EditRowStyle BackColor="#7C6F57"></EditRowStyle>

            <FooterStyle BackColor="#1C5E55" Font-Bold="True" ForeColor="White"></FooterStyle>

            <HeaderStyle BackColor="#1C5E55" Font-Bold="True" ForeColor="White"></HeaderStyle>

            <PagerStyle HorizontalAlign="Center" BackColor="#666666" ForeColor="White"></PagerStyle>

            <RowStyle BackColor="#E3EAEB"></RowStyle>

            <SelectedRowStyle BackColor="#C5BBAF" Font-Bold="True" ForeColor="#333333"></SelectedRowStyle>

            <SortedAscendingCellStyle BackColor="#F8FAFA"></SortedAscendingCellStyle>

            <SortedAscendingHeaderStyle BackColor="#246B61"></SortedAscendingHeaderStyle>

            <SortedDescendingCellStyle BackColor="#D4DFE1"></SortedDescendingCellStyle>

            <SortedDescendingHeaderStyle BackColor="#15524A"></SortedDescendingHeaderStyle>
        </asp:GridView>
    <asp:SqlDataSource ID="SqlDataSource1" runat="server" ConnectionString='<%$ ConnectionStrings:ConStr %>' SelectCommand="select * from QuestionPapersDump order by serial desc"></asp:SqlDataSource>
    </div>
<center><small style="font-size: .8em;margin-top: 10px;">BlackChair</small>   </center>
    </form>
</body>
</html>
