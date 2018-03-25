using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Diagnostics;

namespace DataCollection
{
    public class AClassUtitlites
    {
        static string ConnectionString = ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString;
        static TimeZoneInfo IST = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

        public string getPaperSource(string PNR)
        {
            if(PNR.Length>17)
            {
                //is ca paper
                string CAccode = string.Empty, returner = string.Empty;
                int? canum;
                string CAreturner=string.Empty;

                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    SqlCommand cmd = new SqlCommand("Select CourseCode from CApapers where pnr='" + PNR + "'" + " union select CourseCode from hardtypedpapers where pnr='" + PNR + "'", con);
                    con.Open();
                    CAccode = cmd.ExecuteScalar().ToString();
                    cmd.CommandText = "Select CAnum from HardTypedPapers where pnr = '" + PNR + "'" +"union select CAnumber from CAPapers where pnr = '" + PNR + "'";
                    canum = Convert.ToInt32(cmd.ExecuteScalar().ToString());
                }

                CAreturner = canum==null||canum==0? "CA":canum<20?"CA-"+canum:canum==999?"ETP":"MTP";

                CAreturner = CAccode + " " + CAreturner;
                return CAreturner;
            }

            string termID = PNR.Substring(1, 5);
            string Month = string.Empty, ExamType = string.Empty, Reappear = string.Empty, year = string.Empty, CourseCode = string.Empty;


            if (PNR.Length == 10)
            {
                termID = PNR.Substring(0, 5);
            }

            int? MaxMarks;

            //determining Exam type (ETE,MTE) - only non hard papers
            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                SqlCommand cmd = new SqlCommand("Select maxmarks from papers where pnr='" + PNR + "'"+" union select maxmarks from hardtypedpapers where pnr='" + PNR + "'", con);
                con.Open();
                MaxMarks = Convert.ToInt32(cmd.ExecuteScalar().ToString());
            }

            ExamType = MaxMarks > 50 ? "ETE" : "MTE";

            if (termID.StartsWith("99"))
            {//if PNRless paper
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    SqlCommand cmd = new SqlCommand("Select CourseCode from papers where pnr='" + PNR + "'" + " union select CourseCode from hardtypedpapers where pnr='" + PNR + "'", con);
                    con.Open();
                    CourseCode = cmd.ExecuteScalar().ToString();
                }
                return CourseCode.ToUpper() + " " + ExamType + " - PNRless";
            }

            /*
             * 600s done with determining year and month
             * 500s done with determining session(2013-14) and reappear status
             * both determine if paper was ETE/MTE
             */

            switch (getPNRpattern(PNR).Substring(0,2))
            {
                case "63":
                    Month = PNR.Substring(5,1)=="1"?ExamType=="MTE"?"Oct":"Dec":PNR.Substring(5,1)=="2"? ExamType == "MTE" ? "March" : "May":"Default";
                    year = PNR.Substring(5, 1) == "1" ? termID.Substring(0, 2) : termID.Substring(2, 2);
                    year = "'"+ year;
                    //modern style paper
                    break;
                    
                case "54":
                    //Month = "Default";
                    Reappear = "[Re-Appear]";
                    year = "20"+termID.Substring(0, 2) +"-"+ termID.Substring(2, 2);
                    //modern style paper
                    break;
                    
                case "51":
                    //modern style paper
                    year = "20"+termID.Substring(0, 2) +"-"+ termID.Substring(2, 2);
                    break;

                default:
                    using (SqlConnection con = new SqlConnection(ConnectionString))
                    {
                        SqlCommand cmd = new SqlCommand("Select CourseCode from papers where pnr='" + PNR + "'" + " union select CourseCode from hardtypedpapers where pnr='" + PNR + "'", con);
                        con.Open();
                        CourseCode = cmd.ExecuteScalar().ToString();
                    }
                    Month = CourseCode.ToUpper() + " " + " - Unidentifiable";
                    break;
            }
            return ExamType+" "+ Month+" "+ year+" "+Reappear;
        }

        #region PNR pattern fetch
        /*
         * The following function gives the PNR pattern type in the following formats
         * 
creating dynamic A-Class paper source getter...
types of PNRs:
635 .... modern paper style
514 .... very old [2011-2012]
545 .... medium old, before mod
633 .... 
634 ....
544 ....
543 ....
         */

        protected string getPNRpattern(string PNR)
        {
            int first = 0, second = 0, third = 0;
            bool isFirstHalf = true;
            foreach (char c in PNR)
            {
                Debug.WriteLine("Iteration insider");
                try
                {
                    Convert.ToInt32(c.ToString());
                    if (isFirstHalf)
                        first++;
                    else
                        third++;
                }
                catch (FormatException)
                {
                    //Debug.WriteLine("caught one");
                    second++;
                    isFirstHalf = false;
                }
            }
            //Debug.WriteLine("First = " + first + " second = " + second+" third = "+third);
            return first.ToString() + second.ToString() + third.ToString();
        }

        #endregion
    }
}