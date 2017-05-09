using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace SmartCodeGenerator.Uility
{
    /// <summary>
    /// 
    /// </summary>
    public class SqlDBHelper
    {
        public string SqlConnectionString = string.Empty;
        private SqlConnection conn;
        private SqlDataAdapter sda;
        private SqlDataReader sdr;
        private SqlCommand cmd;
        private SqlParameter param;
        private DataSet ds;
        private DataView dv;

        public SqlDBHelper( )
        { 
        }

        /// <summary>
        /// 
        /// </summary>
        public SqlDBHelper(string connStr)
        {
            this.SqlConnectionString = connStr;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="server"></param>
        /// <param name="database"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="trusted"></param>
        public void SetConnection(string server, string database, string username, string password, bool trusted)
        {
            if (trusted)
            {
                SqlConnectionString = @"Server=" + server + ";Database=" + database + ";Trusted_Connection=True;";
            }
            else
            {
                SqlConnectionString = @"Server=" + server + ";Database=" + database + ";User ID=" + username + ";Password=" + password + ";Trusted_Connection=True;";
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool Open()
        {
            try
            {
                conn = new SqlConnection(SqlConnectionString);
                conn.Open();
                return true;
            }
            catch
            {
                return false;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public void Close()
        {
            if (conn != null)
            {
                conn.Close();
                conn.Dispose();
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="strSql"></param>
        /// <returns></returns>
        public DataSet GetDs(string strSql)
        {
            Open();
            sda = new SqlDataAdapter(strSql, conn);
            ds = new DataSet();
            sda.Fill(ds);
            Close();
            return ds;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ds"></param>
        /// <param name="strSql"></param>
        /// <param name="strTableName"></param>
        public void GetDs(DataSet ds, string strSql, string strTableName)
        {
            Open();
            sda = new SqlDataAdapter(strSql, conn);
            sda.Fill(ds, strTableName);
            Close();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="strSql"></param>
        /// <returns></returns>
        public DataView GetDv(string strSql)
        {
            dv = GetDs(strSql).Tables[0].DefaultView;
            return dv;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="strSql"></param>
        /// <returns></returns>
        public DataTable GetTable(string strSql)
        {
            return GetDs(strSql).Tables[0];
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="strSql"></param>
        /// <returns></returns>
        public SqlDataReader GetDataReader(string strSql)
        {
            #region
            Open();
            cmd = new SqlCommand(strSql, conn);
            sdr = cmd.ExecuteReader(System.Data.CommandBehavior.CloseConnection);
            return sdr;
            #endregion
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="strSql"></param>
        public void RunSql(string strSql)
        {
            Open();
            cmd = new SqlCommand(strSql, conn);
            cmd.ExecuteNonQuery();
            Close();
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="strSql"></param>
        /// <returns></returns>
        public string RunSqlReturn(string strSql)
        {
            string strReturn = "";
            Open();
            try
            {
                cmd = new SqlCommand(strSql, conn);
                strReturn = cmd.ExecuteScalar().ToString();
            }
            catch { }
            Close();
            return strReturn;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="procName"></param>
        /// <returns></returns>
        public int RunProc(string procName)
        {
            cmd = CreateCommand(procName, null);
            cmd.ExecuteNonQuery();
            Close();
            return (int)cmd.Parameters["ReturnValue"].Value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="procName"></param>
        /// <param name="prams"></param>
        /// <returns></returns>
        public int RunProc(string procName, SqlParameter[] prams)
        {
            cmd = CreateCommand(procName, prams);
            cmd.ExecuteNonQuery();
            Close();
            return (int)cmd.Parameters["ReturnValue"].Value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="procName"></param>
        /// <param name="dataReader"></param>
        public void RunProc(string procName, SqlDataReader dataReader)
        {
            cmd = CreateCommand(procName, null);
            dataReader = cmd.ExecuteReader(System.Data.CommandBehavior.CloseConnection);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="procName"></param>
        /// <param name="prams"></param>
        /// <param name="dataReader"></param>
        public void RunProc(string procName, SqlParameter[] prams, SqlDataReader dataReader)
        {
            cmd = CreateCommand(procName, prams);
            dataReader = cmd.ExecuteReader(System.Data.CommandBehavior.CloseConnection);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="storedProcName"></param>
        /// <param name="parameters"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public DataSet RunProcedure(string storedProcName, IDataParameter[] parameters, string tableName)
        {
            using (SqlConnection connection = new SqlConnection(this.SqlConnectionString))
            {
                DataSet dataSet = new DataSet();
                try
                {
                    connection.Open();
                    SqlDataAdapter sqlDA = new SqlDataAdapter();
                    sqlDA.SelectCommand = BuildQueryCommand(connection, storedProcName, parameters);
                    sqlDA.Fill(dataSet, tableName);
                    connection.Close();
                    return dataSet;
                }
                catch (System.Data.SqlClient.SqlException ex)
                {
                    throw new Exception(ex.Message);
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="procName"></param>
        /// <param name="prams"></param>
        /// <returns></returns>
        private SqlCommand CreateCommand(string procName, SqlParameter[] prams)
        {

            Open();
            cmd = new SqlCommand(procName, conn);
            cmd.CommandType = CommandType.StoredProcedure;


            if (prams != null)
            {
                foreach (SqlParameter parameter in prams)
                    cmd.Parameters.Add(parameter);
            }

            cmd.Parameters.Add(
                new SqlParameter("ReturnValue", SqlDbType.Int, 4,
                ParameterDirection.ReturnValue, false, 0, 0,
                string.Empty, DataRowVersion.Default, null));

            return cmd;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="storedProcName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private static SqlCommand BuildQueryCommand(SqlConnection connection, string storedProcName, IDataParameter[] parameters)
        {
            SqlCommand command = new SqlCommand(storedProcName, connection);
            command.CommandType = CommandType.StoredProcedure;
            foreach (SqlParameter parameter in parameters)
            {
                if (parameter != null)
                {
                    // Check the value of the output parameters are not assigned, assign it to DBNull.Value.
                    if ((parameter.Direction == ParameterDirection.InputOutput || parameter.Direction == ParameterDirection.Input) && (parameter.Value == null))
                    {
                        parameter.Value = DBNull.Value;
                    }
                    command.Parameters.Add(parameter);
                }
            }

            return command;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ParamName"></param>
        /// <param name="DbType"></param>
        /// <param name="Size"></param>
        /// <param name="Value"></param>
        /// <returns></returns>
        public SqlParameter MakeInParam(string ParamName, SqlDbType DbType, int Size, object Value)
        {
            return MakeParam(ParamName, DbType, Size, ParameterDirection.Input, Value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ParamName"></param>
        /// <param name="DbType"></param>
        /// <param name="Size"></param>
        /// <returns></returns>
        public SqlParameter MakeOutParam(string ParamName, SqlDbType DbType, int Size)
        {
            return MakeParam(ParamName, DbType, Size, ParameterDirection.Output, null);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="ParamName"></param>
        /// <param name="DbType"></param>
        /// <param name="Size"></param>
        /// <param name="Direction"></param>
        /// <param name="Value"></param>
        /// <returns></returns>
        public SqlParameter MakeParam(string ParamName, SqlDbType DbType, Int32 Size, ParameterDirection Direction, object Value)
        {
            if (Size > 0)
                param = new SqlParameter(ParamName, DbType, Size);
            else
                param = new SqlParameter(ParamName, DbType);

            param.Direction = Direction;
            if (!(Direction == ParameterDirection.Output && Value == null))
                param.Value = Value;

            return param;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public DataTable GetTables(string sql)
        {
            Open();
            DataTable schemaTable;
            cmd = new SqlCommand(sql, conn);

            sdr = cmd.ExecuteReader(CommandBehavior.SchemaOnly);

            schemaTable = sdr.GetSchemaTable();
            Close();
            return schemaTable;
        }

        public DataTable GetTableSchema(string tbName)
        {
            Open();
            DataTable schemaTable;
            cmd = new SqlCommand("select * from [" + tbName  + "]", conn);

            sdr = cmd.ExecuteReader(CommandBehavior.SchemaOnly);

            schemaTable = sdr.GetSchemaTable();
            Close();
            return schemaTable;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public DataTable GetTables()
        {
            Open();
            DataTable dt;
            dt = GetTable("select name from sysobjects where type ='U' order by name ");
            Close();
            return dt;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public DataTable GetViews()
        {
            Open();
            DataTable dt;
            dt = GetTable("select name from sysobjects where type ='V'  order by name ");
            Close();
            return dt;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public DataTable GetDatabases()
        {
            Open();
            DataTable dt;
            dt = GetTable("select name  from master..sysdatabases");
            Close();
            return dt;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public DataTable GetPKs(string tableName)
        {
            SqlParameter[] parameters = {
	                new SqlParameter("@table_name", SqlDbType.VarChar, 50) };
            parameters[0].Value = tableName;


            DataSet ds = RunProcedure("sp_pkeys", parameters, "ds");
            return ds.Tables[0];
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetCurrentDatabasename()
        {
            return this.conn.Database;
        }
    }
}
