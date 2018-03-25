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

namespace DataCollection
{
    public partial class ViewQuestionPaper : System.Web.UI.Page
    {

        static SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString);

        public string doc_questionNumbering = "";

        protected void Page_Load(object sender, EventArgs e)
        {
            verifyUserAuthentication();
            if (Request.QueryString["p"] != null)
                displayPDFwithPNR(new QueryStringEncryption().decryptQueryString(HttpUtility.UrlDecode(Request.QueryString["p"].ToString())));
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


        protected void Button2_Click(object sender, EventArgs e)
        {
            displayPDFwithPNR(TextBox1.Text);
        }
    
        ////Paper generation methods below
        /*
        private void generatePaperForType_ob(string PNR)
        {
            //////fetching values for Top Content
            SqlCommand fetcher = new SqlCommand();
            fetcher.Connection = con;
            con.Open();
            //fetch Paper Set
            fetcher.CommandText = "select paperset from papers where pnr='" + PNR + "'";
            string PaperSet = fetcher.ExecuteScalar().ToString();

            //fetch Course Code
            fetcher.CommandText = "select CourseCode from papers where pnr='" + PNR + "'";
            string CourseCode = fetcher.ExecuteScalar().ToString();

            //fetch Course Name
            fetcher.CommandText = "select CourseName from papers where pnr='" + PNR + "'";
            string CourseName = fetcher.ExecuteScalar().ToString();

            //fetch TimeAllowed
            fetcher.CommandText = "select TimeAllowed from papers where pnr='" + PNR + "'";
            int TimeAllowed = Convert.ToInt32(fetcher.ExecuteScalar().ToString());

            ////Calculate MaxMarks
            //fetch weightage per question
            fetcher.CommandText = "Select top 1(weightage) from questionpapersdump where pnr='" + PNR + "'";
            float weightage = (float)Convert.ToDecimal(fetcher.ExecuteScalar().ToString());

            //fetch number of questions
            fetcher.CommandText = "Select count(*) from questionpapersdump where pnr='" + PNR + "'";
            int QuestionsCount = Convert.ToInt32(fetcher.ExecuteScalar().ToString());

            float MaxMarks = weightage * QuestionsCount;
            ///END///fetching values for Top Content 

            //Instantiating Document Class
            var doc = new Document(PageSize.A4, 52, 52, 60, 50);
            PdfWriter.GetInstance(doc, new FileStream(Server.MapPath("PDFs") + "/Doc1.pdf", FileMode.Create));
            doc.Open();

            printTopContent(doc, PaperSet, PNR, CourseCode, CourseName, TimeAllowed, MaxMarks);

            printInstructions(doc, QuestionsCount, weightage);

            //Chaching the questions of this question paper
            DataTable PaperQuestions = new DataTable("PaperQuestions");

            SqlDataAdapter paperdata = new SqlDataAdapter("Select * from questionpapersdump where pnr='" + PNR + "' order by Number", con);
            paperdata.Fill(PaperQuestions);
            con.Close();
            for (int c = 0; c < QuestionsCount; c++)
            {
                DataRow question = PaperQuestions.Rows[c];

                string questionPattern = detectQuestionPattern(question);
                //return value of the format < digit >< digit > first for question diag second for option diag

                //printMCQQuestion(doc, question, questionPattern,true);
                printMCQQuestion(doc, question, questionPattern, true);
            }

            putEndMark(doc);

            doc.Close();

            AddPageNumber(Server.MapPath("PDFs") + "/Doc1.pdf");//adding page numbers

            paperlink.Text = "Download PDF here: ";
            staticlink.Visible = true;
            lbl_temp.Text = "Done";
            lbl_temp.Visible = true;



        }
        
        private void printTopContent(Document doc, string PaperSet, string PNR, string CourseCode, string CourseName, int TimeAllowed, float MaxMarks)
        {
            //printing paper code
            Paragraph PaperCode = new Paragraph("Paper Code: " + PaperSet.ToUpper(), papercodebold);
            PaperCode.Alignment = Element.ALIGN_RIGHT;

            Paragraph rno = new Paragraph("Registration No:.__________________\n", Topsmall);
            Paragraph cdetails = new Paragraph("COURSE CODE : " + CourseCode + "\nCOURSE NAME : " + CourseName + "\n", TopHeader);
            Paragraph pnr = new Paragraph("PNR No:: " + PNR + "\n", Topsmall);
            PdfPTable paperdetails = new PdfPTable(2);
            paperdetails.SpacingBefore = 5;

            PdfPCell ta = new PdfPCell(new Phrase("Time Allowed: " + formatTime(TimeAllowed), Topsmall));
            PdfPCell mm = new PdfPCell(new Phrase("Max.Marks: " + MaxMarks, Topsmall));


            ta.BorderColor = BaseColor.WHITE;
            mm.BorderColor = BaseColor.WHITE;
            ta.HorizontalAlignment = 0;
            mm.HorizontalAlignment = 2;
            paperdetails.AddCell(ta);
            paperdetails.AddCell(mm);
            paperdetails.AddCell(new PdfPCell(new Phrase("Time Allowed: " + formatTime(TimeAllowed), Topsmall)));//???
            paperdetails.WidthPercentage = 100;

            pnr.Alignment = Element.ALIGN_RIGHT;
            rno.Alignment = Element.ALIGN_CENTER;
            cdetails.Alignment = Element.ALIGN_CENTER;

            // if(PaperSet!="")
            doc.Add(PaperCode);
            doc.Add(rno);
            doc.Add(pnr);
            doc.Add(cdetails);
            doc.Add(paperdetails);

        }

        private void printInstructions(Document doc, int QuestionCount, float weightage)
        {
            Paragraph instinst = new Paragraph("Read the following instructions carefully before attempting the question paper.", fnt_inst);

            List instructions = new List(List.ORDERED, 10);

            iTextSharp.text.ListItem li_inst1 = new iTextSharp.text.ListItem(" Match the Paper Code shaded on the OMR Sheet with the Paper Code mentioned on the question paper and ensure that both are the same.", fnt_inst);
            iTextSharp.text.ListItem li_inst2 = new iTextSharp.text.ListItem(" This question paper contains " + QuestionCount + " questions of " + weightage + " mark each. 0.25 marks will be deducted for each wrong answer.", fnt_inst);
            iTextSharp.text.ListItem li_inst3 = new iTextSharp.text.ListItem(" Do not write or mark anything on the question paper except your registration no. on the designated space.", fnt_inst);
            iTextSharp.text.ListItem li_inst4 = new iTextSharp.text.ListItem(" Submit the question paper and the rought sheet(s) along with the OMR sheet to the invigilator before leaving the examination hall.", fnt_inst);
            //iTextSharp.text.ListItem li_inst5 = new iTextSharp.text.ListItem(" Do not write or mark anything on the question paper except your registration no. on the designated space.", fnt_inst);

            instructions.Add(li_inst1);
            instructions.Add(li_inst2);
            instructions.Add(li_inst3);
            instructions.Add(li_inst4);
            //instructions.Add(li_inst5);

            doc.Add(instinst);
            doc.Add(instructions);
            doc.Add(new Paragraph("\n"));
        }
        */
        private string detectQuestionPattern(DataRow question)
        {
            string returner = "";//of the format <digit><digit> first for question diag second for option diag
            string returner_ques = "";
            string returner_opt = "";
            if (question["Diagram"].ToString() == "")
                returner_ques = "0";
            else
                returner_ques = "1";

            if (question["Option1File"].ToString() == "" && question["Option2File"].ToString() == "" && question["Option3File"].ToString() == "" && question["Option4File"].ToString() == "")
                returner_opt = "0";
            else
                returner_opt = "1";

            returner = returner_ques + returner_opt;
            return returner;
        }//detects and tell diagram status. 00/01/10/11

        private string giveImageName(string ImageURL)
        {
            return ImageURL.Substring(ImageURL.IndexOf("/Diagrams/") + 10);
        }

        private string formatMarks(string rawMarks)
        {
            if (rawMarks.Substring(rawMarks.IndexOf(".") + 1) == "00")
                return rawMarks.Substring(0, rawMarks.IndexOf("."));
            else
                return rawMarks;
        }

        private string PaperType(string PNR)
        {
            if (PNR.Length > 17)
                return "CA";
            else
                return "MTE/ETE";
        }

        private string formatTime(int mins)
        {
            int hrs = mins / 60;
            int min = mins % 60;

            return "0" + hrs + ":" + min + (min == 0 ? "0" : "") + " hrs";
        }

        public void displayPDFwithPNR(string PNR)
        {
            //this function generates the question paper of the passed PNR
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString);
            SqlCommand patternFind = new SqlCommand("Select pattern from papers where pnr='" + PNR + "'", con);

            con.Open();
            string pattern = "";
            if (PNR.Length < 17)
                pattern = patternFind.ExecuteScalar().ToString();

            //Top Detials
            if (PaperType(PNR) == "MTE/ETE")
            {
                pnl_CaPaper.Visible = false;
                pnl_Paper.Visible = true;

                lbl_pnr.Text = PNR;

                SqlCommand Fetch_Ccode = new SqlCommand("Select CourseCode from papers where pnr='" + PNR + "'", con);
                string Ccode = Fetch_Ccode.ExecuteScalar().ToString();

                SqlCommand Fetch_Cname = new SqlCommand("Select CourseName from papers where pnr='" + PNR + "'", con);
                string Corasname = Fetch_Cname.ExecuteScalar().ToString();

                SqlCommand Fetch_TimeAllowed = new SqlCommand("Select TimeAllowed from papers where pnr='" + PNR + "'", con);
                int TimeAllowed = Convert.ToInt32(Fetch_TimeAllowed.ExecuteScalar());

                SqlCommand Fetch_MaxMarks = new SqlCommand("Select maxmarks from papers where pnr='" + PNR + "'", con);
                string maxmarks = Fetch_MaxMarks.ExecuteScalar().ToString();

                SqlCommand Fetch_PartAquesCount = new SqlCommand("select count(*) from QuestionPapersDump where  number=1 and pnr='" + PNR + "'", con);
                int no_partA = Convert.ToInt32(Fetch_PartAquesCount.ExecuteScalar());

                SqlCommand Fetch_PartBquesCount = new SqlCommand("Select max(number) from questionpapersdump where pnr='" + PNR + "'", con);
                int no_partB = Convert.ToInt32(Fetch_PartBquesCount.ExecuteScalar()) - 1;

                //fetch Paper Set
                Fetch_Ccode.CommandText = "select paperset from papers where pnr='" + PNR + "'";
                var ps = Fetch_Ccode.ExecuteScalar();
                string PaperSet = ps == null ? "" : ps.ToString();

                SqlCommand Fetch_PartAquesWeightage = new SqlCommand("select avg(weightage) from QuestionPapersDump where  number=1 and pnr='" + PNR + "'", con);
                float wt_partA = (float)Convert.ToDecimal(Fetch_PartAquesWeightage.ExecuteScalar());

                SqlCommand Fetch_PartBquesWeightage = new SqlCommand("select sum(weightage) from QuestionPapersDump where  number=2 and pnr='" + PNR + "'", con);

                lbl_coursecode.Text = Ccode;
                lbl_coursename.Text = Corasname;
                lbl_timeallowed.Text = formatTime(Convert.ToInt32(TimeAllowed.ToString()));
                lbl_maxmarks.Text = maxmarks;
                lbl_npartA.Text = no_partA.ToString();
                lbl_npartB.Text = no_partB.ToString();
                lbl_wpartA.Text = wt_partA.ToString();

                lbl_npartA1.Text = no_partA.ToString();
                lbl_npartB1.Text = no_partB.ToString();
                lbl_wpartA1.Text = wt_partA.ToString();

                Page.Title = Ccode + " : " + Corasname + " Display Paper";

                float wt_partB;
                //if (!Convert.IsDBNull(Fetch_PartBquesWeightage.ExecuteScalar()))
                //{
                wt_partB = (float)Convert.ToInt32(Fetch_PartBquesWeightage.ExecuteScalar()) / 2;
                lbl_wpartB.Text = wt_partB.ToString();
                lbl_wpartB1.Text = wt_partB.ToString();

                //}
                //else
                //{
                //    wt_partB = 0f;
                //}
                //Top details end

                //Generic stuff
                //DOC Style Reduntant//   var doc1 = new Document(PageSize.A4, 52, 52, 60, 50);
                //DOC Style Reduntant//   string path = Server.MapPath("PDFs");
                //DOC Style Reduntant//PdfWriter.GetInstance(doc1, new FileStream(path + "/Doc1.pdf", FileMode.Create));
                //int j = TextBox1.Text == "" ? 8 : Convert.ToInt32(TextBox1.Text);

                //DOC Style Reduntant//doc1.Open();

                //DOC Style Reduntant//Paragraph PaperCode = new Paragraph("Paper Code: " + PaperSet.ToUpper(), papercodebold);
                //DOC Style Reduntant//PaperCode.Alignment = Element.ALIGN_RIGHT;

                //DOC Style Reduntant//Paragraph rno = new Paragraph("Registration No:.__________________\n", Topsmall);


                //DOC Style Reduntant//Paragraph cdetails = new Paragraph("COURSE CODE : " + Ccode + "\nCOURSE NAME : " + Corasname + "\n", TopHeader);
                //DOC Style Reduntant//Paragraph pnr = new Paragraph("PNR No:: " + PNR + "\n", Topsmall);
                //DOC Style Reduntant//PdfPTable paperdetails = new PdfPTable(2);
                //DOC Style Reduntant//paperdetails.SpacingBefore = 5;

                //DOC Style Reduntant//PdfPCell ta = new PdfPCell(new Phrase("Time Allowed: " + formatTime(TimeAllowed), Topsmall));
                //DOC Style Reduntant//PdfPCell mm = new PdfPCell(new Phrase("Max.Marks: " + maxmarks, Topsmall));

                //DOC Style Reduntant//Paragraph NewLine_s = new Paragraph("\n", Topsmall);

                //DOC Style Reduntant//Paragraph instinst = new Paragraph("Read the following instructions carefully before attempting the question paper.", fnt_inst);

                //DOC Style Reduntant//
                //DOC Style Reduntant//
                /*
                ta.BorderColor = BaseColor.WHITE;
                mm.BorderColor = BaseColor.WHITE;
                ta.HorizontalAlignment = 0;
                mm.HorizontalAlignment = 2;
                paperdetails.AddCell(ta);
                paperdetails.AddCell(mm);
                paperdetails.AddCell(new PdfPCell(new Phrase("Time Allowed: " + formatTime(TimeAllowed), Topsmall)));//???
                paperdetails.WidthPercentage = 100;

                pnr.Alignment = Element.ALIGN_RIGHT;
                rno.Alignment = Element.ALIGN_CENTER;
                cdetails.Alignment = Element.ALIGN_CENTER;

                List instructions = new List(List.ORDERED, 10);

                iTextSharp.text.ListItem li_inst1 = new iTextSharp.text.ListItem(" This question paper is divided into two parts A and B.", fnt_inst);
                iTextSharp.text.ListItem li_inst2 = new iTextSharp.text.ListItem(" Part A contains " + no_partA + " questions of " + wt_partA + " marks each. All questions are compulsary.", fnt_inst);
                iTextSharp.text.ListItem li_inst3 = new iTextSharp.text.ListItem(" Part B contains " + no_partB + " questions of " + wt_partB + " marks each. In each question attempt either question (a) or (b), in case  both (a) and (b) questions are attempted for any question then only the first attempted question will be evaluated.", fnt_inst);
                iTextSharp.text.ListItem li_inst4 = new iTextSharp.text.ListItem(" Answer all questions in serial order.", fnt_inst);
                iTextSharp.text.ListItem li_inst5 = new iTextSharp.text.ListItem(" Do not write or mark anything on the question paper except your registration no. on the designated space.", fnt_inst);

                instructions.Add(li_inst1);
                instructions.Add(li_inst2);
                instructions.Add(li_inst3);
                instructions.Add(li_inst4);
                instructions.Add(li_inst5);
                */

                //DOC Style Reduntant//Paragraph parta = new Paragraph("\nPART A\n");
                //string doc_parta = "Part A";

                //DOC Style Reduntant//parta.Alignment = Element.ALIGN_CENTER;
                //DOC Style Reduntant//parta.Font.SetStyle(Font.BOLDITALIC);
                //DOC Style Reduntant//parta.Font.Size = 11;
                //DOC Style Reduntant//Paragraph partb = new Paragraph("\nPART B\n");

                //string doc_partb = "Part B";
                //DOC Style Reduntant//partb.Alignment = Element.ALIGN_CENTER;
                //DOC Style Reduntant//partb.Font.SetStyle(Font.BOLDITALIC);
                //DOC Style Reduntant//partb.Font.Size = 11;

                //DOC Style Reduntant//Paragraph or = new Paragraph("OR\n");
                //string doc_or = "OR";
                //DOC Style Reduntant//or.Font.SetStyle(Font.BOLDITALIC);
                //DOC Style Reduntant//or.Alignment = Element.ALIGN_CENTER;
                //DOC Style Reduntant//or.Font.Size = 11;
                //DOC Style Reduntant//or.SpacingAfter = 2;

                //DOC Style Reduntant//Paragraph endMark = new Paragraph("\n-- End of Question Paper --", footerfont);
                //DOC Style Reduntant//endMark.Alignment = Element.ALIGN_CENTER;
                // if(PaperSet!="")
                // doc1.Add(PaperCode);
                if (pattern == "ob-lo")
                {
                    lbl_papercode.Text = PaperSet.ToUpper();
                    hdoc_PaperCode.Visible = true;
                    // InstFOob.Visible = true;
                    // InstFOsholo.Visible = false;
                    //InstFoObonly.Visible = false;
                }
                //DOC Style Reduntant//doc1.Add(PaperCode);
                //DOC Style Reduntant//doc1.Add(rno);
                //DOC Style Reduntant//doc1.Add(pnr);
                //DOC Style Reduntant//doc1.Add(cdetails);
                //DOC Style Reduntant//doc1.Add(paperdetails);
                //DOC Style Reduntant//doc1.Add(instinst);//instruction of instructions
                //DOC Style Reduntant//doc1.Add(instructions);


                //Generic stuff ends
                DataTable PaperQuestions = new DataTable("PaperQuestions");

                if (pattern == "sh-lo")//later generatePDF(String pnr,string pattern);
                {
                    //DOC Style Reduntant//doc1.Add(parta);
                    //hdoc_partHeaders.Text = doc_parta;
                    hdoc_PaperCode.Visible = false;
                    renderShort(PNR, no_partA, no_partB);

                    InstFOob.Visible = false;
                    InstFoObonly.Visible = false;
                    InstFOsholo.Visible = true;

                    SqlDataAdapter paperdata = new SqlDataAdapter("Select * from questionpapersdump where pnr='" + PNR + "' order by QuestionID", con);
                    paperdata.Fill(PaperQuestions);

                    //Rendering Short questions PART A
                    //if < 80 chars
                    ///////////short question - simple model
                    for (int c = 0; c < no_partA; c++)
                    {
                        //DOC Style Reduntant//PdfPTable shortques = new PdfPTable(2);
                        //DOC Style Reduntant//shortques.WidthPercentage = 100;
                        //DOC Style Reduntant//shortques.SpacingAfter = 2;

                        //DOC Style Reduntant//float[] widths = new float[] { 8.8f, 1.2f };
                        //DOC Style Reduntant//shortques.SetWidths(widths);

                        //DOC Style Reduntant//PdfPTable sqlongquestion = new PdfPTable(2);
                        //DOC Style Reduntant//sqlongquestion.WidthPercentage = 100;


                        //DOC Style Reduntant//Phrase questionNumbering = new Phrase();

                        DataRow question = PaperQuestions.Rows[c];

                        /////for diagram laden questions 
                        //DOC Style Reduntant//string diagramifany = question["Diagram"].ToString();
                        //DOC Style Reduntant//if (diagramifany != "")
                        //DOC Style Reduntant//diagramifany = diagramifany.Substring(diagramifany.IndexOf("blackchair.manhardeep.com"));
                        //DOC Style Reduntant//PdfPCell DiagramHolder = new PdfPCell();
                        //DOC Style Reduntant//Anchor diagramlink = new Anchor();
                        if (false)
                        {
                            //DOC Style Reduntant//iTextSharp.text.Image diagr = iTextSharp.text.Image.GetInstance(Server.MapPath("Diagrams") + "/" + "IMG_4015.JPG");
                            //DOC Style Reduntant//diagr.ScaleToFit(new Rectangle(0f, 0f, 300f, 300f));
                            //DOC Style Reduntant//DiagramHolder = new PdfPCell(diagr);
                            //DOC Style Reduntant//DiagramHolder.BorderColor = BaseColor.WHITE;
                            //DOC Style Reduntant//DiagramHolder.Colspan = 2;
                            //DOC Style Reduntant//DiagramHolder.HorizontalAlignment = Element.ALIGN_CENTER;
                            //DOC Style Reduntant//DiagramHolder.PaddingTop = 5;

                            //DOC Style Reduntant//diagramlink = new Anchor("View Full size", diagramLinks);
                            //DOC Style Reduntant//diagramlink.Reference = diagramifany;
                        }
                        //////Diagram code ends

                        /*
                        if (question["Part"].ToString() == "a" || question["Part"].ToString() == "")
                            questionNumbering.Add(new Phrase("Q" + question["Number"] + " ", boldquestionfont));
                        questionNumbering.Add(new Phrase(question["Part"] + ") ", boldquestionfont));
                        if (question["SubPart"].ToString() != "")
                            questionNumbering.Add(new Phrase("(" + question["SubPart"] + ") ", boldquestionfont));
                        */

                        //DOC Style Reduntant// if (question["Part"].ToString() == "a" || question["Part"].ToString() == "")
                        //DOC Style Reduntant//doc_questionNumbering = "Q" + question["Number"] + " ";
                        //DOC Style Reduntant// questionNumbering.Add(new Phrase("Q" + question["Number"] + " ", boldquestionfont));
                        //if (question["SubPart"].ToString() == "i" || question["SubPart"].ToString() == "")
                        //DOC Style Reduntant//if ((question["SubPart"].ToString() == "i" || question["SubPart"].ToString() == "") && question["Part"].ToString() != "")
                        //DOC Style Reduntant//doc_questionNumbering = question["Part"] + ") ";
                        //DOC Style Reduntant//questionNumbering.Add(new Phrase(question["Part"] + ") ", boldquestionfont));
                        //DOC Style Reduntant//if (question["SubPart"].ToString() != "")
                        //DOC Style Reduntant//doc_questionNumbering = "(" + question["SubPart"] + ") ";
                        //DOC Style Reduntant//questionNumbering.Add(new Phrase("(" + question["SubPart"] + ") ", boldquestionfont));
                        //  rpt_Questions.DataSource = PaperQuestions;
                        //rpt_Questions.DataBind();

                        //DOC Style Reduntant//Phrase sqst = new Phrase(question["Statement"].ToString(), questionfont);
                        string doc_sqst = question["Statement"].ToString();
                        //DOC Style Reduntant//Phrase sadder = new Phrase();
                        //DOC Style Reduntant//sadder.Add(questionNumbering);
                        //DOC Style Reduntant//sadder.Add(sqst);

                        string marksstring = question["Weightage"].ToString().Substring(0, question["Weightage"].ToString().Length - 1);
                        if (marksstring.ElementAt<char>(marksstring.Length - 1) == '0')
                            marksstring = marksstring.Substring(0, marksstring.Length - 2);

                        //DOC Style Reduntant//Phrase sqm = new Phrase("[ " + marksstring + " Marks ]", questionfont);
                        string doc_sqm = "[ " + marksstring + " Marks ]";
                        //DOC Style Reduntant//sqm.SetLeading(0, 0);

                        //DOC Style Reduntant//PdfPCell sqstmt = new PdfPCell(sadder);
                        //DOC Style Reduntant//sqstmt.BorderColor = BaseColor.WHITE;
                        //DOC Style Reduntant//sqstmt.Padding = 0;

                        //DOC Style Reduntant//PdfPCell smarks = new PdfPCell(sqm);
                        //DOC Style Reduntant//smarks.BorderColor = BaseColor.WHITE;
                        //DOC Style Reduntant//smarks.HorizontalAlignment = Element.ALIGN_RIGHT;
                        //DOC Style Reduntant//smarks.Padding = 0;

                        //DOC Style Reduntant//shortques.AddCell(sqstmt);
                        //DOC Style Reduntant//shortques.AddCell(smarks);
                        /////[ENDS]//////short question - simple model 
                        //////////short question - complex model
                        //DOC Style Reduntant//PdfPCell sqquesSTMT = new PdfPCell(sadder);
                        //DOC Style Reduntant//sqquesSTMT.Colspan = 2;
                        //DOC Style Reduntant//sqquesSTMT.BorderColor = BaseColor.WHITE;
                        //DOC Style Reduntant//sqquesSTMT.Padding = 0;

                        //DOC Style Reduntant//PdfPCell sqkhali = new PdfPCell();
                        //DOC Style Reduntant//sqkhali.BorderColor = BaseColor.WHITE;

                        //DOC Style Reduntant//sqlongquestion.AddCell(sqquesSTMT);
                        //DOC Style Reduntant//sqlongquestion.AddCell(sqkhali);
                        //DOC Style Reduntant//sqlongquestion.AddCell(smarks);
                        /////[ENDS]//////short question - complex model

                        //////ADD diagram to long question table - simple model
                        if (false)
                        {
                            //////adding link to the full size image - cleaner method
                            //DOC Style Reduntant//Chunk chu = new Chunk("See full size", diagramLinks);
                            string doc_chu = "See full size";
                            //DOC Style Reduntant//chu.SetAnchor(diagramifany);
                            //DOC Style Reduntant//Phrase p = new Phrase(chu);

                            //DOC Style Reduntant//PdfPCell dcell = new PdfPCell(p);
                            //DOC Style Reduntant//dcell.Colspan = 2;
                            //DOC Style Reduntant//dcell.HorizontalAlignment = Element.ALIGN_CENTER;
                            //DOC Style Reduntant//dcell.BorderColor = BaseColor.WHITE;
                            /////adding link ends

                            //DOC Style Reduntant//shortques.AddCell(DiagramHolder);
                            //DOC Style Reduntant//shortques.AddCell(dcell);

                            //DOC Style Reduntant//sqlongquestion.AddCell(DiagramHolder);
                            //DOC Style Reduntant//sqlongquestion.AddCell(dcell);

                        }

                        //////adding diagram to long question(simple) ends
                        //if(less than 80)
                        if (question["Statement"].ToString().Length < 80)

                        {//DOC Style Reduntant// doc1.Add(shortques);
                         /******add simple model *********/

                        }
                        else
                        {
                            /*************add complex model *********/
                        }
                        //DOC Style Reduntant//doc1.Add(sqlongquestion);
                        //short questions rendering ends
                    }//short questions loop end
                    /***************PART B *********************/
                    //DOC Style Reduntant//doc1.Add(partb);
                    //begin entering part B long questions

                    DataTable RowsAboveThisQuestion = DataCollection.AddQuestions.createLocalTable("OH yeah");

                    for (int c = no_partA; c < PaperQuestions.Rows.Count; c++)//int totalques = PaperQuestions.Rows.Count;
                    {
                        DataRow question = PaperQuestions.Rows[c];

                        //Adding OR between questions of choice
                        System.Diagnostics.Debug.WriteLine(question["Number"].ToString() + " " + question["Part"].ToString() + " - " + isORPlacable(RowsAboveThisQuestion, question["PNR"].ToString(), question["Number"].ToString(), question["Part"].ToString()));
                        if (isORPlacable(RowsAboveThisQuestion, question["PNR"].ToString(), question["Number"].ToString(), question["Part"].ToString()))
                        {/************or************/}
                        //DOC Style Reduntant//    doc1.Add(or);
                        //END//Adding OR between questions of choice


                        //DOC Style Reduntant//string diagramifany = question["Diagram"].ToString();

                        //              if (diagramifany != "")
                        //                diagramifany = diagramifany.Substring(diagramifany.IndexOf("blackchair.manhardeep.com"));
                        //DOC Style Reduntant//PdfPCell DiagramHolder = new PdfPCell();
                        //DOC Style Reduntant//Anchor diagramlink = new Anchor();
                        if (false)
                        {
                            //DOC Style Reduntant//iTextSharp.text.Image diagr = iTextSharp.text.Image.GetInstance(Server.MapPath("Diagrams") + "/" + giveImageName(question["Diagram"].ToString()));
                            //DOC Style Reduntant//diagr.ScaleToFit(new Rectangle(0f, 0f, 300f, 300f));
                            //DOC Style Reduntant//DiagramHolder = new PdfPCell(diagr);
                            //DOC Style Reduntant//DiagramHolder.BorderColor = BaseColor.WHITE;
                            //DOC Style Reduntant//DiagramHolder.Colspan = 2;
                            //DOC Style Reduntant//DiagramHolder.HorizontalAlignment = Element.ALIGN_CENTER;
                            //DOC Style Reduntant//DiagramHolder.PaddingTop = 5;

                            //DOC Style Reduntant// diagramlink = new Anchor("View Full size", diagramLinks);
                            //DOC Style Reduntant// diagramlink.Reference = diagramifany;
                        }
                        //////Diagram code ends

                        if (question["Statement"].ToString().Length > 80)
                        {
                            //complex model of long question of part B
                            //DOC Style Reduntant// Longsqlongquestion = new PdfPTable(2);
                            //DOC Style Reduntant//Longsqlongquestion.WidthPercentage = 100;

                            //DOC Style Reduntant//Phrase Longsqst = new Phrase(question["Statement"].ToString(), questionfont);

                            //DOC Style Reduntant//Phrase LongquestionNumbering = new Phrase();
                            /*
                            if (question["Part"].ToString() == "a" || question["Part"].ToString() == "")
                                LongquestionNumbering.Add(new Phrase("Q" + question["Number"] + " ", boldquestionfont));
                            LongquestionNumbering.Add(new Phrase("" + question["Part"] + ") ", boldquestionfont));
                            if (question["SubPart"].ToString() != "")
                                LongquestionNumbering.Add(new Phrase("(" + question["SubPart"] + ") ", boldquestionfont));

                            */
                            string doc_LongquestionNumbering = "";
                            if (question["Part"].ToString() == "a" || question["Part"].ToString() == "")
                                doc_LongquestionNumbering = "Q" + question["Number"] + " ";
                            //DOC Style Reduntant//LongquestionNumbering.Add(new Phrase("Q" + question["Number"] + " ", boldquestionfont));
                            if (question["SubPart"].ToString() == "i" || question["SubPart"].ToString() == "")
                                doc_LongquestionNumbering = "" + question["Part"] + ") ";
                            //DOC Style Reduntant//.Add(new Phrase("" + question["Part"] + ") ", boldquestionfont));
                            if (question["SubPart"].ToString() != "")
                                doc_LongquestionNumbering = "(" + question["SubPart"] + ") ";
                            //DOC Style Reduntant//LongquestionNumbering.Add(new Phrase("(" + question["SubPart"] + ") ", boldquestionfont));



                            //DOC Style Reduntant//Phrase Longsadder = new Phrase();
                            //DOC Style Reduntant//Longsadder.Add(LongquestionNumbering);
                            //DOC Style Reduntant//Longsadder.Add(Longsqst);


                            string marksstring = question["Weightage"].ToString().Substring(0, question["Weightage"].ToString().Length - 1);
                            if (marksstring.ElementAt<char>(marksstring.Length - 1) == '0')
                                marksstring = marksstring.Substring(0, marksstring.Length - 2);


                            //DOC Style Reduntant//Phrase Longsqm = new Phrase("[ " + marksstring + " Marks ]", questionfont);
                            string doc_Longsqm = "[ " + marksstring + " Marks ]";
                            //DOC Style Reduntant//Longsqm.SetLeading(0, 0);

                            //DOC Style Reduntant//PdfPCell pcelllongsqm = new PdfPCell(Longsqm);
                            //DOC Style Reduntant//pcelllongsqm.BorderColor = BaseColor.WHITE;
                            //DOC Style Reduntant//pcelllongsqm.HorizontalAlignment = Element.ALIGN_RIGHT;

                            //DOC Style Reduntant//PdfPCell LongsqquesSTMT = new PdfPCell(Longsadder);
                            //DOC Style Reduntant//LongsqquesSTMT.Colspan = 2;
                            //DOC Style Reduntant//LongsqquesSTMT.BorderColor = BaseColor.WHITE;
                            //DOC Style Reduntant//LongsqquesSTMT.Padding = 0;


                            //DOC Style Reduntant//PdfPCell Longsqkhali = new PdfPCell();
                            //DOC Style Reduntant//Longsqkhali.BorderColor = BaseColor.WHITE;


                            //DOC Style Reduntant//Longsqlongquestion.AddCell(LongsqquesSTMT);
                            //DOC Style Reduntant//Longsqlongquestion.AddCell(Longsqkhali);
                            //DOC Style Reduntant//Longsqlongquestion.AddCell(pcelllongsqm);

                            //////ADD diagram to long question table - complex model
                            if (false)
                            {
                                //////adding link to the full size image - cleaner method
                                //DOC Style Reduntant//Chunk chu = new Chunk("See full size", diagramLinks);
                                //DOC Style Reduntant//chu.SetAnchor(diagramifany);
                                //DOC Style Reduntant//Phrase p = new Phrase(chu);
                                string doc_chu = "See full size";
                                //DOC Style Reduntant//PdfPCell dcell = new PdfPCell(p);
                                //DOC Style Reduntant//dcell.Colspan = 2;
                                //DOC Style Reduntant//dcell.HorizontalAlignment = Element.ALIGN_CENTER;
                                //DOC Style Reduntant//dcell.BorderColor = BaseColor.WHITE;
                                /////adding link ends

                                /*
                                //Failed method
                                var DiagrLinking = new Chunk("View full size");
                                DiagrLinking.SetAnchor(diagramifany);
                                DiagramHolder.AddElement(DiagrLinking);
                                */

                                //DOC Style Reduntant//Longsqlongquestion.AddCell(DiagramHolder);
                                //DOC Style Reduntant//Longsqlongquestion.AddCell(dcell);

                            }

                            //////adding diagram to long question(complex) ends

                            //complex model ends

                            //DOC Style Reduntant//doc1.Add(Longsqlongquestion);
                        }
                        else
                        {

                            //simple model of long question of part B
                            //DOC Style Reduntant//PdfPTable slongshortques = new PdfPTable(2);
                            //DOC Style Reduntant//.WidthPercentage = 100;
                            //DOC Style Reduntant//slongshortques.SpacingAfter = 2;

                            //DOC Style Reduntant// slongquestionNumbering = new Phrase();

                            //DOC Style Reduntant//float[] slongwidths = new float[] { 8.8f, 1.2f };
                            //DOC Style Reduntant//.SetWidths(slongwidths);
                            /*
                            if (question["Part"].ToString() == "a" || question["Part"].ToString() == "")
                                slongquestionNumbering.Add(new Phrase("Q" + question["Number"] + " ", boldquestionfont));
                            slongquestionNumbering.Add(new Phrase("" + question["Part"] + ") ", boldquestionfont));
                            if (question["SubPart"].ToString() != "")
                                slongquestionNumbering.Add(new Phrase("(" + question["SubPart"] + ") ", boldquestionfont));
                            */

                            //thon

                            /*
                            if (question["Part"].ToString() == "a" && question["SubPart"].ToString() == "i")
                                slongquestionNumbering.Add(new Phrase("Q" + question["Number"] + " ", boldquestionfont));
                            else if (question["Part"].ToString() == "a" && question["SubPart"].ToString() != "i")
                                slongquestionNumbering.Add(new Phrase("(" + question["SubPart"] + ") ", boldquestionfont));
                            else if (question["Part"].ToString() == "a" || question["Part"].ToString() == "")
                                slongquestionNumbering.Add(new Phrase("Q" + question["Number"] + " ", boldquestionfont));
                            else
                            {
                                  if (question["SubPart"].ToString() == "i" || question["SubPart"].ToString() == "")
                                       slongquestionNumbering.Add(new Phrase("" + question["Part"] + ") ", boldquestionfont));
                                 if (question["SubPart"].ToString() != "")
                                    slongquestionNumbering.Add(new Phrase("(" + question["SubPart"] + ") ", boldquestionfont));
                            }
                            */
                            //DOC Style Reduntant//Phrase slongsqst = new Phrase(question["Statement"].ToString(), questionfont);
                            string doc_slongsqst = question["Statement"].ToString();

                            //DOC Style Reduntant//Phrase slongsadder = new Phrase();
                            //slongsadder.Add(slongquestionNumbering);
                            //DOC Style Reduntant//slongsadder.Add(giveNumbering(question, slongquestionNumbering));
                            //DOC Style Reduntant//slongsadder.Add(slongsqst);

                            string marksstring = question["Weightage"].ToString().Substring(0, question["Weightage"].ToString().Length - 1);
                            if (marksstring.ElementAt<char>(marksstring.Length - 1) == '0')
                                marksstring = marksstring.Substring(0, marksstring.Length - 2);


                            //DOC Style Reduntant//Phrase slongsqm = new Phrase("[ " + marksstring + " Marks ]", questionfont);
                            //DOC Style Reduntant//slongsqm.SetLeading(0, 0);
                            string doc_slongsqm = "[ " + marksstring + " Marks ]";

                            //DOC Style Reduntant//PdfPCell slongsqstmt = new PdfPCell(slongsadder);
                            //DOC Style Reduntant//slongsqstmt.BorderColor = BaseColor.WHITE;
                            //DOC Style Reduntant//slongsqstmt.Padding = 0;

                            //DOC Style Reduntant//PdfPCell slongsmarks = new PdfPCell(slongsqm);
                            //DOC Style Reduntant//slongsmarks.BorderColor = BaseColor.WHITE;
                            //DOC Style Reduntant//slongsmarks.HorizontalAlignment = Element.ALIGN_RIGHT;
                            //DOC Style Reduntant//slongsmarks.Padding = 0;

                            //DOC Style Reduntant//slongshortques.AddCell(slongsqstmt);
                            //DOC Style Reduntant//slongshortques.AddCell(slongsmarks);


                            //////ADD diagram to long question table - simple model
                            if (false)
                            {
                                //////adding link to the full size image - cleaner method
                                //DOC Style Reduntant//Chunk chu = new Chunk("See full size", diagramLinks);
                                string doc_chu = "See full size";
                                //DOC Style Reduntant//chu.SetAnchor(diagramifany);
                                //DOC Style Reduntant//Phrase p = new Phrase(chu);

                                //DOC Style Reduntant//PdfPCell dcell = new PdfPCell(p);
                                //DOC Style Reduntant//dcell.Colspan = 2;
                                //DOC Style Reduntant//dcell.HorizontalAlignment = Element.ALIGN_CENTER;
                                //DOC Style Reduntant//dcell.BorderColor = BaseColor.WHITE;
                                /////adding link ends

                                //DOC Style Reduntant//slongshortques.AddCell(DiagramHolder);
                                //DOC Style Reduntant//slongshortques.AddCell(dcell);

                            }

                            //////adding diagram to long question(simple) ends

                            //simple model ends


                            //DOC Style Reduntant//doc1.Add(slongshortques);

                        }


                        RowsAboveThisQuestion.ImportRow(question);

                    }//long questions loop ends

                    //DOC Style Reduntant//doc1.Add(endMark);
                    //DOC Style Reduntant//doc1.Close();
                    //DOC Style Reduntant//AddPageNumber(Server.MapPath("PDFs") + "/Doc1.pdf");

                    //DOC Style Reduntant//paperlink.Text = "Download PDF here: ";
                    //DOC Style Reduntant//staticlink.Visible = true;
                    //DOC Style Reduntant//lbl_temp.Text = "Done";
                    //DOC Style Reduntant//lbl_temp.Visible = true;

                }
                else if (pattern == "ob-lo")
                {
                    InstFOob.Visible = true;
                    InstFoObonly.Visible = false;
                    InstFOsholo.Visible = false;
                    //DOC Style Reduntant//doc1.Add(parta);
                    //hdoc_partHeaders.Text = doc_parta;
                    //  lbl_temp.Text = "Unable to generate PDF for Pattern \"ob-lo\" yet. Check back later.";
                    //lbl_temp.Visible = true;

                    //doc1.Close();
                    //////////////////////////generatePaperForType_oblo(PNR, no_partA, PaperQuestions, doc1);
                    SqlDataAdapter paperdata = new SqlDataAdapter("select * from questionpapersdump where pnr = '" + PNR + "' and number=1 order by cast(Part as int)", con);
                    //Sorting by (part as int) because it is observed that in oblo type questions where there are 20 mcqs the MCQs are numbred as 1,2,3 instead of a,b,c,d....
                    DataTable PaperQuestions1 = new DataTable();
                    paperdata.Fill(PaperQuestions1);

                    renderOBLOs(PaperQuestions1, no_partA, no_partB, PNR);
                }
                else if (pattern == "ob")
                {
                    //lbl_temp.Text = "Unable to generate PDF for Pattern \"ob\" yet. Check back later.";
                    //lbl_temp.Visible = true;
                    lbl_papercode.Text = PaperSet.ToUpper();
                    hdoc_PaperCode.Visible = true;

                  //  InstFOob.Visible = false;
                    //InstFOsholo.Visible = false;
                    //InstFoObonly.Visible = true;

                    renderOnlyOBs(PNR);
                    //DOC Style Reduntant//doc1.Close();
                    //////////////////////generatePaperForType_ob(PNR);
                }
            }
            else if (PaperType(PNR) == "CA")
            {
                //Genrate CA paper
                con.Close();

                pnl_CaPaper.Visible = true;
                pnl_Paper.Visible = false;
                displayCAPaper(PNR);
            }

        }

