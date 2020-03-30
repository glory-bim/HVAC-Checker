using System;
using System.Collections.Generic;

namespace HVAC_CheckEngine
{
    public static class assistantFunctions
    {
        public static AirTerminal GetAirTerminalOfCertainSystem(List<AirTerminal> airTerminals,string systemType)
        {
            if(systemType==null)
            {
                throw new ArgumentException("systemType为null");
            }
            if (airTerminals == null)
                return null;

            foreach(AirTerminal airTerminal in airTerminals)
            {
                if (airTerminal.systemType == systemType)
                    return airTerminal;
            }
            return null;
        }

        public static Window GetOpenableOuterWindow(List<Window> windows)
        {
            if (windows == null)
                return null;
            foreach(Window window in windows)
            {
                if (window.isExternalWindow.Value&& window.openMode != Window.WindowOpenMode.FixWindow)
                    return window;
            }

            return null;
        }

        public static bool isRoomHaveSomeSystem(Room room, string systemName)
        {
            if (isRoomHaveSomeMechanicalSystem(room, systemName) || isRoomHaveSomeNatureSystem(room, systemName))
            {
                return true;
            }
            else
                return false;
        }

        public static bool isRoomHaveSomeMechanicalSystem(Room room, string systemName)
        {
            //如果房间中有某种系统类型的风口
            List<AirTerminal> airTerminals = HVACFunction.GetRoomContainAirTerminal(room);
            AirTerminal pressureAirTerminal = assistantFunctions.GetAirTerminalOfCertainSystem(airTerminals, systemName);
            if (pressureAirTerminal != null)
            {
                //      如果某种系统类型的风口未连接了风机，则返回否
                List<Fan> fans = HVACFunction.GetFanConnectingAirterminal(pressureAirTerminal);
                if (fans.Count == 0)
                {
                    return false;
                }
                //如果连接了风机则返回是
                return true;
            }
            return false;
        }

        public static bool isRoomHaveSomeNatureSystem(Room room, string systemName)
        {
            //      如果房间中没有可开启外窗，则返回否
            List<Window> windows = HVACFunction.GetWindowsInRoom(room);
            Window aimWindow = assistantFunctions.GetOpenableOuterWindow(windows);
            if (aimWindow == null)
            {
                return false;
            }
            //如果有可开启外窗则返回是
            else
                return true;
        }

        public static bool isRegionHaveSomeSystem(Region region, string systemName)
        {
            List<Room> nonPublicRooms = region.rooms;
            nonPublicRooms.exceptPublicRooms();
            Room corridor = getCorridorOfConnectedRegion(region);
            if (isRoomHaveSomeSystem(corridor, "排烟"))
                return true;

            foreach(Room room in nonPublicRooms)
            {
                if (!assistantFunctions.isRoomHaveSomeSystem(room, "排烟"))
                    return false;
            }
           return true;
            
        }

        public static List<Room> filtrateRoomsBetweenFloor_aAndFloor_b(List<Room> rooms, int floor_a, int floor_b)
        {
            List<Room> aimRooms = new List<Room>();
            foreach (Room room in rooms)
            {
                if (room.m_iStoryNo >= floor_a && room.m_iStoryNo <= floor_b)
                    aimRooms.Add(room);
            }
            return aimRooms;
        }

        public static List<T> exceptSameItems<T>(this List<T> items, List<T> exceptedItems) where T : Element
        {
            List<T> items_copy = new List<T>();
            items_copy.AddRange(items);
            foreach (T item in exceptedItems)
            {
                T aimItem = items_copy.findItem(item);
                items_copy.Remove(aimItem);
            }
            return items_copy;
        }

        public static List<T> getCommonItems<T>(this List<T> firstItems, List<T> secondItems) where T : Element
        {
            List<T> commonItems = new List<T>();
            foreach (T item in firstItems)
            {
                if (secondItems.findItem(item) != null)
                    commonItems.Add(item);
            }
            return commonItems;
        }


        public static T findItem<T>(this List<T> items, T aimItem)where T:Element
        {
            Element aimElement = aimItem as Element;
            foreach (T item in items)
            {
                Element element = item as Element;
                if (element.Id == aimElement.Id)
                    return item;
            }
            return null;
        }



       public static bool isCommonOfenStayRoom(Room room)
        {
            List<string> commonOfenStayRoomTypes = new List<string>(CommonOfenStayRoomTypes);

            return commonOfenStayRoomTypes.Exists(type => type == room.type);

        }


        public static bool isPublicRoom(Room room)
        {
            List<string> publicRooms = new List<string>(PublicRooms);

            return publicRooms.Exists(type => type == room.type);

        }

