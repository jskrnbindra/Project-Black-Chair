<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="QuestionEditting.aspx.cs" Inherits="DataCollection.WebForm2" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Delete Question ::  BlackChair</title>
      <link href="Styler.css" rel="stylesheet" />
  
</head>
<body>
    <form id="form1" runat="server">
    <div>
       <center>
            <asp:HyperLink ID="HyperLink1" NavigateUrl="~/AddQuestions.aspx" runat="server">Go back</asp:HyperLink>
        <br />
           </center>
        <h1 style=" font-size:2em;">Delete Questions (Beta)</h1>
   
                 <div class="progressHolder">
            <asp:UpdateProgress ID="UpdateProgress2" runat="server">
                <ProgressTemplate>
                <center>    <div class="loader"></div></center>
                </ProgressTemplate>
            </asp:UpdateProgress>
                </div>

        <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
        <asp:UpdatePanel runat="server" ID="up_Holder">
            <ContentTemplate>
        
                <asp:DropDownList ID="ddl_Selector" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddl_Selector_SelectedIndexChanged">
            <asp:ListItem Text="Delete by Question ID" Value="QuestionID"></asp:ListItem>
            <asp:ListItem Text="Delete by Serial"  Value="Serial" Selected="true"></asp:ListItem>        
                </asp:DropDownList>
        <asp:TextBox ID="tb_QuestionID" CssClass="tb" runat="server" PlaceHolder ="Question ID" Visible="false" ></asp:TextBox>
        <br />
        <asp:TextBox ID="tb_QuestionSerial" CssClass="tb" runat="server" PlaceHolder="Serial"></asp:TextBox>
        <br />
        <asp:Button ID="btn_Delete" runat="server" CssClass="btn" Text="Delete" CommandArgument="Serial" CommandName="Show" OnClick="btn_Delete_Click" />
                <asp:HyperLink runat="server" ID="hl_Self" Text="cancel" NavigateUrl="~/QuestionEditting.aspx" Visible="false" />
        <br />
        <br />
         

        <asp:GridView ID="gv_QuestionDetails" runat="server">
            <EmptyDataTemplate>This question does not exist</EmptyDataTemplate>
        </asp:GridView>

                </ContentTemplate>
            <Triggers>
                <asp:AsyncPostBackTrigger ControlID="ddl_Selector" />
                <asp:AsyncPostBackTrigger ControlID="btn_Delete" />
            </Triggers>
            </asp:UpdatePanel>
    </div>
    </form>
<center><small style="font-size: .8em;margin-top: 10px;">BlackChair</small>   </center>

</body>
</html>
