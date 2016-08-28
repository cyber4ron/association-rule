using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace apriori
{
    public class DBAccess
    {
        string cs;

        public DBAccess(string s)
        {
            cs = s;
        }

        public DataTable ReadData(string s)
        {
            string sql = s;

            using (SqlConnection conn = new SqlConnection(cs))
            {
                conn.Open();

                DataSet ds = new DataSet();
                SqlDataAdapter da = new SqlDataAdapter(sql, conn);

                da.Fill(ds, "t");

                conn.Close();

                return ds.Tables["t"];
            }
        }
    }
}
