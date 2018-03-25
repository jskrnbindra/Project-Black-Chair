using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;

namespace DataCollection
{
    public partial class QuestionsPDF : System.Web.UI.Page
    {
        public static int QuestionsType;
        public static string CourseCode;
        public static bool IncludeCA;
        public static List<string> RepeatedQuestionOnceEntered = new List<string>();

        static Font Topsmall = FontFactory.GetFont("Helvetica", 9, Font.BOLD, BaseColor.BLACK);
        Font small = FontFactory.GetFont("Helvetica", 9);
        Font TopHeader = FontFactory.GetFont("Helvetica", 12, Font.BOLD, BaseColor.BLACK);
        Font TopHeader17 = FontFactory.GetFont("Helvetica", 17, Font.BOLD, BaseColor.BLACK);
        Font fnt_inst = FontFactory.GetFont("Helvetica", 9, Font.ITALIC);
        Font questionfont = FontFactory.GetFont("Helvetica", 10);
        Font questionfontXtractr = FontFactory.GetFont("Helvetica", 12);
        Font boldquestionfont = FontFactory.GetFont("Helvetica", 10, Font.BOLD, BaseColor.BLACK);
        Font boldquestionfontLINK = FontFactory.GetFont("Helvetica", 10, Font.UNDERLINE, BaseColor.BLUE);
        Font boldquestionfont12 = FontFactory.GetFont("Helvetica", 12, Font.BOLD, BaseColor.BLACK);
        Font footerfont = FontFactory.GetFont("Helvetica", 9, Font.ITALIC, BaseColor.BLACK);
        Font diagramLinks = FontFactory.GetFont("Helvetica", 8, Font.UNDERLINE, BaseColor.BLUE);
        Font papercodebold = FontFactory.GetFont("Helvetica", 17, Font.BOLD, BaseColor.BLACK);

        protected void Page_Load(object sender, EventArgs e)
        {
            RepeatedQuestionOnceEntered.Clear();

            //here we generate PDF and redirect to the PDF
            QuestionsType = Convert.ToInt32(Request.QueryString["QuestionType"].ToString());
            CourseCode = Request.QueryString["CourseCode"].ToString();
            IncludeCA = Request.QueryString["IncludeCA"].ToString() == "Y" ? true : false;

            //Label1.Text = QuestionsType + " oh " + CourseCode + " oh " + IncludeCA.ToString();

            generateQuestionsPDF();

            Response.Redirect("PDFs/Questions.pdf");
        }

