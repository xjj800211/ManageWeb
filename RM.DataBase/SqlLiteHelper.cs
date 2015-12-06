using RM.Common.DotNetCode;
using RM.Common.DotNetData;
using RM.Common.DotNetEncrypt;
using RM.DataBase.DataBase.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;

namespace RM.DataBase
{
    public class SqlLiteHelper : IDbHelper, IDisposable
    {
        protected LogHelper Logger = new LogHelper("SQLServerLog");
        private SQLiteCommand dbCommand = null;
        protected string connectionString = "";
        private static object locker = new object();

        public SQLiteCommand DbCommand
        {
            get
            {
                return this.dbCommand;
            }
            set
            {
                this.dbCommand = value;
            }
        }

        public SqlLiteHelper(string connString)
        {
            this.connectionString = connString;
        }

        public SqlParam[] GetParameter(Hashtable ht)
        {
            SqlParam[] _params = new SqlParam[ht.Count];
            int i = 0;
            foreach (string key in ht.Keys)
            {
                _params[i] = new SqlParam("@" + key, ht[key]);
                i++;
            }
            return _params;
        }

        public object GetObjectValue(StringBuilder sql)
        {
            return this.GetObjectValue(sql, null);
        }

        public object GetObjectValue(StringBuilder sql, SqlParam[] param)
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                using (SQLiteCommand cmd = new SQLiteCommand())
                {
                    try
                    {
                        PrepareCommand(cmd, connection, null, sql.ToString(), param);
                        object obj = cmd.ExecuteScalar();
                        cmd.Parameters.Clear();
                        if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
                        {
                            return null;
                        }
                        else
                        {
                            return obj;
                        }
                    }
                    catch (System.Data.SQLite.SQLiteException e)
                    {
                        throw new Exception(e.Message);
                    }
                }
            }
        }

        public int ExecuteBySql(StringBuilder sql)
        {
            return this.ExecuteBySql(sql, null);
        }

        public int ExecuteBySql(StringBuilder sql, SqlParam[] param)
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                using (SQLiteCommand cmd = new SQLiteCommand())
                {
                    try
                    {
                        PrepareCommand(cmd, connection, null, sql.ToString(), param);
                        int rows = cmd.ExecuteNonQuery();
                        return rows;
                    }
                    catch (System.Data.SQLite.SQLiteException E)
                    {
                        connection.Close();
                        throw new Exception(E.Message);
                    }
                }
            }
        }

        public int BatchExecuteBySql(StringBuilder[] sqls, object[] param)
        {
            int num = 0;
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                SQLiteCommand cmd = new SQLiteCommand();
                cmd.Connection = conn;
                SQLiteTransaction tx = conn.BeginTransaction();
                cmd.Transaction = tx;
                try
                {
                    for (int n = 0; n < sqls.Length; n++)
                    {
                        string strsql = sqls[n].ToString();
                        if (strsql.Trim().Length > 1)
                        {
                            cmd.CommandText = strsql;
                         num= cmd.ExecuteNonQuery();
                        }
                    }
                    tx.Commit();
                }
                catch (System.Data.SQLite.SQLiteException E)
                {
                    tx.Rollback();
                    throw new Exception(E.Message);
                }
            }
            return num;
        }

        public DataTable GetDataTable(string TargetTable)
        {
            SQLiteConnection connection = new SQLiteConnection(connectionString);
            SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM " + TargetTable, connection);
            try
            {
                connection.Open();
                SQLiteDataReader myReader = cmd.ExecuteReader();
                return ReaderToIListHelper.DataTableToIDataReader(myReader);
            }
            catch (System.Data.SQLite.SQLiteException e)
            {
                throw new Exception(e.Message);
            }
        }

        public DataTable GetDataTable(string TargetTable, string orderField, string orderType)
        {
            SQLiteConnection connection = new SQLiteConnection(connectionString);
            SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM " + TargetTable+" order by "+orderField +orderType, connection);
            try
            {
                connection.Open();
                SQLiteDataReader myReader = cmd.ExecuteReader();
                return ReaderToIListHelper.DataTableToIDataReader(myReader);
            }
            catch (System.Data.SQLite.SQLiteException e)
            {
                throw new Exception(e.Message);
            }
        }

        public DataTable GetDataTableBySQL(StringBuilder sql)
        {
            return this.GetDataTableBySQL(sql, null);
        }

        public DataTable GetDataTableBySQL(StringBuilder sql, SqlParam[] param)
        {
            SQLiteConnection connection = new SQLiteConnection(connectionString);
            SQLiteCommand cmd = new SQLiteCommand();
            try
            {
                PrepareCommand(cmd, connection, null, sql.ToString(), param);
                SQLiteDataReader myReader = cmd.ExecuteReader();
                cmd.Parameters.Clear();
                return ReaderToIListHelper.DataTableToIDataReader(myReader);
            }
            catch (System.Data.SQLite.SQLiteException e)
            {
                throw new Exception(e.Message);
            }
        }

        public DataTable GetDataTableProc(string procName, Hashtable ht)
        {
            DataTable result=null;
            return result;
        }

        public DataTable GetDataTableProcReturn(string procName, Hashtable ht, ref Hashtable rs)
        {
            DataTable result=null;

            return result;
        }

        public DataSet GetDataSetBySQL(StringBuilder sql)
        {
            return this.GetDataSetBySQL(sql, null);
        }

        public DataSet GetDataSetBySQL(StringBuilder sql, SqlParam[] param)
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                SQLiteCommand cmd = new SQLiteCommand();
                PrepareCommand(cmd, connection, null, sql.ToString(), param);
                using (SQLiteDataAdapter da = new SQLiteDataAdapter(cmd))
                {
                    DataSet ds = new DataSet();
                    try
                    {
                        da.Fill(ds, "ds");
                        cmd.Parameters.Clear();
                    }
                    catch (System.Data.SQLite.SQLiteException ex)
                    {
                        throw new Exception(ex.Message);
                    }
                    return ds;
                }
            }
        }

        public IList GetDataListBySQL<T>(StringBuilder sql)
        {
            return this.GetDataListBySQL<T>(sql, null);
        }

        public IList GetDataListBySQL<T>(StringBuilder sql, SqlParam[] param)
        {
            SQLiteConnection connection = new SQLiteConnection(connectionString);
            SQLiteCommand cmd = new SQLiteCommand();
            try
            {
                PrepareCommand(cmd, connection, null, sql.ToString(), param);
                SQLiteDataReader myReader = cmd.ExecuteReader();
                cmd.Parameters.Clear();
                return ReaderToIListHelper.ReaderToList<T>(myReader);
            }
            catch (System.Data.SQLite.SQLiteException e)
            {
                throw new Exception(e.Message);
            }
        }

        public int ExecuteByProc(string procName, Hashtable ht)
        {
            int num = 0;
            return num;
        }

        public int ExecuteByProcNotTran(string procName, Hashtable ht)
        {
            int num = 0;
            int result;
            result = num;
            return result;
        }

        public int ExecuteByProcReturn(string procName, Hashtable ht, ref Hashtable rs)
        {
            int num = 0;
            int result;
            result = num;
            return result;
        }

        public int ExecuteByProcReturnMsg(string procName, Hashtable ht, ref object msg)
        {
            int num = 0;
            return num;
        }

        public bool Submit_AddOrEdit(string tableName, string pkName, string pkVal, Hashtable ht)
        {
            bool result;
            if (string.IsNullOrEmpty(pkVal))
            {
                result = (this.InsertByHashtable(tableName, ht) > 0);
            }
            else
            {
                result = (this.UpdateByHashtable(tableName, pkName, pkVal, ht) > 0);
            }
            return result;
        }

        public Hashtable GetHashtableById(string tableName, string pkName, string pkVal)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Select * From ").Append(tableName).Append(" Where ").Append(pkName).Append("=@ID");
            DataTable dt = this.GetDataTableBySQL(sb, new SqlParam[]
			{
				new SqlParam("@ID", pkVal)
			});
            return DataTableHelper.DataTableToHashtable(dt);
        }

        public int IsExist(string tableName, string pkName, string pkVal)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("Select Count(1) from " + tableName);
            strSql.Append(" where " + pkName + " = @" + pkName);
            SqlParam[] param = new SqlParam[]
			{
				new SqlParam("@" + pkName, pkVal)
			};
            return CommonHelper.GetInt(this.GetObjectValue(strSql, param));
        }

        public virtual int InsertByHashtable(string tableName, Hashtable ht)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(" Insert Into ");
            sb.Append(tableName);
            sb.Append("(");
            StringBuilder sp = new StringBuilder();
            StringBuilder sb_prame = new StringBuilder();
            foreach (string key in ht.Keys)
            {
                sb_prame.Append("," + key);
                sp.Append(",@" + key);
            }
            sb.Append(sb_prame.ToString().Substring(1, sb_prame.ToString().Length - 1) + ") Values (");
            sb.Append(sp.ToString().Substring(1, sp.ToString().Length - 1) + ")");
            return this.ExecuteBySql(sb, this.GetParameter(ht));
        }

        public int InsertByHashtableReturnPkVal(string tableName, Hashtable ht)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(" Declare @ReturnValue int Insert Into ");
            sb.Append(tableName);
            sb.Append("(");
            StringBuilder sp = new StringBuilder();
            StringBuilder sb_prame = new StringBuilder();
            foreach (string key in ht.Keys)
            {
                sb_prame.Append("," + key);
                sp.Append(",@" + key);
            }
            sb.Append(sb_prame.ToString().Substring(1, sb_prame.ToString().Length - 1) + ") Values (");
            sb.Append(sp.ToString().Substring(1, sp.ToString().Length - 1) + ") Set @ReturnValue=SCOPE_IDENTITY() Select @ReturnValue");
            object _object = this.GetObjectValue(sb, this.GetParameter(ht));
            return (_object == DBNull.Value) ? 0 : Convert.ToInt32(_object);
        }

        public int UpdateByHashtable(string tableName, string pkName, string pkVal, Hashtable ht)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(" Update ");
            sb.Append(tableName);
            sb.Append(" Set ");
            bool isFirstValue = true;
            foreach (string key in ht.Keys)
            {
                if (isFirstValue)
                {
                    isFirstValue = false;
                    sb.Append(key);
                    sb.Append("=");
                    sb.Append("@" + key);
                }
                else
                {
                    sb.Append("," + key);
                    sb.Append("=");
                    sb.Append("@" + key);
                }
            }
            sb.Append(" Where ").Append(pkName).Append("=").Append("@" + pkName);
            ht[pkName] = pkVal;
            SqlParam[] _params = this.GetParameter(ht);
            return this.ExecuteBySql(sb, _params);
        }

        public int DeleteData(string tableName, string pkName, string pkVal)
        {
            StringBuilder sb = new StringBuilder(string.Concat(new string[]
			{
				"Delete From ",
				tableName,
				" Where ",
				pkName,
				" = @ID"
			}));
            return this.ExecuteBySql(sb, new SqlParam[]
			{
				new SqlParam("@ID", pkVal)
			});
        }

        public int BatchDeleteData(string tableName, string pkName, object[] pkValues)
        {
            SqlParam[] param = new SqlParam[pkValues.Length];
            int index = 0;
            string str = "@ID" + index;
            StringBuilder sql = new StringBuilder(string.Concat(new string[]
			{
				"DELETE FROM ",
				tableName,
				" WHERE ",
				pkName,
				" IN ("
			}));
            for (int i = 0; i < param.Length - 1; i++)
            {
                object obj2 = pkValues[i];
                str = "@ID" + index;
                sql.Append(str).Append(",");
                param[index] = new SqlParam(str, obj2);
                index++;
            }
            str = "@ID" + index;
            sql.Append(str);
            param[index] = new SqlParam(str, pkValues[index]);
            sql.Append(")");
            return this.ExecuteBySql(sql, param);
        }

        public DataTable GetPageList(string sql, SqlParam[] param, string orderField, string orderType, int pageIndex, int pageSize, ref int count)
        {
            StringBuilder sb = new StringBuilder();
            DataTable result;
            try
            {
                int num = (pageIndex - 1) * pageSize;
                int num2 = pageIndex * pageSize;
                sb.Append("Select * From (Select ROW_NUMBER() Over (Order By " + orderField + " " + orderType);
                sb.Append(string.Concat(new object[]
				{
					") As rowNum, * From (",
					sql,
					") As T ) As N Where rowNum > ",
					num,
					" And rowNum <= ",
					num2
				}));
                count = Convert.ToInt32(this.GetObjectValue(new StringBuilder("Select Count(1) From (" + sql + ") As t"), param));
                result = this.GetDataTableBySQL(sb, param);
            }
            catch (Exception e)
            {
                this.Logger.WriteLog(string.Concat(new string[]
				{
					"-----------数据分页（Oracle）-----------\r\n",
					sb.ToString(),
					"\r\n",
					e.Message,
					"\r\n"
				}));
                result = null;
            }
            return result;
        }

        public bool SqlBulkCopyImport(DataTable dt)
        {
            IDbHelperExpand copy = new IDbHelperExpand();
            return copy.MsSqlBulkCopyData(dt, this.connectionString);
        }

        public void Dispose()
        {
            if (this.dbCommand != null)
            {
                this.dbCommand.Dispose();
            }
        }

        private static void PrepareCommand(SQLiteCommand cmd, SQLiteConnection conn, SQLiteTransaction trans, string cmdText, SqlParam[] cmdParms)
        {
            if (conn.State != ConnectionState.Open)
                conn.Open();
            cmd.Connection = conn;
            cmd.CommandText = cmdText;
            if (trans != null)
                cmd.Transaction = trans;
            cmd.CommandType = CommandType.Text;//cmdType;
            if (cmdParms != null)
            {
                foreach (SqlParam parm in cmdParms)
                {
                    SQLiteParameter temp = new SQLiteParameter(parm.FieldName,parm.FiledValue);
                    
                    cmd.Parameters.Add(temp);
                }
            }
        }
    }
}
