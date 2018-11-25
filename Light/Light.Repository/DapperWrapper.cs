using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using Wendu.FrameWork.Core.Cache;
using Wendu.FrameWork.Core.Log;
using Wendu.FrameWork.Core.Dapper;
using Wendu.FrameWork.Core.Common;

namespace SunCard.Common.Dapper
{
    public partial class DapperUtil
    {
        /// <summary>
        /// 插入记录--返回的是数据库自动生成的主键。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="isDbGenerated"> </param>
        /// <returns></returns>
        public static int Insert<T>(T entity, bool isDbGenerated = false)
        {
            Type type = typeof(T);
            if (entity == null)
                throw new Exception();

            using (var conn = new ConnnectionManager().GetOpenConnection(DbConnectionHelper.GetConnectionString(type)))
            {
                var t = conn.Insert(entity, null, default(int?), isDbGenerated);
                return (int)t;
            }
        }

        /// <summary>
        /// 更新整个实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static bool Update<T>(T entity)
        {
            Type type = typeof(T);
            if (entity == null)
                throw new Exception();
            using (var conn = new ConnnectionManager().GetOpenConnection(DbConnectionHelper.GetConnectionString(type)))
            {
                var t = conn.Update(entity);
                return t;
            }
        }

        /// <summary>
        /// 更新实体的部分字段 必须包含主键！
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="anonymousObject"></param>
        /// <returns></returns>
        public static bool UpdatePartialColumns<T>(object anonymousObject)
        {
            Type type = typeof(T);
            using (var conn = new ConnnectionManager().GetOpenConnection(DbConnectionHelper.GetConnectionString(type)))
            {
                var t = conn.Update(typeof(T), anonymousObject);
                return t;
            }
        }

        /// <summary>
        /// 删除实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static bool Delete<T>(T entity) where T:class
        {
            Type type = typeof(T);
            if (entity == null)
                throw new Exception();
            using (var conn = new ConnnectionManager().GetOpenConnection(DbConnectionHelper.GetConnectionString(type)))
            {
                var t = conn.Delete(entity, null, default(int?));
                return t;
            }
        }

        /// <summary>
        /// 根据主键获取实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pk"></param>
        /// <returns></returns>
        public static T QueryByIdentity<T>(object pk) where T : class
        {
            if (pk == null)
                return null;
            Type type = typeof(T);
            string templateKey = string.Format("{0}_{1}_FetchEntityByIdentity", DbConnectionHelper.GetDatabaseName(type), type.Name);
            return QueryByIdentityInternal<T>(templateKey, pk);
        }

        /// <summary>
        /// 根据匿名条件查询对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="anonymousParm"></param>
        /// <returns></returns>
        public static T QueryByParm<T>(object anonymousParm) where T : class
        {
            if (anonymousParm == null)
                return default(T);
            return QueryByQueryByParm<T>(anonymousParm);
        }

        /// <summary>
        /// 通过自定义SQL 查询自定义类型集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="statementId">sql模板ID</param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public static IEnumerable<T> Query<T>(string statementId, object parameter = null)
        {
            var st = SqlStatementManager.ReadStatement(statementId);
            if (!st.IsCached)
                return ExecQuerySql<T>(st, parameter);

            var key = GenerateCacheKey(statementId, parameter);
            var result = RedisCache.Instance.Get<IEnumerable<T>>(key);
            if (result != null)
            {
                return result;
            }
            result = ExecQuerySql<T>(st, parameter).ToList();

            if (result != null)
            {
                RedisCache.Instance.Set<IEnumerable<T>>(key, result, st.Expired);
            }

            return result;

        }

