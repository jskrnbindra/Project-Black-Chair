using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.IO;
using System.Diagnostics;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using System.Text;

namespace DataCollection
{
    public partial class AddPNRless : System.Web.UI.Page
    {

        public static TimeZoneInfo IST = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

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

        protected void btn_AddPaper_Click(object sender, EventArgs e)
        {
            if (!fuc_PNRlessPaper.HasFile)
                lbl_msg.Text = "No file selected to upload babe.!";
            else
            {
                routeRequest();
            }
        }

        protected void addStrightHardPaper(string PaperPath, bool isPNRless)
        {
            DateTime NowTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, IST);
            string TimeStamp = NowTime.ToLongDateString() + " " + NowTime.ToLongTimeString();
            using (SqlConnection con_PaperAdded = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                using (SqlCommand cmd_PaperAdder = new SqlCommand(string.Empty, con_PaperAdded))
                {
                    string hardpapercode = isPNRless ? "3" : "1";
                    cmd_PaperAdder.CommandText = "insert into HardTypedPapers values('" + tb_PNRNumber.Text.Trim() + "','"+ hardpapercode+"',' -1','" + ddl_Pattern.SelectedValue + "','" + tb_TimeAllowed.Text.Trim() + "','" + tb_MaxMarks.Text.Trim() + "','" + tb_PaperSet.Text.Trim() + "','" + tb_School.Text + "','" + tb_CourseCode.Text.Trim().ToUpper() + "','" + tb_CourseName.Text.Trim() + "','" + tb_DriveLink.Text.Trim() + "','" + PaperPath + "','" + TimeStamp + "')";

                    con_PaperAdded.Open();
                    if (cmd_PaperAdder.ExecuteNonQuery() == 1)
                    {
                        fuc_PNRlessPaper.PostedFile.SaveAs(PaperPath);

                        hl_PaperLink.NavigateUrl = "~/Hard Typed Papers/" + tb_PNRNumber.Text.Trim() + ".pdf";
                        hl_PaperLink.Visible = true;
                        lbl_msg.Text = "Paper Added Successfully";
                    }
                    else
                        lbl_msg.Text = "There was an error entering the paper into BlackChair. You ought to report this error to the System Admin. Do not Yo this paper. call +91 9988255277";
                }
            }
        }

