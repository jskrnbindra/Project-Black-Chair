using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Web.UI.WebControls;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace DataCollection
{
    public partial class PDFtester : System.Web.UI.Page
    {
        //Public Content
        static Font Topsmall = FontFactory.GetFont("Helvetica", 9, Font.BOLD, BaseColor.BLACK);
        Font small = FontFactory.GetFont("Helvetica", 9);
        Font TopHeader = FontFactory.GetFont("Helvetica", 12, Font.BOLD, BaseColor.BLACK);
        Font fnt_inst = FontFactory.GetFont("Helvetica", 9, Font.ITALIC);
        Font questionfont = FontFactory.GetFont("Helvetica", 10);
        Font boldquestionfont = FontFactory.GetFont("Helvetica", 10, Font.BOLD, BaseColor.BLACK);
        Font footerfont = FontFactory.GetFont("Helvetica", 9, Font.ITALIC, BaseColor.BLACK);
        Font diagramLinks = FontFactory.GetFont("Helvetica", 8, Font.UNDERLINE, BaseColor.BLUE);
        Font papercodebold = FontFactory.GetFont("Helvetica", 17, Font.BOLD, BaseColor.BLACK);

        static SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString);

        Paragraph NewLine_s = new Paragraph("\n", Topsmall);
        //Public content ends

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

        public void AddPageNumber(string pdfpath)
        {
            byte[] bytes = File.ReadAllBytes(pdfpath);
            Font blackFont = FontFactory.GetFont("Helvetica", 12, Font.NORMAL, BaseColor.BLACK);
            using (MemoryStream stream = new MemoryStream())
            {
                PdfReader reader = new PdfReader(bytes);
                using (PdfStamper stamper = new PdfStamper(reader, stream))
                {
                    int pages = reader.NumberOfPages;
                    string temp = "";
                    for (int i = 1; i <= pages; i++)
                    {
                        temp = String.Format("Page {0} of ", i); temp += pages.ToString();
                        ColumnText.ShowTextAligned(stamper.GetUnderContent(i), Element.ALIGN_RIGHT, new Phrase(temp, blackFont), 578f, 35f, 0);
                    }
                }
                bytes = stream.ToArray();
            }
            File.WriteAllBytes(pdfpath, bytes);
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
           // GeneratePDF();
            lbl_temp.Text = "Done!";
            lbl_temp.Visible = true;
        }

        public string PaperType(string PNR)
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

        public void GeneratePDFwithPNR(string PNR)
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

                float wt_partB;
                //if (!Convert.IsDBNull(Fetch_PartBquesWeightage.ExecuteScalar()))
                //{
                wt_partB = (float)Convert.ToInt32(Fetch_PartBquesWeightage.ExecuteScalar()) / 2;
                //}
                //else
                //{
                //    wt_partB = 0f;
                //}
                //Top details end

                //Generic stuff
                var doc1 = new Document(PageSize.A4, 52, 52, 60, 50);
                string path = Server.MapPath("PDFs");
                PdfWriter.GetInstance(doc1, new FileStream(path + "/Doc1.pdf", FileMode.Create));
                //int j = TextBox1.Text == "" ? 8 : Convert.ToInt32(TextBox1.Text);

                doc1.Open();

                Paragraph PaperCode = new Paragraph("Paper Code: " + PaperSet.ToUpper(), papercodebold);
                PaperCode.Alignment = Element.ALIGN_RIGHT;

                Paragraph rno = new Paragraph("Registration No:.__________________\n", Topsmall);
                Paragraph cdetails = new Paragraph("COURSE CODE : " + Ccode + "\nCOURSE NAME : " + Corasname + "\n", TopHeader);
                Paragraph pnr = new Paragraph("PNR No:: " + PNR + "\n", Topsmall);
                PdfPTable paperdetails = new PdfPTable(2);
                paperdetails.SpacingBefore = 5;

                PdfPCell ta = new PdfPCell(new Phrase("Time Allowed: " + formatTime(TimeAllowed), Topsmall));
                PdfPCell mm = new PdfPCell(new Phrase("Max.Marks: " + maxmarks, Topsmall));

                Paragraph NewLine_s = new Paragraph("\n", Topsmall);

                Paragraph instinst = new Paragraph("Read the following instructions carefully before attempting the question paper.", fnt_inst);

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

                Paragraph parta = new Paragraph("\nPART A\n");
                parta.Alignment = Element.ALIGN_CENTER;
                parta.Font.SetStyle(Font.BOLDITALIC);
                parta.Font.Size = 11;
                Paragraph partb = new Paragraph("\nPART B\n");
                partb.Alignment = Element.ALIGN_CENTER;
                partb.Font.SetStyle(Font.BOLDITALIC);
                partb.Font.Size = 11;

                Paragraph or = new Paragraph("OR\n");
                or.Font.SetStyle(Font.BOLDITALIC);
                or.Alignment = Element.ALIGN_CENTER;
                or.Font.Size = 11;
                or.SpacingAfter = 2;

                Paragraph endMark = new Paragraph("\n-- End of Question Paper --", footerfont);
                endMark.Alignment = Element.ALIGN_CENTER;

                // if(PaperSet!="")
                // doc1.Add(PaperCode);
                if (pattern == "ob-lo")
                    doc1.Add(PaperCode);
                doc1.Add(rno);
                doc1.Add(pnr);
                doc1.Add(cdetails);
                doc1.Add(paperdetails);
                doc1.Add(instinst);
                doc1.Add(instructions);

                //Generic stuff ends
                DataTable PaperQuestions = new DataTable("PaperQuestions");

                if (pattern == "sh-lo")//later generatePDF(String pnr,string pattern);
                {
                    doc1.Add(parta);

                    SqlDataAdapter paperdata = new SqlDataAdapter("Select * from questionpapersdump where pnr='" + PNR + "' order by QuestionID", con);
                    paperdata.Fill(PaperQuestions);

                    //Rendering Short questions PART A
                    //if < 80 chars
                    ///////////short question - simple model
                    for (int c = 0; c < no_partA; c++)
                    {
                        PdfPTable shortques = new PdfPTable(2);
                        shortques.WidthPercentage = 100;
                        shortques.SpacingAfter = 2;

                        float[] widths = new float[] { 8.8f, 1.2f };
                        shortques.SetWidths(widths);

                        PdfPTable sqlongquestion = new PdfPTable(2);
                        sqlongquestion.WidthPercentage = 100;


                        Phrase questionNumbering = new Phrase();

                        DataRow question = PaperQuestions.Rows[c];

                        /////for diagram laden questions 
                        string diagramifany = question["Diagram"].ToString();
                        if (diagramifany != "")
                            diagramifany = diagramifany.Substring(diagramifany.IndexOf("blackchair.manhardeep.com"));
                        PdfPCell DiagramHolder = new PdfPCell();
                        Anchor diagramlink = new Anchor();
                        if (diagramifany != "")
                        {
                            iTextSharp.text.Image diagr = iTextSharp.text.Image.GetInstance(Server.MapPath("Diagrams") + "/" + giveImageName(question["Diagram"].ToString()));
                            //iTextSharp.text.Image diagr = iTextSharp.text.Image.GetInstance(Server.MapPath("Diagrams") + "/" + "IMG_4015.JPG");
                            
                            diagr.ScaleToFit(new Rectangle(0f, 0f, 300f, 300f));
                            DiagramHolder = new PdfPCell(diagr);
                            DiagramHolder.BorderColor = BaseColor.WHITE;
                            DiagramHolder.Colspan = 2;
                            DiagramHolder.HorizontalAlignment = Element.ALIGN_CENTER;
                            DiagramHolder.PaddingTop = 5;

                            diagramlink = new Anchor("View Full size", diagramLinks);
                            diagramlink.Reference = diagramifany;
                        }
                        //////Diagram code ends

                        /*
                        if (question["Part"].ToString() == "a" || question["Part"].ToString() == "")
                            questionNumbering.Add(new Phrase("Q" + question["Number"] + " ", boldquestionfont));
                        questionNumbering.Add(new Phrase(question["Part"] + ") ", boldquestionfont));
                        if (question["SubPart"].ToString() != "")
                            questionNumbering.Add(new Phrase("(" + question["SubPart"] + ") ", boldquestionfont));
                        */

                        if (question["Part"].ToString() == "a" || question["Part"].ToString() == "")
                            questionNumbering.Add(new Phrase("Q" + question["Number"] + " ", boldquestionfont));
                        //if (question["SubPart"].ToString() == "i" || question["SubPart"].ToString() == "")
                        if ((question["SubPart"].ToString() == "i" || question["SubPart"].ToString() == "")&&question["Part"].ToString()!="")
                            questionNumbering.Add(new Phrase(question["Part"] + ") ", boldquestionfont));
                        if (question["SubPart"].ToString() != "")
                            questionNumbering.Add(new Phrase("(" + question["SubPart"] + ") ", boldquestionfont));

                        Phrase sqst = new Phrase(question["Statement"].ToString(), questionfont);

                        Phrase sadder = new Phrase();
                        sadder.Add(questionNumbering);
                        sadder.Add(sqst);

                        string marksstring = question["Weightage"].ToString().Substring(0, question["Weightage"].ToString().Length - 1);
                        if (marksstring.ElementAt<char>(marksstring.Length - 1) == '0')
                            marksstring = marksstring.Substring(0, marksstring.Length - 2);

                        Phrase sqm = new Phrase("[ " + marksstring + " Marks ]", questionfont);
                        sqm.SetLeading(0, 0);

                        PdfPCell sqstmt = new PdfPCell(sadder);
                        sqstmt.BorderColor = BaseColor.WHITE;
                        sqstmt.Padding = 0;

                        PdfPCell smarks = new PdfPCell(sqm);
                        smarks.BorderColor = BaseColor.WHITE;
                        smarks.HorizontalAlignment = Element.ALIGN_RIGHT;
                        smarks.Padding = 0;

                        shortques.AddCell(sqstmt);
                        shortques.AddCell(smarks);
                        /////[ENDS]//////short question - simple model 
                        //////////short question - complex model
                        PdfPCell sqquesSTMT = new PdfPCell(sadder);
                        sqquesSTMT.Colspan = 2;
                        sqquesSTMT.BorderColor = BaseColor.WHITE;
                        sqquesSTMT.Padding = 0;

                        PdfPCell sqkhali = new PdfPCell();
                        sqkhali.BorderColor = BaseColor.WHITE;

                        sqlongquestion.AddCell(sqquesSTMT);
                        sqlongquestion.AddCell(sqkhali);
                        sqlongquestion.AddCell(smarks);
                        /////[ENDS]//////short question - complex model

                        //////ADD diagram to long question table - simple model
                        if (diagramifany != "")
                        {
                            //////adding link to the full size image - cleaner method
                            Chunk chu = new Chunk("See full size", diagramLinks);
                            chu.SetAnchor(diagramifany);
                            Phrase p = new Phrase(chu);

                            PdfPCell dcell = new PdfPCell(p);
                            dcell.Colspan = 2;
                            dcell.HorizontalAlignment = Element.ALIGN_CENTER;
                            dcell.BorderColor = BaseColor.WHITE;
                            /////adding link ends

                            shortques.AddCell(DiagramHolder);
                            shortques.AddCell(dcell);

                            sqlongquestion.AddCell(DiagramHolder);
                            sqlongquestion.AddCell(dcell);

                        }

                        //////adding diagram to long question(simple) ends
                        //if(less than 80)
                        if (question["Statement"].ToString().Length < 80)
                            doc1.Add(shortques);
                        else
                            doc1.Add(sqlongquestion);
                        //short questions rendering ends

                    }//short questions loop end
                    doc1.Add(partb);
                    //begin entering part B long questions

                    DataTable RowsAboveThisQuestion = DataCollection.AddQuestions.createLocalTable("OH yeah");

                    for (int c = no_partA; c < PaperQuestions.Rows.Count; c++)//int totalques = PaperQuestions.Rows.Count;
                    {
                        DataRow question = PaperQuestions.Rows[c];

                        //Adding OR between questions of choice
                        System.Diagnostics.Debug.WriteLine(question["Number"].ToString() + " " + question["Part"].ToString() + " - " + isORPlacable(RowsAboveThisQuestion,question["PNR"].ToString(), question["Number"].ToString(), question["Part"].ToString()));
                        if (isORPlacable(RowsAboveThisQuestion, question["PNR"].ToString(),question["Number"].ToString(), question["Part"].ToString()))
                            doc1.Add(or);
                        //END//Adding OR between questions of choice
                        
                        /////for diagram laden questions 
                        string diagramifany = question["Diagram"].ToString();
                        if (diagramifany != "")
                        diagramifany = diagramifany.Substring(diagramifany.IndexOf("blackchair.manhardeep.com"));///for production environment only
                        //diagramifany = Server.MapPath("Diagrams") + "/" + giveImageName(question["Diagram"].ToString());///for test environment only
                        PdfPCell DiagramHolder = new PdfPCell();
                        Anchor diagramlink = new Anchor();
                        if (diagramifany != "")
                        {
                            iTextSharp.text.Image diagr = iTextSharp.text.Image.GetInstance(Server.MapPath("Diagrams") + "/" + giveImageName(question["Diagram"].ToString()));
                            diagr.ScaleToFit(new Rectangle(0f, 0f, 300f, 300f));
                            DiagramHolder = new PdfPCell(diagr);
                            DiagramHolder.BorderColor = BaseColor.WHITE;
                            DiagramHolder.Colspan = 2;
                            DiagramHolder.HorizontalAlignment = Element.ALIGN_CENTER;
                            DiagramHolder.PaddingTop = 5;

                            diagramlink = new Anchor("View Full size", diagramLinks);
                            diagramlink.Reference = diagramifany;
                        }
                        //////Diagram code ends

                        if (question["Statement"].ToString().Length > 80)
                        {
                            //complex model of long question of part B
                            PdfPTable Longsqlongquestion = new PdfPTable(2);
                            Longsqlongquestion.WidthPercentage = 100;

                            Phrase Longsqst = new Phrase(question["Statement"].ToString(), questionfont);

                            Phrase LongquestionNumbering = new Phrase();
                            /*
                            if (question["Part"].ToString() == "a" || question["Part"].ToString() == "")
                                LongquestionNumbering.Add(new Phrase("Q" + question["Number"] + " ", boldquestionfont));
                            LongquestionNumbering.Add(new Phrase("" + question["Part"] + ") ", boldquestionfont));
                            if (question["SubPart"].ToString() != "")
                                LongquestionNumbering.Add(new Phrase("(" + question["SubPart"] + ") ", boldquestionfont));

                            */

                            if (question["Part"].ToString() == "a" || question["Part"].ToString() == "")
                                LongquestionNumbering.Add(new Phrase("Q" + question["Number"] + " ", boldquestionfont));
                            if (question["SubPart"].ToString() == "i" || question["SubPart"].ToString() == "")
                                LongquestionNumbering.Add(new Phrase("" + question["Part"] + ") ", boldquestionfont));
                            if (question["SubPart"].ToString() != "")
                                LongquestionNumbering.Add(new Phrase("(" + question["SubPart"] + ") ", boldquestionfont));




                            Phrase Longsadder = new Phrase();
                            Longsadder.Add(LongquestionNumbering);
                            Longsadder.Add(Longsqst);


                            string marksstring = question["Weightage"].ToString().Substring(0, question["Weightage"].ToString().Length - 1);
                            if (marksstring.ElementAt<char>(marksstring.Length - 1) == '0')
                                marksstring = marksstring.Substring(0, marksstring.Length - 2);


                            Phrase Longsqm = new Phrase("[ " + marksstring + " Marks ]", questionfont);
                            Longsqm.SetLeading(0, 0);

                            PdfPCell pcelllongsqm = new PdfPCell(Longsqm);
                            pcelllongsqm.BorderColor = BaseColor.WHITE;
                            pcelllongsqm.HorizontalAlignment = Element.ALIGN_RIGHT;

                            PdfPCell LongsqquesSTMT = new PdfPCell(Longsadder);
                            LongsqquesSTMT.Colspan = 2;
                            LongsqquesSTMT.BorderColor = BaseColor.WHITE;
                            LongsqquesSTMT.Padding = 0;


                            PdfPCell Longsqkhali = new PdfPCell();
                            Longsqkhali.BorderColor = BaseColor.WHITE;


                            Longsqlongquestion.AddCell(LongsqquesSTMT);
                            Longsqlongquestion.AddCell(Longsqkhali);
                            Longsqlongquestion.AddCell(pcelllongsqm);

                            //////ADD diagram to long question table - complex model
                            if (diagramifany != "")
                            {
                                //////adding link to the full size image - cleaner method
                                Chunk chu = new Chunk("See full size", diagramLinks);
                                chu.SetAnchor(diagramifany);
                                Phrase p = new Phrase(chu);

                                PdfPCell dcell = new PdfPCell(p);
                                dcell.Colspan = 2;
                                dcell.HorizontalAlignment = Element.ALIGN_CENTER;
                                dcell.BorderColor = BaseColor.WHITE;
                                /////adding link ends

                                /*
                                //Failed method
                                var DiagrLinking = new Chunk("View full size");
                                DiagrLinking.SetAnchor(diagramifany);
                                DiagramHolder.AddElement(DiagrLinking);
                                */

                                Longsqlongquestion.AddCell(DiagramHolder);
                                Longsqlongquestion.AddCell(dcell);

                            }

                            //////adding diagram to long question(complex) ends

                            //complex model ends

                            doc1.Add(Longsqlongquestion);
                        }
                        else
                        {
                            //simple model of long question of part B
                            PdfPTable slongshortques = new PdfPTable(2);
                            slongshortques.WidthPercentage = 100;
                            slongshortques.SpacingAfter = 2;

                            Phrase slongquestionNumbering = new Phrase();

                            float[] slongwidths = new float[] { 8.8f, 1.2f };
                            slongshortques.SetWidths(slongwidths);
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
                            Phrase slongsqst = new Phrase(question["Statement"].ToString(), questionfont);

                            Phrase slongsadder = new Phrase();
                            //slongsadder.Add(slongquestionNumbering);
                            slongsadder.Add(giveNumbering(question,slongquestionNumbering));
                            slongsadder.Add(slongsqst);

                            string marksstring = question["Weightage"].ToString().Substring(0, question["Weightage"].ToString().Length - 1);
                            if (marksstring.ElementAt<char>(marksstring.Length - 1) == '0')
                                marksstring = marksstring.Substring(0, marksstring.Length - 2);


                            Phrase slongsqm = new Phrase("[ " + marksstring + " Marks ]", questionfont);
                            slongsqm.SetLeading(0, 0);

                            PdfPCell slongsqstmt = new PdfPCell(slongsadder);
                            slongsqstmt.BorderColor = BaseColor.WHITE;
                            slongsqstmt.Padding = 0;

                            PdfPCell slongsmarks = new PdfPCell(slongsqm);
                            slongsmarks.BorderColor = BaseColor.WHITE;
                            slongsmarks.HorizontalAlignment = Element.ALIGN_RIGHT;
                            slongsmarks.Padding = 0;

                            slongshortques.AddCell(slongsqstmt);
                            slongshortques.AddCell(slongsmarks);


                            //////ADD diagram to long question table - simple model
                            if (diagramifany != "")
                            {
                                //////adding link to the full size image - cleaner method
                                Chunk chu = new Chunk("See full size", diagramLinks);
                                chu.SetAnchor(diagramifany);
                                Phrase p = new Phrase(chu);

                                PdfPCell dcell = new PdfPCell(p);
                                dcell.Colspan = 2;
                                dcell.HorizontalAlignment = Element.ALIGN_CENTER;
                                dcell.BorderColor = BaseColor.WHITE;
                                /////adding link ends

                                slongshortques.AddCell(DiagramHolder);
                                slongshortques.AddCell(dcell);

                            }

                            //////adding diagram to long question(simple) ends

                            //simple model ends


                            doc1.Add(slongshortques);

                        }
                        RowsAboveThisQuestion.ImportRow(question);

                    }//long questions loop ends

                    doc1.Add(endMark);
                    doc1.Close();
                    AddPageNumber(Server.MapPath("PDFs") + "/Doc1.pdf");

                    paperlink.Text = "Download PDF here: ";
                    staticlink.Visible = true;
                    staticDisplay.Visible = true;
                    staticDisplayer1.Visible = true;

                    lbl_temp.Text = "Done";
                    lbl_temp.Visible = true;

                }
                else if (pattern == "ob-lo")
                {
                    doc1.Add(parta);
                    //  lbl_temp.Text = "Unable to generate PDF for Pattern \"ob-lo\" yet. Check back later.";
                    //lbl_temp.Visible = true;

                    //doc1.Close();
                    generatePaperForType_oblo(PNR, no_partA, PaperQuestions, doc1);

                }
                else if (pattern == "ob")
                {
                    //lbl_temp.Text = "Unable to generate PDF for Pattern \"ob\" yet. Check back later.";
                    //lbl_temp.Visible = true;

                    doc1.Close();
                    generatePaperForType_ob(PNR);
                }
            }
            else if (PaperType(PNR) == "CA")
            {
                //Genrate CA paper
                con.Close();
                generateCAPaper(PNR);
            }
            staticDisplayer1.HRef = "~/ViewQuestionPaper.aspx?p=" + HttpUtility.UrlEncode(new QueryStringEncryption().encryptQueryString(TextBox1.Text.Trim())); 
        }

        private Phrase giveNumbering(DataRow question,Phrase slongquestionNumbering)
        {
            if (question["SubPart"].ToString() == "i")
            {
                if (question["Part"].ToString()!="a")
                    return new Phrase(question["Part"].ToString() + ") (" + question["SubPart"].ToString() + ") ", boldquestionfont);
                else
                   return new Phrase("Q" + question["Number"] + " " + question["Part"].ToString() + ") (" + question["SubPart"].ToString() + ") ", boldquestionfont);
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
                        return new Phrase("Q" + question["Number"] + " " + question["Part"].ToString() + ") ", boldquestionfont);
                    }
                    else
                    {
                        //part not a
                        if (question["Part"].ToString() == "")
                        {
                            //no part
                            //slongquestionNumbering.Add(new Phrase("Q" + question["Number"] + " ", boldquestionfont));
                            return new Phrase("Q" + question["Number"] + " ", boldquestionfont);
                        }
                        else
                        {
                            //part > a
                            return new Phrase(question["Part"] + ") ", boldquestionfont);
                            //slongquestionNumbering.Add(new Phrase(question["Part"] + ") ", boldquestionfont));
                        }
                    }
                }
                else
                {
                    //subpart > i
                    return new Phrase("(" + question["SubPart"] + ") ", boldquestionfont);
                    //slongquestionNumbering.Add(new Phrase("(" + question["SubPart"] + ") ", boldquestionfont));
                }
            }
        }

        private bool isORPlacable(DataTable RowsAboveThisQuestion, string PNR, string Number, string Part)
        {
            if (Part.ToLower() == "a")
                return false;

            System.Diagnostics.Debug.WriteLine("OOO - "+RowsAboveThisQuestion.Select("PNR='" + PNR + "' AND Number=" + Number + " AND Part='" + Part + "'").Count());
            if (RowsAboveThisQuestion.Select("PNR='"+PNR+"' AND Number="+Number+" AND Part='"+Part+"'").Count() == 0)
                return true;
            else
                return false;

            /*

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                SqlCommand tempcmd = new SqlCommand("select count(*) from questionpapersdump where pnr='" + PNR + "' and number=" + Number + "and part='" + Part + "'", con);
                if (con.State != ConnectionState.Open)
                    con.Open();
                else
                {
                    con.Close();
                    con.Open();
                }
                int PartExistsAlready = Convert.ToInt32(tempcmd.ExecuteScalar());
                System.Diagnostics.Debug.WriteLine("DB return: "+PartExistsAlready);
                if (PartExistsAlready == 0)
                    return true;
                else
                    return false;
            }*/
        }

        private void generateCAPaper(string PNR)
        {
            if (con.State != ConnectionState.Open)
                con.Open();
            else
            {
                con.Close();
                con.Open();
            }

            //Getting top data
            SqlCommand CA_fetcher = new SqlCommand("select CourseCode from CApapers where pnr='" + PNR + "'", con);
            var QueryResult = CA_fetcher.ExecuteScalar();
            string CourseCode = QueryResult == null ? "" : QueryResult.ToString();

            CA_fetcher.CommandText = "select CourseName from CApapers where pnr='" + PNR + "'";
            QueryResult = CA_fetcher.ExecuteScalar();
            string CourseName = QueryResult == null ? "" : QueryResult.ToString();

            CA_fetcher.CommandText = "select PaperSet from CApapers where pnr='" + PNR + "'";
            QueryResult = CA_fetcher.ExecuteScalar();
            string paperset = QueryResult == null ? "" : QueryResult.ToString();

            CA_fetcher.CommandText = "select MaxMarks from CApapers where pnr='" + PNR + "'";
            QueryResult = CA_fetcher.ExecuteScalar();
            string maxmarks = QueryResult == null ? "" : QueryResult.ToString();

            CA_fetcher.CommandText = "select TimeAllowed from CApapers where pnr='" + PNR + "'";
            QueryResult = CA_fetcher.ExecuteScalar();
            int timeallowed = Convert.ToInt32(QueryResult == null ? "60" : QueryResult.ToString());

            CA_fetcher.CommandText = "select CAnumber from CApapers where pnr='" + PNR + "'";
            QueryResult = CA_fetcher.ExecuteScalar();
            int canumber = Convert.ToInt32(QueryResult.ToString());
            //END//Getting top data

            var doc = new Document(PageSize.B6.Rotate());
            PdfWriter.GetInstance(doc, new FileStream(Server.MapPath("PDFs") + "/Doc1.pdf", FileMode.Create));
            doc.Open();

            System.Diagnostics.Debug.WriteLine("SET: -" + paperset + "-");
            if (paperset != " ")
            {
                System.Diagnostics.Debug.WriteLine("SETin: -" + paperset + "-");
                Paragraph CAset = new Paragraph("[ Set " + paperset + " ]");
                CAset.Alignment = Element.ALIGN_RIGHT;
                doc.Add(CAset);//CA set added topright 
            }
            if (canumber == 998)
            {
                Paragraph topheader = new Paragraph("Mid-Term Practical Evaluation", TopHeader);
                topheader.Alignment = Element.ALIGN_CENTER;
            doc.Add(topheader); //CA header added with CA number
            }
            else if (canumber == 999)
            {
                Paragraph topheader = new Paragraph("End-Term Practical Evaluation", TopHeader);
                topheader.Alignment = Element.ALIGN_CENTER;
            doc.Add(topheader); //CA header added with CA number
            }
            else
            {
                Paragraph topheader = new Paragraph("Continuous Assessment", TopHeader);
                topheader.Alignment = Element.ALIGN_CENTER;
                if (canumber != 0)
                {
                    Phrase CAnum = new Phrase(" - " + canumber);
                    topheader.Add(CAnum);
                }
            doc.Add(topheader); //CA header added with CA number
            }

           // doc.Add(topheader); //CA header added with CA number

            Paragraph courseDetails = new Paragraph("");
            if (CourseCode != "")
            {
                courseDetails = new Paragraph(CourseCode, TopHeader);
            }
            Phrase cname = new Phrase("");
            if (CourseName != "")
            {
                string separator = CourseCode == "" ? "" : " : ";
                cname = new Phrase(separator + CourseName, TopHeader);
            }
            courseDetails.Add(cname);
            courseDetails.Alignment = Element.ALIGN_CENTER;

            doc.Add(courseDetails);///Course Code and name added

            PdfPTable catimemarks = new PdfPTable(2);
            catimemarks.WidthPercentage = 100;

            PdfPCell ta = new PdfPCell(new Phrase("Time Allowed: " + formatTime(Convert.ToInt32(timeallowed)), Topsmall));
            PdfPCell mm = new PdfPCell(new Phrase("Max.Marks: " + maxmarks, Topsmall));

            ta.BorderWidth = 0;
            ta.HorizontalAlignment = Element.ALIGN_RIGHT;

            mm.BorderWidth = 0;

            catimemarks.AddCell(mm);
            catimemarks.AddCell(ta);

            doc.Add(catimemarks);

            //Start printing CA questions now
            doc.Add(NewLine_s);
            DataTable CAPaper = new DataTable("CApaper");
            SqlDataAdapter da = new SqlDataAdapter("select * from questionpapersdump where pnr = '" + PNR + "' order by number", con);
            da.Fill(CAPaper);//CA paper cached in CAPaper

            for (int j = 0; j < CAPaper.Rows.Count; j++)
            {
                DataRow question = CAPaper.Rows[j];

                if (question["Option1"].ToString() == "")//is not MCQ
                {

                    Phrase ph_Qstmt = new Phrase(question["Statement"].ToString().Trim(), questionfont);
                    string numbering = "";
                    /*
                     * No subpart concept in CA papers, SubParts if any must be added into the question statement
                     */
                    if (question["number"].ToString() != "")
                        if (question["Part"].ToString().ToLower() == "a" || question["Part"].ToString() == "1" || question["Part"].ToString().ToLower() == "i")
                            numbering = "Q" + question["number"].ToString() + ". " + question["part"].ToString() + ") ";
                        else
                        {
                            if (question["part"].ToString() != "")
                                numbering = question["part"].ToString() + ") ";
                            else
                                numbering = "Q" + question["number"].ToString() + ". ";
                        }

                    Phrase ph_Qnum = new Phrase(numbering, boldquestionfont);
                    Phrase ph_qadder = new Phrase(ph_Qnum);
                    ph_qadder.Add(ph_Qstmt);

                    PdfPCell cell_Qstmt = new PdfPCell(ph_qadder);
                    cell_Qstmt.BorderWidth = 0;

                    PdfPTable Quest = new PdfPTable(2);

                    float[] slongwidths = new float[] { 8.8f, 1.2f };
                    Quest.SetWidths(slongwidths);

                    string marks = question["Weightage"].ToString();

                    PdfPCell cell_qmarks = new PdfPCell(new Phrase("[" + formatMarks(marks) + "]", questionfont));
                    cell_qmarks.BorderWidth = 0;
                    cell_qmarks.HorizontalAlignment = Element.ALIGN_RIGHT;

                    Quest.AddCell(cell_Qstmt);
                    Quest.AddCell(cell_qmarks);

                    if (question["Diagram"].ToString() != "")
                    {
                        //is diagramatic question
                        iTextSharp.text.Image diagramImage = iTextSharp.text.Image.GetInstance(Server.MapPath("Diagrams") + "/" + giveImageName(question["Diagram"].ToString()));
                        diagramImage.ScaleToFit(new Rectangle(0f, 0f, 100f, 100f));

                        PdfPTable DiagramHolder = new PdfPTable(1);
                        DiagramHolder.WidthPercentage = 100;

                        PdfPCell cell_ImgHolder = new PdfPCell(diagramImage);

                        DiagramHolder.AddCell(removeBorders(cell_ImgHolder));
                        DiagramHolder.AddCell(addViewFullSizeLink(question["Diagram"].ToString()));

                        PdfPCell cell_dHolder = new PdfPCell(DiagramHolder);
                        cell_dHolder.Colspan = 2;
                        //cell_DiagramHolder.AddElement();

                        Quest.AddCell(removeBorders(cell_dHolder));
                    }

                    doc.Add(Quest);

                    //doc.Add(NewLine_s);

                    ////NON MCQ question Printed
                }
                else//is MCQ question
                {
                    SqlCommand paperPatternFetch = new SqlCommand("select pattern from CApapers where pnr = '" + PNR + "'", con);
                    string pattern = paperPatternFetch.ExecuteScalar().ToString();

                    bool isOnlyMCQ = pattern == "ob" ? true : false;

                    // System.Diagnostics.Debug.WriteLine("isonlymcq: -" + isOnlyMCQ.ToString() + "-");

                    printMCQQuestion(doc, question, detectQuestionPattern(question), isOnlyMCQ);
                }
            }
            doc.Close();
            con.Close();

            paperlink.Text = "Download PDF here: ";
            staticlink.Visible = true;
            staticDisplay.Visible = true;
            staticDisplayer1.Visible = true;
            lbl_temp.Text = "Done";
            lbl_temp.Visible = true;

        }

        protected void Button2_Click(object sender, EventArgs e)
        {
            //System.Threading.Thread.Sleep(1000);

            GeneratePDFwithPNR(TextBox1.Text.Trim());
           // searchInHardPapers(TextBox1.Text.Trim());
        }

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
            staticDisplay.Visible = true;
            staticDisplayer1.Visible = true;
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

        public string detectQuestionPattern(DataRow question)
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

        public void printMCQQuestion(Document doc, DataRow question, string questionPattern, bool isOnlyMCQPaper)
        {

            /*
            3 types - according to length or longest option
            1. one word options - 1x4 grid
            2. phrases of option - 2x2 grid
            3. long options - 4x1 grid
            */
            //lol it    :D   :D     string longest = question[question["Option1"].ToString().Length > question["Option2"].ToString().Length ? "Option1" : "Option2"].ToString().Length > question[question["Option3"].ToString().Length > question["Option4"].ToString().Length ? "Option3" : "Option4"].ToString().Length ? "1" : "2";

            string longest = "1";
            for (int ci = 2; ci < 5; ci++)
            {
                if (question["Option" + ci].ToString().Length > question["Option" + longest].ToString().Length)
                    longest = ci.ToString();
            }

            int LongestOptionLength = question["Option" + longest].ToString().Length;
            //Printing Question statement with question number

            string questionNumbering = "";
            if (isOnlyMCQPaper)
                questionNumbering = "Q" + question["Number"].ToString();
            else
            {
                //for mix type papers (mcq +long) = add Q1. once
                if (question["Part"].ToString() == "a")
                    questionNumbering = "Q" + question["Number"].ToString();
                else
                    questionNumbering = question["Part"].ToString();
            }


            Phrase QuestionNumber = new Phrase(questionNumbering + ". ", boldquestionfont);
            Phrase QuestionStatement = new Phrase(question["Statement"].ToString(), questionfont);

            Phrase QuestionText = new Phrase();
            QuestionText.Add(QuestionNumber);
            QuestionText.Add(QuestionStatement);

            PdfPCell QuestionStmt = new PdfPCell(QuestionText);
            QuestionStmt.BorderColor = BaseColor.WHITE;
            //END//Printing Question statement with question number

            if (questionPattern == "00")//No diagram - simple MCQ question
            {
                ////Creating options cells
                PdfPCell option1 = new PdfPCell(new Phrase("(a) " + question["Option1"].ToString(), questionfont));
                PdfPCell option2 = new PdfPCell(new Phrase("(b) " + question["Option2"].ToString(), questionfont));
                PdfPCell option3 = new PdfPCell(new Phrase("(c) " + question["Option3"].ToString(), questionfont));
                PdfPCell option4 = new PdfPCell(new Phrase("(d) " + question["Option4"].ToString(), questionfont));

                option1.BorderColor = BaseColor.WHITE;
                option2.BorderColor = BaseColor.WHITE;
                option3.BorderColor = BaseColor.WHITE;
                option4.BorderColor = BaseColor.WHITE;
                //END//Creating options cells

                if (LongestOptionLength > 80)
                {
                    //type 3
                    PdfPTable MCQType3Question = new PdfPTable(1);
                    MCQType3Question.WidthPercentage = 100;
                    //no colspann needed for QuestionStmt
                    MCQType3Question.AddCell(QuestionStmt);

                    ////Printing Options
                    MCQType3Question.AddCell(option1);
                    MCQType3Question.AddCell(option2);
                    MCQType3Question.AddCell(option3);
                    MCQType3Question.AddCell(option4);
                    //END//Printing Options

                    doc.Add(MCQType3Question);
                }
                else if (LongestOptionLength > 45)
                {
                    //type 2
                    PdfPTable MCQType2Question = new PdfPTable(2);
                    MCQType2Question.WidthPercentage = 100;

                    QuestionStmt.Colspan = 2;
                    MCQType2Question.AddCell(QuestionStmt);

                    ////Printing Options
                    MCQType2Question.AddCell(option1);
                    MCQType2Question.AddCell(option2);
                    MCQType2Question.AddCell(option3);
                    MCQType2Question.AddCell(option4);
                    //END//Printing Options

                    doc.Add(MCQType2Question);
                }
                else
                {
                    //type 1
                    PdfPTable MCQType1Question = new PdfPTable(4);
                    MCQType1Question.WidthPercentage = 100;

                    QuestionStmt.Colspan = 4;
                    MCQType1Question.AddCell(QuestionStmt);

                    ////Printing Options
                    MCQType1Question.AddCell(option1);
                    MCQType1Question.AddCell(option2);
                    MCQType1Question.AddCell(option3);
                    MCQType1Question.AddCell(option4);
                    //END//Printing Options

                    doc.Add(MCQType1Question);
                }
            }


            /////////////////////////////////Method simplifier
            /*
             * COMMENTED below if statement to implement the mothod Simplifier model to save time yet.
             */
            //else if (questionPattern == "01")//Diagram only in Options
            //{

            else
            {
                ///////////Simple Method - [TIME SAVER]
                PdfPTable GenericMCQ = new PdfPTable(1);//2 rows

                PdfPTable Generic_QuestionPart = new PdfPTable(1);//2 rows
                PdfPTable Generic_OptionsPart = new PdfPTable(1);//2 rows

                PdfPTable Generic_Option1 = new PdfPTable(1);//compound options
                PdfPTable Generic_Option2 = new PdfPTable(1);
                PdfPTable Generic_Option3 = new PdfPTable(1);
                PdfPTable Generic_Option4 = new PdfPTable(1);

                GenericMCQ.WidthPercentage = 100;

                Generic_QuestionPart.WidthPercentage = 100;
                Generic_OptionsPart.WidthPercentage = 100;

                Generic_Option1.WidthPercentage = 100;
                Generic_Option2.WidthPercentage = 100;
                Generic_Option3.WidthPercentage = 100;
                Generic_Option4.WidthPercentage = 100;

                PdfPTable Option1DiagramHolder = new PdfPTable(1);
                PdfPTable Option2DiagramHolder = new PdfPTable(1);
                PdfPTable Option3DiagramHolder = new PdfPTable(1);
                PdfPTable Option4DiagramHolder = new PdfPTable(1);

                iTextSharp.text.Image Option1File;
                iTextSharp.text.Image Option2File;
                iTextSharp.text.Image Option3File;
                iTextSharp.text.Image Option4File;
                /*
                try
                {*/
                PdfPCell cell_Option1Image = new PdfPCell();
                PdfPCell cell_Option2Image = new PdfPCell();
                PdfPCell cell_Option3Image = new PdfPCell();
                PdfPCell cell_Option4Image = new PdfPCell();

                cell_Option1Image.BorderColor = BaseColor.WHITE;
                cell_Option2Image.BorderColor = BaseColor.WHITE;
                cell_Option3Image.BorderColor = BaseColor.WHITE;
                cell_Option4Image.BorderColor = BaseColor.WHITE;

                if (question["Option1File"].ToString() != "")
                {
                    Option1File = iTextSharp.text.Image.GetInstance(Server.MapPath("Diagrams").ToString() + "/" + giveImageName(question["Option1File"].ToString()));
                    Option1File.ScaleToFit(new Rectangle(0f, 0f, 100f, 100f));
                    cell_Option1Image = new PdfPCell(Option1File);
                    //  Option1DiagramHolder.AddCell(addViewFullSizeLink(question["Option1File"].ToString()));
                }
                if (question["Option2File"].ToString() != "")
                {
                    Option2File = iTextSharp.text.Image.GetInstance(Server.MapPath("Diagrams").ToString() + "/" + giveImageName(question["Option2File"].ToString()));
                    Option2File.ScaleToFit(new Rectangle(0f, 0f, 100f, 100f));
                    cell_Option2Image = new PdfPCell(Option2File);
                    //   Option2DiagramHolder.AddCell(addViewFullSizeLink(question["Option2File"].ToString()));
                }
                if (question["Option3File"].ToString() != "")
                {
                    Option3File = iTextSharp.text.Image.GetInstance(Server.MapPath("Diagrams").ToString() + "/" + giveImageName(question["Option3File"].ToString()));
                    Option3File.ScaleToFit(new Rectangle(0f, 0f, 100f, 100f));
                    cell_Option3Image = new PdfPCell(Option3File);
                    // Option3DiagramHolder.AddCell(addViewFullSizeLink(question["Option3File"].ToString()));
                }
                if (question["Option4File"].ToString() != "")
                {
                    Option4File = iTextSharp.text.Image.GetInstance(Server.MapPath("Diagrams").ToString() + "/" + giveImageName(question["Option4File"].ToString()));
                    Option4File.ScaleToFit(new Rectangle(0f, 0f, 100f, 100f));
                    cell_Option4Image = new PdfPCell(Option4File);
                    //  Option4DiagramHolder.AddCell(addViewFullSizeLink(question["Option4File"].ToString()));
                }
                /*   }
                   catch (IOException ImageSearchException)
                   {
                       System.Diagnostics.Debug.WriteLine(ImageSearchException.ToString());
                   }*/

                PdfPCell cell_Option1Text = new PdfPCell(new Phrase("(a) " + question["Option1"].ToString()));
                PdfPCell cell_Option2Text = new PdfPCell(new Phrase("(b) " + question["Option2"].ToString()));
                PdfPCell cell_Option3Text = new PdfPCell(new Phrase("(c) " + question["Option3"].ToString()));
                PdfPCell cell_Option4Text = new PdfPCell(new Phrase("(d) " + question["Option4"].ToString()));


                cell_Option1Text.BorderColor = BaseColor.WHITE;
                cell_Option2Text.BorderColor = BaseColor.WHITE;
                cell_Option3Text.BorderColor = BaseColor.WHITE;
                cell_Option4Text.BorderColor = BaseColor.WHITE;

                Option1DiagramHolder.AddCell(removeBorders(cell_Option1Image));
                Option2DiagramHolder.AddCell(removeBorders(cell_Option2Image));
                Option3DiagramHolder.AddCell(removeBorders(cell_Option3Image));
                Option4DiagramHolder.AddCell(removeBorders(cell_Option4Image));


                if (question["Option1File"].ToString() != "")
                {
                    PdfPCell LinkToImageOptions = addViewFullSizeLink(question["Option1File"].ToString());
                    LinkToImageOptions.HorizontalAlignment = Element.ALIGN_LEFT;
                    LinkToImageOptions.BorderWidth = 0;
                    Option1DiagramHolder.AddCell(LinkToImageOptions);
                }
                if (question["Option2File"].ToString() != "")
                {
                    PdfPCell LinkToImageOptions = addViewFullSizeLink(question["Option2File"].ToString());
                    LinkToImageOptions.HorizontalAlignment = Element.ALIGN_LEFT;
                    LinkToImageOptions.BorderWidth = 0;
                    Option2DiagramHolder.AddCell(LinkToImageOptions);
                }
                if (question["Option3File"].ToString() != "")
                {
                    PdfPCell LinkToImageOptions = addViewFullSizeLink(question["Option3File"].ToString());
                    LinkToImageOptions.HorizontalAlignment = Element.ALIGN_LEFT;
                    LinkToImageOptions.BorderWidth = 0;
                    Option3DiagramHolder.AddCell(LinkToImageOptions);
                }
                if (question["Option4File"].ToString() != "")
                {
                    PdfPCell LinkToImageOptions = addViewFullSizeLink(question["Option4File"].ToString());
                    LinkToImageOptions.HorizontalAlignment = Element.ALIGN_LEFT;
                    LinkToImageOptions.BorderWidth = 0;
                    Option4DiagramHolder.AddCell(LinkToImageOptions);
                }

                Generic_Option1.AddCell(cell_Option1Text);
                Generic_Option2.AddCell(cell_Option2Text);
                Generic_Option3.AddCell(cell_Option3Text);
                Generic_Option4.AddCell(cell_Option4Text);

                Generic_Option1.AddCell(convertTableToCell(Option1DiagramHolder));
                Generic_Option2.AddCell(convertTableToCell(Option2DiagramHolder));
                Generic_Option3.AddCell(convertTableToCell(Option3DiagramHolder));
                Generic_Option4.AddCell(convertTableToCell(Option4DiagramHolder));
                //Four OptionTables ready

                //adding options to Options part of the question
                Generic_OptionsPart.AddCell(convertTableToCell(Generic_Option1));
                Generic_OptionsPart.AddCell(convertTableToCell(Generic_Option2));
                Generic_OptionsPart.AddCell(convertTableToCell(Generic_Option3));
                Generic_OptionsPart.AddCell(convertTableToCell(Generic_Option4));

                PdfPTable QuestionDiagramHolder = new PdfPTable(1);

                PdfPCell QDiagramImage = new PdfPCell();
                QDiagramImage.BorderColor = BaseColor.WHITE;
                if (question["Diagram"].ToString() != "")
                {
                    iTextSharp.text.Image DiagramImage = iTextSharp.text.Image.GetInstance(Server.MapPath("Diagrams").ToString() + "/" + giveImageName(question["Diagram"].ToString()));
                    DiagramImage.ScaleToFit(new Rectangle(0f, 0f, 200f, 200f));
                    QDiagramImage = new PdfPCell(DiagramImage);
                    QuestionDiagramHolder.AddCell(removeBorders(QDiagramImage));
                    PdfPCell LinkToImage = addViewFullSizeLink(question["Diagram"].ToString());
                    LinkToImage.HorizontalAlignment = Element.ALIGN_LEFT;
                    QuestionDiagramHolder.AddCell(LinkToImage);
                }
                
                questionNumbering = question["Part"].ToString();
                Phrase Qnumber = new Phrase(questionNumbering+". ", boldquestionfont);
                Phrase QStatement = new Phrase(question["Statement"].ToString(), questionfont);
                Phrase QuestionAdder = new Phrase();

                QuestionAdder.Add(Qnumber);
                QuestionAdder.Add(QStatement);

                Generic_QuestionPart.AddCell(removeBorders(new PdfPCell(QuestionAdder)));
                Generic_QuestionPart.AddCell(convertTableToCell(QuestionDiagramHolder));

                Generic_OptionsPart.SpacingBefore = 5;

                PdfPCell BorderRemover_Generic_QuestionPart = new PdfPCell(Generic_QuestionPart);
                PdfPCell BorderRemover_Generic_OptionsPart = new PdfPCell(Generic_OptionsPart);

                BorderRemover_Generic_QuestionPart.BorderWidth = 0;
                BorderRemover_Generic_OptionsPart.BorderWidth = 0;

                GenericMCQ.AddCell(BorderRemover_Generic_QuestionPart);
                GenericMCQ.AddCell(BorderRemover_Generic_OptionsPart);

                doc.Add(GenericMCQ);
                /////END//////Simple Method - [TIME SAVER]
                /*

                PdfPTable MCQType1Question = new PdfPTable(10);
                PdfPTable MCQType2Question = new PdfPTable(10);
                PdfPTable MCQType3Question = new PdfPTable(10);
                PdfPTable workTable = new PdfPTable(10);
                if (LongestOptionLength > 80)
                {
                    //type 3
                    MCQType3Question = new PdfPTable(1);
                    MCQType3Question.WidthPercentage = 100;

                    MCQType3Question.AddCell(QuestionStmt);

                }
                else if (LongestOptionLength > 45)
                {
                    //type 2
                    MCQType2Question = new PdfPTable(2);
                    MCQType2Question.WidthPercentage = 100;

                    QuestionStmt.Colspan = 2;
                    MCQType2Question.AddCell(QuestionStmt);
                }

                else
                {
                    //type 1
                    MCQType1Question = new PdfPTable(4);
                    MCQType1Question.WidthPercentage = 100;

                    QuestionStmt.Colspan = 4;
                    MCQType1Question.AddCell(QuestionStmt);
                }
                ////Capturing diagram from db for options
                if (question["Option1File"].ToString()!="")
                {
                    iTextSharp.text.Image Option1File = iTextSharp.text.Image.GetInstance(Server.MapPath("Diagrams").ToString() + "/"+ "IMG_4015.JPG");

                    Option1File.ScaleToFit(new Rectangle(0f, 0f, 100f, 100f));
                    doc.Add(Option1File);///////////////////TEMPORARY TEST
                    string diagramURL = question["Option1File"].ToString();
                    //diagramURL = diagramURL.Substring(diagramURL.IndexOf("blackchair.manhardeep.com"));//Production environment
                    //diagramURL = diagramURL.Substring(0);//test environment

                    //pdfpcell for link
                    PdfPCell ImageHolder = new PdfPCell(Option1File);
                    ImageHolder.BorderColor = BaseColor.WHITE;

                    PdfPTable DiagramHold = new PdfPTable(1);
                    DiagramHold.WidthPercentage = 100;

                    DiagramHold.AddCell(ImageHolder);
                    DiagramHold.AddCell(addViewFullSizeLink(diagramURL));
                    //diagram ready to be added to option

                    PdfPCell Optiontext = new PdfPCell(new Phrase(question["Option1"].ToString()));

                    PdfPTable option1 = new PdfPTable(1);
                    option1.WidthPercentage = 100;

                    option1.AddCell(DiagramHold);
                    option1.AddCell(Optiontext);

                    
                    if (MCQType1Question.NumberOfColumns != 10)
                        workTable = MCQType1Question;
                    else if (MCQType2Question.NumberOfColumns != 10)
                        workTable = MCQType2Question;
                    else if (MCQType3Question.NumberOfColumns != 10)
                        workTable = MCQType3Question;

                    workTable.AddCell(option1);
                }
                else
                {
                    PdfPCell option1 = new PdfPCell(new Phrase("(a) " + question["Option1"].ToString(), questionfont));
                    option1.BorderColor = BaseColor.WHITE;

                    if (MCQType1Question.NumberOfColumns != 10)
                        workTable = MCQType1Question;
                    else if (MCQType2Question.NumberOfColumns != 10)
                        workTable = MCQType2Question; 
                    else if (MCQType3Question.NumberOfColumns != 10)
                        workTable = MCQType3Question;

                    workTable.AddCell(option1);
                }//option 1 done
                 //END//Capturing diagram from db for options

                doc.Add(workTable);
                doc.Add(new Paragraph("ethe"));
                */
            }
            /*   else if (questionPattern == "10")//Digram only in question
               {

               }
               else if (questionPattern == "11")//Diagram in both question and options
               {

               }*
               /*
                * COMMENTED because simple time saver was implemeted then - 
                * following 1 column table model for all MCQs other than simple model where neither the question has Diagram nor the options
                */
            ////////////////END/////////////////Method simplifier

            doc.Add(new Paragraph("\n"));//inter-question spacing
        }

        public PdfPCell addViewFullSizeLink(string diagramURL)
        {
            Chunk chu = new Chunk("See full size", diagramLinks);
            chu.SetAnchor(diagramURL.Substring(diagramURL.IndexOf("blackchair.manhardeep.com")));//for production environment only
          //  chu.SetAnchor(diagramURL);// chu.SetAnchor(diagramURL); //for Test environment only

            Phrase p = new Phrase(chu);

            PdfPCell dcell = new PdfPCell(p);
            dcell.Colspan = 2;
            dcell.HorizontalAlignment = Element.ALIGN_CENTER;
            dcell.BorderColor = BaseColor.WHITE;

            return dcell;
        }

        private void putEndMark(Document doc)
        {
            Paragraph endMark = new Paragraph("\n-- End of Question Paper --", footerfont);
            endMark.Alignment = Element.ALIGN_CENTER;

            doc.Add(endMark);
        }

        public PdfPCell convertTableToCell(PdfPTable WorkTable)
        {
            PdfPCell cell = new PdfPCell(WorkTable);
            cell.BorderWidth = 0;

            return cell;
        }

        public PdfPCell removeBorders(PdfPCell WorkCell)
        {
            WorkCell.BorderWidth = 0;
            return WorkCell;
        }

        public string giveImageName(string ImageURL)
        {
            return ImageURL.Substring(ImageURL.IndexOf("/Diagrams/") + 10);
        }

        private void generatePaperForType_oblo(string PNR, int no_partA, DataTable PaperQuestions, Document doc1)
        {
            //////static content
            Paragraph parta = new Paragraph("\nPART A\n");
            parta.Alignment = Element.ALIGN_CENTER;
            parta.Font.SetStyle(Font.BOLDITALIC);
            parta.Font.Size = 11;
            Paragraph partb = new Paragraph("\nPART B\n");
            partb.Alignment = Element.ALIGN_CENTER;
            partb.Font.SetStyle(Font.BOLDITALIC);
            partb.Font.Size = 11;

            Paragraph or = new Paragraph("OR\n");
            or.Font.SetStyle(Font.BOLDITALIC);
            or.Alignment = Element.ALIGN_CENTER;
            or.Font.Size = 11;
            or.SpacingAfter = 2;

            Paragraph endMark = new Paragraph("\n-- End of Question Paper --", footerfont);
            endMark.Alignment = Element.ALIGN_CENTER;
            ///END/// static content

            //doc1.Add(parta);

            SqlDataAdapter paperdata = new SqlDataAdapter("select * from questionpapersdump where pnr = '" + PNR + "' and number=1 order by cast(Part as int)", con);
            //Sorting by (part as int) because it is observed that in oblo type questions where there are 20 mcqs the MCQs are numbred as 1,2,3 instead of a,b,c,d....
            paperdata.Fill(PaperQuestions);

            //////Rendering part A
            for (int c = 0; c < no_partA; c++)
            {
                DataRow question = PaperQuestions.Rows[c];
                string questionPattern = detectQuestionPattern(question);
                //return value of the format < digit >< digit > first for question diag second for option diag

                //printMCQQuestion(doc, question, questionPattern,true);
                printMCQQuestion(doc1, question, questionPattern, false);

            }
            ///END///Rendering part A

            doc1.Add(partb);

            SqlDataAdapter paperdata1 = new SqlDataAdapter("Select * from questionpapersdump where pnr='" + PNR + "' and number != 1 order by QuestionID", con);
            paperdata1.Fill(PaperQuestions);

            //begin entering part B long questions
            for (int c = no_partA; c < PaperQuestions.Rows.Count; c++)//int totalques = PaperQuestions.Rows.Count;
            {
                DataRow question = PaperQuestions.Rows[c];

                /////for diagram laden questions 
                string diagramifany = question["Diagram"].ToString();
                if (diagramifany != "")
                    diagramifany = diagramifany.Substring(diagramifany.IndexOf("blackchair.manhardeep.com"));
                PdfPCell DiagramHolder = new PdfPCell();
                Anchor diagramlink = new Anchor();
                if (diagramifany != "")
                {
                    iTextSharp.text.Image diagr = iTextSharp.text.Image.GetInstance(Server.MapPath("Diagrams") + "/" + giveImageName(question["Diagram"].ToString()));
                    diagr.ScaleToFit(new Rectangle(0f, 0f, 300f, 300f));
                    DiagramHolder = new PdfPCell(diagr);
                    DiagramHolder.BorderColor = BaseColor.WHITE;
                    DiagramHolder.Colspan = 2;
                    DiagramHolder.HorizontalAlignment = Element.ALIGN_CENTER;
                    DiagramHolder.PaddingTop = 5;

                    diagramlink = new Anchor("View Full size", diagramLinks);
                    diagramlink.Reference = diagramifany;
                }
                //////Diagram code ends

                if (question["Statement"].ToString().Length > 80)
                {
                    //complex model of long question of part B
                    PdfPTable Longsqlongquestion = new PdfPTable(2);
                    Longsqlongquestion.WidthPercentage = 100;

                    Phrase Longsqst = new Phrase(question["Statement"].ToString(), questionfont);

                    Phrase LongquestionNumbering = new Phrase();
                    /*
                    if (question["Part"].ToString() == "a" || question["Part"].ToString() == "")
                        LongquestionNumbering.Add(new Phrase("Q" + question["Number"] + " ", boldquestionfont));
                    LongquestionNumbering.Add(new Phrase("" + question["Part"] + ") ", boldquestionfont));
                    if (question["SubPart"].ToString() != "")
                        LongquestionNumbering.Add(new Phrase("(" + question["SubPart"] + ") ", boldquestionfont));

                    */

                    if (question["Part"].ToString() == "a" || question["Part"].ToString() == "")
                        LongquestionNumbering.Add(new Phrase("Q" + question["Number"] + " ", boldquestionfont));
                    if (question["SubPart"].ToString() == "i" || question["SubPart"].ToString() == "")
                        LongquestionNumbering.Add(new Phrase("" + question["Part"] + ") ", boldquestionfont));
                    if (question["SubPart"].ToString() != "")
                        LongquestionNumbering.Add(new Phrase("(" + question["SubPart"] + ") ", boldquestionfont));




                    Phrase Longsadder = new Phrase();
                    Longsadder.Add(LongquestionNumbering);
                    Longsadder.Add(Longsqst);


                    string marksstring = question["Weightage"].ToString().Substring(0, question["Weightage"].ToString().Length - 1);
                    if (marksstring.ElementAt<char>(marksstring.Length - 1) == '0')
                        marksstring = marksstring.Substring(0, marksstring.Length - 2);


                    Phrase Longsqm = new Phrase("[ " + marksstring + " Marks ]", questionfont);
                    Longsqm.SetLeading(0, 0);

                    PdfPCell pcelllongsqm = new PdfPCell(Longsqm);
                    pcelllongsqm.BorderColor = BaseColor.WHITE;
                    pcelllongsqm.HorizontalAlignment = Element.ALIGN_RIGHT;

                    PdfPCell LongsqquesSTMT = new PdfPCell(Longsadder);
                    LongsqquesSTMT.Colspan = 2;
                    LongsqquesSTMT.BorderColor = BaseColor.WHITE;
                    LongsqquesSTMT.Padding = 0;


                    PdfPCell Longsqkhali = new PdfPCell();
                    Longsqkhali.BorderColor = BaseColor.WHITE;


                    Longsqlongquestion.AddCell(LongsqquesSTMT);
                    Longsqlongquestion.AddCell(Longsqkhali);
                    Longsqlongquestion.AddCell(pcelllongsqm);

                    //////ADD diagram to long question table - complex model
                    if (diagramifany != "")
                    {
                        //////adding link to the full size image - cleaner method
                        Chunk chu = new Chunk("See full size", diagramLinks);
                        chu.SetAnchor(diagramifany);
                        Phrase p = new Phrase(chu);

                        PdfPCell dcell = new PdfPCell(p);
                        dcell.Colspan = 2;
                        dcell.HorizontalAlignment = Element.ALIGN_CENTER;
                        dcell.BorderColor = BaseColor.WHITE;
                        /////adding link ends

                        /*
                        //Failed method
                        var DiagrLinking = new Chunk("View full size");
                        DiagrLinking.SetAnchor(diagramifany);
                        DiagramHolder.AddElement(DiagrLinking);
                        */

                        Longsqlongquestion.AddCell(DiagramHolder);
                        Longsqlongquestion.AddCell(dcell);

                    }

                    //////adding diagram to long question(complex) ends

                    //complex model ends

                    doc1.Add(Longsqlongquestion);
                }
                else
                {
                    //simple model of long question of part B
                    PdfPTable slongshortques = new PdfPTable(2);
                    slongshortques.WidthPercentage = 100;
                    slongshortques.SpacingAfter = 2;

                    Phrase slongquestionNumbering = new Phrase();

                    float[] slongwidths = new float[] { 8.8f, 1.2f };
                    slongshortques.SetWidths(slongwidths);
                    /*
                    if (question["Part"].ToString() == "a" || question["Part"].ToString() == "")
                        slongquestionNumbering.Add(new Phrase("Q" + question["Number"] + " ", boldquestionfont));
                    slongquestionNumbering.Add(new Phrase("" + question["Part"] + ") ", boldquestionfont));
                    if (question["SubPart"].ToString() != "")
                        slongquestionNumbering.Add(new Phrase("(" + question["SubPart"] + ") ", boldquestionfont));
                    */


                    if (question["Part"].ToString() == "a" || question["Part"].ToString() == "")
                        slongquestionNumbering.Add(new Phrase("Q" + question["Number"] + " ", boldquestionfont));
                    if (question["SubPart"].ToString() == "i" || question["SubPart"].ToString() == "")
                        slongquestionNumbering.Add(new Phrase("" + question["Part"] + ") ", boldquestionfont));
                    if (question["SubPart"].ToString() != "")
                        slongquestionNumbering.Add(new Phrase("(" + question["SubPart"] + ") ", boldquestionfont));


                    Phrase slongsqst = new Phrase(question["Statement"].ToString(), questionfont);

                    Phrase slongsadder = new Phrase();
                    slongsadder.Add(slongquestionNumbering);
                    slongsadder.Add(slongsqst);

                    string marksstring = question["Weightage"].ToString().Substring(0, question["Weightage"].ToString().Length - 1);
                    if (marksstring.ElementAt<char>(marksstring.Length - 1) == '0')
                        marksstring = marksstring.Substring(0, marksstring.Length - 2);


                    Phrase slongsqm = new Phrase("[ " + marksstring + " Marks ]", questionfont);
                    slongsqm.SetLeading(0, 0);

                    PdfPCell slongsqstmt = new PdfPCell(slongsadder);
                    slongsqstmt.BorderColor = BaseColor.WHITE;
                    slongsqstmt.Padding = 0;

                    PdfPCell slongsmarks = new PdfPCell(slongsqm);
                    slongsmarks.BorderColor = BaseColor.WHITE;
                    slongsmarks.HorizontalAlignment = Element.ALIGN_RIGHT;
                    slongsmarks.Padding = 0;

                    slongshortques.AddCell(slongsqstmt);
                    slongshortques.AddCell(slongsmarks);


                    //////ADD diagram to long question table - simple model
                    if (diagramifany != "")
                    {
                        //////adding link to the full size image - cleaner method
                        Chunk chu = new Chunk("See full size", diagramLinks);
                        chu.SetAnchor(diagramifany);
                        Phrase p = new Phrase(chu);

                        PdfPCell dcell = new PdfPCell(p);
                        dcell.Colspan = 2;
                        dcell.HorizontalAlignment = Element.ALIGN_CENTER;
                        dcell.BorderColor = BaseColor.WHITE;
                        /////adding link ends

                        slongshortques.AddCell(DiagramHolder);
                        slongshortques.AddCell(dcell);

                    }

                    //////adding diagram to long question(simple) ends

                    //simple model ends


                    doc1.Add(slongshortques);

                }

                if (question["Part"].ToString() == "a")
                    doc1.Add(or);
            }//long questions loop ends
            doc1.Add(endMark);
            doc1.Close();
            AddPageNumber(Server.MapPath("PDFs") + "/Doc1.pdf");
            staticDisplayer1.Visible = true;
            paperlink.Text = "Download PDF here: ";
            staticlink.Visible = true;
            staticDisplay.Visible = true;
            
            lbl_temp.Text = "Done";
            lbl_temp.Visible = true;
            
        }

        public string formatMarks(string rawMarks)
        {
            if (rawMarks.Substring(rawMarks.IndexOf(".") + 1) == "00")
                return rawMarks.Substring(0, rawMarks.IndexOf("."));
            else
                return rawMarks;
        }

        public PdfPTable renderMCQQuestion(DataRow question, string questionPattern, int DTQuestionNumber)
        {
            /*
            3 types - according to length or longest option
            1. one word options - 1x4 grid
            2. phrases of option - 2x2 grid
            3. long options - 4x1 grid
            */
            //lol it    :D   :D     string longest = question[question["Option1"].ToString().Length > question["Option2"].ToString().Length ? "Option1" : "Option2"].ToString().Length > question[question["Option3"].ToString().Length > question["Option4"].ToString().Length ? "Option3" : "Option4"].ToString().Length ? "1" : "2";

            string longest = "1";
            for (int ci = 2; ci < 5; ci++)
            {
                if (question["Option" + ci].ToString().Length > question["Option" + longest].ToString().Length)
                    longest = ci.ToString();
            }

            int LongestOptionLength = question["Option" + longest].ToString().Length;
            //Printing Question statement with question number

            string questionNumbering = DTQuestionNumber.ToString();
            
            /*
            if (isOnlyMCQPaper)
                questionNumbering = "Q" + question["Number"].ToString();
            else
            {
                //for mix type papers (mcq +long) = add Q1. once
                if (question["Part"].ToString() == "a")
                    questionNumbering = "Q" + question["Number"].ToString();
                else
                    questionNumbering = question["Part"].ToString();
            }
            */


            Phrase QuestionNumber = new Phrase(questionNumbering + ". ", boldquestionfont);
            Phrase QuestionStatement = new Phrase(question["Statement"].ToString(), questionfont);

            Phrase QuestionText = new Phrase();
            QuestionText.Add(QuestionNumber);
            QuestionText.Add(QuestionStatement);

            PdfPCell QuestionStmt = new PdfPCell(QuestionText);
            QuestionStmt.BorderColor = BaseColor.WHITE;
            //END//Printing Question statement with question number

            if (questionPattern == "00")//No diagram - simple MCQ question
            {
                ////Creating options cells
                PdfPCell option1 = new PdfPCell(new Phrase("(a) " + question["Option1"].ToString(), questionfont));
                PdfPCell option2 = new PdfPCell(new Phrase("(b) " + question["Option2"].ToString(), questionfont));
                PdfPCell option3 = new PdfPCell(new Phrase("(c) " + question["Option3"].ToString(), questionfont));
                PdfPCell option4 = new PdfPCell(new Phrase("(d) " + question["Option4"].ToString(), questionfont));

                option1.BorderColor = BaseColor.WHITE;
                option2.BorderColor = BaseColor.WHITE;
                option3.BorderColor = BaseColor.WHITE;
                option4.BorderColor = BaseColor.WHITE;
                //END//Creating options cells

                if (LongestOptionLength > 80)
                {
                    //type 3
                    PdfPTable MCQType3Question = new PdfPTable(1);
                    MCQType3Question.WidthPercentage = 100;
                    //no colspann needed for QuestionStmt
                    MCQType3Question.AddCell(QuestionStmt);

                    ////Printing Options
                    MCQType3Question.AddCell(option1);
                    MCQType3Question.AddCell(option2);
                    MCQType3Question.AddCell(option3);
                    MCQType3Question.AddCell(option4);
                    //END//Printing Options

                    //doc.Add(MCQType3Question);
                    return MCQType3Question;
                }
                else if (LongestOptionLength > 45)
                {
                    //type 2
                    PdfPTable MCQType2Question = new PdfPTable(2);
                    MCQType2Question.WidthPercentage = 100;

                    QuestionStmt.Colspan = 2;
                    MCQType2Question.AddCell(QuestionStmt);

                    ////Printing Options
                    MCQType2Question.AddCell(option1);
                    MCQType2Question.AddCell(option2);
                    MCQType2Question.AddCell(option3);
                    MCQType2Question.AddCell(option4);
                    //END//Printing Options

                    //doc.Add(MCQType2Question);
                    return MCQType2Question;
                }
                else
                {
                    //type 1
                    PdfPTable MCQType1Question = new PdfPTable(4);
                    MCQType1Question.WidthPercentage = 100;

                    QuestionStmt.Colspan = 4;
                    MCQType1Question.AddCell(QuestionStmt);

                    ////Printing Options
                    MCQType1Question.AddCell(option1);
                    MCQType1Question.AddCell(option2);
                    MCQType1Question.AddCell(option3);
                    MCQType1Question.AddCell(option4);
                    //END//Printing Options

                    //doc.Add(MCQType1Question);
                    return MCQType1Question;
                }
            }


            /////////////////////////////////Method simplifier
            /*
             * COMMENTED below if statement to implement the mothod Simplifier model to save time yet.
             */
            //else if (questionPattern == "01")//Diagram only in Options
            //{

            else
            {
                ///////////Simple Method - [TIME SAVER]
                PdfPTable GenericMCQ = new PdfPTable(1);//2 rows

                PdfPTable Generic_QuestionPart = new PdfPTable(1);//2 rows
                PdfPTable Generic_OptionsPart = new PdfPTable(1);//2 rows

                PdfPTable Generic_Option1 = new PdfPTable(1);//compound options
                PdfPTable Generic_Option2 = new PdfPTable(1);
                PdfPTable Generic_Option3 = new PdfPTable(1);
                PdfPTable Generic_Option4 = new PdfPTable(1);

                GenericMCQ.WidthPercentage = 100;

                Generic_QuestionPart.WidthPercentage = 100;
                Generic_OptionsPart.WidthPercentage = 100;

                Generic_Option1.WidthPercentage = 100;
                Generic_Option2.WidthPercentage = 100;
                Generic_Option3.WidthPercentage = 100;
                Generic_Option4.WidthPercentage = 100;

                PdfPTable Option1DiagramHolder = new PdfPTable(1);
                PdfPTable Option2DiagramHolder = new PdfPTable(1);
                PdfPTable Option3DiagramHolder = new PdfPTable(1);
                PdfPTable Option4DiagramHolder = new PdfPTable(1);

                iTextSharp.text.Image Option1File;
                iTextSharp.text.Image Option2File;
                iTextSharp.text.Image Option3File;
                iTextSharp.text.Image Option4File;
                /*
                try
                {*/
                PdfPCell cell_Option1Image = new PdfPCell();
                PdfPCell cell_Option2Image = new PdfPCell();
                PdfPCell cell_Option3Image = new PdfPCell();
                PdfPCell cell_Option4Image = new PdfPCell();

                cell_Option1Image.BorderColor = BaseColor.WHITE;
                cell_Option2Image.BorderColor = BaseColor.WHITE;
                cell_Option3Image.BorderColor = BaseColor.WHITE;
                cell_Option4Image.BorderColor = BaseColor.WHITE;

                if (question["Option1File"].ToString() != "")
                {
                    Option1File = iTextSharp.text.Image.GetInstance(Server.MapPath("Diagrams").ToString() + "/" + giveImageName(question["Option1File"].ToString()));
                    Option1File.ScaleToFit(new Rectangle(0f, 0f, 100f, 100f));
                    cell_Option1Image = new PdfPCell(Option1File);
                    //  Option1DiagramHolder.AddCell(addViewFullSizeLink(question["Option1File"].ToString()));
                }
                if (question["Option2File"].ToString() != "")
                {
                    Option2File = iTextSharp.text.Image.GetInstance(Server.MapPath("Diagrams").ToString() + "/" + giveImageName(question["Option2File"].ToString()));
                    Option2File.ScaleToFit(new Rectangle(0f, 0f, 100f, 100f));
                    cell_Option2Image = new PdfPCell(Option2File);
                    //   Option2DiagramHolder.AddCell(addViewFullSizeLink(question["Option2File"].ToString()));
                }
                if (question["Option3File"].ToString() != "")
                {
                    Option3File = iTextSharp.text.Image.GetInstance(Server.MapPath("Diagrams").ToString() + "/" + giveImageName(question["Option3File"].ToString()));
                    Option3File.ScaleToFit(new Rectangle(0f, 0f, 100f, 100f));
                    cell_Option3Image = new PdfPCell(Option3File);
                    // Option3DiagramHolder.AddCell(addViewFullSizeLink(question["Option3File"].ToString()));
                }
                if (question["Option4File"].ToString() != "")
                {
                    Option4File = iTextSharp.text.Image.GetInstance(Server.MapPath("Diagrams").ToString() + "/" + giveImageName(question["Option4File"].ToString()));
                    Option4File.ScaleToFit(new Rectangle(0f, 0f, 100f, 100f));
                    cell_Option4Image = new PdfPCell(Option4File);
                    //  Option4DiagramHolder.AddCell(addViewFullSizeLink(question["Option4File"].ToString()));
                }
                /*   }
                   catch (IOException ImageSearchException)
                   {
                       System.Diagnostics.Debug.WriteLine(ImageSearchException.ToString());
                   }*/

                PdfPCell cell_Option1Text = new PdfPCell(new Phrase("(a) " + question["Option1"].ToString()));
                PdfPCell cell_Option2Text = new PdfPCell(new Phrase("(b) " + question["Option2"].ToString()));
                PdfPCell cell_Option3Text = new PdfPCell(new Phrase("(c) " + question["Option3"].ToString()));
                PdfPCell cell_Option4Text = new PdfPCell(new Phrase("(d) " + question["Option4"].ToString()));


                cell_Option1Text.BorderColor = BaseColor.WHITE;
                cell_Option2Text.BorderColor = BaseColor.WHITE;
                cell_Option3Text.BorderColor = BaseColor.WHITE;
                cell_Option4Text.BorderColor = BaseColor.WHITE;

                Option1DiagramHolder.AddCell(removeBorders(cell_Option1Image));
                Option2DiagramHolder.AddCell(removeBorders(cell_Option2Image));
                Option3DiagramHolder.AddCell(removeBorders(cell_Option3Image));
                Option4DiagramHolder.AddCell(removeBorders(cell_Option4Image));


                if (question["Option1File"].ToString() != "")
                {
                    PdfPCell LinkToImageOptions = addViewFullSizeLink(question["Option1File"].ToString());
                    LinkToImageOptions.HorizontalAlignment = Element.ALIGN_LEFT;
                    LinkToImageOptions.BorderWidth = 0;
                    Option1DiagramHolder.AddCell(LinkToImageOptions);
                }
                if (question["Option2File"].ToString() != "")
                {
                    PdfPCell LinkToImageOptions = addViewFullSizeLink(question["Option2File"].ToString());
                    LinkToImageOptions.HorizontalAlignment = Element.ALIGN_LEFT;
                    LinkToImageOptions.BorderWidth = 0;
                    Option2DiagramHolder.AddCell(LinkToImageOptions);
                }
                if (question["Option3File"].ToString() != "")
                {
                    PdfPCell LinkToImageOptions = addViewFullSizeLink(question["Option3File"].ToString());
                    LinkToImageOptions.HorizontalAlignment = Element.ALIGN_LEFT;
                    LinkToImageOptions.BorderWidth = 0;
                    Option3DiagramHolder.AddCell(LinkToImageOptions);
                }
                if (question["Option4File"].ToString() != "")
                {
                    PdfPCell LinkToImageOptions = addViewFullSizeLink(question["Option4File"].ToString());
                    LinkToImageOptions.HorizontalAlignment = Element.ALIGN_LEFT;
                    LinkToImageOptions.BorderWidth = 0;
                    Option4DiagramHolder.AddCell(LinkToImageOptions);
                }

                Generic_Option1.AddCell(cell_Option1Text);
                Generic_Option2.AddCell(cell_Option2Text);
                Generic_Option3.AddCell(cell_Option3Text);
                Generic_Option4.AddCell(cell_Option4Text);

                Generic_Option1.AddCell(convertTableToCell(Option1DiagramHolder));
                Generic_Option2.AddCell(convertTableToCell(Option2DiagramHolder));
                Generic_Option3.AddCell(convertTableToCell(Option3DiagramHolder));
                Generic_Option4.AddCell(convertTableToCell(Option4DiagramHolder));
                //Four OptionTables ready

                //adding options to Options part of the question
                Generic_OptionsPart.AddCell(convertTableToCell(Generic_Option1));
                Generic_OptionsPart.AddCell(convertTableToCell(Generic_Option2));
                Generic_OptionsPart.AddCell(convertTableToCell(Generic_Option3));
                Generic_OptionsPart.AddCell(convertTableToCell(Generic_Option4));

                PdfPTable QuestionDiagramHolder = new PdfPTable(1);

                PdfPCell QDiagramImage = new PdfPCell();
                QDiagramImage.BorderColor = BaseColor.WHITE;
                if (question["Diagram"].ToString() != "")
                {
                    iTextSharp.text.Image DiagramImage = iTextSharp.text.Image.GetInstance(Server.MapPath("Diagrams").ToString() + "/" + giveImageName(question["Diagram"].ToString()));
                    DiagramImage.ScaleToFit(new Rectangle(0f, 0f, 200f, 200f));
                    QDiagramImage = new PdfPCell(DiagramImage);
                    QuestionDiagramHolder.AddCell(removeBorders(QDiagramImage));
                    PdfPCell LinkToImage = addViewFullSizeLink(question["Diagram"].ToString());
                    LinkToImage.HorizontalAlignment = Element.ALIGN_LEFT;
                    QuestionDiagramHolder.AddCell(LinkToImage);
                }

                questionNumbering = question["Part"].ToString();
                Phrase Qnumber = new Phrase(questionNumbering + ". ", boldquestionfont);
                Phrase QStatement = new Phrase(question["Statement"].ToString(), questionfont);
                Phrase QuestionAdder = new Phrase();

                QuestionAdder.Add(Qnumber);
                QuestionAdder.Add(QStatement);

                Generic_QuestionPart.AddCell(removeBorders(new PdfPCell(QuestionAdder)));
                Generic_QuestionPart.AddCell(convertTableToCell(QuestionDiagramHolder));

                Generic_OptionsPart.SpacingBefore = 5;

                PdfPCell BorderRemover_Generic_QuestionPart = new PdfPCell(Generic_QuestionPart);
                PdfPCell BorderRemover_Generic_OptionsPart = new PdfPCell(Generic_OptionsPart);

                BorderRemover_Generic_QuestionPart.BorderWidth = 0;
                BorderRemover_Generic_OptionsPart.BorderWidth = 0;

                GenericMCQ.AddCell(BorderRemover_Generic_QuestionPart);
                GenericMCQ.AddCell(BorderRemover_Generic_OptionsPart);

                //doc.Add(GenericMCQ);
                return GenericMCQ;



                /////END//////Simple Method - [TIME SAVER]
                /*

                PdfPTable MCQType1Question = new PdfPTable(10);
                PdfPTable MCQType2Question = new PdfPTable(10);
                PdfPTable MCQType3Question = new PdfPTable(10);
                PdfPTable workTable = new PdfPTable(10);
                if (LongestOptionLength > 80)
                {
                    //type 3
                    MCQType3Question = new PdfPTable(1);
                    MCQType3Question.WidthPercentage = 100;

                    MCQType3Question.AddCell(QuestionStmt);

                }
                else if (LongestOptionLength > 45)
                {
                    //type 2
                    MCQType2Question = new PdfPTable(2);
                    MCQType2Question.WidthPercentage = 100;

                    QuestionStmt.Colspan = 2;
                    MCQType2Question.AddCell(QuestionStmt);
                }

                else
                {
                    //type 1
                    MCQType1Question = new PdfPTable(4);
                    MCQType1Question.WidthPercentage = 100;

                    QuestionStmt.Colspan = 4;
                    MCQType1Question.AddCell(QuestionStmt);
                }
                ////Capturing diagram from db for options
                if (question["Option1File"].ToString()!="")
                {
                    iTextSharp.text.Image Option1File = iTextSharp.text.Image.GetInstance(Server.MapPath("Diagrams").ToString() + "/"+ "IMG_4015.JPG");

                    Option1File.ScaleToFit(new Rectangle(0f, 0f, 100f, 100f));
                    doc.Add(Option1File);///////////////////TEMPORARY TEST
                    string diagramURL = question["Option1File"].ToString();
                    //diagramURL = diagramURL.Substring(diagramURL.IndexOf("blackchair.manhardeep.com"));//Production environment
                    //diagramURL = diagramURL.Substring(0);//test environment

                    //pdfpcell for link
                    PdfPCell ImageHolder = new PdfPCell(Option1File);
                    ImageHolder.BorderColor = BaseColor.WHITE;

                    PdfPTable DiagramHold = new PdfPTable(1);
                    DiagramHold.WidthPercentage = 100;

                    DiagramHold.AddCell(ImageHolder);
                    DiagramHold.AddCell(addViewFullSizeLink(diagramURL));
                    //diagram ready to be added to option

                    PdfPCell Optiontext = new PdfPCell(new Phrase(question["Option1"].ToString()));

                    PdfPTable option1 = new PdfPTable(1);
                    option1.WidthPercentage = 100;

                    option1.AddCell(DiagramHold);
                    option1.AddCell(Optiontext);

                    
                    if (MCQType1Question.NumberOfColumns != 10)
                        workTable = MCQType1Question;
                    else if (MCQType2Question.NumberOfColumns != 10)
                        workTable = MCQType2Question;
                    else if (MCQType3Question.NumberOfColumns != 10)
                        workTable = MCQType3Question;

                    workTable.AddCell(option1);
                }
                else
                {
                    PdfPCell option1 = new PdfPCell(new Phrase("(a) " + question["Option1"].ToString(), questionfont));
                    option1.BorderColor = BaseColor.WHITE;

                    if (MCQType1Question.NumberOfColumns != 10)
                        workTable = MCQType1Question;
                    else if (MCQType2Question.NumberOfColumns != 10)
                        workTable = MCQType2Question; 
                    else if (MCQType3Question.NumberOfColumns != 10)
                        workTable = MCQType3Question;

                    workTable.AddCell(option1);
                }//option 1 done
                 //END//Capturing diagram from db for options

                doc.Add(workTable);
                doc.Add(new Paragraph("ethe"));
                */
            }
            /*   else if (questionPattern == "10")//Digram only in question
               {

               }
               else if (questionPattern == "11")//Diagram in both question and options
               {

               }*
               /*
                * COMMENTED because simple time saver was implemeted then - 
                * following 1 column table model for all MCQs other than simple model where neither the question has Diagram nor the options
                */
            ////////////////END/////////////////Method simplifier

            //doc.Add(new Paragraph("\n"));//inter-question spacing
        }

        public void searchInHardPapers(string PNR)
        {
            //System.Threading.Thread.Sleep(1000);

            DataTable ResultsFromHardPapers = new DataTable();

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                using (SqlDataAdapter ada = new SqlDataAdapter("select * from HardTypedPapers where pnr = '" + PNR + "'", con))
                {
                    con.Open();
                    ada.Fill(ResultsFromHardPapers);
                    con.Close();
                }
            }
            if(ResultsFromHardPapers.Rows.Count==1)
            {
                hl_HardResult.NavigateUrl = ResultsFromHardPapers.Rows[0]["FilePath"].ToString().Substring(ResultsFromHardPapers.Rows[0]["FilePath"].ToString().IndexOf("Hard Typed"));
                hl_HardResult.Target = "_blank";
                hl_HardResult.Text = "Hard Paper found";
            }
            else if(ResultsFromHardPapers.Rows.Count == 0)
            {
                lbl_HardResult.Text = "No Hard Paper found for this PNR";
            }
            else
            {
                lbl_HardResult.Text = "Multiple papers found for this PNR. You need to report this to the System Admin. +91 9988255277";
            }
        }

        protected void btn_SearchHardPapers_Click(object sender, EventArgs e)
        {
            searchInHardPapers(TextBox1.Text.Trim());
        }

        //Deprecated methods
        /*
         
        protected void GeneratePDF()
        {
            var doc1 = new Document(PageSize.A4, 52, 52, 60, 25);
            string path = Server.MapPath("PDFs");
            PdfWriter.GetInstance(doc1, new FileStream(path + "/Doc1.pdf", FileMode.Create));
            //int j = TextBox1.Text == "" ? 8 : Convert.ToInt32(TextBox1.Text);

            Font Topsmall = FontFactory.GetFont("Helvetica", 9, Font.BOLD, BaseColor.BLACK);
            Font small = FontFactory.GetFont("Helvetica", 9);
            Font TopHeader = FontFactory.GetFont("Helvetica", 12, Font.BOLD, BaseColor.BLACK);
            Font fnt_inst = FontFactory.GetFont("Helvetica", 9, Font.ITALIC);
            Font questionfont = FontFactory.GetFont("Helvetica", 10);
            Font boldquestionfont = FontFactory.GetFont("Helvetica", 10, Font.BOLD, BaseColor.BLACK);
            Font papercodebold = FontFactory.GetFont("Helvetica", 17, Font.BOLD, BaseColor.BLACK);


            doc1.Open();
            Paragraph rno = new Paragraph("Registration No:.__________________\n", Topsmall);
            Paragraph cdetails = new Paragraph("COURSE CODE : <COURSE>\nCOURSE NAME : <NAME>\n", TopHeader);
            Paragraph pnr = new Paragraph("PNR No:: <PNR>\n", Topsmall);
            PdfPTable paperdetails = new PdfPTable(2);
            PdfPCell ta = new PdfPCell(new Phrase("Time Allowed: <time>", Topsmall));
            PdfPCell mm = new PdfPCell(new Phrase("Max.Marks: <marks>", Topsmall));

            Paragraph NewLine_s = new Paragraph("\n", Topsmall);

            Paragraph instinst = new Paragraph("Read the following instructions carefully before attempting the question paper.", fnt_inst);

            ta.BorderColor = BaseColor.WHITE;
            mm.BorderColor = BaseColor.WHITE;
            ta.HorizontalAlignment = 0;
            mm.HorizontalAlignment = 2;
            paperdetails.AddCell(ta);
            paperdetails.AddCell(mm);
            paperdetails.AddCell(new PdfPCell(new Phrase("Time Allowed: <time>")));
            paperdetails.WidthPercentage = 100;

            pnr.Alignment = Element.ALIGN_RIGHT;
            rno.Alignment = Element.ALIGN_CENTER;
            cdetails.Alignment = Element.ALIGN_CENTER;

            List instructions = new List(List.ORDERED, 10);

            iTextSharp.text.ListItem li_inst1 = new iTextSharp.text.ListItem(" This question paper is divided into two parts A and B.", fnt_inst);
            iTextSharp.text.ListItem li_inst2 = new iTextSharp.text.ListItem(" Part A contains <x> questions of <x> marks each. All questions are compulsary.", fnt_inst);
            iTextSharp.text.ListItem li_inst3 = new iTextSharp.text.ListItem(" Part B contains <x> questions of <x> marks each. In each question attempt either question (a) or (b), in case  both (a) and (b) questions are attempted for any question then only the first attempted question will be evaluated.", fnt_inst);
            iTextSharp.text.ListItem li_inst4 = new iTextSharp.text.ListItem(" Answer all questions in serial order.", fnt_inst);
            iTextSharp.text.ListItem li_inst5 = new iTextSharp.text.ListItem(" Do not write or mark anything on the question paper except your registration no. on the designated space.", fnt_inst);

            instructions.Add(li_inst1);
            instructions.Add(li_inst2);
            instructions.Add(li_inst3);
            instructions.Add(li_inst4);
            instructions.Add(li_inst5);

            Paragraph parta = new Paragraph("\nPART A\n");
            parta.Alignment = Element.ALIGN_CENTER;
            parta.Font.SetStyle(Font.BOLDITALIC);
            parta.Font.Size = 11;
            Paragraph partb = new Paragraph("\nPART B\n");
            partb.Alignment = Element.ALIGN_CENTER;
            partb.Font.SetStyle(Font.BOLDITALIC);
            partb.Font.Size = 11;

            Paragraph or = new Paragraph("OR\n");
            or.Font.SetStyle(Font.BOLDITALIC);
            or.Alignment = Element.ALIGN_CENTER;
            or.Font.Size = 11;
            or.SpacingAfter = 0;

            Font footerfont = FontFactory.GetFont("Helvetica", 9, Font.ITALIC, BaseColor.BLACK);
            Paragraph endMark = new Paragraph("\n-- End of Question Paper --", footerfont);
            endMark.Alignment = Element.ALIGN_CENTER;

            doc1.Add(rno);
            doc1.Add(pnr);
            doc1.Add(cdetails);
            doc1.Add(paperdetails);
            doc1.Add(instinst);
            doc1.Add(instructions);
            doc1.Add(parta);



            //one line limit = 80 chars

            ///////////short question - simple model
            PdfPTable shortques = new PdfPTable(2);
            shortques.WidthPercentage = 100;
            shortques.SpacingAfter = 2;

            float[] widths = new float[] { 8.85f, 1.15f };
            shortques.SetWidths(widths);

            Phrase sqno = new Phrase("Q<x> <y>)", boldquestionfont);

            Phrase sqst = new Phrase(" What are primate cities? Brief the same with suitable example. this is it. CAPS.", questionfont);

            Phrase sadder = new Phrase();
            sadder.Add(sqno);
            sadder.Add(sqst);

            Phrase sqm = new Phrase("[<marks>]", questionfont);

            PdfPCell sqstmt = new PdfPCell(sadder);
            sqstmt.BorderColor = BaseColor.WHITE;
            sqstmt.Padding = 0;
            PdfPCell smarks = new PdfPCell(sqm);
            smarks.BorderColor = BaseColor.WHITE;
            smarks.HorizontalAlignment = Element.ALIGN_RIGHT;
            smarks.Padding = 0;

            shortques.AddCell(sqstmt);
            shortques.AddCell(smarks);

            doc1.Add(shortques);
            /////[ENDS]//////short question - simple model 

            //////////short question - complex model
            Phrase sqphmarks = new Phrase("[<marks>]", questionfont);
            sqphmarks.SetLeading(0, 0);


            Phrase qno = new Phrase("<y>)", boldquestionfont);

            Phrase qst = new Phrase(" Explain this it rank size mass in detailExplain rank size mass in detail.Explain rank size mass insa df detail.Explain rank size mass in detail..Explain rank size mass in detail.Explain rank size mass in detail.Explain rank size mass in detail.", questionfont);

            Phrase adder = new Phrase();
            adder.Add(qno);
            adder.Add(qst);


            PdfPTable sqlongquestion = new PdfPTable(2);
            PdfPCell sqquesSTMT = new PdfPCell(adder);
            sqquesSTMT.Colspan = 2;
            sqquesSTMT.BorderColor = BaseColor.WHITE;
            sqquesSTMT.Padding = 0;

            PdfPCell sqmarks = new PdfPCell(sqphmarks);
            sqmarks.HorizontalAlignment = Element.ALIGN_RIGHT;
            sqmarks.BorderColor = BaseColor.WHITE;
            sqmarks.Padding = 0;
            PdfPCell sqkhali = new PdfPCell();
            sqkhali.BorderColor = BaseColor.WHITE;

            sqlongquestion.WidthPercentage = 100;


            sqlongquestion.AddCell(sqquesSTMT);
            sqlongquestion.AddCell(sqkhali);
            sqlongquestion.AddCell(sqmarks);

            doc1.Add(sqlongquestion);
            /////[ENDS]//////short question - complex model

            doc1.Add(partb);

            pasrtb(doc1);
            simplepasrtb(doc1);
            doc1.Add(or);
            simplepasrtb(doc1);

            doc1.Add(endMark);
            doc1.Close();

            //adding page numbers in the footer of each page in the pdf
            AddPageNumber(path + "/Doc1.pdf");

        }

        private void pasrtb(Document doc1)
        {
            Font questionfont = FontFactory.GetFont("Helvetica", 10);
            Font boldquestionfont = FontFactory.GetFont("Helvetica", 10, Font.BOLD, BaseColor.BLACK);

            Paragraph or = new Paragraph("OR\n");
            or.Font.SetStyle(Font.BOLDITALIC);
            or.Alignment = Element.ALIGN_CENTER;
            or.Font.Size = 11;
            or.SpacingAfter = 0;

            ///////////long question - complex model
            Phrase phmarks = new Phrase("[<marks>]", questionfont);
            phmarks.SetLeading(0, 0);

            Phrase lqno = new Phrase("Q<x> <y>)", boldquestionfont);

            Phrase lqst = new Phrase(" Explain this it rank size mass in detailExplain rank size mass in detail.Explain rank size mass insa df detail.Explain rank size mass in detail..Explain rank size mass in detail.Explain rank size mass in detail.Explain rank size mass in detail.", questionfont);

            Phrase ladder = new Phrase();
            ladder.Add(lqno);
            ladder.Add(lqst);


            PdfPTable longquestion = new PdfPTable(2);
            PdfPCell quesSTMT = new PdfPCell(ladder);
            quesSTMT.Colspan = 2;
            quesSTMT.BorderColor = BaseColor.WHITE;

            PdfPCell marks = new PdfPCell(phmarks);
            marks.HorizontalAlignment = Element.ALIGN_RIGHT;
            marks.BorderColor = BaseColor.WHITE;
            PdfPCell khali = new PdfPCell();
            khali.BorderColor = BaseColor.WHITE;

            longquestion.WidthPercentage = 100;


            longquestion.AddCell(quesSTMT);
            longquestion.AddCell(khali);
            longquestion.AddCell(marks);

            doc1.Add(longquestion);
            /////[ENDS]//////long question - complex model

            doc1.Add(or);
            ///////////long question - complex model
            Phrase dphmarks = new Phrase("[<marks>]", questionfont);
            dphmarks.SetLeading(0, 0);

            Phrase dlqno = new Phrase("Q<x> <y>)", boldquestionfont);

            Phrase dlqst = new Phrase(" Explain this it rank size mass in detailExplain rank size mass in detail.Explain rank size mass insa df detail.Explain rank size mass in detail..Explain rank size mass in detail.Explain rank size mass in detail.Explain rank size mass in detail.", questionfont);

            Phrase dladder = new Phrase();
            dladder.Add(dlqno);
            dladder.Add(dlqst);


            PdfPTable dlongquestion = new PdfPTable(2);
            PdfPCell dquesSTMT = new PdfPCell(dladder);
            dquesSTMT.Colspan = 2;
            dquesSTMT.BorderColor = BaseColor.WHITE;

            PdfPCell dmarks = new PdfPCell(phmarks);
            dmarks.HorizontalAlignment = Element.ALIGN_RIGHT;
            dmarks.BorderColor = BaseColor.WHITE;
            PdfPCell dkhali = new PdfPCell();
            dkhali.BorderColor = BaseColor.WHITE;

            dlongquestion.WidthPercentage = 100;


            dlongquestion.AddCell(quesSTMT);
            dlongquestion.AddCell(khali);
            dlongquestion.AddCell(marks);

            doc1.Add(dlongquestion);
            /////[ENDS]//////long question - complex model

        }

        private void simplepasrtb(Document doc1)
        {
            Font questionfont = FontFactory.GetFont("Helvetica", 10);
            Font boldquestionfont = FontFactory.GetFont("Helvetica", 10, Font.BOLD, BaseColor.BLACK);

            Paragraph or = new Paragraph("OR\n");
            or.Font.SetStyle(Font.BOLDITALIC);
            or.Alignment = Element.ALIGN_CENTER;
            or.Font.Size = 11;
            or.SpacingAfter = 0;

            ///////////short question - simple model
            PdfPTable shortques = new PdfPTable(2);
            shortques.WidthPercentage = 100;
            shortques.SpacingAfter = 2;

            float[] widths = new float[] { 8.85f, 1.15f };
            shortques.SetWidths(widths);

            Phrase sqno = new Phrase("Q<x> <y>)", boldquestionfont);

            Phrase sqst = new Phrase(" What are primate cities? Brief the same with suitable example. this is it. CAPS.", questionfont);

            Phrase sadder = new Phrase();
            sadder.Add(sqno);
            sadder.Add(sqst);

            Phrase sqm = new Phrase("[<marks>]", questionfont);

            PdfPCell sqstmt = new PdfPCell(sadder);
            sqstmt.BorderColor = BaseColor.WHITE;
            sqstmt.Padding = 0;
            PdfPCell smarks = new PdfPCell(sqm);
            smarks.BorderColor = BaseColor.WHITE;
            smarks.HorizontalAlignment = Element.ALIGN_RIGHT;
            smarks.Padding = 0;

            shortques.AddCell(sqstmt);
            shortques.AddCell(smarks);

            doc1.Add(shortques);
            /////[ENDS]//////short question - simple model 

        }

         */
    }
}