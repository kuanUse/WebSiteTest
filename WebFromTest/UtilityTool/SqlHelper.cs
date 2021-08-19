//using MySql.Data.MySqlClient;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Reflection;

namespace UtilityTool
{
    public enum DbCategory { Oracle, MsSql, MySql }
    public class SqlHelper
    {
        public class SqlObj
        {
            public string StrSql { get; set; }
            public IDictionary<string, object> SqlParameters { get; set; }
            public SqlObj(string strSql)
            {
                this.StrSql = strSql;
            }
            public SqlObj(string strSql, IDictionary<string, object> sqlParameters)
            {
                this.StrSql = strSql;
                this.SqlParameters = sqlParameters;
            }
        }
        private static string GetConnectionStrings(string dbAlias)
        {
            return ConfigurationManager.ConnectionStrings[dbAlias].ConnectionString;
        }
        public static DataTable doSql(string strSQL, string dbAlias, DbCategory dbCategory, bool feeback = false)
        {
            var ListSql = new List<SqlObj>();
            ListSql.Add(new SqlObj(strSQL));
            return doSql(ListSql, dbAlias, dbCategory, feeback);
        }
        public static DataTable doSql(string strSQL, IDictionary<string, object> sqlParameters, string dbAlias, DbCategory dbCategory, bool feeback = false)
        {
            var ListSql = new List<SqlObj>();
            ListSql.Add(new SqlObj(strSQL, sqlParameters));
            return doSql(ListSql, dbAlias, dbCategory, feeback);
        }
        public static DataTable doSql(IEnumerable<string> sqlList, string dbAlias, DbCategory dbCategory, bool feeback = false)
        {
            var ListSql = new List<SqlObj>();
            foreach (string strSQL in sqlList)
            {
                ListSql.Add(new SqlObj(strSQL));
            }
            return doSql(ListSql, dbAlias, dbCategory, feeback);
        }
        public static DataTable doSql(IEnumerable<string> queryStrs, IEnumerable<Dictionary<string, object>> sqlParameters, string dbAlias, DbCategory dbCategory, bool feeback = false)
        {
            var ListSql = new List<SqlObj>();
            if (queryStrs.Count() != sqlParameters.Count()) throw new Exception("sql語句數量與參數數量不符合");
            foreach (var str in queryStrs)
            {
                foreach (var param in sqlParameters)
                {
                    ListSql.Add(new SqlObj(str, param));
                    break;
                }

            }
            return doSql(ListSql, dbAlias, dbCategory, feeback);
        }
        public static DataTable doSql(List<SqlObj> sqlList, string dbAlias, DbCategory dbCategory, bool feeback = false)
        {
            DataTable dt = new DataTable();
            string connStr = GetConnectionStrings(dbAlias);
            if (string.IsNullOrWhiteSpace(connStr))
            {
                throw new Exception("DBAlias is not exist in Web.config");
            }
            switch (dbCategory)
            {
                case DbCategory.Oracle:
                    dt = doOracle(sqlList, connStr, feeback);
                    break;
                case DbCategory.MsSql:
                    dt = doMsSql(sqlList, connStr, feeback);
                    break;
                case DbCategory.MySql:
                    break;
                default:
                    throw new Exception(dbCategory + " is not a define DB Type");
            }
            return dt;
        }
        private static DataTable doOracle(List<SqlObj> sqlList, string connStr, bool feeback)
        {
            DataTable dt = new DataTable();

            System.Environment.SetEnvironmentVariable("ORA_NCHAR_LITERAL_REPLACE", "TRUE");
            //將.net地區設定為美國，解決NLS LANG = AMERICAN_AMERICA.ZHT16MSWIN950設定問題。
            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("en-US");
            using (OracleConnection icn = new OracleConnection(connStr))
            {

                if (icn.State != ConnectionState.Open)
                    icn.Open();
                OracleGlobalization SessionGlob = icn.GetSessionInfo();
                OracleTransaction transaction = icn.BeginTransaction();
                try
                {
                    using (OracleCommand command = new OracleCommand())
                    {
                        using (OracleDataAdapter ODA = new OracleDataAdapter(command))
                        {
                            command.BindByName = true;
                            command.Transaction = transaction;
                            foreach (var sqlObj in sqlList)
                            {
                                command.Connection = icn;
                                command.CommandText = sqlObj.StrSql;
                                command.CommandTimeout = 600;
                                command.Parameters.Clear();
                                //傳入SQL查詢參數
                                if (sqlObj.SqlParameters != null && sqlObj.SqlParameters.Any())
                                {
                                    var queryParameter = sqlObj.SqlParameters;
                                    //使用RegularExpression，抓出 ":"到邊界中間的字元
                                    Regex r = new Regex(@":(?<parameter>\w+?)\b");
                                    //比對
                                    foreach (Match item in r.Matches(command.CommandText))
                                    {
                                        //取出比對成功的parameter群組的值
                                        var key = item.Groups["parameter"].Value;

                                        if (command.Parameters.Contains(key)) continue;
                                        if (command.CommandText.IndexOf("DECLARE :" + key, StringComparison.OrdinalIgnoreCase) > -1) continue;
                                        if (!queryParameter.ContainsKey(key))
                                        {
                                            throw new Exception("queryParameter not contain the key " + key);
                                        }
                                        //如果這個key存在於dic裡面

                                        if (queryParameter[key] == null || queryParameter[key] == DBNull.Value)
                                        {
                                            command.Parameters.Add(key, DBNull.Value);
                                            continue;
                                        }
                                        OracleParameter param;
                                        //加入到Parameters裡面
                                        if (queryParameter[key].GetType().Name == "String[]")
                                        {
                                            command.ArrayBindCount = ((string[])queryParameter[key]).Length;
                                        }
                                        Type elementType = GetValueType(queryParameter[key].GetType());
                                        TypeCode elementTypeCode = Type.GetTypeCode(elementType);
                                        switch (elementTypeCode)
                                        {
                                            case TypeCode.DateTime:
                                                param = new OracleParameter(key, OracleDbType.Date);
                                                param.Value = queryParameter[key] ?? DBNull.Value;
                                                command.Parameters.Add(param);
                                                break;
                                            default:
                                                //若為string，強制指定存入型態為NVarchar2
                                                param = new OracleParameter(key, OracleDbType.NVarchar2);
                                                param.Value = queryParameter[key] ?? DBNull.Value;
                                                command.Parameters.Add(param);
                                                break;
                                        }
                                    }
                                }

                                if (feeback)
                                {
                                    ODA.Fill(dt);
                                }
                                else
                                {
                                    command.ExecuteNonQuery();//執行sql不回傳
                                }
                            }
                            transaction.Commit();

                        }
                    }
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw ex;
                }
                finally
                {
                    if (icn.State == ConnectionState.Open)
                        icn.Close();
                }
            }
            return dt;
        }
        private static DataTable doMsSql(List<SqlObj> sqlList, string connStr, bool feeback)
        {
            DataTable dt = new DataTable();
            using (SqlConnection icn = new SqlConnection(connStr))
            {
                if (icn.State != ConnectionState.Open)
                    icn.Open();
                SqlTransaction transaction = icn.BeginTransaction();
                try
                {
                    using (SqlCommand command = new SqlCommand())
                    {
                        using (SqlDataAdapter ODA = new SqlDataAdapter(command))
                        {
                            command.Transaction = transaction;
                            foreach (var sqlObj in sqlList)
                            {
                                command.Connection = icn;
                                command.CommandText = sqlObj.StrSql;
                                command.CommandTimeout = 600;
                                command.Parameters.Clear();
                                //傳入SQL查詢參數
                                if (sqlObj.GetType().GetProperty("queryParameter") != null
                                        && sqlObj.GetType().GetProperty("queryParameter").GetValue(sqlObj, null) != null)
                                {
                                    var queryParameter = sqlObj.SqlParameters;
                                    //使用RegularExpression，抓出 ":"到邊界中間的字元
                                    Regex r = new Regex(@"@(?<parameter>\w+?)\b");
                                    //比對
                                    foreach (Match item in r.Matches(command.CommandText))
                                    {
                                        //取出比對成功的parameter群組的值
                                        var key = item.Groups["parameter"].Value;

                                        if (command.Parameters.Contains(key)) continue;
                                        if (command.CommandText.IndexOf("DECLARE @" + key, StringComparison.OrdinalIgnoreCase) > -1)
                                            continue;
                                        if (!queryParameter.ContainsKey(key))
                                        {
                                            throw new Exception("queryParameter not contain hte key " + key);
                                        }
                                        //如果這個key存在於dic裡面
                                        //加入到Parameters裡面
                                        if (queryParameter[key] == null)
                                        {
                                            command.Parameters.AddWithValue("@" + key, DBNull.Value);
                                            continue;
                                        }
                                        SqlParameter param;
                                        //加入到Parameters裡面
                                        Type elementType = GetValueType(queryParameter[key].GetType());
                                        TypeCode elementTypeCode = Type.GetTypeCode(elementType);
                                        switch (elementTypeCode)
                                        {
                                            case TypeCode.DateTime:
                                                param = new SqlParameter("@" + key, SqlDbType.DateTime);
                                                param.Value = queryParameter[key];
                                                command.Parameters.Add(param);
                                                break;

                                            //若為string，強制指定存入型態為NVarchar
                                            default:
                                                param = new SqlParameter("@" + key, SqlDbType.NVarChar);
                                                param.Value = queryParameter[key];
                                                command.Parameters.Add(param);
                                                break;
                                        }

                                    }
                                }
                                if (feeback)
                                {
                                    ODA.Fill(dt);
                                }
                                else
                                {
                                    command.ExecuteNonQuery();//執行sql不回傳
                                }
                            }
                            transaction.Commit();
                            if (icn.State == ConnectionState.Open)
                                icn.Close();
                        }
                    }
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw ex;
                }
                finally
                {
                    if (icn.State == ConnectionState.Open)
                        icn.Close();
                }
            }
            return dt;
        }
        /*
        private  DataTable doMySql(List<SqlObj> sqlList, string connStr, bool feeback)
        {
            DataTable dt = new DataTable();
            using (MySqlConnection icn = new MySqlConnection(connStr))
            {
                if (icn.State != ConnectionState.Open)
                    icn.Open();
                MySqlTransaction transaction = icn.BeginTransaction();
                try
                {
                    using (MySqlCommand command = new MySqlCommand())
                    {
                        using (MySqlDataAdapter ODA = new MySqlDataAdapter(command))
                        {
                            command.Transaction = transaction;
                            foreach (var sqlObj in sqlList)
                            {
                                command.Connection = icn;
                                command.CommandText = sqlObj.StrSql;
                                command.CommandTimeout = 600;
                                command.Parameters.Clear();
                                //傳入SQL查詢參數
                                if (sqlObj.GetType().GetProperty("queryParameter") != null
                                        && sqlObj.GetType().GetProperty("queryParameter").GetValue(sqlObj, null) != null)
                                {
                                    var queryParameter = sqlObj.SqlParameters;
                                    //使用RegularExpression，抓出 ":"到邊界中間的字元
                                    Regex r = new Regex(@"@(?<parameter>\w+?)\b");
                                    //比對
                                    foreach (Match item in r.Matches(command.CommandText))
                                    {
                                        //取出比對成功的parameter群組的值
                                        var key = item.Groups["parameter"].Value;
                                        //如果這個key存在於dic裡面
                                        if (queryParameter.ContainsKey(key))
                                        {
                                            //加入到Parameters裡面
                                            if (queryParameter[key] == null)
                                            {
                                                command.Parameters.AddWithValue("@" + key, DBNull.Value);
                                                continue;
                                            }
                                            MySqlParameter param;
                                            //加入到Parameters裡面
                                            Type elementType = GetValueType(queryParameter[key].GetType());
                                            TypeCode elementTypeCode = Type.GetTypeCode(elementType);
                                            switch (elementTypeCode)
                                            {
                                                case TypeCode.DateTime:
                                                    param = new MySqlParameter("@" + key, MySqlDbType.Timestamp);
                                                    break;
                                                case TypeCode.Byte:
                                                    param = new MySqlParameter("@" + key, MySqlDbType.LongBlob);
                                                    break;
                                                //若為string，強制指定存入型態為NVarchar
                                                default:
                                                    param = new MySqlParameter("@" + key, MySqlDbType.LongText);
                                                    break;
                                            }
                                            param.Value = queryParameter[key];
                                            command.Parameters.Add(param);
                                        }
                                        else if (command.CommandText.IndexOf("DECLARE @" + key, StringComparison.OrdinalIgnoreCase) > -1)
                                        {
                                            continue;
                                        }
                                        else
                                        {
                                            throw new Exception("queryParameter not contain hte key " + key);
                                        }
                                    }
                                }
                                if (feeback)
                                {
                                    ODA.Fill(dt);
                                }
                                else
                                {
                                    command.ExecuteNonQuery();//執行sql不回傳
                                }
                            }
                            transaction.Commit();
                        }
                    }
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw ex;
                }
                finally
                {
                    if (icn.State == ConnectionState.Open)
                        icn.Close();
                }
            }
            return dt;
        }*/
        public static Type GetValueType(Type type)
        {
            if (type == null || type == typeof(DBNull))
                return null;
            //若為數值型態，則回傳數值Type
            if (type.IsValueType)
                return type;

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                return type.GetGenericArguments()[0];

            var iface = (from i in type.GetInterfaces()
                         where i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                         select i).FirstOrDefault();

            if (iface == null)
                throw new ArgumentException("Does not represent an enumerable type.", "type");

            return GetValueType(iface);
        }
    }
    public static class DataTableExtensions
    {
        //轉換DataTable為List<T>
        public static IList<T> ToList<T>(this DataTable table) where T : new()
        {
            IList<PropertyInfo> properties = typeof(T).GetProperties().ToList();
            IList<T> result = new List<T>();

            //取得DataTable所有的row data
            foreach (DataRow row in table.Rows)
            {
                var item = MappingItem<T>(row, properties);
                result.Add(item);
            }

            return result;
        }
        //轉換DataRow為 T
        public static T ToObject<T>(this DataRow row) where T : new()
        {
            IList<PropertyInfo> properties = typeof(T).GetProperties().ToList();
            var result = MappingItem<T>(row, properties);
            return result;
        }
        private static T MappingItem<T>(DataRow row, IList<PropertyInfo> properties) where T : new()
        {
            T item = new T();
            foreach (var property in properties)
            {
                if (row.Table.Columns.Contains(property.Name))
                {
                    //針對欄位的型態去轉換
                    TypeCode elementTypeCode = Type.GetTypeCode(property.PropertyType);
                    switch (elementTypeCode)
                    {
                        case TypeCode.DateTime:
                            {
                                var dt = new DateTime();
                                if (DateTime.TryParse(row[property.Name].ToString(), out dt))
                                {
                                    property.SetValue(item, dt, null);
                                }
                                else
                                {
                                    property.SetValue(item, null, null);
                                }
                            }
                            break;
                        case TypeCode.Decimal:
                            var dec = new decimal();
                            decimal.TryParse(row[property.Name].ToString(), out dec);
                            property.SetValue(item, dec, null);
                            break;
                        case TypeCode.Double:
                            var dou = new double();
                            double.TryParse(row[property.Name].ToString(), out dou);
                            property.SetValue(item, dou, null);
                            break;
                        case TypeCode.Int16:
                        case TypeCode.Int32:
                            var i = new int();
                            int.TryParse(row[property.Name].ToString(), out i);
                            property.SetValue(item, i, null);
                            break;
                        case TypeCode.Int64:
                            var l = new long();
                            long.TryParse(row[property.Name].ToString(), out l);
                            property.SetValue(item, l, null);
                            break;
                        default:
                            if (row[property.Name] != DBNull.Value)
                            {
                                property.SetValue(item, row[property.Name], null);
                            }
                            break;
                    }
                }
            }
            return item;
        }
    }
}