        public static List<Room> getAllWindowlessRooms(List<Room> rooms)
        {
            List<Room> windowlessRooms = new List<Room>();
            foreach (Room room in rooms)
            {
                if (HVACFunction.GetWindowsInRoom(room).Count == 0)
                    windowlessRooms.Add(room);
            }
            return windowlessRooms;
        }

        //依次遍历区域集合中的每一个区域
        //如果区域是地下或半地下区域，则计算此区域所有房间面积之和是否大于200㎡，
        //如果大于200㎡则将这个区域加到需要排烟区域的集合needSmokeExhaustRegions中
        //如果区域是地上区域，则筛选出此区域所有无窗房间集合windowlessRooms，
        //并计算集合中所有房间面积之和，
        //如果大于200㎡则将此区域加到需要排烟区域的集合needSmokeExhaustRegions中
        //返回需要排烟的区域集合needSmokeExhaustRegions

       public static List<Region> filtrateNeedSmokeExhaustRegions(List<Region> regions)
        {
            List<Region> needSmokeExhaustRegions = new List<Region>();
            //依次遍历区域集合中的每一个区域
            foreach (Region region in regions)
            {
                //如果区域是地下或半地下区域，则计算此区域所有房间面积之和是否大于200㎡，
                if (!isOvergroundRegion(region))
                {
                    if (getSumOfAllRoomsAreaOfRooms(region.rooms) > 200)
                        needSmokeExhaustRegions.Add(region);
                }
                //如果区域是地上区域，则筛选出此区域所有无窗房间集合windowlessRooms，
                else
                {

                    List<Room> windowlessRooms = getAllWindowlessRooms(region.rooms);
                    //并计算集合中所有房间面积之和，
                    //如果大于200㎡则将此区域加到需要排烟区域的集合needSmokeExhaustRegions中
                    if (getSumOfAllRoomsAreaOfRooms(windowlessRooms) > 200)
                        needSmokeExhaustRegions.Add(region);
                }


            }
            //返回需要排烟的区域集合needSmokeExhaustRegions
            return needSmokeExhaustRegions;
        }

        private static bool isOvergroundRegion(Region region)
        {
            if (region.rooms.Count == 0)
                throw new ArgumentException("区域中没有房间");
            if (region.rooms[0].m_eRoomPosition == RoomPosition.overground)
                return true;
            else
                return false;
        }

        public static double getSumOfAllRoomsAreaOfRooms(List<Room> rooms)
        {
            double sum = 0;
            foreach (Room room in rooms)
            {
                if (!isPublicRoom(room))
                    sum += room.m_dArea.Value;
            }
            return sum;
        }

        private static List<Room> filtrateAllPublicRoom(List<Room> rooms)
        {
            List<Room> publicRooms = new List<Room>();
            foreach(Room room in rooms)
            {
                if (isPublicRoom(room))
                    publicRooms.Add(room);
            }
            return publicRooms;
        }

        public static List<Room> exceptPublicRooms(this  List<Room> rooms)
        {
            List<Room> publicRooms = assistantFunctions.filtrateAllPublicRoom(rooms);
            return rooms.exceptSameItems(publicRooms);
        }


        public static bool isStairPressureAirSystemIndependent(Room stairCase)
        {
            //获得楼梯间包含的所有风口集合airTerminalsInStairCase
            List<AirTerminal> airTerminalsInStairCase= HVACFunction.GetRoomContainAirTerminal(stairCase);
            if (airTerminalsInStairCase.Count == 0)
                throw new ArgumentException("未设置机械加压送风系统");
            //遍历airTerminalsInStairCase中的每一个风口
            Dictionary<long, Fan> fans = new Dictionary<long, Fan>();
            foreach(AirTerminal airTerminal in airTerminalsInStairCase)
            {
                //找到风口所连接的风机
                List<Fan> temp_fans = HVACFunction.GetFanConnectingAirterminal(airTerminal);
                if (temp_fans.Count <= 0)
                    throw new modelException("风口未连接风机");
                Fan fan = temp_fans[0];
                //  如果风机未加入到风机集合fans中,
                if (!fans.ContainsKey(fan.Id.Value))
                {
                    fans.Add(fan.Id.Value, fan);
                }
            }
            Dictionary<long, AirTerminal> airTerminalsConnectedFans = new Dictionary<long, AirTerminal>();
            //依次遍历风机集合fans中的每一个风机
            foreach (KeyValuePair<long, Fan> fan in fans)
            {
                //  获得风机连接的所有风口，并将这些风口加入到风口集合airTerminalsConnectedFans中
                List<AirTerminal> temp_airTerminals = HVACFunction.GetOutletsOfFan(fan.Value);
                foreach(AirTerminal airTerminal in temp_airTerminals)
                {
                    if (!airTerminalsConnectedFans.ContainsKey(airTerminal.Id.Value))
                        airTerminalsConnectedFans.Add(airTerminal.Id.Value, airTerminal);
                }
            }
            //如果airTerminalsInStairCase集合中的风口数量与airTerminalsConnectedFans集合中的风口数量不同
            if (airTerminalsInStairCase.Count != airTerminalsConnectedFans.Count)
                //则返回false
                return false;
            else
                 return true;
        }
 
