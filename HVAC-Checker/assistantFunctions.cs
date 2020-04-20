using System;
using System.Collections.Generic;

namespace HVAC_CheckEngine
{
    public static class assistantFunctions
    {
        public static AirTerminal GetAirTerminalOfCertainSystem(List<AirTerminal> airTerminals, string systemType)
        {
            if (systemType == null)
            {
                throw new ArgumentException("systemType为null");
            }
            if (airTerminals == null)
                return null;

            foreach (AirTerminal airTerminal in airTerminals)
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
                if (window.isExternalWindow.Value && window.effectiveArea > 0)
                    return window;
            }

            return null;
        }

        public static bool isRoomHaveSomeSystem(Room room, string systemName)
        {
            if (room == null)
                throw new ArgumentException();
            if (isRoomHaveSomeMechanicalSystem(room, systemName))
            {
                return true;
            }
            else if ( systemName.Contains("排烟")&& isRoomHaveNatureSmokeExhaustSystem(room))
            {
                return true;
            }
            else if(isRoomHaveNatureVentilateSystem(room))
            {
                return true;
            }
            else
                return false;
        }


        public static bool isRoomHaveSomeMechanicalSystem(Room room, string systemName)
        {
            if (room == null || systemName == null)
                throw new ArgumentException();
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

        public static bool isRoomHaveNatureVentilateSystem(Room room)
        {
            if (room == null)
                throw new ArgumentException();
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

        public static bool isRoomHaveNatureSmokeExhaustSystem(Room room)
        {
            if (room == null)
                throw new ArgumentException();
            //      如果房间中没有可开启外窗，则返回否
            List<Window> windows = HVACFunction.GetWindowsInRoom(room);
            Window aimWindow = assistantFunctions.GetOpenableOuterWindow(windows);
            if (aimWindow == null)
            {
                return false;
            }
            else if(!aimWindow.isSmokeExhaustWindow.Value)
            {
                return false;
            }
            //如果有可开启外窗则返回是
            else 
                return true;
        }

        public static bool isRegionHaveSomeSystem(Region region, string systemName)
        {
            if (region == null || systemName == null)
                throw new ArgumentException();
            List<Room> nonPublicRooms = region.rooms;
            nonPublicRooms.exceptPublicRooms();
            Room corridor = getCorridorOfConnectedRegion(region);
            if (isRoomHaveSomeSystem(corridor, "排烟"))
                return true;
            foreach (Room room in nonPublicRooms)
            {
                if (!assistantFunctions.isRoomHaveSomeSystem(room, "排烟"))
                    return false;
            }
            return true;

        }

        public static List<T> filtrateElementsBetweenFloor_aAndFloor_b<T>(List<T> elements, int floor_a, int floor_b) where T : Element
        {
            if (elements == null)
                throw new ArgumentException();
            List<T> aimElements = new List<T>();
            foreach (T element in elements)
            {
                if (element.m_iStoryNo >= floor_a && element.m_iStoryNo <= floor_b)
                    aimElements.Add(element);
            }
            return aimElements;
        }



        public static List<T> exceptSameItems<T>(this List<T> items, List<T> exceptedItems) where T : Element
        {
            if (items == null || exceptedItems == null)
                throw new ArgumentException();
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
            if (firstItems == null || secondItems == null)
                throw new ArgumentException();
            List<T> commonItems = new List<T>();
            foreach (T item in firstItems)
            {
                if (secondItems.findItem(item) != null)
                    commonItems.Add(item);
            }
            return commonItems;
        }

       
        
       
        public static bool isSmokeCompartmentSpanFireCompartment(SmokeCompartment smokeCompartment)
        {
            //获得所有防火分区对象的集合
            List<FireCompartment> fireCompartments = HVACFunction.GetFireCompartment("");
            //从防火分区对象集合中筛选出与防烟分区同层的所有防火分区
            int m_iStoryNo = smokeCompartment.m_iStoryNo.Value;
            int highestm_iStoryNo = HVACFunction.GetHighestStoryNoOfRoom(smokeCompartment);
            fireCompartments = filtrateElementsBetweenFloor_aAndFloor_b(fireCompartments, m_iStoryNo, highestm_iStoryNo);
            //依次判断防烟分区是否与防火分区相交
            foreach(FireCompartment fireCompartment in fireCompartments)
            {
                //如果相交则返回true
                if (HVACFunction.IsSmokeCompartmentIntersectFireCompartment(smokeCompartment, fireCompartment))
                    return true;
            }
            //如果都不相交则返回false
            return false;
        }

        public static T findItem<T>(this List<T> items, T aimItem) where T : Element
        {
            if (items == null|| aimItem == null)
                throw new ArgumentException();

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
            if (room == null)
                throw new ArgumentException();
            List<string> commonOfenStayRoomTypes = new List<string>(CommonOfenStayRoomTypes);

            return commonOfenStayRoomTypes.Exists(type => type == room.type);

        }


        public static bool isPublicRoom(Room room)
        {
            if (room == null)
                throw new ArgumentException();

            List<string> publicRooms = new List<string>(PublicRooms);

            return publicRooms.Exists(type => type == room.type);

        }

        public static List<Room> getAllWindowlessRooms(List<Room> rooms)
        {
            if (rooms == null)
                throw new ArgumentException();

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
            if (regions == null)
                throw new ArgumentException();
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

        public static List<AirTerminal> filtrateAirTerminalOfSomeSystem(List<AirTerminal> airTerminals, string system)
        {
            if (airTerminals == null || system == null)
                throw new ArgumentException();
            List<AirTerminal> aimAirTerminals = new List<AirTerminal>();
            foreach (AirTerminal airTerminal in airTerminals)
            {
                if (airTerminal.systemType.Contains(system))
                    aimAirTerminals.Add(airTerminal);
            }
            return aimAirTerminals;
        }

        public static Window findWindowNoSmallerThanSomeEffectiveArea(this List<Window> windows, double area)
        {
            if (windows == null)
                throw new ArgumentException();
            foreach (Window window in windows)
            {
                if (window.effectiveArea >= area)
                    return window;
            }
            return null;
        }

        public static List<Fan> getAllFansConnectToAirTerminals(List<AirTerminal> airTerminals)
        {
            if (airTerminals == null)
                throw new ArgumentException();
           Dictionary<long,Fan> fans = new Dictionary<long, Fan>();
            foreach (AirTerminal airTerminal in airTerminals)
            {
                List<Fan> temp_fans = HVACFunction.GetFanConnectingAirterminal(airTerminal);
                if (temp_fans.Count == 0)
                    throw new modelException("风口没有连接风机");
                if (!fans.ContainsKey(temp_fans[0].Id.Value))
                    fans.Add(temp_fans[0].Id.Value, temp_fans[0]);
            }
            List<Fan> aimFans =new List<Fan>();
            aimFans.AddRange(fans.Values);
            return aimFans;
        }

        public static double calculateTotalAreaOfWindows(List<Window> windows)
        {
            if (windows == null)
                throw new ArgumentException();
            double sum = 0;
            foreach (Window window in windows)
            {
                sum += window.area.Value;
            }
            return sum;
        }


        public static double calculateTotalEffectiveAreaOfWindows(List<Window> windows)
        {
            if (windows == null)
                throw new ArgumentException();
            double sum = 0;
            foreach (Window window in windows)
            {
                sum += window.effectiveArea.Value;
            }
            return sum;
        }

        public static double calculateSmokeExhaustFlowOfSmokeCompartment(SmokeCompartment smokeCompartment)
        {
            if (smokeCompartment == null)
                throw new ArgumentException();
            List<AirTerminal> smokeExhaustAirTerminals = HVACFunction.GetRoomContainAirTerminal(smokeCompartment);
            return getTotalAirVolumeOfAirTerminals(smokeExhaustAirTerminals);
        }

        public static double calculateExhaustFlowOfRoom(Room room)
        {
            if (room == null)
                throw new ArgumentException();
            List<AirTerminal> exhaustAirTerminals = HVACFunction.GetRoomContainAirTerminal(room);
            exhaustAirTerminals = filtrateAirTerminalOfSomeSystem(exhaustAirTerminals,"排风");
            return getTotalAirVolumeOfAirTerminals(exhaustAirTerminals);
        }

        public static double calculateSupplyFlowOfRoom(Room room)
        {
            if (room == null)
                throw new ArgumentException();
            List<AirTerminal>airTerminals = HVACFunction.GetRoomContainAirTerminal(room);
            List<AirTerminal> supplyAirTerminals = filtrateAirTerminalOfSomeSystem(airTerminals, "送风");
            supplyAirTerminals.AddRange(filtrateAirTerminalOfSomeSystem(airTerminals, "新风"));
            return getTotalAirVolumeOfAirTerminals(supplyAirTerminals);
        }

        public static List<Duct> addDuctsToList(this List<Duct> firstList, List<Duct> secondList)
        {
            if (firstList == null && secondList == null)
                throw new ArgumentException();

            foreach(Duct duct in secondList)
            {
                if(firstList.findItem(duct) == null)
                {
                    firstList.Add(duct);
                }
            }
            return firstList;
        }

        public static List<Duct> filterSomeSystemTypeDuctsFromList(this List<Duct> list, List<string> systemTypes)
        {
            if (list == null || systemTypes == null)
                throw new ArgumentException();

            List<Duct> aimList = new List<Duct>();
            foreach (Duct duct in list)
            {
                foreach(string systemType in systemTypes)
                {
                    if (duct.systemType.Contains(systemType))
                    {
                        aimList.Add(duct);
                        break;
                    }
                }
            }
            return aimList;
        }

        public static List<Duct>filterSameDuctsInTwoList(List<Duct>firstList,List<Duct>secondList)
        {
            List<Duct> aimDucts = new List<Duct>();
            foreach(Duct duct in firstList)
            {
                if(secondList.findItem(duct)!=null)
                {
                    aimDucts.Add(duct);
                }
            }
            return aimDucts;
        }

        public static List<Duct> removeDuctsFromList(this List<Duct> list,List<Duct>ducts)
        {
            if (list == null || ducts == null)
                throw new ArgumentException();
            foreach (Duct duct in ducts)
            {
                Duct needRemovDuct = list.findItem(duct);
                if (needRemovDuct != null)
                {
                    list.Remove(needRemovDuct);
                }
            }
            return list;
        }


        public static SmokeCompartment getMaxAreaSmokeCompartment(List<SmokeCompartment>smokeCompartments)
        {
            if (smokeCompartments == null||smokeCompartments.Count==0)
                throw new ArgumentException();
                
            SmokeCompartment aimSmokeCompartment = smokeCompartments[0];
            foreach(SmokeCompartment smokeCompartment in smokeCompartments)
            {
                if (aimSmokeCompartment.m_dArea.Value < smokeCompartment.m_dArea.Value)
                    aimSmokeCompartment = smokeCompartment;
            }
            return aimSmokeCompartment;
        }

        private static bool isOvergroundRegion(Region region)
        {
            if (region == null)
                throw new ArgumentException();

            if (region.rooms.Count == 0)
                throw new ArgumentException("区域中没有房间");
            if (region.rooms[0].m_eRoomPosition == RoomPosition.overground)
                return true;
            else
                return false;
        }

        public static double getSumOfAllRoomsAreaOfRooms(List<Room> rooms)
        {
            if (rooms == null)
                throw new ArgumentException();

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
            if (rooms == null)
                throw new ArgumentException();

            List<Room> publicRooms = new List<Room>();
            foreach (Room room in rooms)
            {
                if (isPublicRoom(room))
                    publicRooms.Add(room);
            }
            return publicRooms;
        }

        public static List<Room> exceptPublicRooms(this List<Room> rooms)
        {
            if (rooms == null)
                throw new ArgumentException();

            List<Room> publicRooms = assistantFunctions.filtrateAllPublicRoom(rooms);
            return rooms.exceptSameItems(publicRooms);
        }

        public struct exceptRoomCondition
        {
            public string type;
            public string name;
            public double area;
            public RoomPosition roomPosition;
            public AreaType areaType;
            public enum AreaType{ LargerThan,SmallerThan,LargerAndEqualThan,SmallerAndEqualThan}
        }

        public static List<Room>exceptRoomNoSmallerThanArea(this List<Room>rooms,double area)
        {
            List<Room> aimRooms = new List<Room>();
            aimRooms.AddRange(rooms);

            foreach(Room room in rooms)
            {
                if (room.m_dArea.Value >= area)
                    aimRooms.Remove(room);
            }
            return aimRooms;
        }

        public static List<Room> exceptSomeTypeRooms(this List<Room> rooms, string type)
        {
            List<Room> aimRooms = new List<Room>();
            aimRooms.AddRange(rooms);

            foreach (Room room in rooms)
            {
                if (room.type.Equals(type))
                    aimRooms.Remove(room);
            }
            return aimRooms;
        }

        public static List<Room>exceptSomeRooms(this List<Room> rooms,List<exceptRoomCondition> conditions)
        {
            if (rooms == null || conditions == null)
                throw new ArgumentException();
            List<Room> aimRooms = new List<Room>();
            aimRooms.AddRange(rooms);
            foreach(Room room in rooms)
            {
                foreach(exceptRoomCondition condition in conditions)
                {
                    bool isRemoveRoom = false;
                    switch (condition.areaType)
                    {
                        case exceptRoomCondition.AreaType.LargerThan:
                            if (room.type.Contains(condition.type) && room.name.Contains(condition.name) && room.m_dArea > condition.area &&
                                room.m_eRoomPosition == condition.roomPosition)
                            {
                                aimRooms.Remove(room);
                                isRemoveRoom = true;
                            }
                            break;
                        case exceptRoomCondition.AreaType.LargerAndEqualThan:
                            if (room.type.Contains(condition.type) && room.name.Contains(condition.name) && room.m_dArea >= condition.area &&
                               room.m_eRoomPosition == condition.roomPosition)
                            {
                                aimRooms.Remove(room);
                                isRemoveRoom = true;
                            }
                            break;
                        case exceptRoomCondition.AreaType.SmallerThan:
                            if (room.type.Contains(condition.type) && room.name.Contains(condition.name) && room.m_dArea < condition.area &&
                               room.m_eRoomPosition == condition.roomPosition)
                            {
                                aimRooms.Remove(room);
                                isRemoveRoom = true;
                            }
                            break;
                        case exceptRoomCondition.AreaType.SmallerAndEqualThan:
                            if (room.type.Contains(condition.type) && room.name.Contains(condition.name) && room.m_dArea <= condition.area &&
                               room.m_eRoomPosition == condition.roomPosition)
                            {
                                aimRooms.Remove(room);
                                isRemoveRoom = true;
                            }
                            break;
                    }
                    if (isRemoveRoom)
                        break;
                }
            }
            return aimRooms;
        }

        public static bool isStairPressureAirSystemIndependent(Room stairCase)
        {
            if (stairCase == null)
                throw new ArgumentException();
            //获得楼梯间包含的所有风口集合airTerminalsInStairCase
            List<AirTerminal> airTerminalsInStairCase = HVACFunction.GetRoomContainAirTerminal(stairCase);
            if (airTerminalsInStairCase.Count == 0)
                throw new ArgumentException("未设置机械加压送风系统");
            //遍历airTerminalsInStairCase中的每一个风口
            Dictionary<long, Fan> fans = new Dictionary<long, Fan>();
            foreach (AirTerminal airTerminal in airTerminalsInStairCase)
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
                foreach (AirTerminal airTerminal in temp_airTerminals)
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
            if (atria == null)
                throw new ArgumentException();
            //获得前室中的风口的集合airTerminalsInAtria
            List<AirTerminal> airTerminalsInAtria = HVACFunction.GetRoomContainAirTerminal(atria);
            if (airTerminalsInAtria.Count == 0)
                throw new ArgumentException("未设置机械加压送风系统");
            //获得airTerminalsInAtria集合中所有风口所连接的风机的集合Fans
            List<Fan> fans = new List<Fan>();
            foreach (AirTerminal airTerminal in airTerminalsInAtria)
            {
                fans.AddRange(HVACFunction.GetFanConnectingAirterminal(airTerminal));
            }
            if (fans.Count <= 0)
                throw new modelException("风口未连接风机");

            //获得Fans集合中风机所连接的所有风口的集合airTerminalsConnectToFans
            List<AirTerminal> airTerminalsConnectToFans = new List<AirTerminal>();
            foreach (Fan fan in fans)
            {
                airTerminalsConnectToFans.AddRange(HVACFunction.GetOutletsOfFan(fan));
            }
            //从airTerminalsConnectToFans集合中除去airTerminalsInAtria集合中的风口
            airTerminalsConnectToFans = airTerminalsConnectToFans.exceptSameItems(airTerminalsInAtria);
            //依次遍历airTerminalsConnectToFans集合中的风口。
            foreach (AirTerminal airTerminal in airTerminalsConnectToFans)
            {
                //  如果风口不在前室中
                Room room = HVACFunction.GetRoomOfAirterminal(airTerminal);
                if (room != null && !room.type.Contains("前室"))
                    //      则返回false
                    return false;
            }
            //如果所有风口都在前室中
            //  怎返回True
            return true;
        }

        static public List<Fan> getFansOfSomeSyetemType(string systemType)
        {
            if (systemType == null)
                throw new ArgumentException();
            //获得所有风机
            List<Fan> fans = HVACFunction.GetAllFans();
            List<Fan> aimFans = new List<Fan>();
            //依次遍历每一台风机
            foreach (Fan fan in fans)
            {
                //获得风机连接的所有排风口
                List<AirTerminal> airTerminals = HVACFunction.GetInletsOfFan(fan);
                //如果风口里面有排烟口则将风机加入到目标风机集合中
                foreach (AirTerminal airTerminal in airTerminals)
                {
                    if (airTerminal.systemType.Contains(systemType))
                    {
                        aimFans.Add(fan);
                        break;
                    }
                }
            }
            return aimFans;
        }

        static public double getAffordHeightOfSomkeFan(Fan smokeFan)
        {
            if (smokeFan == null)
                throw new ArgumentException();
            //获得风机连接的所有排风口
            List<AirTerminal> airTerminals = HVACFunction.GetInletsOfFan(smokeFan);
            double maxHeight = double.MinValue;
            double minHeight = double.MaxValue;
            foreach(AirTerminal airTerminal in airTerminals)
            {
                if (airTerminal.elevation > maxHeight)
                    maxHeight = airTerminal.elevation.Value;
                if (airTerminal.elevation < minHeight)
                    minHeight = airTerminal.elevation.Value;
            }
            return maxHeight - minHeight;
        }

        //获得前室所有相连房间的集合linkedRooms
        //从linkedRooms中筛选出非楼梯间房间
        //依次遍历linkedRooms
        //获得前室与房间的连通门并放入门的集合中DoorsToCorridor
        //返回DoorsToCorridor
        public static List<Door> getDoorsToCorridorOfAtria(Room atria)
        {
            if (atria == null)
                throw new ArgumentException();

            //获得前室所有相连房间的集合linkedRooms
            List<Room> linkedRooms = HVACFunction.GetConnectedRooms(atria);
            if (linkedRooms.Count <= 0)
                throw new modelException("前室没有与其他房间相连");
            //从linkedRooms中筛选出非楼梯间房间
            List<Room> stairCases = new List<Room>();
            foreach (Room room in linkedRooms)
            {
                if (room.type == "防烟楼梯间")
                    stairCases.Add(room);
            }
            linkedRooms = linkedRooms.exceptSameItems(stairCases);
            if (linkedRooms.Count <= 0)
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

        public static bool isViolateRoomAlreadyInResult(Room room, BimReview result)
        {
            if (room == null)
                throw new ArgumentException();

            List<ComponentAnnotation> violateComponents = result.violationComponents;
            foreach (ComponentAnnotation violateComponent in violateComponents)
            {
                if (room.Id == violateComponent.Id)
                    return true;
            }
            return false;
        }

        private static Room getCorridorOfConnectedRegion(Region region)
        {
            if (region == null || region.rooms == null)
                throw new ArgumentException();

            foreach (Room room in region.rooms)
            {
                if (room.type == "走廊" || room.type == "走道")
                    return room;
            }
            //如果区域没有走廊则抛出异常
            throw new ArgumentException("区域中没有走廊");
        }

        //对楼层对象进行排序
        //从最低楼层开始
        //依次遍历每个楼层
        //如果当前楼层有风口且目标风口为空
        //则将当前楼层风口设为目标风口
        //如果目标风口不为空
        //则将目标风口及楼层列表加入到结果中
        //并将本层风口设置为目标风口
        //清空楼层列表
        //将当前楼层加入到楼层列表中


        public static Dictionary<AirTerminal, List<Floor>> getFloorDivisionOfAirTerminalsBottomUp(List<Floor> floors, List<AirTerminal> airTerminals)
        {
            if (floors == null || airTerminals == null)
                throw new ArgumentException();
            //对楼层对象进行排序
            floors.Sort((x, y) => x.m_iStoryNo.Value.CompareTo(y.m_iStoryNo.Value));
            return getFloorDivisionOfAirTerminals(floors, airTerminals);

        }

        public static Dictionary<AirTerminal, List<Floor>> getFloorDivisionOfAirTerminalsTopToBottom(List<Floor> floors, List<AirTerminal> airTerminals)
        {
            if (floors == null || airTerminals == null)
                throw new ArgumentException();
            //对楼层对象进行排序
            floors.Sort((x, y) => y.m_iStoryNo.Value.CompareTo(x.m_iStoryNo.Value));
            return getFloorDivisionOfAirTerminals(floors, airTerminals);

        }

        private static Dictionary<AirTerminal, List<Floor>> getFloorDivisionOfAirTerminals(List<Floor> floors, List<AirTerminal> airTerminals)
        {
            if (floors == null || airTerminals == null)
                throw new ArgumentException();
            Dictionary<AirTerminal, List<Floor>> FloorDivisionOfAirTerminals = new Dictionary<AirTerminal, List<Floor>>();
            //从最低楼层开始
            AirTerminal aimAirTerminal = null;
            List<Floor> affordFloors = new List<Floor>();
            //依次遍历每个楼层
            foreach (Floor floor in floors)
            {
                //如果当前楼层有风口且目标风口为空
                List<AirTerminal> airTerminalInCurrentFloor = getAirTerminalsInFloor(floor, airTerminals);
                if (airTerminalInCurrentFloor.Count > 0 && aimAirTerminal == null)
                {
                    //则将当前楼层风口设为目标风口
                    aimAirTerminal = airTerminalInCurrentFloor[0];
                }
                //如果目标风口不为空且当前楼层有风口
                else if (airTerminalInCurrentFloor.Count > 0 && aimAirTerminal != null)
                {
                    //则将目标风口及楼层列表加入到结果中
                    FloorDivisionOfAirTerminals.Add(aimAirTerminal, affordFloors);
                    //将本层风口设置为目标风口

                    aimAirTerminal = airTerminalInCurrentFloor[0];
                    //清空楼层列表
                    affordFloors = new List<Floor>();
                }
                //如果负担的楼层已经为3层
                else if (affordFloors.Count == 3)
                {
                    //如果目标风口不为空
                    if (aimAirTerminal != null)
                    {
                        //则将目标风口及楼层列表加入到结果中
                        FloorDivisionOfAirTerminals.Add(aimAirTerminal, affordFloors);
                        aimAirTerminal = null;
                    }
                    //清空楼层列表
                    affordFloors = new List<Floor>();
                }
                //将当前楼层加入到楼层列表中
                affordFloors.Add(floor);
            }
            if (aimAirTerminal != null)
                FloorDivisionOfAirTerminals.Add(aimAirTerminal, affordFloors);

            return FloorDivisionOfAirTerminals;
        }




        public static double getAffordHeightOfFanByFloorDivision(Fan fan, Dictionary<AirTerminal, List<Floor>> floorDivision)
        {
            if (fan == null || floorDivision == null)
                throw new ArgumentException();
            //找到风机的所有加压送风口
            List<AirTerminal> airTerminalsConnectToFan = HVACFunction.GetOutletsOfFan(fan);
            if (airTerminalsConnectToFan.Count == 0)
                return 0;


            double lowestHeight = double.MaxValue;
            double highestHeight = double.MinValue;

            foreach (AirTerminal airTerminal in airTerminalsConnectToFan)
            {
                AirTerminal airTerminalInDictionary = floorDivision.findElementFromDictionary(airTerminal);
                if (airTerminalInDictionary == null)
                    throw new ArgumentException();

                foreach (Floor floor in floorDivision[airTerminalInDictionary])
                {
                    if (lowestHeight > floor.elevation)
                        lowestHeight = floor.elevation.Value;
                    if (highestHeight < floor.elevation.Value + floor.height.Value)
                        highestHeight = floor.elevation.Value + floor.height.Value;
                }

            }
            return highestHeight - lowestHeight;
        }


        static public T1 findElementFromDictionary<T1, T2>(this Dictionary<T1, List<T2>> dictionary, T1 element) where T1 : Element
                                                                                             
        {
            if (dictionary == null || element == null)
                throw new ArgumentException();

            foreach (KeyValuePair<T1, List<T2>> pair in dictionary)
            {
                if (pair.Key.Id.Value.Equals(element.Id.Value))
                    return pair.Key;
            }
            return null;
        }
       
        private static List<AirTerminal> getAirTerminalsInFloor(Floor floor, List<AirTerminal> airTerminals)
        {
            if (floor == null || airTerminals == null)
                throw new ArgumentException();

            List<AirTerminal> aimAirTerminals = new List<AirTerminal>();
            int m_iStoryNo = floor.m_iStoryNo.Value;
            foreach (AirTerminal airTerminal in airTerminals)
            {
                if (airTerminal.m_iStoryNo == m_iStoryNo)
                    aimAirTerminals.Add(airTerminal);
            }
            return aimAirTerminals;
        }

        public static List<T2> getValueAccordingToKey<T1, T2>(this Dictionary<T1, List<T2>> dictionary, T1 key) where T1 : Element
                                                                                                               where T2 : Element
        {
            if (dictionary == null || key == null)
                throw new ArgumentException();

            foreach (KeyValuePair<T1, List<T2>> pair in dictionary)
            {
                if (pair.Key.Id == key.Id)
                    return pair.Value;
            }
            return null;
        }

        public static List<Floor> filterFloorsBetweenlowestAndHighestm_iStoryNo(int lowestm_iStoryNo, int Highestm_iStoryNo)
        {
            List<Floor> allFloors = HVACFunction.GetFloors();
            List<Floor> aimFloors = new List<Floor>();
            foreach (Floor floor in allFloors)
            {
                if (floor.m_iStoryNo >= lowestm_iStoryNo && floor.m_iStoryNo <= Highestm_iStoryNo)
                    aimFloors.Add(floor);
            }
            return aimFloors;
        }

        public static List<Window> getFixOuterWindowsOfRoom(Room room)
        {
            if (room == null)
                throw new ArgumentException();
            List<Window> windows = HVACFunction.GetWindowsInRoom(room);
            List<Window> aimWindows = new List<Window>();
            foreach (Window window in windows)
            {
                if (Math.Abs(window.effectiveArea.Value) < error&& window.isExternalWindow.Value)
                {
                    aimWindows.Add(window);
                }
            }
            return aimWindows;
        }

        public static List<Wall>getOuterWallOfRoom(Room room)
        {
            if (room == null)
                throw new ArgumentException();
            List<Wall> walls = HVACFunction.GetAllWallsOfRoom(room);
            List<Wall> outerWalls = new List<Wall>();
            foreach(Wall wall in walls)
            {
                if(wall.isOuterWall.Value)
                {
                    outerWalls.Add(wall);
                }
            }
            return outerWalls;
        }

        public static bool isAllFanInletsAreOuterAirTerminals(Fan fan)
        {
            if (fan == null)
                throw new ArgumentException();
            //获得补风机的取风风口
            List<AirTerminal> inputAirTerminalOfFan = HVACFunction.GetInletsOfFan(fan);
            //依次遍历每一个取风口
            foreach (AirTerminal airTerminal in inputAirTerminalOfFan)
            {
                //如果取风风口不为室外风口
                if (!HVACFunction.isOuterAirTerminal(airTerminal))
                {
                    return false;
                }
            }
            return true;
        }

        public static double getTotalAirVolumeOfAirTerminals(List<AirTerminal>airTerminals)
        {
            if (airTerminals == null)
                throw new ArgumentException();
            double TotalAirVolume = 0;
            foreach(AirTerminal airTerminal in airTerminals)
            {
                TotalAirVolume += airTerminal.airFlowRate.Value;
            }
            return TotalAirVolume;
        }

       public static bool isAllAirTerminalInSameFloor(List<AirTerminal>airTerminals)
        {
            if (airTerminals == null)
                throw new ArgumentException();
            int m_iStoryNo = 0;
            if (airTerminals.Count > 0)
                m_iStoryNo = airTerminals[0].m_iStoryNo.Value;

            foreach(AirTerminal airTerminal in airTerminals)
            {
                if (!airTerminal.m_iStoryNo.Value.Equals(m_iStoryNo))
                    return false;
            }
            return true;
        }

        private static string[] CommonOfenStayRoomTypes = { "办公室", "会议室", "报告厅", "商场" };
        private static string[] PublicRooms = { "走廊", "走道", "楼梯间", "前室", "避难间" };
        private static double error = 0.00001;
    }
}
