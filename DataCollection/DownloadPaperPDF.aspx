<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="DownloadPaperPDF.aspx.cs" Inherits="DataCollection.PDFtester" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Download Question Paper :: BlackChair</title>
    <link href="Styler.css" rel="stylesheet" />
</head>
<body>
    <form id="form1" runat="server">
            <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
        <center>
            <asp:HyperLink ID="HyperLink1" NavigateUrl="~/AddQuestions.aspx" runat="server">Go back</asp:HyperLink>
        </center>
        <br />
        <div>
            <div class="progressHolder">
            <asp:UpdateProgress ID="UpdateProgress1" runat="server">
                <ProgressTemplate>
                <center>    <div class="loader"></div></center>
                </ProgressTemplate>
            </asp:UpdateProgress>
                </div>
            <h2>PDfreakin'F (beta)</h2>
            <h1 style="margin-bottom: -10px; font-size: 2em;">Download Question Paper PDFs</h1>
            <br />
            <asp:UpdatePanel ID="UpdatePanel1" runat="server">
                <ContentTemplate>

            <asp:TextBox ID="TextBox1" runat="server" CssClass="tb" placeholder="PNR number" autofocus></asp:TextBox>
            <asp:Button ID="Button2" runat="server" Text="Generate Paper" CssClass="btn" OnClick="Button2_Click" />
            <asp:Button ID="btn_SearchHardPapers" runat="server"  CssClass="btn" Text="Search in Hard Typed papers" OnClick="btn_SearchHardPapers_Click" />
            <asp:Label ID="lbl_HardResult" runat="server"></asp:Label>
            <asp:HyperLink ID="hl_HardResult" runat="server"></asp:HyperLink>
            <asp:Label runat="server" ForeColor="RosyBrown" BackColor="SkyBlue" Text="" ID="lbl_temp" Visible="false"></asp:Label>
            <br />
            <br />
            <asp:Label runat="server" ID ="staticDisplay" Text="View undownloadable version online here:" Visible ="false"></asp:Label>&nbsp;
                    <script>
                        var windowObjRef = null;
                        var counter = 0;

                        function ShowPaper() {
                            console.log("opening new");
                //            if (windowObjRef == null || windowObjRef.closed) {
                                var URL = document.getElementById("staticDisplayer1").href;

                                var params = [
                            'height=' + screen.height,
                            'width=' + screen.width,
                            'fullscreen=yes',// only works in IE, but here for completeness
                            'resizable = no',
                            'scrollbars = auto'
                                ].join(',');

                                windowObjRef = window.open(URL, ++counter, params, "toolbar=no,menubar=no");
                       //     }
                         //   else {
                           //     windowObjRef.focus();
                            //};

                        }

                    </script>
                    <br />
                    <a href="test" runat="server" onclick="ShowPaper(); return false;" id="staticDisplayer1" target="_blank" visible="false">View paper [Undownloadable]</a>
            <br />
            <br />
            <asp:Label ID="paperlink" runat="server" Text=""></asp:Label><br />
            <a href="PDFs/Doc1.pdf" target="_blank">
                <asp:Label ID="staticlink" runat="server" Text="blackchair.manhardeep.com/PDFs/Doc1.pdf" Visible="false"></asp:Label></a>
                       </ContentTemplate>
                <Triggers>
                    <asp:AsyncPostBackTrigger ControlID="Button2" />
                    <asp:AsyncPostBackTrigger ControlID="btn_SearchHardPapers" />
                </Triggers>
            </asp:UpdatePanel>
        </div>
<center><small style="font-size: .8em;margin-top: 10px;">BlackChair</small>   </center>
    </form>
</body>
</html>
