using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.IO;


namespace HVAC_CheckEngine
{
    class Program
    {
        static void Main(string[] args)
        {
            string type = "房间";
            string name = "CH/PH Stock 1";
            double area = 0.0;
            RoomPosition roomPosition = RoomPosition.overground;
            HVACFunction.GetRooms(type, name, area, roomPosition);
            Console.ReadLine();
        }
    }


    class XdbAdapter
    {
        public
       static void SqliteConnect()
        {
         
            dynamic type = (new Program()).GetType();
            
            string currentDirectory = Path.GetDirectoryName(type.Assembly.Location);
            int iSub = currentDirectory.IndexOf("\\bin");
            currentDirectory = currentDirectory.Substring(0, iSub);


            string path = new DirectoryInfo(currentDirectory + "/机电.GDB").FullName;
            
            //如果不存在，则创建一个空的数据库,
            if (!System.IO.File.Exists(path))
                SQLiteConnection.CreateFile("MyDatabase.sqlite");

            //创建一个连接
            string connectionstr = @"data source =" + path;
            SQLiteConnection m_dbConnection = new SQLiteConnection(connectionstr);
           // SQLiteConnection m_dbConnection = new SQLiteConnection("Data Source=机电.GDB;Version=3;");
            m_dbConnection.Open();

            ////创建一个数据表
            //string sql = "create table book (title varchar(20), author varchar(10), pages int)";
            //SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            //try
            //{
            //    command.ExecuteNonQuery();
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine(ex);
            //}

            ////插入一些记录
            //sql = "insert into book (title, author, pages) values ('C#程序设计教程', '唐大仕', 570)";
            //command = new SQLiteCommand(sql, m_dbConnection);
            //command.ExecuteNonQuery();

            //sql = "insert into book (title, author, pages) values ('Java程序设计', '唐大仕', 390)";
            //command = new SQLiteCommand(sql, m_dbConnection);
            //command.ExecuteNonQuery();

            //sql = "insert into book (title, author, pages) values ('Visual C++.NET程序设计', '唐大仕，刘光', 410)";
            //command = new SQLiteCommand(sql, m_dbConnection);
            //command.ExecuteNonQuery();

            ////使用sql查询语句，并显示结果
            //sql = "select * from book order by pages desc";
            //command = new SQLiteCommand(sql, m_dbConnection);
            //SQLiteDataReader reader = command.ExecuteReader();
            //while (reader.Read())
            //    Console.WriteLine("书名: " + reader["title"] + "\t页数: " + reader["pages"]);


            //使用sql查询语句，并显示结果
            string sql = "select * from AirTerminals";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())          
                Console.WriteLine("书名: " + reader["AirTerminalType"]  );

            //关闭连接
            m_dbConnection.Close();

            Console.ReadLine();

        }

    }

  

}
