using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BCGL.Sharp;

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
                    rooms_temp = HVACFunction.GetRoomsMoreThan("走廊",20);
                    rooms.AddRange(rooms_temp);
                }
                //  如果建筑高度小于等于32m则获得所长度大于40m的疏散走道
                else
                {
                    rooms_temp = HVACFunction.GetRoomsMoreThan("走廊", 40);
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
                List<Room> corridorsMoreThan20m = HVACFunction.GetRoomsMoreThan("走廊",20);
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

        //通风、空气调节系统的风管在下列部位应设置公称动作温度为70℃的防火阀：
        //1 穿越防火分区处；
        //2 穿越通风、空气调节机房的房间隔墙和楼板处；
        //3 穿越重要或火灾危险性大的场所的房间隔墙和楼板处；
        //4 穿越防火分隔处的变形缝两侧；
        //5 竖向风管与每层水平风管交接处的水平管段上。

        //获得所有防火分区对象
        //依次遍历每一个防火分区对象
        //获得所有穿越防火分区的风管集合ductsCrossFireCompartment
        //从风管集合ductsCrossFireCompartment中筛选出所有空调、通风管道
        //获得所有的竖井
        //依次遍历每一个竖井，获得竖井内的风管集合ductsInShaft以及穿越竖井的风管的集合ductsCrossShaft
        //从风管集合ductsInShaft和风管的集合ductsCrossShaft中筛选出所有空调、通风管道
        //从跨越防火分区的风管集合中除去处于竖井内的风管并将剩余的风管放于ducts集合中。
        //获得穿越设备机房的通风、空调风管集合并将风管放于ducts集合中。
        //获得所有重要房间（具有防火门的房间）
        //获得穿越重要房间的通风、空调风管集合并将风管放于ducts集合中。
        //依次遍历穿越竖井的风管集合ductsCrossShaft中的风管
        //判断风管连接的立管是否跨越了防火分区。如果立管跨越了防火分区，则将风管放入ducts中
        //依次遍历ducts集合中的每一根风管
        //获得风管上的防火阀
        //如果没有防火阀或者防火阀没有在穿越点附近,则在审查结果中标记审查不通过，并将风管加入到审查结果，在风管构件的备注中记录此风管未在穿越点附近设置防火阀
        //获得所有穿越防火分隔处的变形缝的通风、空调风管集合DuctsCrossMovementJointAndFireSide
        //依次遍历以上风管集合DuctsCrossMovementJointAndFireSide
        //获得风管上的所有防火阀
        //如果防火阀少于两个或者防火阀没有在穿越点附近,则在审查结果中标记审查不通过，并将风管加入到审查结果，在风管构件的备注中记录此风管未在穿越点附近设置防火阀
        //如果审查通过
        //则在审查结果批注中注明审查通过相关内容
        //如果审查不通过
        //则在审查结果中注明审查不通过的相关内容

        public static BimReview GB50016_2014_9_3_11()
        {
            //将审查结果初始化
            BimReview result = new BimReview("GB50016_2014", "9.3.11");

            List<Duct> ducts = new List<Duct>();
            //获得所有防火分区对象
            List<FireCompartment> fireCompartments = HVACFunction.GetFireCompartment("");
            List<Duct> ductsCrossFireCompartment = new List<Duct>();
            //依次遍历每一个防火分区对象
            foreach (FireCompartment fireCompartment in fireCompartments)
            {
                //获得所有穿越防火分区的风管集合ductsCrossFireCompartment
                ductsCrossFireCompartment=ductsCrossFireCompartment.addDuctsToList( HVACFunction.GetDuctsCrossFireDistrict(fireCompartment));
            }
            List<string> systemTypes = new List<string>();
            systemTypes.Add("通风");
            systemTypes.Add("空调");
            //从风管集合ductsCrossFireCompartment中筛选出所有空调、通风管道
            ductsCrossFireCompartment = ductsCrossFireCompartment.filterSomeSystemTypeDuctsFromList(systemTypes);
            //获得所有的竖井
            List<Room> shafts = HVACFunction.GetRooms("竖井");
            //依次遍历每一个竖井，获得竖井内的风管集合ductsInShaft以及穿越竖井的风管的集合ductsCrossShaft
            List<Duct> ductsInShaft = new List<Duct>();
            List<Duct> ductsCrossShaft = new List<Duct>();
            foreach (Room shaft in shafts)
            {
                ductsInShaft.AddRange(HVACFunction.getAllDuctsInRoom(shaft));
                ductsCrossShaft.AddRange(HVACFunction.GetDuctsCrossSpace(shaft));
            }
            //从风管集合ductsInShaft和风管的集合ductsCrossShaft中筛选出所有空调、通风管道
            ductsCrossShaft = ductsCrossShaft.filterSomeSystemTypeDuctsFromList(systemTypes);
            ductsInShaft= ductsInShaft.filterSomeSystemTypeDuctsFromList(systemTypes);
            //从跨越防火分区的风管集合中除去处于竖井内的风管并将剩余的风管放于ducts集合中。
            ducts.addDuctsToList(ductsCrossFireCompartment);
            ducts.removeDuctsFromList(ductsInShaft);
            //获得穿越设备机房的风管集合并将风管放于ducts集合中。
            List<Room> EquipmentRoom = HVACFunction.GetRooms("设备用房");
            foreach(Room room in EquipmentRoom)
            {
                ducts=ducts.addDuctsToList(HVACFunction.GetDuctsCrossSpace(room));
            }
            //获得所有重要房间（具有防火门的房间）
            List<Room> importantRoom = HVACFunction.getAllRoomsHaveFireDoor();
            //获得穿越重要房间的风管集合并将风管放于ducts集合中。
            foreach (Room room in importantRoom)
            {
                ducts = ducts.addDuctsToList(HVACFunction.GetDuctsCrossSpace(room));
            }
            //依次遍历穿越竖井的风管集合ductsCrossShaft中的风管
            foreach(Duct duct in ductsCrossShaft)
            {
                //判断风管连接的立管是否跨越了防火分区。如果立管跨越了防火分区，则将风管放入ducts中
                List<Duct>verticalDucts= HVACFunction.getAllVerticalDuctConnectedToDuct(duct);
                verticalDucts = assistantFunctions.filterSameDuctsInTwoList(verticalDucts, ductsInShaft);
                foreach(Duct verticalDuct in verticalDucts)
                {
                    if(ductsCrossFireCompartment.findItem(verticalDuct) !=null)
                    {
                        ducts.Add(duct);
                        break;
                    }
                }
            }

            //依次遍历ducts集合中的每一根风管
            foreach(Duct duct in ducts)
            {
                //获得风管上的防火阀
                List<FireDamper> fireDampers = HVACFunction.getFireDamperOfDuct(duct);
                //如果没有风阀或者风阀没有在穿越点附近,则在审查结果中标记审查不通过，并将风管加入到审查结果，在风管构件的备注中记录此风管未在穿越点附近设置防火阀
                if(fireDampers.Count<1)
                {
                    result.isPassCheck = false;
                    string remark = string.Empty;
                    remark = "此风管未设置防火阀";
                    result.AddViolationComponent(duct.Id.Value, "风管", remark);
                    continue;
                }

            }
            //获得所有穿越防火分隔处的变形缝的风管集合
            List<Duct> ductsCrossMovementJointAndFireSide = HVACFunction.GetDuctsCrossMovementJointAndFireSide();
            //依次判断以上风管集合
            foreach (Duct duct in ductsCrossMovementJointAndFireSide)
            {
                //获得风管上的所有风阀
                List<FireDamper> fireDampers = HVACFunction.getFireDamperOfDuct(duct);
                //如果风阀少于两个或者风阀没有在穿越点附近,则在审查结果中标记审查不通过，并将风管加入到审查结果，在风管构件的备注中记录此风管未在穿越点附近设置防火阀
                if(fireDampers.Count < 2)
                {
                    result.isPassCheck = false;
                    string remark = string.Empty;
                    remark = "此风管未在穿越变形缝两侧设置防火阀";
                    result.AddViolationComponent(duct.Id.Value, "风管", remark);
                    continue;
                }
            }
            //如果审查通过
            //则在审查结果批注中注明审查通过相关内容
            if (result.isPassCheck)
            {
                result.comment = "设计满足规范GB50016_2014中第9.3.11条条文规定。";
            }
            //如果审查不通过
            //则在审查结果中注明审查不通过的相关内容
            else
            {
                result.comment = "设计不满足规范GB50016_2014中第9.3.11条条文规定。";
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
                List<Room> atriasLinkToStairCase = HVACFunction.GetConnectedRooms(stairCase);
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
                    int highestStoryNo = HVACFunction.GetHighestStoryNoOfRoom(stairCase);
                    //  获得楼梯间内的所有窗户的集合
                    List<Window> windows = HVACFunction.GetWindowsInRoom(stairCase);
                    //  从窗户集合中筛选出位于最高楼层的窗户的集合
                    List<Window> windowsInHighestStory = assistantFunctions.filtrateElementsBetweenFloor_aAndFloor_b(windows, highestStoryNo, highestStoryNo);
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
                        int storyNoUpperBound =0;
                        if ((highestStoryNo - 4) * highestStoryNo <= 0)
                            storyNoUpperBound = Math.Max(lowestStoryNo, highestStoryNo - 5);
                        else
                            storyNoUpperBound = Math.Max(lowestStoryNo, highestStoryNo - 4);
                        
                        for (int storyNo=lowestStoryNo;storyNo<= storyNoUpperBound;++storyNo)
                        {
                            //     如果总面积小于2.0㎡，则把当楼梯间记录进审查结果中，并提示专家审核是否有其他开口满足面积要求
                            int highestStoryNoInCurrentIteration = 0;
                            if ((storyNo + 4) * storyNo <= 0)
                                highestStoryNoInCurrentIteration = Math.Min(highestStoryNo, storyNo + 5);
                            else
                                highestStoryNoInCurrentIteration = Math.Min(highestStoryNo, storyNo + 4);

                          
                            List<Window> windowsInFiveStories = assistantFunctions.filtrateElementsBetweenFloor_aAndFloor_b(windows, storyNo, highestStoryNoInCurrentIteration);
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

                        
                        if ((highestStoryNo - 2) * highestStoryNo <= 0)
                            storyNoUpperBound = Math.Max(lowestStoryNo, highestStoryNo - 3);
                        else
                            storyNoUpperBound = Math.Max(lowestStoryNo, highestStoryNo - 2);

                      
                        for (int storyNo = lowestStoryNo; storyNo <= storyNoUpperBound; ++storyNo)
                        {
                            //     如果没有可开启外窗，则把当楼梯间记录进审查结果中，并提示专家审核是否有其他开口满足设置要求
                            int highestStoryNoInCurrentIteration = Math.Min(highestStoryNo, storyNo + 2);
                            if (highestStoryNoInCurrentIteration == 0)
                                highestStoryNoInCurrentIteration = 1;
                            List<Window> windowsInThreeStories = assistantFunctions.filtrateElementsBetweenFloor_aAndFloor_b(windows, storyNo,highestStoryNoInCurrentIteration);
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
                    int highestStoryNo = HVACFunction.GetHighestStoryNoOfRoom(stairCase);
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
        //依次遍历每一个楼梯间
        //如果楼梯间设置了机械加压送风系统
        //获得楼梯间的最高楼层编号
        //获得楼梯间所有固定窗
        //筛选出最高楼层的固定窗
        //如果最高楼层没有固定窗或固定窗面积小于1㎡则将审查结果标记为不通过，并把当楼梯间记录进审查结果中
        //获得楼梯间的所有外墙
        //如果楼梯间有外墙
        //从最底层开会依次向上进行遍历
        //计算五层以内的固定窗面积
        //如果固定窗总面积小于2㎡，则将审查结果标记为不通过，并将当前楼梯间加入到审查结果中
        //如果审查通过
        //则在审查结果批注中注明审查通过相关内容
        //如果审查不通过
        //则在审查结果中注明审查不通过的相关内容
        public static BimReview GB51251_2017_3_3_11()
        {
            //初始化审查结果
            BimReview result = new BimReview("GB51251_2017", "3.3.11");
            //获得所有封闭楼梯间、防烟楼梯间的集合
            List<Room> stairCases = new List<Room>();
            stairCases.AddRange(HVACFunction.GetRooms("封闭楼梯间"));
            stairCases.AddRange(HVACFunction.GetRooms("防烟楼梯间"));
            //依次遍历每一个楼梯间
            foreach(Room stairCase in stairCases)
            {
                //如果楼梯间设置了机械加压送风系统
                if (assistantFunctions.isRoomHaveSomeMechanicalSystem(stairCase,"加压送风"))
                {
                    //获得楼梯间的最高楼层编号
                    int highestStoryNo = HVACFunction.GetHighestStoryNoOfRoom(stairCase);
                    int lowestStoryNo = stairCase.storyNo.Value;
                    //获得楼梯间所有固定窗
                    List<Window> fixWindows = assistantFunctions.getFixOuterWindowsOfRoom(stairCase);
                    //筛选出最高楼层的固定窗
                    List<Window> windowsInHighestStory =assistantFunctions.filtrateElementsBetweenFloor_aAndFloor_b(fixWindows,highestStoryNo, highestStoryNo);
                    //如果最高楼层没有固定窗或固定窗面积小于1平米则将审查结果标记为不通过，并把当楼梯间记录进审查结果中
                    if(windowsInHighestStory.Count==0||assistantFunctions.calculateTotalAreaOfWindows(windowsInHighestStory)<1)
                    {
                        result.isPassCheck = false;
                        string remark = string.Empty;
                        result.AddViolationComponent(stairCase.Id.Value, stairCase.type, remark);
                        continue;
                    }
                    //获得楼梯间的所有外墙
                    List<Wall> outerWall = assistantFunctions.getOuterWallOfRoom(stairCase);
                    //如果楼梯间有外墙
                    if(outerWall.Count>0)
                    {
                        //从最底层开会依次向上进行遍历
                        int storyNoUpperBound = 0;
                        if ((highestStoryNo-4)*highestStoryNo<=0)
                            storyNoUpperBound = Math.Max(lowestStoryNo, highestStoryNo - 5);
                        else
                            storyNoUpperBound = Math.Max(lowestStoryNo, highestStoryNo - 4);

                       
                       
                        for (int currentStoryNo= stairCase.storyNo.Value; currentStoryNo <= storyNoUpperBound; ++currentStoryNo)
                        {
                            //计算五层以内的固定窗面积
                            int highestStoryNoInCurrentIteration = 0;
                            if ((currentStoryNo + 4) * currentStoryNo <= 0)
                                highestStoryNoInCurrentIteration = Math.Min(highestStoryNo, currentStoryNo + 5);
                            else
                                highestStoryNoInCurrentIteration = Math.Min(highestStoryNo, currentStoryNo + 4);

                           

                            List < Window > windowsInFiveStorys = assistantFunctions.filtrateElementsBetweenFloor_aAndFloor_b(fixWindows, currentStoryNo, highestStoryNoInCurrentIteration) ;
                            double areaOfFixWindows = assistantFunctions.calculateTotalAreaOfWindows(windowsInFiveStorys);
                            //如果固定窗总面积小于2㎡，则将审查结果标记为不通过，并将当前楼梯间加入到审查结果中
                            if (areaOfFixWindows<2)
                            {
                                result.isPassCheck = false;
                                string remark = string.Empty;
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
                result.comment = "设计满足规范GB51251_2017中第3.3.11条条文规定。";
            }
            //如果审查不通过
            //则在审查结果中注明审查不通过的相关内容
            else
            {
                result.comment = "设计不满足规范GB51251_2017中第3.3.11条条文规定。";
            }
            return result;         
        }



        //当建筑的机械排烟系统沿水平方向布置时，每个防火分区的机械排烟系统应独立设置。

        //获得所有排烟风机对象
        //依次遍历每一台排烟风机
        //获得风机连接的所有排风口
        //如果风口为排烟风口且风机是水平布置的（排烟风机连接的所有排烟口都在同一层）
        //判断风机的所有排烟口是否都在一个防火分区中
        //如果不在同一个防火分区中，则将审查结果标记为不通过并将此排烟风机加入到审查结果中。
        //如果审查通过
        //则在审查结果批注中注明审查通过相关内容
        //如果审查不通过
        //则在审查结果中注明审查不通过的相关内容

        public static BimReview GB51251_2017_4_4_1()
        {
            //初始化审查结果
            BimReview result = new BimReview("GB51251_2017", "4.4.1");
          
            //通过排烟口找到所有排烟风机
            List<Fan> fans = assistantFunctions.getFansOfSomeSyetemType("排烟");
            foreach(Fan fan in fans)
            {
                //获得风机连接的所有排风口
                List<AirTerminal> airTerminals = HVACFunction.GetInletsOfFan(fan);
                //如果风机是水平布置的（排烟风机连接的所有排烟口都在同一层）
                if (assistantFunctions.isAllAirTerminalInSameFloor(airTerminals))
                {
                    //判断风机的所有排烟口是否都在一个防火分区中

                    FireCompartment fireDistrict = HVACFunction.GetFireCompartmentContainAirTerminal(airTerminals[0]);
                    foreach(AirTerminal airTerminal in airTerminals)
                    {
                        if(!HVACFunction.isAirTerminalInFireCompartment(airTerminal,fireDistrict))
                        {
                            result.isPassCheck = false;
                            string remark = "风机所在的排烟系统跨越了防火分区设置";
                            result.AddViolationComponent(fan.Id.Value, "风机", remark);
                            break;
                        }
                    }
                }
            }
            //如果审查通过
            //则在审查结果批注中注明审查通过相关内容
            if (result.isPassCheck)
            {
                result.comment = "设计满足规范GB51251_2017中第4.4.1条条文规定。";
            }
            //如果审查不通过
            //则在审查结果中注明审查不通过的相关内容
            else
            {
                result.comment = "设计不满足规范GB51251_2017中第4.4.1条条文规定。";
            }
            return result;
        }



        //建筑高度超过50m的公共建筑和建筑高度超过100m的住宅，其排烟系统应竖向分段独立设置，且公共建筑每段高度不应超过50m，住宅建筑每段高度不应超过100m。

        //如果建筑类型为公共建筑且建筑高度大于50m或者建筑类型为住宅且建筑高度大于100m
        //获得所以排烟风机
        //依次遍历每一个排烟风机
        //获得风机所有连接的所有排烟风口
        //获得最大风口标高及最小风口标高，有两者差值求出风机所负担的高度。
        //如果建筑类型为公共建筑且风机负担的高度大于50m或建筑类型为住宅且风机负担的高度大于100m
        //则将审查结果标记为不通过并将此排烟风机加入到审查结果中。
        //如果审查通过
        //则在审查结果批注中注明审查通过相关内容
        //如果审查不通过
        //则在审查结果中注明审查不通过的相关内容
        public static BimReview GB51251_2017_4_4_2()
        {
            //初始化审查结果
            BimReview result = new BimReview("GB51251_2017", "4.4.2");

            //如果建筑类型为公共建筑且建筑高度大于50m或者建筑类型为住宅且建筑高度大于100m
            if (globalData.buildingType.Contains("公共建筑") && globalData.buildingHeight > 50 || globalData.buildingType.Contains("住宅") && globalData.buildingHeight > 100)
            {
                //通过排烟口找到所有排烟风机
                List<Fan> smokeFans = assistantFunctions.getFansOfSomeSyetemType("排烟");

                //依次遍历每一个排烟风机
                foreach (Fan smokeFan in smokeFans)
                {
                    //获得最大风口标高及最小风口标高，有两者差值求出风机所负担的高度。
                    double affordHeightOfSmokeFan = assistantFunctions.getAffordHeightOfSomkeFan(smokeFan);
                    //如果建筑类型为公共建筑且风机负担的高度大于50m或建筑类型为住宅且风机负担的高度大于100m
                    if(globalData.buildingType.Contains("公共建筑") && affordHeightOfSmokeFan > 50000 || globalData.buildingType.Contains("住宅") && affordHeightOfSmokeFan > 100000)
                    {
                        //则将审查结果标记为不通过并将此排烟风机加入到审查结果中。
                        result.isPassCheck = false;
                        string remark = "风机所在的排烟系统设置高度超过规范要求";
                        result.AddViolationComponent(smokeFan.Id.Value, "风机", remark);
                    }
                }
            }

            //如果审查通过
            //则在审查结果批注中注明审查通过相关内容
            if (result.isPassCheck)
            {
                result.comment = "设计满足规范GB51251_2017中第4.4.2条条文规定。";
            }
            //如果审查不通过
            //则在审查结果中注明审查不通过的相关内容
            else
            {
                result.comment = "设计不满足规范GB51251_2017中第4.4.2条条文规定。";
            }
            return result;
        }

        //除地上建筑的走道或建筑面积小于500m2的房间外，设置排烟系统的场所应设置补风系统。

        //获取所有房间的集合rooms
        //从rooms中除去地上走廊及地上面积小于500平米的房间
        //依次遍历rooms中的每一个房间
        //如果房间设置了机械排烟系统
        //如果此房间没有设置补风系统
        //则将审查结果标记为不通过，且把当前房间记录进审查结果中。并在批注中记录此房间未设补风系统
        //如果房间设置了自然排烟系统
        //将房间加入到结果中，并在批注中记录此房间采用自然排烟方式，请专家核对此房间补风系统是否满足要求
        //如果审查通过
        //则在审查结果批注中注明审查通过相关内容
        //如果审查不通过
        //则在审查结果中注明审查不通过的相关内容
        public static BimReview GB51251_2017_4_5_1()
        {
            //初始化审查结果
            BimReview result = new BimReview("GB51251_2017", "4.5.1");
            //获取所有房间的集合rooms
            List<Room> rooms = HVACFunction.GetRooms("");
            //从rooms中除去地上走廊及地上面积小于500平米的房间
            List<assistantFunctions.exceptRoomCondition> conditions = new List<assistantFunctions.exceptRoomCondition>();
            assistantFunctions.exceptRoomCondition overgroundCorridorCondition = new assistantFunctions.exceptRoomCondition();
            overgroundCorridorCondition.area = 0;
            overgroundCorridorCondition.type = "走廊";
            overgroundCorridorCondition.name = "";
            overgroundCorridorCondition.roomPosition = RoomPosition.overground;
            overgroundCorridorCondition.areaType = assistantFunctions.exceptRoomCondition.AreaType.LargerThan;
            conditions.Add(overgroundCorridorCondition);
           
            assistantFunctions.exceptRoomCondition overgroundSmallerThan500sqmCondition = new assistantFunctions.exceptRoomCondition();
            overgroundSmallerThan500sqmCondition.area = 500;
            overgroundSmallerThan500sqmCondition.type = "";
            overgroundSmallerThan500sqmCondition.name = "";
            overgroundSmallerThan500sqmCondition.roomPosition = RoomPosition.overground;
            overgroundSmallerThan500sqmCondition.areaType = assistantFunctions.exceptRoomCondition.AreaType.SmallerThan;
            conditions.Add(overgroundSmallerThan500sqmCondition);

            rooms= rooms.exceptSomeRooms(conditions);
            //依次遍历rooms中的每一个房间
            foreach(Room room in rooms)
            {
                //如果房间设置了机械排烟系统且此房间没有设置补风系统
                if (assistantFunctions.isRoomHaveSomeMechanicalSystem(room,"排烟")&&!assistantFunctions.isRoomHaveSomeSystem(room,"补风"))
                {
                    //则将审查结果标记为不通过，且把当前房间记录进审查结果中。并在批注中记录此房间未设补风系统
                    result.isPassCheck = false;
                    string remark = "此房间未设补风系统";
                    result.AddViolationComponent(room.Id.Value, room.type, remark);
                }
                //如果房间设置了自然排烟系统
                else if(assistantFunctions.isRoomHaveNatureSmokeExhaustSystem(room))
                {
                    //将房间加入到结果中，并在批注中记录此房间采用自然排烟方式，请专家核对此房间补风系统是否满足要求
                    string remark = "此房间采用自然排烟方式，请专家核对此房间补风系统是否满足要求";
                    result.AddViolationComponent(room.Id.Value, room.type, remark);
                }

            }
            //如果审查通过
            //则在审查结果批注中注明审查通过相关内容
            if (result.isPassCheck)
            {
                result.comment = "设计满足规范GB51251_2017中第4.5.1条条文规定。";
            }
            //如果审查不通过
            //则在审查结果中注明审查不通过的相关内容
            else
            {
                result.comment = "设计不满足规范GB51251_2017中第4.5.1条条文规定。";
            }

            result.comment += GB51251_2017_4_5_1_additionComment(result);
            return result;
        }

        private static string GB51251_2017_4_5_1_additionComment(BimReview result)
        {
            string comment = "";

            foreach(ComponentAnnotation componetAnnotation in result.violationComponents)
            {
                if (componetAnnotation.remark.Contains("核对"))
                {
                    comment += "请专家核对采用自然排烟方式的房间是否满足补风要求。";
                    break;
                }
            }
            return comment;
        }

        //补风系统应直接从室外引入空气，且补风量不应小于排烟量的50％

        //获得所有房间
        //依次遍历所有房间
        //如果房间设置了机械排烟系统且设置了机械补风系统
        //获得所有补风系统的补风机
        //依次遍历每一台补风机
        //获得补风机的取风风口
        //如果取风风口不为室外风口
        //则将审查结果标记为不通过，且把当前房间记录进审查结果中。并在批注中记录此房间补风系统未从室外引入空气
        //如果取风口为室外风口
        //通过叠加房间内的所有补风口的风量获得房间的补风量
        //获取房间所有排烟风口
        //通过叠加房间内所有排烟口的风量获得房间的排烟量
        //如果补风量小于排烟量的50%
        //则将审查结果标记为不通过，且把当前房间记录进审查结果中。并在批注中记录此房间补风量小于排烟量的50%
        //如果审查通过
        //则在审查结果批注中注明审查通过相关内容
        //如果审查不通过
        //则在审查结果中注明审查不通过的相关内容
        public static BimReview GB51251_2017_4_5_2()
        {
            //初始化审查结果
            BimReview result = new BimReview("GB51251_2017", "4.5.2");
            //获得所有房间
            List<Room> rooms = HVACFunction.GetRooms("");
            //依次遍历所有房间
            foreach(Room room in rooms)
            {
                bool isRoomViolation = false;
                //如果房间设置了机械排烟系统且设置了机械补风系统
                if(assistantFunctions.isRoomHaveSomeMechanicalSystem(room,"排烟")&&assistantFunctions.isRoomHaveSomeMechanicalSystem(room,"补风"))
                {
                    //获得所有补风系统的补风机
                    List<AirTerminal> airTerminals = HVACFunction.GetRoomContainAirTerminal(room);
                    List<AirTerminal> supplementAirTerminals = assistantFunctions.filtrateAirTerminalOfSomeSystem(airTerminals ,"补风");
                    List<Fan> fans = assistantFunctions.getAllFansConnectToAirTerminals(supplementAirTerminals);
                    //依次遍历每一台补风机
                    foreach(Fan fan in fans)
                    { 
                        //如果风机的取风口不全为室外风口
                      if(!assistantFunctions.isAllFanInletsAreOuterAirTerminals(fan))
                      {
                            //则将审查结果标记为不通过，且把当前房间记录进审查结果中。并在批注中记录此房间补风系统未从室外引入空气
                            result.isPassCheck = false;
                            string remark = "此房间补风系统未从室外引入空气";
                            result.AddViolationComponent(room.Id.Value, room.type, remark);
                            isRoomViolation = true;
                            break;
                      }
                    }
                    if (isRoomViolation)
                        continue;

                    //获取房间所有排烟风口
                    List<AirTerminal> smokeExhaustAirTeriminals = assistantFunctions.filtrateAirTerminalOfSomeSystem(airTerminals,"排烟");
                    //通过叠加房间内所有排烟口的风量获得房间的排烟量
                    double totalFlowRateOfSmokeExhaust = assistantFunctions.getTotalAirVolumeOfAirTerminals(smokeExhaustAirTeriminals);
                    //通过叠加房间内的所有补风口的风量获得房间的补风量
                    double totalFlowRateOfAirSupplement = assistantFunctions.getTotalAirVolumeOfAirTerminals(supplementAirTerminals);
                    //如果补风量小于排烟量的50%
                    if(totalFlowRateOfAirSupplement<0.5*totalFlowRateOfSmokeExhaust)
                    {
                        //则将审查结果标记为不通过，且把当前房间记录进审查结果中。并在批注中记录此房间补风量小于排烟量的50%
                        result.isPassCheck = false;
                        string remark = "此房间补风量小于排烟量的50%";
                        result.AddViolationComponent(room.Id.Value, room.type, remark);
                    }

                }
            }
            //如果审查通过
            //则在审查结果批注中注明审查通过相关内容
            if (result.isPassCheck)
            {
                result.comment = "设计满足规范GB51251_2017中第4.5.2条条文规定。";
            }
            //如果审查不通过
            //则在审查结果中注明审查不通过的相关内容
            else
            {
                result.comment = "设计不满足规范GB51251_2017中第4.5.2条条文规定。";
            }

            return result;
        }

      

        public static BimReview GB51251_2017_4_5_2()
        {
            //初始化审查结果
            BimReview result = new BimReview("GB51251_2017", "4.5.2");
            //获得所有房间
            List<Room> rooms = HVACFunction.GetRooms("");
            //依次遍历所有房间
            foreach (Room room in rooms)
            {
                bool isRoomViolation = false;
                //如果房间设置了机械排烟系统且设置了机械补风系统
                if (assistantFunctions.isRoomHaveSomeMechanicalSystem(room, "排烟") && assistantFunctions.isRoomHaveSomeMechanicalSystem(room, "补风"))
                {
                    //获得所有补风系统的补风机
                    List<AirTerminal> airTerminals = HVACFunction.GetRoomContainAirTerminal(room);
                    List<AirTerminal> supplementAirTerminals = assistantFunctions.filtrateAirTerminalOfSomeSystem(airTerminals, "补风");
                    List<Fan> fans = assistantFunctions.getAllFansConnectToAirTerminals(supplementAirTerminals);
                    //依次遍历每一台补风机
                    foreach (Fan fan in fans)
                    {
                        //如果风机的取风口不全为室外风口
                        if (!assistantFunctions.isAllFanInletsAreOuterAirTerminals(fan))
                        {
                            //则将审查结果标记为不通过，且把当前房间记录进审查结果中。并在批注中记录此房间补风系统未从室外引入空气
                            result.isPassCheck = false;
                            string remark = "此房间补风系统未从室外引入空气";
                            result.AddViolationComponent(room.Id.Value, room.type, remark);
                            isRoomViolation = true;
                            break;
                        }
                    }
                    if (isRoomViolation)
                        continue;

                    //获取房间所有排烟风口
                    List<AirTerminal> smokeExhaustAirTeriminals = assistantFunctions.filtrateAirTerminalOfSomeSystem(airTerminals, "排烟");
                    //通过叠加房间内所有排烟口的风量获得房间的排烟量
                    double totalFlowRateOfSmokeExhaust = assistantFunctions.getTotalAirVolumeOfAirTerminals(smokeExhaustAirTeriminals);
                    //通过叠加房间内的所有补风口的风量获得房间的补风量
                    double totalFlowRateOfAirSupplement = assistantFunctions.getTotalAirVolumeOfAirTerminals(supplementAirTerminals);
                    //如果补风量小于排烟量的50%
                    if (totalFlowRateOfAirSupplement < 0.5 * totalFlowRateOfSmokeExhaust)
                    {
                        //则将审查结果标记为不通过，且把当前房间记录进审查结果中。并在批注中记录此房间补风量小于排烟量的50%
                        result.isPassCheck = false;
                        string remark = "此房间补风量小于排烟量的50%";
                        result.AddViolationComponent(room.Id.Value, room.type, remark);
                    }

                }
            }
            //如果审查通过
            //则在审查结果批注中注明审查通过相关内容
            if (result.isPassCheck)
            {
                result.comment = "设计满足规范GB51251_2017中第4.5.2条条文规定。";
            }
            //如果审查不通过
            //则在审查结果中注明审查不通过的相关内容
            else
            {
                result.comment = "设计不满足规范GB51251_2017中第4.5.2条条文规定。";
            }

            return result;
        }


        //排烟管道下列部位应设置排烟防火阀：
        //1 垂直风管与每层水平风管交接处的水平管段上；
        //2 一个排烟系统负担多个防烟分区的排烟支管上；
        //3 排烟风机入口处；
        //4 穿越防火分区处

        //获得所有防火分区对象
        //依次遍历每一个防火分区对象
        //获得所有穿越防火分区的风管集合ductsCrossFireCompartment
        //从风管集合ductsCrossFireCompartment中筛选出所有排烟管道
        //获得所有的竖井
        //依次遍历每一个竖井，获得竖井内的风管集合ductsInShaft以及穿越竖井的风管的集合ductsCrossShaft
        //从风管集合ductsInShaft和风管的集合ductsCrossShaft中筛选出所排烟管道
        //从跨越防火分区的风管集合中除去处于竖井内的风管并将剩余的风管放于ducts集合中。
        //依次遍历穿越竖井的风管集合ductsCrossShaft中的风管
        //判断风管连接的立管是否跨越了防火分区。如果立管跨越了防火分区，则将风管放入ducts中
        //获得所有与排烟风机相连的入口风管，并放入ducts集合中。
        //依次遍历ducts集合中的每一根风管
        //获得风管上的排烟防火阀
        //如果没有排烟防火阀或者排烟防火阀没有在穿越点附近,则在审查结果中标记审查不通过，并将风管加入到审查结果，在风管构件的备注中记录此风管未在穿越点附近设置排烟防火阀
        //如果审查通过
        //则在审查结果批注中注明审查通过相关内容
        //如果审查不通过
        //则在审查结果中注明审查不通过的相关内容

        public static BimReview GB51251_2017_4_4_10()
        {
            //将审查结果初始化
            BimReview result = new BimReview("GB51251_2017", "4.4.10");

            List<Duct> ducts = new List<Duct>();
            //获得所有防火分区对象
            List<FireCompartment> fireCompartments = HVACFunction.GetFireCompartment("");
            List<Duct> ductsCrossFireCompartment = new List<Duct>();
            //依次遍历每一个防火分区对象
            foreach (FireCompartment fireCompartment in fireCompartments)
            {
                //获得所有穿越防火分区的风管集合ductsCrossFireCompartment
                ductsCrossFireCompartment = ductsCrossFireCompartment.addDuctsToList(HVACFunction.GetDuctsCrossFireDistrict(fireCompartment));
            }
            List<string> systemTypes = new List<string>();
            systemTypes.Add("排烟");
            //从风管集合ductsCrossFireCompartment中筛选出所有排烟管道
            ductsCrossFireCompartment = ductsCrossFireCompartment.filterSomeSystemTypeDuctsFromList(systemTypes);
            //获得所有的竖井
            List<Room> shafts = HVACFunction.GetRooms("竖井");
            //依次遍历每一个竖井，获得竖井内的风管集合ductsInShaft以及穿越竖井的风管的集合ductsCrossShaft
            List<Duct> ductsInShaft = new List<Duct>();
            List<Duct> ductsCrossShaft = new List<Duct>();
            foreach (Room shaft in shafts)
            {
                ductsInShaft.AddRange(HVACFunction.getAllDuctsInRoom(shaft));
                ductsCrossShaft.AddRange(HVACFunction.GetDuctsCrossSpace(shaft));
            }
            //从风管集合ductsInShaft和风管的集合ductsCrossShaft中筛选出所有排烟风管
            ductsCrossShaft = ductsCrossShaft.filterSomeSystemTypeDuctsFromList(systemTypes);
            ductsInShaft = ductsInShaft.filterSomeSystemTypeDuctsFromList(systemTypes);
            //从跨越防火分区的风管集合中除去处于竖井内的风管并将剩余的风管放于ducts集合中。
            ducts.addDuctsToList(ductsCrossFireCompartment);
            ducts.removeDuctsFromList(ductsInShaft);
            
          
            //依次遍历穿越竖井的风管集合ductsCrossShaft中的风管
            foreach (Duct duct in ductsCrossShaft)
            {
                //判断风管连接的立管是否跨越了防火分区。如果立管跨越了防火分区，则将风管放入ducts中
                List<Duct> verticalDucts = HVACFunction.getAllVerticalDuctConnectedToDuct(duct);
                verticalDucts = assistantFunctions.filterSameDuctsInTwoList(verticalDucts, ductsInShaft);
                foreach (Duct verticalDuct in verticalDucts)
                {
                    if (ductsCrossFireCompartment.findItem(verticalDuct) != null)
                    {
                        ducts.Add(duct);
                        break;
                    }
                }
            }
            //获得所有与排烟风机相连的入口风管，并放入ducts集合中。

            //依次遍历ducts集合中的每一根风管
            foreach (Duct duct in ducts)
            {
                //获得风管上的防火阀
                List<FireDamper> fireDampers = HVACFunction.getFireDamperOfDuct(duct);
                //如果没有风阀或者风阀没有在穿越点附近,则在审查结果中标记审查不通过，并将风管加入到审查结果，在风管构件的备注中记录此风管未在穿越点附近设置防火阀
                if (fireDampers.Count < 1)
                {
                    result.isPassCheck = false;
                    string remark = string.Empty;
                    remark = "此风管未设置防火阀";
                    result.AddViolationComponent(duct.Id.Value, "风管", remark);
                    continue;
                }

            }
           

            //如果审查通过
            //则在审查结果批注中注明审查通过相关内容
            if (result.isPassCheck)
            {
                result.comment = "设计满足规范GB50016_2014中第9.3.11条条文规定。";
            }
            //如果审查不通过
            //则在审查结果中注明审查不通过的相关内容
            else
            {
                result.comment = "设计不满足规范GB50016_2014中第9.3.11条条文规定。";
            }
            return result;
        }

        //除敞开式汽车库、建筑面积小于1000m2的地下一层汽车库和修车库外，汽车库、修车库应设置排烟系统，并应划分防烟分区。

        //获得所有的汽车库修车库
        //从汽车库修车库中除去敞开式汽车库及建筑面积小于1000㎡的地下一层汽车库和修车库
        //依次遍历每一个汽车库
        //如果汽车库未设置排烟系统
        //则将审查结果标记为不通过，且把当前房间记录进审查结果中。并在批注中记录此车库未设置排烟系统
        //如果汽车库设置了排烟系统
        //获得汽车库中的防烟分区集合
        //如果集合为空
        //则将审查结果标记为不通过，且把当前房间记录进审查结果中。并在批注中记录此车库未设置防烟分区
        //如果审查通过
        //则在审查结果批注中注明审查通过相关内容
        //如果审查不通过
        //则在审查结果中注明审查不通过的相关内容

        public static BimReview GB50067_2014_8_2_1()
        {
            //初始化审查结果
            BimReview result = new BimReview("GB50067_2014", "8.2.1");
            //获得所有的汽车库修车库
            List<Room> garages = HVACFunction.GetRooms("汽车库");
            garages.AddRange(HVACFunction.GetRooms("修车库"));
            //从汽车库修车库中除去敞开式汽车库及建筑面积小于1000㎡的地下一层汽车库和修车库

            garages = garages.exceptSomeTypeRooms("敞开式汽车库");

            List<Room> groundfloorGarage = assistantFunctions.filtrateElementsBetweenFloor_aAndFloor_b(garages, -1, -1);
            groundfloorGarage.exceptRoomNoSmallerThanArea(1000);
            garages=garages.exceptSameItems(groundfloorGarage);
        // 依次遍历每一个汽车库
            foreach(Room garage in garages)
            {
                //如果汽车库未设置排烟系统
                if(!assistantFunctions.isRoomHaveSomeSystem(garage,"排烟"))
                {
                    //则将审查结果标记为不通过，且把当前房间记录进审查结果中。并在批注中记录此车库未设置排烟系统
                    result.isPassCheck = false;
                    string remark = "此车库未设置排烟系统";
                    result.AddViolationComponent(garage.Id.Value, garage.type, remark);
                }
                //如果汽车库设置了排烟系统
                else
                {
                    //获得汽车库中的防烟分区集合
                    List<SmokeCompartment> smokeCompartments = HVACFunction.GetSmokeCompartmentsInRoom(garage);
                    //如果集合为空
                    if(smokeCompartments.Count==0)
                    {
                        //则将审查结果标记为不通过，且把当前房间记录进审查结果中。并在批注中记录此车库未设置防烟分区
                        result.isPassCheck = false;
                        string remark = "此车库未设置防烟分区";
                        result.AddViolationComponent(garage.Id.Value, garage.type, remark);
                    }
                }
            }
            //如果审查通过
            //则在审查结果批注中注明审查通过相关内容
            if (result.isPassCheck)
            {
                result.comment = "设计满足规范GB50067_2014中第8.2.1条条文规定。";
            }
            //如果审查不通过
            //则在审查结果中注明审查不通过的相关内容
            else
            {
                result.comment = "设计不满足规范GB50067_2014中第8.2.1条条文规定。";
            }

            return result;

        }


        //防烟分区的建筑面积不宜大于2000m2，且防烟分区不应跨越防火分区。防烟分区可采用挡烟垂壁、隔墙或从顶棚下突出不小于0．5m的梁划分。

        //获得所有的汽车库、修车库
        //依次遍历每一个车库
        //获得车库中的所有防烟分区
        //依次判断每一个防烟分区
        //如果防烟分区的面积大于2000㎡
        //则将审查结果标记为不通过，且把当前房间记录进审查结果中。并在批注中记录此车库防烟分区大于2000㎡
        //如果防烟跨越防火分区
        //则将审查结果标记为不通过，且把当前房间记录进审查结果中。并在批注中记录此车库防烟分区跨越防火分区
        //如果审查通过
        //则在审查结果批注中注明审查通过相关内容
        //如果审查不通过
        //则在审查结果中注明审查不通过的相关内容
        public static BimReview GB50067_2014_8_2_2()
        {
            //初始化审查结果
            BimReview result = new BimReview("GB50067_2014", "8.2.2");
            //获得所有的汽车库、修车库
            List<Room> garages = HVACFunction.GetRooms("汽车库");
            garages.AddRange(HVACFunction.GetRooms("修车库"));
            // 依次遍历每一个车库
            foreach(Room garage in garages)
            {
                //获得车库中的所有防烟分区
                List<SmokeCompartment> smokeCompartments = HVACFunction.GetSmokeCompartmentsInRoom(garage);
                //依次判断每一个防烟分区
                foreach (SmokeCompartment smokeCompartmen in smokeCompartments)
                {
                    //如果防烟分区的面积大于2000㎡
                    if(smokeCompartmen.area.Value>2000)
                    {
                        //则将审查结果标记为不通过，且把当前房间记录进审查结果中。并在批注中记录此车库防烟分区大于2000㎡
                        result.isPassCheck = false;
                        string remark = "此车库防烟分区大于2000㎡";
                        result.AddViolationComponent(garage.Id.Value, garage.type, remark);
                    }
                    //如果防烟跨越防火分区
                    else if (assistantFunctions.isSmokeCompartmentSpanFireCompartment(smokeCompartmen))
                    {
                        //则将审查结果标记为不通过，且把当前房间记录进审查结果中。并在批注中记录此车库防烟分区跨越防火分区
                        result.isPassCheck = false;
                        string remark = "此车库防烟分区跨越防火分区";
                        result.AddViolationComponent(garage.Id.Value, garage.type, remark);
                    }
                }
            }
            //如果审查通过
            //则在审查结果批注中注明审查通过相关内容
            if (result.isPassCheck)
            {
                result.comment = "设计满足规范GB50067_2014中第8.2.2条条文规定。请专家复核防烟分区是否采用挡烟垂壁、隔墙或从顶棚下突出不小于0．5m的梁划分";
            }
            //如果审查不通过
            //则在审查结果中注明审查不通过的相关内容
            else
            {
                result.comment = "设计不满足规范GB50067_2014中第8.2.2条条文规定。请专家复核防烟分区是否采用挡烟垂壁、隔墙或从顶棚下突出不小于0．5m的梁划分";
            }

            return result;

        }

        //下列场所应设置机械防烟、排烟设施：
        //1 地下车站的站厅和站台；
        //2 连续长度大于300m的区间隧道和全封闭车道；
        //3 防烟楼梯间和前室。

        //若果建筑类型为地铁建筑
        //获得所有的地下站厅和站台放入需要排烟的房间集合中needSmokeExhaustRooms
        //获得所有长度大于300m的区间隧道和全封闭车道放入需要排烟的房间集合中needSmokeExhaustRooms
        //获得所有的防烟楼梯间和前室并放入需要正压送风的房间中needPressureSupplyRooms
        //依次判断需要排烟的房间中的每一个房间是否设置了机械排烟系统
        //如果没有设置排烟系统则将审查结果标记为不通过，且把当前房间记录进审查结果中。并在批注中记录此房间没有设置排烟系统
        //依次判断需要加压送风房间中的每一个房间是否设置了机械加压送风系统
        //如果没有设置加压送风系统则将审查结果标记为不通过，且把当前房间记录进审查结果中。并在批注中记录此房间没有设置加压送风系统
        //如果审查通过
        //则在审查结果批注中注明审查通过相关内容
        //如果审查不通过
        //则在审查结果中注明审查不通过的相关内容
        public static BimReview GB50157_2013_28_4_2()
        {
            //初始化审查结果
            BimReview result = new BimReview("GB50157_2013", "28.4.2");

            //若果建筑类型为地铁建筑
            if (globalData.buildingType.Contains("地铁建筑"))
            {
                List<Room> needSmokeExhaustRooms = new List<Room>();
                //获得所有的地下站厅和站台放入需要排烟的房间集合中needSmokeExhaustRooms
                needSmokeExhaustRooms.AddRange(HVACFunction.GetRooms("站厅", "", 0, RoomPosition.underground));
                needSmokeExhaustRooms.AddRange(HVACFunction.GetRooms("站台", "", 0, RoomPosition.underground));
                //获得所有长度大于300m的区间隧道和全封闭车道放入需要排烟的房间集合中needSmokeExhaustRooms
                needSmokeExhaustRooms.AddRange(HVACFunction.GetRoomsMoreThan("区域隧道", 300));
                needSmokeExhaustRooms.AddRange(HVACFunction.GetRoomsMoreThan("全封闭车道", 300));
                //获得所有的防烟楼梯间和前室并放入需要正压送风的房间中needPressureSupplyRooms
                List<Room> needPressureSupplyRooms = new List<Room>();
                needPressureSupplyRooms.AddRange(HVACFunction.GetRooms("防烟楼梯间"));
                needPressureSupplyRooms.AddRange(HVACFunction.GetRooms("前室"));
                //依次判断需要排烟的房间中的每一个房间是否设置了机械排烟系统
                foreach(Room room in needSmokeExhaustRooms)
                {
                    //如果没有设置排烟系统则将审查结果标记为不通过，且把当前房间记录进审查结果中。并在批注中记录此房间没有设置排烟系统
                    if (!assistantFunctions.isRoomHaveSomeMechanicalSystem(room, "排烟")) 
                    {
                        
                        result.isPassCheck = false;
                        string remark = "此房间没有设置排烟系统";
                        result.AddViolationComponent(room.Id.Value, room.type, remark);
                    }
                }
                //依次判断需要加压送风房间中的每一个房间是否设置了机械加压送风系统
                foreach (Room room in needPressureSupplyRooms)
                {
                    //如果没有设置加压送风系统则将审查结果标记为不通过，且把当前房间记录进审查结果中。并在批注中记录此房间没有设置加压送风系统
                    if(!assistantFunctions.isRoomHaveSomeMechanicalSystem(room,"加压送风"))
                    {
                        result.isPassCheck = false;
                        string remark = "此房间没有设置加压送风系统";
                        result.AddViolationComponent(room.Id.Value, room.type, remark);
                    }
                }
            }
            //如果审查通过
            //则在审查结果批注中注明审查通过相关内容
            if (result.isPassCheck)
            {
                result.comment = "设计满足规范GB50157_2013中第28.4.2条条文规定。";
            }
            //如果审查不通过
            //则在审查结果中注明审查不通过的相关内容
            else
            {
                result.comment = "设计不满足规范GB50157_2013中第28.4.2条条文规定。";
            }

            return result;
        }

        //当地下车站设备及管理用房、内走道、地下长通道和出入口通道需设置机械排烟时，
        //其排烟量应根据一个防烟分区的建筑面积按1m³/(㎡·min)计算，排烟区域的不风量不应小于排烟量的50%。
        //当排烟设备负担两个或两个以上防烟分区时，其设备能力应根据最大的防烟分区的建筑面积按2m³/(㎡·min)计算的排烟量配置

        //如果建筑类型为城市轨道交通建筑
        //获得地下设备用房及管理用房、内走道、地下长通道和出入口通道
        //依次遍历这些房间
        //如果房间设置了机械排烟系统
        //获得房间内的所有防烟分区对象
        //依次遍历每一个防烟分区
        //通过叠加防烟分区内排烟口的排烟量计算防烟分区的排烟量
        //如果防烟分区的排烟量小于烟分区的建筑面积乘以1m³/(㎡·min)
        //则将审查结果标记为不通过，且把当前防烟分区记录进审查结果中。并在批注中记录此防烟分区排烟量不满足规范要求
        //通过防烟分区内的所有排烟口对象找到负担此防烟分区的所有排烟风机对象。
        //依次遍历这些风机，并记录下这些风机服务了此防烟分区
        //则将审查结果标记为不通过，且把当前风机记录进审查结果中。并在批注中记录此风机排烟量不满足规范要求
        //遍历以上操作所获得的风机与所负担的防烟分区的关系
        //获得风机所负担的所有防烟分区。
        //如果风机负担了两个及两个以上防烟分区
        //获得面积最大的防烟分区
        //如果排烟风机的排烟量小于最大防烟分区面积乘以m³/(㎡·min)
        //则将审查结果标记为不通过，且把当前风机记录进审查结果中。并在批注中记录此风机排烟量不满足规范要求
        //如果风机负担了一个防烟分区

        //如果审查通过
        //则在审查结果批注中注明审查通过相关内容
        //如果审查不通过
        //则在审查结果中注明审查不通过的相关内容
        public static BimReview GB50490_2009_8_4_19()
        {
            //初始化审查结果
            BimReview result = new BimReview("GB50490_2009", "8.4.19");
            //如果建筑类型为城市轨道交通建筑
            if(globalData.buildingType.Contains("城市轨道交通建筑"))
            {
                //获得地下设备用房及管理用房、内走道、地下长通道和出入口通道
                List<Room> rooms = HVACFunction.GetRooms("设备用房", "", 0, RoomPosition.underground);
                rooms.AddRange(HVACFunction.GetRooms("管理用房", "", 0, RoomPosition.underground));
                rooms.AddRange(HVACFunction.GetRooms("走道", "", 0, RoomPosition.underground));
                rooms.AddRange(HVACFunction.GetRooms("长通道", "", 0, RoomPosition.underground));
                rooms.AddRange(HVACFunction.GetRooms("出入口通道", "", 0, RoomPosition.underground));

                Dictionary<Fan, List<SmokeCompartment>> fanAffordSmokeCompartmentsRelation = new Dictionary<Fan, List<SmokeCompartment>>();
                //依次遍历这些房间
                foreach(Room room in rooms)
                {
                    //如果房间设置了机械排烟系统
                    if(assistantFunctions.isRoomHaveSomeMechanicalSystem(room,"排烟"))
                    {
                        //获得房间内的所有防烟分区对象
                        List<SmokeCompartment> smokeCompartments = HVACFunction.GetSmokeCompartmentsInRoom(room);
                        //依次遍历每一个防烟分区
                        foreach(SmokeCompartment smokeCompartment in smokeCompartments)
                        {
                            //通过叠加防烟分区内排烟口的排烟量计算防烟分区的排烟量
                            double smokeExhaustFlow= assistantFunctions.calculateSmokeExhaustFlowOfSmokeCompartment(smokeCompartment);
                            //如果防烟分区的排烟量小于烟分区的建筑面积乘以1m³/(㎡·min)
                            if(smokeExhaustFlow<smokeCompartment.area.Value*60)
                            {
                                //则将审查结果标记为不通过，且把当前防烟分区记录进审查结果中。并在批注中记录此防烟分区排烟量不满足规范要求
                                result.isPassCheck = false;
                                string remark = "防烟分区排烟量不满足规范要求";
                                result.AddViolationComponent(smokeCompartment.Id.Value,"防烟分区", remark);
                            }
                            //通过防烟分区内的所有排烟口对象找到负担此防烟分区的所有排烟风机对象，
                            List<Fan>fans= assistantFunctions.getAllFansConnectToAirTerminals(HVACFunction.GetRoomContainAirTerminal(smokeCompartment));
                            //依次遍历每一台风机
                            foreach(Fan fan in fans)
                            {
                                //更新风机服务的防烟分区列表
                                Fan fanInDictionary = assistantFunctions.findElementFromDictionary(fanAffordSmokeCompartmentsRelation, fan);
                                if (fanInDictionary == null)
                                {
                                    fanAffordSmokeCompartmentsRelation.Add(fan, new List<SmokeCompartment>());
                                    fanInDictionary = fan;
                                }
                                fanAffordSmokeCompartmentsRelation[fanInDictionary].Add(smokeCompartment);
                            }
                            
                        }
                    }

                }

                //遍历以上操作所获得的风机与所负担的防烟分区的关系
                foreach(KeyValuePair<Fan,List<SmokeCompartment>>pair in fanAffordSmokeCompartmentsRelation)
                {
                    Fan fan = pair.Key;
                    List<SmokeCompartment> smokeCompartments = pair.Value;
                    //如果风机负担了两个及两个以上防烟分区
                    if (smokeCompartments.Count >= 2)
                    {
                        //获得风机所负担的所有防烟分区。
                        SmokeCompartment maxSmokeCompartment = assistantFunctions.getMaxAreaSmokeCompartment(smokeCompartments);
                        //如果排烟风机的排烟量小于最大防烟分区面积乘以2m³/(㎡·min)
                        if (fan.m_flowRate < maxSmokeCompartment.area.Value * 2 * 60)
                        {
                            //则将审查结果标记为不通过，且把当前风机记录进审查结果中。并在批注中记录此风机排烟量不满足规范要求
                            result.isPassCheck = false;
                            string remark = "此风机排烟量不满足规范要求";
                            result.AddViolationComponent(fan.Id.Value, "风机", remark);
                        }

                    }
                    //如果风机负担了一个防烟分区
                    else if (smokeCompartments.Count == 1)
                    {
                        //如果排烟风机的排烟量小于防烟分区面积乘以1m³/(㎡·min)
                        if (fan.m_flowRate < smokeCompartments[0].area.Value * 60)
                        {
                            //则将审查结果标记为不通过，且把当前风机记录进审查结果中。并在批注中记录此风机排烟量不满足规范要求
                            result.isPassCheck = false;
                            string remark = "此风机排烟量不满足规范要求";
                            result.AddViolationComponent(fan.Id.Value, "风机", remark);
                        }
                    }

                }

            }

            //如果审查通过
            //则在审查结果批注中注明审查通过相关内容
            if (result.isPassCheck)
            {
                result.comment = "设计满足规范GB50490_2009中第8.4.19条条文规定。";
            }
            //如果审查不通过
            //则在审查结果中注明审查不通过的相关内容
            else
            {
                result.comment = "设计不满足规范GB50490_2009中第8.4.19条条文规定。";
            }

            return result;

        }

        //公共卫生间和浴室通风应符合下列规定：
        //1  公共卫生间应设置机械排风系统。公共浴室宜设气窗；无条件设气窗时，应设独立的机械排风系统。应采取措施保证浴室、卫生间对更衣室以及其他公共区域的负压；
        //2  公共卫生间、浴室及附属房间采用机械通风时，其通风量宜按换气次数确定。

        //获得所有的公共卫生间、公共浴室
        //依次遍历每一个房间
        //如果房间类型为公共卫生间
        //如果没有设置机械排风系统
        //则将审查结果标记为不通过，且把当前房间记录进审查结果中。并在批注中记录公共卫生间没有设置机械排风系统
        //计算房间的排风量（通过叠加房间排风口的风量获得）
        //计算房间的送风量(通过叠加房间的所有送风口及新风口风量获得)
        //如果排风量大于等于送风量
        //则将审查结果标记为不通过，且把当前房间记录进审查结果中。并在批注中记录公共卫生间未保持负压
        //如果房间的换气次数小于5次或大于15次
        //则将审查结果标记为不通过，且把当前房间记录进审查结果中。并在批注中记录公共卫生间换气次数不满足规范要求
        //如果为为公共浴室且没有设置排风系统
        //则将审查结果标记为不通过，且把当前房间记录进审查结果中。并在批注中记录公共浴室没有设置排风系统
        //如果公共浴室设置了机械排风系统
        //计算公共浴室的送风量及排风量
        //如果送风量大于等于排风量
        //则将审查结果标记为不通过，且把当前房间记录进审查结果中。并在批注中记录公共浴室未保持负压
        //如果房间名称为淋浴且换气次数小于5次或者房间名称为浴池且换气次数小于6次或房间为桑拿房且换气次数小于6次
        //则将审查结果标记为不通过，且把当前房间记录进审查结果中。并在批注中记录公共浴室换气次数不满足规范要求
        //如果其他类型公共浴室房间换气次数小于10次
        //则将审查结果标记为不通过，且把当前房间记录进审查结果中。并在批注中记录公共浴室换气次数不满足规范要求
        //如果审查通过
        //则在审查结果批注中注明审查通过相关内容
        //如果审查不通过
        //则在审查结果中注明审查不通过的相关内容
        public static BimReview GB50736_2012_6_3_6()
        {
            //初始化审查结果
            BimReview result = new BimReview("GB50736_2012", "6.3.6");
            //获得所有的公共卫生间、公共浴室
            List<Room> rooms = HVACFunction.GetRooms("公共卫生间");
            rooms.AddRange(HVACFunction.GetRooms("公共浴室"));
            //依次遍历每一个房间
            foreach(Room room in rooms)
            {
                //如果房间类型为公共卫生间
                if(room.type.Contains("公共卫生间"))
                {
                    //如果没有设置机械排风系统
                    if(!assistantFunctions.isRoomHaveSomeMechanicalSystem(room,"排风"))
                    {
                        //则将审查结果标记为不通过，且把当前房间记录进审查结果中。并在批注中记录公共卫生间没有设置机械排风系统
                        result.isPassCheck = false;
                        string remark = "公共卫生间没有设置机械排风系统";
                        result.AddViolationComponent(room.Id.Value, room.type, remark);
                        continue;
                    }
                    //计算房间的排风量（通过叠加房间排风口的风量获得）
                    double exhaustFlow = assistantFunctions.calculateExhaustFlowOfRoom(room);
                    //计算房间的送风量(通过叠加房间的所有送风口及新风口风量获得)
                    double supplyFlow = assistantFunctions.calculateSupplyFlowOfRoom(room);
                    //如果排风量大于等于送风量
                    if(exhaustFlow<=supplyFlow)
                    {
                        //则将审查结果标记为不通过，且把当前房间记录进审查结果中。并在批注中记录公共卫生间未保持负压
                        result.isPassCheck = false;
                        string remark = "公共卫生间未保持负压";
                        result.AddViolationComponent(room.Id.Value, room.type, remark);
                        continue;
                    }
                    //如果房间的换气次数小于5次或大于15次

                    double ventilationRate = exhaustFlow / room.area.Value/room.height.Value;
                    if (ventilationRate<5||ventilationRate>15)
                    {
                        //则将审查结果标记为不通过，且把当前房间记录进审查结果中。并在批注中记录公共卫生间换气次数不满足规范要求
                        result.isPassCheck = false;
                        string remark = "公共卫生间换气次数不满足规范要求";
                        result.AddViolationComponent(room.Id.Value, room.type, remark);
                        continue;
                    }

                }
                //如果为为公共浴室
                if(room.type.Contains("公共浴室"))
                {
                    //如果浴室没有设置排风系统，则将审查结果标记为不通过，且把当前房间记录进审查结果中。并在批注中记录公共浴室没有设置排风系统
                    if(!assistantFunctions.isRoomHaveSomeSystem(room,"排风"))
                    {
                        result.isPassCheck = false;
                        string remark = "公共浴室没有设置排风系统";
                        result.AddViolationComponent(room.Id.Value, room.type, remark);
                        continue;
                    }
                    //如果公共浴室设置了机械排风系统
                    else if(assistantFunctions.isRoomHaveSomeMechanicalSystem(room,"排风"))
                    {
                        //计算公共浴室的送风量及排风量
                        double exhaustFlow = assistantFunctions.calculateExhaustFlowOfRoom(room);
                        double supplyFlow = assistantFunctions.calculateSupplyFlowOfRoom(room);
                        //如果送风量大于等于排风量
                        if (exhaustFlow <= supplyFlow)
                        {
                            //则将审查结果标记为不通过，且把当前房间记录进审查结果中。并在批注中记录公共浴室未保持负压
                            result.isPassCheck = false;
                            string remark = "公共浴室未保持负压";
                            result.AddViolationComponent(room.Id.Value, room.type, remark);
                            continue;
                        }
                        //如果房间名称为淋浴且换气次数小于5次或者房间名称为浴池且换气次数小于6次或房间为桑拿房且换气次数小于6次
                        double ventilationRate = exhaustFlow / room.area.Value / room.height.Value;
                        if(room.name.Contains("淋浴")&&ventilationRate<5||room.name.Contains("浴池")&&ventilationRate<6||room.name.Contains("桑拿房")&&ventilationRate<6)
                        {
                            //则将审查结果标记为不通过，且把当前房间记录进审查结果中。并在批注中记录公共浴室换气次数不满足规范要求
                            result.isPassCheck = false;
                            string remark = "公共浴室换气次数不满足规范要求";
                            result.AddViolationComponent(room.Id.Value, room.type, remark);
                            continue;
                        }
                        //如果其他类型公共浴室房间换气次数小于10次
                        else if (!room.name.Contains("淋浴")&& !room.name.Contains("浴池")&& !room.name.Contains("桑拿房")&& ventilationRate < 10)
                        {
                            //则将审查结果标记为不通过，且把当前房间记录进审查结果中。并在批注中记录公共浴室换气次数不满足规范要求
                            result.isPassCheck = false;
                            string remark = "公共浴室换气次数不满足规范要求";
                            result.AddViolationComponent(room.Id.Value, room.type, remark);
                            continue;
                        }
                    }
                }
            }

            //如果审查通过
            //则在审查结果批注中注明审查通过相关内容
            if (result.isPassCheck)
            {
                result.comment = "设计满足规范GB50736_2012中第6.3.6条条文规定。";
            }
            //如果审查不通过
            //则在审查结果中注明审查不通过的相关内容
            else
            {
                result.comment = "设计不满足规范GB50736_2012中第6.3.6条条文规定。";
            }

            return result;
        }

        //通风空调系统下列部位应设置防火阀：
        //1.风管穿越防火分区的防火墙及楼梯处；
        //2.每层水平干管与垂直总管的交接处；
        //3.穿越变形缝且有隔墙处。

        //获得所有防火分区对象
        //依次遍历每一个防火分区对象
        //获得所有穿越防火分区的风管集合ductsCrossFireCompartment
        //从风管集合ductsCrossFireCompartment中筛选出所有空调、通风管道
        //获得所有的竖井
        //依次遍历每一个竖井，获得竖井内的风管集合ductsInShaft以及穿越竖井的风管的集合ductsCrossShaft
        //从竖井中风管的集合ductsInShaft和穿越竖井的风管集合ductsCrossShaft中筛选出所有空调、通风管道
        //从跨越防火分区的风管集合中除去处于竖井内的风管并将剩余的风管放于ducts集合中。
        //依次遍历穿越竖井的风管集合ductsCrossShaft中的风管
        //判断风管连接的立管是否跨越了防火分区。如果立管跨越了防火分区，则将风管放入ducts中
        //获得所有穿越防火分隔处的变形缝的通风、空调风管并放入ducts
        //依次遍历ducts集合中的每一根风管
        //获得风管上的防火阀
        //如果没有防火阀或者防火阀没有在穿越点附近,则在审查结果中标记审查不通过，并将风管加入到审查结果，在风管构件的备注中记录此风管未在穿越点附近设置防火阀
        //如果审查通过
        //则在审查结果批注中注明审查通过相关内容
        //如果审查不通过
        //则在审查结果中注明审查不通过的相关内容

        public static BimReview GB50157_2013_28_4_22()
        {
            //将审查结果初始化
            BimReview result = new BimReview("GB50157_2013", "28.4.22");

            List<Duct> ducts = new List<Duct>();
            //获得所有防火分区对象
            List<FireCompartment> fireCompartments = HVACFunction.GetFireCompartment("");
            List<Duct> ductsCrossFireCompartment = new List<Duct>();
            //依次遍历每一个防火分区对象
            foreach (FireCompartment fireCompartment in fireCompartments)
            {
                //获得所有穿越防火分区的风管集合ductsCrossFireCompartment
                ductsCrossFireCompartment = ductsCrossFireCompartment.addDuctsToList(HVACFunction.GetDuctsCrossFireDistrict(fireCompartment));
            }
            List<string> systemTypes = new List<string>();
            systemTypes.Add("通风");
            systemTypes.Add("空调");
            //从风管集合ductsCrossFireCompartment中筛选出所有空调、通风管道
            ductsCrossFireCompartment = ductsCrossFireCompartment.filterSomeSystemTypeDuctsFromList(systemTypes);
            //获得所有的竖井
            List<Room> shafts = HVACFunction.GetRooms("竖井");
            //依次遍历每一个竖井，获得竖井内的风管集合ductsInShaft以及穿越竖井的风管的集合ductsCrossShaft
            List<Duct> ductsInShaft = new List<Duct>();
            List<Duct> ductsCrossShaft = new List<Duct>();
            foreach (Room shaft in shafts)
            {
                ductsInShaft.AddRange(HVACFunction.getAllDuctsInRoom(shaft));
                ductsCrossShaft.AddRange(HVACFunction.GetDuctsCrossSpace(shaft));
            }
            //从风管集合ductsInShaft和风管的集合ductsCrossShaft中筛选出所有空调、通风管道
            ductsCrossShaft = ductsCrossShaft.filterSomeSystemTypeDuctsFromList(systemTypes);
            ductsInShaft = ductsInShaft.filterSomeSystemTypeDuctsFromList(systemTypes);
            //从跨越防火分区的风管集合中除去处于竖井内的风管并将剩余的风管放于ducts集合中。
            ducts.addDuctsToList(ductsCrossFireCompartment);
            ducts.removeDuctsFromList(ductsInShaft);
           
            //依次遍历穿越竖井的风管集合ductsCrossShaft中的风管
            foreach (Duct duct in ductsCrossShaft)
            {
                //判断风管连接的立管是否跨越了防火分区。如果立管跨越了防火分区，则将风管放入ducts中
                List<Duct> verticalDucts = HVACFunction.getAllVerticalDuctConnectedToDuct(duct);
                verticalDucts = assistantFunctions.filterSameDuctsInTwoList(verticalDucts, ductsInShaft);
                foreach (Duct verticalDuct in verticalDucts)
                {
                    if (ductsCrossFireCompartment.findItem(verticalDuct) != null)
                    {
                        ducts.Add(duct);
                        break;
                    }
                }
            }

            ducts.addDuctsToList(HVACFunction.GetDuctsCrossMovementJointAndFireSide());
            //依次遍历ducts集合中的每一根风管
            foreach (Duct duct in ducts)
            {
                //获得风管上的防火阀
                List<FireDamper> fireDampers = HVACFunction.getFireDamperOfDuct(duct);
                //如果没有风阀或者风阀没有在穿越点附近,则在审查结果中标记审查不通过，并将风管加入到审查结果，在风管构件的备注中记录此风管未在穿越点附近设置防火阀
                if (fireDampers.Count < 1)
                {
                    result.isPassCheck = false;
                    string remark = string.Empty;
                    remark = "此风管未设置防火阀";
                    result.AddViolationComponent(duct.Id.Value, "风管", remark);
                    continue;
                }
            }

            //如果审查通过
            //则在审查结果批注中注明审查通过相关内容
            if (result.isPassCheck)
            {
                result.comment = "设计满足规范GB50157_2013中第28.4.22条条文规定。";
            }
            //如果审查不通过
            //则在审查结果中注明审查不通过的相关内容
            else
            {
                result.comment = "设计不满足规范GB50157_2013中第28.4.22条条文规定。";
            }
            return result;
        }


        /**
        民用建筑供暖通风与空气调节设计规范 GB50736-2012：5.9.13条文：
        室内供暖系统管道中的热媒流速，应根据系统的水力平衡要求及防噪声要求等因素确定，最大流速不宜超过表5．9．13的限值。
        */
        //初始化审查结果
        //获取所有采暖系统水管管道集合
        //判断集合中的管道是否在室内？未实现
        //在室内的话 
        //管径等于15 流速大于0.8 结果标记为不通过，且把当前水管记录进审查结果中
        //管径等于20 流速大于1.0 结果标记为不通过，且把当前水管记录进审查结果中
        //管径等于25 流速大于1.2 结果标记为不通过，且把当前水管记录进审查结果中
        //管径等于32 流速大于1.4 结果标记为不通过，且把当前水管记录进审查结果中
        //管径等于40 流速大于1.8 结果标记为不通过，且把当前水管记录进审查结果中
        //管径大于50 流速大于2.0 结果标记为不通过，且把当前水管记录进审查结果中
        //如果审查通过
        //则在审查结果批注中注明审查通过相关内容
        //如果审查不通过
        //则在审查结果中注明审查不通过的相关内容
        public static BimReview GB50736_2012_5_9_13()
        {
            //将审查结果初始化
            BimReview result = new BimReview("GB50736_2012", "5.9.13");           
            List<Pipe> heatPipes = new List<Pipe>();
            heatPipes = HVACFunction.GetPipes("采暖系统");                       
            //     
            foreach (Pipe pipe in heatPipes)
            {
                if (pipe.m_DN == 15)
                {
                    if (pipe.m_velocity > 0.8)
                    {
                        result.isPassCheck = false;
                        result.AddViolationComponent(pipe.Id.Value, pipe.type, "");
                    }
                    else
                    {
                        result.isPassCheck = true;
                    }
                }
                else if (pipe.m_DN == 20)
                {
                    if (pipe.m_velocity > 1.0)
                    {
                        result.isPassCheck = false;
                        result.AddViolationComponent(pipe.Id.Value, pipe.type, "");
                    }
                    else
                    {
                        result.isPassCheck = true;
                    }
                }
                else if (pipe.m_DN == 25)
                {
                    if (pipe.m_velocity > 1.2)
                    {
                        result.isPassCheck = false;
                        result.AddViolationComponent(pipe.Id.Value, pipe.type, "");
                    }
                    else
                    {
                        result.isPassCheck = true;
                    }
                }
                else if (pipe.m_DN == 32)
                {
                    if (pipe.m_velocity > 1.4)
                    {
                        result.isPassCheck = false;
                        result.AddViolationComponent(pipe.Id.Value, pipe.type, "");
                    }
                    else
                    {
                        result.isPassCheck = true;
                    }
                }
                else if (pipe.m_DN == 40)
                {
                    if (pipe.m_velocity > 1.8)
                    {
                        result.isPassCheck = false;
                        result.AddViolationComponent(pipe.Id.Value, pipe.type, "");
                    }
                    else
                    {
                        result.isPassCheck = true;
                    }
                }
                else if (pipe.m_DN > 50)
                {
                    if (pipe.m_velocity > 2.0)
                    {
                        result.isPassCheck = false;
                        result.AddViolationComponent(pipe.Id.Value, pipe.type, "");
                    }
                    else
                    {
                        result.isPassCheck = true;
                    }
                }                                                    
            }

            //如果审查通过
            //则在审查结果批注中注明审查通过相关内容
            if (result.isPassCheck)
            {
                result.comment = "设计满足规范GB50736_2012中第5.9.13条条文规定。";
            }
            //如果审查不通过
            //则在审查结果中注明审查不通过的相关内容
            else
            {
                result.comment = "设计不满足规范GB50736_2012中第5.9.13条条文规定。";
            }
            return result;
        }

       


        /**
      民用建筑供暖通风与空气调节设计规范 GB50736-2012：6.6.5条文：
       //机械通风的进排风口风速宜按表6．6．5采用。   （加属性//）
            表6.6.5 机械通风系统的进排风口空气流速(m/s)
            住宅和公共建筑   新风入口 3.5~4.5  风机出口 5.0~10.5
            机房、库房  新风入口 4.5~5.0  风机出口 8.0~14.0
      */
        //初始化审查结果
        //获取所有风口集合
        //遍历每个风口，并获取其包围盒
        //获取所有外墙集合
        //判断风口是否在外墙上，如果在
        //判断风口所在房间；
        //如果房间是机房或者库房 
        //风速在4.5 与5之间，则通过，否则结果标记为不通过，且把当前风口记录进审查结果中
        //风速在8.0 与14之间，则通过，否则结果标记为不通过，且把当前风口记录进审查结果中

        //如果房间类型不是是机房或者库房 
        //风速在4.5 与5之间，则通过，否则结果标记为不通过，且把当前风口记录进审查结果中
        //风速在8.0 与14之间，则通过，否则结果标记为不通过，且把当前风口记录进审查结果中

        //如果审查通过
        //则在审查结果批注中注明审查通过相关内容
        //如果审查不通过
        //则在审查结果中注明审查不通过的相关内容
        public static BimReview GB50736_2012_6_6_5()
        {
            //将审查结果初始化
            BimReview result = new BimReview("GB50736_2012", "6.6.5");         
            List<AirTerminal> airTerminals = new List<AirTerminal>();
            airTerminals = HVACFunction.GetAirterminals();
            //     
            foreach (AirTerminal airterminal in airTerminals)
            {
                PointInt pt1 = new PointInt(0,0,0);
                PointInt pt2 = new PointInt(0, 0, 0);
                string strId = "0";
                AABB aabbAirterminal  = new AABB(pt1,pt2,strId);
                HVACFunction.GetAirTerminalAABB(aabbAirterminal, Convert.ToString( airterminal.Id));                                      
                List<Wall> walls = new List<Wall>();
                walls = HVACFunction.GetOutSideWalls();
                foreach (Wall wall in walls)
                {                 
                    AABB aabbWall = new AABB(pt1, pt2, strId);
                    HVACFunction.GetWallAABB(aabbWall, Convert.ToString(wall.Id));

                    if (Geometry_Utils_BBox.IsBBoxIntersectsBBox3D(aabbWall, aabbAirterminal)&&(!aabbWall.IsAABBContainsAABB3D(aabbAirterminal)))
                    {
                        AirterminalVelocityResult(airterminal, ref result);
                    }             
            }
        }
            //如果审查通过
            //则在审查结果批注中注明审查通过相关内容
            if (result.isPassCheck)
            {
                result.comment = "设计满足规范GB50736_2012中第6.6.5条条文规定。";
            }
            //如果审查不通过
            //则在审查结果中注明审查不通过的相关内容
            else
            {
                result.comment = "设计不满足规范GB50736_2012中第6.6.5条条文规定。";
            }
            return result;
        }


        private static void AirterminalVelocityResult(AirTerminal airterminal, ref BimReview result)
        {
            Room room = HVACFunction.GetRoomOfAirterminal(airterminal);
            if (room.type == "机房" || room.type == "库房")
            {
                if (airterminal.airVelocity > 4.5 && airterminal.airVelocity < 5.0)
                {
                    result.isPassCheck = true;
                }
                else
                {
                    result.isPassCheck = false;

                    result.AddViolationComponent(airterminal.Id.Value, airterminal.systemType, "");
                }
                if (airterminal.airVelocity > 8 && airterminal.airVelocity < 14)
                {
                    result.isPassCheck = true;
                }
                else
                {
                    result.isPassCheck = false;
                    result.AddViolationComponent(airterminal.Id.Value, airterminal.systemType, "");
                }

            }
            else
            {
                if (airterminal.airVelocity > 3.5 && airterminal.airVelocity < 4.5)
                {
                    result.isPassCheck = true;
                }
                else
                {
                    result.isPassCheck = false;
                    result.AddViolationComponent(airterminal.Id.Value, airterminal.systemType, "");
                }
                if (airterminal.airVelocity > 5.0 && airterminal.airVelocity < 10.0)
                {
                    result.isPassCheck = true;
                }
            }
        }


        //民用建筑供暖通风与空气调节设计规范 GB50736-2012：6.6.7条文：        
        //667 风管与通风机及空气处理机组等振动设备的连接处，应装设柔性接头，其长度宜为150mm～300mm。 
        //初始化审查结果
        //获取所有风机集合
        //依次遍历每个风机
        //找到与每个风机找相连的软管集合。
        // 如果与每个风机相连的软管大于2，
        //小于2的话，结果标记为不通过，且把当前风机记录进审查结果中
        //则判断其长度是否在150mm～300mm之间。
        //没在的话，结果标记为不通过，且把当前风机记录进审查结果中

        //获取所有AHU集合
        //依次遍历每个AHU
        //找到与每个AHU找相连的软管集合。
        //如果与每个AHU相连的软管大于2，
        //小于2的话，结果标记为不通过，且把当前AHU记录进审查结果中
        //则判断其长度是否在150mm～300mm之间。
        //没在的话，结果标记为不通过，且把当前AHU记录进审查结果中


        //如果审查通过
        //则在审查结果批注中注明审查通过相关内容
        //如果审查不通过
        //则在审查结果中注明审查不通过的相关内容
        public static BimReview GB50736_2012_6_6_7()
        {
            //初始化审查结果
            BimReview result = new BimReview("GB50736_2012", "6.6.7");

            List<Fan> fans = HVACFunction.GetAllFans();
            foreach(Fan fan in fans)
            {
                List<FlexibleShortTube> flexiTubes = HVACFunction.GetFlexibleShortTubesOfFan(fan);
                if (flexiTubes.Count() > 2)
                {
                    if ((flexiTubes[0].m_length > 150 && flexiTubes[0].m_length < 300) && (flexiTubes[1].m_length > 150 && flexiTubes[1].m_length < 300))
                    {
                        result.isPassCheck = true;
                    }
                    else
                    {
                        result.isPassCheck = false;
                        result.AddViolationComponent(fan.Id.Value, fan.type, "");
                    }
                }
                else
                {
                    result.isPassCheck = false;
                    result.AddViolationComponent(fan.Id.Value, fan.type, "");
                }
            }
           
            List<AssemblyAHU> aHUs = HVACFunction.GetAllAssemblyAHUs();
            foreach (AssemblyAHU aHU in aHUs)
            {
                List<FlexibleShortTube> flexiTubes = HVACFunction.GetFlexibleShortTubesOfAssemblyAHUs(aHU);
                if (flexiTubes.Count() > 2)
                {
                    if ((flexiTubes[0].m_length > 150 && flexiTubes[0].m_length < 300) && (flexiTubes[1].m_length > 150 && flexiTubes[1].m_length < 300))
                    {
                        result.isPassCheck = true;
                    }
                    else
                    {
                        result.isPassCheck = false;
                        result.AddViolationComponent(aHU.Id.Value, aHU.type, "");
                    }
                }
                else
                {
                    result.isPassCheck = false;
                    result.AddViolationComponent(aHU.Id.Value, aHU.type, "");
                }
            }

            if (result.isPassCheck)
            {
                result.comment = "设计满足规范GB50736_2012中第6.6.5条条文规定。";
            }  
            else
            {
                result.comment = "设计不满足规范GB50736_2012中第6.6.5条条文规定。";
            }
            return result;
        }


        //民用建筑供暖通风与空气调节设计规范 GB50736-2012：9.1.5条文：  
        //915锅炉房、换热机房和制冷机房的能量计量应符合下列规定：
        //1  应计量燃料的消耗量；
        //2  应计量耗电量；
        //3  应计量集中供热系统的供热量；
        //4  应计量补水量；
          // 5 new 应计量集中空调系统冷源的供冷量；
          //6  new循环水泵耗电量宜单独计量。

        //获取燃气表集合
        //获取热表集合
        //获取水表集合
        //如果集合里表数都大于0则通过，
        //否则标记为不通过，没有的表记录进审查结果中
        


        //如果审查通过
        //则在审查结果批注中注明审查通过相关内容
        //如果审查不通过
        //则在审查结果中注明审查不通过的相关内容
        public static BimReview GB50736_2012_9_1_5()
        {
            //初始化审查结果
            BimReview result = new BimReview("GB50736_2012", "9.1.5");
            List<GasMeter> gasMeters = HVACFunction.GetGasMeters();
            if (gasMeters.Count() > 0)
            {
                result.isPassCheck = true;
            }
            else
            {
                result.isPassCheck = false;
                result.comment = "设计不满足规范GB50736_2012中第6.6.5条条文规定。 没有计量燃料的消耗量。";
            }
            if(globalData.haveSubentryMeasures)
            {
                result.isPassCheck = true;
            }
            else
            {
                result.isPassCheck = false;
                result.comment = "设计不满足规范GB50736_2012中第6.6.5条条文规定。 没有计量耗电量。";
            }

            List<HeatMeter> heatMeters = HVACFunction.GetHeatMeters();
            if (heatMeters.Count() > 0)
            {
                result.isPassCheck = true;
            }
            else
            {
                result.isPassCheck = false;
                result.comment = "设计不满足规范GB50736_2012中第6.6.5条条文规定。  没有计量集中供热系统的供热量。";
            }
            List<WaterMeter> waterMeters = HVACFunction.GetWaterMeters();
            if (waterMeters.Count() > 0)
            {
                result.isPassCheck = true;
            }
            else
            {
                result.isPassCheck = false;
                result.comment = "设计不满足规范GB50736_2012中第6.6.5条条文规定。  没有计量补水量。";
            }
            if (result.isPassCheck)
            {
                result.comment = "设计满足规范GB50736_2012中第6.6.5条条文规定。";
            }
            //如果审查不通过
            //则在审查结果中注明审查不通过的相关内容
            //else
            //{
            //    result.comment = "设计不满足规范GB50736_2012中第6.6.5条条文规定。";
            //}
            return result;
        }

        //公共建筑节能设计标准 GB 50189-2015
        // 4.5.2锅炉房、换热机房和制冷机房应进行能量计量，能量计量应包括下列内容：
        //1 燃料的消耗量；
        //2 制冷机的耗电量； globle
        //3 集中供热系统的供热量；
        //4 补水量。
        //获取燃气表集合
        //获取热表集合
        //获取水表集合
        //如果集合里表数都大于0则通过，
        //否则标记为不通过，没有的表记录进审查结果中

        //如果审查通过
        //则在审查结果批注中注明审查通过相关内容
        //如果审查不通过
        //则在审查结果中注明审查不通过的相关内容

        public static BimReview GB50189_2015_4_5_2()
        {
            //初始化审查结果
            BimReview result = new BimReview("GB50189_2015", "4.5.2");
            List<GasMeter> gasMeters = HVACFunction.GetGasMeters();
            if(gasMeters.Count()>0)
            {
                result.isPassCheck = true;
            }
            else
            {
                result.isPassCheck = false;
                result.comment = "设计不满足规范GB50736_2012中第6.6.5条条文规定。 没有计量燃料的消耗量。";
            }
            if (globalData.haveSubentryMeasures)
            {
                result.isPassCheck = true;
            }
            else
            {
                result.isPassCheck = false;
                result.comment = "设计不满足规范GB50736_2012中第6.6.5条条文规定。 没有计量耗电量。";
            }
            List<HeatMeter> heatMeters = HVACFunction.GetHeatMeters();
            if (heatMeters.Count() > 0)
            {
                result.isPassCheck = true;
            }
            else
            {
                result.isPassCheck = false;
                result.comment = "设计不满足规范GB50736_2012中第6.6.5条条文规定。  没有计量集中供热系统的供热量。";
            }
            List<WaterMeter> waterMeters = HVACFunction.GetWaterMeters();
            if (waterMeters.Count() > 0)
            {
                result.isPassCheck = true;
            }
            else
            {
                result.isPassCheck = false;                
                result.comment = "设计不满足规范GB50736_2012中第6.6.5条条文规定。没有计量补水量。";
            }
            if (result.isPassCheck)
            {
                result.comment = "设计满足规范GB50736_2012中第6.6.5条条文规定。";
            }          
            //else
            //{
            //    result.comment = "设计不满足规范GB50736_2012中第6.6.5条条文规定。";
            //}
            return result;
        }

        //建筑设计防火规范GB50016-2014
        //9.3.16燃油或燃气锅炉房应设置自然通风或机械通风设施。燃气锅炉房应选用防爆型的事故排风机。当采取机械通风时，机械通风设施应设置导除静电的接地装置，通风量应符合下列规定：
        //1 燃油锅炉房的正常通风量应按换气次数不少于3次／h确定，事故排风量应按换气次数不少于6次／h确定；
        //2 燃气锅炉房的正常通风量应按换气次数不少于6次／h确定，事故排风量应按换气次数不少于12次／h确定。  有可开启外窗 机械通风 加高档风量参数
    
        public static BimReview GB50016_2014_9_3_16()
        {
            //初始化审查结果
            BimReview result = new BimReview("GB50016_2014", "9.3.16");
            string strOil = "燃油";
            string strGas = "燃气";
            List<Room> roomOil = HVACFunction.GetRoomsContainingString(strOil);
            List<Room> roomGas = HVACFunction.GetRoomsContainingString(strGas);
            // List<Room> UnionRooms = roomOil.Concat(roomGas).ToList<Room>();

            CheckRoomVentilationRate(roomOil, ref result, 3);
            CheckRoomVentilationRate(roomGas, ref result, 6);

            return result;
        }
        private static void CheckRoomVentilationRate(List<Room> rooms, ref BimReview result, int iNum)
        {
            foreach (Room room in rooms)
            {
                bool stairCaseHaveMechanicalPressureSystem = assistantFunctions.isRoomHaveNatureVentilateSystem(room);

                //  如果楼梯间采用了机械加压送风系统且机械加压送风系统未设置独立
                if (stairCaseHaveMechanicalPressureSystem)
                {
                    List<AirTerminal> airtermimals = HVACFunction.GetRoomContainAirTerminal(room);
                    foreach (AirTerminal airtermimal in airtermimals)
                    {
                        List<Fan> fans = HVACFunction.GetFanConnectingAirterminal(airtermimal);
                        foreach (Fan fan in fans)
                        {
                            List<AirTerminal> outlets = HVACFunction.GetOutletsOfFan(fan);
                            foreach (AirTerminal outlet in outlets)
                            {
                                if (!airtermimals.Contains(outlet))
                                {
                                    result.isPassCheck = false;
                                }


                            }
                            if (fan.m_flowRate > iNum * room.m_dVolume)
                            {
                                result.isPassCheck = true;
                            }
                            else
                            {
                                result.isPassCheck = false;
                            }
                        }
                    }

                }

            }

        }


        //前室采用自然通风方式时，独立前室、消防电梯前室可开启外窗或开口的面积不应小于2.0m2，
        //共用前室、合用前室不应小于3．0m2。

        //如果审查通过
        //则在审查结果批注中注明审查通过相关内容
        //如果审查不通过
        //则在审查结果中注明审查不通过的相关内容


        public static BimReview GB51251_2017_3_2_2()
        {
            //将审查结果初始化
            BimReview result = new BimReview("GB51251_2017", "3.2.2");


            //获取所有防烟楼梯间集合staircases
            //依次遍历每个防烟楼梯间
            List<Room> independentAnteRooms = new List<Room>();
            List<Room> fireElevatorAnteRooms = new List<Room>();
            independentAnteRooms = HVACFunction.GetRooms("独立前室");
            fireElevatorAnteRooms = HVACFunction.GetRooms("消防电梯前室");
            List<Room> UnionRooms = independentAnteRooms.Concat(fireElevatorAnteRooms).ToList<Room>();
            //     
            foreach (Room stairCase in UnionRooms)
            {
                bool stairCaseHaveMechanicalPressureSystem = assistantFunctions.isRoomHaveSomeNatureSystem(stairCase, "自然通风");

                //  如果楼梯间采用了机械加压送风系统且机械加压送风系统未设置独立
                if (stairCaseHaveMechanicalPressureSystem)
                {
                    //     找到此楼梯间的所有前室atrias
                    List<Window> windows = new List<Window>();
                    windows = HVACFunction.GetWindowsInRoom(stairCase);

                    //     依次遍历每一个前室
                    foreach (Window window in windows)
                    {
                        if (HVACFunction.GetArea(window) - 2.0 > 0.01)
                        {
                            result.isPassCheck = true;
                        }
                        else
                        {
                            result.isPassCheck = false;
                        }
                    }
                }
                else
                {
                    //则将审查结果标记为不通过，且把当前楼梯间记录进审查结果中。
                    result.isPassCheck = false;
                    string remark = string.Empty;
                    result.AddViolationComponent(stairCase.Id.Value, stairCase.type, remark);
                }

            }



            List<Room> sharedAnteRooms = HVACFunction.GetRooms("共用前室");
            List<Room> combinedAnteRooms = HVACFunction.GetRooms("合用前室");
            UnionRooms.Clear();
            UnionRooms = sharedAnteRooms.Concat(combinedAnteRooms).ToList<Room>();

            foreach (Room stairCase in UnionRooms)
            {
                bool stairCaseHaveMechanicalPressureSystem = assistantFunctions.isRoomHaveSomeNatureSystem(stairCase, "自然通风");

                //  如果楼梯间采用了机械加压送风系统且机械加压送风系统未设置独立
                if (stairCaseHaveMechanicalPressureSystem)
                {
                    //     找到此楼梯间的所有前室atrias
                    List<Window> windows = new List<Window>();
                    windows = HVACFunction.GetWindowsInRoom(stairCase);

                    //     依次遍历每一个前室
                    foreach (Window window in windows)
                    {
                        if (HVACFunction.GetArea(window) - 3.0 > 0.01)
                        {
                            result.isPassCheck = true;
                        }
                        else
                        {
                            result.isPassCheck = false;
                        }
                    }
                }
                else
                {
                    //则将审查结果标记为不通过，且把当前楼梯间记录进审查结果中。
                    result.isPassCheck = false;
                    string remark = string.Empty;
                    result.AddViolationComponent(stairCase.Id.Value, stairCase.type, remark);
                }

            }

            //如果审查通过
            //则在审查结果批注中注明审查通过相关内容
            if (result.isPassCheck)
            {
                result.comment = "设计满足规范GB51251_2017中第3.2.2条条文规定。";
            }
            //如果审查不通过
            //则在审查结果中注明审查不通过的相关内容
            else
            {
                result.comment = "设计不满足规范GB51251_2017中第3.2.2条条文规定。";
            }
            return result;
        }

        //建筑防烟排烟系统技术标准
        //323采用自然通风方式的避难层（间）应设有不同朝向的可开启外窗，其有效面积不应小于该避难层（间）地面面积的2％，且每个朝向的面积不应小于2．0m2。加房间TYpe
        public static BimReview GB51251_2017_3_2_3()
        {
            //将审查结果初始化
            BimReview result = new BimReview("GB51251_2017", "3.2.2");
            List<Room> rooms = HVACFunction.GetRooms("避难");

        foreach (Room room in rooms)
        {
                List<Window> windows = HVACFunction.GetWindowsInRoom(room);
                double dAreatotal = 0.0;
                List<string> faceList = new List<string>();
                foreach (Window window in windows)
                {
                    if (!faceList.Exists(x => x == window.sFaceOrient))
                    {
                        faceList.Add(window.sFaceOrient);
                    }
                    dAreatotal += (double)window.effectiveArea;
                }
                if (faceList.Count() < 2)
                {
                    result.isPassCheck = false;
                    result.comment = "设计不满足规范GB51251_2017中第3.2.2条条文规定。没有设置不同朝向的可开启外窗";
                }
                if (dAreatotal < room.m_dArea * 0.02)
                {
                    result.isPassCheck = false;
                    result.comment = "设计不满足规范GB51251_2017中第3.2.2条条文规定。";
                }
            }

        //则在审查结果批注中注明审查通过相关内容
        if (result.isPassCheck)
            {
                result.comment = "设计满足规范GB51251_2017中第3.2.2条条文规定。";
            }
            //如果审查不通过
            //则在审查结果中注明审查不通过的相关内容
        
            return result;   //如果审查通过
     
        }

        //机械加压送风系统应采用管道送风，且不应采用土建风道。送风管道应采用不燃材料制作且内壁应光滑。
        //    当送风管道内壁为金属时，设计风速不应大于20m／s；当送风管道内壁为非金属时，设计风速不应大于15m／s；
        //    送风管道的厚度应符合现行国家标准《通风与空调工程施工质量验收规范》GB 50243的规定。
        //    风口找风机判断土建风道 加风管材料参数 正压送 systemtype风口 正压送风机
        public static BimReview GB51251_2017_3_3_7()
        {
            //将审查结果初始化
            BimReview result = new BimReview("GB51251_2017", "3.3.7");


            List<AirTerminal> airTerminals = HVACFunction.GetAirterminals("机械加压送风");
            foreach (AirTerminal airterminal in airTerminals)
            {
                //duct.mat

                List<Fan> fans = HVACFunction.GetFanConnectingAirterminal(airterminal);
               if( fans.Count < 0)
                {
                    result.isPassCheck = false;

                }
            }

            List<Duct>  ducts = HVACFunction.GetDucts("机械加压送风");
            foreach(Duct duct in ducts)
            {
                //duct.mat
                if(duct.airVelocity>20.0)
                {
                    result.isPassCheck = false;
                }


            }

         

            //如果审查通过
            //则在审查结果批注中注明审查通过相关内容
            if (result.isPassCheck)
            {
                result.comment = "设计满足规范GB51251_2017中第3.2.2条条文规定。";
            }
            //如果审查不通过
            //则在审查结果中注明审查不通过的相关内容
            else
            {
                result.comment = "设计不满足规范GB51251_2017中第3.2.2条条文规定。";
            }
            return result;
        }


        //《建筑防排烟系统技术标准》    GB51251--2017
        //4.24公共建筑、工业建筑防烟分区的最大允许面积及其长边最大允许长度应符合表4．2．4的规定，
        //    当工业建筑采用自然排烟系统时，其防烟分区的长边长度尚不应大于建筑内空间净高的8倍。

        //初始化审查结果
        //判断是否是公共建筑或者工业建筑
        //获取所有防烟分区集合
        //遍历防烟分区集合

        // 防烟分区净高小于3米，面积大于500平米或者长边最大允许长度超高24米
        //则结果标记为不通过，且把当前防烟分区记录进审查结果中

        //防烟分区净高大于3米小于等于6米，面积大于1000平米或者长边最大允许长度超高36米
        //则结果标记为不通过，且把当前防烟分区记录进审查结果中

        //防烟分区净高大于6米，面积大于2000平米或者长边最大允许长度超高60米
        //则结果标记为不通过，且把当前防烟分区记录进审查结果中       


        //如果审查通过
        //则在审查结果批注中注明审查通过相关内容
        //如果审查不通过
        //则在审查结果中注明审查不通过的相关内容

        public static BimReview GB51251_2017_4_2_4()
        {
            //将审查结果初始化
            BimReview result = new BimReview("GB51251_2017", "3.3.7");

            if (globalData.buildingType.Contains("公共建筑") || globalData.buildingType.Contains("工业建筑"))
            {
                List<Room> rooms = HVACFunction.GetRooms("防烟分区");

                foreach (Room room in rooms)
                {
                    if(room.m_dHeight<3.0|| room.m_dHeight == 3.0)
                    {
                       if( room.m_dArea>500 || room.m_dMaxlength > 24)
                        {
                            result.isPassCheck = false;
                        }
                    }
                    else if (room.m_dHeight > 3.0 && ( room.m_dHeight < 6.0|| room.m_dHeight == 6.0))
                    {
                        if (room.m_dArea > 1000 || room.m_dMaxlength > 36)
                        {
                            result.isPassCheck = false;
                        }
                    }
                    else if(room.m_dHeight > 6.0)
                    {
                        if (room.m_dArea > 2000 || room.m_dMaxlength > 60)
                        {
                            result.isPassCheck = false;
                        }
                    }                          
                }
            }

            if ( globalData.buildingType.Contains("工业建筑"))
            {
                List<Room> rooms = HVACFunction.GetRooms("防烟分区");

                foreach (Room room in rooms)
                {
                    if (room.m_dArea > 500 || room.m_dMaxlength > 8 * room.m_dHeight)
                    {
                        result.isPassCheck = false;
                    }
                }
            }

            //如果审查通过
            //则在审查结果批注中注明审查通过相关内容
            if (result.isPassCheck)
            {
                result.comment = "设计满足规范GB51251_2017中第3.2.2条条文规定。";
            }
            //如果审查不通过
            //则在审查结果中注明审查不通过的相关内容
            else
            {
                result.comment = "设计不满足规范GB51251_2017中第3.2.2条条文规定。";
            }
            return result;
        }



        //4．4．7 机械排烟系统应采用管道排烟，且不应采用土建风道。
        //排烟管道应采用不燃材料制作且内壁应光滑。当排烟管道内壁为金属时，管道设计风速不应大于20m／s；
        //当排烟管道内壁为非金属时，管道设计风速不应大于15m／s；
        //排烟管道的厚度应按现行国家标准《通风与空调工程施工质量验收规范》GB 50243的有关规定执行。
        public static BimReview GB51251_2017_4_4_7()
        {
            //将审查结果初始化
            BimReview result = new BimReview("GB51251_2017", "4.4.7");
            List<Duct> ducts = HVACFunction.GetDucts("机械排烟");
            foreach (Duct duct in ducts)
            {
                //duct.mat
                if (duct.airVelocity > 20.0)
                {
                    result.isPassCheck = false;
                }


            }

            //如果审查通过
            //则在审查结果批注中注明审查通过相关内容
            if (result.isPassCheck)
            {
                result.comment = "设计满足规范GB51251_2017中第3.2.2条条文规定。";
            }
            //如果审查不通过
            //则在审查结果中注明审查不通过的相关内容
            else
            {
                result.comment = "设计不满足规范GB51251_2017中第3.2.2条条文规定。";
            }
            return result;
        }

        //《通风与空调工程施工规范》 GB50738-2011
        //8．4．2 风管与设备相连处应设置长度为150mm～300mm的柔性短管，
        //柔性短管安装后应松紧适度，不应扭曲，并不应作为找正、找平的异径连接管。

        //初始化审查结果
        //获取所有风机集合
        //依次遍历每个风机
        //找到与每个风机找相连的软管集合。
        // 如果与每个风机相连的软管大于2，
        //小于2的话，结果标记为不通过，且把当前风机记录进审查结果中
        //则判断其长度是否在150mm～300mm之间。
        //没在的话，结果标记为不通过，且把当前风机记录进审查结果中

        //获取所有AHU集合
        //依次遍历每个AHU
        //找到与每个AHU找相连的软管集合。
        //如果与每个AHU相连的软管大于2，
        //小于2的话，结果标记为不通过，且把当前AHU记录进审查结果中
        //则判断其长度是否在150mm～300mm之间。
        //没在的话，结果标记为不通过，且把当前AHU记录进审查结果中


        //如果审查通过
        //则在审查结果批注中注明审查通过相关内容
        //如果审查不通过
        //则在审查结果中注明审查不通过的相关内容


        public static BimReview GB51251_2017_8_4_2()
        {
            //将审查结果初始化
            BimReview result = new BimReview("GB50736_2017", "8.4.2");
            List<Fan> fans = HVACFunction.GetAllFans();
            foreach (Fan fan in fans)
            {
                List<FlexibleShortTube> flexiTubes = HVACFunction.GetFlexibleShortTubesOfFan(fan);
                if (flexiTubes.Count() > 2)
                {
                    if ((flexiTubes[0].m_length > 150 && flexiTubes[0].m_length < 300) && (flexiTubes[1].m_length > 150 && flexiTubes[1].m_length < 300))
                    {
                        result.isPassCheck = true;
                    }
                    else
                    {
                        result.isPassCheck = false;
                    }
                }
                else
                {
                    result.isPassCheck = false;
                }
            }
            
            List<AssemblyAHU> aHUs = HVACFunction.GetAllAssemblyAHUs();
            foreach (AssemblyAHU fan in aHUs)
            {
                List<FlexibleShortTube> flexiTubes = HVACFunction.GetFlexibleShortTubesOfAssemblyAHUs(fan);
                if (flexiTubes.Count() > 2)
                {
                    if ((flexiTubes[0].m_length > 150 && flexiTubes[0].m_length < 300) && (flexiTubes[1].m_length > 150 && flexiTubes[1].m_length < 300))
                    {
                        result.isPassCheck = true;
                    }
                    else
                    {
                        result.isPassCheck = false;
                    }
                }
                else
                {
                    result.isPassCheck = false;
                }
            }

            if (result.isPassCheck)
            {
                result.comment = "设计满足规范GB50736_2012中第6.6.5条条文规定。";
            }
            //如果审查不通过
            //则在审查结果中注明审查不通过的相关内容
            else
            {
                result.comment = "设计不满足规范GB50736_2012中第6.6.5条条文规定。";
            }
            return result;
        }

        //843风管穿越建筑物变形缝空间时，应设置长度为200mm～300mm的柔性短管(图8．4．3-1)；
        //风管穿越建筑物变形缝墙体时，应设置钢制套管，风管与套管之间应采用柔性防水材料填塞密实。
        //    穿越建筑物变形缝墙体的风管两端外侧应设置长度为150mm～300mm的柔性短管，柔性短管距变形缝墙体的距离宜为150mm～200mm(图8．4．3-2)，
        //柔性短管的保温性能应符合风管系统功能要求。保温不用管，专家审

        public static BimReview GB51251_2017_8_4_3()
        {
            //将审查结果初始化
            BimReview result = new BimReview("GB51251_2017", "3.3.7");
            List<Room> rooms = HVACFunction.GetRooms("避难");

            foreach (Room room in rooms)
            {
                List<Window> windows = HVACFunction.GetWindowsInRoom(room);
                double dAreatotal = 0.0;
                foreach (Window window in windows)
                {
                  //  dAreatotal += window.effectiveArea;
                }
                if (dAreatotal < room.m_dArea * 0.02)
                {
                    result.isPassCheck = false;
                }

            }

            //如果审查通过
            //则在审查结果批注中注明审查通过相关内容
            if (result.isPassCheck)
            {
                result.comment = "设计满足规范GB51251_2017中第3.2.2条条文规定。";
            }
            //如果审查不通过
            //则在审查结果中注明审查不通过的相关内容
            else
            {
                result.comment = "设计不满足规范GB51251_2017中第3.2.2条条文规定。";
            }
            return result;
        }


        //管道穿越结构变形缝处应设置金属柔性短管(图11．1．4-1、图11．1．4-2)，
        //金属柔性短管长度宜为150mm～300mm，并应满足结构变形的要求，其保温性能应符合管道系统功能要求。
        public static BimReview GB51251_2017_11_1_4()
        {
            BimReview result = new BimReview("GB50736_2017", "11.1.4");

            List<Fan> fans = HVACFunction.GetAllFans();
            foreach (Fan fan in fans)
            {
                List<FlexibleShortTube> flexiTubes = HVACFunction.GetFlexibleShortTubesOfFan(fan);
                if (flexiTubes.Count() > 2)
                {
                    if ((flexiTubes[0].m_length > 150 && flexiTubes[0].m_length < 300) && (flexiTubes[1].m_length > 150 && flexiTubes[1].m_length < 300))
                    {
                        result.isPassCheck = true;
                    }
                    else
                    {
                        result.isPassCheck = false;
                    }
                }
                else
                {
                    result.isPassCheck = false;
                }
            }

            List<AssemblyAHU> aHUs = HVACFunction.GetAllAssemblyAHUs();
            foreach (AssemblyAHU fan in aHUs)
            {
                List<FlexibleShortTube> flexiTubes = HVACFunction.GetFlexibleShortTubesOfAssemblyAHUs(fan);
                if (flexiTubes.Count() > 2)
                {
                    if ((flexiTubes[0].m_length > 150 && flexiTubes[0].m_length < 300) && (flexiTubes[1].m_length > 150 && flexiTubes[1].m_length < 300))
                    {
                        result.isPassCheck = true;
                    }
                    else
                    {
                        result.isPassCheck = false;
                    }
                }
                else
                {
                    result.isPassCheck = false;
                }
            }

            if (result.isPassCheck)
            {
                result.comment = "设计满足规范GB50736_2012中第6.6.5条条文规定。";
            }
            //如果审查不通过
            //则在审查结果中注明审查不通过的相关内容
            else
            {
                result.comment = "设计不满足规范GB50736_2012中第6.6.5条条文规定。";
            }
            return result;
        }

        //城市轨道交通技术规范GB 50490-2009 
        // 8．4．17 地下车站站厅、站台公共区和设备及管理用房应划分防烟分区，且防烟分区不应跨越防火分区。
        //站厅、站台公共区每个防烟分区的建筑面积不应超过2000m2，设备及管理用房每个防烟分区的建筑面积不应超过750m2。

        //初始化审查结果
        //获取所有站厅防烟分区集合
        //获取所有站台公共区防烟分区集合
        //合并站厅与站台公共区防烟分区集合。
        //遍历合并后集合，判断建筑面积是否超过2000平米，
        //超过的话，结果标记为不通过，且把当前防烟分区记录进审查结果中

        //获取所有设备防烟分区集合
        //获取所有管理用房防烟分区集合
        //合并设备与管理用房防烟分区集合。
        //遍历合并后集合，判断建筑面积是否超过2000平米，
        //超过的话，结果标记为不通过，且把当前防烟分区记录进审查结果中

        //如果审查通过
        //则在审查结果批注中注明审查通过相关内容
        //如果审查不通过
        //则在审查结果中注明审查不通过的相关内容

        public static BimReview GB50490_2009_8_4_17()
        {
            //将审查结果初始化
            BimReview result = new BimReview("GB50490_2009", "8.4.17");
            string sName = "站厅";
            List<SmokeCompartment> smokeCompartments = HVACFunction.GetSmokeCompartment(sName);
            sName = "站台公共区";
            List<SmokeCompartment> smokeCompartment1s = HVACFunction.GetSmokeCompartment(sName);
            List<SmokeCompartment> unionSmokeCompartments =  smokeCompartments.Concat(smokeCompartment1s).ToList<SmokeCompartment>();

            foreach (SmokeCompartment smokeCompartment in unionSmokeCompartments)
            {               
                if (smokeCompartment.m_dArea > 2000.0)
                {
                    result.isPassCheck = false;
                    result.AddViolationComponent(smokeCompartment.Id.Value, smokeCompartment.type, "");
                }
            }


            sName = "设备";
            List<SmokeCompartment> smokeCompartmentEqus = HVACFunction.GetSmokeCompartment(sName);
            sName = "管理用房";
            List<SmokeCompartment> smokeCompartment1Manges = HVACFunction.GetSmokeCompartment(sName);
            List<SmokeCompartment> union1SmokeCompartments = smokeCompartmentEqus.Concat(smokeCompartment1Manges).ToList<SmokeCompartment>();

            foreach (SmokeCompartment smokeCompartment in union1SmokeCompartments)
            {
                if (smokeCompartment.m_dArea > 750.0)
                {
                    result.isPassCheck = false;
                    result.AddViolationComponent(smokeCompartment.Id.Value, smokeCompartment.type, "");
                }
            }

            result.isPassCheck = true;

            //如果审查通过
            //则在审查结果批注中注明审查通过相关内容
            if (result.isPassCheck)
            {
                result.comment = "设计满足规范GB51251_2017中第3.2.2条条文规定。";
            }
            //如果审查不通过
            //则在审查结果中注明审查不通过的相关内容
            else
            {
                result.comment = "设计不满足规范GB51251_2017中第3.2.2条条文规定。";
            }
            return result;
        }
    }
        public class modelException : Exception
    {
        public modelException(string message) : base(message)
        { }
    }

}
