using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.IO;

namespace DataCollection
{
    public class AutomaticBackup
    {
        public AutomaticBackup()
        {
            //    BackupCon.ConnectionString = ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString;
            //  BackupCon.Open();
        }

        ~AutomaticBackup()
        {
            //BackupCon.Open();
        }

        static string cs = ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString;
        static TimeZoneInfo IST = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

        public bool backupQuestionPapersDump()
        {
            bool isBackupComplete = false;


            SqlConnection BackupCon = new SqlConnection(cs);
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
                cmd.CommandText = "INSERT INTO QuestionPapersDump1 SELECT * FROM QuestionPapersDump where serial not in (select serial from QuestionPapersDump1)";
                int RowsReturned = 0;

                SqlCommand LogsUpdater = new SqlCommand();
                LogsUpdater.Connection = BackupCon;
                string ErrMSG = "";

                try
                {
                    RowsReturned = cmd.ExecuteNonQuery();
                    isBackupComplete = true;
                    ErrMSG = "Not Applicable";
                    DateTime Time = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, IST);
                    LogsUpdater.CommandText = "INSERT INTO Logs_BackUp(\"DB Object\",TimeStamp,Status,RowsUpdated,\"Error Message\") values ('QuestionPapersDump','" + Time.ToLongDateString() + " " + Time.ToLongTimeString() + "','Success'," + RowsReturned + ",'" + ErrMSG + "')";
                }
                catch (Exception e)
                {
                    isBackupComplete = false;
                    ErrMSG = e.Message;
                    System.Diagnostics.Debug.WriteLine(e.Message);
                    DateTime Time = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, IST);
                    LogsUpdater.CommandText = "INSERT INTO Logs_BackUp(\"DB Object\",TimeStamp,Status,RowsUpdated,\"Error Message\") values ('QuestionPapersDump','" + Time.ToLongDateString() + " " + Time.ToLongTimeString() + "','Failed',0,'" + ErrMSG + "')";
                }
                finally
                {
                    LogsUpdater.ExecuteNonQuery();//skiping exception handling for this query for now.
                    BackupCon.Close();
                }
            }

            return isBackupComplete;
        }

        public bool backupPapers()
        {
            bool isBackupComplete = false;

            SqlConnection BackupCon = new SqlConnection(cs);
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
                cmd.CommandText = "INSERT INTO Papers1 SELECT * FROM Papers where  serial not in (select serial from Papers1)";
                int RowsReturned = 0;

                SqlCommand LogsUpdater = new SqlCommand();
                LogsUpdater.Connection = BackupCon;
                string ErrMSG = "";

                try
                {
                    RowsReturned = cmd.ExecuteNonQuery();
                    isBackupComplete = true;
                    ErrMSG = "Not Applicable";
                    DateTime Time = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, IST);
                    LogsUpdater.CommandText = "INSERT INTO Logs_BackUp(\"DB Object\",TimeStamp,Status,RowsUpdated,\"Error Message\") values ('Papers','" + Time.ToLongDateString() + " " + Time.ToLongTimeString() + "','Success'," + RowsReturned + ",'" + ErrMSG + "')";
                }
                catch (Exception e)
                {
                    isBackupComplete = false;
                    ErrMSG = e.Message;
                    System.Diagnostics.Debug.WriteLine(e.Message);
                    DateTime Time = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, IST);
                    LogsUpdater.CommandText = "INSERT INTO Logs_BackUp(\"DB Object\",TimeStamp,Status,RowsUpdated,\"Error Message\") values ('Papers','" + Time.ToLongDateString() + " " + Time.ToLongTimeString() + "','Failed',0,'" + ErrMSG + "')";
                }
                finally
                {
                    LogsUpdater.ExecuteNonQuery();//skiping exception handling for this query for now.
                    BackupCon.Close();
                }
            }

            return isBackupComplete;
        }

        public bool backupCAPapers()
        {
            bool isBackupComplete = false;

            SqlConnection BackupCon = new SqlConnection(cs);
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
                cmd.CommandText = "INSERT INTO CAPapers1 SELECT * FROM CAPapers where  serial not in (select serial from CAPapers1)";
                int RowsReturned = 0;

                SqlCommand LogsUpdater = new SqlCommand();
                LogsUpdater.Connection = BackupCon;
                string ErrMSG = "";

                try
                {
                    RowsReturned = cmd.ExecuteNonQuery();
                    isBackupComplete = true;
                    ErrMSG = "Not Applicable";
                    DateTime Time = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, IST);
                    LogsUpdater.CommandText = "INSERT INTO Logs_BackUp(\"DB Object\",TimeStamp,Status,RowsUpdated,\"Error Message\") values ('CAPapers','" + Time.ToLongDateString() + " " + Time.ToLongTimeString() + "','Success'," + RowsReturned + ",'" + ErrMSG + "')";
                }
                catch (Exception e)
                {
                    isBackupComplete = false;
                    ErrMSG = e.Message;
                    System.Diagnostics.Debug.WriteLine(e.Message);
                    DateTime Time = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, IST);
                    LogsUpdater.CommandText = "INSERT INTO Logs_BackUp(\"DB Object\",TimeStamp,Status,RowsUpdated,\"Error Message\") values ('CAPapers','" + Time.ToLongDateString() + " " + Time.ToLongTimeString() + "','Failed',0,'" + ErrMSG + "')";
                }
                finally
                {
                    LogsUpdater.ExecuteNonQuery();//skiping exception handling for this query for now.
                    BackupCon.Close();
                }
            }

            return isBackupComplete;
        }

        public void performManualBackup()
        {
            backupQuestionPapersDump();
            backupPapers();
            performManualBackup();

        }

        public bool backupHardPapers()
        {
            bool isBackupComplete = false;

            SqlConnection BackupCon = new SqlConnection(cs);
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
                cmd.CommandText = "INSERT INTO HardTypedPapers1 SELECT * FROM HardTypedPapers where  serial not in (select serial from HardTypedPapers1)";
                int RowsReturned = 0;

                SqlCommand LogsUpdater = new SqlCommand();
                LogsUpdater.Connection = BackupCon;
                string ErrMSG = "";

                try
                {
                    RowsReturned = cmd.ExecuteNonQuery();
                    isBackupComplete = true;
                    ErrMSG = "Not Applicable";
                    DateTime Time = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, IST);
                    LogsUpdater.CommandText = "INSERT INTO Logs_BackUp(\"DB Object\",TimeStamp,Status,RowsUpdated,\"Error Message\") values ('HardTypedPapers','" + Time.ToLongDateString() + " " + Time.ToLongTimeString() + "','Success'," + RowsReturned + ",'" + ErrMSG + "')";
                }
                catch (Exception e)
                {
                    isBackupComplete = false;
                    ErrMSG = e.Message;
                    System.Diagnostics.Debug.WriteLine(e.Message);
                    DateTime Time = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, IST);
                    LogsUpdater.CommandText = "INSERT INTO Logs_BackUp(\"DB Object\",TimeStamp,Status,RowsUpdated,\"Error Message\") values ('HardTypedPapers','" + Time.ToLongDateString() + " " + Time.ToLongTimeString() + "','Failed',0,'" + ErrMSG + "')";
                }
                finally
                {
                    LogsUpdater.ExecuteNonQuery();//skiping exception handling for this query for now.
                    BackupCon.Close();
                }
            }

            return isBackupComplete;
        }

        public void backupToExcelFiles()
        {


        }

        public void pushAsExcel(DataTable WorkTable)
        {

        }

        public bool cleanDBofTempTable()
        {
            bool isBackupComplete = false;

            System.Diagnostics.Debug.Write("Cleaning in ");

            SqlConnection BackupCon = new SqlConnection(cs);
            if (BackupCon.State == ConnectionState.Open)
            {
                BackupCon.Close();
                BackupCon.Open();
            }
            else
                BackupCon.Open();

            SqlCommand LogsUpdater = new SqlCommand();
            LogsUpdater.Connection = BackupCon;
            string ErrMSG = "";

            try
            {
                //RowsReturned = cmd.ExecuteNonQuery();
                #region performCleanUP

                DataTable dt = new DataTable();
                using (SqlDataAdapter ada = new SqlDataAdapter("select tblName from TempTableKeeper", BackupCon))
                {
                    //BackupCon.Open();
                    ada.Fill(dt);
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = BackupCon;
                        foreach (DataRow dr in dt.Rows)
                        {
                            cmd.CommandText = "drop table " + dr["tblName"].ToString();
                            System.Diagnostics.Debug.WriteLine("drop table " + dr["tblName"].ToString());
                            cmd.ExecuteNonQuery();
                        }

                        cmd.CommandText = "truncate table TempTableKeeper";
                        cmd.ExecuteNonQuery();
                    }

                }
                #endregion

                isBackupComplete = true;
                ErrMSG = "Not Applicable";
                DateTime Time = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, IST);
                LogsUpdater.CommandText = "INSERT INTO Logs_BackUp(\"DB Object\",TimeStamp,Status,RowsUpdated,\"Error Message\") values ('TempTablesCleanUp','" + Time.ToLongDateString() + " " + Time.ToLongTimeString() + "','Success',0,'" + ErrMSG + "')";
            }
            catch (Exception e)
            {
                isBackupComplete = false;
                ErrMSG = "Some Freaking Error Ocuured";
                System.Diagnostics.Debug.WriteLine(e.Message);
                DateTime Time = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, IST);
                LogsUpdater.CommandText = "INSERT INTO Logs_BackUp(\"DB Object\",TimeStamp,Status,RowsUpdated,\"Error Message\") values ('TempTablesCleanUp','" + Time.ToLongDateString() + " " + Time.ToLongTimeString() + "','Failed',0,'" + ErrMSG + "')";
            }
            finally
            {
                LogsUpdater.ExecuteNonQuery();//skiping exception handling for this query for now.
                BackupCon.Close();
            }
            return isBackupComplete;
        }

        /*
         * Dependent on Microsoft.Office.Interop.Excel
         * Will implement after verifying the computational and time trade offs for the site
         * 
        public void CopyToExcel(DataTable WorkTable, string ExcelFilePath)
        {
            int ColumnCount;
            if (WorkTable == null || (ColumnCount = WorkTable.Columns.Count) == 0)
                throw new Exception("Null or Empty input table to convert to xlsx.");
            

        }
        */
    }
}