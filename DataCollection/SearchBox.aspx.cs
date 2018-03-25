using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.Services;

namespace DataCollection
{
    public partial class WebForm1 : System.Web.UI.Page
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
        public static string[] getCourseNameSearchResults(string CourseName)
        {
            using (SqlConnection con_SearchByCourseName = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                using (SqlCommand cmd_getCourseCodeFromCourseName = new SqlCommand("getCourseCodeFromCourseName", con_SearchByCourseName))
                {
                    cmd_getCourseCodeFromCourseName.CommandType = CommandType.StoredProcedure;

                    SqlParameter param_CourseName = new SqlParameter("@CourseName",SqlDbType.NVarChar);
                    param_CourseName.Value = CourseName;

                    cmd_getCourseCodeFromCourseName.Parameters.Add(param_CourseName);

                    con_SearchByCourseName.Open();
                    var result = cmd_getCourseCodeFromCourseName.ExecuteScalar();
                    con_SearchByCourseName.Close();

                    string CourseCode = result == null ? null : result.ToString();

                    if(CourseCode==null)
                    {
                        string [] EmptyReturner = new string[2]; 

                        EmptyReturner[0] = "{\"NumberOfPapersFound\": 0, \"Message\":\"No papers Found for this Course. \",\"Papers\":[]}";
                        EmptyReturner[1] = "{\"NumberOfPapersFound\": 0, \"Message\":\"No papers Found for this Course. \",\"Papers\":[]}";

                        return EmptyReturner;
                    }
                    else
                    {
                        return givePapersFromSearchQuery(CourseCode);
                    }
                }
            }
        }

        [WebMethod]
        public static string getAutocompleteSuggestion(string prefix)
        {
            Debug.WriteLine("data is : " + prefix);
            using (SqlConnection con_AutoCompleteCon = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                using (SqlCommand cmd_AutoCompleteSuggestor = new SqlCommand("GiveAutocompleteCourseNameSuggestion", con_AutoCompleteCon))
                {
                    SqlParameter param_IncompleteCourseName = new SqlParameter("@IncompleteCourseName", SqlDbType.NVarChar);
                    param_IncompleteCourseName.Value = prefix;

                    cmd_AutoCompleteSuggestor.CommandType = CommandType.StoredProcedure;
                    cmd_AutoCompleteSuggestor.Parameters.Add(param_IncompleteCourseName);

                    con_AutoCompleteCon.Open();

                    var Result = cmd_AutoCompleteSuggestor.ExecuteScalar()??"none";

                    return Result.ToString();
                }
            }
        }

