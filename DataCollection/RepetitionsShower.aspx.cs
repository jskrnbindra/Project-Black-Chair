using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;

namespace DataCollection
{
    public partial class RepetitionsShower : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            QueryStringEncryption Decryptor = new QueryStringEncryption();

            //Decryptor.decryptQueryString(Request.QueryString["QuestionID"].ToString());

            string serialsList = Request.QueryString["sl"].ToString();

            string PaperPNR = Decryptor.decryptQueryString(Request.QueryString["p"].ToString());

            if (serialsList != "NULL")
            {
                string[] SerialsList = serialsList.Split(new char[] { 's' });

                foreach (string PNR in givePNRs(SerialsList))
                {
                    HyperLink ViewPaperLink = new HyperLink();
                    ViewPaperLink.Text = new QuestionsPDF().getQuestionSourcePaper(PNR);
                    ViewPaperLink.NavigateUrl = "ViewQuestionPaper.aspx?p=" + Decryptor.encryptQueryString(PNR);
                    ViewPaperLink.CssClass = "DynamicLinks";
                    if (PaperPNR == PNR)
                        ViewPaperLink.CssClass = "DynamicLinks SamePaperLink";
                    else
                        ViewPaperLink.CssClass = "DynamicLinks";

                    pnl_LinksHolder.Controls.Add(ViewPaperLink);
                }
            }
        }

        protected string[] givePNRs(string[] Serials)
        {
            string[] Returner = new string[Serials.Length];

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                SqlCommand SerialtoPNR = new SqlCommand();
                SerialtoPNR.Connection = con;

                con.Open();

                int pointer = 0;
                foreach (string serial in Serials)
                {
                    SerialtoPNR.CommandText = "select PNR from QuestionPapersDump where serial=" + serial;
                    Returner[pointer] = SerialtoPNR.ExecuteScalar().ToString(); pointer++;
                }
            }
            return Returner;
        }
    }
}