        private string giveNumbering(DataRow question)
        {
            if (question["SubPart"].ToString() == "i")
            {
                if (question["Part"].ToString() != "a")
                    return question["Part"].ToString() + ") (" + question["SubPart"].ToString() + ") ";
                //DOC Style Reduntant//return new Phrase(question["Part"].ToString() + ") (" + question["SubPart"].ToString() + ") ", boldquestionfont);

                else
                    //DOC Style Reduntant//return new Phrase("Q" + question["Number"] + " " + question["Part"].ToString() + ") (" + question["SubPart"].ToString() + ") ", boldquestionfont);
                    return "Q" + question["Number"] + " " + question["Part"].ToString() + ") (" + question["SubPart"].ToString() + ") ";
                //slongquestionNumbering.Add(new Phrase("Q" + question["Number"] + " " + question["Part"].ToString() + ") (" + question["SubPart"].ToString() + ")", boldquestionfont));
            }
            else
            {//subpart not i
                if (question["SubPart"].ToString() == "")
                {
                    //no subpart
                    if (question["Part"].ToString() == "a")
                    {
                        //slongquestionNumbering.Add(new Phrase("Q" + question["Number"] + " " + question["Part"].ToString() + ") ", boldquestionfont));
                        //DOC Style Reduntant//return new Phrase("Q" + question["Number"] + " " + question["Part"].ToString() + ") ", boldquestionfont);
                        return "Q" + question["Number"] + " " + question["Part"].ToString() + ") ";
                    }
                    else
                    {
                        //part not a
                        if (question["Part"].ToString() == "")
                        {
                            //no part
                            //slongquestionNumbering.Add(new Phrase("Q" + question["Number"] + " ", boldquestionfont));
                            //DOC Style Reduntant//return new Phrase("Q" + question["Number"] + " ", boldquestionfont);
                            return "Q" + question["Number"] + " ";
                        }
                        else
                        {
                            //part > a
                            //DOC Style Reduntant// new Phrase(question["Part"] + ") ", boldquestionfont);
                            return question["Part"] + ") ";
                            //slongquestionNumbering.Add(new Phrase(question["Part"] + ") ", boldquestionfont));
                        }
                    }
                }
                else
                {
                    //subpart > i
                    //DOC Style Reduntant//return new Phrase("(" + question["SubPart"] + ") ", boldquestionfont);
                    return "(" + question["SubPart"] + ") ";
                    //slongquestionNumbering.Add(new Phrase("(" + question["SubPart"] + ") ", boldquestionfont));
                }
            }
        }

