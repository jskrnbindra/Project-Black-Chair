<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="QuestionsExtraction.aspx.cs" Inherits="DataCollection.QuestionsExtraction" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Extract Questions :: BlackChair</title>
    <link href="Styler.css" rel="stylesheet" />
</head>
<body>
    <form id="form1" runat="server">
         <center>
            <asp:HyperLink ID="HyperLink1" NavigateUrl="~/AddQuestions.aspx" runat="server">Go back</asp:HyperLink>
        </center>
        <br />
    <div>
        <h1 style="font-size: 2em;">Extract Questions by type (Weightage)</h1>
        Select question type according to weightage:
    <asp:DropDownList runat="server" ID="ddl_QuestionType">
        <asp:ListItem Text="1 mark" Value="1" />
        <asp:ListItem Text="2 and 2.5 marks" Value="2" />
        <asp:ListItem Text="5 marks" Value="3" />
        <asp:ListItem Text="10 marks" Value="4" />
        <asp:ListItem Text="15 marks" Value="5" />
    </asp:DropDownList><br />
        <asp:TextBox runat="server" ID="tb_CourseCode" placeholder="Course Code" />&nbsp;(leave empty to display questions from entire database)<br />
        <asp:CheckBox runat="server" ID="cb_IncludeCA" Text="Include CA Questions" /><br />
        <asp:Button runat="server" ID ="btn_ShowQuestions" OnClick="btn_ShowQuestions_Click" Text="Show Questions" />
        <br />
        <asp:Label runat="server" Id="lbl_msg" Font-Bold="true" Font-Size="22" ForeColor="Teal" BackColor="Silver" Visible="false" Text="Looks like the Course Code you entered is invalid. Check Course Code and try again. If the problem persists contact admin. +91 9988-255-277" />
        <br />
        <asp:HyperLink ID="hl_ViewPDF" runat="server" Text="View PDF" Visible="false" />
    </div>
<center><small style="font-size: .8em;margin-top: 10px;">BlackChair</small>   </center>
    </form>
</body>
</html>