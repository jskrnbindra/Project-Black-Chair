using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.Services;

namespace DataCollection
{
    public partial class CourseCodeSearch : System.Web.UI.Page
    {
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

        [WebMethod]
        public static string[] GetCustomers(string prefix)
        {
            List<string> customers = new List<string>();
            using (SqlConnection conn = new SqlConnection())
            {
                conn.ConnectionString = ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString;
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = "select distinct(CourseCode) from Papers where CourseCode like @SearchText + '%'";
                    cmd.Parameters.AddWithValue("@SearchText", prefix);
                    cmd.Connection = conn;
                    conn.Open();
                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            customers.Add(string.Format("{0}", sdr["CourseCode"].ToString().ToUpper()));
                        }
                    }
                    conn.Close();
                }
            }
            return customers.ToArray();
        }

        protected void btn_FindPapers_Click(object sender, EventArgs e)
        {
           //System.Threading.Thread.Sleep(1000);

            DataTable SearchResults = findPapersForCourseCode(tb_CourseCode.Text);
            if(SearchResults.Rows.Count==0)
            {
                lbl_msg.Text = "No papers found for this freakin' course code";
            }
            else
            {
                lbl_msg.Text = SearchResults.Rows.Count + " papers found";
                displaySearchResults(SearchResults);
            }
        }

        //the followin function searches for papers from both the indexed and the Hard Typed papers
        protected DataTable findPapersForCourseCode(string CourseCode)
        {
                    DataTable returner = new DataTable();
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                using (SqlDataAdapter ada = new SqlDataAdapter("ShowPapersWithCourseCode", con))
                {
                    ada.SelectCommand.CommandType = CommandType.StoredProcedure;
                    ada.SelectCommand.Parameters.Add(new SqlParameter { ParameterName = "@CourseCode", Value = CourseCode, SqlDbType = SqlDbType.NVarChar, Size = 10 });

                    con.Open();
                    ada.Fill(returner);

                    return returner;
                }
            }
        }

        //displays search results in a panel control as hyperlinks
        protected void displaySearchResults(DataTable SearchResults)
        {
            AClassUtitlites util = new AClassUtitlites();

            QuestionsPDF PapersUtitlity = new QuestionsPDF();

            foreach(DataRow paper in SearchResults.Rows)
            {
                HyperLink paperlink = new HyperLink();
                paperlink.CssClass = "DynamicPaperLinks";

                if (paper["filepath"].ToString().Length>8)
                {
                    //is hard typed paper - SearchResults table has CourseCode as coursecode if the paper is from DB else it has the file path of the hard typed paper
                    paperlink.NavigateUrl = PathToURL(paper["filepath"].ToString());
                    paperlink.Text = util.getPaperSource(paper["PNR"].ToString());
                    pnl_SearchResults.Controls.Add(paperlink);
                }
                else
                {
                    //is from DB
                    paperlink.NavigateUrl = "~/ViewQuestionPaper.aspx?p="+ new QueryStringEncryption().encryptQueryString(paper["PNR"].ToString());
                    paperlink.Text = util.getPaperSource(paper["PNR"].ToString());
                    pnl_SearchResults.Controls.Add(paperlink);
                }
            }
        }

        protected string PathToURL(string Filepath)
        {
            return Filepath.Substring(Filepath.IndexOf("blackchair.manhardeep.com")+26);//production environment
            //return Filepath.Substring(Filepath.IndexOf("Hard"));//FOR test environment only
        }
    }
}