        private bool isORPlacable(DataTable RowsAboveThisQuestion, string PNR, string Number, string Part)
        {
            if (Part.ToLower() == "a")
                return false;

            System.Diagnostics.Debug.WriteLine("OOO - " + RowsAboveThisQuestion.Select("PNR='" + PNR + "' AND Number=" + Number + " AND Part='" + Part + "'").Count());
            if (RowsAboveThisQuestion.Select("PNR='" + PNR + "' AND Number=" + Number + " AND Part='" + Part + "'").Count() == 0)
                return true;
            else
                return false;
        }

        //renders short questions in the "sho-lo" category
        private void renderShort(string PNR, int nPartA, int nPartB)
        {
            addPart("A");

            DataTable dt_questions = fetchAllQuestions(PNR);

            Panel sep = new Panel();
            sep.CssClass = "Spreader";

            for (int c = 0; c < nPartA; c++)
            {
                DataRow question = dt_questions.Rows[c];
                Panel stmtholder = new Panel();
                stmtholder.CssClass = "qholder";

                Label MCQstmt_num = new Label { Text = giveNumbering(question) + "&nbsp;" };
                Label MCQstmt_stmt = new Label { Text = Server.HtmlEncode(question["Statement"].ToString()) };

                string MarksString = formatMarks(question["Weightage"].ToString());
                if (MarksString.Contains("."))
                    if (MarksString.Substring(MarksString.IndexOf(".") + 1).Contains("0"))
                        MarksString = MarksString.Substring(0, MarksString.Length - 1);

                Label MCQstmt_Marks = new Label { Text = "[ " + MarksString + " Marks ]" };

                MCQstmt_num.CssClass = "StmtNum";
                MCQstmt_stmt.CssClass = "StmtStmt";
                MCQstmt_Marks.CssClass = "MarksAligner";

                stmtholder.Controls.Add(MCQstmt_num);
                stmtholder.Controls.Add(MCQstmt_stmt);
                stmtholder.Controls.Add(MCQstmt_Marks);
                /////ends stmt adder

                if (giveNumbering(question).First<char>() == 'Q')
                {
                    Panel QuestionsSeparator = new Panel();
                    QuestionsSeparator.CssClass = "qholder QuestionsSeparator";
                    pnl_QuestionsHolder.Controls.Add(QuestionsSeparator);
                }

                pnl_QuestionsHolder.Controls.Add(stmtholder);

                Panel qholder = new Panel();
                qholder.CssClass = "qholder";

                //diagrams
                if (question["Diagram"].ToString() != "")
                {
                    string url = "Diagrams" + "/" + giveImageName(question["Diagram"].ToString());
                    Image DiagramImage = new Image { ImageUrl = url };

                    DiagramImage.ImageUrl = "Diagrams" + "/" + giveImageName(question["Diagram"].ToString());
                    DiagramImage.CssClass = "DiagramImage";
                    Panel qholder1 = new Panel();
                    qholder.CssClass = "qholder";

                    HyperLink WrapperLink = new HyperLink();
                    WrapperLink.Target = "_blank";
                    WrapperLink.NavigateUrl = DiagramImage.ImageUrl;

                    WrapperLink.Controls.Add(DiagramImage);

                    qholder1.Controls.Add(WrapperLink);

                    qholder.Controls.Add(qholder1);
                }
                pnl_QuestionsHolder.Controls.Add(qholder);

                pnl_QuestionsHolder.Controls.Add(sep);
            }
            renderLong(dt_questions, PNR, nPartA, nPartB);
        }