        [WebMethod]
        public static string[] givePapersFromSearchQuery(string UserSearchQuery)
        {
            string[] SearchResultsWithRecommendations = new string[2];
          
            #region Validate Input
            if (UserSearchQuery.Length < 1)
            {
                Debug.WriteLine("Empty Input");
                string emptyResponse = "{\"NumberOfPapersFound\": 0, \"Message\":\"Empty Input\", \"Papers\":[]}";

                SearchResultsWithRecommendations[0] = emptyResponse;
                SearchResultsWithRecommendations[1] = emptyResponse;

                return SearchResultsWithRecommendations;
            }

            string[] words = UserSearchQuery.Split(' ');
            int WordsCounter = 0;
            bool flag_WordLimitExceed = false;
            foreach(string word in words)
            {
                if(WordsCounter>20) { flag_WordLimitExceed = true;break; }
                if (word != "") WordsCounter++;
            }
            if(flag_WordLimitExceed)
            {
                Debug.WriteLine("Word limit Exceeded");
                string emptyResponse = "{\"NumberOfPapersFound\": 0, \"Message\":\"Word limit Exceeded\", \"Papers\":[]}";

                SearchResultsWithRecommendations[0] = emptyResponse;
                SearchResultsWithRecommendations[1] = emptyResponse;

                return SearchResultsWithRecommendations;
            }
            #endregion

            #region Understand User Search Query
            string UserSearchText = UserSearchQuery.ToUpper() + " ";

            string PapersInJSON = string.Empty;

            Regex rx_CourseCodeExtractor = new Regex(@"[a-zA-Z]{3}(\s{1,})?\d{3}([a-zA-Z]{1})?", RegexOptions.Compiled);
            Regex rx_PaperTypeExtractor = new Regex(@"(MTE|mte|ETE|ete|MTP|mtp|ETP|etp|(\s?(ca|CA)\s{0,}-\s{0,}\d{1})|(\s?(ca|CA)\s{0,}\d)|(\s?ca\s{1,})|(\s?CA\s{1,})|(\s?re\s{1,})|(\s?RE\s{1,})|reappear|REAPPEAR)", RegexOptions.Compiled);
            Regex rx_CAnumberExtractor = new Regex(@"\d{1,2}", RegexOptions.Compiled);

            MatchCollection mc_CourseCodes = rx_CourseCodeExtractor.Matches(UserSearchText);
            MatchCollection mc_PaperTypes = rx_PaperTypeExtractor.Matches(UserSearchText);

            

            if (mc_CourseCodes.Count == 0 && mc_PaperTypes.Count == 0)//no course code and Paper type in User Search Query
            {
                Debug.WriteLine("No CourseCode or papertype found");
                string emptyResponse = "{\"NumberOfPapersFound\": 0, \"Message\":\"No CourseCode or Papertype found\", \"Papers\":[]}";
                
                SearchResultsWithRecommendations[0] = emptyResponse;
                SearchResultsWithRecommendations[1] = emptyResponse;

                //Response.Write(emptyResponse);
                return SearchResultsWithRecommendations;
            }

            bool CourseCodeAsked, MTEAsked, ETEAsked, CAAsked, REAPPEARAsked, MTPAsked, ETPAsked;

            MTEAsked = false;
            ETEAsked = false;
            CAAsked = false;
            REAPPEARAsked = false;
            MTPAsked = false;
            ETPAsked = false;

            CourseCodeAsked = mc_CourseCodes.Count == 0 ? false : true;

            List<string> CAs = new List<string>();
            List<int> CANumbers = new List<int>();

            HashSet<string> FormattedPaperTypesAsked = new HashSet<string>();


            foreach (Match match in mc_PaperTypes)
            {
                if (match.ToString().ToUpper().Contains("CA"))
                {
                    //       Debug.WriteLine(match.ToString() + " <-- Match");
                    CAAsked = true;
                    CAs.Add(match.ToString().ToUpper());
                    //         Debug.WriteLine(rx_CAnumberExtractor.Match(match.ToString()).ToString() + " <-- rx_CAnumberExtractor.Match(match.ToString()).ToString())");
                    try
                    {
                        CANumbers.Add(Convert.ToInt32(rx_CAnumberExtractor.Match(match.ToString()).ToString()));
                    }
                    catch (FormatException)
                    {
                        //if a ca has no number control comes here. Do nothing.
                    }
                }

                //if once value becomes true, stays true
                MTEAsked = match.ToString().ToUpper().Contains("MTE") || MTEAsked == true ? true : false;
                ETEAsked = match.ToString().ToUpper().Contains("ETE") || ETEAsked == true ? true : false;
                REAPPEARAsked = match.ToString().ToUpper().Contains("RE") || REAPPEARAsked == true ? true : false;
                MTPAsked = match.ToString().ToUpper().Contains("MTP") || MTPAsked == true ? true : false;
                ETPAsked = match.ToString().ToUpper().Contains("ETP") || ETPAsked == true ? true : false;


                //BEGIN//end adding to hashset
                if (MTEAsked) FormattedPaperTypesAsked.Add("MTE");
                if (ETEAsked) FormattedPaperTypesAsked.Add("ETE");
                if (REAPPEARAsked) FormattedPaperTypesAsked.Add("REAPPEAR");
                if (MTPAsked) FormattedPaperTypesAsked.Add("MTP");
                if (ETPAsked) FormattedPaperTypesAsked.Add("ETP");
                if (CAAsked && CANumbers.Count == 0)
                {
                    FormattedPaperTypesAsked.Add("CA");
                }
                else if (CAAsked && CANumbers.Count > 0)
                {
                    foreach (int num in CANumbers)
                        FormattedPaperTypesAsked.Add("CA" + num);
                }
                //END// adding to hashset
            }





            //   List<string> CourseCodesAsked = new List<string>();
            HashSet<string> CourseCodesAsked = new HashSet<string>();

            foreach (Match m in mc_CourseCodes)
                CourseCodesAsked.Add(formatCourseCode(m.ToString()));

            /*result of this function in the following HashSets<string>
             * 
             * CourseCodesAsked 
             * FormattedPaperTypesAsked
             * 
             * Passing the info to data fetcher function
             */

            #endregion

            PapersInJSON = fetchPapersAsJSON(CourseCodesAsked, FormattedPaperTypesAsked, false);

            #region Get Easy Recommendations
            string CSV_PNRsInStrictSearchResults = fetchPapersAsJSON(CourseCodesAsked, FormattedPaperTypesAsked, true);

            List<string> List_PNRsInStrictSearch;

            Debug.WriteLine(CSV_PNRsInStrictSearchResults + "<--RecommendationsInJSON");

            if (CSV_PNRsInStrictSearchResults == string.Empty)
                List_PNRsInStrictSearch = null;
            else
            {
                CSV_PNRsInStrictSearchResults = CSV_PNRsInStrictSearchResults.Last<char>() == ',' ? CSV_PNRsInStrictSearchResults.Substring(0, CSV_PNRsInStrictSearchResults.Length - 1) : CSV_PNRsInStrictSearchResults;
                Debug.WriteLine(CSV_PNRsInStrictSearchResults + "<--RecommendationsInJSON");
                List_PNRsInStrictSearch = CSV_PNRsInStrictSearchResults.Split(',').ToList<string>();
            }
            #endregion

            string RecommendationsInJSON = giveRecommendationsFor(CourseCodesAsked, FormattedPaperTypesAsked, List_PNRsInStrictSearch);

            //for debugging purpose
            Debug.WriteLine("Recommendations>>");
            Debug.WriteLine(RecommendationsInJSON);
            Debug.WriteLine("<<");

            SearchResultsWithRecommendations[0] = PapersInJSON;
            SearchResultsWithRecommendations[1] = RecommendationsInJSON;

            return SearchResultsWithRecommendations;
        }

