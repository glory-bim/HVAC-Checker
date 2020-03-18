using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace HVAC_CheckEngine
{

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
            List<Room> rooms_temp = null;
      
            rooms_temp = HVACFunction.GetRooms("防烟楼梯间");
            if (rooms_temp != null)
                rooms.AddRange(rooms_temp);

            rooms_temp = HVACFunction.GetRooms("前室");
            if (rooms_temp != null)
                rooms.AddRange(rooms_temp);

            rooms_temp = HVACFunction.GetRooms("避难间");
            if (rooms_temp != null)
                rooms.AddRange(rooms_temp);

            //依次对以上房间进行如下判断：
            foreach (Room room in rooms)
            {
                //如果房间中没有正压送风系统，则在审查结果中标注审核不通过，并将当前房间信息加到违规构建列表中

                if (!assistantFunctions.isRoomHaveSomeSystem(room, "正压送风"))
                {
                    result.isPassCheck = false;
                    string remark = string.Empty;
                    if (((globalData.buildingType.Contains("公共建筑") || globalData.buildingType.Contains("厂房") || globalData.buildingType.Contains("仓库")) && globalData.buildingHeight <= 50) ||
                       globalData.buildingType.Contains("住宅") && globalData.buildingHeight <= 100)
                       //如果违规房间为防烟楼梯间则在此构件中备注需要专家审核
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
            List<Room> rooms_Class_C_productPlant = new List<Room>();
            List<Room> rooms_needSpecialRemark = new List<Room>();

            List<Room> rooms_temp = null;
            if (globalData.buildingType.Contains("厂房") || globalData.buildingType.Contains("仓库"))
            {

                //  获取所有面积大于5000㎡的丁类生产车间，并放入房间集合中
                rooms_temp = HVACFunction.GetRooms("丁类生产车间", "", 5000, RoomPosition.overground | RoomPosition.underground | RoomPosition.semi_underground);
                rooms.AddRange(rooms_temp);
                //  获取所有面积大于1000㎡的丙类仓库，并放入房间集合中
                rooms_temp = HVACFunction.GetRooms("丙类仓库", "", 1000, RoomPosition.overground | RoomPosition.underground | RoomPosition.semi_underground);
                rooms.AddRange(rooms_temp);

                //  如果建筑高度大于32m
                if (globalData.buildingHeight > 32)
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


                //获取所有丙类生产场所，并放入房间集合中 rooms_Class_C_productPlant
                rooms_Class_C_productPlant = HVACFunction.GetRooms("丙类生产");


                
                //  如果建筑类型为丙类厂房
                if (globalData.buildingType.Contains("丙类厂房"))
                {
                    //      获取所有面积大于300m2的地上房间rooms_temp
                    rooms_temp = HVACFunction.GetRooms("", "", 300, RoomPosition.overground);
                    //为避免重复，从rooms_temp中除去已加入rooms中的房间,以及 rooms_Class_C_productPlant中的房间
                    rooms_temp = rooms_temp.exceptSameItems(rooms);
                    rooms_temp = rooms_temp.exceptSameItems(rooms_Class_C_productPlant);
                    //从rooms_temp除去公共房间
                    rooms_temp = rooms_temp.exceptPublicRooms();
                    //将rooms_temp集合中的房间加入到rooms_needSpecialRemark集合中
                    rooms_needSpecialRemark = rooms_temp;
                }
                //  对房间集合rooms中的所有房间进行如下操作
                foreach (Room room in rooms)
                {
                    // 判断他们是否有排烟系统。
                    //  如果没有排烟系统，则在审查结果中记录审查不通过，并把当前房间ID加到审查结果中
                    if (!assistantFunctions.isRoomHaveSomeSystem(room, "排烟"))
                    {
                        result.isPassCheck = false;
                        string remark = string.Empty;

                        result.AddViolationComponent(room.Id.Value, room.type, remark);
                    }
                }
                //  对房间集合rooms_Class_C_productPlant中的所有房间进行如下操作
                foreach (Room room in rooms_Class_C_productPlant)
                {
                    // 判断他们是否有排烟系统。
                    //  如果没有排烟系统，则在审查结果中记录审查不通过，并把当前房间ID加到审查结果中，并备注当前房间需要专家复审
                    if (!assistantFunctions.isRoomHaveSomeSystem(room, "排烟"))
                    {
                        result.isPassCheck = false;
                        string remark = string.Empty;
                        remark = "此房间需专家核对是否人员或可燃物较多";
                        result.AddViolationComponent(room.Id.Value, room.type, remark);
                    }
                }
                //  对房间集合rooms_needSpecialRemark中的所有房间进行如下操作
                foreach (Room room in rooms_needSpecialRemark)
                {
                    // 判断他们是否有排烟系统。
                    //  如果没有排烟系统，则在审查结果中记录审查不通过，并把当前房间ID加到审查结果中
                    if (!assistantFunctions.isRoomHaveSomeSystem(room, "排烟"))
                    {
                        result.isPassCheck = false;
                        string remark = string.Empty;
                        remark = "此房间需专家核对是否经常有人停留或可燃物较多";
                        result.AddViolationComponent(room.Id.Value, room.type, remark);
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
                    comment += "请专家复核：相关违规房间是否人员长期停留或人员、可燃物较多";
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

            List<Room> Rooms = new List<Room>();
            List<Room> between100And300sqmOvergroundCommonRooms = new List<Room>();
            List<Room> greaterThan300sqmOvergroundCommonRooms = new List<Room>();
            //如果建筑类型为公共建筑或住宅
            if (globalData.buildingType == "公共建筑" || globalData.buildingType == "住宅")
            {
                //    获得所有地上面积大于100平米的地上歌舞娱乐游艺场所的房间集合overgroundEntertainmentRooms
                List<Room> greaterThan100sqmOvergroundEntertainmentRooms = HVACFunction.GetRooms("歌舞娱乐放映游艺场所", "", 100, RoomPosition.overground);
                //    从greaterThan100sqmOvergroundEntertainmentRooms集合中筛选出位于1~3层的房间greaterThan100sqmF1_3FloorEntertainmentRooms，并将这些房间加入到房间结合中房间集合Rooms中
                List<Room> greaterThan100sqmF1_3FloorEntertainmentRooms = assistantFunctions.filtrateElementsBetweenFloor_aAndFloor_b(greaterThan100sqmOvergroundEntertainmentRooms, 1, 3);
                Rooms.AddRange(greaterThan100sqmF1_3FloorEntertainmentRooms);

                //    获得所有歌舞娱乐游艺娱乐场所的房间集合entertainmentRooms。 
                List<Room> entertainmentRooms = HVACFunction.GetRooms("歌舞娱乐放映游艺场所");
                //    从entertainmentRooms中筛选出1到三层的所有歌舞娱乐游艺场所的房间集合F1_3FloorEntertainmentRooms
                List<Room> F1_3FloorEntertainmentRooms = assistantFunctions.filtrateElementsBetweenFloor_aAndFloor_b(entertainmentRooms, 1, 3);
                //    从entertainmentRooms集合中减去F1_3FloorEntertainmentRooms集合获得一个otherEntertainmentRooms。并将这个集合加入到房间集合Rooms中

                List<Room> otherEntertainmentRooms = entertainmentRooms.exceptSameItems(F1_3FloorEntertainmentRooms);

                Rooms.AddRange(otherEntertainmentRooms);

                //    获得所有中庭房间，并加入到房间集合Rooms中
                List<Room> courtyards = HVACFunction.GetRooms("中庭");
                Rooms.AddRange(courtyards);

                //    获得长度大于20m的疏散走道集合，并放入房间集合Rooms中
                List<Room> corridorsMoreThan20m = HVACFunction.GetRoomsMoreThan(20);
                Rooms.AddRange(corridorsMoreThan20m);
            }
            //如果建筑类型为公共建筑
            if (globalData.buildingType == "公共建筑")
            {
                //    获得所有建筑面积大于100的地上房间的集合greaterThan100sqmOvergroundRooms
                List<Room> greaterThan100sqmOvergroundRooms = HVACFunction.GetRooms("", "", 100, RoomPosition.overground);
                //     从greaterThan100sqmOvergroundRooms需要除去Rooms中的房间，获得大于100地上普通房间集合greaterThan100sqmOvergroundCommonRooms

                List<Room> greaterThan100sqmOvergroundCommonRooms = greaterThan100sqmOvergroundRooms.exceptSameItems(Rooms);
                //    获得所有建筑面积大于300的地上房间的集合greaterThan300sqmOvergroundRooms
                List<Room> greaterThan300sqmOvergroundRooms = HVACFunction.GetRooms("", "", 300, RoomPosition.overground);
                //    从greaterThan300sqmOvergroundRooms需要除去Rooms中的房间，获得大于300地上普通房间集合greaterThan300sqmOvergroundCommonRooms
                greaterThan300sqmOvergroundCommonRooms = greaterThan300sqmOvergroundRooms.exceptSameItems(Rooms);
                //    将集合greaterThan100sqmOvergroundRooms减去集合greaterThan300sqmOvergroundRooms
                //    获得所有建筑面积大于100㎡且小于等于300㎡的地上普通房间的集合between100And300sqmOvergroundRooms
                between100And300sqmOvergroundCommonRooms = greaterThan100sqmOvergroundCommonRooms.exceptSameItems(greaterThan300sqmOvergroundCommonRooms);
            }
            //    依次判定房间集合Rooms中的房间是否有排烟设施，如果没有则在审查记录中标记审查不通过，并将违规构件加入到审查结果中
            foreach (Room room in Rooms)
            {
                if (!assistantFunctions.isRoomHaveSomeSystem(room, "排烟"))
                {
                    result.isPassCheck = false;
                    string remark = string.Empty;
                    result.AddViolationComponent(room.Id.Value, room.type, remark);
                }
            }
            //    依次判定房间集合between100And300sqmOvergroundRooms中的房间是否有排烟设施，如果没有则在审查结果中标记审查不通过，并在违规构件的备注中记录需要专家复核此房间是否人员经常停留
            //      并将违规构件加入到审查结果中
            foreach (Room room in between100And300sqmOvergroundCommonRooms)
            {
                if (!assistantFunctions.isRoomHaveSomeSystem(room, "排烟"))
                {
                    result.isPassCheck = false;
                    string remark = string.Empty;
                    if (!assistantFunctions.isCommonOfenStayRoom(room))
                        remark = "需专家复核此房间是否人员经常停留";
                    result.AddViolationComponent(room.Id.Value, room.type, remark);
                }
            }
            //    依次判定房间集合greaterThan300sqmOvergroundRooms中的房间是否有排烟设施，如果没有则在审查结果中标记审查不通过，并在违规构件的备注中记录需要转件复核此房间是否人员经常停留
            //     或可燃物较多并将违规构件加入到审查结果中
            foreach (Room room in greaterThan300sqmOvergroundCommonRooms)
            {
                if (!assistantFunctions.isRoomHaveSomeSystem(room, "排烟"))
                {
                    result.isPassCheck = false;
                    string remark = string.Empty;
                    if (!assistantFunctions.isCommonOfenStayRoom(room))
                        remark = "需专家复核此房间是否人员经常停留或可燃物较多";
                    result.AddViolationComponent(room.Id.Value, room.type, remark);
                }
            }
            //如果审查通过
            //则在审查结果批注中注明审查通过相关内容
            if (result.isPassCheck)
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
                if (component.remark == "需专家复核此房间是否人员经常停留或可燃物较多")
                {
                    comment += "请专家复核：相关违规房间是否人员长期停留或可燃物较多";
                    return comment;
                }
            }

            foreach (ComponentAnnotation component in result.violationComponents)
            {
                if (component.remark == "需专家复核此房间是否人员经常停留")
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
        //除去集合undergroundLargerThan50sqmRooms中的公共区域（走廊，楼梯间等）
        //获得所有地上大于50㎡房间的集合overgroundLargerThan50sqmRooms
        //从overgroundLargerThan50sqmRooms集合中筛选出所有的无窗房间，overgroundLargerThan50sqmWindowlessRooms
        //除去集合overgroundLargerThan50sqmWindowlessRooms中的公共区域（走廊，楼梯间等）

        //获得所有连通区域的集合connectedRegions。
        //从集合connectedRegions中筛选出所有需要排烟的区域的集合needSmokeExhaustRegions
        //依次判断undergroundLargerThan50sqmRooms集合中的房间是否设置了排烟系统，如果房间没有设置排烟系统则将房间加到审查结果中
        //依次判断overgroundLargerThan50sqmWindowlessRooms集合中的房间是否设置了排烟系统，如果房间没有设置排烟系统则将房间加到审查结果中
        //依次判断needSmokeExhaustRegions集合中每个区域是否设置了排烟系统，如果没有设置排烟系统，则将这些房间加入到审查结果中
        //如果审查通过
        //则在审查结果批注中注明审查通过相关内容
        //如果审查不通过
        //则在审查结果中注明审查不通过的相关内容

        public static BimReview GB50016_2014_8_5_4()
        {
            //将审查结果初始化
            BimReview result = new BimReview("GB50016_2014", "8.5.4");

            //获得所有地下及半地下大于50㎡房间的集合undergroundLargerThan50sqmRooms
            List<Room> undergroundLargerThan50sqmRooms = HVACFunction.GetRooms("", "", 50, RoomPosition.semi_underground | RoomPosition.underground);
            //除去集合undergroundLargerThan50sqmRooms中的公共区域（走廊，楼梯间等）
            undergroundLargerThan50sqmRooms = undergroundLargerThan50sqmRooms.exceptPublicRooms();
            //获得所有地上大于50㎡房间的集合overgroundLargerThan50sqmRooms
            List<Room> overgroundLargerThan50sqmRooms = HVACFunction.GetRooms("", "", 50, RoomPosition.overground);
            //从overgroundLargerThan50sqmRooms集合中筛选出所有的无窗房间，overgroundLargerThan50sqmWindowlessRooms
            List<Room> overgroundLargerThan50sqmWindowlessRooms = assistantFunctions.getAllWindowlessRooms(overgroundLargerThan50sqmRooms);
            //除去集合overgroundLargerThan50sqmWindowlessRooms中的公共区域（走廊，楼梯间等）
            overgroundLargerThan50sqmWindowlessRooms = overgroundLargerThan50sqmWindowlessRooms.exceptPublicRooms();

            //获得所有连通区域的集合regions。
            List<Region> connectedRegions = HVACFunction.GetConnectedRegion();
            //集合needSmokeExhaustRegions中
            List<Region> needSmokeExhaustRegions = new List<Region>();
            needSmokeExhaustRegions = assistantFunctions.filtrateNeedSmokeExhaustRegions(connectedRegions);

            //依次判断undergroundLargerThan50sqmRooms集合中的房间是否设置了排烟系统，
            //如果房间没有设置排烟系统则将房间加到审查结果中
            foreach (Room room in undergroundLargerThan50sqmRooms)
            {
                if (!assistantFunctions.isRoomHaveSomeSystem(room, "排烟"))
                {
                    result.isPassCheck = false;
                    string remark = string.Empty;
                    remark = "需专家复核此房间是否人员经常停留或可燃物较多";
                    result.AddViolationComponent(room.Id.Value, room.type, remark);
                }
            }
            //依次判断overgroundLargerThan50sqmWindowlessRooms集合中的房间是否设置了排烟系统，
            //如果房间没有设置排烟系统则将房间加到审查结果中
            foreach (Room room in overgroundLargerThan50sqmWindowlessRooms)
            {
                if (!assistantFunctions.isRoomHaveSomeSystem(room, "排烟"))
                {
                    result.isPassCheck = false;
                    string remark = string.Empty;
                    remark = "需专家复核此房间是否人员经常停留或可燃物较多";
                    result.AddViolationComponent(room.Id.Value, room.type, remark);
                }
            }
            //依次判断needSmokeExhaustRegions集合中每个区域是否设置了排烟系统，如果没有设置排烟系统，则将违规房间加入到审查结果中
            //如果审查通过
            foreach (Region region in needSmokeExhaustRegions)
            {
                if (!assistantFunctions.isRegionHaveSomeSystem(region, "排烟"))
                {
                    List<Room> violateRooms = region.rooms;
                    violateRooms = violateRooms.exceptPublicRooms();
                    foreach (Room room in violateRooms)
                    {
                        if (!assistantFunctions.isRoomHaveSomeSystem(room, "排烟") && !assistantFunctions.isViolateRoomAlreadyInResult(room, result))
                        {
                            result.isPassCheck = false;
                            string remark = string.Empty;
                            remark = "需专家复核此房间是否人员经常停留或可燃物较多";
                            result.AddViolationComponent(room.Id.Value, room.type, remark);
                        }
                    }
                }
            }
            //如果审查通过
            //则在审查结果批注中注明审查通过相关内容
            if (result.isPassCheck)
            {
                result.comment = "设计满足规范GB50016_2014中第8.5.4条条文规定。";
            }
            //如果审查不通过
            //则在审查结果中注明审查不通过的相关内容
            else
            {
                result.comment = "设计不满足规范GB50016_2014中第8.5.4条条文规定。请专家复核：相关违规房间是否人员长期停留或可燃物较多";
            }
            return result;
        }


        //建筑高度大于50m的公共建筑、工业建筑和建筑高度大于100m的住宅建筑，其防烟楼梯间、独立前室、
        //共用前室、合用前室及消防电梯前室应采用机械加压送风系统。

        //初始化审查结果
        //如果建筑类型为公共建筑、工业建筑且建筑高度大于50m或者建筑类型为住宅且建筑高度大于100m
        //  则获得建筑中所有防烟楼梯间及前室的集合rooms
        //  依次判断集合rooms中的房间是否使用了机械加压送风系统
        //  如果没有设置机械加压送风系统，则在审查结果中标记审查不通过
        //如果审查通过
        //则在审查结果批注中注明审查通过相关内容
        //如果审查不通过
        //则在审查结果批注中注明审查不通过相关内容
        public static BimReview GB51251_2017_3_1_2()
        {
            //初始化审查结果
            BimReview result = new BimReview("GB51251_2017", "3.1.2");
            bool isPublicBuildingHeigherThan50m = globalData.buildingType.Contains("公共建筑") &&
                globalData.buildingHeight > 50;
            bool isIndustrialBuildingHeigherThan50m = globalData.buildingType.Contains("工业") &&
                globalData.buildingHeight > 50;
            bool isResidenceBuildingHeigherThan100m = globalData.buildingType.Contains("住宅") &&
                globalData.buildingHeight > 100;

            List<Room> rooms = new List<Room>();
            //如果建筑类型为公共建筑、工业建筑且建筑高度大于50m或者建筑类型为住宅且建筑高度大于100m
            if (isPublicBuildingHeigherThan50m || isIndustrialBuildingHeigherThan50m || isResidenceBuildingHeigherThan100m)
            {
                //  则获得建筑中所有防烟楼梯间及前室的集合rooms
                List<Room> temp_rooms = HVACFunction.GetRooms("防烟楼梯间");
                rooms.AddRange(temp_rooms);
                temp_rooms = HVACFunction.GetRooms("前室");
                rooms.AddRange(temp_rooms);

                //  依次判断集合rooms中的房间是否使用了机械加压送风系统
                foreach (Room room in rooms)
                {
                    //  如果没有设置机械加压送风系统，则在审查结果中标记审查不通过
                    if (!assistantFunctions.isRoomHaveSomeMechanicalSystem(room, "加压送风"))
                    {
                        result.isPassCheck = false;
                        result.AddViolationComponent(room.Id.Value, room.type, "");
                    }    
                }             
            }
            //如果审查通过
            //则在审查结果批注中注明审查通过相关内容
            if (result.isPassCheck)
            {
                result.comment = "设计满足规范GB51251_2017中第3.1.2条条文规定。";
            }
            //如果审查不通过
            //则在审查结果中注明审查不通过的相关内容
            else
            {
                result.comment = "设计不满足规范GB51251_2017中第3.1.2条条文规定。";
            }
            return result;
        }
        //防烟楼梯间及其前室的机械加压送风系统的设置应符合下列规定：
        //1 建筑高度小于或等于50m的公共建筑、工业建筑和建筑高度小于或等于100m的住宅建筑，当采用独立前室且其仅有一个门与走道或房间相通时，
        //可仅在楼梯间设置机械加压送风系统；当独立前室有多个门时，楼梯间、独立前室应分别独立设置机械加压送风系统。
        //2 当采用合用前室时，楼梯间、合用前室应分别独立设置机械加压送风系统。
        //3 当采用剪刀楼梯时，其两个楼梯间及其前室的机械加压送风系统应分别独立设置。

        //初始化审查结果
        //获取所有防烟楼梯间集合staircases
        //依次遍历每个防烟楼梯间
        //  如果楼梯间采用了机械加压送风系统且机械加压送风系统未设置独立（加压送风系统送风口处于前室或其他楼梯间）
        //     则将审查结果标记为不通过，且把当前楼梯间记录进审查结果中。
        //  找到此楼梯间的所有前室atrias
        //  依次遍历每一个前室
        //     如果建筑类型为公共建筑或工业建筑且建筑高度大于50m或建筑类型为住宅且建筑高度大于100m
        //     或楼梯间没有设置机械加压送风系统或前室不为独立前室或独立前室通向走廊的
        //     门多于一个
        //     如果前室没有设置加压系统
        //         则将审查结果标记为不通过，且把当前室记录进审查结果中。
        //     如果前室设置了机械加压送风系统且机械加压送风系统未独立设置（加压送风口处于楼梯间中）
        //         则将审查结果标记为不通过，且把当前室记录进审查结果中。
        //如果审查通过
        //则在审查结果批注中注明审查通过相关内容
        //如果审查不通过
        //则在审查结果中注明审查不通过的相关内容

        public static BimReview GB51251_2017_3_1_5()
        {
            //将审查结果初始化
            BimReview result = new BimReview("GB51251_2017", "3.1.5");


            //获取所有防烟楼梯间集合staircases
            //依次遍历每个防烟楼梯间
            List<Room> staircases = new List<Room>();
            staircases = HVACFunction.GetRooms("防烟楼梯间");
            //     找到此楼梯间的所有前室atrias
            foreach (Room stairCase in staircases)
            {
                bool stairCaseHaveMechanicalPressureSystem = assistantFunctions.isRoomHaveSomeMechanicalSystem(stairCase, "加压送风");
                bool stairCaseMechanicalPressureSystemIsIndependent = false;
                if (stairCaseHaveMechanicalPressureSystem)
                {
                    stairCaseMechanicalPressureSystemIsIndependent = assistantFunctions.isStairPressureAirSystemIndependent(stairCase);
                }
               
                //  如果楼梯间采用了机械加压送风系统且机械加压送风系统未设置独立
                if (stairCaseHaveMechanicalPressureSystem && !stairCaseMechanicalPressureSystemIsIndependent)
                {
                    //则将审查结果标记为不通过，且把当前楼梯间记录进审查结果中。
                    result.isPassCheck = false;
                    string remark = string.Empty;
                    result.AddViolationComponent(stairCase.Id.Value, stairCase.type, remark);
                }
                //     找到此楼梯间的所有前室atrias
                List<Room> atriasLinkToStairCase = HVACFunction.getConnectedRooms(stairCase);
                //     依次遍历每一个前室
                foreach (Room atria in atriasLinkToStairCase)
                {
                   bool atriaHaveMechanicalPressureSystem = assistantFunctions.isRoomHaveSomeMechanicalSystem(atria, "加压送风");
                   bool atriaPressureAirSystemIsIndependent = false;
                   if (atriaHaveMechanicalPressureSystem)
                       atriaPressureAirSystemIsIndependent = assistantFunctions.isAtriaPressureAirSystemIndependent(atria);
                   bool atriaIsIndependent = atria.type == "独立前室";
                   int numberOfDoorsToCorridor = assistantFunctions.getDoorsToCorridorOfAtria(atria).Count;
                    //     如果建筑类型为公共建筑或工业建筑且建筑高度大于50m或建筑类型为住宅且建筑高度大于100m
                    //     或楼梯间没有设置机械加压送风系统或前室不为独立前室或独立前室通向走廊的
                    //     门多于一个
                    if ((globalData.buildingType == "公共建筑" || globalData.buildingType == "工业建筑") && globalData.buildingHeight > 50 ||
                        (globalData.buildingType == "住宅" && globalData.buildingHeight > 100) || !stairCaseHaveMechanicalPressureSystem
                        || !atriaIsIndependent || numberOfDoorsToCorridor > 1)
                    {
                        //     如果前室没有设置加压系统
                        if (!atriaHaveMechanicalPressureSystem)
                        {
                            //       则将审查结果标记为不通过，且把当前楼梯间记录进审查结果中。
                            result.isPassCheck = false;
                            string remark = string.Empty;
                            result.AddViolationComponent(atria.Id.Value, atria.type, remark);
                            continue;
                        }
                    }
                    //     如果前室设置了机械加压送风系统且机械加压送风系统未独立设置（加压送风口处于楼梯间中）
                    //         则将审查结果标记为不通过，且把当前室记录进审查结果中。
                   if(atriaHaveMechanicalPressureSystem&&!atriaPressureAirSystemIsIndependent)
                   {
                        result.isPassCheck = false;
                        string remark = string.Empty;
                        result.AddViolationComponent(atria.Id.Value, atria.type, remark);
                   }

                }
            }

            //如果审查通过
            //则在审查结果批注中注明审查通过相关内容
            if (result.isPassCheck)
            {
                result.comment = "设计满足规范GB51251_2017中第3.1.5条条文规定。";
            }
            //如果审查不通过
            //则在审查结果中注明审查不通过的相关内容
            else
            {
                result.comment = "设计不满足规范GB51251_2017中第3.1.5条条文规定。";
            }
            return result;
        }

        //采用自然通风方式的封闭楼梯间、防烟楼梯间，应在最高部位设置面积不小于1．0m2的可开启外窗或开口；当建筑高度大于10m时，
        //尚应在楼梯间的外墙上每5层内设置总面积不小于2．0m2的可开启外窗或开口，且布置间隔不大于3层。

        //获取所有的封闭楼梯间及防烟楼梯间的集合stairCases
        //依次遍历每一个楼梯间
        //如果楼梯间设置了自然通风系统
        //  获得楼梯间的最低楼层编号及最高楼层编号
        //  获得楼梯间内的所有窗户的集合
        //  从窗户集合中筛选出位于最高楼层的窗户的集合
        //  查找这些窗户中是否有面积大于等于1㎡的窗户
        //  如果没有则将审查结果标记为不通过，则把当楼梯间记录进审查结果中，并提示专家审核是否最高部位有不小于1㎡的开口
        //  如果建筑高度大于10m
        //     从最底层起依次计算从当前层起向上五层内的所有窗的总面积（一直到当前楼层编号为【最高楼层编号-4】为止）
        //     如果总面积小于2.0㎡，则把当楼梯间记录进审查结果中，并提示专家审核是否有其他开口满足面积要求
        //     从最低楼层起依次查找从当前楼层向上三层内是否有可开启外窗，（一直到当前楼层编号为【最高楼层编号-2】为止）
        //     如果没有可开启外窗，则把当楼梯间记录进审查结果中，并提示专家审核是否有其他开口满足设置要求
        //如果审查通过
        //则在审查结果批注中注明审查通过相关内容
        //如果审查不通过
        //则在审查结果中注明审查不通过的相关内容

        public static BimReview GB51251_2017_3_2_1()
        {
            //初始化审查结果
            BimReview result = new BimReview("GB51251_2017", "3.2.1");
            //获取所有的封闭楼梯间及防烟楼梯间的集合stairCases
            List<Room> stairCases = new List<Room>();

            stairCases.AddRange(HVACFunction.GetRooms("防烟楼梯间"));
            stairCases.AddRange(HVACFunction.GetRooms("封闭楼梯间"));

            //依次遍历每一个楼梯间
            foreach(Room stairCase in stairCases)
            {
               
                //如果楼梯间设置了自然通风系统
                if (assistantFunctions.isRoomHaveNatureVentilateSystem(stairCase))
                {
                    //  获得楼梯间的最低楼层编号及最高楼层编号
                    int lowestStoryNo = stairCase.storyNo.Value;
                    int highestStoryNo = HVACFunction.getHighestStoryNoOfRoom(stairCase);
                    //  获得楼梯间内的所有窗户的集合
                    List<Windows> windows = HVACFunction.GetWindowsInRoom(stairCase);
                    //  从窗户集合中筛选出位于最高楼层的窗户的集合
                    List<Windows> windowsInHighestStory = assistantFunctions.filtrateElementsBetweenFloor_aAndFloor_b(windows, highestStoryNo, highestStoryNo);
                    //  查找这些窗户中是否有面积大于等于1㎡的窗户
                    if(windowsInHighestStory.findWindowNoSmallerThanSomeArea(1)==null)
                    {
                        //  如果没有则将审查结果标记为不通过，则把当楼梯间记录进审查结果中，并提示专家审核是否最高部位有不小于1㎡的开口
                        result.isPassCheck = false;
                        string remark = string.Empty;
                        remark = "需专家复核此楼梯间最高部位是否有不小于1㎡的其他开口";
                        result.AddViolationComponent(stairCase.Id.Value, stairCase.type, remark);
                        continue;
                    }
                    //  如果建筑高度大于10m
                    if(globalData.buildingHeight>10)
                    {
                        bool isCurrentStairCaseViolate = false;

                        //     从最底层起依次计算从当前层起向上五层内的所有窗的总面积（一直到当前楼层编号为【最高楼层编号-4】为止）
                        for (int storyNo=lowestStoryNo;storyNo<=Math.Max(lowestStoryNo,highestStoryNo-4);++storyNo)
                        {
                            //     如果总面积小于2.0㎡，则把当楼梯间记录进审查结果中，并提示专家审核是否有其他开口满足面积要求
                            List<Windows> windowsInFiveStories = assistantFunctions.filtrateElementsBetweenFloor_aAndFloor_b(windows, storyNo, Math.Min(highestStoryNo,storyNo + 4));
                            if (assistantFunctions.calculateTotalAreaOfWindows(windowsInFiveStories) < 2)
                            {
                                result.isPassCheck = false;
                                string remark = string.Empty;
                                remark = "需专家复核此楼梯间是否还有其它开口满足面积要求";
                                result.AddViolationComponent(stairCase.Id.Value, stairCase.type, remark);
                                isCurrentStairCaseViolate = true;
                                break;
                            }
                        }
                        if (isCurrentStairCaseViolate)
                            continue;
                        //     从最低楼层起依次查找从当前楼层向上三层内是否有可开启外窗，（一直到当前楼层编号为【最高楼层编号-2】为止）
                        for (int storyNo = lowestStoryNo; storyNo <= Math.Max(lowestStoryNo, highestStoryNo - 2); ++storyNo)
                        {
                            //     如果没有可开启外窗，则把当楼梯间记录进审查结果中，并提示专家审核是否有其他开口满足设置要求
                            List<Windows> windowsInThreeStories = assistantFunctions.filtrateElementsBetweenFloor_aAndFloor_b(windows, storyNo,Math.Min(highestStoryNo, storyNo + 2));
                            if (windowsInThreeStories.Count<=0)
                            {
                                result.isPassCheck = false;
                                string remark = string.Empty;
                                remark = "需专家复核此楼梯间是否还有其它开口满足设置要求";
                                result.AddViolationComponent(stairCase.Id.Value, stairCase.type, remark);
                                break;
                            }
                        }
                       
                    }

                }
            }
            //如果审查通过
            //则在审查结果批注中注明审查通过相关内容
            if (result.isPassCheck)
            {
                result.comment = "设计满足规范GB51251_2017中第3.2.1条条文规定。";
            }
            //如果审查不通过
            //则在审查结果中注明审查不通过的相关内容
            else
            {
                result.comment = "可开启外窗设置不满足规范GB51251_2017中第3.2.1条条文规定。请专家复核楼梯间中是否有其他开口满足规范要求";
            }
            return result;
        }

        //建筑高度大于100m的建筑，其机械加压送风系统应竖向分段独立设置，且每段高度不应超过100m。

        //获得所有防烟楼梯间及封闭楼梯间
        //依次遍历所有楼梯间
        //如果楼梯间采用了机械加压送风系统
        //获得此楼梯间的最低楼层编号及最高楼层编号
        //获得楼梯间内的所有正压送风口
        //自下向上获得每个风口负担的楼层划分
        //自上向下获得每个风口负担的楼层划分
        //找到此楼梯间的所有正压送风机
        //依次遍历每台风机
        //找到每台风机的所有加压送风口
        //根据自下向上方式获得的风口楼层划分，确定风机负担的高度。
        //如果风机有风机负担的高度大于100m
        //则从新遍历所有风机
        //根据自上向下的方式获得风口楼层划分，确定风机负担的高度
        //如果风机负担的高度大于100m
        //则标记审查不通过，并把当前楼梯间记录到审查结果中
        //如果审查通过
        //则在审查结果批注中注明审查通过相关内容
        //如果审查不通过
        //则在审查结果中注明审查不通过的相关内容

        public static BimReview GB51251_2017_3_3_1()
        {
            //将审查结果初始化
            BimReview result = new BimReview("GB51251_2017", "3.3.1");
            //获得所有防烟楼梯间及封闭楼梯间
            List<Room> stairCases = new List<Room>();
            stairCases.AddRange(HVACFunction.GetRooms("防烟楼梯间"));
            stairCases.AddRange(HVACFunction.GetRooms("封闭楼梯间"));
            //依次遍历所有楼梯间
            foreach(Room stairCase in stairCases)
            {
                //如果楼梯间采用了机械加压送风系统
                if(assistantFunctions.isRoomHaveSomeMechanicalSystem(stairCase,"加压送风"))
                {

                    //获得楼梯间内的所有正压送风口
                    List<AirTerminal> airTerminals = HVACFunction.GetRoomContainAirTerminal(stairCase);
                    List<AirTerminal> pressureAirTerminals = assistantFunctions.filtrateAirTerminalOfSomeSystem(airTerminals, "加压送风");

                    //获得此楼梯间的最低楼层编号及最高楼层编号
                    int highestStoryNo = HVACFunction.getHighestStoryNoOfRoom(stairCase);
                    int lowestStoryNo = stairCase.storyNo.Value;
                    List<Floor> floors = assistantFunctions.filterFloorsBetweenlowestAndHighestStoryNo(lowestStoryNo, highestStoryNo);

                    //自下向上获得每个风口负担的楼层划分
                    Dictionary<AirTerminal, List<Floor>> floorDivisionOfAirTerminalBottomUp = assistantFunctions.getFloorDivisionOfAirTerminalsBottomUp(floors, pressureAirTerminals);
                    //自上向下获得每个风口负担的楼层划分
                    Dictionary<AirTerminal, List<Floor>> floorDivisionOfAirTerminalTopToBottom = assistantFunctions.getFloorDivisionOfAirTerminalsTopToBottom(floors, pressureAirTerminals);

                    //找到此楼梯间的所有正压送风机
                    List<Fan> fans = assistantFunctions.getAllFansConnectToAirTerminals(pressureAirTerminals);
                    bool isNeedReTraverse = false;
                    //依次遍历每台风机
                    foreach(Fan fan in fans)
                    {
                        //以自下向上方式获得确定正压送风机负担的高度。
                        //如果风机有风机负担的高度大于100m
                        if (assistantFunctions.getAffordHeightOfFanByFloorDivision(fan,floorDivisionOfAirTerminalBottomUp) > 100000)
                        {
                            isNeedReTraverse = true;
                            break;
                        }
                           
                    }
                    //则从新遍历所有风机
                    if(isNeedReTraverse)
                        foreach(Fan fan in fans)
                        {
                            //根据自上向下的方式获得风口楼层划分，确定风机负担的高度
                            //如果风机负担的高度大于100m
                            //则标记审查不通过，并把当前楼梯间记录到审查结果中
                            if (assistantFunctions.getAffordHeightOfFanByFloorDivision(fan, floorDivisionOfAirTerminalTopToBottom) > 100000)
                            {
                                result.isPassCheck = false;
                                string remark = string.Empty;
                                result.AddViolationComponent(stairCase.Id.Value, stairCase.type, remark);
                                break;
                            }
                            
                        }
                }
            }
            //如果审查通过
            //则在审查结果批注中注明审查通过相关内容
            if (result.isPassCheck)
            {
                result.comment = "设计满足规范GB51251_2017中第3.3.1条条文规定。";
            }
            //如果审查不通过
            //则在审查结果中注明审查不通过的相关内容
            else
            {
                result.comment = "设计不满足规范GB51251_2017中第3.3.1条条文规定。";
            }
            return result;
        }

        //设置机械加压送风系统的封闭楼梯间、防烟楼梯间，尚应在其顶部设置不小于1m2的固定窗。靠外墙的防烟楼梯间，尚应在其外墙上每5层内设置总面积不小于2m2的固定窗。

        //获得所有封闭楼梯间、防烟楼梯间的集合
        //如果楼梯间设置了机械加压送风系统
        //获得楼梯间的最高楼层编号
        //获得楼梯间所有固定窗
        //筛选出最高楼层的固定窗
        //如果最高楼层没有固定窗则将审查结果标记为不通过，并把当楼梯间记录进审查结果中
        //获得楼梯间的所有外墙
        //如果楼梯间有外墙
        //

    }
    public class modelException : Exception
    {
        public modelException(string message) : base(message)
        { }
    }

}
