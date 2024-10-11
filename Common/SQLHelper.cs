using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;

namespace Common
{
    public class SQLHelper<T> where T : class, new()
    {
        private static readonly string ConnectionString = Global.ConnectionString;
        public static int Timeout = 20000;
        private static readonly DateTime _minDate = new DateTime(1900, 01, 01);

        #region Get data

        /// <summary>
        /// Check if something exists based on field-value
        /// </summary>
        /// <param name="field">Column to check</param>
        /// <param name="value">Value to check</param>
        public static bool CheckIfExists(string field, string value)
        {
            SqlConnection conn = new SqlConnection(Global.ConnectionString);
            T model = new T();
            Type type = model.GetType();
            string tableName = type.Name.StartsWith("Model") ? type.Name : type.Name.Replace("Model", "");
            string sql = string.Format("SELECT TOP 1 {0} FROM {1} WITH (NOLOCK) WHERE {0} = {2}", field, tableName, value);
            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.CommandType = CommandType.Text;
            cmd.CommandTimeout = 6000;
            SqlDataReader reader = null;
            try
            {
                conn.Open();
                reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                return reader.HasRows;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                conn.Close();
            }
        }

        /// <summary>
        /// Execute stored procedure and return a single row
        /// </summary>
        /// <param name="procedureName"></param>
        /// <param name="paramName"></param>
        /// <param name="paramValue"></param>
        /// <returns>Return a record if found; Return default record if not found</returns>
        /// <exception cref="Exception"></exception>
        public static T SPToModel(string procedureName, string[] paramName, object[] paramValue)
        {
            T model = new T();
            SqlConnection conn = new SqlConnection(ConnectionString);
            try
            {
                SqlCommand cmd = new SqlCommand(procedureName, conn);
                cmd.CommandTimeout = Timeout;
                cmd.CommandType = CommandType.StoredProcedure;
                if (paramName != null)
                {
                    for (int i = 0; i < paramName.Length; i++)
                    {
                        SqlParameter sqlParam = new SqlParameter(paramName[i], paramValue[i]);
                        cmd.Parameters.Add(sqlParam);
                    }
                }

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                model = reader.MapToSingle<T>();
            }
            catch (Exception e)
            {
                throw new Exception(e.ToString());
            }
            finally
            {
                conn.Close();
            }

            return model;
        }

