using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.IO;


namespace HVAC_Checker
{
    class Program
    {
        static void Main(string[] args)
        {
            XdbAdapter.SqliteConnect();
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

    class HVACFunction
    {

        //获取指定类型、指定名称、大于一定面积的地上或地下房间ID集合
         List<string> GetRoomID(string type, string name, double area, bool isUp)
        {

         
            List<string> IDs = new List<string>();         
            return IDs;
        }

        // 判断房间是否有魔种构件并返回构件ID
        List<string> GetElementIDFromRoom()
        {
            List<string> IDs = new List<string>();         
            return IDs;
        }

        //找到与风口相连的风机的ID
        List<string> GetFanIDFromAirterminal(string airterminalID)
        {
            List<string> IDs = new List<string>();
            return IDs;
        }

        //找到风机的取风口或排风口ID
        List<string> GetAirterminalFromFan(string fanID)
        {
            List<string> IDs = new List<string>();
            return IDs;
        }

        //找到大于一定长度的走道ID

        //找到一个风系统的全部风口ID
        List<string> GetOutletIDsFromDuctSysterm(string ductSystermID)
        {
            List<string> IDs = new List<string>();
            return IDs;
        }

        //找到穿越某些空间的风管ID
        List<string> GetDuctIDFromSpace(string spaceID)
        {
            List<string> IDs = new List<string>();
            return IDs;
        }
        //找到穿越楼层的风管ID
        List<string> GetDuctIDFromFloor(string FloorID)
        {
            List<string> IDs = new List<string>();
            return IDs;
        }
        //获的房间的包含关系

        //根据ID获的窗对象
        Windows GetWindows(string WindowsID)
        {
            Windows window = new Windows();
            return window;
        }
        //根据ID获的房间对象
        Room GetRoom(string roomID)
        {
            Room window = new Room();
            return window;
        }
        //根据ID获的风机对象
        Fan GetFan(string FanID)
        {
            Fan fan = new Fan();
            return fan;
        }
        //根据ID获的风口对象
        AirTerminal GetAirTerminal(string AirTerminalID)
        {
            AirTerminal airTerminal = new AirTerminal();
            return airTerminal;
        }
        //生成审查结果
        //各条文审查子函数
    }

}