        /// <summary>
        /// 通过自定义SQL 查询
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="statementId"></param>
        /// <param name="dictionary"> 模板引擎使用的变量</param>
        /// <param name="parameter">SQL中的参数</param>
        /// <returns></returns>
        public static IEnumerable<T> Query<T>(string statementId, IDictionary<string, object> dictionary, object parameter = null)
        {
            //IEnumerable redis cache read error????
            var st = SqlStatementManager.ReadStatement(statementId, dictionary);
            if (!st.IsCached)
            {
                return ExecQuerySql<T>(st, parameter);
            }
            string key = GenerateCacheKey(statementId, parameter, DictionaryToStringKey(dictionary));
            var result = RedisCache.Instance.Get<IEnumerable<T>>(key);
            if (result != null)
            {
                return result;
            }
            result = ExecQuerySql<T>(st, parameter).ToList();
            if (result != null)
            {
                RedisCache.Instance.Set<IEnumerable<T>>(key, result, st.Expired);
            }
            return result;
        }

        /// <summary>
        /// 执行自定义SQL 用于 insert update delete
        /// </summary>
        /// <param name="statementId"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public static int Execute(string statementId, IDictionary<string, object> dictionary, object parameter = null)
        {
            var st = SqlStatementManager.ReadStatement(statementId, dictionary);
            return InternalRawExecute(st, parameter);
        }

        /// <summary>
        /// 执行自定义SQL 用于 insert update delete
        /// </summary>
        /// <param name="statementId"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public static int Execute(string statementId, object parameter = null)
        {
            var st = SqlStatementManager.ReadStatement(statementId);
            return InternalRawExecute(st, parameter);
        }

        /// <summary>
        /// 分页查询,这个T 需继承自 PagingEntityBase
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="statementId"> sql id </param>
        /// <param name="pageNum">当前页码</param>
        /// <param name="pageSize">页Size</param>
        /// <param name="orderBy">排序字段</param>
        /// <param name="parameter"> 查询参数 </param>
        /// <returns></returns>
        public static IEnumerable<T> QueryPageing<T>(string statementId, int pageNum, int pageSize, string orderBy, IDictionary<string, object> dictionary, object parameter = null) where T : class
        {
            var st = SqlStatementManager.ReadStatement(statementId, dictionary);
            var st1 = PagingStatement.GeneratePaging(pageNum, pageSize, orderBy, st);
            if (!st.IsCached)
            {
                return ExecQuerySql<T>(st1, parameter);
            }
            string key = GenerateCacheKey(statementId, parameter, DictionaryToStringKey(dictionary) + string.Format("{0}:{1}:{2}:", pageNum, pageSize, orderBy));
            var result = RedisCache.Instance.Get<IEnumerable<T>>(key);
            if (result != null)
            {
                return result;
            }
            result = ExecQuerySql<T>(st1, parameter).ToList();
            if (result != null)
            {
                RedisCache.Instance.Set<IEnumerable<T>>(key, result, st.Expired);
            }
            return result;
        }

        public static IEnumerable<T> QueryPageing<T>(string statementId, int pageNum, int pageSize, string orderBy, object parameter = null)
        {
            var st = SqlStatementManager.ReadStatement(statementId);
            var st1 = PagingStatement.GeneratePaging(pageNum, pageSize, orderBy, st);
            if (!st.IsCached)
            {
                return ExecQuerySql<T>(st1, parameter);
            }

            string key = GenerateCacheKey(statementId, parameter, string.Format(":{0}:{1}:{2}:", pageNum, pageSize, orderBy));
            var result = RedisCache.Instance.Get<IEnumerable<T>>(key);
            if (result != null)
            {
                return result;
            }
            result = ExecQuerySql<T>(st1, parameter).ToList();
            if (result != null)
            {
                RedisCache.Instance.Set<IEnumerable<T>>(key, result, st.Expired);
            }
            return result;
        }