        //Generic function to render long questions in all the categories ("sho-lo","ob-lo")
        private void renderLong(DataTable dt_questions, string PNR, int nPartA, int nPartB)
        {
            addPart("B");

            Panel sep = new Panel();
            sep.CssClass = "Spreader";

            pnl_QuestionsHolder.Controls.Add(sep);

            DataTable RowsAboveThisQuestion = DataCollection.AddQuestions.createLocalTable("Oh yeah");

            for (int c = nPartA; c < dt_questions.Rows.Count; c++)
            {
                DataRow question = dt_questions.Rows[c];

                if (isORPlacable(RowsAboveThisQuestion, question["PNR"].ToString(), question["Number"].ToString(), question["Part"].ToString()))
                    addOR();
                ///statement adder
                Panel stmtholder = new Panel();
                stmtholder.CssClass = "qholder";

                Label MCQstmt_num = new Label { Text = giveNumbering(question) + "&nbsp;" };
                Label MCQstmt_stmt = new Label { Text = Server.HtmlEncode(question["Statement"].ToString()) };
                Label MCQstmt_Marks = new Label { Text = "[ " + formatMarks(question["Weightage"].ToString()) + " Marks ]" };

                MCQstmt_num.CssClass = "StmtNum";
                MCQstmt_stmt.CssClass = "StmtStmt";
                MCQstmt_Marks.CssClass = "MarksAligner";

                stmtholder.Controls.Add(MCQstmt_num);
                stmtholder.Controls.Add(MCQstmt_stmt);
                stmtholder.Controls.Add(MCQstmt_Marks);
                /////ends stmt adder

                if (giveNumbering(question).First<char>() == 'Q')
                {
                    Panel QuestionsSeparator = new Panel();
                    QuestionsSeparator.CssClass = "qholder QuestionsSeparator";
                    pnl_QuestionsHolder.Controls.Add(QuestionsSeparator);
                }

                pnl_QuestionsHolder.Controls.Add(stmtholder);

                Panel qholder = new Panel();
                qholder.CssClass = "qholder";

                //diagrams
                if (question["Diagram"].ToString() != "")
                {
                    Debug.WriteLine("Enteres in to th sits vous plait");
                    Debug.WriteLine("Enteres in to th sits vous plait");
                    Debug.WriteLine("Enteres in to th sits vous plait");
                    string url = "Diagrams" + "/" + giveImageName(question["Diagram"].ToString());
                    Image DiagramImage = new Image { ImageUrl = url };

                    DiagramImage.ImageUrl = "Diagrams" + "/" + giveImageName(question["Diagram"].ToString());
                    DiagramImage.CssClass = "DiagramImage";
                    //Panel qholder1 = new Panel();
                    //qholder.CssClass = "qholder";

                    HyperLink WrapperLink = new HyperLink();
                    WrapperLink.Target = "_blank";
                    WrapperLink.NavigateUrl = DiagramImage.ImageUrl;

                    WrapperLink.Controls.Add(DiagramImage);

                    qholder.Controls.Add(WrapperLink);
                }
                pnl_QuestionsHolder.Controls.Add(qholder);

                RowsAboveThisQuestion.ImportRow(question);
            }
            addEndMark();
        }