        //获得前室中的风口的集合airTerminalsInAtria
        //获得airTerminalsInAtria集合中所有风口所连接的风机的集合Fans
        //获得Fans集合中风机所连接的所有风口的集合airTerminalsConnectToFans
        //从airTerminalsConnectToFans集合中除去airTerminalsInAtria集合中的风口
        //依次遍历airTerminalsConnectToFans集合中的风口。
        //  如果风口不在前室中
        //      则返回false
        //如果所有风口都在前室中
        //  怎返回True
        public static bool isAtriaPressureAirSystemIndependent(Room atria)
        {
            //获得前室中的风口的集合airTerminalsInAtria
            List<AirTerminal> airTerminalsInAtria = HVACFunction.GetRoomContainAirTerminal(atria);
            if (airTerminalsInAtria.Count == 0)
                throw new ArgumentException("未设置机械加压送风系统");
            //获得airTerminalsInAtria集合中所有风口所连接的风机的集合Fans
            List<Fan> fans = new List<Fan>();
            foreach(AirTerminal airTerminal in airTerminalsInAtria)
            {
                fans.AddRange(HVACFunction.GetFanConnectingAirterminal(airTerminal));
            }
            if (fans.Count <= 0)
                throw new modelException("风口未连接风机");

            //获得Fans集合中风机所连接的所有风口的集合airTerminalsConnectToFans
            List<AirTerminal> airTerminalsConnectToFans = new List<AirTerminal>();
            foreach(Fan fan in fans)
            {
                airTerminalsConnectToFans.AddRange(HVACFunction.GetOutletsOfFan(fan));
            }
            //从airTerminalsConnectToFans集合中除去airTerminalsInAtria集合中的风口
            airTerminalsConnectToFans=airTerminalsConnectToFans.exceptSameItems(airTerminalsInAtria);
            //依次遍历airTerminalsConnectToFans集合中的风口。
            foreach(AirTerminal airTerminal in airTerminalsConnectToFans)
            {
                //  如果风口不在前室中
                Room room = HVACFunction.GetRoomOfAirterminal(airTerminal);
                if (!room.type.Contains("前室"))
                    //      则返回false
                    return false;
            }
            //如果所有风口都在前室中
            //  怎返回True
            return true;
        }

        //获得前室所有相连房间的集合linkedRooms
        //从linkedRooms中筛选出非楼梯间房间
        //依次遍历linkedRooms
        //获得前室与房间的连通门并放入门的集合中DoorsToCorridor
        //返回DoorsToCorridor
        public static List<Door> getDoorsToCorridorOfAtria(Room atria)
        {
            //获得前室所有相连房间的集合linkedRooms
            List<Room> linkedRooms = HVACFunction.GetConnectedRooms(atria);
            if (linkedRooms.Count <= 0)
                throw new modelException("前室没有与其他房间相连");
            //从linkedRooms中筛选出非楼梯间房间
            List<Room> stairCases = new List<Room>();
            foreach(Room room in linkedRooms)
            {
                if (room.type == "防烟楼梯间")
                    stairCases.Add(room);
            }
            linkedRooms = linkedRooms.exceptSameItems(stairCases);
            if(linkedRooms.Count<=0)
                throw new modelException("前室仅与楼梯间相连");
            List<Door> doorsToCorridor = new List<Door>();
            //依次遍历linkedRooms
            foreach (Room room in linkedRooms)
            {
                //获得前室与房间的连通门并放入门的集合中DoorsToCorridor
                doorsToCorridor.AddRange(HVACFunction.getDoorsBetweenTwoRooms(atria, room));
            }
            //返回DoorsToCorridor
            return doorsToCorridor;
        }

        public static bool isViolateRoomAlreadyInResult(Room room,BimReview result)
        {
            List<ComponentAnnotation> violateComponents = result.violationComponents;
            foreach(ComponentAnnotation violateComponent in violateComponents)
            {
                if (room.Id == violateComponent.Id)
                    return true;
            }
            return false;
        }

        private static Room getCorridorOfConnectedRegion(Region region)
        {
            foreach(Room room in region.rooms)
            {
                if (room.type == "走廊" || room.type == "走道")
                    return room;
            }
            //如果区域没有走廊则抛出异常
            throw new ArgumentException("区域中没有走廊");
        }
        private static string[] CommonOfenStayRoomTypes = { "办公室", "会议室", "报告厅", "商场" };
        private static string[] PublicRooms = { "走廊", "走道", "楼梯间", "前室", "避难间" };
    }
}
