<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="PaperCollectionUtility.aspx.cs" Inherits="DataCollection.PaperCollectionUtility" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">

<head runat="server">
    <title>Papers Collection Utility - BlackChair</title>
    <link href="animate.css" rel="stylesheet" />
    <link href="PCU_Style.css" rel="stylesheet" />
    <link href="loader.css" rel="stylesheet" />
</head>

<body>
    <form id="form1" runat="server">
    
            <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
        <br />
       
        <center>
            <div class="TopInfo">
      
            <asp:UpdatePanel ID="UP_Refresher" runat="server">
                <ContentTemplate>
                    <asp:Timer runat="server" Interval="1000" ID="tmr_SecsTrigger" OnTick="tmr_SecsTrigger_Tick"></asp:Timer>
                    <asp:Label runat="server" ID="lbl_secs"></asp:Label>
                    <br />
                    <asp:LinkButton ID="lb_RefreshNow" runat="server" OnClick="lb_RefreshNow_Click">Force-Refresh Now</asp:LinkButton>
                </ContentTemplate>
            </asp:UpdatePanel>

            </div>
            <h1>Paper Collection Utility</h1>
            <asp:Label ID="lbl_msg" runat="server" Font-Size="40"></asp:Label>
            <asp:TextBox ID="tb_PaperCapturedCC" PlaceHolder="Course Code" runat="server" CssClass="TEXBOX" required></asp:TextBox>
            <br />
      
             <asp:DropDownList ID="ddl_PaperSet" runat="server" CssClass="DDL">
                <asp:ListItem Value="Z" Text="---Select Paper Set---"></asp:ListItem>
                <asp:ListItem Value="0" Text="No Set"></asp:ListItem>
                <asp:ListItem Value="A" Text="A"></asp:ListItem>
                <asp:ListItem Value="B" Text="B"></asp:ListItem>
                <asp:ListItem Value="C" Text="C"></asp:ListItem>
                <asp:ListItem Value="D" Text="D"></asp:ListItem>
                <asp:ListItem Value="E" Text="E"></asp:ListItem>
            </asp:DropDownList>

       <br />
             <asp:Button ID="btn_AddNewCapturedPaper" CssClass="btning" runat="server" Text="Add Paper" OnClick="btn_AddNewCapturedPaper_Click" />
             <br />
             <br />
            <hr />
            <h2>Captured Papers</h2> <br />
             
            <asp:UpdateProgress runat="server" ID="UP_1" AssociatedUpdatePanelID="UP_Fetcher">
                    <ProgressTemplate>
                Refreshing...
                 <div class='uil-default-css' style='transform:scale(0.14);margin: 0px;'>
                      <div style='top:80px;left:95px;width:10px;height:40px;background:#00b2ff;-webkit-transform:rotate(0deg) translate(0,-60px);transform:rotate(0deg) translate(0,-60px);border-radius:4px;position:absolute;'></div><div style='top:80px;left:95px;width:10px;height:40px;background:#00b2ff;-webkit-transform:rotate(30deg) translate(0,-60px);transform:rotate(30deg) translate(0,-60px);border-radius:4px;position:absolute;'></div><div style='top:80px;left:95px;width:10px;height:40px;background:#00b2ff;-webkit-transform:rotate(60deg) translate(0,-60px);transform:rotate(60deg) translate(0,-60px);border-radius:4px;position:absolute;'></div><div style='top:80px;left:95px;width:10px;height:40px;background:#00b2ff;-webkit-transform:rotate(90deg) translate(0,-60px);transform:rotate(90deg) translate(0,-60px);border-radius:4px;position:absolute;'></div><div style='top:80px;left:95px;width:10px;height:40px;background:#00b2ff;-webkit-transform:rotate(120deg) translate(0,-60px);transform:rotate(120deg) translate(0,-60px);border-radius:4px;position:absolute;'></div><div style='top:80px;left:95px;width:10px;height:40px;background:#00b2ff;-webkit-transform:rotate(150deg) translate(0,-60px);transform:rotate(150deg) translate(0,-60px);border-radius:4px;position:absolute;'></div><div style='top:80px;left:95px;width:10px;height:40px;background:#00b2ff;-webkit-transform:rotate(180deg) translate(0,-60px);transform:rotate(180deg) translate(0,-60px);border-radius:4px;position:absolute;'></div><div style='top:80px;left:95px;width:10px;height:40px;background:#00b2ff;-webkit-transform:rotate(210deg) translate(0,-60px);transform:rotate(210deg) translate(0,-60px);border-radius:4px;position:absolute;'></div><div style='top:80px;left:95px;width:10px;height:40px;background:#00b2ff;-webkit-transform:rotate(240deg) translate(0,-60px);transform:rotate(240deg) translate(0,-60px);border-radius:4px;position:absolute;'></div><div style='top:80px;left:95px;width:10px;height:40px;background:#00b2ff;-webkit-transform:rotate(270deg) translate(0,-60px);transform:rotate(270deg) translate(0,-60px);border-radius:4px;position:absolute;'></div><div style='top:80px;left:95px;width:10px;height:40px;background:#00b2ff;-webkit-transform:rotate(300deg) translate(0,-60px);transform:rotate(300deg) translate(0,-60px);border-radius:4px;position:absolute;'></div><div style='top:80px;left:95px;width:10px;height:40px;background:#00b2ff;-webkit-transform:rotate(330deg) translate(0,-60px);transform:rotate(330deg) translate(0,-60px);border-radius:4px;position:absolute;'></div>
                  </div>
                 <br />   
            </ProgressTemplate>
        </asp:UpdateProgress>

            <asp:UpdatePanel ID="UP_Fetcher" runat="server" ChildrenAsTriggers="false" UpdateMode="Conditional">
                <ContentTemplate>
                    <asp:Label ID="lbl_LastUpdateAt" CssClass="laupdate" runat="server" Text=""></asp:Label>
                    <asp:GridView ID="PapersCollected" CssClass="fadeIn animated gvfetcher" runat="server" AutoGenerateColumns="False" DataSourceID="LivePaperFeed" CellPadding="4" ForeColor="#333333" GridLines="None" AllowSorting="True">
                        <EmptyDataTemplate>
                            No papers yet...
                        </EmptyDataTemplate>
                        <AlternatingRowStyle BackColor="White"></AlternatingRowStyle>
                        <Columns>
                            <asp:BoundField DataField="CourseCode" HeaderText="CourseCode" SortExpression="CourseCode"></asp:BoundField>
                        <asp:BoundField DataField="Paper_Set" HeaderText="Paper_Set" SortExpression="Paper_Set"></asp:BoundField>