        //Generic function to render a single MCQ question
        private void renderMCQ(DataRow question)
        {
            Panel sep = new Panel();
            sep.CssClass = "Spreader Spacemaker";

            Panel qholder = new Panel();
            qholder.CssClass = "qholder";

            Panel stmtholder = new Panel();
            stmtholder.CssClass = "qholder";

            Label MCQstmt_num = new Label { Text = giveNumbering(question) + "&nbsp;" };
            Label MCQstmt_stmt = new Label { Text = question["Statement"].ToString() };

            MCQstmt_num.CssClass = "StmtNum";
            MCQstmt_stmt.CssClass = "StmtStmt";

            stmtholder.Controls.Add(MCQstmt_num);
            stmtholder.Controls.Add(MCQstmt_stmt);

            //Label MCQstmt = new Label { Text = giveNumbering(question) + question["Statement"].ToString() };
            //MCQstmt.CssClass = "QuestionStatement";
            Label MCQoption1 = new Label { Text = "(a) " + question["Option1"].ToString() };
            Label MCQoption2 = new Label { Text = "(b) " + question["Option2"].ToString() };
            Label MCQoption3 = new Label { Text = "(c) " + question["Option3"].ToString() };
            Label MCQoption4 = new Label { Text = "(d) " + question["Option4"].ToString() };

            MCQoption1.CssClass = "MCQoption";
            MCQoption2.CssClass = "MCQoption";
            MCQoption3.CssClass = "MCQoption";
            MCQoption4.CssClass = "MCQoption";

            Image DiagramImage = new Image();
            Image Option1File = new Image();
            Image Option2File = new Image();
            Image Option3File = new Image();
            Image Option4File = new Image();


            qholder.Controls.Add(stmtholder);

            //diagrams
            if (question["Diagram"].ToString() != "")
            {
                DiagramImage.ImageUrl = "Diagrams" + "/" + giveImageName(question["Diagram"].ToString());
                DiagramImage.CssClass = "DiagramImage";
                DiagramImage.AlternateText = "Could not load Image, Please report this issue using dash form.";

                Panel qholder1 = new Panel();
                qholder.CssClass = "qholder";

                HyperLink WrapperLink = new HyperLink();
                WrapperLink.NavigateUrl = "Diagrams" + "/" + giveImageName(question["Diagram"].ToString());
                WrapperLink.Controls.Add(DiagramImage);
                WrapperLink.Target = "_blank";
                WrapperLink.ToolTip = "Click to view full size";

                qholder1.Controls.Add(WrapperLink);

                qholder.Controls.Add(qholder1);
            }

            qholder.Controls.Add(MCQoption1);
            if (question["Option1File"].ToString() != "")
            {
                Option1File.ImageUrl = "Diagrams" + "/" + giveImageName(question["Option1File"].ToString());
                Option1File.CssClass = "DiagramImage";
                Option1File.AlternateText = "Could not load Image, Please report this issue using dash form.";

                Panel opt1holder = new Panel();
                opt1holder.CssClass = "qholder";

                HyperLink WrapperLink = new HyperLink();
                WrapperLink.NavigateUrl = "Diagrams" + "/" + giveImageName(question["Option1File"].ToString());
                WrapperLink.Controls.Add(Option1File);
                WrapperLink.Target = "_blank";
                WrapperLink.ToolTip = "Click to view full size";

                opt1holder.Controls.Add(WrapperLink);

                qholder.Controls.Add(opt1holder);
            }

            qholder.Controls.Add(MCQoption2);
            if (question["Option2File"].ToString() != "")
            {
                Option2File.ImageUrl = "Diagrams" + "/" + giveImageName(question["Option2File"].ToString());
                Option2File.CssClass = "DiagramImage";
                Option2File.AlternateText = "Could not load Image, Please report this issue using dash form.";

                Panel opt1holder = new Panel();
                opt1holder.CssClass = "qholder";

                HyperLink WrapperLink = new HyperLink();
                WrapperLink.NavigateUrl = "Diagrams" + "/" + giveImageName(question["Option2File"].ToString());
                WrapperLink.Controls.Add(Option2File);
                WrapperLink.Target = "_blank";
                WrapperLink.ToolTip = "Click to view full size";

                opt1holder.Controls.Add(WrapperLink);

                qholder.Controls.Add(opt1holder);
            }

            qholder.Controls.Add(MCQoption3);
            if (question["Option3File"].ToString() != "")
            {
                Option3File.ImageUrl = "Diagrams" + "/" + giveImageName(question["Option3File"].ToString());
                Option3File.CssClass = "DiagramImage";
                Option3File.AlternateText = "Could not load Image, Please report this issue using dash form.";

                Panel opt1holder = new Panel();
                opt1holder.CssClass = "qholder";

                HyperLink WrapperLink = new HyperLink();
                WrapperLink.NavigateUrl = "Diagrams" + "/" + giveImageName(question["Option3File"].ToString());
                WrapperLink.Controls.Add(Option3File);
                WrapperLink.Target = "_blank";
                WrapperLink.ToolTip = "Click to view full size";

                opt1holder.Controls.Add(WrapperLink);

                qholder.Controls.Add(opt1holder);
            }

            qholder.Controls.Add(MCQoption4);
            if (question["Option4File"].ToString() != "")
            {
                Option4File.ImageUrl = "Diagrams" + "/" + giveImageName(question["Option4File"].ToString());
                Option4File.CssClass = "DiagramImage";
                Option4File.AlternateText = "Could not load Image, Please report this issue using dash form.";

                Panel opt1holder = new Panel();
                opt1holder.CssClass = "qholder";

                HyperLink WrapperLink = new HyperLink();
                WrapperLink.NavigateUrl = "Diagrams" + "/" + giveImageName(question["Option4File"].ToString());
                WrapperLink.Target = "_blank";
                WrapperLink.ToolTip = "Click to view full size";

                WrapperLink.Controls.Add(Option4File);

                opt1holder.Controls.Add(WrapperLink);

                qholder.Controls.Add(opt1holder);
            }


            pnl_QuestionsHolder.Controls.Add(qholder);
            pnl_QuestionsHolder.Controls.Add(sep);
        }

