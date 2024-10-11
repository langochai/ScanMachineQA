using System;
using System.Data;
using System.Data.SqlClient;

namespace Common
{
    public class SQLUtilities
    {
        private static readonly string ConnectionString = Global.ConnectionString;
        public static int Timeout = 20000;
        private static readonly DateTime _minDate = new DateTime(1900, 01, 01);

        public static DataTable GetDataTableFromSP(string procedureName, string[] paramName, object[] paramValue)
        {
            DataTable table = new DataTable();
            SqlConnection mySqlConnection = new SqlConnection(ConnectionString);
            SqlParameter sqlParam;
            mySqlConnection.Open();

            try
            {
                SqlCommand mySqlCommand = new SqlCommand(procedureName, mySqlConnection);
                mySqlCommand.CommandType = CommandType.StoredProcedure;
                SqlDataAdapter mySqlDataAdapter = new SqlDataAdapter(mySqlCommand);

                DataSet myDataSet = new DataSet();
                if (paramName != null)
                {
                    for (int i = 0; i < paramName.Length; i++)
                    {
                        sqlParam = new SqlParameter(paramName[i], paramValue[i]);
                        mySqlCommand.Parameters.Add(sqlParam);
                    }
                }

                mySqlDataAdapter.Fill(myDataSet);

                table = myDataSet.Tables[0];
            }
            catch (SqlException e)
            {
                throw new Exception(e.ToString());
            }
            finally
            {
                mySqlConnection.Close();
            }

            return table;
        }

        public static DataSet GetDataSetFromSP(string procedureName, string[] paramName, object[] paramValue)
        {
            DataSet myDataSet = new DataSet();
            SqlConnection mySqlConnection = new SqlConnection(ConnectionString);
            SqlParameter sqlParam;
            try
            {
                mySqlConnection.Open();

                SqlCommand mySqlCommand = new SqlCommand(procedureName, mySqlConnection);
                mySqlCommand.CommandType = CommandType.StoredProcedure;
                SqlDataAdapter mySqlDataAdapter = new SqlDataAdapter(mySqlCommand);

                if (paramName != null)
                {
                    for (int i = 0; i < paramName.Length; i++)
                    {
                        sqlParam = new SqlParameter(paramName[i], paramValue[i]);
                        mySqlCommand.Parameters.Add(sqlParam);
                    }
                }
                mySqlDataAdapter.Fill(myDataSet);
            }
            catch (SqlException e)
            {
                throw new Exception(e.ToString());
            }
            finally
            {
                mySqlConnection.Close();
            }
            return myDataSet;
        }

        public static DataTable GetDataTableFromQuery(string sqlQuery)
        {
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();
            try
            {
                SqlConnection con = new SqlConnection(ConnectionString);
                SqlDataAdapter da = new SqlDataAdapter(sqlQuery, con);

                con.Open();
                da.Fill(ds);
                dt = ds.Tables[0];
                con.Close();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }

            return dt;
        }

        public static void ExcuteProcedure(string storeProcedureName, string[] paramName, object[] paramValue)
        {
            SqlConnection cn = new SqlConnection(ConnectionString);
            try
            {
                SqlCommand cmd = new SqlCommand(storeProcedureName, cn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 0;
                SqlParameter sqlParam;
                cn.Open();
                if (paramName != null)
                {
                    for (int i = 0; i < paramName.Length; i++)
                    {
                        sqlParam = new SqlParameter(paramName[i], paramValue[i]);
                        cmd.Parameters.Add(sqlParam);
                    }
                }
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            finally
            {
                cn.Close();
            }
        }
    }
}