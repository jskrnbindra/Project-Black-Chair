using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using DataCollection;
using System.IO;
using System.Data.OleDb;

namespace DataCollection
{
    public partial class DataSecurity : System.Web.UI.Page
    {
        public int QPDcount = 0, PapersCount = 0, CApapersCount = 0;
        public static int NamesCount = 0;
        static TimeZoneInfo IST = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

        protected void Page_Load(object sender, EventArgs e)
        {
            if(!IsPostBack)
            getStats();

            verifyUserAuthentication();
            SecurityAgent SecAgent = new SecurityAgent();
            if (SecAgent.getPermissionsLevelfor(Request.Cookies["UserID"].Value) == 7)
            {
                RightHolder.Visible = true;
                ResetterControls.Visible = true;
                NameUpdater.Visible = true;
                pnl_HTP.Visible = true;
            }
            else
            {
                RightHolder.Visible = false;
                ResetterControls.Visible = false;
                NameUpdater.Visible = false;
                pnl_HTP.Visible = false;
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

        protected void getStats()
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                SqlCommand cmd = new SqlCommand("select count(*) from QuestionPapersDump", con);
                con.Open();
                QPDcount = Convert.ToInt32(cmd.ExecuteScalar());
                cmd.CommandText = "select count(*) from Papers";
                PapersCount = Convert.ToInt32(cmd.ExecuteScalar());
                cmd.CommandText = "select count(*) from CAPapers";
                CApapersCount = Convert.ToInt32(cmd.ExecuteScalar());
                cmd.CommandText = "select count(distinct(UID)) from NamesKeeper";
                lbl_StatsNames.Text = cmd.ExecuteScalar().ToString();
                cmd.CommandText = "select count(*) from HardTypedPapers";
                lbl_HardTypedPapers.Text = cmd.ExecuteScalar().ToString()+ " "+lbl_HardTypedPapers.Text;
                cmd.CommandText = "select top 1(ResetTimeStamp) from ResetHistory order by serial desc";
                string LastResetDateTimeString = cmd.ExecuteScalar().ToString();
                lbl_reset.Text = giveTimeSince(LastResetDateTimeString);
                lbl_resetPrimitive.Text = LastResetDateTimeString;
                cmd.CommandText = "select count(distinct(maxmarks)) from papers";
                int countOfDistinctMaxMarks =  Convert.ToInt32(cmd.ExecuteScalar().ToString());

                if(countOfDistinctMaxMarks>4)//anamoly exists
                {
                    pnl_MaxMarksAnomoly.BackColor = System.Drawing.Color.OrangeRed;
                    lbl_MaxMarksAnamoly.Text = "Caution: Max Marks anamoly detected !!";
                }
                else
                {

                }
            }
            lbl_StatQPD.Text = QPDcount + lbl_StatQPD.Text;
            lbl_StatP.Text = PapersCount + lbl_StatP.Text;
            lbl_StatCAP.Text = CApapersCount + lbl_StatCAP.Text;
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                SqlDataAdapter ada = new SqlDataAdapter();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = con;
                string TableToBackup = ddl_Tables.SelectedItem.Text;
                cmd.CommandText = "Select * from " + TableToBackup + " order by Serial";
                ada.SelectCommand = cmd;

                DataTable dt_Temp = new DataTable();
                con.Open();
                ada.Fill(dt_Temp);
                pushToHTTP(dt_Temp);
            }
        }

        protected void pushToHTTP(DataTable DownloadableTable)
        {
            Response.Clear();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment;filename=Backup_" + ddl_Tables.SelectedItem.Text + ".xls");
            Response.Charset = "";
            Response.ContentType = "application/vnd.ms-excel";

            GridView gv_ExcelFileData = new GridView();
            gv_ExcelFileData.DataSource = DownloadableTable;
            gv_ExcelFileData.AllowPaging = false;
            gv_ExcelFileData.DataBind();

            using (StringWriter sw = new StringWriter())
            {
                HtmlTextWriter htw = new HtmlTextWriter(sw);
                gv_ExcelFileData.RenderControl(htw);
                Response.Output.Write(sw.ToString());
                Response.Flush();
                Response.End();
            }
        }

        public override void VerifyRenderingInServerForm(Control control)
        {
            /* Verifies that the control is rendered */
        }

