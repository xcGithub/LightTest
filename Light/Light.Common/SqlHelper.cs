using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace xxoo.Common
{
    public class SqlHelper
    {

        private static readonly string str = ""; // ConfigurationManager.ConnectionStrings["sql"].ConnectionString;
        /// <summary>
        /// 进行增删改操作
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="ps">可变参数</param>
        /// <returns>返回收影响的行数</returns>
        public static int ExecuteNonQuery(string sql , params SqlParameter[] ps)
        {
            using (SqlConnection conn = new SqlConnection(str))
            {
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    if (ps != null)
                    {
                        cmd.Parameters.AddRange(ps);
                    }
                    conn.Open();
                    return cmd.ExecuteNonQuery();
                }
            }
        }
        /// <summary>
        /// 执行查询
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="ps">可变参数</param>
        /// <returns>返回首行首列</returns>
        public static object ExecuteScalar(string sql, params SqlParameter[] ps)
        {
            using (SqlConnection conn = new SqlConnection(str))
            {
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    if (ps != null)
                    {
                        cmd.Parameters.AddRange(ps);
                    }
                    conn.Open();
                    return cmd.ExecuteScalar();
                }
            }
        }
        ///// <summary>
        ///// 查询表 .Net core下DataTable目前还没实现
        ///// </summary>
        ///// <param name="sql">sql语句</param>
        ///// <param name="ps">可变数组</param>
        ///// <returns>返回对象集合</returns>
        //public static DataTable ExecuteTable(string sql, params SqlParameter[] ps)
        //{
        //    DataTable dt = new DataTable();
        //    using (SqlDataAdapter sda = new SqlDataAdapter(sql, str))
        //    {

        //        if (ps != null)
        //        {
        //            sda.SelectCommand.Parameters.AddRange(ps);

        //        }
        //        sda.Fill(dt);
        //    }
        //    return dt;
        //}


        /// <summary>
        /// 查询返回的SqlDataReader
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="ps">可变参数</param>
        /// <returns></returns>
        public static SqlDataReader ExecuteReader(string sql, params SqlParameter[] ps)
        {

            SqlConnection conn = new SqlConnection();
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                if (ps != null)
                {

                    cmd.Parameters.AddRange(ps);
                }
                try
                {
                    conn.Open();
                    return cmd.ExecuteReader(CommandBehavior.CloseConnection);
                }
                catch (Exception ex)
                {
                    conn.Dispose();
                    conn.Close();
                    throw ex;
                }

            }
        }
		
		  /// <summary>
        /// 执行多条SQL语句，实现数据库事务。
        /// </summary>
        /// <param name="SQLStringList">多条SQL语句</param>		
        public static int ExecuteSqlTran(List<String> SQLStringList)
        {
            using (SqlConnection conn = new SqlConnection(str))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                SqlTransaction tx = conn.BeginTransaction();
                cmd.Transaction = tx;
                int count = 0;
                try
                {

                    for (int n = 0; n < SQLStringList.Count; n++)
                    {
                        string strsql = SQLStringList[n];
                        if (strsql.Trim().Length > 1)
                        {
                            cmd.CommandText = strsql;
                            count += cmd.ExecuteNonQuery();
                        }
                    }
                    tx.Commit();
                    conn.Close();
                    return count;
                }
                catch (System.Data.SqlClient.SqlException e)
                {
                    tx.Rollback();
                    conn.Close();
                    throw e;
                }
            }
        }
    }
}
