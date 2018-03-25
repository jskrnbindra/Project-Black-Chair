<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="UnderTheHood.aspx.cs" Inherits="DataCollection.UnderTheHood" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Security Manager :: BlackChair</title>
    <style>
        .btn
        {
            color: #fff;
            padding: 5px 10px;
            outline: none;
            border: none;
        }
        .btn:hover
        {
            cursor: pointer;
            box-shadow: 1px 1px 2px #999;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
         <center>
            <asp:HyperLink ID="HyperLink1" NavigateUrl="~/AddQuestions.aspx" runat="server">Go back</asp:HyperLink>
             <asp:Button ID="btn_logout" runat="server" Text="Logout" OnClick="btn_logout_Click"></asp:Button>
        </center>
           <br />
    <div>
    <Center>
        <h1>BlackChair - Under The Hood</h1>
        <br />
        <asp:Button ID="Button1" CssClass="btn" runat="server" Text="Disallow New Browsers to Join" BackColor="Red" OnClick="Button1_Click"></asp:Button>
        <asp:Button ID="Button2" CssClass="btn" runat="server" Text="Allow New Browsers to Join" BackColor="Green" OnClick="Button2_Click"></asp:Button>
        <br />
        <asp:Label ID="Label2" runat="server"></asp:Label>
        <br />
        <asp:Label ID="Label1" runat="server" Text="Command executed successfully" Visible="false"></asp:Label>
        <br />
        <asp:FormView ID="FormView1" runat="server" DataSourceID="SqlDataSource1">
           <ItemTemplate>
                <h3>Authenticated Users:
                    
                    <asp:LinkButton ID="LinkButton1" OnClick="LinkButton1_Click" runat="server">

                     <asp:Label Text='<%# Bind("Connections") %>' runat="server" id="ConnectionsLabel"/>

                    </asp:LinkButton>



                </h3>
            </ItemTemplate>
        </asp:FormView>
        <asp:GridView ID="GridView1" runat="server" DataSourceID="SqlDataSource2" Visible="false"></asp:GridView>
        <asp:SqlDataSource ID="SqlDataSource2" runat="server" ConnectionString='<%$ ConnectionStrings:ConStr %>' SelectCommand="SELECT * FROM [Users] ORDER BY [Serial]"></asp:SqlDataSource>
        <br />
<hr />
        <h2>Add New User</h2>
        <asp:TextBox ID="tb_NewUserName" runat="server" placeholder="New user's name"></asp:TextBox>
        <br /> Permissions level:
        <asp:DropDownList ID="ddl_permissionsList" runat="server" ToolTip="Permissions Category">
            <asp:ListItem Text="1" />
            <asp:ListItem Text="4" />
            <asp:ListItem Text="7" />
        </asp:DropDownList><br />
        <asp:Button runat="server" ID="btn_UnlockForNewUser" Text="Unlock BlackChair" OnClick="btn_UnlockForNewUser_Click" />
        <hr />

        <br />
        <h2>Remove User</h2>
        <asp:DropDownList ID="ddl_UsersList" runat="server" DataSourceID="UsersList" DataTextField="Name" DataValueField="Name"></asp:DropDownList>
        <asp:SqlDataSource runat="server" ID="UsersList" ConnectionString='<%$ ConnectionStrings:ConStr %>' SelectCommand="SELECT [Name] FROM [Users]"></asp:SqlDataSource>
<asp:Button ID="btn_UserDelete" runat="server" Text="Remove from BlackChair" OnClick="btn_UserDelete_Click" BackColor="Red" ForeColor="White"></asp:Button>
        <hr />
        <h2>Change User Group</h2>
        <asp:TextBox ID="tb_PresentUserGroup" placeholder="Enter New Cookie content" runat="server"></asp:TextBox>
        <asp:Button ID="btn_ChangeUserGroup" runat="server" Text="Change User Group" ForeColor="White" BackColor="Crimson" OnClick="btn_ChangeUserGroup_Click"></asp:Button>
        <br />
        <asp:Label runat="server" ID="lbl_UserGroupHolder"></asp:Label>
        <br />
        <hr />
        <h2>Switch Online</h2>
        <asp:TextBox ID="tb_tblDownload" runat="server" PlaceHolder="Switch Name"></asp:TextBox>
        <asp:Button ID="btn_Switch" runat="server" Text="Switch Now" OnClick="btn_Switch_Click"></asp:Button>
    <asp:SqlDataSource runat="server" ID="SqlDataSource1" ConnectionString='<%$ ConnectionStrings:ConStr %>' SelectCommand="SELECT * FROM [security]"></asp:SqlDataSource>
</Center>
    </div>
    </form>
</body>
</html>
 