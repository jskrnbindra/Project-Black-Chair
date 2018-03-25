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
    public partial class Utilities : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            lbl_PNRlabel.Visible = false;
            verifyUserAuthentication();
        }

        protected void verifyUserAuthentication()
        {
            SecurityAgent SecAgent = new SecurityAgent();
            if (Request.Cookies["__BlackChair-Authenticator"] != null && Request.Cookies["UserID"]!=null)
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

        protected void btn_PNRSearch_Click(object sender, EventArgs e)
        {
            if(tb_PNR.Text!="")
            {
                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("searchPNR", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        SqlParameter pnr = new SqlParameter("@pnr",SqlDbType.NVarChar);

                        cmd.Parameters.AddWithValue("@pnr",tb_PNR.Text.Trim());
                        con.Open();
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataSet ds = new DataSet();
                        int RowsReturned=da.Fill(ds);
                        gv_Reader.DataSource = ds;
                        gv_Reader.DataBind();
                        if (RowsReturned!= 0)
                        {
                            lbl_PNRlabel.Visible = true;
                            lbl_PNRlabel.BackColor = System.Drawing.Color.LightGreen;
                            lbl_PNRlabel.ForeColor = System.Drawing.Color.MediumBlue;
                            lbl_PNRlabel.Text = "Results: ";
                        }
                    }
                }
                searchInHardPapers(tb_PNR.Text.Trim());
            }
            else
            {
                lbl_PNRlabel.Visible = true;
                lbl_PNRlabel.BackColor = System.Drawing.Color.LightPink;
                lbl_PNRlabel.ForeColor = System.Drawing.Color.Red;
                lbl_PNRlabel.Text = "You didn't enter a PNR number.";
            }
            
        }

        protected void searchInHardPapers(string PNR)
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                using (SqlDataAdapter ada= new SqlDataAdapter("select Serial, PNR, PaperType = case HardPaperCode when 1 then 'StraightPaper' when 2 then 'CA'  when 3 then 'PNRless'  when 4 then 'MTP (WTP-1)' when 5 then 'ETP (WTP-2)' when 1 then 'StraightPaper'   when 1 then 'StraightPaper' end from HardTypedPapers where pnr = '"+PNR+"'", con))
                {
                    DataTable ResultsFromHardPapers = new DataTable();
                    con.Open();
                    ada.Fill(ResultsFromHardPapers);
                    gv_HardResults.DataSource = ResultsFromHardPapers;
                    gv_HardResults.DataBind();
                }
            }
        }
    }
}