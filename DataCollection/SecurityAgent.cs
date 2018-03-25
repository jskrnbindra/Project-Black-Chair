using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace DataCollection
{
    public class SecurityAgent
    {
        public string CurrentUserGroup;
        static TimeZoneInfo IST = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

        public SecurityAgent()
        {
            CurrentUserGroup = giveCurrentUserGroup();
        }

        public bool isBlackChairOpenToNewUsers()
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("Select acceptNewCookies from security", con);
                return Convert.ToInt32(cmd.ExecuteScalar().ToString()) == 1 ? true : false;
            }
        }

        public void newUserAdded()
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("Update security set Connections=Connections+1", con);
                cmd.ExecuteNonQuery();
            }
        }

        public string giveCurrentUserGroup()
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("select top 1(UserGroup) from UserGroups order by serial desc", con);
                return cmd.ExecuteScalar().ToString();
            }
        }

        public string getNewUser()
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("Select top 1(UserName) from Users order by serial desc", con);
                return cmd.ExecuteScalar().ToString();
            }
        }

        public int getPermissionsLevelfor(string UserName)//UserName is encrypted
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("Select PermissionsLevel from Users where UserName='" + UserName + "'", con);
                return Convert.ToInt32(cmd.ExecuteScalar().ToString());
            }
        }

        public bool isValidUserName(string UserName)
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("Select count(*) from users where username='" + UserName + "'", con);
                return Convert.ToInt32(cmd.ExecuteScalar().ToString()) > 0 ? true : false;
            }
        }
/*
        public void forceBackup()///////DROPPED
        {
            DataSecurity ds = new DataSecurity();
            try
            {
                ds.pushToHTTP(fillDataTable("QuestionPapersDump"));
                ds.pushToHTTP(fillDataTable("Papers"));
                ds.pushToHTTP(fillDataTable("CAPapers"));
                logBackupActivity(true);
            }
            catch (Exception)
            {
                logBackupActivity(false);
            }
        }
        */
        protected DataTable fillDataTable(string TableName)
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                SqlDataAdapter ada = new SqlDataAdapter("Select * from " + TableName + " order by Serial", con);

                DataTable dt_Temp = new DataTable();
                con.Open();
                ada.Fill(dt_Temp);
                return dt_Temp;
            }
        }

        protected void logBackupActivity(bool Success)
        {
            DateTime Time = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, IST);

            string status = Success ? "Success" : "Failed";
            string errmsg = Success ? "Not Applicable" : "Some Error Occurred";

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                SqlCommand LogsUpdater = new SqlCommand("INSERT INTO Logs_BackUp(\"DB Object\",TimeStamp,Status,RowsUpdated,\"Error Message\") values ('Forced_ALL','" + Time.ToLongDateString() + " " + Time.ToLongTimeString() + "','"+status+"','Not Applicable','"+errmsg+"')", con);
                LogsUpdater.ExecuteNonQuery();//skippping error handling for this yet
            }
        }

    }
}