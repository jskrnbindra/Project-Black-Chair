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
    public partial class QuestionsExtraction : System.Web.UI.Page
    {
        static string cs = ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
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

        protected void btn_ShowQuestions_Click(object sender, EventArgs e)
        {
            DataTable ExtractedQuestions = new DataTable();

            string IncludeCA = cb_IncludeCA.Checked ? "Y" : "N";
            string CourseCode = tb_CourseCode.Text == "" ? "N" : tb_CourseCode.Text.Trim();
            string QuestionType = string.Empty;

           switch( Convert.ToInt32(ddl_QuestionType.SelectedValue))
            {
                case 1:
                    ExtractedQuestions = tb_CourseCode.Text == "" ? extractQuestions(1, cb_IncludeCA.Checked): extractQuestions(1,tb_CourseCode.Text,cb_IncludeCA.Checked);
                    QuestionType = "1";
                    break;
                case 2:
                    ExtractedQuestions = tb_CourseCode.Text == "" ? extractQuestions(2, cb_IncludeCA.Checked) : extractQuestions(2, tb_CourseCode.Text, cb_IncludeCA.Checked);
                    QuestionType = "2";
                    break;

                case 3:
                    ExtractedQuestions = tb_CourseCode.Text == "" ? extractQuestions(5, cb_IncludeCA.Checked) : extractQuestions(5, tb_CourseCode.Text, cb_IncludeCA.Checked);
                    QuestionType = "5";
                    break;
                case 4:
                    ExtractedQuestions = tb_CourseCode.Text == "" ? extractQuestions(10, cb_IncludeCA.Checked) : extractQuestions(10, tb_CourseCode.Text, cb_IncludeCA.Checked);
                    QuestionType = "10";
                    break;
                case 5:
                    ExtractedQuestions = tb_CourseCode.Text == "" ? extractQuestions(15, cb_IncludeCA.Checked) : extractQuestions(15, tb_CourseCode.Text, cb_IncludeCA.Checked);
                    QuestionType = "15";
                    break;
                default:
                    System.Diagnostics.Debug.Write("No match in switch");
                    break;
            }

            string qstringlink = "QuestionsPDF.aspx?QuestionType=" + QuestionType + "&CourseCode=" + CourseCode + "&IncludeCA=" + IncludeCA;
            System.Diagnostics.Debug.WriteLine(qstringlink);
            hl_ViewPDF.NavigateUrl = qstringlink;
            hl_ViewPDF.Visible = true;

            lbl_msg.Text = ExtractedQuestions.Rows.Count > 0 ? ExtractedQuestions.Rows.Count.ToString()+" questions" : "No questions";
            lbl_msg.Visible = true;
            //gv_ExtractedQuestions.DataSource = ExtractedQuestions;
            //gv_ExtractedQuestions.DataBind();
        }

        public DataTable extractQuestions(float Weightage,string CourseCode,bool IncludeCAQuestions)
        {
            DataTable returner = new DataTable();

            string weightageString = Weightage == 2 ? " between 2 and 3" : "="+Weightage.ToString();

            string QueryWithCA = "select * from QuestionPapersDump where Weightage " + weightageString + " and CourseCode = '" + CourseCode + "'";
            string QueryWithoutCA = "select * from QuestionPapersDump where Weightage " + weightageString + " and CourseCode = '" + CourseCode + "' and pnr not in (select pnr from capapers)";

            using (SqlConnection con = new SqlConnection(cs))
            {
                SqlDataAdapter ada = new SqlDataAdapter(IncludeCAQuestions?QueryWithCA:QueryWithoutCA, con);
                con.Open();
                try
                {
                    ada.Fill(returner);
                }
                catch(SqlException)
                {
                    lbl_msg.Visible = true;
                }
                con.Close();
            }
                System.Diagnostics.Debug.Write("INSIDE CC TYPE");
            return returner;
        }
        
        public DataTable extractQuestions(float Weightage, bool IncludeCAQuestions)
        {
            DataTable returner = new DataTable();
            string weightageString = Weightage == 2 ? " between 2 and 3" : "="+Weightage.ToString();

            string QueryWithCA = "select * from QuestionPapersDump where Weightage " + weightageString;
            string QueryWithoutCA = "select * from QuestionPapersDump where Weightage " + weightageString + " and pnr not in (select pnr from capapers)";

            using (SqlConnection con = new SqlConnection(cs))
            {
                SqlDataAdapter ada = new SqlDataAdapter(IncludeCAQuestions?QueryWithCA:QueryWithoutCA, con);
                con.Open();
                try
                {
                    ada.Fill(returner);
                }
                catch (SqlException)
                {
                    lbl_msg.Visible = true;
                }
                con.Close();
            }
            System.Diagnostics.Debug.Write("INSIDE CC TYPE");
            return returner;
        }

    }
}