        protected void generateQuestionsPDF()
        {
            DataTable WorkTable = getDataTableForQuestions(QuestionsType, CourseCode, IncludeCA);

            int RepetitionsCount = getRepetitionsCount(WorkTable);

            string strQtype = string.Empty;
            switch (QuestionsType)
            {
                case 1:
                    strQtype = "Very short/MCQ Qestions [1 Mark]";
                    break;
                case 2:
                    strQtype = "Short Questions [2-2.5 Marks]";
                    break;
                case 5:
                    strQtype = "Short Questions [5 Marks]";
                    break;
                case 10:
                    strQtype = "Long Questions [10 Marks]";
                    break;
                case 15:
                    strQtype = "Long Questions [15 Marks]";
                    break;
                default:
                    break;
            }

            var ExtractedQuestionsPDF = new Document(PageSize.A4, 30, 20, 50, 50);
            string SavePath = Server.MapPath("PDFs");
            PdfWriter.GetInstance(ExtractedQuestionsPDF, new FileStream(SavePath + "/Questions.pdf", FileMode.Create));

            ExtractedQuestionsPDF.Open();
            //addding header
            Paragraph header = new Paragraph(strQtype, TopHeader);
            header.Alignment = Element.ALIGN_CENTER;

            ExtractedQuestionsPDF.Add(header);
            //END//addding header

            if (CourseCode != "N")
            {
                Paragraph CCode = new Paragraph(CourseCode, TopHeader);
                CCode.Alignment = Element.ALIGN_CENTER;
                ExtractedQuestionsPDF.Add(CCode);
            }
            ///data set stats
            DataView view = new DataView(WorkTable);
            int qCount = view.ToTable(true,new string[] { "Statement" }).Rows.Count;
            //int qCount = WorkTable.Rows.Count;///200 questions found from 33 papers

            DataView dv = new DataView(WorkTable);

            int pCount = dv.ToTable(true, "PNR").Rows.Count;

            string QuestionsGrammarCorrector = qCount > 1 ? "questions" : "question";
            string PapersGrammarCorrector = pCount > 1 ? "papers" : "paper";

            //int repcount = getRepetitionsCount(WorkTable);

            string RepsJoiner = string.Empty;

            if (RepetitionsCount > 1)
                RepsJoiner = " with " + RepetitionsCount.ToString() + " repeated questions";
            else if(RepetitionsCount == 1)
                RepsJoiner = " with " + RepetitionsCount.ToString() + " repeated question";

            Paragraph DataSetstats = new Paragraph("Showing " + qCount + " " + QuestionsGrammarCorrector + " from " + pCount + " " + PapersGrammarCorrector+RepsJoiner, Topsmall);
            DataSetstats.Alignment = Element.ALIGN_CENTER;

            ExtractedQuestionsPDF.Add(DataSetstats);
            //END/data set stats

            //// adding "Questions" header
            Paragraph QuestionsHeader = new Paragraph("Questions", TopHeader17);
            QuestionsHeader.Alignment = Element.ALIGN_CENTER;

            ExtractedQuestionsPDF.Add(new Paragraph("\n"));

            ExtractedQuestionsPDF.Add(QuestionsHeader);
            ExtractedQuestionsPDF.Add(new Paragraph("\n", small));
            //ExtractedQuestionsPDF.Add(new Paragraph("\n"));//spacemaker

            //END// adding "Questions" header
            /*
            ////CA indicator
            PdfPCell CAindicator = new PdfPCell(new Phrase("CA", Topsmall));
            CAindicator.HorizontalAlignment = Element.ALIGN_CENTER;
            //END//CA indicator
            */
            ////Real-time Question Table
            PdfPTable QuestionTable = new PdfPTable(4);//Numbering &Statement + Weightage + CA/MTE/ETE + RepetedSTAR
            QuestionTable.WidthPercentage = 100;
            QuestionTable.SetWidths(new float[] { .79f, .06f, .12f, .03f });

            PdfPCell cell_QuestionHolder = new PdfPCell();

            PDFtester PDFfuncs = new PDFtester();//getting custom PDF functions class

            int QuestionNumber = 1;

            for (int c = 0; c < qCount; c++)//rendering questions from Datatable to PDF///CAN BE OPTIMIZED MORE
            {
                DataRow question = WorkTable.Rows[c];

                QuestionTable.FlushContent();
                ////Adding question statement
                if (question["Option1"].ToString() == "")//is not MCQ
                    cell_QuestionHolder = PDFfuncs.convertTableToCell(renderNonMCQQuestion(question, QuestionNumber++));
                else
                    cell_QuestionHolder = PDFfuncs.convertTableToCell(PDFfuncs.renderMCQQuestion(question, PDFfuncs.detectQuestionPattern(question), QuestionNumber++));

                //QuestionTable.AddCell(cell_QuestionHolder);////PICKUP////
                //END//Adding question statement

                ////Adding Weightage
                //QuestionTable.AddCell(PDFfuncs.removeBorders(new PdfPCell(new Phrase("[" + PDFfuncs.formatMarks(question["Weightage"].ToString()) + "]", questionfont))));////PICKUP////
                //END//Adding Weightage

                ////Adding QuestionPaper type
                Anchor PaperLinker = new Anchor(new Phrase(getQuestionSourcePaper(question["PNR"].ToString()), boldquestionfontLINK));
                //PaperLinker.Reference = "http://localhost:50876/ViewQuestionPaper.aspx?p=" + new QueryStringEncryption().encryptQueryString(question["PNR"].ToString());//for TEST environment only
                PaperLinker.Reference = "http://blackchair.manhardeep.com/ViewQuestionPaper.aspx?p=" + new QueryStringEncryption().encryptQueryString(question["PNR"].ToString());//for PRODUCTION environment only
                //QuestionTable.AddCell(PDFfuncs.removeBorders(new PdfPCell(PaperLinker)));////PICKUP////
                //END//Adding QuestionPaper type

                ////Adding Repetitions thingy
                if (isRepeatedQuestion(question["QuestionID"].ToString()))
                {
                    if (!RepeatedQuestionOnceEntered.Contains(question["Statement"].ToString()))//Add to PDF only if not in pdf already
                    {
                        RepeatedQuestionOnceEntered.Add(question["Statement"].ToString());

                        iTextSharp.text.Image repIcon = iTextSharp.text.Image.GetInstance(Server.MapPath("Icons") + "/repIcon.png");

                        Chunk chnk_RepeatedIcon = new Chunk(repIcon, 0, 0, false);
                        Anchor repImage = new Anchor(chnk_RepeatedIcon);
                        QueryStringEncryption Ecryptor = new QueryStringEncryption();
                        //repImage.Reference = "http://localhost:50876/RepetitionsShower.aspx?QuestionID=" + Ecryptor.encryptQueryString(question["QuestionID"].ToString()) + "&sl=" + giveRepetitionsHyperlinkURL(question["Statement"].ToString()) + "&p=" + Ecryptor.encryptQueryString(question["PNR"].ToString());
                        repImage.Reference = "http://blackchair.manhardeep.com/RepetitionsShower.aspx?QuestionID=" + Ecryptor.encryptQueryString(question["QuestionID"].ToString()) + "&sl=" + giveRepetitionsHyperlinkURL(question["Statement"].ToString()) + "&p=" + Ecryptor.encryptQueryString(question["PNR"].ToString());

                        PdfPCell cell_ImgREP = new PdfPCell(repImage);
                        cell_ImgREP.VerticalAlignment = Element.ALIGN_BOTTOM;
                        repIcon.ScaleAbsolute(30f, 30f);

                        QuestionTable.AddCell(cell_QuestionHolder);////PICKUP////
                        QuestionTable.AddCell(PDFfuncs.removeBorders(new PdfPCell(new Phrase("[" + PDFfuncs.formatMarks(question["Weightage"].ToString()) + "]", questionfont))));////PICKUP////
                        QuestionTable.AddCell(PDFfuncs.removeBorders(new PdfPCell(PaperLinker)));////PICKUP////
                        QuestionTable.AddCell(PDFfuncs.removeBorders(new PdfPCell(cell_ImgREP)));
                        
                        ExtractedQuestionsPDF.Add(QuestionTable);
                    }
                    else
                    {
                        QuestionNumber--;
                        qCount++;
                        System.Diagnostics.Debug.WriteLine("in already in reps");
                        //QuestionTable.FlushContent();
                    }
                }
                else//if not repeated question
                {
                    QuestionTable.AddCell(cell_QuestionHolder);////PICKUP////
                    QuestionTable.AddCell(PDFfuncs.removeBorders(new PdfPCell(new Phrase("[" + PDFfuncs.formatMarks(question["Weightage"].ToString()) + "]", questionfont))));////PICKUP////
                    QuestionTable.AddCell(PDFfuncs.removeBorders(new PdfPCell(PaperLinker)));////PICKUP////
                    QuestionTable.AddCell(PDFfuncs.removeBorders(new PdfPCell()));
                    ExtractedQuestionsPDF.Add(QuestionTable);
                }
                //END//Adding Repetitions thingy

                ExtractedQuestionsPDF.Add(new Paragraph("\n"));
            }

            ExtractedQuestionsPDF.Close();
            PDFfuncs.AddPageNumber(Server.MapPath("PDFs") + "/Questions.pdf");
        }