        protected static string fetchPapersAsJSON(HashSet<string> CourseCodes, HashSet<string> PaperTypes, bool isRecommendationsSearch)
        {
            /*Three cases as described by the region titles below
            * case of both == 0 is not handeled here as it is handeled in givePapersFromSearchQuery() method above
            rest all covered
            */

            #region (CourseCodes.Count > 0 && PaperTypes.Count == 0)
            if (CourseCodes.Count > 0 && PaperTypes.Count == 0)
            {
                //case where no paper type is mentioned for one or more course codes
                //show all papers of this/these course code(s)
                int NumberOfPapers = 0;
                string JSONreturn = string.Empty;
                string PNRsReturn = string.Empty;

                foreach (string CourseCode in CourseCodes)
                {
                    //extract CA, MTE, MTP,ETE,ETP, RE papers of this coursecode
                    JSONreturn += "{\"" + CourseCode + "\":[";

                    string CA_JSON_part = string.Empty;
                    string MTE_JSON_part = string.Empty;
                    string ETE_JSON_part = string.Empty;
                    string MTP_JSON_part = string.Empty;
                    string ETP_JSON_part = string.Empty;
                    string REAPPEAR_JSON_part = string.Empty;

                    #region CA_PART
                    DataTable PapersHolder = extractPapersOf(CourseCode, "CA") ?? new DataTable();

                    if (PapersHolder.Rows.Count > 0)
                    {
                        NumberOfPapers += PapersHolder.Rows.Count;
                        CA_JSON_part = "{\"CA\":[";
                        foreach (DataRow row in PapersHolder.Rows)
                        {
                            CA_JSON_part += "\"" + row["PNR"] + "\",";
                            PNRsReturn += row["PNR"] + ",";
                        }

                        CA_JSON_part = CA_JSON_part.Substring(0, CA_JSON_part.Length - 1) + "]}";
                        // PNRsReturn = PNRsReturn.Substring(0, PNRsReturn.Length - 1);
                        try
                        {
                            //PNRsReturn = PNRsReturn.Last<char>() == ',' ? PNRsReturn.Substring(0, PNRsReturn.Length - 1) : PNRsReturn;//removing the last extra comma
                        }
                        catch (InvalidOperationException) { }
                    }
                    #endregion
                    PapersHolder.Clear();
                    #region MTE_PART
                    PapersHolder = extractPapersOf(CourseCode, "MTE") ?? new DataTable();

                    if (PapersHolder.Rows.Count > 0)
                    {
                        NumberOfPapers += PapersHolder.Rows.Count;
                        MTE_JSON_part = "{\"MTE\":[";
                        foreach (DataRow row in PapersHolder.Rows)
                        {
                            MTE_JSON_part += "\"" + row["PNR"] + "\",";
                            PNRsReturn += row["PNR"] + ",";
                        }

                        MTE_JSON_part = MTE_JSON_part.Substring(0, MTE_JSON_part.Length - 1) + "]}";
                        //      PNRsReturn = PNRsReturn.Substring(0, PNRsReturn.Length - 1);
                    }
                    #endregion
                    PapersHolder.Clear();
                    #region ETE_PART

                    PapersHolder = extractPapersOf(CourseCode, "ETE") ?? new DataTable();

                    if (PapersHolder.Rows.Count > 0)
                    {
                        NumberOfPapers += PapersHolder.Rows.Count;
                        ETE_JSON_part = "{\"ETE\":[";
                        foreach (DataRow row in PapersHolder.Rows)
                        {
                            ETE_JSON_part += "\"" + row["PNR"] + "\",";
                            PNRsReturn += row["PNR"] + ",";
                        }

                        ETE_JSON_part = ETE_JSON_part.Substring(0, ETE_JSON_part.Length - 1) + "]}";
                        //          PNRsReturn = PNRsReturn.Substring(0, PNRsReturn.Length - 1);
                    }
                    #endregion
                    PapersHolder.Clear();
                    #region MTP_PART
                    PapersHolder = extractPapersOf(CourseCode, "MTP") ?? new DataTable();

                    if (PapersHolder.Rows.Count > 0)
                    {
                        NumberOfPapers += PapersHolder.Rows.Count;
                        MTP_JSON_part = "{\"MTP\":[";
                        foreach (DataRow row in PapersHolder.Rows)
                        {
                            MTP_JSON_part += "\"" + row["PNR"] + "\",";
                            PNRsReturn += row["PNR"] + ",";
                        }

                        MTP_JSON_part = MTP_JSON_part.Substring(0, MTP_JSON_part.Length - 1) + "]}";
                        //            PNRsReturn = PNRsReturn.Substring(0, PNRsReturn.Length - 1);
                    }
                    #endregion
                    PapersHolder.Clear();
                    #region ETP_PART
                    PapersHolder = extractPapersOf(CourseCode, "ETP") ?? new DataTable();

                    if (PapersHolder.Rows.Count > 0)
                    {
                        NumberOfPapers += PapersHolder.Rows.Count;
                        ETP_JSON_part = "{\"ETP\":[";
                        foreach (DataRow row in PapersHolder.Rows)
                        {
                            ETP_JSON_part += "\"" + row["PNR"] + "\",";
                            PNRsReturn += row["PNR"] + ",";
                        }

                        ETP_JSON_part = ETP_JSON_part.Substring(0, ETP_JSON_part.Length - 1) + "]}";
                        //      PNRsReturn = PNRsReturn.Substring(0, PNRsReturn.Length - 1);
                    }
                    #endregion
                    PapersHolder.Clear();
                    #region REAPPEAR_PART
                    PapersHolder = extractPapersOf(CourseCode, "REAPPEAR") ?? new DataTable();

                    if (PapersHolder.Rows.Count > 0)
                    {
                        NumberOfPapers += PapersHolder.Rows.Count;
                        REAPPEAR_JSON_part = "{\"REAPPEAR\":[";
                        foreach (DataRow row in PapersHolder.Rows)
                        {
                            REAPPEAR_JSON_part += "\"" + row["PNR"] + "\",";
                            PNRsReturn += row["PNR"] + ",";
                        }

                        REAPPEAR_JSON_part = REAPPEAR_JSON_part.Substring(0, REAPPEAR_JSON_part.Length - 1) + "]}";//removing the last extra comma
                                                                                                                   //      PNRsReturn = PNRsReturn.Substring(0, PNRsReturn.Length - 1);
                    }
                    #endregion

                    JSONreturn += ETE_JSON_part == "" ? "" : ETE_JSON_part + ",";
                    JSONreturn += MTE_JSON_part == "" ? "" : MTE_JSON_part + ",";
                    JSONreturn += REAPPEAR_JSON_part == "" ? "" : REAPPEAR_JSON_part + ",";
                    JSONreturn += CA_JSON_part == "" ? "" : CA_JSON_part + ",";
                    JSONreturn += MTP_JSON_part == "" ? "" : MTP_JSON_part + ",";
                    JSONreturn += ETP_JSON_part == "" ? "" : ETP_JSON_part + ",";

                    JSONreturn = JSONreturn.Last<char>() == ',' ? JSONreturn.Substring(0, JSONreturn.Length - 1) : JSONreturn;//removing the last extra comma
                    try
                    {
                        //PNRsReturn = PNRsReturn.Last<char>() == ',' ? PNRsReturn.Substring(0, PNRsReturn.Length - 1) : PNRsReturn;//removing the last extra comma
                    }
                    catch (InvalidOperationException) { }

                    JSONreturn += "]},";//ending each coursecode 
                    PNRsReturn += ",";
                }//each course code loop ends

                JSONreturn = JSONreturn.Substring(0, JSONreturn.Length - 1);//removing the last extra comma
                                                                            //   PNRsReturn = PNRsReturn.Substring(0, PNRsReturn.Length - 1);//removing the last extra comma
                try
                {
                    //PNRsReturn = PNRsReturn.Last<char>() == ',' ? PNRsReturn.Substring(0, PNRsReturn.Length - 1) : PNRsReturn;//removing the last extra comma
                }
                catch (InvalidOperationException) { }
                JSONreturn += "]}";

                JSONreturn = "{\"NumberOfPapersFound\": " + NumberOfPapers + ", \"Message\":\"Success\", \"Papers\":[" + JSONreturn;

                return isRecommendationsSearch ? PNRsReturn : JSONreturn;
            }
            #endregion

            #region (CourseCodes.Count > 0 && PaperTypes.Count > 0)
            else if (CourseCodes.Count > 0 && PaperTypes.Count > 0)
            {
                //one or more course code and one or more papertype   -  show all stuffs

                int NumberOfPapers = 0;
                string JSONreturn = string.Empty;
                string PNRsReturn = string.Empty;

                foreach (string CourseCode in CourseCodes)
                {
                    //extract asked papers of this coursecode
                    JSONreturn += "{\"" + CourseCode + "\":[";

                    foreach (string PaperType in PaperTypes)
                    {
                        DataTable PapersHolder = extractPapersOf(CourseCode, PaperType) ?? new DataTable();

                        if (PapersHolder.Rows.Count > 0)
                        {
                            NumberOfPapers += PapersHolder.Rows.Count;
                            JSONreturn += "{\"" + PaperType + "\":[";
                            foreach (DataRow row in PapersHolder.Rows)
                            {
                                JSONreturn += "\"" + row["PNR"] + "\",";
                                PNRsReturn += row["PNR"] + ",";
                            }

                            JSONreturn = JSONreturn.Substring(0, JSONreturn.Length - 1) + "]},";
                            try
                            {
                                //PNRsReturn = PNRsReturn.Last<char>() == ',' ? PNRsReturn.Substring(0, PNRsReturn.Length - 1) : PNRsReturn;//removing the last extra comma
                            }
                            catch (InvalidOperationException) { }
                            // PNRsReturn = PNRsReturn.Substring(0, PNRsReturn.Length - 1) + ",";
                        }
                    }
                    JSONreturn = JSONreturn.Last<char>() == ',' ? JSONreturn.Substring(0, JSONreturn.Length - 1) : JSONreturn;//removing the last extra comma
                    try
                    {
                        //PNRsReturn = PNRsReturn.Last<char>() == ',' ? PNRsReturn.Substring(0, PNRsReturn.Length - 1) : PNRsReturn;//removing the last extra comma
                    }
                    catch (InvalidOperationException) { }
                    JSONreturn += "]},";//ending each coursecode 

                }//each course code loop ends

                JSONreturn = JSONreturn.Substring(0, JSONreturn.Length - 1);//removing the last extra comma
                                                                            // PNRsReturn = PNRsReturn.Substring(0, PNRsReturn.Length - 1);//removing the last extra comma
                try
                {
                    //PNRsReturn = PNRsReturn.Last<char>() == ',' ? PNRsReturn.Substring(0, PNRsReturn.Length - 1) : PNRsReturn;//removing the last extra comma
                }
                catch (InvalidOperationException) { }
                JSONreturn += "]}";

                JSONreturn = "{\"NumberOfPapersFound\": " + NumberOfPapers + ", \"Message\":\"Success\", \"Papers\":[" + JSONreturn;

                Debug.WriteLine(JSONreturn);

                return isRecommendationsSearch ? PNRsReturn : JSONreturn;
            }
            #endregion

            #region (CourseCodes.Count == 0 && PaperTypes.Count > 0)
            else if (CourseCodes.Count == 0 && PaperTypes.Count > 0)
            {
                //no course code and one or more paper type  -  show trending MTE papers
                return isRecommendationsSearch ? string.Empty : "{\"NumberOfPapersFound\": \"0\", \"Message\": \"No Course Code identified\", \"Papers\": []}";
            }
            #endregion

            return "{\"NumberOfPapersFound\": 0, \"Message\":\"Control reached end without giving anything\",\"Papers\":[]}";///temp
        }

