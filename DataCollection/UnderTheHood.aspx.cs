using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Text;
using System.IO;

namespace DataCollection
{
    public partial class UnderTheHood : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!isSessionLoggedIn()) Response.Redirect("UnderTheHood_Authenticator.aspx");

            SecurityAgent SecAgent = new SecurityAgent();

            if(!IsPostBack)
            tb_PresentUserGroup.Text = SecAgent.CurrentUserGroup;

            if(SecAgent.isBlackChairOpenToNewUsers())
            {
                Label2.Text = "Presently ALLOWING new browsers to register.";
            }
            else
            {
                Label2.Text = "Presently NOT ALLOWING new browsers to register.";
            }
        }

        protected bool isSessionLoggedIn()
        {
            if (Session["AdminLogged"] != null)
                return Session["AdminLogged"].ToString() == "Y" ? true : false;
            else
                return false;
        }

        SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString);
        SqlCommand cmd = new SqlCommand();
        
        protected void Button2_Click(object sender, EventArgs e)
        {
            cmd.CommandText = "update security set acceptNewCookies=1";
            cmd.Connection = con;

            try
            {
                con.Open();
                cmd.ExecuteNonQuery();
                Label2.Text = "Presently ALLOWING new browsers to register.";
            }
            catch (Exception)
            {
                Label1.Text = "ERROR: ";
            }
            finally
            {
                Label1.Visible = true;
            }
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            cmd.CommandText = "update security set acceptNewCookies=0";
            cmd.Connection = con;

            try
            {
                con.Open();
                cmd.ExecuteNonQuery();
                Label2.Text = "Presently NOT ALLOWING new browsers to register.";
            }
            catch (Exception)
            {
                Label1.Text = "ERROR: ";
            }
            finally
            {
                Label1.Visible = true;
            }
        }

        protected void btn_UnlockForNewUser_Click(object sender, EventArgs e)
        {
            TimeZoneInfo IST = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
            DateTime taim = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, IST);

            QueryStringEncryption Encryptor = new QueryStringEncryption();

            
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                SqlCommand cmd = new SqlCommand("insert into Users values('"+tb_NewUserName.Text.Trim()+"','"+Encryptor.encryptQueryString(tb_NewUserName.Text.Trim()) +"',"+ddl_permissionsList.SelectedItem.Text+",'"+taim.ToShortDateString()+" "+taim.ToShortTimeString()+"')",con);
                con.Open();
                cmd.ExecuteNonQuery();
            }
            Button2_Click(new Object(), new EventArgs());
        }

        protected void btn_ChangeUserGroup_Click(object sender, EventArgs e)
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                SqlCommand cmd = new SqlCommand("insert into UserGroups values ('"+tb_PresentUserGroup.Text.Trim()+"')", con);
                con.Open();
                cmd.ExecuteNonQuery();
                btn_ChangeUserGroup.BackColor = System.Drawing.Color.Green;
            }
        }

        protected void LinkButton1_Click(object sender, EventArgs e)
        {
            GridView1.Visible = true;
        }

        protected void btn_UserDelete_Click(object sender, EventArgs e)
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                SqlCommand cmd = new SqlCommand("delete from users where Name='"+ddl_UsersList.SelectedValue+"'", con);
                con.Open();
                cmd.ExecuteNonQuery();
                btn_UserDelete.BackColor = System.Drawing.Color.Green;
            }
        }

        protected void btn_Switch_Click(object sender, EventArgs e)
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                SqlDataAdapter ada = new SqlDataAdapter();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = con;
                cmd.CommandText = "Select * from " + tb_tblDownload.Text.Trim() + " order by Serial";
                ada.SelectCommand = cmd;

                DataTable dt_Temp = new DataTable();
                con.Open();
                try
                {
                    ada.Fill(dt_Temp);
                }
                catch
                {
                    btn_Switch.Text = "Unable to download this.";
                }
                pushToHTTP(dt_Temp,tb_tblDownload.Text.Trim());
            }
        }

        public void pushToHTTP(DataTable DownloadableTable, string DownloadFileName)
        {
            Response.Clear();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment;filename=Backup_" + DownloadFileName + ".xls");
            Response.Charset = "";
            Response.ContentType = "application/vnd.ms-excel";

            GridView gv_ExcelFileData = new GridView();
            gv_ExcelFileData.DataSource = DownloadableTable;
            gv_ExcelFileData.AllowPaging = false;
            gv_ExcelFileData.DataBind();

            using (StringWriter sw = new StringWriter())
            {
                HtmlTextWriter htw = new HtmlTextWriter(sw);
                gv_ExcelFileData.RenderControl(htw);
                Response.Output.Write(sw.ToString());
                Response.Flush();
                Response.End();
            }
        }

        protected void btn_logout_Click(object sender, EventArgs e)
        {
            Session["AdminLogged"] = "N";
            Response.Redirect(Request.RawUrl);
        }
    }
}