        protected DataTable getDataTableForQuestions(int QuestionType, string CourseCode, bool IncludeCA)
        {
            DataTable Returner = null;
            QuestionsExtraction QE = new QuestionsExtraction();
            if (CourseCode == "N")
                Returner = QE.extractQuestions(QuestionType, IncludeCA);
            else
                Returner = QE.extractQuestions(QuestionType, CourseCode, IncludeCA);
            System.Diagnostics.Debug.WriteLine("in get get get get" + Returner.Rows.Count);
            return Returner;
        }

        protected PdfPTable renderNonMCQQuestion(DataRow question, int DTQuestionNumber)
        {
            ///Renders all short and long questions indiscriminately
            //Gives back the Qno + statement table

            PdfPTable Returner = new PdfPTable(1);
            Returner.WidthPercentage = 100;
            //Returner.SetWidths(new float[] { .05f, .95f });

            PDFtester PDFfuncs = new PDFtester();

            Phrase ph_Numbering = new Phrase(DTQuestionNumber + ". ", boldquestionfont12);
            Phrase ph_Statement = new Phrase(question["Statement"].ToString(), questionfontXtractr);

            ph_Numbering.Add(ph_Statement);

            PdfPCell cell_Question = new PdfPCell(ph_Numbering);

            PdfPCell cell_Diagram = null;

            if (question["Diagram"].ToString() != "")
            {
                iTextSharp.text.Image diagramImage = iTextSharp.text.Image.GetInstance(Server.MapPath("Diagrams") + "/" + PDFfuncs.giveImageName(question["Diagram"].ToString()));
                diagramImage.ScaleToFit(new Rectangle(0f, 0f, 100f, 100f));

                PdfPTable DiagramHolder = new PdfPTable(1);
                DiagramHolder.WidthPercentage = 100;

                PdfPCell cell_ImgHolder = new PdfPCell(diagramImage);

                DiagramHolder.AddCell(PDFfuncs.removeBorders(cell_ImgHolder));
                DiagramHolder.AddCell(PDFfuncs.addViewFullSizeLink(question["Diagram"].ToString()));

                PdfPCell cell_dHolder = new PdfPCell(DiagramHolder);
                cell_dHolder.Colspan = 2;

                cell_Diagram = PDFfuncs.removeBorders(cell_dHolder);
            }
            else cell_Diagram = new PdfPCell();//if no diagram

            Returner.AddCell(PDFfuncs.removeBorders(cell_Question));
            Returner.AddCell(PDFfuncs.removeBorders(cell_Diagram));

            return Returner;
        }

