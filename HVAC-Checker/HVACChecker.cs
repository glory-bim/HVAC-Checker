using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace HVAC_CheckEngine
{
    using Region = List<Room>;
    public static class HVACChecker
    {
        /**
     建筑设计防火规范 GB50016-2014：8.5.1条文：
     建筑的下列场所或部位应设置防烟设施：
     1 防烟楼梯间及其前室；
     2 消防电梯间前室或合用前室；
     3 避难走道的前室、避难层(间)。
     建筑高度不大于50m的公共建筑、厂房、仓库和建筑高度不大于100m的住宅建筑，当其防烟楼梯间的前室或合用前室符合下列条件之一时，楼梯间可不设置防烟系统：
     1 前室或合用前室采用敞开的阳台、凹廊；
     2 前室或合用前室具有不同朝向的可开启外窗，且可开启外窗的面积满足自然排烟口的面积要求。
     */
        
        public static BimReview GB50016_2014_8_5_1()
        {
            BimReview result = new BimReview("GB50016_2014", "8.5.1");

            //获得建筑中所有防烟楼梯间、前室及避难间的集合
            List<Room> rooms = new List<Room>();
            List<Room> rooms_temp=null;
            rooms_temp = HVACFunction.GetRooms("防烟楼梯间");
            if (rooms_temp!=null)
                rooms.AddRange(rooms_temp);

            rooms_temp = HVACFunction.GetRooms("前室");
            if(rooms_temp!=null)
                rooms.AddRange(rooms_temp);

            rooms_temp = HVACFunction.GetRooms("避难间");
            if (rooms_temp != null)
                rooms.AddRange(rooms_temp);

            //依次对以上房间进行如下判断：
            foreach (Room room in rooms)
            {
                //如果房间中没有正压送风系统，则在审查结果中标注审核不通过，并将当前房间信息加到违规构建列表中

                if(!isRoomHaveSomeSystem(room,"正压送风"))
                {
                    result.isPassCheck = false;
                    string remark = string.Empty;
                    if (((globalData.buildingType.Contains("公共建筑") || globalData.buildingType.Contains("厂房") || globalData.buildingType.Contains("仓库")) && globalData.buildingHeight <= 50) ||
                       globalData.buildingType.Contains("住宅") && globalData.buildingHeight <= 100)
                        if (room.type.Contains("楼梯间"))
                        {
                            remark = "此楼梯间需要专家复核";
                        }
                            result.AddViolationComponent(room.Id.Value, room.type, remark);
                }
            }
            //经过以上操作后，如果审查通过，则在审查结果中注明审查通过
            if (result.isPassCheck)
            {
                result.comment = "设计满足规范GB50016_2014中第8.5.1条条文规定。";
            }
            //                如果审查不通过，则在审查结果中注明审查未通过，并写明原因
            else
            {
                result.comment = build_GB50016_2014_8_5_1_ViolationComment(ref result);
            }
            return result;
        }
        private static string build_GB50016_2014_8_5_1_ViolationComment(ref BimReview result)
        {
            string comment = "设计不满足规范GB50016_2014中第8.5.1条条文规定。";
            
            foreach (ComponentAnnotation component in result.violationComponents)
            {
                    //如果有楼梯间则在审查结果批注中加入请专家复核提示
                if (component.remark.Contains("需要专家复核"))
               {
                        comment += "请专家复核：未设置防烟设施的楼梯间前室或合用前室是否采用敞开的阳台、凹廊，或者前室或合用前室是否具有不同朝向的可开启外窗，且可开启外窗的面积满足自然排烟口的面积要求。";
                        break;
               }
            }

            return comment;
        }
/*
        厂房或仓库的下列场所或部位应设置排烟设施：
    1 人员或可燃物较多的丙类生产场所，丙类厂房内建筑面积大于300m2且经常有人停留或可燃物较多的地上房间；
    2 建筑面积大于5000m2的丁类生产车间；
    3 占地面积大于1000m2的丙类仓库；
    4 高度大于32m的高层厂房(仓库)内长度大于20m的疏散走道，其他厂房(仓库)内长度大于40m的疏散走道。
    */

  
        public static BimReview GB50016_2014_8_5_2()
        {
            //对审查结果进行初始化
            BimReview result = new BimReview("GB50016_2014", "8.5.2");
            //如果建筑类型为厂房或仓库
            List<Room> rooms = new List<Room>();
            List<Room> rooms_temp = null;
            if (globalData.buildingType.Contains("厂房")|| globalData.buildingType.Contains("仓库"))
            {
                //  如果建筑类型为丙类厂房
                if(globalData.buildingType.Contains("丙类厂房"))
                {
                    //      获取所有面积大于300m2的地上房间，并放入房间集合中
                    rooms_temp = HVACFunction.GetRooms("", "", 300, RoomPosition.overground );
                    rooms.AddRange(rooms_temp);
                }

                //  获取所有面积大于5000㎡的丁类生产车间，并放入房间集合中
                rooms_temp = HVACFunction.GetRooms("丁类生产车间", "", 5000, RoomPosition.overground | RoomPosition.underground | RoomPosition.semi_underground);
                rooms.AddRange(rooms_temp);
                //  获取所有面积大于1000㎡的丙类仓库，并放入房间集合中
                rooms_temp = HVACFunction.GetRooms("丙类仓库", "", 1000, RoomPosition.overground|RoomPosition.underground|RoomPosition.semi_underground);
                rooms.AddRange(rooms_temp);

                //  如果建筑高度大于32m
                if (globalData.buildingHeight>32)
                {
                    //      则获得所有长度大于20m的疏散走道
                    rooms_temp = HVACFunction.GetRoomsMoreThan(20);
                    rooms.AddRange(rooms_temp);
                }
                //  如果建筑高度小于等于32m则获得所长度大于40m的疏散走道
                else
                {
                    rooms_temp = HVACFunction.GetRoomsMoreThan(40);
                    rooms.AddRange(rooms_temp);
                }


                //获取所有丙类生产场所，并放入房间集合中
                rooms_temp = HVACFunction.GetRooms("丙类生产");
                rooms.AddRange(rooms_temp);

                //  对房间集合中的所有房间进行如下操作

                foreach (Room room in rooms)
                {
                // 判断他们是否有排烟系统。
                 //  如果没有排烟系统，则在审查结果中记录审查不通过，并把当前房间ID加到审查结果中
                    if(!isRoomHaveSomeSystem(room,"排烟"))
                    {
                         result.isPassCheck = false;
                        string remark = string.Empty;
                        if(globalData.buildingType.Contains("丙类厂房")&&room.area>=300&&room.roomPosition==RoomPosition.overground&&room.type!="丁类生产车间"&&
                           room.type != "丙类仓库"&& room.type !="走廊" && room.type != "走道" && !isCommonOfenStayRoom(room))
                           remark = "此房间需专家核对是否为人员长期停留或可燃物较多";

                        if(room.type.Contains("丙类生产")&&!isCommonOfenStayRoom(room))
                           remark = "此房间需专家核对是否为人员长期停留或可燃物较多";

                        result.AddViolationComponent(room.Id.Value, room.type,remark);
                    }
                }
            }
            //如果审查通过
            //   则在审查结果批注中注明审查通过相关内容
            if (result.isPassCheck)
            {
                result.comment = "设计满足规范GB50016_2014中第8.5.2条条文规定。";
            }
            //如果审查不通过
            //   则在审查结果中注明审查不通过的相关内容
            else
            {
                result.comment = build_GB50016_2014_8_5_2_ViolationComment(ref result);
            }
            return result;
        }
        

        private static string build_GB50016_2014_8_5_2_ViolationComment(ref BimReview result)
        {
            string comment = "设计不满足规范GB50016_2014中第8.5.2条条文规定。";
          
            foreach (ComponentAnnotation component in result.violationComponents)
            {
               if (component.remark.Contains("需专家核对"))
               {
                   comment += "请专家复核：相关违规房间是否人员长期停留或可燃物较多";
                   break;
               }
            }
            return comment;
        }


        //民用建筑的下列场所或部位应设置排烟设施：
        //1 设置在一、二、三层且房间建筑面积大于100m2的歌舞娱乐放映游艺场所，设置在四层及以上楼层、地下或半地下的歌舞娱乐放映游艺场所；
        //2 中庭；
        //3 公共建筑内建筑面积大于100m2且经常有人停留的地上房间；
        //4 公共建筑内建筑面积大于300m2且可燃物较多的地上房间；
        //5 建筑内长度大于20m的疏散走道。

        public static BimReview GB50016_2014_8_5_3()
        {
            //将审查结果初始化
            BimReview result = new BimReview("GB50016_2014", "8.5.3");

            List<Room> Rooms=new List<Room>();
            List<Room> between100And300sqmOvergroundCommonRooms = new List<Room>();
            List<Room> greaterThan300sqmOvergroundCommonRooms = new List<Room>();
            //如果建筑类型为公共建筑或住宅
            if (globalData.buildingType=="公共建筑"||globalData.buildingType=="住宅")
            { 
                //    获得所有地上面积大于100平米的地上歌舞娱乐游艺场所的房间集合overgroundEntertainmentRooms
                List<Room> greaterThan100sqmOvergroundEntertainmentRooms = HVACFunction.GetRooms("歌舞娱乐放映游艺场所", "", 100, RoomPosition.overground);
                //    从greaterThan100sqmOvergroundEntertainmentRooms集合中筛选出位于1~3层的房间greaterThan100sqmF1_3FloorEntertainmentRooms，并将这些房间加入到房间结合中房间集合Rooms中
                List<Room> greaterThan100sqmF1_3FloorEntertainmentRooms = filtrateRoomsBetweenFloor_aAndFloor_b(greaterThan100sqmOvergroundEntertainmentRooms, 1, 3);
                Rooms.AddRange(greaterThan100sqmF1_3FloorEntertainmentRooms);

                //    获得所有歌舞娱乐游艺娱乐场所的房间集合entertainmentRooms。 
                List<Room> entertainmentRooms = HVACFunction.GetRooms("歌舞娱乐放映游艺场所");
                //    从entertainmentRooms中筛选出1到三层的所有歌舞娱乐游艺场所的房间集合F1_3FloorEntertainmentRooms
                List<Room> F1_3FloorEntertainmentRooms = filtrateRoomsBetweenFloor_aAndFloor_b(entertainmentRooms, 1, 3);
                //    从entertainmentRooms集合中减去F1_3FloorEntertainmentRooms集合获得一个otherEntertainmentRooms。并将这个集合加入到房间集合Rooms中
  
                List<Room>  otherEntertainmentRooms =entertainmentRooms.exceptSameRooms(F1_3FloorEntertainmentRooms);

                Rooms.AddRange(otherEntertainmentRooms);

                //    获得所有中庭房间，并加入到房间集合Rooms中
                List<Room> courtyards = HVACFunction.GetRooms("中庭");
                Rooms.AddRange(courtyards);

                //    获得长度大于20m的疏散走道集合，并放入房间集合Rooms中
                List<Room> corridorsMoreThan20m = HVACFunction.GetRoomsMoreThan(20);
                Rooms.AddRange(corridorsMoreThan20m);
            }
            //如果建筑类型为公共建筑
            if (globalData.buildingType=="公共建筑")
            {
                //    获得所有建筑面积大于100的地上房间的集合greaterThan100sqmOvergroundRooms
                List<Room>greaterThan100sqmOvergroundRooms = HVACFunction.GetRooms("", "", 100, RoomPosition.overground);
                //     从greaterThan100sqmOvergroundRooms需要除去Rooms中的房间，获得大于100地上普通房间集合greaterThan100sqmOvergroundCommonRooms
            
                List<Room> greaterThan100sqmOvergroundCommonRooms=greaterThan100sqmOvergroundRooms.exceptSameRooms(Rooms);
                //    获得所有建筑面积大于300的地上房间的集合greaterThan300sqmOvergroundRooms
                List<Room> greaterThan300sqmOvergroundRooms = HVACFunction.GetRooms("", "", 300, RoomPosition.overground);
                //    从greaterThan300sqmOvergroundRooms需要除去Rooms中的房间，获得大于300地上普通房间集合greaterThan300sqmOvergroundCommonRooms
                greaterThan300sqmOvergroundCommonRooms=greaterThan300sqmOvergroundRooms.exceptSameRooms(Rooms);
                //    将集合greaterThan100sqmOvergroundRooms减去集合greaterThan300sqmOvergroundRooms
                //    获得所有建筑面积大于100㎡且小于等于300㎡的地上普通房间的集合between100And300sqmOvergroundRooms
                between100And300sqmOvergroundCommonRooms=greaterThan100sqmOvergroundCommonRooms.exceptSameRooms(greaterThan300sqmOvergroundCommonRooms); 
            }
            //    依次判定房间集合Rooms中的房间是否有排烟设施，如果没有则在审查记录中标记审查不通过，并将违规构件加入到审查结果中
            foreach(Room room in Rooms)
            {
                if (!isRoomHaveSomeSystem(room, "排烟"))
                {
                    result.isPassCheck = false;
                    string remark = string.Empty;
                    result.AddViolationComponent(room.Id.Value, room.type, remark);
                }
            }
            //    依次判定房间集合between100And300sqmOvergroundRooms中的房间是否有排烟设施，如果没有则在审查结果中标记审查不通过，并在违规构件的备注中记录需要专家复核此房间是否人员经常停留
            //      并将违规构件加入到审查结果中
            foreach(Room room in between100And300sqmOvergroundCommonRooms)
            {
                if (!isRoomHaveSomeSystem(room, "排烟"))
                {
                    result.isPassCheck = false;
                    string remark = string.Empty;
                    if (!isCommonOfenStayRoom(room))
                        remark = "需专家复核此房间是否人员经常停留";
                    result.AddViolationComponent(room.Id.Value, room.type, remark);
                }
            }
            //    依次判定房间集合greaterThan300sqmOvergroundRooms中的房间是否有排烟设施，如果没有则在审查结果中标记审查不通过，并在违规构件的备注中记录需要转件复核此房间是否人员经常停留
            //     或可燃物较多并将违规构件加入到审查结果中
            foreach(Room room in greaterThan300sqmOvergroundCommonRooms)
            {
                if (!isRoomHaveSomeSystem(room, "排烟"))
                {
                    result.isPassCheck = false;
                    string remark = string.Empty;
                    if(!isCommonOfenStayRoom(room))
                        remark = "需专家复核此房间是否人员经常停留或可燃物较多";
                    result.AddViolationComponent(room.Id.Value, room.type, remark);
                }
            }
            //如果审查通过
            //则在审查结果批注中注明审查通过相关内容
            if(result.isPassCheck)
            {
                result.comment = "设计满足规范GB50016_2014中第8.5.3条条文规定。";
            }
            //如果审查不通过
            //则在审查结果中注明审查不通过的相关内容
            else 
            {
                result.comment = build_GB50016_2014_8_5_3_ViolationComment(ref result);   
            }
            return result;
        }


        private static string build_GB50016_2014_8_5_3_ViolationComment(ref BimReview result)
        {
            string comment = "设计不满足规范GB50016_2014中第8.5.3条条文规定。";

            foreach (ComponentAnnotation component in result.violationComponents)
            {
                if (component.remark=="需专家复核此房间是否人员经常停留或可燃物较多")
                {
                    comment += "请专家复核：相关违规房间是否人员长期停留或可燃物较多";
                    return comment;
                }
            }

            foreach (ComponentAnnotation component in result.violationComponents)
            {
                if (component.remark=="需专家复核此房间是否人员经常停留")
                {
                    comment += "请专家复核：相关违规房间是否人员长期停留";
                    return comment;
                }
            }
            return comment;
        }

        //地下或半地下建筑(室)、地上建筑内的无窗房间，当总建筑面积大于200m2或一个房间建筑面积大于50m2，
        //且经常有人停留或可燃物较多时，应设置排烟设施。

        //获得所有地下及半地下大于50㎡房间的集合undergroundLargerThan50sqmRooms
        //获得所有地上大于50㎡房间的集合overgroundLargerThan50sqmRooms
        //从overgroundLargerThan50sqmRooms集合中筛选出所有的无窗房间，overgroundLargerThan50sqmWindowlessRooms
        //获得所有连通区域的集合regions。
        //筛选出地下或半地下房间总面积大于200㎡以及地上无窗房间总面积大于200㎡的区域集合needSmokeExhaustRegions
        //依次判断undergroundLargerThan50sqmRooms集合中的房间是否设置了排烟系统，如果房间没有设置排烟系统则将房间加到审查结果中
        //依次判断overgroundLargerThan50sqmWindowlessRooms集合中的房间是否设置了排烟系统，如果房间没有设置排烟系统则将房间加到审查结果中
        //依次判断needSmokeExhaustRegions集合中每个区域是否设置了排烟系统，如果没有设置排烟系统，则将这些房间加入到审查结果中
        //如果审查通过
        //则在审查结果批注中注明审查通过相关内容
        //如果审查不通过
        //则在审查结果中注明审查不通过的相关内容
        /*
        public static BimReview GB50016_2014_8_5_4()
        {

        }
        *>
        private static bool isRoomHaveSomeSystem(Room room, string systemName)
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
            //如果房间中没有某种系统类型的风口
            else
            {
                //      如果房间中没有可开启外窗，则返回否
                List<Windows> windows = HVACFunction.GetWindowsInRoom(room);
                Windows aimWindow = assistantFunctions.GetOpenableOuterWindow(windows);
                if (aimWindow == null)
                {
                    return false;
                }
                //如果有可开启外窗则返回是
                else
                    return true;
            }
        }

        private static List<Room> filtrateRoomsBetweenFloor_aAndFloor_b(List<Room>rooms, int floor_a,int floor_b)
        {
            List<Room> aimRooms = new List<Room>();
            foreach(Room room in rooms)
            {
                if (room.storyNo >= floor_a && room.storyNo <= floor_b)
                    aimRooms.Add(room);
            }
            return aimRooms;
        }

        public  static List<Room> exceptSameRooms(this List<Room> rooms,List<Room> exceptedRooms)
        {
            List<Room> rooms_copy = new List<Room>();
            rooms_copy.AddRange(rooms);
            foreach(Room room in exceptedRooms)
            {
                Room aimRoom= rooms_copy.findRoom(room);
                rooms_copy.Remove(aimRoom);
            }
            return rooms_copy;
        }

        public static Room findRoom(this List<Room>rooms,Room aimRoom)
        {
            foreach(Room room in rooms)
            {
                if (room.Id == aimRoom.Id)
                    return room;
            }
            return null;
        }

        private static string[]CommonOfenStayRoomTypes = { "办公室", "会议室", "报告厅", "商场" };

        private static bool isCommonOfenStayRoom(Room room)
        {
            List<string> commonOfenStayRoomTypes = new List<string>(CommonOfenStayRoomTypes);

            return commonOfenStayRoomTypes.Exists(type => type == room.type);

        }

        
        private static List<Room>getAllWindowlessRooms(List<Room> rooms)
        {
            List<Room> windowlessRooms = new List<Room>();
            foreach(Room room in rooms)
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
        /*
        private static List<Region> getNeedSmokeExhaustRegions(List<Region> regions)
        {
            List<Region> needSmokeExhaustRegions = new List<Region>();
            //依次遍历区域集合中的每一个区域
            foreach(Region region in regions)
            {
                if(regions.First.)
            }
        }
        */

    }

}