</Columns>
                        <EditRowStyle BackColor="#2461BF"></EditRowStyle>

                        <FooterStyle BackColor="#507CD1" Font-Bold="True" ForeColor="White"></FooterStyle>

                        <HeaderStyle BackColor="#507CD1" Font-Bold="True" ForeColor="White"></HeaderStyle>

                        <PagerStyle HorizontalAlign="Center" BackColor="#2461BF" ForeColor="White"></PagerStyle>

                        <RowStyle BackColor="#EFF3FB"></RowStyle>

                        <SelectedRowStyle BackColor="#D1DDF1" Font-Bold="True" ForeColor="#333333"></SelectedRowStyle>

                        <SortedAscendingCellStyle BackColor="#F5F7FB"></SortedAscendingCellStyle>

                        <SortedAscendingHeaderStyle BackColor="#6D95E1"></SortedAscendingHeaderStyle>

                        <SortedDescendingCellStyle BackColor="#E9EBEF"></SortedDescendingCellStyle>

                        <SortedDescendingHeaderStyle BackColor="#4870BE"></SortedDescendingHeaderStyle>
                    </asp:GridView>
                </ContentTemplate>
            </asp:UpdatePanel>

            <asp:SqlDataSource runat="server" ID="LivePaperFeed" ConnectionString='<%$ ConnectionStrings:ConStr %>' SelectCommand="select CourseCode,Paper_Set from LivePapersFeed order by serial desc"></asp:SqlDataSource>
            <br />
            <br />
            <hr />
            BlackChair
        </center>
    </form>
</body>
</html>