        public string getQuestionSourcePaper(string PNR)
        {
            if (PNR.Length > 17)//is CA
            {
                int? CaNum;
                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
                {
                    SqlCommand cmd = new SqlCommand("Select canumber from capapers where pnr='" + PNR + "'", con);
                    con.Open();
                    try
                    {
                        CaNum = Convert.ToInt32(cmd.ExecuteScalar().ToString());//either CA num or 0 or 998/999
                    }
                    catch (NullReferenceException)//if not in CApapers check in hard papers
                    {
                        cmd.CommandText = "Select canum from HardTypedPapers where pnr='" + PNR + "'";
                        CaNum = Convert.ToInt32(cmd.ExecuteScalar().ToString());//either CA num or 0 or 998/999
                    }
                }
                if (CaNum == 999) return "ETP";
                else if (CaNum == 998) return "MTP";
                else return CaNum == 0 ? "CA" : "CA-" + CaNum;
            }
            else//is not CA
            {
                int? MaxMarks; string termid = string.Empty;

                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
                {
                    SqlCommand cmd = new SqlCommand("Select maxmarks from papers where pnr='" + PNR + "'", con);
                    con.Open();

                    MaxMarks = Convert.ToInt32(cmd.ExecuteScalar().ToString());
                }

                termid = PNR.Substring(0, 6);

                if (termid.StartsWith("99"))
                    return MaxMarks > 50 ? "ETE" : "MTE";
                else
                {
                    try
                    {
                        termid = termid.Substring(1);
                    int term = 0;
                    string month;
                    try
                    {
                        term = Convert.ToInt32(termid.Substring(4));
                        // System.Diagnostics.Debug.WriteLine("=================TERM======================" + term);
                    }
                    catch (Exception)
                    {
                        ////term id (1 or 2) not available
                        return MaxMarks > 50 ? "ETE" : "MTE";
                    }
                    //                      ETE Dec '16
                    string examtype = MaxMarks > 50 ? "ETE" : "MTE";
                    month = term == 1 ? MaxMarks > 50 ? "Dec" : "Oct" : MaxMarks > 50 ? "May" : "Mar";
                    // System.Diagnostics.Debug.WriteLine("=================TERM======================ID" + termid);

                    int year = 0;

                    if (term == 1)
                        year = Convert.ToInt32(termid.Substring(0, 2));
                    else
                        year = Convert.ToInt32(termid.Substring(0, 2)) + 1;

                    string returner = examtype + " " + month + " '" + year;

                    return returner;
                }
                    catch(NullReferenceException)
                    {
                        using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
                        {
                            SqlCommand cmd = new SqlCommand("Select CourseCode from HardTypedPapers where pnr='" + PNR + "'", con);
                            con.Open();
                            try
                            {
                                return cmd.ExecuteScalar().ToString();
                            }
                            catch
                            {
                                throw new Exception("No papers found for this Course Code"); 
                            }
                        }
                    }
            }
                
            }
        }