        protected void addPNRlessHardPaper(string PaperPath)
        {
            DateTime NowTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, IST);
            string TimeStamp = NowTime.ToLongDateString() + "" + NowTime.ToLongTimeString();
            using (SqlConnection con_PaperAdded = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                using (SqlCommand cmd_PaperAdder = new SqlCommand(string.Empty, con_PaperAdded))
                {
                    cmd_PaperAdder.CommandText = "insert into HardTypedPapers values('" + tb_PNRNumber.Text.Trim() + "','3','-1','" + ddl_Pattern.SelectedValue + "','" + tb_TimeAllowed.Text.Trim() + "','" + tb_MaxMarks.Text.Trim() + "','" + tb_PaperSet.Text.Trim() + "','" + tb_School.Text + "','" + tb_CourseCode.Text.Trim().ToUpper() + "','" + tb_CourseName.Text.Trim() + "','" + tb_DriveLink.Text.Trim() + "','" + PaperPath + "','" + TimeStamp + "')";

                    con_PaperAdded.Open();
                    if (cmd_PaperAdder.ExecuteNonQuery() == 1)
                    {
                        fuc_PNRlessPaper.PostedFile.SaveAs(PaperPath);

                        hl_PaperLink.NavigateUrl = "~/Hard Typed Papers/" + tb_PNRNumber.Text.Trim() + ".pdf";
                        hl_PaperLink.Visible = true;
                        lbl_msg.Text = "Paper Added Successfully";
                    }
                    else
                        lbl_msg.Text = "There was an error entering the paper into BlackChair. You ought to report this error to the System Admin with a screenshot of this screen. Do not Yo this paper. call +91 9988255277";
                }
            }
        }

        protected bool isNewPaper(string PNR)
        {
            bool isNotInHardTypedPapers = false, isNotInPapers = false;
            using (SqlConnection con_PaperAdded = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                string termID = string.Empty;
                using (SqlCommand cmd_PaperAdder = new SqlCommand("select count(*) from HardTypedPapers where pnr='" + PNR + "'", con_PaperAdded))
                {
                    con_PaperAdded.Open();
                    isNotInHardTypedPapers = Convert.ToInt32(cmd_PaperAdder.ExecuteScalar().ToString()) == 0 ? true : false;
                    cmd_PaperAdder.CommandText = "select count(*) from Papers where pnr='" + PNR + "'";
                    isNotInPapers = Convert.ToInt32(cmd_PaperAdder.ExecuteScalar().ToString()) == 0 ? true : false;

                    return isNotInHardTypedPapers && isNotInPapers;
                }
            }
        }

        protected void cb_CA_CheckedChanged(object sender, EventArgs e)
        {
            if (cb_CA.Checked)
            {
                tb_PNRNumber.Visible = false;
                tb_CAnum.Visible = true;
                pnl_CADupCheck.Visible = true;
                pnl_ModalBackground.Visible = true;
                cb_PNRless.Enabled = false;
                cb_MTPETP.Enabled = false;
                ddl_Pattern.Enabled = false;
            }
            else
            {
                tb_PNRNumber.Visible = true;
                tb_CAnum.Visible = false;
                pnl_CADupCheck.Visible = false;
                pnl_ModalBackground.Visible = false;
                cb_PNRless.Enabled = true;
                cb_MTPETP.Enabled = true;
                ddl_Pattern.Enabled = true;
            }
        }

        protected void cb_MTPETP_CheckedChanged(object sender, EventArgs e)
        {
            cb_CA.Checked = true;
            if (cb_MTPETP.Checked)
            {
                rbl_MTPETP.Visible = true;
                tb_CAnum.Visible = false;
            }
            else
            {
                rbl_MTPETP.Visible = false;
                cb_CA.Checked = false;
                tb_CAnum.Visible = true;
            }
        }

        protected void cb_PNRless_CheckedChanged(object sender, EventArgs e)
        {
            if (cb_PNRless.Checked)
            {
                pnl_PNRlessModal.Visible = true;
                pnl_ModalBackground.Visible = true;
                cb_CA.Visible = false;
                cb_MTPETP.Visible = false;
            }
            else
            {
                pnl_PNRlessModal.Visible = false;
                pnl_ModalBackground.Visible = false;
                cb_CA.Visible = true;
                cb_MTPETP.Visible = true;
                tb_PNRNumber.Text = string.Empty;
                tb_PNRNumber.BorderWidth = 0;
            }
        }

        protected void PNRgenerator_Click(object sender, EventArgs e)
        {
            if (tb_Modal_CCode.Text.Trim() != "" && tb_Modal_CCode.Text.Trim().Length > 5)
            {
                displayAlreadyEnteredPapers(tb_Modal_CCode.Text.Trim());
            }
            else
                PNRgenerator.Text = "Invalid! Try Again";
        }

        protected void CancelPNRless_Click(object sender, EventArgs e)
        {
            Response.Redirect(Request.RawUrl);
        }

        protected DataTable getAlreadyEnteredPNRlesses(string CourseCode)
        {
            DataTable returner = new DataTable();
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                using (SqlDataAdapter ada = new SqlDataAdapter("select pnr, pattern, TimeAllowed, MaxMarks, Paperset, CourseCode, FilePath from HardTypedPapers where pnr like '999%' and CourseCode ='" + CourseCode + "' union select pnr, pattern, TimeAllowed, MaxMarks, Paperset, CourseCode, school from papers where pnr like '999%' and CourseCode ='" + CourseCode + "'", con))
                {
                    con.Open();
                    ada.Fill(returner);
                    return returner;
                }
            }
        }

        protected void displayAlreadyEnteredPapers(string CourseCode)
        {
            DataTable papers = getAlreadyEnteredPNRlesses(CourseCode);
            System.Diagnostics.Debug.WriteLine(papers.Rows.Count + "------ROW COUNT------");
            if (papers.Rows.Count > 0)
            {
                PNRgenerator.Visible = false;
                pnl_LinksHolder.Visible = true;
                pnl_msgholder.Visible = true;
                QueryStringEncryption Encryptor = new QueryStringEncryption();
                foreach (DataRow paper in papers.Rows)
                {
                    HyperLink paperlink = new HyperLink();
                    paperlink.CssClass = "DynamicPaperLinks";

                    if (paper["FilePath"].ToString().Contains(".pdf"))
                    {
                        //System.Diagnostics.Debug.WriteLine("IN IN I NI IN ININI INI NININI");
                        //is HardTypedPaper
                        paperlink.NavigateUrl = paper["FilePath"].ToString().Substring(paper["FilePath"].ToString().IndexOf("Hard Typed"));
                        paperlink.Text = CourseCode.ToUpper() + " - " + paper["MaxMarks"].ToString();
                        pnl_LinksHolder.Controls.Add(paperlink);
                    }
                    else
                    {
                        //is not HardTypedPaper
                        paperlink.NavigateUrl = "ViewQuestionPaper.aspx?p=" + Encryptor.encryptQueryString(paper["PNR"].ToString());
                        paperlink.Text = CourseCode.ToUpper() + " - " + paper["MaxMarks"].ToString() + " - Not hard";
                        pnl_LinksHolder.Controls.Add(paperlink);
                    }
                    Literal newLine = new Literal();
                    newLine.Text = "<br>";
                    pnl_LinksHolder.Controls.Add(newLine);
                }
                btn_ConfirmNew.Visible = true;
            }
            else
            {
                pnl_LinksHolder.Visible = false;
                pnl_msgholder.Visible = false;
                confirmNewPNRlessEntry();
            }
        }

        protected void confirmNewPNRlessEntry()
        {
            tb_PNRNumber.Text = new AddQuestions().giveNewPNR(tb_Modal_CCode.Text.Trim());
            Debug.WriteLine("textbox thing-" + tb_Modal_CCode.Text + "-");
            tb_CourseCode.Text = tb_Modal_CCode.Text;//sth with it
            pnl_ModalBackground.Visible = false;
            pnl_PNRlessModal.Visible = false;
            tb_PNRNumber.BorderColor = System.Drawing.Color.Green;
            tb_PNRNumber.BorderWidth = 3;
            tb_PNRNumber.BorderStyle = BorderStyle.Dashed;


        }

        protected void btn_ConfirmNew_Click(object sender, EventArgs e)
        {
            confirmNewPNRlessEntry();
        }

        protected void btn_CheckDuplicate_Click(object sender, EventArgs e)
        {
            int HardPaperCode = cb_CA.Checked && !cb_MTPETP.Checked ? 2 : cb_MTPETP.Checked && rbl_MTPETP.SelectedValue == "MTP" ? 4 : cb_MTPETP.Checked && rbl_MTPETP.SelectedValue == "ETP" ? 5 : -1;
            Debug.WriteLine("Hard Paper Code: -"+HardPaperCode+"-");
            searchThisStatement(tb_FirstQuestion.Text.Trim(), tb_CAmodal_ccode.Text.Trim(), HardPaperCode);
            btn_CheckDuplicate.Visible = false;
            btn_ConfirmCA.Visible = true;
        }

        protected void searchThisStatement(string Statement, string CourseCode, int HardPaperCode)
        {
            DataTable Papers = new DataTable();
            using(SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                string SelectQuery = string.Empty;
              //  using(SqlDataAdapter ada = new SqlDataAdapter())
                //{
                   // ada.SelectCommand.Connection = con;

                    switch(HardPaperCode)
                    {
                        case 2://CA papers
                            //ada.SelectCommand.CommandText = "select pnr,canum,filepath from HardTypedPapers where HardPaperCode between 2 and 5 and HardPaperCode != 3 and coursecode = '" + CourseCode + "'  union select  pnr,canumber,paperset from capapers where coursecode='" + CourseCode + "' and CANumber<20";
                            SelectQuery = "select pnr,canum,filepath from HardTypedPapers where HardPaperCode = 2 and coursecode = '" + CourseCode + "'  union select  pnr,canumber,paperset from capapers where coursecode='" + CourseCode + "' and CANumber<20";
                            break;

                        case 4://MTPs
                            //ada.SelectCommand.CommandText = "select pnr,canum,filepath from HardTypedPapers where HardPaperCode between 2 and 5 and HardPaperCode != 3 and coursecode = '" + CourseCode + "'  union select  pnr,canumber,paperset from capapers where coursecode='" + CourseCode + "' and CANumber=998";
                            SelectQuery = "select pnr,canum,filepath from HardTypedPapers where HardPaperCode = 4 and coursecode = '" + CourseCode + "'  union select  pnr,canumber,paperset from capapers where coursecode='" + CourseCode + "' and CANumber=998";
                            break;

                        case 5://ETPs
                            //ada.SelectCommand.CommandText = "select pnr,canum,filepath from HardTypedPapers where HardPaperCode between 2 and 5 and HardPaperCode != 3 and coursecode = '" + CourseCode + "'  union select  pnr,canumber,paperset from capapers where coursecode='" + CourseCode + "' and CANumber=999";
                            SelectQuery = "select pnr,canum,filepath from HardTypedPapers where HardPaperCode = 5 and coursecode = '" + CourseCode + "'  union select  pnr,canumber,paperset from capapers where coursecode='" + CourseCode + "' and CANumber=999";
                            break;

                        default:
                            break;
                    }
                using (SqlDataAdapter ada = new SqlDataAdapter(SelectQuery, con))
                {
                    con.Open();
                    ada.Fill(Papers);
                }
                    
               // }
            }

            if (Papers.Rows.Count > 0)
            {
                string NewPaperFirstQuestion = tb_FirstQuestion.Text.Trim();

                int splitterSeek = 0, WordCount = 0;
                for (int c = 0; c < NewPaperFirstQuestion.Length; c++)
                {
                    splitterSeek = c;

                    if (WordCount > 6)
                        break;

                    if (NewPaperFirstQuestion.ElementAt<char>(c) == ' ') WordCount++;
                }

                NewPaperFirstQuestion = NewPaperFirstQuestion.Substring(0, splitterSeek);
                Debug.WriteLine("7 WORDS from textbox --" + NewPaperFirstQuestion + "----");

                DataTable DuplicaciesHolder = Papers.Clone();
                int hardcopies = 0, softcopies = 0;
                Debug.WriteLine(Papers.Rows.Count + "----------ROWS COUNT");
                foreach (DataRow paper in Papers.Rows)//checking for duplicacies
                {
                     if (paper["filepath"].ToString().Length > 10)
                    {
                        //is Hard Typed
                        string HardCApaperPath = paper["filepath"].ToString().Substring(paper["filepath"].ToString().IndexOf("blackchair.manhardeep.com"));//For Production Environment only
                        //string HardCApaperPath = Server.MapPath("Hard Typed Papers")+"/"+paper["filepath"].ToString().Substring(paper["filepath"].ToString().IndexOf("Hard Typed")+17);//For TEST Environment only
                        Debug.WriteLine("QUESTION FROM ALREADY ENTERED PDF-" + giveFirstQuestionfromPDF(HardCApaperPath).Trim()+"-");
                        Debug.WriteLine("Comparison ="+ NewPaperFirstQuestion == giveFirstQuestionfromPDF(HardCApaperPath).Trim() + "=");
                        if(NewPaperFirstQuestion == giveFirstQuestionfromPDF(HardCApaperPath))
                        {
                            //match found
                            //DuplicaciesHolder.ImportRow(paper);
                            //DuplicaciesHolder.Rows.Add(paper);
                            DuplicaciesHolder.Rows.Add(paper.ItemArray);
                            hardcopies++;
                            Debug.WriteLine("in side PDF DUPLICACY CHECK");
                        }
                    }
                    else
                    {
                        //is not Hard Typed
                        //Debug.WriteLine("textbox question: -"+NewPaperFirstQuestion+"-");
                        Debug.WriteLine("FROM DB question: -"+ giveFirstQuestionFromDB(paper["PNR"].ToString()) + "-");
                        if(NewPaperFirstQuestion == giveFirstQuestionFromDB(paper["PNR"].ToString()))
                            {
                            //match found
                            softcopies++;
                            //DuplicaciesHolder.ImportRow(paper);
                            //DuplicaciesHolder.Rows.Add(paper);
                            DuplicaciesHolder.Rows.Add(paper.ItemArray);
                            Debug.WriteLine("in side PDF DUPLICACY CHECK");
                        }
                    }
                }
                if (DuplicaciesHolder.Rows.Count > 0)
                {
                    lbl_DuplicacyMsg.Text = "Maybe we have this paper already. Check the following papers before adding this paper. If all the questions in the below papers and your paper are same, cancel this entry and Yo this paper. Otherwise, proceed to enter the paper. hrd="+hardcopies+" sft="+softcopies;
                    //display duplicacies holder
                    displayDuplicacies(DuplicaciesHolder);
                }
                else
                {
                    lbl_DuplicacyMsg.Text = "No matches found with all the existing papers of " + CourseCode.ToUpper()+". You may proceed.";
                }
            }
            else
            {
                //No papers for this course code entered before
                lbl_DuplicacyMsg.Text = "No papers for this course code entered before. You may proceed.";
            }
        }

        public void displayDuplicacies(DataTable Duplicacies)
        {
            Debug.WriteLine(Duplicacies.Rows.Count + "------DUP ROW COUNT");
            Debug.WriteLine(Duplicacies.Columns.Count + "------DUP Col COUNT");
            foreach(DataRow Duplicacy in Duplicacies.Rows)
            {
                HyperLink paperlink = new HyperLink();
                Debug.WriteLine("DUPLICACY COLUMN 2(0 based)-"+Duplicacy[2].ToString()+"-");
                if(Duplicacy[2].ToString().Length<3)
                {
                    //not hard
                    paperlink.NavigateUrl = "~/ViewQuestionPaper.aspx?p=" + new QueryStringEncryption().encryptQueryString(Duplicacy["PNR"].ToString());
                    string CAnumbering = Duplicacy["Canum"].ToString() == "999" ? " ETP" : Duplicacy["Canum"].ToString() == "998" ? " MTP" : " CA-"+Duplicacy["Canum"].ToString();
                    paperlink.Text = tb_CAmodal_ccode.Text.ToUpper() + CAnumbering;
                    paperlink.Target = "_blank";
                    paperlink.CssClass = "DynamicPaperLinks";
                }
                else
                {
                    //is hard paper
                    paperlink.NavigateUrl = "~/Hard Typed Papers/" + Duplicacy["PNR"].ToString()+".pdf";
                    string CAnumbering = Duplicacy["Canum"].ToString() == "999" ? " ETP" : Duplicacy["Canum"].ToString() == "998" ? " MTP" : " CA-" + Duplicacy["Canum"].ToString();
                    paperlink.Text = tb_CAmodal_ccode.Text.ToUpper() + CAnumbering;
                    paperlink.Target = "_blank";
                    paperlink.CssClass = "DynamicPaperLinks";
                }
                pnl_DynamicDupLinks.Controls.Add(paperlink);
            }
        }

        public string giveFirstQuestionfromPDF(string PDFpath)
        {
            PdfReader reader = new PdfReader(PDFpath);
            string FirstPageText = PdfTextExtractor.GetTextFromPage(reader, 1);

            FirstPageText = FirstPageText.Substring(FirstPageText.IndexOf("Q1.") + 4);
            
            int WordCount = 0, splitterSeek = 0;
            for (int c = 0; c < FirstPageText.Length; c++)
            {
                if (WordCount > 6)
                {
                    splitterSeek = c;
                    break;
                }
                if (FirstPageText.ElementAt<char>(c) == ' ') WordCount++;
            }
            return FirstPageText.Substring(0, splitterSeek);
        }

        public string giveFirstQuestionFromDB(string PNR)
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand("Select statement from questionpapersdump where pnr='" + PNR + "' and number = 1", con))
                {
                    con.Open();
                    string FirstQuestion =  cmd.ExecuteScalar().ToString();//can not be null, no need for nulls to check, presently
                    con.Close();
                    int WordCount = 0, splitterSeek = 0;
                    Debug.WriteLine("Before loop: -"+FirstQuestion+"-");

                    for (int c = 0; c < FirstQuestion.Length; c++)
                    {
                        splitterSeek = c;

                        if (WordCount > 6)
                            break;

                        if (FirstQuestion.ElementAt<char>(c) == ' ') WordCount++;
                    }

                    Debug.WriteLine("After loop: -" + FirstQuestion + "-");
                    Debug.WriteLine("Splitter seek: -" + splitterSeek+ "-");
                    Debug.WriteLine("after substring: -" + FirstQuestion.Substring(0, splitterSeek) + "-");

                    return FirstQuestion.Substring(0, splitterSeek);//returning first seven words
                }
            }
        }

        protected void btn_ConfirmCA_Click(object sender, EventArgs e)
        {
            pnl_CADupCheck.Visible = false;
            pnl_ModalBackground.Visible = false;
            tb_CourseCode.Text = tb_CAmodal_ccode.Text.ToUpper();
        }

        protected void routeRequest()
        {
            if (!cb_CA.Checked && !cb_PNRless.Checked && !cb_MTPETP.Checked && tb_PNRNumber.Visible && tb_PNRNumber.Text != "")//is straigt paper
                handleStraightPapers(false);
            else if (cb_CA.Checked && !cb_MTPETP.Checked)//is CA paper
                handleCAPapers();
            else if (cb_MTPETP.Checked)
                handleMTPETP(rbl_MTPETP.SelectedItem.Text == "MTP" ? 998 : 999);
            else if (cb_PNRless.Checked)
                handleStraightPapers(true);
            else
                lbl_msg.Text = "Other triggered";
        }

        protected void handleStraightPapers(bool isPNRless)
        {
            if(isNewPaper(tb_PNRNumber.Text.Trim()))
            {
                string PaperPath = Server.MapPath("~") + "/Hard Typed Papers/" + tb_PNRNumber.Text.Trim() + ".pdf";
                System.Diagnostics.Debug.WriteLine("PAPER PATH-" + PaperPath + "-");
                if(isPNRless)
                addStrightHardPaper(PaperPath,true);
                else
                    addStrightHardPaper(PaperPath,false);



            }
            else
            {
                lbl_msg.Text = "This paper is already with us. Recheck and report to +91 9988255277. Do not Yo this paper, Un-Yo if already Yoed.";
                hl_PaperLink.Visible = false;
            }
        }

        protected void handleCAPapers()
        {
            string newPNR = tb_CourseCode.Text.Trim().ToUpper() + DateTime.Now.Ticks.ToString();

            string PaperPath = Server.MapPath("~") + "/Hard Typed Papers/" + newPNR + ".pdf";

            DateTime NowTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, IST);
            string TimeStamp = NowTime.ToLongDateString() + " " + NowTime.ToLongTimeString();

            using (SqlConnection con_PaperAdded = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                using (SqlCommand cmd_PaperAdder = new SqlCommand(string.Empty, con_PaperAdded))
                {
                    cmd_PaperAdder.CommandText = "insert into HardTypedPapers values('" + newPNR + "','2','"+ Convert.ToInt32(tb_CAnum.Text)+"','sh-lo','" + tb_TimeAllowed.Text.Trim() + "','" + tb_MaxMarks.Text.Trim() + "','" + tb_PaperSet.Text.Trim() + "','" + tb_School.Text + "','" + tb_CourseCode.Text.Trim().ToUpper() + "','" + tb_CourseName.Text.Trim() + "','" + tb_DriveLink.Text.Trim() + "','" + PaperPath + "','" + TimeStamp + "')";

                    con_PaperAdded.Open();
                    if (cmd_PaperAdder.ExecuteNonQuery() == 1)
                    {
                        fuc_PNRlessPaper.PostedFile.SaveAs(PaperPath);

                        hl_PaperLink.NavigateUrl = "~/Hard Typed Papers/" + newPNR + ".pdf";
                        hl_PaperLink.Visible = true;
                        lbl_msg.Text = "Paper Added Successfully";
                    }
                    else
                        lbl_msg.Text = "There was an error entering the paper into BlackChair. You ought to report this error to the System Admin. Do not Yo this paper. call +91 9988255277";
                }
            }
        }

        protected void handleMTPETP(int MTPorETP)//998 = MTP and 999 = ETP for MTP/ETP
        {
            string newPNR = tb_CourseCode.Text.Trim().ToUpper() + DateTime.Now.Ticks.ToString();

            string PaperPath = Server.MapPath("~") + "/Hard Typed Papers/" + newPNR + ".pdf";

            DateTime NowTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, IST);
            string TimeStamp = NowTime.ToLongDateString() + " " + NowTime.ToLongTimeString();

            using (SqlConnection con_PaperAdded = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                using (SqlCommand cmd_PaperAdder = new SqlCommand(string.Empty, con_PaperAdded))
                {
                    int HardPaperCode = MTPorETP == 999 ? 5 : 4;

                    cmd_PaperAdder.CommandText = "insert into HardTypedPapers values('" + newPNR + "','"+ HardPaperCode + "','"+ MTPorETP + "','sh-lo','" + tb_TimeAllowed.Text.Trim() + "','" + tb_MaxMarks.Text.Trim() + "','" + tb_PaperSet.Text.Trim() + "','" + tb_School.Text + "','" + tb_CourseCode.Text.Trim().ToUpper() + "','" + tb_CourseName.Text.Trim() + "','" + tb_DriveLink.Text.Trim() + "','" + PaperPath + "','" + TimeStamp + "')";

                    con_PaperAdded.Open();
                    if (cmd_PaperAdder.ExecuteNonQuery() == 1)
                    {
                        fuc_PNRlessPaper.PostedFile.SaveAs(PaperPath);

                        hl_PaperLink.NavigateUrl = "~/Hard Typed Papers/" + newPNR + ".pdf";
                        hl_PaperLink.Visible = true;
                        lbl_msg.Text = "Paper Added Successfully";
                    }
                    else
                        lbl_msg.Text = "There was an error entering the paper into BlackChair. You ought to report this error to the System Admin. Do not Yo this paper. call +91 9988255277";
                }
            }
        }

        protected void rbl_MTPETP_SelectedIndexChanged(object sender, EventArgs e)
        {
            cb_CA.Checked = true;
            cb_CA_CheckedChanged(new object(), new EventArgs());
        }

        protected void btn_CheckPNR_Click(object sender, EventArgs e)
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand("with AllPapers as (select pnr from HardTypedPapers union all select pnr from papers) select count(*) from AllPapers where pnr='" + tb_CheckPNR.Text+"'", con))
                {
                    con.Open();
                    int result = Convert.ToInt32(cmd.ExecuteScalar().ToString());
                    if (result==1)
                    {
                        lbl_CheckPNR.Text = "This paper is already with us. Yo this paper and mail it back to the one who sent it to you. Stating that this papers is already entered.";
                    }
                    else if(result==0)
                    {
                        lbl_CheckPNR.Text = "This paper is not entered already. You may proceed.";
                    }
                    else
                    {
                        lbl_CheckPNR.Text = "INCONSISTENCY DETECTED! This is an internal database error. You need to report it to the DB Admin (+91 9988255277). This paper might be already with us. Mail back this paper stating the error message to the sender.";
                    }
                }
            }
        }
    }
}