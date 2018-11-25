using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.Sqlite;


namespace xxoo.Common
{
    public class SqliteHelper
    {
        //<add connectionString="Data Source=cater.db;Version=3;" name="conStr"/>
        // 连接字符串  cater.db 相对路径 bin目录下

        //连接字符串  //readonly 只读字段
        private static readonly string connStr = "Data Source=E:/cc/test/QcfDataTest/SQLiteT4/bin/Debug/cater.db;"; //ConfigurationManager.ConnectionStrings["conStr"].ConnectionString;

        /// <summary>
        /// 增删改
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="slPars">参数</param>
        /// <returns>受影响行数</returns>
        public static int ExecuteNonQuery(string sql, params SqliteParameter[] slPars)
        {
            using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                {
                    if (slPars != null)
                    {
                        cmd.Parameters.AddRange(slPars);
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
        /// <param name="slPars">参数</param>
        /// <returns>首行首列</returns>
        public static object ExecuteScalar(string sql, params SqliteParameter[] slPars)
        {
            using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                {
                    if (slPars != null)
                    {
                        cmd.Parameters.AddRange(slPars);
                    }
                    conn.Open();
                    return cmd.ExecuteScalar();
                }
            }
        }

        ///// <summary>
        ///// 查询表
        ///// </summary>
        ///// <param name="sql">sql语句</param>
        ///// <param name="slPars">参数</param>
        ///// <returns>返回表</returns>
        //public static DataTable ExecuteTable(string sql, params SqliteParameter[] slPars)
        //{
        //    DataTable dt = new DataTable();
        //    using (SqliteDataAdapter sld = new SqliteDataAdapter(sql, connStr))
        //    {
        //        if (slPars != null)
        //        {
        //            sld.SelectCommand.Parameters.AddRange(slPars);
        //        }
        //        sld.Fill(dt);
        //    }
        //    return dt;
        //}

        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="slPars">参数</param>
        /// <returns>发挥SQLiteDataReader</returns>
        public static SqliteDataReader ExecuteReader(string sql, params SqliteParameter[] slPars)
        {
            SqliteConnection conn = new SqliteConnection(connStr);
            SqliteCommand cmd = new SqliteCommand(sql, conn);
            try
            {
                if (slPars != null)
                {
                    cmd.Parameters.AddRange(slPars);
                }
                conn.Open();
                return cmd.ExecuteReader(CommandBehavior.CloseConnection);
            }
            catch (Exception ex)
            {
                conn.Close();
                conn.Dispose();
                throw ex;
            }


        }
    }

}