        protected bool isRepeatedQuestion(string QuestionID)
        {
            int Repetitions = 0;
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                SqlCommand cmd = new SqlCommand("Select count(questionid) from RepetitionsKeeper where QuestionID='" + QuestionID + "'", con);
                con.Open();
                Repetitions = Convert.ToInt32(cmd.ExecuteScalar().ToString());//either CA num or 0 or 998/999
            }
            return Repetitions > 0 ? true : false; //later return array of PNRs
        }

        protected string giveRepetitionsHyperlinkURL(string Statement)
        {
            //extracted the repeated question's PNR

            DataTable SerialsList = null;

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                SqlCommand cmd = new SqlCommand("getRepeatedSerialsList", con);
                cmd.CommandType = CommandType.StoredProcedure;

                SqlParameter param_statement = new SqlParameter("@Statement", SqlDbType.VarChar);
                cmd.Parameters.AddWithValue("@statement", Statement);

                SqlDataAdapter ada = new SqlDataAdapter(cmd);

                SerialsList = new DataTable();

                con.Open();
                ada.Fill(SerialsList);
            }

            string returner = string.Empty;

            if (SerialsList.Rows.Count == 0) return "NULL";//only in case of some error(DB updations like deletion) this LOC gets executed
            for (int c = 0; c < SerialsList.Rows.Count; c++)
            {
                DataRow serial = SerialsList.Rows[c];
                returner += serial["Serial"].ToString() + "s";
            }
            return returner.Trim('s');
        }

        private int getRepetitionsCount(DataTable WorkTable)
        {
            int returner = 0;
            List<string> Repetitions_Temp_Buffer = new List<string>();

            for(int c = 0;c<WorkTable.Rows.Count;c++)
            {
                DataRow question = WorkTable.Rows[c];
                if(isRepeatedQuestion(question["QuestionID"].ToString()))
                {
                    if(!Repetitions_Temp_Buffer.Contains(question["Statement"].ToString()))
                    {
                        Repetitions_Temp_Buffer.Add(question["Statement"].ToString());
                        returner++;
                    }
                    
                }
            }
            return returner;
        }

        /*
         * REDUNDANT will send reps count in query string or sth .. this is complicating things
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
        */
    }


    /*
    public class PDFformatter : PdfPageEventHelper
    {
        PdfContentByte ContentBytes;
        PdfTemplate template;

        private string rightHeadText;

        BaseFont bf = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1250, BaseFont.NOT_EMBEDDED);

        #region property
        public string TopRightText
        {
            get { return rightHeadText; }
            set { rightHeadText = value; }
        }
        #endregion

        public override void OnOpenDocument(PdfWriter writer, Document document)
        {
            ContentBytes = writer.DirectContent;
            template = ContentBytes.CreateTemplate(100, 100);
        }

        public override void OnEndPage(PdfWriter writer, Document document)
        {
            base.OnEndPage(writer, document);


            ContentBytes.BeginText();
            ContentBytes.SetFontAndSize(bf, 12);
            ContentBytes.SetTextMatrix(document.PageSize.GetRight(100),document.PageSize.GetTop(10));
            ContentBytes.ShowText("Here freaking");
            ContentBytes.EndText();

            ContentBytes.AddTemplate(template,document.PageSize.GetRight(100),document.GetRight(20));
        }

        public override void OnCloseDocument(PdfWriter writer, Document document)
        {
            base.OnCloseDocument(writer, document);

            template.BeginText();
            template.SetFontAndSize(bf, 12);
            template.SetTextMatrix(0, 0);
            template.ShowText("thsi");
            template.EndText();
        }

    }
    */

}