        private void renderOBLOs(DataTable dt_question, int nPartA, int nPartB, string PNR)
        {

            addPart("A");
            for (int c = 0; c < nPartA; c++)
            {
                DataRow question = dt_question.Rows[c];
                renderMCQ(question);
            }
            dt_question = fetchAllQuestions(PNR);
            renderLong(dt_question, PNR, nPartA, nPartB);
        }

        private void renderOnlyOBs(string PNR)
        {
            SqlConnection ccon = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString);

            DataTable dt_questions = new DataTable();
            SqlDataAdapter ada = new SqlDataAdapter("Select * from questionpapersdump where pnr='" + PNR + "' order by Number", ccon);
            ccon.Open();
            ada.Fill(dt_questions);

            SqlCommand obfetcher = new SqlCommand();
            obfetcher.Connection = ccon;
            obfetcher.CommandText = "Select count(*) from questionpapersdump where PNR= '" + PNR + "'";
            string obQcount = obfetcher.ExecuteScalar().ToString();
            obfetcher.CommandText = "Select avg(Weightage) from questionpapersdump where PNR= '" + PNR + "'";
            string obQwt = obfetcher.ExecuteScalar().ToString();
            ccon.Close();
            obQwt = obQwt.Substring(0,obQwt.IndexOf("."));