        public static List<T> SPToList(string procedureName, string[] paramName, object[] paramValue)
        {
            List<T> lst = new List<T>();
            SqlConnection conn = new SqlConnection(ConnectionString);
            try
            {
                SqlCommand cmd = new SqlCommand(procedureName, conn);
                cmd.CommandTimeout = Timeout;
                cmd.CommandType = CommandType.StoredProcedure;
                if (paramName != null)
                {
                    for (int i = 0; i < paramName.Length; i++)
                    {
                        SqlParameter sqlParam = new SqlParameter(paramName[i], paramValue[i]);
                        cmd.Parameters.Add(sqlParam);
                    }
                }

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                lst = reader.MapToList<T>();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            finally
            {
                conn.Close();
            }

            return lst;
        }

        public static T SqlToModel(string sql)
        {
            T model = new T();
            SqlConnection conn = new SqlConnection(ConnectionString);
            try
            {
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.CommandTimeout = Timeout;
                cmd.CommandType = CommandType.Text;
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                model = reader.MapToSingle<T>();
            }
            catch (Exception e)
            {
                throw new Exception(e.ToString());
            }
            finally
            {
                conn.Close();
            }

            return model;
        }

        public static List<T> SqlToList(string sql)
        {
            List<T> lst = new List<T>();
            SqlConnection conn = new SqlConnection(ConnectionString);
            try
            {
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.CommandTimeout = Timeout;
                cmd.CommandType = CommandType.Text;

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                lst = reader.MapToList<T>();
            }
            catch (Exception e)
            {
                throw new Exception(e.ToString());
            }
            finally
            {
                conn.Close();
            }

            return lst;
        }

        public static T FindByID(int id)
        {
            SqlConnection conn = new SqlConnection(ConnectionString);
            T model = new T();
            Type type = model.GetType();
            string tableName = type.Name.StartsWith("Model") ? type.Name : type.Name.Replace("Model", "");
            try
            {
                string sql = string.Format("SELECT top 1 * FROM [{0}] with (nolock) WHERE ID = {1}", tableName, id);
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.CommandTimeout = Timeout;
                cmd.CommandType = CommandType.Text;
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                model = reader.MapToSingle<T>();
            }
            catch (Exception e)
            {
                throw new Exception(e.ToString());
            }
            finally
            {
                conn.Close();
            }
            return model;
        }

        public static T FindByColumns(string[] fields, object[] values)
        {
            if (fields.Length != values.Length)
                throw new ArgumentException("The number of fields must match the number of values.");

            SqlConnection conn = new SqlConnection(ConnectionString);
            T model = new T();
            Type type = model.GetType();
            string tableName = type.Name.StartsWith("Model") ? type.Name : type.Name.Replace("Model", "");

            try
            {
                string whereClause = string.Join(" AND ", fields.Select((field, index) => $"[{field}] = @value{index}"));
                string sql = $"SELECT TOP 1 * FROM [{tableName}] WITH (NOLOCK) WHERE {whereClause}";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.CommandTimeout = Timeout;
                cmd.CommandType = CommandType.Text;

                for (int i = 0; i < values.Length; i++)
                {
                    cmd.Parameters.AddWithValue($"@value{i}", values[i]);
                }

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                model = reader.MapToSingle<T>();
            }
            catch (Exception e)
            {
                throw new Exception(e.ToString());
            }
            finally
            {
                conn.Close();
            }

            return model;
        }

        public static List<T> FindAllByColumns(string[] fields, object[] values)
        {
            if (fields.Length != values.Length)
                throw new ArgumentException("The number of fields must match the number of values.");

            SqlConnection conn = new SqlConnection(ConnectionString);
            List<T> models = new List<T>();
            T model = new T();
            Type type = model.GetType();
            string tableName = type.Name.StartsWith("Model") ? type.Name : type.Name.Replace("Model", "");

            try
            {
                string whereClause = string.Join(" AND ", fields.Select((field, index) => $"[{field}] = @value{index}"));
                string sql = $"SELECT * FROM [{tableName}] WITH (NOLOCK) WHERE {whereClause}";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.CommandTimeout = Timeout;
                cmd.CommandType = CommandType.Text;

                for (int i = 0; i < values.Length; i++)
                {
                    cmd.Parameters.AddWithValue($"@value{i}", values[i]);
                }

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    T item = reader.MapToSingle<T>();
                    models.Add(item);
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.ToString());
            }
            finally
            {
                conn.Close();
            }

            return models;
        }

        public static List<T> FindAll()
        {
            SqlConnection conn = new SqlConnection(ConnectionString);
            List<T> lst = new List<T>();
            T model = new T();
            Type type = model.GetType();
            string tableName = type.Name.StartsWith("Model") ? type.Name : type.Name.Replace("Model", "");
            try
            {
                string sql = string.Format("SELECT * FROM [{0}] with (nolock)", tableName);
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.CommandTimeout = Timeout;
                cmd.CommandType = CommandType.Text;
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                lst = reader.MapToList<T>();
            }
            catch (Exception e)
            {
                throw new Exception(e.ToString());
            }
            finally
            {
                conn.Close();
            }

            return lst;
        }

        #endregion Get data

        #region Generate queries with model

        public static string SQLInsert(T model)
        {
            Type type = model.GetType();
            string tableName = type.Name.StartsWith("Model") ? type.Name : type.Name.Replace("Model", "");

            string Insert = "insert into " + tableName + " (";
            PropertyInfo[] pis = type.GetProperties();

            for (int i = 0; i < pis.Length; i++)
            {
                if (!pis[i].Name.Equals("ID"))
                {
                    Insert = Insert + pis[i].Name;
                    Insert = Insert + ",";
                }
            }
            Insert = Insert.Substring(0, Insert.Length - 1);
            Insert = Insert + ") values (";
            for (int i = 0; i < pis.Length; i++)
            {
                if (!pis[i].Name.Equals("ID"))
                {
                    Insert = Insert + "@";
                    Insert = Insert + pis[i].Name;
                    Insert = Insert + ",";
                }
            }
            Insert = Insert.Substring(0, Insert.Length - 1);
            Insert = Insert + ") Select Scope_Identity()";
            return Insert;
        }

        public static string SQLUpdate(T model)
        {
            Type type = model.GetType();
            string tableName = type.Name.StartsWith("Model") ? type.Name : type.Name.Replace("Model", "");
            string Update = "UPDATE " + tableName + " SET ";
            PropertyInfo[] pis = type.GetProperties();

            for (int i = 0; i < pis.Length; i++)
            {
                if (!pis[i].Name.Equals("ID"))
                {
                    Update = Update + pis[i].Name;
                    Update = Update + "=@" + pis[i].Name;
                    Update = Update + ",";
                }
            }
            Update = Update.Substring(0, Update.Length - 1);
            Update = Update + " WHERE ID=" + type.GetProperty("ID").GetValue(model, null).ToString();

            return Update;
        }

