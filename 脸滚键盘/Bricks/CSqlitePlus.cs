﻿using System.Data.SQLite;
using System.IO;

namespace NSMain.Bricks
{
    public class CSqlitePlus
    {

        public CSqlitePlus(string dbPath, string dbName = null)
        {
            Close();
            connection = CreateDatabaseConnection(dbPath, dbName);
            DbName = dbName;
            GlobalVal.Flag.IsSqlconnOpening = true;
        }

        ~CSqlitePlus()
        {
            GlobalVal.Flag.IsSqlconnOpening = false;
            SQLiteConnection.ClearAllPools();
        }

        // 使用全局静态变量保存连接
        private readonly SQLiteConnection connection;
        public string DbName;

        /// <summary>
        /// 数据库建立连接
        /// </summary>
        /// <param name="dbPath"></param>
        /// <param name="dbName"></param>
        /// <returns></returns>
        private SQLiteConnection CreateDatabaseConnection(string dbPath, string dbName = null)
        {
            // 数据库文件夹
            dbName = string.IsNullOrEmpty(dbName) ? "bookData.db" : dbName;
            string dbFilePath = Path.Combine(dbPath, dbName);
            return new SQLiteConnection("DataSource = " + dbFilePath + ";foreign keys=true;");
        }

        // 打开连接
        private void Open()
        {
            if (connection != null && connection.State != System.Data.ConnectionState.Open)
            {
                connection.Open();
                //System.Console.WriteLine(string.Format("打开数据库 {0} 的连接", this.DbName));
            }
        }

        // 关闭连接
        public void Close()
        {
            if (connection != null)
            {
                connection.Close();
                //System.Console.WriteLine(string.Format("关闭数据库 {0} 的连接", this.DbName));
            }
        }

        /// <summary>
        /// 执行非查询语句
        /// </summary>
        /// <param name="sql"></param>
        public void ExecuteNonQuery(string sql)
        {
            // 确保连接打开
            Open();

            using (SQLiteTransaction tr = connection.BeginTransaction())
            {
                using (SQLiteCommand command = connection.CreateCommand())
                {
                    command.CommandText = sql;
                    try
                    {
                        command.ExecuteNonQuery();
                    }
                    catch (System.Exception ex)
                    {
                        System.Console.WriteLine(ex.Message);
                    }

                }
                try
                {
                    tr.Commit();
                }
                catch (System.Exception ex)
                {
                    System.Console.WriteLine(ex.Message);
                }

            }
        }

        /// <summary>
        /// 执行查询语句
        /// </summary>
        /// <param name="sql"></param>
        public SQLiteDataReader ExecuteQuery(string sql)
        {
            // 确保连接打开
            Open();

            using (SQLiteTransaction tr = connection.BeginTransaction())
            {
                using (SQLiteCommand command = connection.CreateCommand())
                {

                    command.CommandText = sql;
                    try
                    {
                        // 执行查询，返回一个SQLiteDataReader对象
                        return command.ExecuteReader();
                    }
                    catch (System.Exception ex)
                    {
                        System.Console.WriteLine(ex.Message);
                        return null;
                    }
                }
            }
        }


        /// <summary>
        /// 压缩数据库
        /// </summary>
        /// <param name="cSqlite"></param>
        public void Vacuum()
        {
            SQLiteCommand cmd = new SQLiteCommand("VACUUM", connection);
            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (SQLiteException ex)
            {
                System.Console.WriteLine(ex.Message);
            }
        }


    }
}
