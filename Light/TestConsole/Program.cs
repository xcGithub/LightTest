using Dapper;
using Light.Common;
using Light.Model.TableModel;
using Light.Repository;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using xxoo.Common;

namespace TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            //Console.WriteLine("Hello World!");

            //List<UserInfo> list = Test();
            //Console.WriteLine( JsonHelper.SerializeObject(list));
            //Console.ReadKey();


            var count = SqliteHelper.ExecuteScalar(" select count(1) from UserInfo ");
            Console.WriteLine(count);

            IDataReader dr = SqliteHelper.ExecuteReader(" select * from UserInfo where UserId = 20");
            var user20 = DataToModelHelper.RefDataReaderToList<UserInfo>(dr)[0];
            user20.UserName = "xxoo";
            user20.Pwd = "12345";
            user20.LoginUserName = "dalaofeng";
            DapperUtil.Update<UserInfo>(user20);

            //UserInfo user = new UserInfo();
            //user.UserId = 20;
            //user.UserName = "xxoo";
            //user.Pwd = "12345";
            //user.LoginUserName = "dalaofeng";
            //DapperUtil.Insert<UserInfo>(user);

            //var rows = Testds();
            //foreach (dynamic item in rows)
            //{
            //    var id = item.UserId;
            //    var n = item.UserName;
            //    Console.WriteLine($"id={id} n={n}");
            //}

            //string path = Path.Combine(Directory.GetCurrentDirectory(), "config.json");
            //var provider = new JsonConfigurationProvider( new JsonConfigurationSource() { Path = path });
            //provider.Load();

            //string url = null;
            //provider.TryGet("url", out url);
            //Console.WriteLine($"url={url}");

            //string one = null;
            //provider.TryGet("port:one", out one);
            //Console.WriteLine($"port-one={one}");


            //string two = null;
            //provider.TryGet("port:two", out two);
            //Console.WriteLine($"port0two={two}");

            Console.ReadKey();

            //sqlTest();

            //Console.ReadKey();

        }

        public static IEnumerable<dynamic> Testds()
        {
            string querySql = @" select * from UserInfo ";


            using (IDbConnection conn = DataBaseConfig.GetSqliteConnection())
            {
                return conn.Query(querySql).ToList();
            }
        }

        public static List<UserInfo> Test()
        {
          
            string querySql = @" select * from UserInfo ";

            using (IDbConnection conn = DataBaseConfig.GetSqliteConnection())
            {
               return conn.Query<UserInfo>(querySql).ToList();
            }




        }


        public static void sqlTest()
        {
            string querySql = @" select * from UserInfo "; 

            IDataReader reader = SqliteHelper.ExecuteReader(querySql);

            List<UserInfo> list = DataToModelHelper.RefDataReaderToList<UserInfo>(reader);
            
        }
    }
}