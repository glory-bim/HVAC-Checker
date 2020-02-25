#define DEBUG
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.IO;

#if DEBUG
namespace HVAC_CheckEngine
{


   public class HVACFunction
    {
        private
            string m_archXdb;
        string m_hvacXdb;
        static string m_TestPath;

        public HVACFunction(string Archxdb, string HVACxdb)
        {
            m_archXdb = Archxdb;
            m_hvacXdb = HVACxdb;
        }

      

        //1获取指定类型、指定名称、大于一定面积的地上或地下房间对象集合
        public static List<Room> GetRooms(string type, string name, double area, RoomPosition roomPosition)
        {
            List<Room> rooms = new List<Room>();
            return rooms;
        }

        //2 判断房间是否有某种构件并返回构件对象
        //  Room    某种构件  风口
        public static List<AirTerminal> GetRoomContainAirTerminal(Room room)
        {
            List<AirTerminal> airterminals = new List<AirTerminal>();
            return airterminals;
        }

        public static List<Windows> GetWindowsInRoom(Room room)
        {
            List<Windows> windows = new List<Windows>();
            return windows;
        }

        //3找到与风口相连的风机对象//一条路径 三通、四通只记录了一个id
        public static List<Fan> GetFanConnectingAirterminal(AirTerminal airterminal)
        {
            List<Fan> Fans = new List<Fan>();
            return Fans;
        }

        //4找到风机的排风口对象//怎么判断风口前端后端
        static List<AirTerminal> GetOutLetsOfFan(Fan fan)
        {
            List<AirTerminal> airTerminals = new List<AirTerminal>();
            return airTerminals;
        }

        //5找到大于一定长度的走道对象  double  “走道、走廊”    长度清华引擎 计算学院  张荷花


        public static List<Room> GetRoomsMoreThan(double dLength)
        {
            List<Room> rooms = new List<Room>();

            return rooms;
        }


        //6找到一个风机的进风口对象集合
        public static List<AirTerminal> GetInletOfFan(Fan fan)
        {
            List<AirTerminal> inlets = new List<AirTerminal>();
           
            //关闭连接
            dbConnection.Close();



            return inlets;
        }
       

        //7找到穿越某些房间的风管对象集合  清华引擎 构件相交  包含
        public static List<Duct> GetDuctsCrossSpace(Room room)
        {
            List<Duct> ducts = new List<Duct>();

            return ducts;
        }
        //8找到穿越防火分区的风管对象集合  userlable
        public static List<Duct> GetDuctsCrossFireDistrict(FireDistrict fireDistrict)
        {
            List<Duct> ducts = new List<Duct>();
            return ducts;
        }
        //9找到穿越防火分隔处的变形缝两侧的风管集合  差变形缝对象

        public static List<Duct> GetDuctsCrossFireSide()
        {
            List<Duct> ducts = new List<Duct>();

            return ducts;
        }


        //10获得构建所在房间的对象  几何 包含 遍历表都查

        public static Room GetRoomOfAirterminal(AirTerminal airTerminal)
        {
            Room room = new Room(lid);
            //关闭连接               
            return room;
        }

        //11获得一个窗户对象的有效面积  差公式 xdb差参数  开启角度  开启方式
        public static double GetArea(Windows window)
        {
            double dArea = 0.0;
            return dArea;
        }

        //12找到属于某种类型的房间对象集合
        public static List<Room> GetRooms(string roomType)
        {
            List<Room> rooms = new List<Room>();
            return rooms;

        }

        // 14判断房间属于地上房间或地下房间  //差参数
        public static bool isOvergroundRoom(Room room)
        {

            bool isUp = true;

            return isUp;

        }
        //15获取所有楼层对象集合
        public static List<Floor> GetFloors()
        {
            List<Floor> floors = new List<Floor>();
            
            return floors;
        }

       
        //16获得风机所连接的所有风管集合  支路 干管  风口到末端 
        public static List<Duct> GetDuctsOfFan(Fan fan)
        {
            List<Duct> ducts = new List<Duct>();
           
            return ducts;
        }
        //17判断是否风机所连风系统所有支路都连接了风口  //管堵
        public static bool isAllBranchLinkingAirTerminal(Fan fan)
        {

            bool isLink = true;

            return isLink;

        }
        //获得防烟分区长边长度

        public static double GetFireDistrictLength(FireDistrict fan)
        {
            double dLength = 0.0;
            return dLength;
        }


        //生成审查结果
        //各条文审查子函数
    }

}
public enum RoomPosition { overground, underground, semi_underground }

#endif