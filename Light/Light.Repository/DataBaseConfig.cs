using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace Light.Repository
{
    /// <summary>
    /// 数据库配置
    /// </summary>
    public class DataBaseConfig
    {
        #region SqlServer链接配置
        /// <summary>
        /// 默认的Sql Server的链接字符串
        /// </summary>
        private static string DefaultSqlConnectionString = @"Data Source=.;Initial Catalog=Light;User ID=sa;Password=sa;";
        public static IDbConnection GetSqlConnection(string sqlConnectionString = null)
        {
            if (string.IsNullOrWhiteSpace(sqlConnectionString))
            {
                sqlConnectionString = DefaultSqlConnectionString;
            }
            IDbConnection conn = new SqlConnection(sqlConnectionString);
            conn.Open();
            return conn;
        }
        #endregion

        #region SqlLite链接配置


        private static string DefaultSqlLiteConnectionString = "DataSource=cater.db;";
        public static IDbConnection GetSqliteConnection(string sqliteConnectionString = null)
        {
            if (string.IsNullOrWhiteSpace(sqliteConnectionString))
            {
                sqliteConnectionString = DefaultSqlLiteConnectionString;
            }
            SqliteConnection conn = new SqliteConnection(sqliteConnectionString);
            conn.Open();
            return conn;
        }

        #endregion
    }
}
