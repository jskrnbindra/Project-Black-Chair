
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AddHardTypedPaper.aspx.cs" Inherits="DataCollection.AddPNRless" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Hard Typed Papers Entry :: BlackChair</title>
    <link href="Styler.css" rel="stylesheet" />
    <style>
        .tb {
            max-width: 250px;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <center>
            <asp:HyperLink ID="HyperLink1" NavigateUrl="~/AddQuestions.aspx" runat="server">Go back</asp:HyperLink>
        <br />
            <asp:Label ID="lbl_msg" runat="server" BackColor="Turquoise" Text=""></asp:Label>
            <asp:HyperLink ID="hl_PaperLink" Visible="false" runat="server" Text="Preview Paper"></asp:HyperLink>
        </center>
        <br />
        <h1 style="font-size: 2em;">Add New Hard Typed Paper</h1>
        <small style="font-size:.7em;">To check if the paper is already entered, use <b>Ctrl+F</b> to search the PNR in the "Already entered Hard Typed Papers" section below</small>
        <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
        <asp:UpdatePanel ID="UpdatePanel1" runat="server">
            <ContentTemplate>
        <asp:Panel ID="pnl_ModalBackground" runat="server" Visible="false" CssClass="ModalHolder"> </asp:Panel>
                <asp:CheckBox ID="cb_CA" runat="server" Text="Hard CA" OnCheckedChanged="cb_CA_CheckedChanged" AutoPostBack="true" />
                <asp:CheckBox ID="cb_MTPETP" runat="server" Text="Hard MTP/ETP" OnCheckedChanged="cb_MTPETP_CheckedChanged" AutoPostBack="true" />
                <asp:CheckBox ID="cb_PNRless" OnCheckedChanged="cb_PNRless_CheckedChanged" runat="server" Text="Hard PNRless" AutoPostBack="true" />
                <asp:RadioButtonList ID="rbl_MTPETP" runat="server" Visible="false" OnSelectedIndexChanged="rbl_MTPETP_SelectedIndexChanged" AutoPostBack="true">
                    <asp:ListItem Text="MTP"></asp:ListItem>
                    <asp:ListItem Text="ETP"></asp:ListItem>
                </asp:RadioButtonList>
                <asp:TextBox ID="tb_PNRNumber" runat="server" CssClass="tb" PlaceHolder="PNR Number"></asp:TextBox>
                <asp:TextBox ID="tb_CAnum" CssClass="tb" runat="server" Visible="false" placeholder="CA number(1st, 2nd, etc)"></asp:TextBox>

        <asp:Panel ID="pnl_PNRlessModal"  CssClass="PNRlessModal" runat="server" Visible="false"  DefaultButton="PNRgenerator">
        Generate new PNR
                <asp:TextBox ID="tb_Modal_CCode" runat="server" Placeholder="Course Code"></asp:TextBox>
            <br />
            <asp:Button ID="btn_ConfirmNew" runat="server" Visible="false" Text="Confirm new Entry"  OnClick="btn_ConfirmNew_Click" />
            <asp:Button ID="PNRgenerator" runat="server" Text="Press Enter" OnClick="PNRgenerator_Click" />
            <asp:Button ID="CancelPNRless" runat="server" Text="Cancel"  OnClick="CancelPNRless_Click" />
            <asp:Panel ID="pnl_oldHolder" runat="server">
            <asp:Panel ID="pnl_msgholder" runat="server" Visible="false">
                Check if the paper you are about to enter is already entered or not from the following list:<br />
            </asp:Panel>
            <asp:Panel ID="pnl_LinksHolder" runat="server" Visible="false">
            </asp:Panel>
                </asp:Panel>
        </asp:Panel>
                <asp:Panel ID="pnl_CADupCheck" runat="server" Visible="false" CssClass="PNRlessModal">
                    <asp:TextBox runat="server" ID="tb_CAmodal_ccode" placeholder="Course Code"></asp:TextBox>
                    <asp:TextBox ID="tb_FirstQuestion" runat="server" PlaceHolder="Type in the Q1. (First Question)" TextMode="MultiLine"></asp:TextBox>
                    <asp:Button ID="btn_ConfirmCA" runat="server" Visible="false" Text="Confirm Entry" OnClick="btn_ConfirmCA_Click" />
                <asp:Button ID="btn_CheckDuplicate" runat="server" Text="Check" OnClick="btn_CheckDuplicate_Click" />
                    <asp:Button runat="server" ID="btn_Cancel" OnClick="CancelPNRless_Click" Text="Cancel" />
                    <br />
                    <asp:Label ID="lbl_DuplicacyMsg" runat="server"></asp:Label>
                    <asp:Panel ID="pnl_DynamicDupLinks" runat="server"></asp:Panel>
                </asp:Panel>
                
            </ContentTemplate>
            <Triggers>
                <asp:AsyncPostBackTrigger ControlID="cb_MTPETP" />
                <asp:AsyncPostBackTrigger ControlID="cb_CA" />
                <asp:AsyncPostBackTrigger ControlID="cb_PNRless" />
                <asp:PostBackTrigger ControlID="btn_ConfirmCA" />
                <asp:PostBackTrigger ControlID="btn_ConfirmNew" />
            </Triggers>
        </asp:UpdatePanel>

        <br />
        <asp:TextBox ID="tb_PaperSet" runat="server" CssClass="tb" PlaceHolder="Paper Set"></asp:TextBox>
        <asp:TextBox ID="tb_CourseCode" runat="server" CssClass="tb" PlaceHolder="Course Code"></asp:TextBox>
        <asp:TextBox ID="tb_TimeAllowed" runat="server" CssClass="tb" PlaceHolder="Time Allowed"></asp:TextBox>
        <asp:TextBox ID="tb_CourseName" runat="server" CssClass="tb" PlaceHolder="Course Name"></asp:TextBox>
        <asp:DropDownList runat="server" ID="ddl_Pattern" CssClass="tb" Width="150">
            <asp:ListItem Value="sh-lo">sh-lo</asp:ListItem>
            <asp:ListItem Value="ob-lo">ob-lo</asp:ListItem>
            <asp:ListItem Value="ob">ob</asp:ListItem>
        </asp:DropDownList>
        <asp:TextBox ID="tb_School" runat="server" CssClass="tb" Placeholder="School"></asp:TextBox>
        <asp:TextBox ID="tb_MaxMarks" runat="server" CssClass="tb" PlaceHolder="Maximum Marks"></asp:TextBox>
        <asp:TextBox ID="tb_DriveLink" runat="server" CssClass="tb" PlaceHolder="Paper drive link"></asp:TextBox>
        <br />

        Upload Paper PDF:&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
        <asp:FileUpload ID="fuc_PNRlessPaper" runat="server" AllowMultiple="false" ValidationGroup="FUC"  />
        <br />
        <small style="font-size: .6em; color: #ff6a00">Upload only PDF files           </small>
        <br />

        <asp:Button ID="btn_AddPaper" CssClass="bcb" runat="server" Text="Add Paper" OnClick="btn_AddPaper_Click" />

        <h3>Already entered hard typed papers:</h3>

        <asp:GridView ID="GridView1" Visible="false" runat="server" AutoGenerateColumns="False" DataSourceID="SqlDataSource1" AllowSorting="True">
            <Columns>
                <asp:BoundField DataField="Total_Count" HeaderText="Total_Count" ReadOnly="True" SortExpression="Total_Count"></asp:BoundField>
            </Columns>
        </asp:GridView>

        <asp:TextBox ID="tb_CheckPNR" runat="server" PlaceHolder="Enter PNR to check" CssClass="tb"></asp:TextBox><br />
        <asp:Button ID="btn_CheckPNR" runat="server" Text="Check PNR" OnClick="btn_CheckPNR_Click" CssClass="bcb" /><br />
        <asp:Label ID="lbl_CheckPNR" BackColor="Teal" ForeColor="WhiteSmoke" runat="server"></asp:Label>
        <asp:SqlDataSource runat="server" ID="SqlDataSource1" ConnectionString='<%$ ConnectionStrings:ConStr %>' SelectCommand="SELECT count(*) as Total_Count FROM [HardTypedPapers]"></asp:SqlDataSource>
        <center><small style="font-size: .8em;margin-top: 10px;">BlackChair</small>   </center>
         
          
    </form>
</body>
</html>
