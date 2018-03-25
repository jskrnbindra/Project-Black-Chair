<%@ Page Language="C#" AutoEventWireup="true" EnableEventValidation="false" CodeBehind="SecurityPanel.aspx.cs" Inherits="DataCollection.DataSecurity" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>BlackChair :: Security Panel</title>
    <style>
        .BackUpGrid {
            width: 800px;
            float: left;
        }

        .OneSection {
            display: block;
            width: 100%;
        }

        .RightHolder {
            float: left;
            background: red;
            min-height: 100px;
            padding: 20px;
            margin: 5px;
            min-width: 300px;
            color: #fff;
            overflow: hidden;
        }

        .SecPnlBtn {
            padding: 10px;
            margin: 15px;
            border: none;
            outline: none;
        }

        .lightbg {
            background: #00bf6c;
        }

        #pnl_Stats {
            background: #2883d0;
        }

        .StatsLabels {
        }

        #NameUpdater {
            background: #7e0461;
            max-width: 300px;
        }

        .rightPart {
            width: 400px;
            float: left;
            overflow: hidden;
        }
        .MaxMarksAnomoly{
            float: left;
            background: red;
            
            padding: 20px;
            margin: 5px;
            min-width: 300px;
            color: #fff;
            overflow: hidden;
        }
        
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <center>
            <asp:HyperLink ID="HyperLink1" NavigateUrl="~/AddQuestions.aspx" runat="server">Go back</asp:HyperLink>
        </center>
        <br />
        <div>
            <center>
    <h1>BlackChair - Security Panel</h1>
            </center>
            <br />
            <br />
            <asp:Label ID="lbl_top" runat="server" Text=""></asp:Label>
            <section class="OneSection">
                <asp:Panel ID="BackUpLogHolder" CssClass="BackUpGrid" runat="server">
                    <h2>Backup history</h2>
                    <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="False" PageSize="20" DataSourceID="SqlDataSource1" AllowSorting="True" AllowPaging="True" BackColor="White" BorderColor="#999999" BorderStyle="None" BorderWidth="1px" CellPadding="3" GridLines="Vertical">
                        <AlternatingRowStyle BackColor="#DCDCDC"></AlternatingRowStyle>
                        <Columns>
                            <asp:BoundField DataField="Serial" HeaderText="Serial" ReadOnly="True" InsertVisible="False" SortExpression="Serial"></asp:BoundField>
                            <asp:BoundField DataField="DB Object" HeaderText="DB Object" SortExpression="DB Object"></asp:BoundField>
                            <asp:BoundField DataField="TimeStamp" HeaderText="TimeStamp" SortExpression="TimeStamp"></asp:BoundField>
                            <asp:BoundField DataField="Status" HeaderText="Status" SortExpression="Status"></asp:BoundField>
                            <asp:BoundField DataField="RowsUpdated" HeaderText="RowsUpdated" SortExpression="RowsUpdated"></asp:BoundField>
                            <asp:BoundField DataField="Error Message" HeaderText="Error Message" SortExpression="Error Message"></asp:BoundField>
                        </Columns>
                        <FooterStyle BackColor="#CCCCCC" ForeColor="Black"></FooterStyle>

                        <HeaderStyle BackColor="#000084" Font-Bold="True" ForeColor="White"></HeaderStyle>

                        <PagerStyle HorizontalAlign="Center" BackColor="#999999" ForeColor="Black"></PagerStyle>

                        <RowStyle BackColor="#EEEEEE" ForeColor="Black"></RowStyle>

                        <SelectedRowStyle BackColor="#008A8C" Font-Bold="True" ForeColor="White"></SelectedRowStyle>

                        <SortedAscendingCellStyle BackColor="#F1F1F1"></SortedAscendingCellStyle>

                        <SortedAscendingHeaderStyle BackColor="#0000A9"></SortedAscendingHeaderStyle>

                        <SortedDescendingCellStyle BackColor="#CAC9C9"></SortedDescendingCellStyle>

                        <SortedDescendingHeaderStyle BackColor="#000065"></SortedDescendingHeaderStyle>
                    </asp:GridView>
                   <asp:Panel runat="server" ID="pnl_HTP">
                     <br />
                    <br />
                    <br /><br />
                    <br />
                    <br /><br />
                    <br />
                    <br />
                    <br />     <br /><br />
                    <br />
                    <br />
                    <br />
                    <h2>Hard Typed Papers</h2>

                       <asp:GridView ID="gv_HardTypedPapers" PageSize="5" Width="600px" CssClass="BackUpGrid" runat="server" AutoGenerateColumns="False" DataKeyNames="PNR" DataSourceID="HardTypedPapers_DataSource" CellPadding="4" ForeColor="#333333" GridLines="None" AllowPaging="True" AllowSorting="True">
                           <EmptyDataTemplate>
                               No Hard Typed Papers entered Yet
                           </EmptyDataTemplate>
                           <AlternatingRowStyle BackColor="White"></AlternatingRowStyle>
                           <Columns>
                               <asp:BoundField DataField="Serial" HeaderText="Serial" ReadOnly="True" InsertVisible="False" SortExpression="Serial"></asp:BoundField>
                               <asp:BoundField DataField="PNR" HeaderText="PNR" ReadOnly="True" SortExpression="PNR"></asp:BoundField>
                               <asp:BoundField DataField="HardPaperCode" HeaderText="HardPaperCode" SortExpression="HardPaperCode"></asp:BoundField>
                               <asp:BoundField DataField="CAnum" HeaderText="CAnum" SortExpression="CAnum"></asp:BoundField>
                               <asp:BoundField DataField="Pattern" HeaderText="Pattern" SortExpression="Pattern"></asp:BoundField>
                               <asp:BoundField DataField="TimeAllowed" HeaderText="TimeAllowed" SortExpression="TimeAllowed"></asp:BoundField>
                               <asp:BoundField DataField="MaxMarks" HeaderText="MaxMarks" SortExpression="MaxMarks"></asp:BoundField>
                               <asp:BoundField DataField="PaperSet" HeaderText="PaperSet" SortExpression="PaperSet"></asp:BoundField>
                               <asp:BoundField DataField="School" HeaderText="School" SortExpression="School"></asp:BoundField>
                               <asp:BoundField DataField="CourseCode" HeaderText="CourseCode" SortExpression="CourseCode"></asp:BoundField>
                               <asp:BoundField DataField="CourseName" HeaderText="CourseName" SortExpression="CourseName"></asp:BoundField>
                               <asp:BoundField DataField="DriveLink" HeaderText="DriveLink" SortExpression="DriveLink"></asp:BoundField>
                               <asp:BoundField DataField="FilePath" HeaderText="FilePath" SortExpression="FilePath"></asp:BoundField>
                               <asp:BoundField DataField="TimeStamp" HeaderText="TimeStamp" SortExpression="TimeStamp"></asp:BoundField>
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
</asp:Panel>

                    <asp:SqlDataSource runat="server" ID="HardTypedPapers_DataSource" ConnectionString='<%$ ConnectionStrings:ConStr %>' SelectCommand="SELECT * FROM [HardTypedPapers] ORDER BY [Serial] DESC"></asp:SqlDataSource>
                    <asp:SqlDataSource runat="server" ID="SqlDataSource1" ConnectionString='<%$ ConnectionStrings:ConStr %>' SelectCommand="SELECT * FROM [Logs_BackUp] ORDER BY SERIAL DESC"></asp:SqlDataSource>
                </asp:Panel>

                <asp:Panel runat="server" ID="RightSec" CssClass="rightPart">

                    <asp:Panel ID="pnl_MaxMarksAnomoly" runat="server" CssClass="MaxMarksAnomoly" BackColor="LightSeaGreen">
                    <asp:Label runat="server" ID="lbl_MaxMarksAnamoly" Font-Bold="true" Text="No max marks anamoly"></asp:Label>
                    </asp:Panel>
                <asp:Panel ID="RightHolder" CssClass="RightHolder" runat="server">
                    <h2>Offline Backup</h2>
                    <asp:DropDownList ID="ddl_Tables" runat="server">
                        <asp:ListItem Text="QuestionPapersDump" Value="1"></asp:ListItem>
                        <asp:ListItem Text="Papers" Value="2"></asp:ListItem>
                        <asp:ListItem Text="CAPapers" Value="3"></asp:ListItem>
                    </asp:DropDownList>
                    <asp:Button ID="Button1" runat="server" Text="Download as Excel" OnClick="Button1_Click" />
                    <br />
                    <br />

                </asp:Panel>

                <asp:Panel runat="server" ID="NameUpdater" CssClass="RightHolder">
                    <strong style="font-size: 1.9em;">Names update</strong><br /><asp:Label ID="lbl_StatsNames" runat="server" /> names as of now<br /><br />

                    <asp:Panel runat="server" ID="pnl_UpdationControls" Visible="false">
                    <asp:FileUpload ID="fuc_NamesFile" runat="server" AllowMultiple="false" /><br />
                    <asp:Label runat="server" Font-Size="8" Text="Upload only .xls or .xlsx files" ForeColor="YellowGreen" />
                    <br /><br />
                  </asp:Panel>

                    <asp:Button ID="btn_NameUpdater" BackColor="LightSeaGreen" runat="server" CssClass="SecPnlBtn" Text="Add New DataSet" OnClick="btn_NameUpdater_Click" />
                    <asp:Button ID="btn_ConfirmWrite" Visible="false" BackColor="OrangeRed" ForeColor="White" runat="server" CssClass="SecPnlBtn" Text="Write To Server" OnClick="btn_ConfirmWrite_Click" />
                    <asp:Button ID="btn_NameCanceller" CssClass="SecPnlBtn" OnClick="btn_NameCanceller_Click" runat="server" Text="Cancel" Visible="false" />
                    <br />
                    <asp:Label ID="lbl_NameKeeper" runat="server" Text="Note: Before uploading the .xls file make sure you have its registration number column renamed to UID, Name to NAME, stream to STREAM and Program to PROGRAM." Font-Bold="true" Font-Size="Medium"></asp:Label>        
                  
                </asp:Panel>

                <asp:Panel runat="server" ID="ResetterControls" CssClass="RightHolder lightbg">
                    <strong style="font-size: 1.9em;">DB Admin</strong><br />
                    Last reset: <asp:Label ID="lbl_reset" runat="server"></asp:Label><br />
                    <asp:Label ID="lbl_resetPrimitive" Font-Size="8" runat="server"></asp:Label><br />
                    <asp:Button ID="Resetter" runat="server" CssClass="SecPnlBtn" Text="Reset Backup Objects" BackColor="OrangeRed" ForeColor="White" CommandName="Confirmation" OnClick="Resetter_Click" />
                    <asp:Button ID="btn_ResetCancel" runat="server" CssClass="SecPnlBtn" Text="Cancel" OnClick="btn_ResetCancel_Click" Visible="false" BackColor="Green" ForeColor="White" />
                </asp:Panel>

                <asp:Panel runat="server" ID="pnl_Stats" CssClass="RightHolder">
                    <strong style="font-size: 1.9em;">Stats</strong><br />
                    <asp:Label ID="lbl_StatQPD" runat="server" Font-Size="20" Text=" Questions" CssClass="StatsLabels"></asp:Label><br />
                    <asp:Label ID="lbl_StatP" runat="server" Font-Size="20" Text=" MTE/ETEs" CssClass="StatsLabels"></asp:Label><br />
                    <asp:Label ID="lbl_StatCAP" runat="server" Font-Size="20" Text=" CA Papers" CssClass="StatsLabels"></asp:Label><br />
                    <asp:Label ID="lbl_HardTypedPapers"  Font-Size="20"  runat="server" Text="Hard Papers" CssClass="StatsLabels"></asp:Label>
                </asp:Panel>

                </asp:Panel>
            </section>
        </div> 
        <asp:GridView ID="GridView2" runat="server"></asp:GridView>     
<center><small style="font-size: .8em;margin-top: 10px;display: block;clear: both;">BlackChair</small>   </center>
    </form>

</body>
</html>
