using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace DataCollection
{
    public partial class WebForm2 : System.Web.UI.Page
    {
        string cs = ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            verifyUserAuthentication();
        }

        protected void verifyUserAuthentication()
        {
            SecurityAgent SecAgent = new SecurityAgent();
            if (Request.Cookies["__BlackChair-Authenticator"] != null && Request.Cookies["UserID"] != null)
            {
                if (Request.Cookies["__BlackChair-Authenticator"].Value == SecAgent.CurrentUserGroup)
                {
                    if (SecAgent.isValidUserName(Request.Cookies["UserID"].Value)) { }
                    else
                    {
                        Response.Redirect("~/UnidentifiedUser.aspx");
                    }
                }
                else
                {
                    if (SecAgent.isBlackChairOpenToNewUsers())
                    {
                        SecAgent.newUserAdded();
                        Response.Cookies["__BlackChair-Authenticator"].Value = SecAgent.CurrentUserGroup;
                        Response.Cookies["__BlackChair-Authenticator"].Expires = DateTime.Now.AddMonths(1);

                        Response.Cookies["UserID"].Value = SecAgent.getNewUser();
                        Response.Cookies["UserID"].Expires = DateTime.Now.AddMonths(1);
                    }
                    else
                        Response.Redirect("~/UnidentifiedUser.aspx");
                }
            }
            else
            {
                if (SecAgent.isBlackChairOpenToNewUsers())
                {
                    SecAgent.newUserAdded();
                    Response.Cookies["__BlackChair-Authenticator"].Value = SecAgent.CurrentUserGroup;
                    Response.Cookies["__BlackChair-Authenticator"].Expires = DateTime.Now.AddMonths(1);

                    Response.Cookies["UserID"].Value = SecAgent.getNewUser();
                    Response.Cookies["UserID"].Expires = DateTime.Now.AddMonths(1);
                }
                else
                {
                    Response.Redirect("~/UnidentifiedUser.aspx");
                }
            }
        }

        protected void btn_Delete_Click(object sender, EventArgs e)
        {
            if(btn_Delete.CommandName=="Show")
            {
                using (SqlConnection con = new SqlConnection(cs))
                {
                    string SelectCommandText = string.Empty;

                    if (btn_Delete.CommandArgument == "Serial")
                        SelectCommandText = "select * from QuestionPapersDump where serial="+tb_QuestionSerial.Text.Trim();
                    else if (btn_Delete.CommandArgument == "QuestionID")
                        SelectCommandText = "select * from QuestionPapersDump where QuestionID='"+tb_QuestionID.Text.Trim()+"'";

                    SqlCommand SelectCommand = new SqlCommand(SelectCommandText,con);
                    SqlParameter param = new SqlParameter("@serial", tb_QuestionSerial.Text);
                    SelectCommand.Parameters.Add(param);

                    using (SqlDataAdapter ada = new SqlDataAdapter(SelectCommand))
                    {

                        DataTable Question = new DataTable();
                        con.Open();
                        ada.Fill(Question);
                        con.Close();
                        gv_QuestionDetails.DataSource = Question;
                        gv_QuestionDetails.DataBind();

                        if (Question.Rows.Count == 0)
                        {
                            btn_Delete.CommandName = "Show";
                            btn_Delete.Text = "Delete";
                        }
                        else
                        {
                            btn_Delete.Text = "Confirm Delete";
                            btn_Delete.CommandName = "Delete";
                            hl_Self.Visible = true;
                        }
                    }
                }
            }

            else if(btn_Delete.CommandName=="Delete")
            {
                using (SqlConnection con = new SqlConnection(cs))
                {
                    string DeleteCommand = string.Empty;

                    if (btn_Delete.CommandArgument == "Serial")
                        DeleteCommand = "DELETE from QuestionPapersDump where serial=" + tb_QuestionSerial.Text.Trim();
                    else if (btn_Delete.CommandArgument == "QuestionID")
                        DeleteCommand = "DELETE from QuestionPapersDump where QuestionID='" + tb_QuestionID.Text.Trim() + "'";

                    using (SqlCommand cmd = new SqlCommand(DeleteCommand, con))
                    {
                        con.Open();
                        if (cmd.ExecuteNonQuery() == 1)
                        {
                            btn_Delete.Text = "Question Deleted";
                            btn_Delete.BackColor = System.Drawing.Color.Green;
                            hl_Self.Text = "Delete Another Question";
                        }
                        else
                        {
                            btn_Delete.Text = "Something unexpected happened";
                            btn_Delete.BackColor = System.Drawing.Color.Red;
                            hl_Self.Text = "Reset";
                        }
                        btn_Delete.Enabled = false;
                        ddl_Selector.Enabled = false;

                        con.Close();
                        gv_QuestionDetails.DataSource = null;
                        gv_QuestionDetails.DataBind();
                    }
                }
                btn_Delete.CommandName = "Show";
            }
        }

        protected void ddl_Selector_SelectedIndexChanged(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("here bro");
            if (ddl_Selector.SelectedValue == "Serial")
            {
                System.Diagnostics.Debug.WriteLine("in bro");

                tb_QuestionID.Visible = false;
                tb_QuestionSerial.Visible = true;
                btn_Delete.CommandArgument = "Serial";
            }
            else if (ddl_Selector.SelectedValue == "QuestionID")
            {
                System.Diagnostics.Debug.WriteLine("in bro");

                tb_QuestionSerial.Visible = false;
                tb_QuestionID.Visible = true;
                btn_Delete.CommandArgument = "QuestionID";
            }
            btn_Delete.BackColor = System.Drawing.Color.Teal;
            btn_Delete.CommandName = "Show";
            btn_Delete.Text = "Delete";
        }
    }
}