        /// <summary>
        /// 调用存储过程分页查询
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="statementId"></param>
        /// <param name="pageNum"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalRecord"></param>
        /// <returns></returns>
        public static IEnumerable<T> QueryPageExecProcedure<T>(string statementId, int pageNum, int pageSize, out int totalRecord, string where = "") where T : class,new()
        {
            var st = SqlStatementManager.ReadStatement(statementId);
            var t = typeof(T);
            var oblist = new List<T>();
            using (var conn = new ConnnectionManager().GetOpenConnection(DbConnectionHelper.GetConnectionString(statementId)))
            {
                totalRecord = 0;
                SqlParameter[] param = new SqlParameter[]
                {
                    new SqlParameter("@Tables", st.SourceName),
                    new SqlParameter("@PK", st.PrimaryKey),
                    new SqlParameter("@Sort", st.Order),
                    new SqlParameter("@PageNumber", pageNum),
                    new SqlParameter("@PageSize", pageSize),
                    //new SqlParameter("@Fields", Utils.ReflectFilderCol<T>(t)),//此方法法遇到到多表相同的字段不能正确区分，适合查询单独表分页
                    new SqlParameter("@Fields", st.StatementText),//替换成这种写法
                    new SqlParameter("@Filter",where),
                    new SqlParameter("@Group",st.Group),
                    new SqlParameter("@TotalCount",0),
                    new SqlParameter("@TotalPageCount",0),
                    new SqlParameter("@RecorderCount",totalRecord),
                };
                param[8].Direction = ParameterDirection.Output;
                param[9].Direction = ParameterDirection.Output;
                SqlCommand cmd = new SqlCommand("Paging_SubQuery", (SqlConnection)conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddRange(param);
                var dataSet = new DataSet();
                var da = new SqlDataAdapter(cmd);
                da.Fill(dataSet);
                totalRecord = Convert.ToInt32(cmd.Parameters["@TotalCount"].Value);
                if (dataSet != null)
                {
                    var dt = dataSet.Tables[0];
                    var prlist = new List<PropertyInfo>();
                    Array.ForEach(t.GetProperties(), p => { if (dt.Columns.IndexOf(p.Name) != -1) prlist.Add(p); });
                    foreach (DataRow row in dt.Rows)
                    {
                        var ob = new T();
                        prlist.ForEach(p => { if (row[p.Name] != DBNull.Value) p.SetValue(ob, row[p.Name], null); });
                        oblist.Add(ob);
                    }
                }
            }
            return oblist;
        }

        public static IEnumerable<T> QueryPageExecProcedure<T>(string statementId, int pageNum, int pageSize, out int totalRecord, string where = "", string OrderBy = "") where T : class,new()
        {
            var st = SqlStatementManager.ReadStatement(statementId);
            var t = typeof(T);
            var oblist = new List<T>();
            using (var conn = new ConnnectionManager().GetOpenConnection(DbConnectionHelper.GetConnectionString(statementId)))
            {
                totalRecord = 0;
                SqlParameter[] param = new SqlParameter[]
                {
                    new SqlParameter("@Tables", st.SourceName),
                    new SqlParameter("@PK", st.PrimaryKey),
                    new SqlParameter("@Sort", OrderBy),
                    new SqlParameter("@PageNumber", pageNum),
                    new SqlParameter("@PageSize", pageSize),
                    //new SqlParameter("@Fields", Utils.ReflectFilderCol<T>(t)),//此方法法遇到到多表相同的字段不能正确区分，适合查询单独表分页
                    new SqlParameter("@Fields", st.StatementText),//替换成这种写法
                    new SqlParameter("@Filter",where),
                    new SqlParameter("@Group",st.Group),
                    new SqlParameter("@TotalCount",0),
                    new SqlParameter("@TotalPageCount",0),
                    new SqlParameter("@RecorderCount",totalRecord),
                };
                param[8].Direction = ParameterDirection.Output;
                param[9].Direction = ParameterDirection.Output;
                SqlCommand cmd = new SqlCommand("Paging_SubQuery", (SqlConnection)conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddRange(param);
                var dataSet = new DataSet();
                var da = new SqlDataAdapter(cmd);
                da.Fill(dataSet);
                totalRecord = Convert.ToInt32(cmd.Parameters["@TotalCount"].Value);
                if (dataSet != null)
                {
                    var dt = dataSet.Tables[0];
                    var prlist = new List<PropertyInfo>();
                    Array.ForEach(t.GetProperties(), p => { if (dt.Columns.IndexOf(p.Name) != -1) prlist.Add(p); });
                    foreach (DataRow row in dt.Rows)
                    {
                        var ob = new T();
                        prlist.ForEach(p => { if (row[p.Name] != DBNull.Value) p.SetValue(ob, row[p.Name], null); });
                        oblist.Add(ob);
                    }
                }
            }
            return oblist;
        }

        public static IEnumerable<T> QueryPageExecProcedure<T>(string statementId, int pageNum, int pageSize, Object parm, out int totalRecord) where T : class,new()
        {
            var st = SqlStatementManager.ReadStatement(statementId);
            var t = typeof(T);
            var oblist = new List<T>();
            using (var conn = new ConnnectionManager().GetOpenConnection(DbConnectionHelper.GetConnectionString(statementId)))
            {
                totalRecord = 0;
                SqlParameter[] param = new SqlParameter[]
                {
                    new SqlParameter("@Tables", st.SourceName),
                    new SqlParameter("@PK", st.PrimaryKey),
                    new SqlParameter("@Sort", st.Order),
                    new SqlParameter("@PageNumber", pageNum),
                    new SqlParameter("@PageSize", pageSize),
                    //new SqlParameter("@Fields", Utils.ReflectFilderCol<T>(t)),//此方法法遇到到多表相同的字段不能正确区分，适合查询单独表分页
                    new SqlParameter("@Fields", st.StatementText),//替换成这种写法
                    new SqlParameter("@Filter", st.Where),
                    new SqlParameter("@Group",st.Group),
                    new SqlParameter("@TotalCount",0),
                    new SqlParameter("@TotalPageCount",0),
                    new SqlParameter("@RecorderCount",totalRecord)
                };
                param[8].Direction = ParameterDirection.Output;
                param[9].Direction = ParameterDirection.Output;
                SqlCommand cmd = new SqlCommand("Paging_SubQuery", (SqlConnection)conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddRange(param);
                var dataSet = new DataSet();
                var da = new SqlDataAdapter(cmd);
                da.Fill(dataSet);
                totalRecord = Convert.ToInt32(cmd.Parameters["@TotalCount"].Value);
                if (dataSet != null)
                {
                    var dt = dataSet.Tables[0];
                    var prlist = new List<PropertyInfo>();
                    Array.ForEach(t.GetProperties(), p => { if (dt.Columns.IndexOf(p.Name) != -1) prlist.Add(p); });
                    foreach (DataRow row in dt.Rows)
                    {
                        var ob = new T();
                        prlist.ForEach(p => { if (row[p.Name] != DBNull.Value) p.SetValue(ob, row[p.Name], null); });
                        oblist.Add(ob);
                    }
                }
            }
            return oblist;
        }

        public static T ExecuteProcedureModel<T>(string statementId, object anonymousObject) where T : class,new()
        {

            return ExecuteProcedureIEnumerable<T>(statementId, anonymousObject).FirstOrDefault();
        }

        public static IEnumerable<T> ExecuteProcedureIEnumerable<T>(string statementId, object anonymousObject) where T : class,new()
        {
            var st = SqlStatementManager.ReadStatement(statementId);
            using (var conn = new ConnnectionManager().GetOpenConnection(DbConnectionHelper.GetProcdureConnStr(statementId)))
            {
                var parems = new DynamicParameters();
                if (anonymousObject != null)
                {
                    Type type = anonymousObject.GetType();
                    PropertyInfo[] pArray = type.GetProperties();
                    foreach (var item in pArray)
                    {
                        parems.Add("@" + item.Name, item.GetValue(anonymousObject, null));
                    }
                }
                //SqlMapper.Query<T>(conn, st.StatementText.Trim(), parems, null, null, CommandType.StoredProcedure);
                return SqlMapper.Query<T>(conn, st.StatementText.Trim(), anonymousObject, null, true, null, CommandType.StoredProcedure);
            }
        }

        /// <summary>
        /// 执行存储过程
        /// </summary>
        /// <param name="statementId"></param>
        /// <param name="anonymousObject">new{这里的属性与存储过程中的变量名要一致}</param>
        /// <returns></returns>
        public static void ExecuteProcedure(string statementId, object anonymousObject = null)
        {
            var st = SqlStatementManager.ReadStatement(statementId);
            using (var conn = new ConnnectionManager().GetOpenConnection(DbConnectionHelper.GetProcdureConnStr(statementId)))
            {
                SqlMapper.Execute(conn, st.StatementText.Trim(), anonymousObject, null, null, CommandType.StoredProcedure);
            }
        }

        /// <summary>
        /// 执行存储过程并返回存储过程返回值
        /// </summary>
        /// <param name="statementId">模板ID</param>
        /// <param name="returnValue">与数据库中存储过程返回值变量相同</param>
        /// <param name="anonymousObject">new{}</param>
        /// <returns></returns>
        public static string ExecuteProcedure(string statementId, string returnValue, object anonymousObject = null)
        {
            if (string.IsNullOrWhiteSpace(returnValue))
            {
                ExecuteProcedure(statementId, anonymousObject);
                return string.Empty;
            }
            var st = SqlStatementManager.ReadStatement(statementId);
            using (var conn = new ConnnectionManager().GetOpenConnection(DbConnectionHelper.GetProcdureConnStr(statementId)))
            {
                var parems = new DynamicParameters();
                if (anonymousObject != null)
                {
                    Type type = anonymousObject.GetType();
                    PropertyInfo[] pArray = type.GetProperties();
                    foreach (var item in pArray)
                    {
                        parems.Add("@" + item.Name, item.GetValue(anonymousObject, null));
                    }
                    parems.Add(returnValue, "", DbType.String, ParameterDirection.Output);
                }
                SqlMapper.Execute(conn, st.StatementText.Trim(), parems, null, null, CommandType.StoredProcedure);
                return parems.Get<string>(returnValue);
            }
        }

        /***************************************************************************************************************************/

        private static IEnumerable<T> ExecQuerySql<T>(SqlStatement statement, object parameter)
        {
            IEnumerable<T> result;
            long count = 0;
            while (true)
            {
                if (count > 10)
                {
                    NLogUtil.Error(string.Format("ExecQuerySql error when executing: 模板ID:{0}--参数{1}", statement, Utils.ToHashString(Utils.ReflectAnonymousTypeParameters(parameter))), NLog.LogManager.GetCurrentClassLogger());
                }
                string connetionString = DbConnectionHelper.GetConnectionString(statement.StatementId);
                try
                {
                    using (var conn = new ConnnectionManager().GetOpenConnection(connetionString))
                    {
                        result = conn.Query<T>(statement.StatementText, parameter);
                    }
                    count++;
                    break;
                }
                catch (Exception e)
                {
                    NLogUtil.Error(string.Format("ExecQuerySql error when executing: {0}\r\nUsing connStr: {1}", statement.StatementText, connetionString), NLog.LogManager.GetCurrentClassLogger());
                    count++;
                    throw (e);
                }
            }
            return result;
        }

        private static IEnumerable<dynamic> ExecQuerySqlDynamic(SqlStatement statement, object parameter)
        {

            IEnumerable<dynamic> result;
            using (var conn = new ConnnectionManager().GetOpenConnection(DbConnectionHelper.GetConnectionString(statement.StatementId)))
            {
                result = conn.Query(statement.StatementText, parameter);
            }
            return result;
        }

        private static int InternalRawExecute(SqlStatement statement, object parameter)
        {
            int result;
            using (
                var conn =
                    new ConnnectionManager().GetOpenConnection(
                        DbConnectionHelper.GetConnectionString(statement.StatementId)))
            {
                result = conn.Execute(statement.StatementText, parameter);

            }

            return result;
        }
        private static string GenerateCacheKey(string statementId, object parameter = null, string others = "")
        {
            string key = string.Format("{0}:{1}", statementId, (parameter == null ? "" : Utils.ToHashString(others + Utils.ReflectAnonymousTypeParameters(parameter))));
            return key;
        }
        private static string DictionaryToStringKey(IEnumerable<KeyValuePair<string, object>> dictionary)
        {
            if (dictionary == null)
                return string.Empty;

            var sb = new StringBuilder("dic:");
            foreach (var o in dictionary)
            {
                sb.Append(string.Format("{0}:{1}", o.Key, o.Value ?? ""));
            }

            return sb.ToString();
        }

        private static T QueryByIdentityInternal<T>(string templateKey, object pk) where T : class
        {

            if (pk == null)
                return null;

            var st = SqlStatementManager.ReadStatement(templateKey);

            string key = st.PrimaryKey;
            if (key.Equals("none"))
                throw new ArgumentNullException();

            var dynParms = new DynamicParameters();
            dynParms.Add("@" + key, pk);
            return Query<T>(templateKey, dynParms).FirstOrDefault();

        }

        internal class PagingStatement
        {
            private const string Template = "declare @total int;"
                                            + " set @total=(select COUNT(1) from {4});"
                                            + " WITH PagingSet AS "
                                            + " ( "
                                            + "     SELECT  ROW_NUMBER() OVER(ORDER BY {0}) AS RowNum, {1}"
                                            + "  ) "
                                            + "  SELECT  @total as TotalCount,*  "
                                            + "   FROM PagingSet "
                                            + "  WHERE RowNum BETWEEN {2}  "
                                            + "                   AND {3} ;";

            internal static SqlStatement GeneratePaging(int pageNum, int pageSize, string orderBy, SqlStatement statement)
            {
                if (statement == null || string.IsNullOrEmpty(statement.StatementText))
                    throw new ArgumentNullException();


                var result = statement.Clone() as SqlStatement;

                if (string.IsNullOrEmpty(orderBy))
                    throw new ArgumentException("need order by column!");


                int rowStart = (pageNum - 1) * pageSize + 1;
                int rowEnd = pageNum * pageSize;

                if (result != null)
                {
                    string sqlselect = ComputerSql(result.StatementText, "select");
                    string sqlfrom = ComputerSql(result.StatementText, "from");
                    string output = string.Format(Template, orderBy, sqlselect, rowStart, rowEnd, sqlfrom);

                    result.StatementText = output;
                }
                return result;
            }
            internal static string ComputerSql(string sql, string parn)
            {
                var index = sql.IndexOf(parn, StringComparison.OrdinalIgnoreCase);
                if (index < 0)
                    throw new ArgumentNullException();
                return sql.Substring(index + parn.Length);
            }
        }

        private static T QueryByQueryByParm<T>(object anonymousParm) where T : class
        {
            T result;
            Type type = typeof(T);
            string connetionString = DbConnectionHelper.GetConnectionString(type);
            string sql = string.Format("select * from {0} WITH(NOLOCK) where {1}", DbConnectionHelper.GetDataTableName(type), ReflectQueryParameters(anonymousParm));
            using (var conn = new ConnnectionManager().GetOpenConnection(connetionString))
            {
                result = conn.Query<T>(sql, anonymousParm).FirstOrDefault();
            }
            return result;
        }

        internal static string ReflectQueryParameters(object parameter)
        {
            if (parameter == null)
                return "";
            StringBuilder str = new StringBuilder();
            Type type = parameter.GetType();
            PropertyInfo[] pInfo = type.GetProperties();
            for (int i = 0; i < pInfo.Length; i++)
            {
                str.Append(pInfo[i].Name + "=@" + pInfo[i].Name);
                if (i < pInfo.Length - 1)
                    str.Append(" and ");
            }
            return str.ToString().Trim();
        }
    }
}