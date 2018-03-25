<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AddQuestions.aspx.cs" Inherits="DataCollection.AddQuestions" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Online Questions Entry :: BlackChair</title>
    <link href="Styler.css" rel="stylesheet" /><!-- hello -->
 <script src="https://ajax.googleapis.com/ajax/libs/jquery/3.1.1/jquery.min.js"></script>
    <style>
        .FishName
        {
            position: relative;
            background:#ffd800;
            padding: 13px; 
            font-size: .9em;
            padding: 0px 5px;
            display:inline-flex;
            top: 80px;
            -ms-transform: rotate(17deg); /* IE 9 */
            -webkit-transform: rotate(17deg); /* Chrome, Safari, Opera */
        }
        .rotate {

/* Safari */
-webkit-transform: rotate(-35deg);

/* Firefox */
-moz-transform: rotate(-35deg);

/* IE */
-ms-transform: rotate(-35deg);

/* Opera */
-o-transform: rotate(-35deg);

/* Internet Explorer */
filter: progid:DXImageTransform.Microsoft.BasicImage(rotation=3);

}
        .right
        {
            float:right;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <asp:Label ID="Label1" runat="server" Text="" BackColor="PeachPuff" ForeColor="Black" Font-Bold="true" ></asp:Label>
            <asp:Button ID="btn_ClosePrev" runat="server" Text="Close Previous Session" OnClick="btn_ClosePrev_Click" Visible="false" />
      <section id="MainPage">
          <h1 id="result"></h1>
            
          <center><h1 style="margin-bottom:-10px;font-size:2em;">Add new Question</h1>
          </center>
          <asp:CheckBox Text="CA Question" ID="cb_CA" runat="server" AutoPostBack="true" OnCheckedChanged="cb_CA_CheckedChanged" />
          &nbsp;&nbsp;&nbsp;<asp:CheckBox ID="cb_nopnr" runat="server" AutoPostBack="true" OnCheckedChanged="cb_nopnr_CheckedChanged" Text="PNRless Paper" />
          &nbsp;&nbsp;&nbsp;<asp:CheckBox ID="cb_MTPETP" runat="server" AutoPostBack="true" Text="MTP/ETP" OnCheckedChanged="cb_MTPETP_CheckedChanged" />
          <asp:RadioButton ID="rb_MTP" runat="server" GroupName="MTPETP" Text="MTP" visible="false" Checked="true" />
          <asp:RadioButton ID="rb_ETP" runat="server" GroupName="MTPETP" Text="ETP" Visible="false" />
          <asp:HyperLink ID="hl_HardTypedPapersLink" CssClass="right" NavigateUrl="~/AddHardTypedPaper.aspx" runat="server">Add Hard Typed Papers</asp:HyperLink>

<br />       
             <asp:TextBox ID="Tb_CA" CssClass="tb"  Visible="false" runat="server" placeholder="CA number(1st, 2nd, etc)"></asp:TextBox>
        <asp:TextBox ID="Tb_PNR" CssClass="tb"  runat="server" placeholder="PNR number"></asp:TextBox>
          <asp:TextBox ID="Tb_set" CssClass="tb"  runat="server" placeholder="Paper Set"></asp:TextBox>
         <asp:TextBox ID="Tb_CCode" CssClass="tb"  runat="server" placeholder="Course code"></asp:TextBox>
        <asp:TextBox ID="Tb_Time" CssClass="tb"  runat="server" placeholder="Time allowed"></asp:TextBox>
        <asp:TextBox ID="Tb_CName" CssClass="tb"  runat="server" placeholder="Course name"></asp:TextBox>
          <asp:DropDownList runat="server" ID="ddl_Pattern" CssClass="tb" Width ="150" >
              <asp:ListItem Value="sh-lo">sh-lo</asp:ListItem>
              <asp:ListItem Value="ob-lo">ob-lo</asp:ListItem>
              <asp:ListItem Value="ob">ob</asp:ListItem>
          </asp:DropDownList>
          <asp:TextBox ID="tb_MaxMarks" runat="server" CssClass="tb" placeholder="Maximum Marks" Visible="false"></asp:TextBox>
          <asp:TextBox runat="server" ID="tb_School" CssClass="tb" placeholder="School"></asp:TextBox>
          <hr />
          <br />
          <br />
        <asp:RadioButtonList ID="RadioButtonList1" runat="server" RepeatDirection="Horizontal" OnSelectedIndexChanged="RadioButtonList1_SelectedIndexChanged" AutoPostBack="true">
            <asp:ListItem Text="Objective" Value="1" ></asp:ListItem>
            <asp:ListItem Text="Subjective" Value="2" Selected="True"></asp:ListItem>
        </asp:RadioButtonList>
      
           <asp:TextBox ID="Tb_No" CssClass="tb" runat="server" placeholder="Question number" autofocus></asp:TextBox>
        <asp:TextBox ID="Tb_PartNo" CssClass="tb"  runat="server" placeholder="Question Part"></asp:TextBox>
        <asp:TextBox ID="Tb_SubNo" runat="server"  CssClass="tb"  placeholder="Sub part number"></asp:TextBox>
        <asp:TextBox ID="Tb_Waitage" runat="server" CssClass="tb"  placeholder="Waitage marks"></asp:TextBox>
        
   
        <asp:TextBox CssClass="tb"  runat="server" ID="Tb_Stmt" placeholder="Question statement" Rows="5" Columns="50" Height="100" onkeypress="QuestigonFormatter(event, this)" TextMode="MultiLine"></asp:TextBox>
        <asp:TextBox CssClass="tb"  runat="server" ID="Tb_CAStmt" placeholder="CA Question statement" Rows="5" Columns="50" Height="100" onkeypress="QuestigonFormatter(event, this)" TextMode="MultiLine" AutoPostBack="true" OnTextChanged="Tb_CAStmt_TextChanged" Visible="false"></asp:TextBox>

          <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>

          <asp:UpdatePanel ID="UpdatePanel1" runat="server">

              <ContentTemplate >
                  <asp:Panel ID="Panel1" runat="server"  Visible="false">
                      <div class="pnl_dup">
                <asp:Label ID="Label2" runat="server" Text=""></asp:Label>
                      Looks like this paper is already saved.<br/>
                      Possible Duplicates:<br/>
                <asp:GridView ID="GridView2" runat="server"></asp:GridView>
                      <br />
                  <asp:Button ID="btn_cancelEntry" runat="server" Text="Cancel Paper Entry" OnClick="btn_cancelEntry_Click" />
                        
                      </div>
          </asp:Panel>
                    <asp:Panel runat="server" ID="CacheControls" Visible="false">
                              <asp:Label ID="TableRows" runat="server"></asp:Label>
                          <asp:Button ID="btn_commitSync" runat="server" Text="Commit Paper Sync" BackColor="Green" OnClick="btn_commitSync_Click" />
                          <asp:Button ID="btn_cancelSync" runat="server" Text="Cancel Paper Sync" BackColor="Red" OnClick="btn_cancelSync_Click" />
                              </asp:Panel>
              </ContentTemplate>
              <Triggers>
                  <asp:AsyncPostBackTrigger ControlID="Tb_CAStmt" />
              </Triggers>
          </asp:UpdatePanel>
            
         <!-- <br /><small style="font-size:.6em;margin-left:40px">"\n" will be added to the question statement everytime you press 'Enter' key.</small>-->
                 <!-- Script to add line breaks in Question Statement -->
          <script>
              var backspaceChecker = false;
              function QuestionFormatter(e) {
                  /*
                  if (backspaceChecker == true)
                  {
                      var temp = document.getElementById('Tb_Stmt').value;
                      document.getElementById('Tb_Stmt').value = temp.substring(0, n - 1);
                      backspaceChecker = false;
                  }*/
                 // else{
                  var code = (e.keyCode ? e.keyCode : e.which);
                  if (code == 13) { //Enter keycode
                      document.getElementById('Tb_Stmt').value = document.getElementById('Tb_Stmt').value + "\\n";
                      backspaceChecker = true;
                  }//}
            }
        </script>

        <section id="Mcq" runat="server" style="visibility:hidden; display:none">
            <label id="lbl_mcq" runat="server">MCQ options</label>
            <asp:CheckBox ID="Cb_File" runat="server" OnCheckedChanged="Cb_File_CheckedChanged" AutoPostBack="true"/>Diagramatic options<br />
            <asp:TextBox CssClass="tb"  ID="Tb_Opt1" runat="server" placeholder="Option1" style="visibility:visible"></asp:TextBox>
            <asp:FileUpload ID="Fu_Opt1" runat="server" style="visibility:hidden; display:none" />
            <asp:TextBox CssClass="tb"  ID="Tb_Opt2" runat="server" placeholder="Option2" style="visibility:visible"></asp:TextBox>
            <asp:FileUpload ID="Fu_Opt2" runat="server" style="visibility:hidden; display:none" />
            <asp:TextBox CssClass="tb"  ID="Tb_Opt3" runat="server" placeholder="Option3" style="visibility:visible"></asp:TextBox>
            <asp:FileUpload ID="Fu_Opt3" runat="server" style="visibility:hidden; display:none" />
            <asp:TextBox CssClass="tb"  ID="Tb_Opt4" runat="server" placeholder="Option4" style="visibility:visible"></asp:TextBox>
            <asp:FileUpload ID="Fu_Opt4" runat="server" style="visibility:hidden; display:none" />
        </section>
        <section id="Diagramatic" runat="server" style="visibility:visible">
            <label>Diagram</label><br />
            <asp:FileUpload ID="Fu_Diag" runat="server" />
        </section>
            <asp:TextBox ID="Tb_ImageLink" CssClass="tb"  placeholder="Paper drive link" runat="server" style="max-width:500px"></asp:TextBox>

        <asp:Button ID="Button1" runat="server" Text="Add Question" OnClick="Button1_Click" OnClientClick="tes" />
        </section>
        <section>
            <br />
            <h3
               style="margin-bottom:-20px;color:#000;">
                Last question added:
            </h3>
                <p class="FishName rotate"><%= FishName %></p>

            <div style="overflow:scroll">
                <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="False" CellPadding="4" ForeColor="#333333" GridLines="None" DataKeyNames="QuestionID" DataSourceID="SqlDataSource1">
                    <AlternatingRowStyle BackColor="White" ForeColor="#284775"></AlternatingRowStyle>
                    <Columns>
                        <asp:BoundField DataField="Serial" HeaderText="Serial" ReadOnly="True" InsertVisible="False" SortExpression="Serial"></asp:BoundField>
                        <asp:BoundField DataField="QuestionID" HeaderText="QuestionID" SortExpression="QuestionID" ReadOnly="True"></asp:BoundField>
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
                <EditRowStyle BackColor="#999999"></EditRowStyle>

                <FooterStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White"></FooterStyle>

                <HeaderStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" CssClass="gridviewheader"></HeaderStyle>

                <PagerStyle HorizontalAlign="Center" BackColor="#284775" ForeColor="White"></PagerStyle>

                <RowStyle BackColor="#F7F6F3" ForeColor="#333333"></RowStyle>

                <SelectedRowStyle BackColor="#E2DED6" Font-Bold="True" ForeColor="#333333"></SelectedRowStyle>

                <SortedAscendingCellStyle BackColor="#E9E7E2"></SortedAscendingCellStyle>

                <SortedAscendingHeaderStyle BackColor="#506C8C"></SortedAscendingHeaderStyle>

                <SortedDescendingCellStyle BackColor="#FFFDF8"></SortedDescendingCellStyle>

                <SortedDescendingHeaderStyle BackColor="#6F8DAE"></SortedDescendingHeaderStyle>
            </asp:GridView>
                </div>
            <asp:SqlDataSource runat="server" ID="SqlDataSource1" ConnectionString='<%$ ConnectionStrings:ConStr %>' SelectCommand="select top 1 * from QuestionPapersDump order by Serial desc"></asp:SqlDataSource>
        </section>
        <br />
        <br />
        <center>
            <asp:HyperLink ID="hl_SearchBox" NavigateUrl="~/SearchBox.aspx" runat="server">Search Box</asp:HyperLink>
            <asp:HyperLink ID="hl_DeleteQuestion" NavigateUrl="~/QuestionEditting.aspx" runat="server">Delete Questions</asp:HyperLink>&nbsp;&nbsp;
            <asp:HyperLink ID="hl_ProdSearch" NavigateUrl="~/CourseCodeSearch.aspx" runat="server">Production Env. Search</asp:HyperLink>&nbsp;&nbsp;
            <asp:HyperLink ID="hl_SeeAllEntries" NavigateUrl="~/ViewAllQuestions.aspx" runat="server">See all entries</asp:HyperLink>&nbsp;&nbsp;
            <asp:HyperLink ID="hl_QuestionExtraction" NavigateUrl="~/QuestionsExtraction.aspx" runat="server">Questions Extraction</asp:HyperLink>&nbsp;&nbsp;
            <asp:HyperLink ID="hl_PNRSearch" NavigateUrl="~/Utilities.aspx" runat="server">Search with PNR</asp:HyperLink>&nbsp;&nbsp;
            <asp:HyperLink ID="HyperLink2" NavigateUrl="~/DownloadPaperPDF.aspx" runat="server">Download Papers</asp:HyperLink>
        </center>
        <br />
        <center><small><a href="SecurityPanel.aspx" style="font-size:10px!important;">Security Panel</a></small></center>
        <br />
<center><small style="font-size: .8em;margin-top: 10px;">BlackChair</small>   </center>
        
        <asp:Panel ID="pnl_ModalBackground" runat="server" Visible="false" CssClass="ModalHolder"> </asp:Panel>
        <asp:Panel ID="pnl_PNRlessModal"  CssClass="PNRlessModal" runat="server" Visible="false"  DefaultButton="PNRgenerator">
        Generate new PNR
                <asp:TextBox ID="tb_Modal_CCode" runat="server" Placeholder="Course Code"></asp:TextBox>
            <br />
            <asp:Button ID="PNRgenerator" runat="server" Text="Press Enter" OnClick="PNRgenerator_Click" />
            <asp:Button ID="CancelPNRless" runat="server" Text="Cancel" OnClick="CancelPNRless_Click" />
        </asp:Panel>
    </form>
</body>
</html>