        public static string SQLUpdate(T model, string field)
        {
            Type type = model.GetType();
            string tableName = type.Name.StartsWith("Model") ? type.Name : type.Name.Replace("Model", "");
            //tableName = tableName.Substring(0, tableName.Length - 5);

            string Update = "UPDATE " + tableName + " SET ";
            PropertyInfo[] pis = type.GetProperties();
            for (int i = 0; i < pis.Length; i++)
            {
                if (pis[i].Name.Equals(field))
                {
                    Update = Update + pis[i].Name;
                    Update = Update + "=@" + pis[i].Name;
                    Update = Update + ",";
                    break;
                }
            }
            Update = Update.Substring(0, Update.Length - 1);
            Update = Update + " WHERE ID=" + type.GetProperty("ID").GetValue(model, null).ToString();
            return Update;
        }

        public static string SQLDelete(T model)
        {
            Type type = model.GetType();
            string tableName = type.Name.StartsWith("Model") ? type.Name : type.Name.Replace("Model", "");
            PropertyInfo pi = type.GetProperty("ID");
            string Delete = "DELETE FROM " + tableName + " WHERE ID = " + pi.GetValue(model);
            return Delete;
        }

        #endregion Generate queries with model

        #region Execute queries with model

        public static ResultQuery Insert(T model)
        {
            ResultQuery r = new ResultQuery();
            r.TotalRow = 1;
            Type type = model.GetType();
            SqlConnection conn = new SqlConnection(ConnectionString);
            try
            {
                string sql = SQLInsert(model);
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandTimeout = Timeout;
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = sql;

                PropertyInfo[] propertiesName = type.GetProperties();
                for (int i = 0; i < propertiesName.Length; i++)
                {
                    object value = propertiesName[i].GetValue(model, null);

                    if (!propertiesName[i].Name.Equals("ID") && !propertiesName[i].Name.Equals("iD"))
                    {
                        if (propertiesName[i].Name.ToLower().Equals("createdby") || propertiesName[i].Name.ToLower().Equals("updatedby"))
                        {
                            cmd.Parameters.Add("@" + propertiesName[i].Name, SqlDbType.NVarChar).Value = !String.IsNullOrEmpty(Global.AppUserName) ? Global.AppUserName : (value ?? "");
                        }
                        else if (propertiesName[i].Name.ToLower().Equals("createddate") || propertiesName[i].Name.ToLower().Equals("updateddate"))
                        {
                            cmd.Parameters.Add("@" + propertiesName[i].Name, SqlDbType.DateTime).Value = DateTime.Now;
                        }
                        else if (propertiesName[i].Name.ToLower().Equals("userinsertid") || propertiesName[i].Name.ToLower().Equals("userupdateid"))
                        {
                            cmd.Parameters.Add("@" + propertiesName[i].Name, SqlDbType.Int).Value = Global.UserID != 0 ? Global.UserID : (value ?? 0);
                        }
                        else if (value != null)
                        {
                            if (propertiesName[i].PropertyType.Equals(typeof(DateTime)))
                            {
                                if ((DateTime)value == DateTime.MinValue)
                                    value = _minDate; ;
                            }
                            if (propertiesName[i].PropertyType.Name.Equals("Byte[]"))
                            {
                                cmd.Parameters.Add("@" + propertiesName[i].Name, SqlDbType.Image).Value = value;
                            }
                            else
                            {
                                cmd.Parameters.Add("@" + propertiesName[i].Name, ConvertToSQLType(propertiesName[i].PropertyType)).Value = value;
                            }
                        }
                        else
                        {
                            if (propertiesName[i].PropertyType.Equals(typeof(DateTime?)))
                            {
                                cmd.Parameters.Add("@" + propertiesName[i].Name, ConvertToSQLType(propertiesName[i].PropertyType)).Value = DBNull.Value;
                            }
                            else
                            {
                                cmd.Parameters.Add("@" + propertiesName[i].Name, ConvertToSQLType(propertiesName[i].PropertyType)).Value = "";
                            }
                        }
                    }
                }

                conn.Open();
                r.ID = (int)(decimal)cmd.ExecuteScalar();
                r.IsSuccess = true;
            }
            catch (Exception ex)
            {
                r.IsSuccess = false;
                r.ErrorText = ex.ToString();
                throw new Exception(ex.Message);
            }
            finally
            {
                if (conn.State != ConnectionState.Closed) conn.Close();
                conn.Dispose();
            }
            return r;
        }

