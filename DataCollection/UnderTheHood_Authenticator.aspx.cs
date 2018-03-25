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
    public partial class UnderTheHood_Authenticator : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["AdminLogged"] != null)
                if (Session["AdminLogged"].ToString() == "Y") Response.Redirect("UnderTheHood.aspx");
        }

        protected void btn_logger_Click(object sender, EventArgs e)
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand("UnderTheHoodAuthenticator", con))
                {
                    con.Open();

                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@passkey",tb_PassKey.Text.Trim());

                    if (Convert.ToInt32(cmd.ExecuteScalar().ToString()) == 1)
                    {
                        Session["AdminLogged"] = "Y";
                        Response.Redirect("UnderTheHood.aspx");
                    }
                    else
                        lbl_LoginMsg.Text = "Invalid PassKey!";

                }
            }
        }
    }
}