            if (obQwt == "1")
                obQwt += " mark ";
            else
                obQwt += " marks ";

            lbl_QcountOB.Text = obQcount;
            lbl_QwtOB.Text = obQwt;
            InstFoObonly.Visible = true;
            InstFOob.Visible = false;
            InstFOsholo.Visible = false;

            for (int c = 0; c < dt_questions.Rows.Count; c++)
                renderMCQ(dt_questions.Rows[c]);
            addEndMark();
        }

        private DataTable fetchAllQuestions(string PNR)
        {
            using (SqlConnection ccon = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                SqlDataAdapter ada = new SqlDataAdapter("Select * from questionpapersdump where pnr='" + PNR + "' order by QuestionID", ccon);
                DataTable returner = new DataTable();
                ccon.Open();
                ada.Fill(returner);
                ccon.Close();
                return returner;
            }
        }

        private void displayCAPaper(string PNR)
        {
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString);
            SqlCommand CAFetcher = new SqlCommand();
            CAFetcher.Connection = con;
            con.Open();

            CAFetcher.CommandText = "Select CourseCode from CAPapers where pnr = '" + PNR + "'";
            string ccode = CAFetcher.ExecuteScalar().ToString();

            CAFetcher.CommandText = "Select CourseName from CAPapers where pnr = '" + PNR + "'";
            string cname = CAFetcher.ExecuteScalar().ToString();

            CAFetcher.CommandText = "Select CAnumber from CAPapers where pnr = '" + PNR + "'";
            int canum = Convert.ToInt32(CAFetcher.ExecuteScalar().ToString());

            CAFetcher.CommandText = "Select TimeAllowed from CAPapers where pnr = '" + PNR + "'";
            int TimeAllowed = Convert.ToInt32(CAFetcher.ExecuteScalar().ToString());

            CAFetcher.CommandText = "Select MaxMarks from CAPapers where pnr = '" + PNR + "'";
            string maxmarks = CAFetcher.ExecuteScalar().ToString();

            CAFetcher.CommandText = "Select PaperSet from CAPapers where pnr = '" + PNR + "'";
            string paperset = CAFetcher.ExecuteScalar().ToString().ToUpper();

            ////Displaying top details
            Label TopHeader = new Label();
            TopHeader.CssClass = "TopHeader CAnumber";
            TopHeader.ID = "CAnumber";

            TopHeader.Text = canum==999?"End-Term Practical Evaluation":canum==998? "Mid-Term Practical Evaluation":"Continuous Assessment";

            System.Diagnostics.Debug.WriteLine("paper set: " + paperset);

            Label capaperset = new Label { Text = "[ Set " + paperset + " ]" };
            capaperset.CssClass = "caPaperSet";

            if (paperset != " ")
                pnl_CaPaper.Controls.Add(capaperset);

            if (canum != 0&&canum<20)
                TopHeader.Text += " - " + canum;
            pnl_CaPaper.Controls.Add(TopHeader);

            Label CourseDetails = new Label { Text = ccode };
            CourseDetails.CssClass = "TopHeader";
            if (cname != "")
                CourseDetails.Text += " : " + cname;
            pnl_CaPaper.Controls.Add(CourseDetails);

            Label maxmrkz = new Label { Text = "Max.Marks: " + maxmarks };
            maxmrkz.CssClass = "paperdetails lefter";

            Label timeAllowed = new Label { Text = "Time Allowed: " + formatTime(TimeAllowed) };
            timeAllowed.CssClass = "paperdetails righter";

            pnl_CaPaper.Controls.Add(maxmrkz);
            pnl_CaPaper.Controls.Add(timeAllowed);

            //END//Displaying top details


            DataTable CAQuestions = new DataTable();
            SqlDataAdapter ad = new SqlDataAdapter("select * from questionpapersdump where pnr='" + PNR + "' order by Number", con);
            ad.Fill(CAQuestions);

            Panel SpaceMaker = new Panel();
            SpaceMaker.CssClass = "Spacemaker";

            for (int c = 0; c < CAQuestions.Rows.Count; c++)
            {
                DataRow question = CAQuestions.Rows[c];

                Panel stmtholder = new Panel();
                stmtholder.CssClass = "qholder";

                Label MCQstmt_num = new Label { Text = giveNumbering(question) + "&nbsp;" };
                Label MCQstmt_stmt = new Label { Text = question["Statement"].ToString() };
                Label MCQstmt_Marks = new Label { Text = "[" + formatMarks(question["Weightage"].ToString()) + "]" };

                MCQstmt_num.CssClass = "StmtNum";
                MCQstmt_stmt.CssClass = "StmtStmt";
                MCQstmt_Marks.CssClass = "MarksAligner";

                stmtholder.Controls.Add(MCQstmt_num);
                stmtholder.Controls.Add(MCQstmt_stmt);
                stmtholder.Controls.Add(MCQstmt_Marks);
                /////ends stmt adder

                if (giveNumbering(question).First<char>() == 'Q')
                {
                    Panel QuestionsSeparator = new Panel();
                    QuestionsSeparator.CssClass = "qholder QuestionsSeparator";
                    pnl_CaPaper.Controls.Add(QuestionsSeparator);
                }

                pnl_CaPaper.Controls.Add(stmtholder);

                Panel qholder = new Panel();
                qholder.CssClass = "qholder";

                //diagrams
                if (question["Diagram"].ToString() != "")
                {
                    string url = "Diagrams" + "/" + giveImageName(question["Diagram"].ToString());
                    Image DiagramImage = new Image { ImageUrl = url };

                    DiagramImage.ImageUrl = "Diagrams" + "/" + giveImageName(question["Diagram"].ToString());
                    DiagramImage.CssClass = "DiagramImage";
                    Panel qholder1 = new Panel();
                    qholder.CssClass = "qholder";

                    HyperLink WrapperLink = new HyperLink();
                    WrapperLink.Target = "_blank";
                    WrapperLink.NavigateUrl = DiagramImage.ImageUrl;

                    WrapperLink.Controls.Add(DiagramImage);

                    qholder1.Controls.Add(WrapperLink);

                    qholder.Controls.Add(qholder1);
                }
                pnl_CaPaper.Controls.Add(qholder);
                pnl_CaPaper.Controls.Add(SpaceMaker);

            }

        }

        private void addPart(string part)
        {
            Panel sep = new Panel();
            sep.CssClass = "Spreader Spacemaker";

            Label partheader = new Label();
            partheader.Text = "PART " + part;
            partheader.CssClass = "PartHeaders";
            pnl_QuestionsHolder.Controls.Add(partheader);
            pnl_QuestionsHolder.Controls.Add(sep);

        }

        private void addEndMark()
        {
            Label endMark = new Label { Text = "-- End of Question Paper --" };
            endMark.CssClass = "Endmark";
            pnl_QuestionsHolder.Controls.Add(endMark);
        }

        private void addOR()
        {
            Label or = new Label();
            or.Text = "OR";
            or.CssClass = "OrClass";

            pnl_QuestionsHolder.Controls.Add(or);
        }
    }
}