        public static ResultQuery Update(T model)
        {
            ResultQuery r = new ResultQuery();
            r.TotalRow = 0;
            Type type = model.GetType();
            SqlConnection conn = new SqlConnection(ConnectionString);
            try
            {
                string sql = SQLUpdate(model);
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandTimeout = Timeout;
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = sql;

                PropertyInfo[] propertiesName = type.GetProperties();
                for (int i = 0; i < propertiesName.Length; i++)
                {
                    SqlDbType dbType = ConvertToSQLType(propertiesName[i].PropertyType);
                    object value = propertiesName[i].GetValue(model, null);

                    if (propertiesName[i].Name.ToLower().Equals("updatedby"))
                    {
                        cmd.Parameters.Add("@" + propertiesName[i].Name, SqlDbType.NVarChar).Value = !String.IsNullOrEmpty(Global.AppUserName) ? Global.AppUserName : (value ?? "");
                    }
                    else if (propertiesName[i].Name.ToLower().Equals("updateddate"))
                    {
                        cmd.Parameters.Add("@" + propertiesName[i].Name, SqlDbType.DateTime).Value = DateTime.Now;
                    }
                    else if (propertiesName[i].Name.ToLower().Equals("userupdateid"))
                    {
                        cmd.Parameters.Add("@" + propertiesName[i].Name, SqlDbType.Int).Value = Global.UserID != 0 ? Global.UserID : (value ?? 0);
                    }
                    else if (value != null)
                    {
                        if (propertiesName[i].PropertyType.Equals(typeof(DateTime)))
                        {
                            if ((DateTime)value == DateTime.MinValue)
                                value = _minDate;
                        }
                        if (propertiesName[i].PropertyType.Name.Equals("Byte[]"))
                        {
                            cmd.Parameters.Add("@" + propertiesName[i].Name, SqlDbType.Image).Value = value;
                        }
                        else
                        {
                            cmd.Parameters.Add("@" + propertiesName[i].Name, dbType).Value = value;
                        }
                    }
                    else
                    {
                        if (propertiesName[i].PropertyType.Equals(typeof(DateTime?)))
                        {
                            cmd.Parameters.Add("@" + propertiesName[i].Name, dbType).Value = DBNull.Value;
                        }
                        else
                            cmd.Parameters.Add("@" + propertiesName[i].Name, dbType).Value = "";
                    }

                    //if (value != null)
                    //{
                    //    if (pis[i].PropertyType.Equals(typeof(DateTime)))
                    //    {
                    //        if ((DateTime)value == DateTime.MinValue)
                    //            value = _minDate;
                    //    }
                    //    else
                    //    {
                    //        cmd.Parameters.Add("@" + pis[i].Name, dbType).Value = value;
                    //    }
                    //}
                    //else
                    //{
                    //    if (pis[i].PropertyType.Equals(typeof(DateTime?)))
                    //    {
                    //        cmd.Parameters.Add("@" + pis[i].Name, dbType).Value = DBNull.Value;
                    //    }
                    //    else
                    //        cmd.Parameters.Add("@" + pis[i].Name, dbType).Value = "";
                    //}
                }
                conn.Open();
                r.TotalRow = cmd.ExecuteNonQuery();
                r.IsSuccess = true;
            }
            catch (Exception ex)
            {
                r.IsSuccess = false;
                r.ErrorText = ex.ToString();
                throw new Exception(ex.Message);
            }
            finally
            {
                if (conn.State != ConnectionState.Closed) conn.Close();
                conn.Dispose();
            }
            return r;
        }