        public static DataTable loadToDataTable(string ExcelFile)
        {
            DataTable returner = null;
            int sheets = 0;

            string path = "F:\\BlackChair - LPU Data for top students\\Batch 2013\\b.xlsx";
            string connStr = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + path + ";Extended Properties=Excel 12.0;";

            //using (OleDbConnection olecon = new OleDbConnection(@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + ExcelFile + ";Extended Properties='Excel 12.0;HDR=YES;IMEX=1;';"))
            using (OleDbConnection olecon = new OleDbConnection(connStr))
            {
                olecon.Open();

                OleDbCommand olecmd = new OleDbCommand();
                OleDbDataAdapter oleada = new OleDbDataAdapter();
                DataSet ds = new DataSet();

                DataTable dt = olecon.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);

                string SheetName = string.Empty;

                if (dt != null)
                {
                    var tempDataTable = (from dataRow in dt.AsEnumerable() where !dataRow["Table_Name"].ToString().Contains("FilterDatabase") select dataRow).CopyToDataTable();
                    dt = tempDataTable;
                    sheets = dt.Rows.Count;
                    SheetName = dt.Rows[0]["Table_Name"].ToString();
                }

                olecmd.Connection = olecon;
                olecmd.CommandType = CommandType.Text;
                olecmd.CommandText = "Select * from [" + SheetName + "]";
                oleada = new OleDbDataAdapter(olecmd);

                oleada.Fill(ds, "ExcelFile");
                returner = ds.Tables["ExcelFile"];
                olecon.Close();
                return returner;
            }
        }

        protected void btn_NameUpdater_Click(object sender, EventArgs e)
        {
            lbl_NameKeeper.Text = "Adding data to table --> BlackChair.NamesKeeper";

            btn_NameUpdater.Visible = false;
            btn_ConfirmWrite.Visible = true;
            pnl_UpdationControls.Visible = true;
            btn_NameCanceller.Visible = true;
        }

        protected int WriteXLStoServer(string XLSFilePath)
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                SqlBulkCopy sbc = new SqlBulkCopy(con);
                sbc.DestinationTableName = "NamesKeeper";
                DataTable ToBeWritten = convertToDataTable(XLSFilePath);

                SqlBulkCopyColumnMapping cm1 = new SqlBulkCopyColumnMapping("UID", 1);
                SqlBulkCopyColumnMapping cm2 = new SqlBulkCopyColumnMapping("NAME", 2);

                sbc.ColumnMappings.Add(cm1);
                sbc.ColumnMappings.Add(cm2);
                if (ToBeWritten.Columns.Contains("STREAM"))
                {
                    SqlBulkCopyColumnMapping cm3 = new SqlBulkCopyColumnMapping("STREAM", 3);
                    sbc.ColumnMappings.Add(cm3);
                }
                if (ToBeWritten.Columns.Contains("PROGRAM"))
                {
                    SqlBulkCopyColumnMapping cm4 = new SqlBulkCopyColumnMapping("PROGRAM", 4);
                    sbc.ColumnMappings.Add(cm4);
                }

                con.Open();
                //  GridView2.DataSource = ToBeWritten;
                //GridView2.DataBind();

                try
                {
                    sbc.WriteToServer(ToBeWritten);
                }
                catch (SqlException e)
                {
                    if (e.Message.Contains("Violation of PRIMARY KEY constraint") && e.Message.Contains("Cannot insert duplicate key"))
                    { System.Diagnostics.Debug.WriteLine("XXXXXXXXXXXXXXXXXXXXXXXXXXXX"); }
                    else throw new Exception("Some error occured while Writing data into server. Check column mappings.");
                }

                return ToBeWritten.Rows.Count;
            }
        }

        public DataTable convertToDataTable(string XLSFilePath)
        {
            DataTable ExtractedDatafromXLS = null;
            int sheetcount = 0;

            FileInfo XLSfile = new FileInfo(XLSFilePath);
            string OleDBconstr = string.Empty;


            string extension = XLSfile.Extension;
            switch (extension)
            {
                case ".xls":
                    OleDBconstr = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + XLSFilePath + ";Extended Properties='Excel 8.0;HDR=Yes;IMEX=1;'";
                    break;
                case ".xlsx":
                    OleDBconstr = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + XLSFilePath + ";Extended Properties='Excel 12.0;HDR=Yes;IMEX=1;'";
                    break;
                default:
                    OleDBconstr = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + XLSFilePath + ";Extended Properties='Excel 8.0;HDR=Yes;IMEX=1;'";
                    break;
            }


            using (OleDbConnection conxls = new OleDbConnection(OleDBconstr))
            {

                conxls.Open();
                DataTable dt = conxls.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);

                string sheetName = string.Empty;

                if (dt != null)
                {
                    var tempTable = (from DataRow in dt.AsEnumerable() where !DataRow["Table_Name"].ToString().Contains("FilterDatabase") select DataRow).CopyToDataTable();
                    dt = tempTable;

                    sheetcount = dt.Rows.Count;

                    sheetName = dt.Rows[0]["Table_Name"].ToString();

                }
                OleDbDataAdapter oda = new OleDbDataAdapter("SELECT * FROM [" + sheetName + "]", conxls);
                DataSet ds = new DataSet();
                oda.Fill(ds, "NamesDataSet");
                ExtractedDatafromXLS = ds.Tables["NamesDataSet"];

                conxls.Close();

                return ExtractedDatafromXLS;
            }


        }

        protected void btn_NameCanceller_Click(object sender, EventArgs e)
        {
            Response.Redirect(Request.RawUrl);
        }

        protected void btn_ConfirmWrite_Click(object sender, EventArgs e)
        {
            if (!fuc_NamesFile.HasFile)
                lbl_NameKeeper.Text = "No file selected";
            else
            {
                ///START SERVER EXPORT HERE
                string timeticks = DateTime.Now.Ticks.ToString();
                string FilePath = timeticks + "_" + fuc_NamesFile.FileName;
                fuc_NamesFile.SaveAs(Server.MapPath("Name Sheets") + "\\" + FilePath);

                lbl_NameKeeper.Text = WriteXLStoServer(Server.MapPath("Name Sheets") + "\\" + FilePath) + " names added";
                cleanNamesKeeper();//NAMESKEEPER CLEANER
                btn_NameCanceller.Text = "Done and Cleaned";
                btn_NameCanceller.Visible = true;
                btn_ConfirmWrite.Visible = false;
                pnl_UpdationControls.Visible = false;
            }
        }

        protected void Resetter_Click(object sender, EventArgs e)
        {
            if (Resetter.CommandName == "Confirmation")
            {
                Resetter.Text = "Sure? This can not be undone";
                Resetter.BackColor = System.Drawing.Color.Red;
                btn_ResetCancel.Visible = true;
                Resetter.CommandName = "Execute";
            }
            else if (Resetter.CommandName == "Execute")
            {
                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
                {
                    SqlCommand cmd = new SqlCommand("truncate table QuestionPapersDump1", con);
                    con.Open();
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = "truncate table Papers1";
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = "truncate table CAPapers1";
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = "truncate table RepetitionsKeeper";
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = "truncate table HardTypedPapers1";
                    cmd.ExecuteNonQuery();

                    DateTime Time = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, IST);

                    cmd.CommandText = "insert into ResetHistory values('"+Time.ToShortDateString()+" "+Time.ToShortTimeString()+"','Success')";//skipping error part
                    //cmd.CommandText = "insert into ResetHistory values('"+Time.Ticks.ToString()+"','Success')";//skipping error part
                    cmd.ExecuteNonQuery();

                    Resetter.Text = "Command Executed Successfully";
                    Resetter.BackColor = System.Drawing.Color.ForestGreen;

                    btn_ResetCancel.Visible = false;

                    Resetter.CommandName = "Done";
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("----------------------------------In here----------------------------------");
            }
        }

        protected void btn_ResetCancel_Click(object sender, EventArgs e)
        {
            Response.Redirect(Request.RawUrl);
        }

        public void cleanNamesKeeper()
        {
            SqlConnection BackupCon = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString);
            if (BackupCon.State == ConnectionState.Open)
            {
                BackupCon.Close();
                BackupCon.Open();
            }
            else
                BackupCon.Open();

            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.Connection = BackupCon;
                cmd.CommandText = "CleanNamesKeeper";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.ExecuteNonQuery();
                BackupCon.Close();
            }
        }

        protected string giveTimeSince(string DateTimeString)
        {
            DateTime Now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, IST);
            TimeSpan TimeElapsed = Now - Convert.ToDateTime(DateTimeString);
            
            string days = TimeElapsed.Days != 0 ? TimeElapsed.Days > 1 ? TimeElapsed.Days+" days " : TimeElapsed.Days + " day " : string.Empty;
            string hours = TimeElapsed.Hours != 0 ? TimeElapsed.Hours > 1 ? TimeElapsed.Hours+" hours " : TimeElapsed.Hours + " hour " : string.Empty;
            string minutes = TimeElapsed.Minutes == 1 ? TimeElapsed.Minutes + " minute ago" : TimeElapsed.Minutes + " minutes ago";

            return days + hours + minutes;
        }
    }
}