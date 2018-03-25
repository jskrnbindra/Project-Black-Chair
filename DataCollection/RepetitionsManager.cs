using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace DataCollection
{
    public class RepetitionsManager
    {
        public bool updateRepetionsKeeper()
        {
            bool isSuccessful = false;

            string cs = ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString;
            TimeZoneInfo IST = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

            SqlConnection RepititionsCon = new SqlConnection(cs);
            if (RepititionsCon.State == ConnectionState.Open)
            {
                RepititionsCon.Close();
                RepititionsCon.Open();
            }
            else
                RepititionsCon.Open();

            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.Connection = RepititionsCon;
                cmd.CommandText = "UpdateRepetitionsKeeper";
                cmd.CommandType = CommandType.StoredProcedure;
                int RowsReturned = 0;

                SqlCommand LogsUpdater = new SqlCommand();
                LogsUpdater.Connection = RepititionsCon;
                string ErrMSG = "";

                try
                {
                    RowsReturned = cmd.ExecuteNonQuery();
                    isSuccessful = true;
                    ErrMSG = "Not Applicable";
                    DateTime Time = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, IST);
                    LogsUpdater.CommandText = "INSERT INTO Logs_BackUp(\"DB Object\",TimeStamp,Status,RowsUpdated,\"Error Message\") values ('RepetitionsKeeper','"+Time.ToLongDateString()+" "+Time.ToLongTimeString()+"','Success',"+RowsReturned+",'"+ErrMSG+"')";
                }
                catch (Exception e)
                {
                    isSuccessful = false;
                    ErrMSG = e.Message;
                    System.Diagnostics.Debug.WriteLine(e.Message);
                    DateTime Time = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, IST);
                    LogsUpdater.CommandText = "INSERT INTO Logs_BackUp(\"DB Object\",TimeStamp,Status,RowsUpdated,\"Error Message\") values ('RepetitionsKeeper','" + Time.ToLongDateString() + " " + Time.ToLongTimeString() + "','Success'," + RowsReturned + ",'" + ErrMSG + "')";
                }
                finally
                {
                    LogsUpdater.ExecuteNonQuery();//skiping exception handling for this query for now.
                    RepititionsCon.Close();
                }
            }
            return isSuccessful;
        }

    }


}