        public static ResultQuery Update(T model, string field)
        {
            ResultQuery r = new ResultQuery();
            r.TotalRow = 0;
            Type type = model.GetType();
            SqlConnection conn = new SqlConnection(ConnectionString);
            try
            {
                string sql = SQLUpdate(model, field);
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandTimeout = Timeout;
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = sql;

                PropertyInfo[] pis = type.GetProperties();
                for (int i = 0; i < pis.Length; i++)
                {
                    SqlDbType dbType = ConvertToSQLType(pis[i].PropertyType);
                    object value = pis[i].GetValue(model, null);
                    if (pis[i].Name.Equals(field))
                    {
                        if (value != null)
                        {
                            if (pis[i].PropertyType.Equals(typeof(DateTime)))
                            {
                                if ((DateTime)value == DateTime.MinValue)
                                    value = _minDate;
                            }
                            else
                            {
                                cmd.Parameters.Add("@" + pis[i].Name, dbType).Value = value;
                            }
                        }
                        else
                            cmd.Parameters.Add("@" + pis[i].Name, dbType).Value = "";
                        break;
                    }
                }
                conn.Open();
                r.TotalRow = cmd.ExecuteNonQuery();
                r.IsSuccess = true;
            }
            catch (Exception ex)
            {
                r.IsSuccess = false;
                r.ErrorText = ex.ToString();
            }
            finally
            {
                if (conn.State != ConnectionState.Closed) conn.Close();
                conn.Dispose();
            }
            return r;
        }

        public static ResultQuery Delete(T model)
        {
            ResultQuery r = new ResultQuery();
            r.TotalRow = 0;
            SqlConnection conn = new SqlConnection(ConnectionString);
            try
            {
                string sql = SQLDelete(model);
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = Timeout;
                cmd.Connection.Open();
                r.TotalRow = cmd.ExecuteNonQuery();
                r.IsSuccess = true;
            }
            catch (Exception ex)
            {
                r.IsSuccess = false;
                r.ErrorText = ex.ToString();
            }
            finally
            {
                if (conn.State != ConnectionState.Closed) conn.Close();
                conn.Dispose();
            }
            return r;
        }

        public static void DeleteModelByID(int id)
        {
            SqlConnection conn = new SqlConnection(ConnectionString);
            T model = new T();
            Type type = model.GetType();
            string tableName = type.Name.StartsWith("Model") ? type.Name : type.Name.Replace("Model", "");
            try
            {
                string sql = string.Format("DELETE FROM [{0}] WHERE ID = {1}", tableName, id);
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.CommandTimeout = Timeout;
                cmd.CommandType = CommandType.Text;
                conn.Open();
                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw new Exception(e.ToString());
            }
            finally
            {
                conn.Close();
            }
        }

        public static void DeleteListModel(List<T> models)
        {
            if (models == null || !models.Any())
            {
                throw new ArgumentException("The collection of models is null or empty.");
            }

            Type type = typeof(T);
            string tableName = type.Name.StartsWith("Model") ? type.Name : type.Name.Replace("Model", "");
            var ids = models.Select(model => type.GetProperty("ID").GetValue(model)).ToList();
            string sql = $"DELETE FROM {tableName} WHERE ID IN ({string.Join(", ", ids)})";
            ExcuteNonQuerySQL(sql);
        }

        #endregion Execute queries with model

        #region Utilities

        /// <summary>
        /// Execute query and return number of affected rows
        /// </summary>
        /// <param name="sql">SQL query</param>
        /// <returns>Number of affected rows</returns>
        public static int ExcuteNonQuerySQL(string sql)
        {
            SqlConnection conn = new SqlConnection(Global.ConnectionString);
            SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.CommandType = CommandType.Text;
            cmd.CommandTimeout = 6000;
            try
            {
                cmd.Connection.Open();
                return cmd.ExecuteNonQuery();
            }
            catch (SqlException se)
            {
                throw se;
            }
            finally
            {
                conn.Close();
            }
        }

        public static SqlDbType ConvertToSQLType(Type type)
        {
            if (type == typeof(string))
            {
                return SqlDbType.NVarChar;
            }
            if (type == typeof(int))
            {
                return SqlDbType.Int;
            }
            if (type == typeof(Int16))
            {
                return SqlDbType.TinyInt;
            }
            if (type == typeof(Int64))
            {
                return SqlDbType.BigInt;
            }
            if (type == typeof(DateTime))
            {
                return SqlDbType.DateTime;
            }
            if (type == typeof(DateTime?))
            {
                return SqlDbType.DateTime;
            }
            if (type == typeof(long))
            {
                return SqlDbType.BigInt;
            }
            if (type == typeof(Decimal))
            {
                return SqlDbType.Decimal;
            }
            if (type == typeof(Byte[]))
            {
                return SqlDbType.Image;
            }
            if (type == typeof(Guid))
            {
                return SqlDbType.UniqueIdentifier;
            }
            return SqlDbType.NVarChar;
        }

        #endregion Utilities
    }

    public class ResultQuery
    {
        public int ID { get; set; }
        public int TotalRow { get; set; }
        public bool IsSuccess { get; set; }
        public string ErrorText { get; set; }
    }
}