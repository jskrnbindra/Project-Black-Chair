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
    public partial class PaperCollectionUtility : System.Web.UI.Page
    {
        TimeZoneInfo IST = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
                Session["TimeOut"] = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, IST).AddSeconds(19).ToString();
        }

        protected void btn_AddNewCapturedPaper_Click(object sender, EventArgs e)
        {
            if (ddl_PaperSet.SelectedValue == "Z")
            {
                lbl_msg.Text = "Select Paper Set first";
                lbl_msg.Font.Bold = true;
                lbl_msg.BackColor = System.Drawing.Color.Orange;
            }
            else
            {
                if (!isPresent(tb_PaperCapturedCC.Text.Trim(), ddl_PaperSet.SelectedValue))
                {
                    using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
                    {
                        using (SqlCommand cmd = new SqlCommand("AddPaper", con))
                        {
                            SqlParameter param_courseCode = new SqlParameter("@PaperCourseCode", SqlDbType.NVarChar);
                            SqlParameter param_DateTime = new SqlParameter("@DateTime", SqlDbType.NVarChar);
                            SqlParameter param_Set = new SqlParameter("@Set", SqlDbType.Char);

                            param_courseCode.Value = tb_PaperCapturedCC.Text;
                            param_DateTime.Value = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, IST).ToShortDateString() + " " + TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, IST).ToShortTimeString();
                            param_Set.Value = ddl_PaperSet.SelectedValue.ToString();

                            cmd.Parameters.Add(param_courseCode);
                            cmd.Parameters.Add(param_DateTime);
                            cmd.Parameters.Add(param_Set);

                            cmd.CommandType = CommandType.StoredProcedure;

                            lbl_msg.ForeColor = System.Drawing.Color.White;

                            con.Open();
                            if (cmd.ExecuteNonQuery() == 1)
                            {
                                lbl_msg.Text = "Paper added and broadcasted to all";
                                lbl_msg.BackColor = System.Drawing.Color.Green;
                            }

                            else
                            {
                                lbl_msg.Text = "Error occured. Call now: 9988255277";
                                lbl_msg.BackColor = System.Drawing.Color.Red;
                            }
                        }
                    }
                    refreshData();
                }
                else
                {
                    lbl_msg.BackColor = System.Drawing.Color.Orange;
                    lbl_msg.Font.Bold = true;
                    lbl_msg.Text = "Paper already captured.";
                }

            }
        }

        protected void refreshData()
        {
            PapersCollected.DataBind();
            UP_Fetcher.Update();
            System.Diagnostics.Debug.WriteLine("Refershed manually");
            lbl_LastUpdateAt.Text = "Last Updated at "+TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, IST).ToLongTimeString();
        }

        protected void tmr_SecsTrigger_Tick(object sender, EventArgs e)
        {
            try
            {
                if (0 > DateTime.Compare(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, IST), DateTime.Parse(Session["TimeOut"].ToString())))
                    lbl_secs.Text = "Refreshing again in " + ((Int32)DateTime.Parse(Session["TimeOut"].ToString()).Subtract(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, IST)).TotalSeconds).ToString() + "s";
                else
                {
                    Session["TimeOut"] = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, IST).AddSeconds(19).ToString();
                    refreshData();
                }
            }
            catch
            {
                Response.Redirect(Request.RawUrl);
            }
        }

        protected void lb_RefreshNow_Click(object sender, EventArgs e)
        {
            try
            {
                Session["TimeOut"] = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, IST).AddSeconds(19).ToString();
            }
            catch
            {
                Response.Redirect(Request.RawUrl);
            }
            refreshData();
        }

        protected bool isPresent(string CCode, string set)
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand("select count(*) from LivePapersFeed where CourseCode = '"+CCode+"' and Paper_Set='"+set+"'", con))
                {
                    con.Open();
                    if (Convert.ToInt32(cmd.ExecuteScalar().ToString()) > 0)
                        return true;
                    else
                        return false;
                }
            }
        }
    }
}