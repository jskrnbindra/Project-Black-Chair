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
    public partial class AddQuestions : System.Web.UI.Page
    {
        public static DataTable CacheTable;
        static TimeZoneInfo IST = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        static string cs = ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString;
        public string FishName = string.Empty;
        public static string TemporaryTableName = string.Empty;
        
        protected void Page_Load(object sender, EventArgs e)
        {
            verifyUserAuthentication();
            FishName = getLastModifiedByName();
            SecurityAgent SecAgent = new SecurityAgent();
            if (SecAgent.getPermissionsLevelfor(Request.Cookies["UserID"].Value) == 7)
            {
                hl_SeeAllEntries.Visible = true;
                hl_QuestionExtraction.Visible = true;
                hl_ProdSearch.Visible = true;
                hl_DeleteQuestion.Visible = true;
                hl_SearchBox.Visible = true;
            }
            else
            {
                hl_SeeAllEntries.Visible = false;
                hl_QuestionExtraction.Visible = false;
                hl_ProdSearch.Visible = false;
                hl_DeleteQuestion.Visible = false;
                hl_SearchBox.Visible = false;
            }
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

        protected void RadioButtonList1_SelectedIndexChanged(object sender, EventArgs e)
        {

            if (RadioButtonList1.SelectedValue == "1")
            {
                //lbl.InnerHtml = "mcq";

                Mcq.Style["Visibility"] = "visible";
                Mcq.Style.Remove("display");
            }
            if (RadioButtonList1.SelectedValue == "2")
            {
                //lbl.InnerHtml = "simple";
                Mcq.Style.Add("display", "none");
                Tb_Opt1.Text = string.Empty;
                Tb_Opt2.Text = string.Empty;
                Tb_Opt3.Text = string.Empty;
                Tb_Opt4.Text = string.Empty;
            }
        }

        protected void Cb_File_CheckedChanged(object sender, EventArgs e)
        {

            if (Cb_File.Checked == true)
            {
                Fu_Opt1.Style["visibility"] = "visible";
                Fu_Opt1.Style.Remove("display");
                Fu_Opt2.Style["visibility"] = "visible";
                Fu_Opt2.Style.Remove("display");
                Fu_Opt3.Style["visibility"] = "visible";
                Fu_Opt3.Style.Remove("display");
                Fu_Opt4.Style["visibility"] = "visible";
                Fu_Opt4.Style.Remove("display");
                Tb_Opt1.Style["width"] = "800px";
                Tb_Opt2.Style["width"] = "800px";
                Tb_Opt3.Style["width"] = "800px";
                Tb_Opt4.Style["width"] = "800px";

            }
            if (Cb_File.Checked == false)
            {
                Tb_Opt1.Style["width"] = "1000px";
                Tb_Opt2.Style["width"] = "1000px";
                Tb_Opt3.Style["width"] = "1000px";
                Tb_Opt4.Style["width"] = "1000px";
                Fu_Opt1.Style["visibility"] = "hidden";
                Fu_Opt1.Style.Add("display", "none");
                Fu_Opt2.Style["visibility"] = "hidden";
                Fu_Opt2.Style.Add("display", "none");
                Fu_Opt3.Style["visibility"] = "hidden";
                Fu_Opt3.Style.Add("display", "none");
                Fu_Opt4.Style["visibility"] = "hidden";
                Fu_Opt4.Style.Add("display", "none");
            }
        }

        protected void Button1_Click(object sender, EventArgs e)
        {

            string DiagramFile = "", Option1File = "", Option2File = "", Option3File = "", Option4File = "";
            if (Fu_Diag.HasFile)
            {
                //dfjsdflds
                string timeticks = DateTime.Now.Ticks.ToString();
                DiagramFile = timeticks + "_" + Fu_Diag.PostedFile.FileName.ToString();
                Fu_Diag.SaveAs(Server.MapPath("~") + "/Diagrams/" + DiagramFile);
                DiagramFile = Server.MapPath("~") + "/Diagrams/" + DiagramFile;
            }
            if (Fu_Opt1.HasFile)
            {
                string timeticks = DateTime.Now.Ticks.ToString();
                Option1File = timeticks + "_" + Fu_Opt1.PostedFile.FileName.ToString();
                Fu_Opt1.SaveAs(Server.MapPath("~") + "/Diagrams/" + Option1File);
                Option1File = Server.MapPath("~") + "/Diagrams/" + Option1File;
            }
            if (Fu_Opt2.HasFile)
            {
                string timeticks = DateTime.Now.Ticks.ToString();
                Option2File = timeticks + "_" + Fu_Opt2.PostedFile.FileName.ToString();
                Fu_Opt2.SaveAs(Server.MapPath("~") + "/Diagrams/" + Option2File);
                Option2File = Server.MapPath("~") + "/Diagrams/" + Option2File;
            }
            if (Fu_Opt3.HasFile)
            {
                string timeticks = DateTime.Now.Ticks.ToString();
                Option3File = timeticks + "_" + Fu_Opt3.PostedFile.FileName.ToString();
                Fu_Opt3.SaveAs(Server.MapPath("~") + "/Diagrams/" + Option3File);
                Option3File = Server.MapPath("~") + "/Diagrams/" + Option3File;
            }
            if (Fu_Opt4.HasFile)
            {
                string timeticks = DateTime.Now.Ticks.ToString();
                Option4File = timeticks + "_" + Fu_Opt4.PostedFile.FileName.ToString();
                Fu_Opt4.SaveAs(Server.MapPath("~") + "/Diagrams/" + Option4File);
                Option4File = Server.MapPath("~") + "/Diagrams/" + Option4File;
            }

            string conString = ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString;
            SqlConnection con = new SqlConnection(conString);
            string PNR = Tb_PNR.Text;
            if (Tb_CA.Visible == true)
            {

                //is a CA paper
                handleCAPapers();

                /*    PNR = "CA";
                    if (Tb_CCode.Text != "")
                    {//ideal
                        PNR += Tb_CCode.Text;

                        if (Tb_CA.Text != "")
                            PNR += Tb_CA.Text;
                        if (Tb_set.Text != "")
                            PNR += Tb_set.Text;
                    }
                    else
                    {//no course code
                        if (Tb_CName.Text != "")
                        {//no couse code but course name
                            PNR += giveFirstChars(Tb_CName.Text);
                            if (Tb_CA.Text != "")
                                PNR += Tb_CA.Text;
                            if (Tb_set.Text != "")
                                PNR += Tb_set.Text;
                        }
                        else
                        {//no course code no course name
                            if (Tb_CA.Text != "")//CA+CaNumber else only CA
                                PNR += Tb_CA.Text;
                            if (Tb_set.Text != "")//else only CA
                                PNR += Tb_set.Text;
                        }

                    }*/

            }
            else
            {

                string QuestionID = PNR.Trim() + Tb_No.Text.Trim() + Tb_PartNo.Text.Trim() + Tb_SubNo.Text.Trim();
                ///need to make sure if the other set has the same pnr or not.

                SqlParameter qid = new SqlParameter("@QuestionID", SqlDbType.NVarChar);
                SqlParameter num = new SqlParameter("@number", SqlDbType.Int);
                SqlParameter part = new SqlParameter("@part", SqlDbType.NVarChar);
                SqlParameter spart = new SqlParameter("@subpart", SqlDbType.NVarChar);
                SqlParameter stmt = new SqlParameter("@statement", SqlDbType.NVarChar);
                SqlParameter diagram = new SqlParameter("@Diagram", SqlDbType.NVarChar);
                SqlParameter opt1 = new SqlParameter("@option1", SqlDbType.NVarChar);
                SqlParameter opt1file = new SqlParameter("@option1file", SqlDbType.NVarChar);
                SqlParameter opt2 = new SqlParameter("@option2", SqlDbType.NVarChar);
                SqlParameter opt2file = new SqlParameter("@option2file", SqlDbType.NVarChar);
                SqlParameter opt3 = new SqlParameter("@option3", SqlDbType.NVarChar);
                SqlParameter opt3file = new SqlParameter("@option3file", SqlDbType.NVarChar);
                SqlParameter opt4 = new SqlParameter("@option4", SqlDbType.NVarChar);
                SqlParameter opt4file = new SqlParameter("@option4file", SqlDbType.NVarChar);
                SqlParameter weig = new SqlParameter("@Weightage", SqlDbType.NVarChar);
                SqlParameter pnr = new SqlParameter("@PNR", SqlDbType.NVarChar);
                SqlParameter set = new SqlParameter("@set", SqlDbType.NVarChar);
                SqlParameter ccode = new SqlParameter("@cCode", SqlDbType.NVarChar);
                SqlParameter cname = new SqlParameter("@cName", SqlDbType.NVarChar);
                SqlParameter time = new SqlParameter("@TimeAllowed", SqlDbType.NVarChar);
                SqlParameter drivelink = new SqlParameter("@drivelink", SqlDbType.NVarChar);
                SqlParameter TimeStamp = new SqlParameter("@TimeStamp", SqlDbType.NVarChar);

                SqlCommand cmd = new SqlCommand("AddNewQuestion", con);
                cmd.CommandType = CommandType.StoredProcedure;

                /*/line breaks fix
                string tempstmt = Tb_Stmt.Text.Trim();
                string goodstmt = "";
                for(int ci =0;ci<tempstmt.Length;ci++)
                {
                    if(tempstmt.ElementAt<char>(ci)=='\\'&& tempstmt.ElementAt<char>(ci+1) == 'n'&& tempstmt.ElementAt<char>(ci+2) == ' ')
                    {
                        goodstmt+=
                    }
                }

        */
                cmd.Parameters.AddWithValue("@QuestionID", QuestionID);
                cmd.Parameters.AddWithValue("@number", Tb_No.Text.Trim());
                cmd.Parameters.AddWithValue("@part", Tb_PartNo.Text.Trim().ToLower());
                cmd.Parameters.AddWithValue("@subpart", Tb_SubNo.Text.Trim());
                //cmd.Parameters.AddWithValue("@statement",mikh);//line breaks fix
                //cmd.Parameters.AddWithValue("@statement", Tb_Stmt.Text.Replace("\n", "\\n").Trim());//line breaks fix
                cmd.Parameters.AddWithValue("@statement", Tb_Stmt.Text.Trim());//line breaks fix NEW - browser sends hidden line break chars along implicitly
                cmd.Parameters.AddWithValue("@Diagram", DiagramFile.Trim());
                cmd.Parameters.AddWithValue("@option1", Tb_Opt1.Text.Trim());
                cmd.Parameters.AddWithValue("@option1file", Option1File);
                cmd.Parameters.AddWithValue("@option2", Tb_Opt2.Text.Trim());
                cmd.Parameters.AddWithValue("@option2file", Option2File);
                cmd.Parameters.AddWithValue("@option3", Tb_Opt3.Text.Trim());
                cmd.Parameters.AddWithValue("@option3file", Option3File);
                cmd.Parameters.AddWithValue("@option4", Tb_Opt4.Text.Trim());
                cmd.Parameters.AddWithValue("@option4file", Option4File);
                cmd.Parameters.AddWithValue("@Weightage", Tb_Waitage.Text.Trim());
                cmd.Parameters.AddWithValue("@PNR", PNR);
                cmd.Parameters.AddWithValue("@cCode", Tb_CCode.Text.Trim());
                cmd.Parameters.AddWithValue("@set", Tb_set.Text.Trim());
                cmd.Parameters.AddWithValue("@cName", Tb_CName.Text.Trim());
                cmd.Parameters.AddWithValue("@TimeAllowed", Tb_Time.Text.Trim());
                cmd.Parameters.AddWithValue("@drivelink", Tb_ImageLink.Text.Trim());
                //cmd.Parameters.AddWithValue("@TimeStamp", DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString());

                DateTime taim = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, IST);

                cmd.Parameters.AddWithValue("@TimeStamp", taim.ToLongDateString() + " " + taim.ToLongTimeString());

                con.Open();
                cmd.ExecuteNonQuery();
                logEntry(QuestionID);///loggin user activity
                UpdatePapers(PNR);
                Console.WriteLine("Data Saved into Database successfully.");
                GridView1.DataSourceID = "SqlDataSource1";
                con.Close();
            }
        }
  
        /// <summary>
        /// ////////////////LEFTERLEFTERLEFTERLEFTER
        /// </summary>
        /// <param name="QuestionID"></param>

        protected void logEntry(string QuestionID)
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                SqlCommand cmd = new SqlCommand("insert into Logs_DataEntry values('"+QuestionID+"','"+new QueryStringEncryption().decryptQueryString(Request.Cookies["UserID"].Value.ToString())+"')", con);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private string giveFirstChars(string CourseName)
        {
            CourseName.Trim();
            CourseName = " " + CourseName;
            string returner = "";
            for (int j = 0; j < CourseName.Length; j++)
            {
                if (CourseName.ElementAt(j) == ' ')
                {
                    returner += CourseName.ElementAt(j + 1);
                }
            }
            return returner;
        }

        protected void cb_CA_CheckedChanged(object sender, EventArgs e)
        {
            if (cb_CA.Checked)
            {
                Tb_CAStmt.Visible = true;
                Tb_Stmt.Visible = false;
                Tb_PNR.Visible = false;
                Tb_CA.Visible = true;
                Tb_set.ToolTip = "CA set (Set A, Set B, etc)";
                //ddl_Pattern.Enabled = false;
                ddl_Pattern.ToolTip = "No support for \"ob-lo\" or \"ob\" CA papers yet";
                tb_MaxMarks.Visible = true;
                Tb_SubNo.Visible = false;
                cb_nopnr.Visible = false;

                createLocalTable();
                initializeNewCAEntry();
            }
            else
            {
                Label1.Text = "";
                Tb_CAStmt.Visible = false;
                Tb_Stmt.Visible = true;
                Tb_PNR.Visible = true;
                Tb_CA.Visible = false;
                tb_MaxMarks.Visible = false;
                Panel1.Visible = false;
                Tb_SubNo.Visible = true;
                cb_nopnr.Visible = true;

                CacheTable.Dispose();
                using (SqlConnection con = new SqlConnection(cs))
                {
                    using (SqlCommand cmd = new SqlCommand("select tblname from TempTableKeeper where userid='"+Request.Cookies["UserID"].Value+"'", con))
                    {
                        con.Open();
                        string deleterName = cmd.ExecuteScalar().ToString();
                        cmd.CommandText = "drop table " + deleterName;
                        cmd.ExecuteNonQuery();
                        cmd.CommandText = "delete from TempTableKeeper where tblname='"+deleterName+"'";
                        cmd.ExecuteNonQuery();
                    }
                }//DISPOSE initializeNewCAEntry();

                    }
                }

        protected void updateCAPapers(string pnrno)
        {
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString);
            SqlCommand cmd1 = new SqlCommand("UpdateCAPapers", con);

            cmd1.CommandType = CommandType.StoredProcedure;

            SqlParameter pnr = new SqlParameter("@pnr", SqlDbType.NVarChar);
            SqlParameter pattern = new SqlParameter("@pattern", SqlDbType.NVarChar);
            SqlParameter CAnum = new SqlParameter("@canum", SqlDbType.Int);
            SqlParameter pset = new SqlParameter("@pset", SqlDbType.Char);
            SqlParameter mmarks = new SqlParameter("@mmarks", SqlDbType.Decimal);
            SqlParameter Time = new SqlParameter("@time", SqlDbType.Int);
            SqlParameter ccode = new SqlParameter("@ccode", SqlDbType.NVarChar);
            SqlParameter cname = new SqlParameter("@cname", SqlDbType.NVarChar);
            SqlParameter school = new SqlParameter("@school", SqlDbType.NVarChar);
            SqlParameter timestamp = new SqlParameter("@timestamp", SqlDbType.NVarChar);

            DateTime taim = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, IST);

            cmd1.Parameters.AddWithValue("@pnr", pnrno);
            cmd1.Parameters.AddWithValue("@pattern", ddl_Pattern.SelectedValue.ToString());
            //cmd1.Parameters.AddWithValue("@canum", Convert.ToInt32(Tb_CA.Text == "" ? "0" : Tb_CA.Text));
            cmd1.Parameters.AddWithValue("@canum", Convert.ToInt32(Tb_CA.Text == "" ? cb_MTPETP.Checked ? rb_MTP.Checked ? "998" : "999":"0" :Tb_CA.Text));//998 = MTP and 999 = ETP
            cmd1.Parameters.AddWithValue("@pset", Tb_set.Text.ToUpper());
            cmd1.Parameters.AddWithValue("@mmarks", Convert.ToInt32(tb_MaxMarks.Text));
            cmd1.Parameters.AddWithValue("@time", Tb_Time.Text);
            cmd1.Parameters.AddWithValue("@ccode", Tb_CCode.Text);
            cmd1.Parameters.AddWithValue("@cname", Tb_CName.Text);
            cmd1.Parameters.AddWithValue("@school", tb_School.Text);
            cmd1.Parameters.AddWithValue("@timestamp", taim.ToLongDateString() + " " + taim.ToLongTimeString());

            con.Open();

            cmd1.ExecuteNonQuery();

            con.Close();
        }

        private void UpdatePapers(string pnrno)
        {

            if (cb_CA.Checked)
                updateCAPapers(pnrno);
            else
            {
                SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString);
                SqlCommand cmd = new SqlCommand("UpdatePapers", con);

                cmd.CommandType = CommandType.StoredProcedure;

                SqlParameter pnr = new SqlParameter("@pnr", SqlDbType.NVarChar);
                SqlParameter termid = new SqlParameter("@TermID", SqlDbType.NVarChar);
                SqlParameter year = new SqlParameter("@year", SqlDbType.NVarChar);
                SqlParameter pattern = new SqlParameter("@pattern", SqlDbType.NVarChar);
                SqlParameter Time = new SqlParameter("@time", SqlDbType.Int);
                SqlParameter mmarks = new SqlParameter("@mmarks", SqlDbType.Decimal);
                SqlParameter pset = new SqlParameter("@pset", SqlDbType.Char);
                SqlParameter school = new SqlParameter("@school", SqlDbType.NVarChar);
                SqlParameter ccode = new SqlParameter("@ccode", SqlDbType.NVarChar);
                SqlParameter cname = new SqlParameter("@cname", SqlDbType.NVarChar);
                SqlParameter timestamp = new SqlParameter("@timestamp", SqlDbType.NVarChar);

                string tid = pnrno.Substring(0, 6);
                string yr = pnrno.Substring(1, 4);
                DateTime taim = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, IST);

                cmd.Parameters.AddWithValue("@pnr", pnrno);
                cmd.Parameters.AddWithValue("@TermID", tid);
                cmd.Parameters.AddWithValue("@year", yr);
                cmd.Parameters.AddWithValue("@pattern", ddl_Pattern.SelectedValue.ToString());
                cmd.Parameters.AddWithValue("@time", Tb_Time.Text);
                if (cb_CA.Checked)
                    cmd.Parameters.AddWithValue("@mmarks", Convert.ToInt32(tb_MaxMarks.Text));
                else
                    cmd.Parameters.AddWithValue("@mmarks", giveMaxMarks(pnrno));
                cmd.Parameters.AddWithValue("@pset", Tb_set.Text.ToUpper());
                cmd.Parameters.AddWithValue("@school", tb_School.Text);
                cmd.Parameters.AddWithValue("@ccode", Tb_CCode.Text);
                cmd.Parameters.AddWithValue("@cname", Tb_CName.Text);
                cmd.Parameters.AddWithValue("@timestamp", taim.ToLongDateString() + " " + taim.ToLongTimeString());

                con.Open();
                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (SqlException e)
                {
                    string errorMessage = e.ToString();
                    if (errorMessage.Contains("Violation of PRIMARY KEY constraint") && errorMessage.Contains("Cannot insert duplicate key in object"))
                    {
                        //do nothing
                        System.Diagnostics.Debug.WriteLine("giv " + giveMaxMarks(pnrno));
                        SqlCommand updatercmd = new SqlCommand("Update papers set maxmarks=" + giveMaxMarks(pnrno) + ",TimeStamp='" + taim.ToLongDateString() + " " + taim.ToLongTimeString() + "' where pnr='" + pnrno + "'", con);
                        if (updatercmd.ExecuteNonQuery() == 1) { }
                        //Label1.Text = "Success";
                    }
                    else if (errorMessage.Contains("String or binary data would be truncated"))
                    {
                        Label1.Text = "ERROR: Looks like a problem with PNR number.";

                    }
                    else
                    {
                        Label1.Text = "ERROR: Check your values once.";
                    }
                }
                con.Close();
            }
        }

        private float giveMaxMarks(string pnrno)
        {
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString);
            SqlCommand cmd = new SqlCommand("Select MaxMarks from papers where pnr='" + pnrno + "'", con);
            con.Open();
            float dbmax = (float)Convert.ToDecimal(cmd.ExecuteScalar());
            //Console.WriteLine("1" +dbmax);
            System.Diagnostics.Debug.WriteLine("1 " + dbmax);

            con.Close();

            float maxmarks = 0f;

            if (ddl_Pattern.SelectedValue == "ob")
                maxmarks = (float)Convert.ToDecimal(Tb_Waitage.Text);

            else if (ddl_Pattern.SelectedValue == "sh-lo" || ddl_Pattern.SelectedValue == "ob-lo")
            {
                //max marks logic
                if (Tb_No.Text == "1")
                    maxmarks = (float)Convert.ToDecimal(Tb_Waitage.Text);
                else
                {
                    if (Tb_PartNo.Text != "")
                        if (Tb_PartNo.Text == "a")
                            maxmarks = (float)Convert.ToDecimal(Tb_Waitage.Text);
                        else
                            maxmarks = 0f;
                    else
                        maxmarks = (float)Convert.ToDecimal(Tb_Waitage.Text);
                }
                //max marks logic ends
            }
            //Console.WriteLine("2" + maxmarks);
            System.Diagnostics.Debug.WriteLine("2 " + maxmarks);

            return dbmax + maxmarks;
        }

        protected void createLocalTable()
        {
            CacheTable = new DataTable("CA_Cache");

            DataColumn Serial = new DataColumn("Serial", Type.GetType("System.Int32"));
            Serial.AutoIncrement = true;
            Serial.AllowDBNull = false;

            DataColumn QuestionID = new DataColumn("QuestionID", Type.GetType("System.String"));
            QuestionID.AllowDBNull = false;

            DataColumn Number = new DataColumn("Number", Type.GetType("System.Int32"));
            Number.AllowDBNull = false;

            DataColumn Part = new DataColumn("Part", Type.GetType("System.String"));
            DataColumn SubPart = new DataColumn("SubPart", Type.GetType("System.String"));

            DataColumn Statement = new DataColumn("Statement", Type.GetType("System.String"));
            Statement.AllowDBNull = false;

            DataColumn Diagram = new DataColumn("Diagram", Type.GetType("System.String"));
            DataColumn Option1 = new DataColumn("Option1", Type.GetType("System.String"));
            DataColumn Option1File = new DataColumn("Option1File", Type.GetType("System.String"));
            DataColumn Option2 = new DataColumn("Option2", Type.GetType("System.String"));
            DataColumn Option2File = new DataColumn("Option2File", Type.GetType("System.String"));
            DataColumn Option3 = new DataColumn("Option3", Type.GetType("System.String"));
            DataColumn Option3File = new DataColumn("Option3File", Type.GetType("System.String"));
            DataColumn Option4 = new DataColumn("Option4", Type.GetType("System.String"));
            DataColumn Option4File = new DataColumn("Option4File", Type.GetType("System.String"));
            DataColumn Weightage = new DataColumn("Weightage", Type.GetType("System.Decimal"));

            DataColumn PNR = new DataColumn("PNR", Type.GetType("System.String"));
            PNR.AllowDBNull = false;

            DataColumn CourseCode = new DataColumn("CourseCode", Type.GetType("System.String"));
            DataColumn CourseName = new DataColumn("CourseName", Type.GetType("System.String"));
            DataColumn MaxTimeAllowed = new DataColumn("MaxTimeAllowed", Type.GetType("System.Int32"));

            DataColumn PaperSet = new DataColumn("PaperSet", Type.GetType("System.String"));
            DataColumn DriveLink = new DataColumn("DriveLink", Type.GetType("System.String"));
            DataColumn TimeStamps = new DataColumn("TimeStamps", Type.GetType("System.String"));

            CacheTable.Columns.Add(Serial);
            CacheTable.Columns.Add(QuestionID);
            CacheTable.Columns.Add(Number);
            CacheTable.Columns.Add(Part);
            CacheTable.Columns.Add(SubPart);
            CacheTable.Columns.Add(Statement);
            CacheTable.Columns.Add(Diagram);
            CacheTable.Columns.Add(Option1);
            CacheTable.Columns.Add(Option1File);
            CacheTable.Columns.Add(Option2);
            CacheTable.Columns.Add(Option2File);
            CacheTable.Columns.Add(Option3);
            CacheTable.Columns.Add(Option3File);
            CacheTable.Columns.Add(Option4);
            CacheTable.Columns.Add(Option4File);
            CacheTable.Columns.Add(Weightage);
            CacheTable.Columns.Add(PNR);
            CacheTable.Columns.Add(CourseCode);
            CacheTable.Columns.Add(CourseName);
            CacheTable.Columns.Add(MaxTimeAllowed);
            CacheTable.Columns.Add(PaperSet);
            CacheTable.Columns.Add(DriveLink);
            CacheTable.Columns.Add(TimeStamps);

            DataColumn[] Primarykey = new DataColumn[1];
            Primarykey[0] = QuestionID;

            CacheTable.PrimaryKey = Primarykey;

        }

        public static DataTable createLocalTable(string vain)
        {
            CacheTable = new DataTable("CA_Cache");

            DataColumn Serial = new DataColumn("Serial", Type.GetType("System.Int32"));
            Serial.AutoIncrement = true;
            Serial.AllowDBNull = false;

            DataColumn QuestionID = new DataColumn("QuestionID", Type.GetType("System.String"));
            QuestionID.AllowDBNull = false;

            DataColumn Number = new DataColumn("Number", Type.GetType("System.Int32"));
            Number.AllowDBNull = false;

            DataColumn Part = new DataColumn("Part", Type.GetType("System.String"));
            DataColumn SubPart = new DataColumn("SubPart", Type.GetType("System.String"));

            DataColumn Statement = new DataColumn("Statement", Type.GetType("System.String"));
            Statement.AllowDBNull = false;

            DataColumn Diagram = new DataColumn("Diagram", Type.GetType("System.String"));
            DataColumn Option1 = new DataColumn("Option1", Type.GetType("System.String"));
            DataColumn Option1File = new DataColumn("Option1File", Type.GetType("System.String"));
            DataColumn Option2 = new DataColumn("Option2", Type.GetType("System.String"));
            DataColumn Option2File = new DataColumn("Option2File", Type.GetType("System.String"));
            DataColumn Option3 = new DataColumn("Option3", Type.GetType("System.String"));
            DataColumn Option3File = new DataColumn("Option3File", Type.GetType("System.String"));
            DataColumn Option4 = new DataColumn("Option4", Type.GetType("System.String"));
            DataColumn Option4File = new DataColumn("Option4File", Type.GetType("System.String"));
            DataColumn Weightage = new DataColumn("Weightage", Type.GetType("System.Decimal"));

            DataColumn PNR = new DataColumn("PNR", Type.GetType("System.String"));
            PNR.AllowDBNull = false;

            DataColumn CourseCode = new DataColumn("CourseCode", Type.GetType("System.String"));
            DataColumn CourseName = new DataColumn("CourseName", Type.GetType("System.String"));
            DataColumn MaxTimeAllowed = new DataColumn("MaxTimeAllowed", Type.GetType("System.Int32"));

            DataColumn PaperSet = new DataColumn("PaperSet", Type.GetType("System.String"));
            DataColumn DriveLink = new DataColumn("DriveLink", Type.GetType("System.String"));
            DataColumn TimeStamps = new DataColumn("TimeStamps", Type.GetType("System.String"));

            CacheTable.Columns.Add(Serial);
            CacheTable.Columns.Add(QuestionID);
            CacheTable.Columns.Add(Number);
            CacheTable.Columns.Add(Part);
            CacheTable.Columns.Add(SubPart);
            CacheTable.Columns.Add(Statement);
            CacheTable.Columns.Add(Diagram);
            CacheTable.Columns.Add(Option1);
            CacheTable.Columns.Add(Option1File);
            CacheTable.Columns.Add(Option2);
            CacheTable.Columns.Add(Option2File);
            CacheTable.Columns.Add(Option3);
            CacheTable.Columns.Add(Option3File);
            CacheTable.Columns.Add(Option4);
            CacheTable.Columns.Add(Option4File);
            CacheTable.Columns.Add(Weightage);
            CacheTable.Columns.Add(PNR);
            CacheTable.Columns.Add(CourseCode);
            CacheTable.Columns.Add(CourseName);
            CacheTable.Columns.Add(MaxTimeAllowed);
            CacheTable.Columns.Add(PaperSet);
            CacheTable.Columns.Add(DriveLink);
            CacheTable.Columns.Add(TimeStamps);

            DataColumn[] Primarykey = new DataColumn[1];
            Primarykey[0] = QuestionID;

            CacheTable.PrimaryKey = Primarykey;

            return CacheTable;
        }//method used in DownloadPaperPDF.aspx.cs to create RowsAboveThisQuestion Table

        static string caPNR = "";

        protected void generateCAPNR(string Ccode)
        {
            if (caPNR == "")
                caPNR = Ccode + DateTime.Now.Ticks.ToString();
        }

        protected void initializeNewCAEntry()
        {
            DataTable PreviousSessionData = new DataTable();
            using (SqlConnection con = new SqlConnection(cs))
            {
                using (SqlDataAdapter ada = new SqlDataAdapter("select * from TempTablekeeper where userid='"+Request.Cookies["UserID"].Value + "'",con))
                {
                    con.Open();
                    ada.Fill(PreviousSessionData);
                    if(PreviousSessionData.Rows.Count>0)///dup check
                    {
                        //throw new Exception("A CA entry session is already initialised for this user. Report this error to Admin. +91 9988255277");
                        Label1.Text = "A CA entry session is already initialised for this user. Report this error to Admin. +91 9988255277";
                        btn_ClosePrev.Visible = true;
                    }
                    else
                    {
                        SqlCommand cmd = new SqlCommand(string.Empty,con);
                        string tablenametemp = "A" + DateTime.Now.Ticks;
                        TemporaryTableName = tablenametemp;
                        cmd.CommandText = "Select * into " + tablenametemp+ " from QuestionPapersDump where 1=9";
                        cmd.ExecuteNonQuery();
                        cmd.CommandText = "insert into TempTablekeeper values('" + tablenametemp + "','"+Request.Cookies["UserID"].Value+"')";
                        cmd.ExecuteNonQuery();
                        Label1.Text = "Your CA session ID is = " + TemporaryTableName + ".";
                    }
                }
            }
        }

        private string giveTableName()
        {
            using (SqlConnection con = new SqlConnection(cs))
            {
                using (SqlCommand cmd = new SqlCommand("Select tblName from TempTableKeeper where UserID='" + Request.Cookies["UserID"].Value + "'",con))
                {
                    con.Open();
                    return cmd.ExecuteScalar().ToString();//handle later
                }
            }
        }

        protected void handleCAPapers()
        {
            ////switching from DataTable to persistent temp table for convenience of Data Entry Crew
            //creating temp table
            string temptablename = giveTableName();

            generateCAPNR(Tb_CCode.Text);//generating new PNR, assuming it is a new CA paper
            string caQID = caPNR + Tb_No.Text;
            caQID += Tb_PartNo.Text == "" ? "-" : Tb_PartNo.Text;
            //FORMAT = PNR + NUMBER + -/Part;
            System.Diagnostics.Debug.WriteLine("PNR " + caPNR);
            System.Diagnostics.Debug.WriteLine("QID " + caQID);

            /////////diagram and option files

            string DiagramFile = "", Option1File = "", Option2File = "", Option3File = "", Option4File = "";
            if (Fu_Diag.HasFile)
            {
                string timeticks = DateTime.Now.Ticks.ToString();
                DiagramFile = timeticks + "_" + Fu_Diag.PostedFile.FileName.ToString();
                Fu_Diag.SaveAs(Server.MapPath("~") + "/Diagrams/" + DiagramFile);
                DiagramFile = Server.MapPath("~") + "/Diagrams/" + DiagramFile;
            }
            if (Fu_Opt1.HasFile)
            {
                string timeticks = DateTime.Now.Ticks.ToString();
                Option1File = timeticks + "_" + Fu_Opt1.PostedFile.FileName.ToString();
                Fu_Opt1.SaveAs(Server.MapPath("~") + "/Diagrams/" + Option1File);
                Option1File = Server.MapPath("~") + "/Diagrams/" + Option1File;
            }
            if (Fu_Opt2.HasFile)
            {
                string timeticks = DateTime.Now.Ticks.ToString();
                Option2File = timeticks + "_" + Fu_Opt2.PostedFile.FileName.ToString();
                Fu_Opt2.SaveAs(Server.MapPath("~") + "/Diagrams/" + Option2File);
                Option2File = Server.MapPath("~") + "/Diagrams/" + Option2File;
            }
            if (Fu_Opt3.HasFile)
            {
                string timeticks = DateTime.Now.Ticks.ToString();
                Option3File = timeticks + "_" + Fu_Opt3.PostedFile.FileName.ToString();
                Fu_Opt3.SaveAs(Server.MapPath("~") + "/Diagrams/" + Option3File);
                Option3File = Server.MapPath("~") + "/Diagrams/" + Option3File;
            }
            if (Fu_Opt4.HasFile)
            {
                string timeticks = DateTime.Now.Ticks.ToString();
                Option4File = timeticks + "_" + Fu_Opt4.PostedFile.FileName.ToString();
                Fu_Opt4.SaveAs(Server.MapPath("~") + "/Diagrams/" + Option4File);
                Option4File = Server.MapPath("~") + "/Diagrams/" + Option4File;
            }
            /////////diagram and option files

           // CacheTable.Rows.Add(new object[] { null, caQID, Tb_No.Text, Tb_PartNo.Text, null, Tb_CAStmt.Text, DiagramFile, Tb_Opt1.Text.Trim(), Option1File, Tb_Opt2.Text.Trim(), Option2File, Tb_Opt3.Text.Trim(), Option3File, Tb_Opt4.Text.Trim(), Option4File, Convert.ToDecimal(Tb_Waitage.Text), caPNR, Tb_CCode.Text, Tb_CName.Text, Convert.ToInt32(Tb_Time.Text), Tb_set.Text, Tb_ImageLink.Text, DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() });
            string RowCountinTempTbl=string.Empty;
            using(SqlConnection con = new SqlConnection(cs))///storing in persistent temp table
            {
                using(SqlCommand cmd = new SqlCommand("insert into "+temptablename+" values('"+caQID+"','"+Tb_No.Text+"','"+Tb_PartNo.Text+"','','"+Tb_CAStmt.Text+"','"+DiagramFile+"','"+Tb_Opt1.Text+"','"+Option1File+"','"+Tb_Opt2.Text+"','"+Option2File+"','"+Tb_Opt3.Text+"','"+Option3File+"','"+Tb_Opt4.Text+"','"+Option4File+"',"+ Convert.ToDecimal(Tb_Waitage.Text)+",'"+caPNR+"','"+Tb_CCode.Text+"','"+Tb_CName.Text+"',"+ Convert.ToInt32(Tb_Time.Text)+",'"+Tb_set.Text.ToUpper()+"','"+Tb_ImageLink.Text+"','"+ DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString()+"')",con))
                {
                    con.Open();
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = "select count(*) from " + temptablename;
                    RowCountinTempTbl = cmd.ExecuteScalar().ToString();
                }
            }

            logEntry(caQID);
            if (Convert.ToInt32(RowCountinTempTbl ) > 0)
            {
                //TableRows.Text = CacheTable.Rows.Count + " questions cached";
                TableRows.Text = RowCountinTempTbl + " questions cached";
                CacheControls.Visible = true;
                btn_cancelEntry.Visible = false;
            }
        }

        protected void checkForDuplicates(string Statement)
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                con.Open();

                SqlCommand cmd = new SqlCommand("DetectDuplicates", con);
                cmd.CommandType = CommandType.StoredProcedure;

                SqlParameter param_stmt = new SqlParameter("@Statement", SqlDbType.NVarChar);
                SqlParameter param_number = new SqlParameter("@Number", SqlDbType.Int);
                SqlParameter param_part = new SqlParameter("@Part", SqlDbType.NVarChar);
                SqlParameter param_Ccode = new SqlParameter("@CourseCode", SqlDbType.NVarChar);

                cmd.Parameters.AddWithValue("@Statement", Tb_CAStmt.Text.Trim());
                cmd.Parameters.AddWithValue("@Number", Convert.ToInt32(Tb_No.Text));
                cmd.Parameters.AddWithValue("@CourseCode", Tb_CCode.Text);

                if (Tb_PartNo.Text != "")
                    cmd.Parameters.AddWithValue("@Part", Tb_PartNo.Text);
                else
                    cmd.Parameters.AddWithValue("@Part", string.Empty);

                SqlDataAdapter ada = new SqlDataAdapter(cmd);

                DataTable dt = new DataTable();
                ada.Fill(dt);

                if (dt.Rows.Count > 0)
                {
                    Panel1.Visible = true;
                    GridView2.DataSource = dt;
                    GridView2.DataBind();
                }
                else
                    // Label2.Text = "No rows returned";
                    Panel1.Visible = false;
            }
        }

        protected void Tb_CAStmt_TextChanged(object sender, EventArgs e)
        {
            checkForDuplicates(Tb_CAStmt.Text);
        }

        protected void btn_commitSync_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("PNR in COmmitter: " + caPNR);
            updateCAPapers(caPNR);
            using (SqlConnection con = new SqlConnection(cs))
            {
                using (SqlCommand cmd = new SqlCommand("insert into QuestionPapersDump select [QuestionID],[Number],[Part],[SubPart],[Statement],[Diagram],[Option1],[Option1File],[Option2],[Option2File],[Option3],[Option3File],[Option4],[Option4File],[Weightage],[PNR],[CourseCode],[CourseName],[MaxTimeAllowed],[PaperSet],[DriveLink],[TimeStamps] from " + giveTableName(), con))
                {
                    con.Open();
                    if (cmd.ExecuteNonQuery()>0)
                    {
                        cmd.CommandText = "drop table " + giveTableName();
                        cmd.ExecuteNonQuery();
                        cmd.CommandText = "delete from TempTableKeeper where UserID='"+Request.Cookies["UserID"].Value+"'";
                        cmd.ExecuteNonQuery();
                    }
                }
            }
                    /*
                    using (SqlConnection con = new SqlConnection(cs))
                    {
                        con.Open();
                        SqlBulkCopy sbc = new SqlBulkCopy(con);
                        sbc.DestinationTableName = "QuestionPapersDump";
                        sbc.WriteToServer(CacheTable);
                    }*/
                    CacheTable.Clear();
            caPNR = "";
            Response.Redirect(Request.RawUrl);
        }

        protected void btn_cancelSync_Click(object sender, EventArgs e)
        {
            using (SqlConnection con = new SqlConnection(cs))
            {
                using (SqlCommand cmd = new SqlCommand("drop table "+ giveTableName(), con))
                {
                    con.Open();
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = "delete from TempTableKeeper where userId = '"+Request.Cookies["UserID"].Value+"'";
                    cmd.ExecuteNonQuery();
                }
            }
            CacheTable.Clear();
            caPNR = "";
            Response.Redirect(Request.RawUrl);
        }

        protected void btn_cancelEntry_Click(object sender, EventArgs e)
        {
            btn_cancelSync_Click(new object(), new EventArgs());
            CacheTable.Clear();
            caPNR = "";
            Response.Redirect(Request.RawUrl);
        }

        protected void cb_nopnr_CheckedChanged(object sender, EventArgs e)
        {
            if (cb_nopnr.Checked)
            {
                pnl_ModalBackground.Visible = true;
                pnl_PNRlessModal.Visible = true;
                cb_CA.Visible = false;
                cb_MTPETP.Visible = false;
            }
            else
            {
                pnl_ModalBackground.Visible = false;
                pnl_PNRlessModal.Visible = false;
                cb_CA.Visible = true;
                cb_MTPETP.Visible = true;
            }

        }

        public void PNRgenerator_Click(object sender, EventArgs e)
        {
            if (tb_Modal_CCode.Text != "" && tb_Modal_CCode.Text.Length > 5)
            {
                Tb_PNR.Text = giveNewPNR(tb_Modal_CCode.Text.Trim());
                pnl_ModalBackground.Visible = false;
                pnl_PNRlessModal.Visible = false;
                Tb_PNR.BorderColor = System.Drawing.Color.Green;
                Tb_PNR.BorderWidth = 3;
                Tb_PNR.BorderStyle = BorderStyle.Dashed;

                Tb_CCode.Text = tb_Modal_CCode.Text;
            }
            else
                PNRgenerator.Text = "Invalid! Try Again";
        }

        public string giveNewPNR(string CourseCode)
        {
            string ticks = DateTime.Now.Ticks.ToString();
            ticks = ticks.Substring(ticks.Length - 5);//getting last five digits from the ticks

            string dbPreNum = giveNextPNRPreCount().ToString();
            dbPreNum.PadLeft(6, '0');
            string returner = dbPreNum + CourseCode.Substring(0, 3) + "00000";

            return returner;
        }

        protected int giveNextPNRPreCount()
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                SqlCommand cmd = new SqlCommand("Select min(PNR_first_half) from noPNRkeeper", con);
                con.Open();
                int returner = Convert.ToInt32(cmd.ExecuteScalar().ToString());

                SqlCommand PNRlessNumberKeeperInserter = new SqlCommand("NewPNRlessnNumberKeeper", con);
                PNRlessNumberKeeperInserter.CommandType = CommandType.StoredProcedure;
                SqlParameter newPNRlessNumber = new SqlParameter("@DecrementedValue", SqlDbType.Int);
                PNRlessNumberKeeperInserter.Parameters.AddWithValue("@DecrementedValue", returner - 1);

                PNRlessNumberKeeperInserter.ExecuteNonQuery();
                return returner - 1;
            }
        }

        protected void CancelPNRless_Click(object sender, EventArgs e)
        {
            pnl_ModalBackground.Visible = false;
            pnl_PNRlessModal.Visible = false;
            cb_nopnr.Checked = false;

            cb_MTPETP.Visible = true;
            cb_CA.Visible = true;
        }

        protected void cb_MTPETP_CheckedChanged(object sender, EventArgs e)
        {
            if (cb_MTPETP.Checked)
            {
                rb_ETP.Visible = true;
                rb_MTP.Visible = true;
                cb_CA.Enabled = false;
                cb_nopnr.Enabled = false;
                cb_CA.Checked = true;

                cb_CA_CheckedChanged(new object(),new EventArgs());
                Tb_CA.Enabled = false;
                Tb_CA.ToolTip = "Disabled";
                ddl_Pattern.Enabled = false;
            }
            else
            {
                rb_ETP.Visible = false;
                rb_MTP.Visible = false;
                cb_CA.Enabled = true;
                cb_nopnr.Enabled = true;
                cb_CA.Checked = false;
                cb_CA_CheckedChanged(new object(), new EventArgs());
                Tb_CA.Enabled = true;
                Tb_CA.ToolTip = "";
                ddl_Pattern.Enabled = true;

            }
        }

        protected string getLastModifiedByName()
        {
            using (SqlConnection con = new SqlConnection(cs))
            {
                SqlCommand cmd = new SqlCommand("select \"user\" from logs_DataEntry where QuestionID in (select QuestionID from questionpapersdump where serial in (select max(serial) from questionpapersdump))", con);
                con.Open();
                return cmd.ExecuteScalar().ToString();
            }
        }

        protected void btn_ClosePrev_Click(object sender, EventArgs e)
        {
            using (SqlConnection con = new SqlConnection(cs))
            {
                using (SqlCommand cmd = new SqlCommand("select tblname from temptablekeeper where userid='"+Request.Cookies["UserID"].Value+"'", con))
                {
                    con.Open();
                    string deletername = cmd.ExecuteScalar().ToString();
                    cmd.CommandText = "Drop table " + deletername;
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = "delete from TempTablekeeper where UserID ='" + Request.Cookies["UserID"].Value + "'";
                    cmd.ExecuteNonQuery();
                    Label1.Text = "Previous session ended. Start fresh by hard refreshing this page.";
                    btn_ClosePrev.Visible = false;
                }
            }
        }
        
        /*
* Wasted way for tackeling PNRless papers
* Rebuild - generate PNR and populate textbox

protected string giveNewPNR(string CourseCode)
{
   if (thisPNRexistsBefore("ddaf"))
       return "xxxxxxxxx";
   else
   {
       string ticks = DateTime.Now.Ticks.ToString();
       ticks = ticks.Substring(ticks.Length - 5);//getting last five digits from the ticks

       string dbPreNum = giveNextPNRPreCount().ToString();
       dbPreNum.PadLeft(6, '0');

       string returner = dbPreNum + CourseCode.Substring(0, 3) + "00000";

       return returner;
   }
}

protected bool thisPNRexistsBefore(string PNR)
{
   using(SqlConnection con = new SqlConnection(cs))
   {
       SqlCommand PNRChecker = new SqlCommand("select count(*) from questionpapersdump where pnr = '"+PNR+"'", con);
       if (Convert.ToInt32(PNRChecker.ExecuteScalar().ToString()) ==> 0)
           return true;
       else return false;
   }

}*/
    }
}