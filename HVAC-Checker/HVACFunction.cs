using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HVAC_Checker
{
    class HVACFunction
    {
        //获取指定类型、指定名称、大于一定面积的地上或地下房间对象集合
        static List<Room> GetRooms(string type, string name, double area, bool isUp)
        {
            List<Room> rooms = new List<Room>();
            return rooms;
        }

        // 判断房间是否有某种构件并返回构件对象
    

            //找到与风口相连的风机对象
            static List<Fan> GetFanIDFromAirterminal(string airterminalID)
            {
                List<Fan> Fans = new List<Fan>();
                return Fans;
            }

            //找到风机的取风口或排风口对象
            static List<AirTerminal> GetAirTerminalFromFan(string fanID)
            {
                List<AirTerminal> airTerminals = new List<AirTerminal>();
                return airTerminals; 
            }

            //找到大于一定长度的走道对象

            //找到一个风系统的全部风口对象集合
            static List<AirTerminal> GetOutletIDsFromDuctSysterm(string ductSystermID)
            {
                List<AirTerminal> airTerminals = new List<AirTerminal>();
                return airTerminals;
             }

            //找到穿越某些房间的风管对象集合
            static List<Duct> GetDuctIDsFromSpace(string spaceID)
            {
                List<Duct> ducts = new List<Duct>();
                return ducts;
            }
            //找到穿越防火分区的风管对象集合
            static List<Duct> GetDuctIDsFromFireDistrict(string spaceID)
            {
                List<Duct> ducts = new List<Duct>();
                return ducts;
            }
        //找到穿越防火分隔处的变形缝两侧的风管集合
        static List<Duct> GetDuctIDsFromFireSide(string spaceID)
        {
            List<Duct> ducts = new List<Duct>();
            return ducts;
        }


        //获得构建所在房间的对象

        //获得一个窗户对象的有效面积
        static double GetArea(Windows window)
        {
            double dArea = 0.0;
            return dArea;
        }

        //找到属于某种类型的房间对象集合
        static List<Room> GetRooms(string roomType)
        {
            List<Room> rooms = new List<Room>();
            return rooms;
        }
        //找到名称包含某一字段的房间对象集合
        static List<Room> GetRoomsFromPartName(string roomName)
        {
            List<Room> rooms = new List<Room>();
            return rooms;
        }

       // 判断房间属于地上房间或地下房间
       static bool isUpRoom(Room room)
        {
            bool isUp = true;
            return isUp;
        }
        //获取所有楼层对象集合
        static List<Floor> GetFloors()
        {
            List<Floor> floors = new List<Floor>();
            return floors;
        }
        //获得风机所连接的所有风管集合
        static List<Duct> GetDuctsFromFan(Fan fan)
        {
            List<Duct> ducts = new List<Duct>();
            return ducts;
        }
        //判断是否风机所连风系统所有支路都连接了风口
        static bool isLinkAirTerminal(Fan fan)
        {
            bool isLink = true;
            return isLink;
        }
        //获得防烟分区长边长度

        static double GetFireDistrictLength(FireDistrict fan)
        {
            double dLength = 0.0;
            return dLength;
        }

    
            //生成审查结果
            //各条文审查子函数
        }
    
}