        protected static DataTable extractPapersOf(string CourseCode, string PaperType)
        {
            DataTable returner = new DataTable();

            using (SqlConnection UserQueryDataFetcherCon = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                SqlCommand PNRFetcherCMD = new SqlCommand("", UserQueryDataFetcherCon);
                SqlDataAdapter PapersDataAdapter = new SqlDataAdapter(PNRFetcherCMD);
                string QueryText = string.Empty;

                if (PaperType.StartsWith("CA"))
                {
                    int CANumber = PaperType.Length == 2 ? 0 : Convert.ToInt32(PaperType.Substring(2));
                    //if canumber = 0 get all CA papers of this coursecode, else get specific canumber only

                    if (CANumber == 0)
                        QueryText = "SELECT PNR FROM CAPAPERS WHERE CourseCode='" + CourseCode + "' union SELECT PNR FROM HardTypedPapers WHERE CourseCode='" + CourseCode + "' and HardPaperCode = 2";
                    else
                        QueryText = "SELECT PNR FROM CAPAPERS WHERE CourseCode='" + CourseCode + "' and CANumber = " + CANumber+ " union SELECT PNR FROM HardTypedPapers WHERE CourseCode='" + CourseCode + "' and HardPaperCode = 2 and CANumber = " + CANumber;

                    PNRFetcherCMD.CommandText = QueryText;
                    UserQueryDataFetcherCon.Open();
                    PapersDataAdapter.Fill(returner);
                    UserQueryDataFetcherCon.Close();

                    return returner.Rows.Count > 0 ? returner : null;
                }
                else
                {
                    switch (PaperType)
                    {
                        case "MTE":
                            QueryText = "SELECT PNR FROM Papers WHERE CourseCode='" + CourseCode + "' and MaxMarks < 50 union SELECT PNR FROM HardTypedPapers WHERE CourseCode='" + CourseCode + "' and MaxMarks < 50";
                            break;
                        case "ETE":
                            QueryText = "SELECT PNR FROM Papers WHERE CourseCode='" + CourseCode + "' and MaxMarks > 50 union SELECT PNR FROM HardTypedPapers WHERE CourseCode='" + CourseCode + "' and MaxMarks > 50";
                            break;

                        case "MTP":
                            QueryText = "SELECT PNR FROM CAPapers WHERE CourseCode='" + CourseCode + "' and CAnumber=998 union SELECT PNR FROM HardTypedPapers WHERE CourseCode='" + CourseCode + "' and HardPaperCode=4";
                            break;

                        case "ETP":
                            QueryText = "SELECT PNR FROM CAPapers WHERE CourseCode='" + CourseCode + "' and CAnumber=999 union SELECT PNR FROM HardTypedPapers WHERE CourseCode='" + CourseCode + "' and HardPaperCode=5";
                            break;

                        case "REAPPEAR":
                            QueryText = ";with cte as (select substring(pnr,6,1) as ReSignal,pnr from papers where coursecode = '" + CourseCode + "' union select substring(pnr,6,1) as ReSignal,pnr from HardTypedPapers where coursecode = '" + CourseCode + "' and HardPaperCode =1 and len(PNR) > 12) select pnr from cte where ReSignal like '[a-zA-Z]'";
                            break;
                    }

                    PNRFetcherCMD.CommandText = QueryText;
                    UserQueryDataFetcherCon.Open();
                    PapersDataAdapter.Fill(returner);
                    UserQueryDataFetcherCon.Close();
                }

            }

            return returner.Rows.Count > 0 ? returner : null;
        }

