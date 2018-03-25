<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CourseCodeSearch.aspx.cs" Inherits="DataCollection.CourseCodeSearch" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Production Environment Search :: BlackChair</title>
    <link href="Styler.css" rel="stylesheet" />
       <script src="http://ajax.aspnetcdn.com/ajax/jQuery/jquery-1.10.0.min.js" type="text/javascript"></script>
    <script src="http://ajax.aspnetcdn.com/ajax/jquery.ui/1.9.2/jquery-ui.min.js" type="text/javascript"></script>
    <link href="http://ajax.aspnetcdn.com/ajax/jquery.ui/1.9.2/themes/blitzer/jquery-ui.css"
           rel="Stylesheet" type="text/css" />
  
    <style>

        .DynamicPaperLinks
        {
            margin: 10px;
            display:block;
        }
        #pnl_SearchResults
        {
            border-top: 1px solid black;
            max-width: 400px;
        }
        
    </style>
</head>
<body>

    <form id="form1" runat="server">
    <div>
       <center>
            <asp:HyperLink ID="HyperLink1" NavigateUrl="~/AddQuestions.aspx" runat="server">Go back</asp:HyperLink>
        <br />
           </center>
        <h1 style=" font-size:2em;">Production Environment Search (Beta)</h1>
   
                 <div class="progressHolder">
            <asp:UpdateProgress ID="UpdateProgress2" runat="server">
                <ProgressTemplate>
                <center>    <div class="loader"></div></center>
                </ProgressTemplate>
            </asp:UpdateProgress>
                </div>

        <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
                  <asp:TextBox ID="tb_CourseCode" runat="server" placeholder="Start entering Course Code..." Width="300"></asp:TextBox>

        <asp:UpdatePanel ID="UpdatePanel1" runat="server">
       <ContentTemplate>
            
        <br />
           
        <asp:Button ID="btn_FindPapers" CssClass="bcb" runat="server" Text="Find Papers" OnClick="btn_FindPapers_Click" />
        <asp:Label ID="lbl_msg" runat="server" BackColor="Wheat"></asp:Label>
        <script>
            $(function () {
                $("[id$=tb_CourseCode]").autocomplete({
                source: function (request, response) {
                    $.ajax({
                        url: '<%=ResolveUrl("~/CourseCodeSearch.aspx/GetCustomers") %>',
                        data: "{ 'prefix': '" + request.term + "'}",
                        dataType: "json",
                        type: "POST",
                        contentType: "application/json; charset=utf-8",
                        success: function (data) {
                            response(data.d)
                        },
                        error: function (response) {
                            console.log("Error: while retreiving data from server for autocomplete");
                        },
                        failure: function (response) {
                            console.log("Failure: while retreiving data from server for autocomplete");
                        }
                    });
                },
            //    select: function (e, i) {
                //    $("#hf_CourseCode").val(i.item.val);
              //  },
                minLength: 1,
                autofocus: true
            });

                $("[id$=tb_CourseCode]").autocomplete("option", "autoFocus", true);
          //  $("#hf_CourseCode").on("autocompleteselect", function (event, ui) {
             //   $("#hf_CourseCode").val(ui.item.val);
           // });
        });  
           </script> 
        <asp:Panel runat="server"  ID="pnl_SearchResults">
        </asp:Panel>
      </ContentTemplate>
            <Triggers>
                <asp:AsyncPostBackTrigger ControlID="btn_FindPapers" />
            </Triggers>
        </asp:UpdatePanel>
       
<center><small style="font-size: .8em;margin-top: 10px;">BlackChair</small>   </center>
    </div>
    </form>
</body>
</html>

