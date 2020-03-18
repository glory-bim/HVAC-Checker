﻿using System;
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


                //获取所有丙类生产场所，并放入房间集合中
                rooms_Class_C_productPlant = HVACFunction.GetRooms("丙类生产");


                //  对房间集合中的所有房间进行如下操作
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
                    //将rooms_temp集合中的房间加入到rooms集合中
                    rooms_needSpecialRemark = rooms_temp;
                }

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

                foreach (Room room in rooms_Class_C_productPlant)
                {
                    // 判断他们是否有排烟系统。
                    //  如果没有排烟系统，则在审查结果中记录审查不通过，并把当前房间ID加到审查结果中
                    if (!assistantFunctions.isRoomHaveSomeSystem(room, "排烟"))
                    {
                        result.isPassCheck = false;
                        string remark = string.Empty;
                        remark = "此房间需专家核对是否人员或可燃物较多";
                        result.AddViolationComponent(room.Id.Value, room.type, remark);
                    }
                }

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
                List<Room> greaterThan100sqmF1_3FloorEntertainmentRooms = assistantFunctions.filtrateRoomsBetweenFloor_aAndFloor_b(greaterThan100sqmOvergroundEntertainmentRooms, 1, 3);
                Rooms.AddRange(greaterThan100sqmF1_3FloorEntertainmentRooms);

                //    获得所有歌舞娱乐游艺娱乐场所的房间集合entertainmentRooms。 
                List<Room> entertainmentRooms = HVACFunction.GetRooms("歌舞娱乐放映游艺场所");
                //    从entertainmentRooms中筛选出1到三层的所有歌舞娱乐游艺场所的房间集合F1_3FloorEntertainmentRooms
                List<Room> F1_3FloorEntertainmentRooms = assistantFunctions.filtrateRoomsBetweenFloor_aAndFloor_b(entertainmentRooms, 1, 3);
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
            //依次判断needSmokeExhaustRegions集合中每个区域是否设置了排烟系统，如果没有设置排烟系统，则将这些房间加入到审查结果中
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
                bool stairCaseMechanicalPressureSystemIsIndependent = assistantFunctions.isStairPressureAirSystemIndependent(stairCase);
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



  


        // 5．9．13  室内供暖系统管道中的热媒流速，应根据系统的水力平衡要求及防噪声要求等因素确定，最大流速不宜超过表5．9．13的限值。
        public static BimReview GB50736_2012_5_9_13()
        {
            //将审查结果初始化
            BimReview result = new BimReview("GB50736_2012", "5.9.13");


            //获取所有防烟楼梯间集合staircases
            //依次遍历每个防烟楼梯间
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
                    }
                    else
                    {
                        result.isPassCheck = true;
                    }
                }
                else if (pipe.m_DN == 20)
                {
                    if (pipe.m_velocity > 0.8)
                    {
                        result.isPassCheck = false;
                    }
                    else
                    {
                        result.isPassCheck = true;
                    }
                }
                else if (pipe.m_DN == 25)
                {
                    if (pipe.m_velocity > 0.8)
                    {
                        result.isPassCheck = false;
                    }
                    else
                    {
                        result.isPassCheck = true;
                    }
                }
                else if (pipe.m_DN == 32)
                {
                    if (pipe.m_velocity > 0.8)
                    {
                        result.isPassCheck = false;
                    }
                    else
                    {
                        result.isPassCheck = true;
                    }
                }
                else if (pipe.m_DN == 40)
                {
                    if (pipe.m_velocity > 0.8)
                    {
                        result.isPassCheck = false;
                    }
                    else
                    {
                        result.isPassCheck = true;
                    }
                }
                else if (pipe.m_DN > 50)
                {
                    if (pipe.m_velocity > 0.8)
                    {
                        result.isPassCheck = false;
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

        private static void AirterminalVelocityResult(AirTerminal airterminal, ref BimReview result)
        {
            Room room = HVACFunction.GetRoomOfAirterminal(airterminal);
            if (room.type == "机房" || room.type == "库房")
            {
                if (airterminal.airVelocity > 4.5 && airterminal.airVelocity < 5.0)
                {

                }
                if (airterminal.airVelocity > 8 && airterminal.airVelocity < 14)
                {

                }

            }
            else
            {
                if (airterminal.airVelocity > 5.0 && airterminal.airVelocity < 10.0)
                {

                }
                if (airterminal.airVelocity > 3.5 && airterminal.airVelocity < 4.5)
                {
                    result.isPassCheck = true;
                }
            }
        }

        //机械通风的进排风口风速宜按表6．6．5采用   （加属性//）  
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


                List<Room> rooms = new List<Room>();
                rooms = HVACFunction.GetAllRooms();

                foreach (Room room in rooms)
                {
                    //创建一个连接
                    List<PointIntList> Points = new List<PointIntList>() ;
                    string elementId = "";

                    Polygon2D poly = new Polygon2D(Points, elementId);
                    HVACFunction.GetRoomPolygon(poly, room);
                    PointInt pt = aabbAirterminal.Center();
                    if (!Geometry_Utils_BBox.IsPointInBBox2D(poly, aabbAirterminal.Center())
                        && !Geometry_Utils_BBox.IsBBoxIntersectsBBox3D(poly, aabbAirterminal)
                        && !Geometry_Utils_BBox.IsPointInBBox2D(aabbAirterminal, poly.Center())
                        && !Geometry_Utils_BBox.IsPointInBBox2D(poly, aabbAirterminal.Min)
                        && !Geometry_Utils_BBox.IsPointInBBox2D(poly, aabbAirterminal.Max))
                    {

                        AirterminalVelocityResult(airterminal, ref result);

                    }
                }
                               
                List<Wall> walls = new List<Wall>();
                walls = HVACFunction.GetOutSideWalls();
                foreach (Wall wall in walls)
                {                 
                    AABB aabbWall = new AABB(pt1, pt2, strId);
                    HVACFunction.GetWallAABB(aabbWall, Convert.ToString(wall.Id));

                    if (Geometry_Utils_BBox.IsBBoxIntersectsBBox3D(aabbWall, aabbAirterminal))
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

       //667 风管与通风机及空气处理机组等振动设备的连接处，应装设柔性接头，其长度宜为150mm～300mm。  进出口都有FlexibleShortTubes

        //初始化审查结果
        //如果建筑类型为公共建筑、工业建筑且建筑高度大于50m或者建筑类型为住宅且建筑高度大于100m
        //  则获得建筑中所有防烟楼梯间及前室的集合rooms
        //  依次判断集合rooms中的房间是否使用了机械加压送风系统
        //  如果没有设置机械加压送风系统，则在审查结果中标记审查不通过
        //如果审查通过
        //则在审查结果批注中注明审查通过相关内容
        //如果审查不通过
        //则在审查结果批注中注明审查不通过相关内容
        public static BimReview GB50736_2012_6_6_7()
        {
            //初始化审查结果
            BimReview result = new BimReview("GB50736_2012", "6.6.7");

            List<Fan> fans = HVACFunction.GetAllFans();
            foreach(Fan fan in fans)
            {

                List<FlexibleShortTubes> flexiTubes = HVACFunction.GetFlexibleShortTubesOfFan(fan);
              if(flexiTubes.Count()>2)
                {
                 if((flexiTubes[0].m_length >150 && flexiTubes[0].m_length<300) && (flexiTubes[1].m_length > 150 && flexiTubes[1].m_length < 300))
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

                List<FlexibleShortTubes> flexiTubes = HVACFunction.GetFlexibleShortTubesOfAssemblyAHUs(fan);
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


        //915锅炉房、换热机房和制冷机房的能量计量应符合下列规定：
        //1  应计量燃料的消耗量；
        //2  应计量耗电量；
        //3  应计量集中供热系统的供热量；
        //4  应计量补水量；
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
            }
            List<HeatMeter> heatMeters = HVACFunction.GetHeatMeters();
            if (heatMeters.Count() > 0)
            {
                result.isPassCheck = true;
            }
            else
            {
                result.isPassCheck = false;
            }
            List<WaterMeter> waterMeters = HVACFunction.GetWaterMeters();
            if (waterMeters.Count() > 0)
            {
                result.isPassCheck = true;
            }
            else
            {
                result.isPassCheck = false;
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


    // 452   锅炉房、换热机房和制冷机房应进行能量计量，能量计量应包括下列内容：
    //1 燃料的消耗量；
    //2 制冷机的耗电量； globle
    //3 集中供热系统的供热量；
    //4 补水量。

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
            }
            List<HeatMeter> heatMeters = HVACFunction.GetHeatMeters();
            if (heatMeters.Count() > 0)
            {
                result.isPassCheck = true;
            }
            else
            {
                result.isPassCheck = false;
            }
            List<WaterMeter> waterMeters = HVACFunction.GetWaterMeters();
            if (waterMeters.Count() > 0)
            {
                result.isPassCheck = true;
            }
            else
            {
                result.isPassCheck = false;
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


        //    燃油或燃气锅炉房应设置自然通风或机械通风设施。燃气锅炉房应选用防爆型的事故排风机。当采取机械通风时，机械通风设施应设置导除静电的接地装置，通风量应符合下列规定：
        //1 燃油锅炉房的正常通风量应按换气次数不少于3次／h确定，事故排风量应按换气次数不少于6次／h确定；
        //2 燃气锅炉房的正常通风量应按换气次数不少于6次／h确定，事故排风量应按换气次数不少于12次／h确定。  有可开启外窗 机械通风 加高档风量参数


        private static void CheckRoomVentilationRate(List<Room> rooms, ref BimReview result, int iNum)
        {
            foreach (Room room in rooms)
            {
                bool stairCaseHaveMechanicalPressureSystem = assistantFunctions.isRoomHaveSomeNatureSystem(room, "机械通风");

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
                            if (fan.m_flowRate > iNum * room.m_volume)
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


                //前室采用自然通风方式时，独立前室、消防电梯前室可开启外窗或开口的面积不应小于2.0m2，
                //共用前室、合用前室不应小于3．0m2。


                //独立前室independent anteroom： 只与一部疏散楼梯相连的前室。
                //消防电梯前室
                //共用前室 shared anteroom （居住建筑）剪刀楼梯间的两个楼梯间共用同一前室时的前室。
                //合用前室 combined anteroom 防烟楼梯间前室与消防电梯前室合用时的前室。
                //可开启外窗或开口的面积

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




                //323采用自然通风方式的避难层（间）应设有不同朝向的可开启外窗，其有效面积不应小于该避难层（间）地面面积的2％，且每个朝向的面积不应小于2．0m2。加房间TYpe
                public static BimReview GB51251_2017_3_2_3()
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


            }
        public class modelException : Exception
    {
        public modelException(string message) : base(message)
        { }
    }

}