        protected static string giveRecommendationsFor(HashSet<string> CourseCodes, HashSet<string> PaperTypes, List<string> PNRsAlreadyInStrictSearchResult)
        {
            int NumberOfPapers = 0;
            string JSONreturn = string.Empty;

            Debug.WriteLine(PaperTypes.Count);
            Debug.WriteLine(CourseCodes.Count);

            PNRsAlreadyInStrictSearchResult = PNRsAlreadyInStrictSearchResult ?? new List<string>();

            Debug.WriteLine(PNRsAlreadyInStrictSearchResult.Count);

            foreach (string pnr in PNRsAlreadyInStrictSearchResult)
                Debug.WriteLine(pnr);

            foreach (string CourseCode in CourseCodes)
            {
                //extract CA, MTE, MTP,ETE,ETP, RE papers of this coursecode
                JSONreturn += "{\"" + CourseCode + "\":[";

                string CA_JSON_part = string.Empty;
                string MTE_JSON_part = string.Empty;
                string ETE_JSON_part = string.Empty;
                string MTP_JSON_part = string.Empty;
                string ETP_JSON_part = string.Empty;
                string REAPPEAR_JSON_part = string.Empty;

                #region CA_PART
                DataTable PapersHolder = extractPapersOf(CourseCode, "CA") ?? new DataTable();

                //removing PNRs which are already in the strict search results
                //  if (PNRsAlreadyInStrictSearchResult.Count > 0 && PapersHolder.Rows.Count > 0)
                //{     
                Debug.WriteLine(PapersHolder.Rows.Count + " count before loops:");

                for (int c = 0; c < PapersHolder.Rows.Count; c++)
                    for (int d = 0; d < PNRsAlreadyInStrictSearchResult.Count; d++)
                    {
                        if (PapersHolder.Rows.Count == 0) break;
                        Debug.WriteLine(PNRsAlreadyInStrictSearchResult.ElementAt<string>(d) + " at " + d);
                        Debug.WriteLine(PapersHolder.Rows[c]["PNR"].ToString() + " at " + c);
                        if (PNRsAlreadyInStrictSearchResult.ElementAt<string>(d).Equals(PapersHolder.Rows[c]["PNR"].ToString()))
                            PapersHolder.Rows.RemoveAt(c);
                    }
                //}

                if (PapersHolder.Rows.Count > 0)
                {
                    NumberOfPapers += PapersHolder.Rows.Count;
                    CA_JSON_part = "{\"CA\":[";
                    foreach (DataRow row in PapersHolder.Rows)
                        CA_JSON_part += "\"" + row["PNR"] + "\",";

                    CA_JSON_part = CA_JSON_part.Substring(0, CA_JSON_part.Length - 1) + "]}";
                }
                #endregion
                PapersHolder.Clear();
                #region MTE_PART
                PapersHolder = extractPapersOf(CourseCode, "MTE") ?? new DataTable();

                for (int c = 0; c < PapersHolder.Rows.Count; c++)
                    for (int d = 0; d < PNRsAlreadyInStrictSearchResult.Count; d++)
                    {
                        if (PapersHolder.Rows.Count == 0) break;
                        if (PNRsAlreadyInStrictSearchResult.ElementAt<string>(d).Equals(PapersHolder.Rows[c]["PNR"].ToString()))
                            PapersHolder.Rows.RemoveAt(c);
                    }

                if (PapersHolder.Rows.Count > 0)
                {
                    NumberOfPapers += PapersHolder.Rows.Count;
                    MTE_JSON_part = "{\"MTE\":[";
                    foreach (DataRow row in PapersHolder.Rows)
                        MTE_JSON_part += "\"" + row["PNR"] + "\",";

                    MTE_JSON_part = MTE_JSON_part.Substring(0, MTE_JSON_part.Length - 1) + "]}";
                }
                #endregion
                PapersHolder.Clear();
                #region ETE_PART

                PapersHolder = extractPapersOf(CourseCode, "ETE") ?? new DataTable();
                for (int c = 0; c < PapersHolder.Rows.Count; c++)
                    for (int d = 0; d < PNRsAlreadyInStrictSearchResult.Count; d++)
                    {
                        if (PapersHolder.Rows.Count == 0) break;
                        if (PNRsAlreadyInStrictSearchResult.ElementAt<string>(d).Equals(PapersHolder.Rows[c]["PNR"].ToString()))
                            PapersHolder.Rows.RemoveAt(c);
                    }

                if (PapersHolder.Rows.Count > 0)
                {
                    NumberOfPapers += PapersHolder.Rows.Count;
                    ETE_JSON_part = "{\"ETE\":[";
                    foreach (DataRow row in PapersHolder.Rows)
                        ETE_JSON_part += "\"" + row["PNR"] + "\",";

                    ETE_JSON_part = ETE_JSON_part.Substring(0, ETE_JSON_part.Length - 1) + "]}";
                }
                #endregion
                PapersHolder.Clear();
                #region MTP_PART
                PapersHolder = extractPapersOf(CourseCode, "MTP") ?? new DataTable();

                for (int c = 0; c < PapersHolder.Rows.Count; c++)
                    for (int d = 0; d < PNRsAlreadyInStrictSearchResult.Count; d++)
                    {
                        if (PapersHolder.Rows.Count == 0) break;
                        if (PNRsAlreadyInStrictSearchResult.ElementAt<string>(d).Equals(PapersHolder.Rows[c]["PNR"].ToString()))
                            PapersHolder.Rows.RemoveAt(c);
                    }

                if (PapersHolder.Rows.Count > 0)
                {
                    NumberOfPapers += PapersHolder.Rows.Count;
                    MTP_JSON_part = "{\"MTP\":[";
                    foreach (DataRow row in PapersHolder.Rows)
                        MTP_JSON_part += "\"" + row["PNR"] + "\",";

                    MTP_JSON_part = MTP_JSON_part.Substring(0, MTP_JSON_part.Length - 1) + "]}";
                }
                #endregion
                PapersHolder.Clear();
                #region ETP_PART
                PapersHolder = extractPapersOf(CourseCode, "ETP") ?? new DataTable();

                for (int c = 0; c < PapersHolder.Rows.Count; c++)
                    for (int d = 0; d < PNRsAlreadyInStrictSearchResult.Count; d++)
                    {
                        if (PapersHolder.Rows.Count == 0) break;
                        if (PNRsAlreadyInStrictSearchResult.ElementAt<string>(d).Equals(PapersHolder.Rows[c]["PNR"].ToString()))
                            PapersHolder.Rows.RemoveAt(c);
                    }

                if (PapersHolder.Rows.Count > 0)
                {
                    NumberOfPapers += PapersHolder.Rows.Count;
                    ETP_JSON_part = "{\"ETP\":[";
                    foreach (DataRow row in PapersHolder.Rows)
                        ETP_JSON_part += "\"" + row["PNR"] + "\",";

                    ETP_JSON_part = ETP_JSON_part.Substring(0, ETP_JSON_part.Length - 1) + "]}";
                }
                #endregion
                PapersHolder.Clear();
                #region REAPPEAR_PART
                PapersHolder = extractPapersOf(CourseCode, "REAPPEAR") ?? new DataTable();

                for (int c = 0; c < PapersHolder.Rows.Count; c++)
                    for (int d = 0; d < PNRsAlreadyInStrictSearchResult.Count; d++)
                    {
                        if (PapersHolder.Rows.Count == 0) break;
                        if (PNRsAlreadyInStrictSearchResult.ElementAt<string>(d).Equals(PapersHolder.Rows[c]["PNR"].ToString()))
                            PapersHolder.Rows.RemoveAt(c);
                    }

                if (PapersHolder.Rows.Count > 0)
                {
                    NumberOfPapers += PapersHolder.Rows.Count;
                    REAPPEAR_JSON_part = "{\"REAPPEAR\":[";
                    foreach (DataRow row in PapersHolder.Rows)
                        REAPPEAR_JSON_part += "\"" + row["PNR"] + "\",";

                    REAPPEAR_JSON_part = REAPPEAR_JSON_part.Substring(0, REAPPEAR_JSON_part.Length - 1) + "]}";//removing the last extra comma
                }
                #endregion

                JSONreturn += ETE_JSON_part == "" ? "" : ETE_JSON_part + ",";
                JSONreturn += MTE_JSON_part == "" ? "" : MTE_JSON_part + ",";
                JSONreturn += REAPPEAR_JSON_part == "" ? "" : REAPPEAR_JSON_part + ",";
                JSONreturn += CA_JSON_part == "" ? "" : CA_JSON_part + ",";
                JSONreturn += MTP_JSON_part == "" ? "" : MTP_JSON_part + ",";
                JSONreturn += ETP_JSON_part == "" ? "" : ETP_JSON_part + ",";

                JSONreturn = JSONreturn.Last<char>() == ',' ? JSONreturn.Substring(0, JSONreturn.Length - 1) : JSONreturn;//removing the last extra comma

                JSONreturn += "]},";//ending each coursecode 
            }//each course code loop ends

            JSONreturn = JSONreturn.Substring(0, JSONreturn.Length == 0 ? 0 : JSONreturn.Length - 1);//removing the last extra comma or doing nothing if the JSON string is already empty

            JSONreturn += "]}";

            JSONreturn = "{\"NumberOfPapersFound\": " + NumberOfPapers + ", \"Message\":\"Success\", \"Papers\":[" + JSONreturn;

            return JSONreturn;
        }

        private static string formatCourseCode(string RegxMatchCourseCode)
        {
            string formattedCourseCode = string.Empty;

            foreach (char c in RegxMatchCourseCode)
                if (c != ' ')
                    formattedCourseCode += c;

            return formattedCourseCode.ToUpper();
        }
    }
}