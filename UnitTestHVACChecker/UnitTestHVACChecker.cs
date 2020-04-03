using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HVAC_CheckEngine;
using System.Collections.Generic;
using Microsoft.QualityTools.Testing.Fakes;
using NPOI;
using NPOI.SS.UserModel;

namespace UnitTestHVACChecker
{
    [TestClass]
    public class GB50016_2014_8_5_1_Test
    {
        [TestMethod]
        public void test_Correct_multiRoom_notpass()
        {

            using (ShimsContext.Create())
            {

                FakeHVACFunction.testDataTableName = "GB50016_2014_8_5_1顺序测试";
                FakeHVACFunction.systemType = "正压送风";

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomsString = FakeHVACFunction.GetRooms;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomContainAirTerminalRoom = FakeHVACFunction.GetRoomContainAirTerminal;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetFanConnectingAirterminalAirTerminal = FakeHVACFunction.GetFanConnectingAirterminal;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetWindowsInRoomRoom = FakeHVACFunction.GetWindowsInRoom;
                //arrange
                globalData.buildingHeight = 100;
                globalData.buildingType = "住宅";
                string comment = "设计不满足规范GB50016_2014中第8.5.1条条文规定。请专家复核：未设置防烟设施的楼梯间前室或合用前室是否采用敞开的阳台、凹廊，或者前室或合用前室是否具有不同朝向的可开启外窗，且可开启外窗的面积满足自然排烟口的面积要求。";
                bool isPassCheck = false;
                List<ComponentAnnotation> componentViolations = new List<ComponentAnnotation>();


                //打开测试数据文件
                IWorkbook Workbook = WorkbookFactory.Create(ExcelPath);
                //读取测试数据表
                ISheet Sheet = Workbook.GetSheet("GB50016_2014_8_5_1顺序测试");
                List<Room> rooms = new List<Room>();
                //依次读取数据行，并根据数据内容创建房间，并加入房间集合中
                for (int index = 1; index <= Sheet.LastRowNum; ++index)
                {
                    IRow row = (IRow)Sheet.GetRow(index);
                    bool isCurrentRoomPassCheck = row.GetCell(5).BooleanCellValue;
                    if (!isCurrentRoomPassCheck)
                    {
                        long roomId = Convert.ToInt64(row.GetCell(0).ToString());
                        String type = row.GetCell(1).ToString();
                        bool isNeedReCheck = row.GetCell(9).BooleanCellValue;
                        ComponentAnnotation componentAnnotation = new ComponentAnnotation();
                        componentAnnotation.Id = roomId;
                        componentAnnotation.type = type;
                        if (isNeedReCheck)
                            componentAnnotation.remark = "此楼梯间需要专家复核";
                        else
                            componentAnnotation.remark = string.Empty;
                        componentViolations.Add(componentAnnotation);
                    }
                }
                //act
                BimReview result = new BimReview();
                result = HVACChecker.GB50016_2014_8_5_1();

                //assert
                Assert.AreEqual(comment, result.comment);
                Assert.IsFalse(result.isPassCheck);
                Custom_Assert.AreComponentViolationListEqual(componentViolations, result.violationComponents);
            }
        }


        [TestMethod]
        [DeploymentItem(@"D:\wangT\HVAC-Checker\UnitTestHVACChecker\测试数据\测试数据.xlsx")]
        [DataSource("MyExcelDataSource3")]
        public void test_Correct_singleRoom()
        {
            using (ShimsContext.Create())
            {

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomsString = (string type) =>
                {
                    List<Room> Rooms = new List<Room>();
                    string systemType = context.DataRow["房间类型"].ToString();
                    if (systemType == type)
                    {
                        long Id = Convert.ToInt64(context.DataRow["房间ID"].ToString());
                        Room room = new Room(Id);

                        room.type = systemType;
                        Rooms.Add(room);
                    }

                    return Rooms;
                };

                FakeHVACFunction.testDataTableName = "GB50016_2014_8_5_1";
                FakeHVACFunction.systemType = "正压送风";
                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomContainAirTerminalRoom = FakeHVACFunction.GetRoomContainAirTerminal;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetFanConnectingAirterminalAirTerminal = FakeHVACFunction.GetFanConnectingAirterminal;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetWindowsInRoomRoom = FakeHVACFunction.GetWindowsInRoom;
                //arrange
                globalData.buildingHeight = Double.Parse(context.DataRow["楼层高度"].ToString());
                globalData.buildingType = context.DataRow["建筑类型"].ToString();
                string comment = context.DataRow["批注"].ToString();
                bool isPassCheck = Boolean.Parse(context.DataRow["是否通过"].ToString());
                List<ComponentAnnotation> componentViolations = new List<ComponentAnnotation>();

                if (!isPassCheck)
                {
                    ComponentAnnotation componentAnnotation = new ComponentAnnotation();
                    componentAnnotation.Id = Convert.ToInt64(context.DataRow["房间ID"].ToString());
                    componentAnnotation.type = context.DataRow["房间类型"].ToString();
                    bool isNeedReCheck = Boolean.Parse(context.DataRow["是否需要复核"].ToString());
                    if (isNeedReCheck)
                        componentAnnotation.remark = "此楼梯间需要专家复核";
                    else
                        componentAnnotation.remark = string.Empty;
                    componentViolations.Add(componentAnnotation);
                }
                //act
                BimReview result = new BimReview();
                result = HVACChecker.GB50016_2014_8_5_1();

                //assert
                Assert.AreEqual(comment, result.comment);
                Assert.AreEqual(isPassCheck, result.isPassCheck);
                Custom_Assert.AreComponentViolationListEqual(componentViolations, result.violationComponents);
            }

        }


        [TestMethod]
        public void test_Correct_EmptyRooms()
        {
            using (ShimsContext.Create())
            {

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomsString = (string type) =>
                {
                    return new List<Room>();
                };

                FakeHVACFunction.testDataTableName = "GB50016_2014_8_5_1";
                FakeHVACFunction.systemType = "正压送风";
                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomContainAirTerminalRoom = FakeHVACFunction.GetRoomContainAirTerminal;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetFanConnectingAirterminalAirTerminal = FakeHVACFunction.GetFanConnectingAirterminal;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetWindowsInRoomRoom = FakeHVACFunction.GetWindowsInRoom;
                //arrange

                string comment = "设计满足规范GB50016_2014中第8.5.1条条文规定。";
                bool isPassCheck = true;
                List<ComponentAnnotation> componentViolations = new List<ComponentAnnotation>();

                //act
                BimReview result = new BimReview();
                result = HVACChecker.GB50016_2014_8_5_1();

                //assert
                Assert.AreEqual(comment, result.comment);
                Assert.IsTrue(result.isPassCheck);
                Custom_Assert.AreComponentViolationListEqual(componentViolations, result.violationComponents);
            }

        }
        private static string ExcelPath = @"D:\wangT\HVAC-Checker\UnitTestHVACChecker\测试数据\测试数据.xlsx";

        private TestContext context;
        public TestContext TestContext
        {
            get { return context; }
            set { context = value; }
        }
    }

    [TestClass]
    public class GB50016_2014_8_5_2_Test
    {
        [TestMethod]
        public void test_Correct_multiRoom()
        {

            using (ShimsContext.Create())
            {

                FakeHVACFunction.testDataTableName = "GB50016_2014_8_5_2";
                FakeHVACFunction.systemType = "排烟";

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomsString = FakeHVACFunction.GetRooms;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomsStringStringDoubleRoomPosition = FakeHVACFunction.GetRoomsMutiArgu;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomsMoreThanStringDouble = FakeHVACFunction.GetRoomsMoreThan;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomContainAirTerminalRoom = FakeHVACFunction.GetRoomContainAirTerminal;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetFanConnectingAirterminalAirTerminal = FakeHVACFunction.GetFanConnectingAirterminal;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetWindowsInRoomRoom = FakeHVACFunction.GetWindowsInRoom;

                //arrange
                globalData.buildingHeight = 32;
                globalData.buildingType = "丙类厂房";
                string comment = "设计不满足规范GB50016_2014中第8.5.2条条文规定。请专家复核：相关违规房间是否人员长期停留或人员、可燃物较多";
                bool isPassCheck = false;
                List<ComponentAnnotation> componentViolations = new List<ComponentAnnotation>();

                //打开测试数据文件
                IWorkbook Workbook = WorkbookFactory.Create(ExcelPath);
                //读取测试数据表
                ISheet Sheet = Workbook.GetSheet("GB50016_2014_8_5_2");
                List<Room> rooms = new List<Room>();
                //依次读取数据行，并根据数据内容创建房间，并加入房间集合中
                for (int index = 1; index <= Sheet.LastRowNum; ++index)
                {
                    IRow row = (IRow)Sheet.GetRow(index);
                    bool isCurrentRoomPassCheck = row.GetCell(5).BooleanCellValue;
                    if (!isCurrentRoomPassCheck)
                    {
                        long roomId = Convert.ToInt64(row.GetCell(0).ToString());
                        String type = row.GetCell(1).ToString();
                        bool isNeedReCheck = row.GetCell(9).BooleanCellValue;
                        ComponentAnnotation componentAnnotation = new ComponentAnnotation();
                        componentAnnotation.Id = roomId;
                        componentAnnotation.type = type;
                        int remarkType = Convert.ToInt32(row.GetCell(16).ToString());
                        if (isNeedReCheck)
                            if (remarkType == 1)
                                componentAnnotation.remark = "此房间需专家核对是否人员或可燃物较多";
                            else
                                componentAnnotation.remark = "此房间需专家核对是否经常有人停留或可燃物较多";
                        else
                            componentAnnotation.remark = string.Empty;
                        componentViolations.Add(componentAnnotation);
                    }
                }
                //act
                BimReview result = new BimReview();
                result = HVACChecker.GB50016_2014_8_5_2();

                //assert
                Assert.AreEqual(comment, result.comment);
                Assert.IsFalse(result.isPassCheck);
                Custom_Assert.AreComponentViolationListEqual(componentViolations, result.violationComponents);
            }
        }

        [TestMethod]
        [DeploymentItem(@"D:\wangT\HVAC-Checker\UnitTestHVACChecker\测试数据\测试数据.xlsx")]
        [DataSource("MyExcelDataSource4")]
        public void test_Correct_singleRoom()
        {
            using (ShimsContext.Create())
            {
                FakeHVACFunction.testDataTableName = "GB50016_2014_8_5_2逐条测试";
                FakeHVACFunction.systemType = "排烟";



                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomsString = (string roomType) =>
                {
                    List<Room> Rooms = new List<Room>();
                    string Type = context.DataRow["房间类型"].ToString();
                    if (Type.Contains(roomType))
                    {
                        long Id = Convert.ToInt64(context.DataRow["房间ID"].ToString());
                        Room room = new Room(Id);
                        room.type = Type;
                        room.area = Convert.ToDouble(context.DataRow["房间面积"].ToString());
                        room.name = context.DataRow["房间名称"].ToString();
                        room.numberOfPeople = Convert.ToInt32(context.DataRow["房间人数"].ToString());
                        room.roomPosition = (RoomPosition)Convert.ToInt32(context.DataRow["房间位置"].ToString());
                        Rooms.Add(room);
                    }
                    return Rooms;
                };

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomsStringStringDoubleRoomPosition = (string roomType, string roomName, double roomArea, RoomPosition roomPosition) =>
                 {
                     List<Room> Rooms = new List<Room>();
                     string Type = context.DataRow["房间类型"].ToString();
                     double area = Convert.ToDouble(context.DataRow["房间面积"].ToString());
                     string name = context.DataRow["房间名称"].ToString();
                     RoomPosition position = (RoomPosition)Convert.ToInt32(context.DataRow["房间位置"].ToString());
                     if (Type.Contains(roomType) && name.Contains(roomName) && area >= roomArea && ((int)(roomPosition & position) != 0))
                     {
                         long Id = Convert.ToInt64(context.DataRow["房间ID"].ToString());
                         Room room = new Room(Id);
                         room.type = Type;
                         room.area = area;
                         room.name = name;
                         room.numberOfPeople = Convert.ToInt32(context.DataRow["房间人数"].ToString());
                         room.roomPosition = position;
                         Rooms.Add(room);
                     }
                     return Rooms;
                 };

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomsMoreThanStringDouble = (string type,double dLength) =>
                  {
                      List<Room> Rooms = new List<Room>();
                      string roomType = context.DataRow["房间类型"].ToString();
                      double corridorLength = Convert.ToDouble(context.DataRow["走廊长度"].ToString());
                      if (roomType.Contains(type) && corridorLength > dLength )
                      {


                          long Id = Convert.ToInt64(context.DataRow["房间ID"].ToString());
                          Room room = new Room(Id);
                          room.type = roomType;
                          room.name = context.DataRow["房间名称"].ToString();
                          room.area = Convert.ToDouble(context.DataRow["房间面积"].ToString());
                          room.roomPosition = (RoomPosition)Convert.ToInt32(context.DataRow["房间位置"].ToString());
                          room.numberOfPeople = Convert.ToInt32(context.DataRow["房间人数"].ToString());
                          Rooms.Add(room);

                      }
                      return Rooms;
                  };

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomContainAirTerminalRoom = FakeHVACFunction.GetRoomContainAirTerminal;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetFanConnectingAirterminalAirTerminal = FakeHVACFunction.GetFanConnectingAirterminal;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetWindowsInRoomRoom = FakeHVACFunction.GetWindowsInRoom;
                //arrange
                globalData.buildingHeight = Double.Parse(context.DataRow["楼层高度"].ToString());
                globalData.buildingType = context.DataRow["建筑类型"].ToString();
                string comment = context.DataRow["批注"].ToString();
                bool isPassCheck = Boolean.Parse(context.DataRow["是否通过"].ToString());

                List<ComponentAnnotation> componentViolations = new List<ComponentAnnotation>();
                if (!isPassCheck)
                {
                    ComponentAnnotation componentAnnotation = new ComponentAnnotation();
                    componentAnnotation.Id = Convert.ToInt64(context.DataRow["房间ID"].ToString());
                    componentAnnotation.type = context.DataRow["房间类型"].ToString();
                    bool isNeedReCheck = Boolean.Parse(context.DataRow["是否需要复核"].ToString());
                    int remarkType = Convert.ToInt32(context.DataRow["复审类型"].ToString());
                    if (isNeedReCheck)
                        if (remarkType == 1)
                            componentAnnotation.remark = "此房间需专家核对是否人员或可燃物较多";
                        else
                            componentAnnotation.remark = "此房间需专家核对是否经常有人停留或可燃物较多";
                    else
                        componentAnnotation.remark = string.Empty;
                    componentViolations.Add(componentAnnotation);
                }
                //act
                BimReview result = new BimReview();
                result = HVACChecker.GB50016_2014_8_5_2();

                //assert
                Assert.AreEqual(comment, result.comment);
                Assert.AreEqual(isPassCheck, result.isPassCheck);
                Custom_Assert.AreComponentViolationListEqual(componentViolations, result.violationComponents);
            }

        }

        [TestMethod]
        public void test_Correct_EmptyRooms()
        {
            using (ShimsContext.Create())
            {
                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomsString = (string type) =>
                {
                    return new List<Room>();
                };
                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomsStringStringDoubleRoomPosition = (string roomType, string roomName, double roomArea, RoomPosition roomPosition) =>
                {
                    return new List<Room>();
                };

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomsMoreThanStringDouble = (string type,double dLength) =>
                  {
                      return new List<Room>();
                  };
                FakeHVACFunction.testDataTableName = "GB50016_2014_8_5_2";
                FakeHVACFunction.systemType = "排烟";

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomContainAirTerminalRoom = FakeHVACFunction.GetRoomContainAirTerminal;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetFanConnectingAirterminalAirTerminal = FakeHVACFunction.GetFanConnectingAirterminal;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetWindowsInRoomRoom = FakeHVACFunction.GetWindowsInRoom;
                //arrange
                globalData.buildingHeight = 32;
                globalData.buildingType = "丙类厂房";
                string comment = "设计满足规范GB50016_2014中第8.5.2条条文规定。";

                List<ComponentAnnotation> componentViolations = new List<ComponentAnnotation>();

                //act
                BimReview result = new BimReview();
                result = HVACChecker.GB50016_2014_8_5_2();

                //assert
                Assert.AreEqual(comment, result.comment);
                Assert.IsTrue(result.isPassCheck);
                Custom_Assert.AreComponentViolationListEqual(componentViolations, result.violationComponents);
            }

        }

        private TestContext context;
        public TestContext TestContext
        {
            get { return context; }
            set { context = value; }
        }

        private static string ExcelPath = @"D:\wangT\HVAC-Checker\UnitTestHVACChecker\测试数据\测试数据.xlsx";

        private static double error = 0.0001;
    }

    [TestClass]
    public class GB50016_2014_8_5_3_Test
    {
        [TestMethod]
        public void test_Correct_multiRoom()
        {

            using (ShimsContext.Create())
            {

                FakeHVACFunction.testDataTableName = "GB50016_2014_8_5_3";
                FakeHVACFunction.systemType = "排烟";

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomsString = FakeHVACFunction.GetRooms;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomsStringStringDoubleRoomPosition = FakeHVACFunction.GetRoomsMutiArgu;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomsMoreThanStringDouble = FakeHVACFunction.GetRoomsMoreThan;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomContainAirTerminalRoom = FakeHVACFunction.GetRoomContainAirTerminal;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetFanConnectingAirterminalAirTerminal = FakeHVACFunction.GetFanConnectingAirterminal;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetWindowsInRoomRoom = FakeHVACFunction.GetWindowsInRoom;

                //arrange
                globalData.buildingHeight = 32;
                globalData.buildingType = "公共建筑";
                string comment = "设计不满足规范GB50016_2014中第8.5.3条条文规定。请专家复核：相关违规房间是否人员长期停留或可燃物较多";

                List<ComponentAnnotation> componentViolations = new List<ComponentAnnotation>();

                //打开测试数据文件
                IWorkbook Workbook = WorkbookFactory.Create(ExcelPath);
                //读取测试数据表
                ISheet Sheet = Workbook.GetSheet("GB50016_2014_8_5_3");
                List<Room> rooms = new List<Room>();
                //依次读取数据行，并根据数据内容创建房间，并加入房间集合中
                for (int index = 1; index <= Sheet.LastRowNum; ++index)
                {
                    IRow row = (IRow)Sheet.GetRow(index);
                    bool isCurrentRoomPassCheck = row.GetCell(5).BooleanCellValue;
                    if (!isCurrentRoomPassCheck)
                    {
                        long roomId = Convert.ToInt64(row.GetCell(0).ToString());
                        String type = row.GetCell(1).ToString();
                        bool isNeedReCheck = row.GetCell(9).BooleanCellValue;
                        ComponentAnnotation componentAnnotation = new ComponentAnnotation();
                        componentAnnotation.Id = roomId;
                        componentAnnotation.type = type;
                        int remarkType = (int)row.GetCell(16).NumericCellValue;
                        if (isNeedReCheck)
                        {
                            if (remarkType == 1)
                                componentAnnotation.remark = "需专家复核此房间是否人员经常停留";
                            else
                                componentAnnotation.remark = "需专家复核此房间是否人员经常停留或可燃物较多";
                        }
                        else
                            componentAnnotation.remark = string.Empty;
                        componentViolations.Add(componentAnnotation);
                    }
                }
                //act
                BimReview result = new BimReview();
                result = HVACChecker.GB50016_2014_8_5_3();

                //assert
                Assert.AreEqual(comment, result.comment);
                Assert.IsFalse(result.isPassCheck);
                Custom_Assert.AreComponentViolationListEqual(componentViolations, result.violationComponents);
            }
        }

        [TestMethod]
        [DeploymentItem(@"D:\wangT\HVAC-Checker\UnitTestHVACChecker\测试数据\测试数据.xlsx")]
        [DataSource("MyExcelDataSource5")]
        public void test_Correct_singleRoom()
        {
            using (ShimsContext.Create())
            {
                FakeHVACFunction.testDataTableName = "GB50016_2014_8_5_3逐条测试";
                FakeHVACFunction.systemType = "排烟";



                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomsString = (string roomType) =>
                {
                    List<Room> Rooms = new List<Room>();
                    string Type = context.DataRow["房间类型"].ToString();
                    if (Type.Contains(roomType))
                    {
                        long Id = Convert.ToInt64(context.DataRow["房间ID"].ToString());
                        Room room = new Room(Id);
                        room.type = Type;
                        room.area = Convert.ToDouble(context.DataRow["房间面积"].ToString());
                        room.name = context.DataRow["房间名称"].ToString();
                        room.numberOfPeople = Convert.ToInt32(context.DataRow["房间人数"].ToString());
                        room.roomPosition = (RoomPosition)Convert.ToInt32(context.DataRow["房间位置"].ToString());
                        room.storyNo = Convert.ToInt32(context.DataRow["房间楼层编号"].ToString());
                        Rooms.Add(room);
                    }
                    return Rooms;
                };

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomsStringStringDoubleRoomPosition = (string roomType, string roomName, double roomArea, RoomPosition roomPosition) =>
                {
                    List<Room> Rooms = new List<Room>();
                    string Type = context.DataRow["房间类型"].ToString();
                    double area = Convert.ToDouble(context.DataRow["房间面积"].ToString());
                    string name = context.DataRow["房间名称"].ToString();
                    RoomPosition position = (RoomPosition)Convert.ToInt32(context.DataRow["房间位置"].ToString());
                    if (Type.Contains(roomType) && name.Contains(roomName) && area >= roomArea && ((int)(roomPosition & position) != 0))
                    {
                        long Id = Convert.ToInt64(context.DataRow["房间ID"].ToString());
                        Room room = new Room(Id);
                        room.type = Type;
                        room.area = area;
                        room.name = name;
                        room.numberOfPeople = Convert.ToInt32(context.DataRow["房间人数"].ToString());
                        room.roomPosition = position;
                        room.storyNo = Convert.ToInt32(context.DataRow["房间楼层编号"].ToString());
                        Rooms.Add(room);
                    }
                    return Rooms;
                };

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomsMoreThanStringDouble = (string type,double dLength) =>
                {
                    List<Room> Rooms = new List<Room>();
                    string roomType = context.DataRow["房间类型"].ToString();
                    double corridorLength = Convert.ToDouble(context.DataRow["走廊长度"].ToString());
                    if (roomType.Contains(type) && corridorLength > dLength )
                    {


                        long Id = Convert.ToInt64(context.DataRow["房间ID"].ToString());
                        Room room = new Room(Id);
                        room.type = roomType;
                        room.name = context.DataRow["房间名称"].ToString();
                        room.area = Convert.ToDouble(context.DataRow["房间面积"].ToString());
                        room.roomPosition = (RoomPosition)Convert.ToInt32(context.DataRow["房间位置"].ToString());
                        room.numberOfPeople = Convert.ToInt32(context.DataRow["房间人数"].ToString());
                        room.storyNo = Convert.ToInt32(context.DataRow["房间楼层编号"].ToString());
                        Rooms.Add(room);

                    }
                    return Rooms;
                };

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomContainAirTerminalRoom = FakeHVACFunction.GetRoomContainAirTerminal;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetFanConnectingAirterminalAirTerminal = FakeHVACFunction.GetFanConnectingAirterminal;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetWindowsInRoomRoom = FakeHVACFunction.GetWindowsInRoom;
                //arrange
                globalData.buildingHeight = Double.Parse(context.DataRow["楼层高度"].ToString());
                globalData.buildingType = context.DataRow["建筑类型"].ToString();
                string comment = context.DataRow["批注"].ToString();
                bool isPassCheck = Boolean.Parse(context.DataRow["是否通过"].ToString());

                List<ComponentAnnotation> componentViolations = new List<ComponentAnnotation>();
                if (!isPassCheck)
                {
                    ComponentAnnotation componentAnnotation = new ComponentAnnotation();
                    componentAnnotation.Id = Convert.ToInt64(context.DataRow["房间ID"].ToString());
                    componentAnnotation.type = context.DataRow["房间类型"].ToString();
                    bool isNeedReCheck = Boolean.Parse(context.DataRow["是否需要复核"].ToString());
                    int remarkType = Convert.ToInt32(context.DataRow["复审类型"].ToString());
                    if (isNeedReCheck)
                    {
                        if (remarkType == 1)
                            componentAnnotation.remark = "需专家复核此房间是否人员经常停留";
                        else
                            componentAnnotation.remark = "需专家复核此房间是否人员经常停留或可燃物较多";
                    }
                    else
                        componentAnnotation.remark = string.Empty;
                    componentViolations.Add(componentAnnotation);
                }
                //act
                BimReview result = new BimReview();
                result = HVACChecker.GB50016_2014_8_5_3();

                //assert
                Assert.AreEqual(comment, result.comment);
                Assert.AreEqual(isPassCheck, result.isPassCheck);
                Custom_Assert.AreComponentViolationListEqual(componentViolations, result.violationComponents);
            }

        }

        [TestMethod]
        public void test_Correct_EmptyRooms()
        {
            using (ShimsContext.Create())
            {
                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomsString = (string type) =>
                {
                    return new List<Room>();
                };
                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomsStringStringDoubleRoomPosition = (string roomType, string roomName, double roomArea, RoomPosition roomPosition) =>
                {
                    return new List<Room>();
                };

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomsMoreThanStringDouble = (string type,double dLength) =>
                {
                    return new List<Room>();
                };
                FakeHVACFunction.testDataTableName = "GB50016_2014_8_5_3";
                FakeHVACFunction.systemType = "排烟";

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomContainAirTerminalRoom = FakeHVACFunction.GetRoomContainAirTerminal;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetFanConnectingAirterminalAirTerminal = FakeHVACFunction.GetFanConnectingAirterminal;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetWindowsInRoomRoom = FakeHVACFunction.GetWindowsInRoom;
                //arrange
                globalData.buildingHeight = 32;
                globalData.buildingType = "公共建筑";
                string comment = "设计满足规范GB50016_2014中第8.5.3条条文规定。";
                bool isPassCheck = true;
                List<ComponentAnnotation> componentViolations = new List<ComponentAnnotation>();

                //act
                BimReview result = new BimReview();
                result = HVACChecker.GB50016_2014_8_5_3();

                //assert
                Assert.AreEqual(comment, result.comment);
                Assert.IsTrue(isPassCheck);
                Custom_Assert.AreComponentViolationListEqual(componentViolations, result.violationComponents);
            }

        }

        private TestContext context;
        public TestContext TestContext
        {
            get { return context; }
            set { context = value; }
        }

        private static string ExcelPath = @"D:\wangT\HVAC-Checker\UnitTestHVACChecker\测试数据\测试数据.xlsx";

        private static double error = 0.0001;
    }

    [TestClass]
    public class GB50016_2014_8_5_4_Test
    {
        [TestMethod]
        public void test_Correct_multiRegion_unpass()
        {

            using (ShimsContext.Create())
            {

                FakeHVACFunction.testDataTableName = "GB50016_2014_8_5_4多区域";
                FakeHVACFunction.systemType = "排烟";

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomsString = FakeHVACFunction.GetRooms;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomsStringStringDoubleRoomPosition = FakeHVACFunction.GetRoomsMutiArgu;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomsMoreThanStringDouble = FakeHVACFunction.GetRoomsMoreThan;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomContainAirTerminalRoom = FakeHVACFunction.GetRoomContainAirTerminal;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetFanConnectingAirterminalAirTerminal = FakeHVACFunction.GetFanConnectingAirterminal;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetWindowsInRoomRoom = FakeHVACFunction.GetWindowsInRoom;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetConnectedRegion = FakeHVACFunction.GetConnectedRegions;

                //arrange
                globalData.buildingHeight = 32;
                globalData.buildingType = "公共建筑";
                string comment = "设计不满足规范GB50016_2014中第8.5.4条条文规定。请专家复核：相关违规房间是否人员长期停留或可燃物较多";

                List<ComponentAnnotation> componentViolations = new List<ComponentAnnotation>();

                //打开测试数据文件
                IWorkbook Workbook = WorkbookFactory.Create(ExcelPath);
                //读取测试数据表
                ISheet Sheet = Workbook.GetSheet("GB50016_2014_8_5_4多区域");
                List<Room> rooms = new List<Room>();
                //依次读取数据行，并根据数据内容创建房间，并加入房间集合中
                for (int index = 1; index <= Sheet.LastRowNum; ++index)
                {
                    IRow row = (IRow)Sheet.GetRow(index);
                    bool isCurrentRoomPassCheck = row.GetCell(5).BooleanCellValue;
                    if (!isCurrentRoomPassCheck)
                    {
                        long roomId = Convert.ToInt64(row.GetCell(0).ToString());
                        String type = row.GetCell(1).ToString();
                        bool isNeedReCheck = row.GetCell(9).BooleanCellValue;
                        ComponentAnnotation componentAnnotation = new ComponentAnnotation();
                        componentAnnotation.Id = roomId;
                        componentAnnotation.type = type;
                        componentAnnotation.remark = "需专家复核此房间是否人员经常停留或可燃物较多";
                        componentViolations.Add(componentAnnotation);
                    }
                }
                //act
                BimReview result = new BimReview();
                result = HVACChecker.GB50016_2014_8_5_4();

                //assert
                Assert.AreEqual(comment, result.comment);
                Assert.IsFalse(result.isPassCheck);
                Custom_Assert.AreComponentViolationListEqual(componentViolations, result.violationComponents);
            }
        }

        [TestMethod]
        public void test_Correct_multiRegion_pass()
        {

            using (ShimsContext.Create())
            {

                FakeHVACFunction.testDataTableName = "GB50016_2014_8_5_4多区域_通过";
                FakeHVACFunction.systemType = "排烟";

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomsString = FakeHVACFunction.GetRooms;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomsStringStringDoubleRoomPosition = FakeHVACFunction.GetRoomsMutiArgu;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomsMoreThanStringDouble = FakeHVACFunction.GetRoomsMoreThan;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomContainAirTerminalRoom = FakeHVACFunction.GetRoomContainAirTerminal;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetFanConnectingAirterminalAirTerminal = FakeHVACFunction.GetFanConnectingAirterminal;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetWindowsInRoomRoom = FakeHVACFunction.GetWindowsInRoom;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetConnectedRegion = FakeHVACFunction.GetConnectedRegions;

                //arrange
                globalData.buildingHeight = 32;
                globalData.buildingType = "公共建筑";
                string comment = "设计满足规范GB50016_2014中第8.5.4条条文规定。";

                List<ComponentAnnotation> componentViolations = new List<ComponentAnnotation>();

                //打开测试数据文件
                IWorkbook Workbook = WorkbookFactory.Create(ExcelPath);
                //读取测试数据表
                ISheet Sheet = Workbook.GetSheet("GB50016_2014_8_5_4多区域_通过");
                List<Room> rooms = new List<Room>();
                //依次读取数据行，并根据数据内容创建房间，并加入房间集合中
                for (int index = 1; index <= Sheet.LastRowNum; ++index)
                {
                    IRow row = (IRow)Sheet.GetRow(index);
                    bool isCurrentRoomPassCheck = row.GetCell(5).BooleanCellValue;
                    if (!isCurrentRoomPassCheck)
                    {
                        long roomId = Convert.ToInt64(row.GetCell(0).ToString());
                        String type = row.GetCell(1).ToString();
                        bool isNeedReCheck = row.GetCell(9).BooleanCellValue;
                        ComponentAnnotation componentAnnotation = new ComponentAnnotation();
                        componentAnnotation.Id = roomId;
                        componentAnnotation.type = type;
                        componentAnnotation.remark = "需专家复核此房间是否人员经常停留或可燃物较多";
                        componentViolations.Add(componentAnnotation);
                    }
                }
                //act
                BimReview result = new BimReview();
                result = HVACChecker.GB50016_2014_8_5_4();

                //assert
                Assert.AreEqual(comment, result.comment);
                Assert.IsTrue(result.isPassCheck);
                Custom_Assert.AreComponentViolationListEqual(componentViolations, result.violationComponents);
            }
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void test_Correct_multiRegion_illegalRegion()
        {

            using (ShimsContext.Create())
            {

                FakeHVACFunction.testDataTableName = "GB50016_2014_8_5_4多区域_无走廊区域";
                FakeHVACFunction.systemType = "排烟";

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomsString = FakeHVACFunction.GetRooms;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomsStringStringDoubleRoomPosition = FakeHVACFunction.GetRoomsMutiArgu;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomsMoreThanStringDouble = FakeHVACFunction.GetRoomsMoreThan;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomContainAirTerminalRoom = FakeHVACFunction.GetRoomContainAirTerminal;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetFanConnectingAirterminalAirTerminal = FakeHVACFunction.GetFanConnectingAirterminal;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetWindowsInRoomRoom = FakeHVACFunction.GetWindowsInRoom;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetConnectedRegion = FakeHVACFunction.GetConnectedRegions;

                //arrange
                globalData.buildingHeight = 32;
                globalData.buildingType = "公共建筑";
                string comment = "设计不满足规范GB50016_2014中第8.5.4条条文规定。请专家复核：相关违规房间是否人员长期停留或可燃物较多";

                List<ComponentAnnotation> componentViolations = new List<ComponentAnnotation>();

                //打开测试数据文件
                IWorkbook Workbook = WorkbookFactory.Create(ExcelPath);
                //读取测试数据表
                ISheet Sheet = Workbook.GetSheet("GB50016_2014_8_5_4多区域_无走廊区域");
                List<Room> rooms = new List<Room>();
                //依次读取数据行，并根据数据内容创建房间，并加入房间集合中
                for (int index = 1; index <= Sheet.LastRowNum; ++index)
                {
                    IRow row = (IRow)Sheet.GetRow(index);
                    bool isCurrentRoomPassCheck = row.GetCell(5).BooleanCellValue;
                    if (!isCurrentRoomPassCheck)
                    {
                        long roomId = Convert.ToInt64(row.GetCell(0).ToString());
                        String type = row.GetCell(1).ToString();
                        bool isNeedReCheck = row.GetCell(9).BooleanCellValue;
                        ComponentAnnotation componentAnnotation = new ComponentAnnotation();
                        componentAnnotation.Id = roomId;
                        componentAnnotation.type = type;
                        componentAnnotation.remark = "需专家复核此房间是否人员经常停留或可燃物较多";
                        componentViolations.Add(componentAnnotation);
                    }
                }
                //act
                BimReview result = new BimReview();
                result = HVACChecker.GB50016_2014_8_5_4();

                //assert
                Assert.AreEqual(comment, result.comment);
                Assert.IsFalse(result.isPassCheck);
                Custom_Assert.AreComponentViolationListEqual(componentViolations, result.violationComponents);
            }
        }


        private TestContext context;
        public TestContext TestContext
        {
            get { return context; }
            set { context = value; }
        }

        private static string ExcelPath = @"D:\wangT\HVAC-Checker\UnitTestHVACChecker\测试数据\测试数据.xlsx";

        private static double error = 0.0001;
    }

    [TestClass]
    public class GB50016_2014_9_3_11_Test
    {
        [TestMethod]
        public void test_pass()
        {
            using (ShimsContext.Create())
            {
                FakeHVACFunction.inital();

                FakeHVACFunction.ductSheetName_new = "风管(通过)";

             

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomsString = FakeHVACFunction.GetRooms_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetFireCompartmentString = FakeHVACFunction.GetFireCompartment_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetDuctsCrossFireDistrictFireCompartment = FakeHVACFunction.GetDuctsCrossFireDistrict_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.getAllDuctsInRoomRoom = FakeHVACFunction.getAllDuctsInRoom_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetDuctsCrossSpaceRoom = FakeHVACFunction.GetDuctsCrossSpace_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.getAllRoomsHaveFireDoor = FakeHVACFunction.getAllRoomsHaveFireDoor_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.getAllVerticalDuctConnectedToDuctDuct = FakeHVACFunction.getAllVerticalDuctConnectedToDuct_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.getFireDamperOfDuctDuct = FakeHVACFunction.getFireDamperOfDuct_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetDuctsCrossMovementJointAndFireSide = FakeHVACFunction.GetDuctsCrossMovementJointAndFireSide_new;
                //arrange



                string comment = "设计满足规范GB50016_2014中第9.3.11条条文规定。";

                List<ComponentAnnotation> componentViolations = new List<ComponentAnnotation>();
                FakeHVACFunction.ExcelPath_new = @"D:\wangT\HVAC-Checker\UnitTestHVACChecker\测试数据\测试数据_GB550016_2014_9_3_11.xlsx";
                //打开测试数据文件
                string importExcelPath = FakeHVACFunction.ExcelPath_new;
                //打开数据文件
                IWorkbook workbook = WorkbookFactory.Create(importExcelPath);
                //读取数据表格
                ISheet sheet_ducts = workbook.GetSheet(FakeHVACFunction.ductSheetName_new);

                List<Duct> ducts = new List<Duct>();
                //依次读取数据行，并根据数据内容创建房间，并加入房间集合中
                for (int index = 1; index <= sheet_ducts.LastRowNum; ++index)
                {
                    IRow row = (IRow)sheet_ducts.GetRow(index);
                    if (!row.GetCell(sheet_ducts.getColNumber("是否通过")).BooleanCellValue)
                    {
                        long ductId = Convert.ToInt64(row.GetCell(sheet_ducts.getColNumber("ID")).ToString());
                       
                        ComponentAnnotation componentAnnotation = new ComponentAnnotation();
                        componentAnnotation.Id = ductId;
                        componentAnnotation.type = "风管";
                        int commentType = Convert.ToInt32(row.GetCell(sheet_ducts.getColNumber("批注类型")).ToString());
                        if (commentType == 1)
                            componentAnnotation.remark = "此风管未设置防火阀";
                        else if (commentType == 2)
                            componentAnnotation.remark = "此风管未在穿越点附近设置防火阀";
                        else if (commentType == 3)
                            componentAnnotation.remark = "此风管未在穿越变形缝两侧设置防火阀";
                        componentViolations.Add(componentAnnotation);
                    }
                }


                //act
                BimReview result = new BimReview();
                result = HVACChecker.GB50016_2014_9_3_11();


                //assert
                Assert.AreEqual(comment, result.comment);
                Assert.IsTrue(result.isPassCheck);
                Custom_Assert.AreComponentViolationListEqual(componentViolations, result.violationComponents);
            }
        }

        [TestMethod]
        public void test_unpass()
        {
            using (ShimsContext.Create())
            {
                FakeHVACFunction.inital();

                FakeHVACFunction.ductSheetName_new = "风管(不通过)";

                FakeHVACFunction.movementJointName_new = "变形缝(不通过)";

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomsString = FakeHVACFunction.GetRooms_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetFireCompartmentString = FakeHVACFunction.GetFireCompartment_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetDuctsCrossFireDistrictFireCompartment = FakeHVACFunction.GetDuctsCrossFireDistrict_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.getAllDuctsInRoomRoom = FakeHVACFunction.getAllDuctsInRoom_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetDuctsCrossSpaceRoom = FakeHVACFunction.GetDuctsCrossSpace_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.getAllRoomsHaveFireDoor = FakeHVACFunction.getAllRoomsHaveFireDoor_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.getAllVerticalDuctConnectedToDuctDuct = FakeHVACFunction.getAllVerticalDuctConnectedToDuct_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.getFireDamperOfDuctDuct = FakeHVACFunction.getFireDamperOfDuct_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetDuctsCrossMovementJointAndFireSide = FakeHVACFunction.GetDuctsCrossMovementJointAndFireSide_new;
                //arrange



                string comment = "设计不满足规范GB50016_2014中第9.3.11条条文规定。";

                List<ComponentAnnotation> componentViolations = new List<ComponentAnnotation>();
                FakeHVACFunction.ExcelPath_new = @"D:\wangT\HVAC-Checker\UnitTestHVACChecker\测试数据\测试数据_GB550016_2014_9_3_11.xlsx";
                //打开测试数据文件
                string importExcelPath = FakeHVACFunction.ExcelPath_new;
                //打开数据文件
                IWorkbook workbook = WorkbookFactory.Create(importExcelPath);
                //读取数据表格
                ISheet sheet_ducts = workbook.GetSheet(FakeHVACFunction.ductSheetName_new);

                List<Duct> ducts = new List<Duct>();
                //依次读取数据行，并根据数据内容创建房间，并加入房间集合中
                for (int index = 1; index <= sheet_ducts.LastRowNum; ++index)
                {
                    IRow row = (IRow)sheet_ducts.GetRow(index);
                    if (!row.GetCell(sheet_ducts.getColNumber("是否通过")).BooleanCellValue)
                    {
                        long ductId = Convert.ToInt64(row.GetCell(sheet_ducts.getColNumber("ID")).ToString());

                        ComponentAnnotation componentAnnotation = new ComponentAnnotation();
                        componentAnnotation.Id = ductId;
                        componentAnnotation.type = "风管";
                        int commentType = Convert.ToInt32(row.GetCell(sheet_ducts.getColNumber("批注类型")).ToString());
                        if (commentType == 1)
                            componentAnnotation.remark = "此风管未设置防火阀";
                        else if (commentType == 2)
                            componentAnnotation.remark = "此风管未在穿越变形缝两侧设置防火阀";
                        componentViolations.Add(componentAnnotation);
                    }
                }


                //act
                BimReview result = new BimReview();
                result = HVACChecker.GB50016_2014_9_3_11();


                //assert
                Assert.AreEqual(comment, result.comment);
                Assert.IsFalse(result.isPassCheck);
                Custom_Assert.AreComponentViolationListEqual(componentViolations, result.violationComponents);
            }
        }
    }

    [TestClass]
    public class GB51251_2017_3_1_2_Test
    {
        [TestMethod]
        public void test_Correct_multiRoom_public_unpass()
        {

            using (ShimsContext.Create())
            {
                FakeHVACFunction.testDataTableName = "GB51251_2017_3_1_2不通过";
                FakeHVACFunction.systemType = "加压送风";

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomsString = FakeHVACFunction.GetRooms;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomContainAirTerminalRoom = FakeHVACFunction.GetRoomContainAirTerminal;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetFanConnectingAirterminalAirTerminal = FakeHVACFunction.GetFanConnectingAirterminal;

                //arrange
                globalData.buildingHeight = 60;
                globalData.buildingType = "公共建筑";
                string comment = "设计不满足规范GB51251_2017中第3.1.2条条文规定。";

                List<ComponentAnnotation> componentViolations = new List<ComponentAnnotation>();

                //打开测试数据文件
                IWorkbook Workbook = WorkbookFactory.Create(ExcelPath);
                //读取测试数据表
                ISheet Sheet = Workbook.GetSheet("GB51251_2017_3_1_2不通过");
                List<Room> rooms = new List<Room>();
                //依次读取数据行，并根据数据内容创建房间，并加入房间集合中
                for (int index = 1; index <= Sheet.LastRowNum; ++index)
                {
                    IRow row = (IRow)Sheet.GetRow(index);
                    bool isCurrentRoomPassCheck = row.GetCell(5).BooleanCellValue;
                    if (!isCurrentRoomPassCheck)
                    {
                        long roomId = Convert.ToInt64(row.GetCell(0).ToString());
                        String type = row.GetCell(1).ToString();
                        ComponentAnnotation componentAnnotation = new ComponentAnnotation();
                        componentAnnotation.Id = roomId;
                        componentAnnotation.type = type;
                        componentAnnotation.remark = string.Empty;
                        componentViolations.Add(componentAnnotation);
                    }
                }
                //act
                BimReview result = new BimReview();
                result = HVACChecker.GB51251_2017_3_1_2();

                //assert
                Assert.AreEqual(comment, result.comment);
                Assert.IsFalse(result.isPassCheck);
                Custom_Assert.AreComponentViolationListEqual(componentViolations, result.violationComponents);
            }
        }

        [TestMethod]
        public void test_Correct_multiRoom_industry_unpass()
        {

            using (ShimsContext.Create())
            {
                FakeHVACFunction.testDataTableName = "GB51251_2017_3_1_2不通过";
                FakeHVACFunction.systemType = "加压送风";

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomsString = FakeHVACFunction.GetRooms;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomContainAirTerminalRoom = FakeHVACFunction.GetRoomContainAirTerminal;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetFanConnectingAirterminalAirTerminal = FakeHVACFunction.GetFanConnectingAirterminal;

                //arrange
                globalData.buildingHeight = 60;
                globalData.buildingType = "工业";
                string comment = "设计不满足规范GB51251_2017中第3.1.2条条文规定。";

                List<ComponentAnnotation> componentViolations = new List<ComponentAnnotation>();

                //打开测试数据文件
                IWorkbook Workbook = WorkbookFactory.Create(ExcelPath);
                //读取测试数据表
                ISheet Sheet = Workbook.GetSheet("GB51251_2017_3_1_2不通过");
                List<Room> rooms = new List<Room>();
                //依次读取数据行，并根据数据内容创建房间，并加入房间集合中
                for (int index = 1; index <= Sheet.LastRowNum; ++index)
                {
                    IRow row = (IRow)Sheet.GetRow(index);
                    bool isCurrentRoomPassCheck = row.GetCell(5).BooleanCellValue;
                    if (!isCurrentRoomPassCheck)
                    {
                        long roomId = Convert.ToInt64(row.GetCell(0).ToString());
                        String type = row.GetCell(1).ToString();
                        ComponentAnnotation componentAnnotation = new ComponentAnnotation();
                        componentAnnotation.Id = roomId;
                        componentAnnotation.type = type;
                        componentAnnotation.remark = string.Empty;
                        componentViolations.Add(componentAnnotation);
                    }
                }
                //act
                BimReview result = new BimReview();
                result = HVACChecker.GB51251_2017_3_1_2();

                //assert
                Assert.AreEqual(comment, result.comment);
                Assert.IsFalse(result.isPassCheck);
                Custom_Assert.AreComponentViolationListEqual(componentViolations, result.violationComponents);
            }
        }

        [TestMethod]
        public void test_Correct_multiRoom_residence_unpass()
        {

            using (ShimsContext.Create())
            {
                FakeHVACFunction.testDataTableName = "GB51251_2017_3_1_2不通过";
                FakeHVACFunction.systemType = "加压送风";

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomsString = FakeHVACFunction.GetRooms;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomContainAirTerminalRoom = FakeHVACFunction.GetRoomContainAirTerminal;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetFanConnectingAirterminalAirTerminal = FakeHVACFunction.GetFanConnectingAirterminal;

                //arrange
                globalData.buildingHeight = 120;
                globalData.buildingType = "住宅";
                string comment = "设计不满足规范GB51251_2017中第3.1.2条条文规定。";

                List<ComponentAnnotation> componentViolations = new List<ComponentAnnotation>();

                //打开测试数据文件
                IWorkbook Workbook = WorkbookFactory.Create(ExcelPath);
                //读取测试数据表
                ISheet Sheet = Workbook.GetSheet("GB51251_2017_3_1_2不通过");
                List<Room> rooms = new List<Room>();
                //依次读取数据行，并根据数据内容创建房间，并加入房间集合中
                for (int index = 1; index <= Sheet.LastRowNum; ++index)
                {
                    IRow row = (IRow)Sheet.GetRow(index);
                    bool isCurrentRoomPassCheck = row.GetCell(5).BooleanCellValue;
                    if (!isCurrentRoomPassCheck)
                    {
                        long roomId = Convert.ToInt64(row.GetCell(0).ToString());
                        String type = row.GetCell(1).ToString();
                        ComponentAnnotation componentAnnotation = new ComponentAnnotation();
                        componentAnnotation.Id = roomId;
                        componentAnnotation.type = type;
                        componentAnnotation.remark = string.Empty;
                        componentViolations.Add(componentAnnotation);
                    }
                }
                //act
                BimReview result = new BimReview();
                result = HVACChecker.GB51251_2017_3_1_2();

                //assert
                Assert.AreEqual(comment, result.comment);
                Assert.IsFalse(result.isPassCheck);
                Custom_Assert.AreComponentViolationListEqual(componentViolations, result.violationComponents);
            }
        }

        [TestMethod]
        public void test_Correct_multiRoom_public_pass()
        {

            using (ShimsContext.Create())
            {
                FakeHVACFunction.testDataTableName = "GB51251_2017_3_1_2通过";
                FakeHVACFunction.systemType = "加压送风";

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomsString = FakeHVACFunction.GetRooms;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomContainAirTerminalRoom = FakeHVACFunction.GetRoomContainAirTerminal;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetFanConnectingAirterminalAirTerminal = FakeHVACFunction.GetFanConnectingAirterminal;

                //arrange
                globalData.buildingHeight = 120;
                globalData.buildingType = "公共建筑";
                string comment = "设计满足规范GB51251_2017中第3.1.2条条文规定。";

                List<ComponentAnnotation> componentViolations = new List<ComponentAnnotation>();

                //打开测试数据文件
                IWorkbook Workbook = WorkbookFactory.Create(ExcelPath);
                //读取测试数据表
                ISheet Sheet = Workbook.GetSheet("GB51251_2017_3_1_2通过");
                List<Room> rooms = new List<Room>();
                //依次读取数据行，并根据数据内容创建房间，并加入房间集合中
                for (int index = 1; index <= Sheet.LastRowNum; ++index)
                {
                    IRow row = (IRow)Sheet.GetRow(index);
                    bool isCurrentRoomPassCheck = row.GetCell(5).BooleanCellValue;
                    if (!isCurrentRoomPassCheck)
                    {
                        long roomId = Convert.ToInt64(row.GetCell(0).ToString());
                        String type = row.GetCell(1).ToString();
                        ComponentAnnotation componentAnnotation = new ComponentAnnotation();
                        componentAnnotation.Id = roomId;
                        componentAnnotation.type = type;
                        componentAnnotation.remark = string.Empty;
                        componentViolations.Add(componentAnnotation);
                    }
                }
                //act
                BimReview result = new BimReview();
                result = HVACChecker.GB51251_2017_3_1_2();

                //assert
                Assert.AreEqual(comment, result.comment);
                Assert.IsTrue(result.isPassCheck);
                Custom_Assert.AreComponentViolationListEqual(componentViolations, result.violationComponents);
            }
        }
        private static string ExcelPath = @"D:\wangT\HVAC-Checker\UnitTestHVACChecker\测试数据\测试数据.xlsx";

        private static double error = 0.0001;
    }


    [TestClass]
    public class GB51251_2017_3_1_5_Test
    {
        [TestMethod]
        public void test_publicBuilding_pass()
        {

            using (ShimsContext.Create())
            {
                FakeHVACFunction.inital();
                FakeHVACFunction.roomSheetName_new = "房间(通过测试)";


               

        HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomContainAirTerminalRoom = FakeHVACFunction.GetRoomContainAirTerminal_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetFanConnectingAirterminalAirTerminal = FakeHVACFunction.GetFanConnectingAirterminal_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetOutletsOfFanFan = FakeHVACFunction.GetOutputLetsOfFan_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomOfAirterminalAirTerminal = FakeHVACFunction.GetRoomOfAirterminal_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.getDoorsBetweenTwoRoomsRoomRoom = FakeHVACFunction.getDoorsBetweenTwoRooms_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.getConnectedRoomsRoom = FakeHVACFunction.getConnectedRooms_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomsString = FakeHVACFunction.GetRooms_new;
                //arrange
                globalData.buildingHeight = 50;
                globalData.buildingType = "公共建筑";
                string comment = "设计满足规范GB51251_2017中第3.1.5条条文规定。";

                List<ComponentAnnotation> componentViolations = new List<ComponentAnnotation>();
                FakeHVACFunction.ExcelPath_new = @"D:\wangT\HVAC-Checker\UnitTestHVACChecker\测试数据\测试数据_GB51251_2017_3_1_5.xlsx";
                //打开测试数据文件
                string importExcelPath = FakeHVACFunction.ExcelPath_new;
                //打开数据文件
                IWorkbook workbook = WorkbookFactory.Create(importExcelPath);
                //读取数据表格
                ISheet sheet_rooms = workbook.GetSheet("房间(通过测试)");

                List<Room> rooms = new List<Room>();
                //依次读取数据行，并根据数据内容创建房间，并加入房间集合中
                for (int index = 1; index <= sheet_rooms.LastRowNum; ++index)
                {
                    IRow row = (IRow)sheet_rooms.GetRow(index);
                    bool isCurrentRoomPassCheck = row.GetCell(sheet_rooms.getColNumber("是否通过")).BooleanCellValue;
                    if (!isCurrentRoomPassCheck)
                    {
                        long roomId = Convert.ToInt64(row.GetCell(sheet_rooms.getColNumber("ID")).ToString());
                        String type = row.GetCell(sheet_rooms.getColNumber("房间类型")).ToString();
                        ComponentAnnotation componentAnnotation = new ComponentAnnotation();
                        componentAnnotation.Id = roomId;
                        componentAnnotation.type = type;
                        componentAnnotation.remark = string.Empty;
                        componentViolations.Add(componentAnnotation);
                    }
                }
                //act
                BimReview result = new BimReview();
                result = HVACChecker.GB51251_2017_3_1_5();


                //assert
                Assert.AreEqual(comment, result.comment);
                Assert.IsTrue(result.isPassCheck);
                Custom_Assert.AreComponentViolationListEqual(componentViolations, result.violationComponents);
            }
        }

        [TestMethod]
        public void test_publicBuilding_unPass_heighterThan50m()
        {

            using (ShimsContext.Create())
            {
                FakeHVACFunction.inital();
                FakeHVACFunction.roomSheetName_new = "房间(通过测试)";
                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomContainAirTerminalRoom = FakeHVACFunction.GetRoomContainAirTerminal_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetFanConnectingAirterminalAirTerminal = FakeHVACFunction.GetFanConnectingAirterminal_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetOutletsOfFanFan = FakeHVACFunction.GetOutputLetsOfFan_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomOfAirterminalAirTerminal = FakeHVACFunction.GetRoomOfAirterminal_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.getDoorsBetweenTwoRoomsRoomRoom = FakeHVACFunction.getDoorsBetweenTwoRooms_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.getConnectedRoomsRoom = FakeHVACFunction.getConnectedRooms_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomsString = FakeHVACFunction.GetRooms_new;
                //arrange
                globalData.buildingHeight = 51;
                globalData.buildingType = "公共建筑";
                string comment = "设计不满足规范GB51251_2017中第3.1.5条条文规定。";

                List<ComponentAnnotation> componentViolations = new List<ComponentAnnotation>();
                FakeHVACFunction.ExcelPath_new = @"D:\wangT\HVAC-Checker\UnitTestHVACChecker\测试数据\测试数据_GB51251_2017_3_1_5.xlsx";
                //打开测试数据文件
                string importExcelPath = FakeHVACFunction.ExcelPath_new;
                //打开数据文件
                IWorkbook workbook = WorkbookFactory.Create(importExcelPath);
                //读取数据表格
                ISheet sheet_rooms = workbook.GetSheet("房间(通过测试)");

                List<Room> rooms = new List<Room>();
                //依次读取数据行，并根据数据内容创建房间，并加入房间集合中
                IRow row = (IRow)sheet_rooms.GetRow(8);
                long roomId = Convert.ToInt64(row.GetCell(sheet_rooms.getColNumber("ID")).ToString());
                String type = row.GetCell(sheet_rooms.getColNumber("房间类型")).ToString();
                ComponentAnnotation componentAnnotation = new ComponentAnnotation();
                componentAnnotation.Id = roomId;
                componentAnnotation.type = type;
                componentAnnotation.remark = string.Empty;
                componentViolations.Add(componentAnnotation);

                //act
                BimReview result = new BimReview();
                result = HVACChecker.GB51251_2017_3_1_5();


                //assert
                Assert.AreEqual(comment, result.comment);
                Assert.IsFalse(result.isPassCheck);
                Custom_Assert.AreComponentViolationListEqual(componentViolations, result.violationComponents);
            }
        }

        [TestMethod]
        public void test_residenceBuilding_unPass_heighterThan100m()
        {

            using (ShimsContext.Create())
            {
                FakeHVACFunction.inital();
                FakeHVACFunction.roomSheetName_new = "房间(通过测试)";
                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomContainAirTerminalRoom = FakeHVACFunction.GetRoomContainAirTerminal_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetFanConnectingAirterminalAirTerminal = FakeHVACFunction.GetFanConnectingAirterminal_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetOutletsOfFanFan = FakeHVACFunction.GetOutputLetsOfFan_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomOfAirterminalAirTerminal = FakeHVACFunction.GetRoomOfAirterminal_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.getDoorsBetweenTwoRoomsRoomRoom = FakeHVACFunction.getDoorsBetweenTwoRooms_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.getConnectedRoomsRoom = FakeHVACFunction.getConnectedRooms_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomsString = FakeHVACFunction.GetRooms_new;
                //arrange
                globalData.buildingHeight = 101;
                globalData.buildingType = "住宅";
                string comment = "设计不满足规范GB51251_2017中第3.1.5条条文规定。";

                List<ComponentAnnotation> componentViolations = new List<ComponentAnnotation>();
                FakeHVACFunction.ExcelPath_new = @"D:\wangT\HVAC-Checker\UnitTestHVACChecker\测试数据\测试数据_GB51251_2017_3_1_5.xlsx";
                //打开测试数据文件
                string importExcelPath = FakeHVACFunction.ExcelPath_new;
                //打开数据文件
                IWorkbook workbook = WorkbookFactory.Create(importExcelPath);
                //读取数据表格
                ISheet sheet_rooms = workbook.GetSheet("房间(通过测试)");

                List<Room> rooms = new List<Room>();
                //依次读取数据行，并根据数据内容创建房间，并加入房间集合中
                IRow row = (IRow)sheet_rooms.GetRow(8);
                long roomId = Convert.ToInt64(row.GetCell(sheet_rooms.getColNumber("ID")).ToString());
                String type = row.GetCell(sheet_rooms.getColNumber("房间类型")).ToString();
                ComponentAnnotation componentAnnotation = new ComponentAnnotation();
                componentAnnotation.Id = roomId;
                componentAnnotation.type = type;
                componentAnnotation.remark = string.Empty;
                componentViolations.Add(componentAnnotation);

                //act
                BimReview result = new BimReview();
                result = HVACChecker.GB51251_2017_3_1_5();


                //assert
                Assert.AreEqual(comment, result.comment);
                Assert.IsFalse(result.isPassCheck);
                Custom_Assert.AreComponentViolationListEqual(componentViolations, result.violationComponents);
            }
        }

        [TestMethod]
        public void test_residenceBuilding_unPass_multicase()
        {

            using (ShimsContext.Create())
            {
                FakeHVACFunction.inital();
                FakeHVACFunction.roomSheetName_new = "房间(通过不测试)";
                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomContainAirTerminalRoom = FakeHVACFunction.GetRoomContainAirTerminal_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetFanConnectingAirterminalAirTerminal = FakeHVACFunction.GetFanConnectingAirterminal_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetOutletsOfFanFan = FakeHVACFunction.GetOutputLetsOfFan_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomOfAirterminalAirTerminal = FakeHVACFunction.GetRoomOfAirterminal_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.getDoorsBetweenTwoRoomsRoomRoom = FakeHVACFunction.getDoorsBetweenTwoRooms_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.getConnectedRoomsRoom = FakeHVACFunction.getConnectedRooms_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomsString = FakeHVACFunction.GetRooms_new;
                //arrange
                globalData.buildingHeight = 100;
                globalData.buildingType = "住宅";
                string comment = "设计不满足规范GB51251_2017中第3.1.5条条文规定。";

                List<ComponentAnnotation> componentViolations = new List<ComponentAnnotation>();
                FakeHVACFunction.ExcelPath_new = @"D:\wangT\HVAC-Checker\UnitTestHVACChecker\测试数据\测试数据_GB51251_2017_3_1_5.xlsx";
                //打开测试数据文件
                string importExcelPath = FakeHVACFunction.ExcelPath_new;
                //打开数据文件
                IWorkbook workbook = WorkbookFactory.Create(importExcelPath);
                //读取数据表格
                ISheet sheet_rooms = workbook.GetSheet(FakeHVACFunction.roomSheetName_new);

                List<Room> rooms = new List<Room>();
                //依次读取数据行，并根据数据内容创建房间，并加入房间集合中
                for (int index = 1; index <= sheet_rooms.LastRowNum; ++index)
                {
                    IRow row = (IRow)sheet_rooms.GetRow(index);
                    if (!row.GetCell(sheet_rooms.getColNumber("是否通过")).BooleanCellValue)
                    {
                        long roomId = Convert.ToInt64(row.GetCell(sheet_rooms.getColNumber("ID")).ToString());
                        String type = row.GetCell(sheet_rooms.getColNumber("房间类型")).ToString();
                        ComponentAnnotation componentAnnotation = new ComponentAnnotation();
                        componentAnnotation.Id = roomId;
                        componentAnnotation.type = type;
                        componentAnnotation.remark = string.Empty;
                        componentViolations.Add(componentAnnotation);
                    }

                }


                //act
                BimReview result = new BimReview();
                result = HVACChecker.GB51251_2017_3_1_5();


                //assert
                Assert.AreEqual(comment, result.comment);
                Assert.IsFalse(result.isPassCheck);
                Custom_Assert.AreComponentViolationListEqual(componentViolations, result.violationComponents);
            }
        }




    }
    [TestClass]
    public class GB51251_2017_3_2_1_Test
    {
        [TestMethod]
        public void test_pass()
        {
            using (ShimsContext.Create())
            {
                FakeHVACFunction.inital();

                FakeHVACFunction.roomSheetName_new = "房间（通过≤10m）";

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomsString = FakeHVACFunction.GetRooms_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetHighestStoryNoOfRoomRoom = FakeHVACFunction.getHighestStoryNoOfRoom_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetWindowsInRoomRoom = FakeHVACFunction.GetWindowsInRoom_new;

                //arrange
                globalData.buildingHeight = 10;
                globalData.buildingType = "住宅";
                string comment = "设计满足规范GB51251_2017中第3.2.1条条文规定。";

                List<ComponentAnnotation> componentViolations = new List<ComponentAnnotation>();
                FakeHVACFunction.ExcelPath_new = @"D:\wangT\HVAC-Checker\UnitTestHVACChecker\测试数据\测试数据_GB51251_2017_3_2_1.xlsx";
                //打开测试数据文件
                string importExcelPath = FakeHVACFunction.ExcelPath_new;
                //打开数据文件
                IWorkbook workbook = WorkbookFactory.Create(importExcelPath);
                //读取数据表格
                ISheet sheet_rooms = workbook.GetSheet(FakeHVACFunction.roomSheetName_new);

                List<Room> rooms = new List<Room>();
                //依次读取数据行，并根据数据内容创建房间，并加入房间集合中
                for (int index = 1; index <= sheet_rooms.LastRowNum; ++index)
                {
                    IRow row = (IRow)sheet_rooms.GetRow(index);
                    if (!row.GetCell(sheet_rooms.getColNumber("是否通过")).BooleanCellValue)
                    {
                        long roomId = Convert.ToInt64(row.GetCell(sheet_rooms.getColNumber("ID")).ToString());
                        String type = row.GetCell(sheet_rooms.getColNumber("房间类型")).ToString();
                        ComponentAnnotation componentAnnotation = new ComponentAnnotation();
                        componentAnnotation.Id = roomId;
                        componentAnnotation.type = type;
                        componentAnnotation.remark = string.Empty;
                        componentViolations.Add(componentAnnotation);
                    }

                }


                //act
                BimReview result = new BimReview();
                result = HVACChecker.GB51251_2017_3_2_1();


                //assert
                Assert.AreEqual(comment, result.comment);
                Assert.IsTrue(result.isPassCheck);
                Custom_Assert.AreComponentViolationListEqual(componentViolations, result.violationComponents);
            }
        }

        [TestMethod]
        public void test_unpass_lowerThan10m()
        {
            using (ShimsContext.Create())
            {
                FakeHVACFunction.inital();

                FakeHVACFunction.roomSheetName_new = "房间（不通过≤10m）";

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomsString = FakeHVACFunction.GetRooms_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetHighestStoryNoOfRoomRoom = FakeHVACFunction.getHighestStoryNoOfRoom_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetWindowsInRoomRoom = FakeHVACFunction.GetWindowsInRoom_new;

                //arrange
                globalData.buildingHeight = 10;
                globalData.buildingType = "住宅";
                string comment = "可开启外窗设置不满足规范GB51251_2017中第3.2.1条条文规定。请专家复核楼梯间中是否有其他开口满足规范要求";

                List<ComponentAnnotation> componentViolations = new List<ComponentAnnotation>();
                FakeHVACFunction.ExcelPath_new = @"D:\wangT\HVAC-Checker\UnitTestHVACChecker\测试数据\测试数据_GB51251_2017_3_2_1.xlsx";
                //打开测试数据文件
                string importExcelPath = FakeHVACFunction.ExcelPath_new;
                //打开数据文件
                IWorkbook workbook = WorkbookFactory.Create(importExcelPath);
                //读取数据表格
                ISheet sheet_rooms = workbook.GetSheet(FakeHVACFunction.roomSheetName_new);

                List<Room> rooms = new List<Room>();
                //依次读取数据行，并根据数据内容创建房间，并加入房间集合中
                for (int index = 1; index <= sheet_rooms.LastRowNum; ++index)
                {
                    IRow row = (IRow)sheet_rooms.GetRow(index);
                    if (!row.GetCell(sheet_rooms.getColNumber("是否通过")).BooleanCellValue)
                    {
                        long roomId = Convert.ToInt64(row.GetCell(sheet_rooms.getColNumber("ID")).ToString());
                        String type = row.GetCell(sheet_rooms.getColNumber("房间类型")).ToString();
                        ComponentAnnotation componentAnnotation = new ComponentAnnotation();
                        componentAnnotation.Id = roomId;
                        componentAnnotation.type = type;
                        componentAnnotation.remark = row.GetCell(sheet_rooms.getColNumber("批注")).ToString(); ;
                        componentViolations.Add(componentAnnotation);
                    }

                }


                //act
                BimReview result = new BimReview();
                result = HVACChecker.GB51251_2017_3_2_1();


                //assert
                Assert.AreEqual(comment, result.comment);
                Assert.IsFalse(result.isPassCheck);
                Custom_Assert.AreComponentViolationListEqual(componentViolations, result.violationComponents);
            }
        }

        [TestMethod]
        public void test_pass_higherThan10m()
        {
            using (ShimsContext.Create())
            {
                FakeHVACFunction.inital();

                FakeHVACFunction.roomSheetName_new = "房间（通过＞10m）";

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomsString = FakeHVACFunction.GetRooms_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetHighestStoryNoOfRoomRoom = FakeHVACFunction.getHighestStoryNoOfRoom_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetWindowsInRoomRoom = FakeHVACFunction.GetWindowsInRoom_new;

                //arrange
                globalData.buildingHeight = 11;
                globalData.buildingType = "住宅";
                string comment = "设计满足规范GB51251_2017中第3.2.1条条文规定。";

                List<ComponentAnnotation> componentViolations = new List<ComponentAnnotation>();
                FakeHVACFunction.ExcelPath_new = @"D:\wangT\HVAC-Checker\UnitTestHVACChecker\测试数据\测试数据_GB51251_2017_3_2_1.xlsx";
                //打开测试数据文件
                string importExcelPath = FakeHVACFunction.ExcelPath_new;
                //打开数据文件
                IWorkbook workbook = WorkbookFactory.Create(importExcelPath);
                //读取数据表格
                ISheet sheet_rooms = workbook.GetSheet(FakeHVACFunction.roomSheetName_new);

                List<Room> rooms = new List<Room>();
                //依次读取数据行，并根据数据内容创建房间，并加入房间集合中
                for (int index = 1; index <= sheet_rooms.LastRowNum; ++index)
                {
                    IRow row = (IRow)sheet_rooms.GetRow(index);
                    if (!row.GetCell(sheet_rooms.getColNumber("是否通过")).BooleanCellValue)
                    {
                        long roomId = Convert.ToInt64(row.GetCell(sheet_rooms.getColNumber("ID")).ToString());
                        String type = row.GetCell(sheet_rooms.getColNumber("房间类型")).ToString();
                        ComponentAnnotation componentAnnotation = new ComponentAnnotation();
                        componentAnnotation.Id = roomId;
                        componentAnnotation.type = type;
                        componentAnnotation.remark = row.GetCell(sheet_rooms.getColNumber("批注")).ToString(); ;
                        componentViolations.Add(componentAnnotation);
                    }

                }


                //act
                BimReview result = new BimReview();
                result = HVACChecker.GB51251_2017_3_2_1();


                //assert
                Assert.AreEqual(comment, result.comment);
                Assert.IsTrue(result.isPassCheck);
                Custom_Assert.AreComponentViolationListEqual(componentViolations, result.violationComponents);
            }
        }

        [TestMethod]
        public void test_unpass_higherThan10m()
        {
            using (ShimsContext.Create())
            {
                FakeHVACFunction.inital();

                FakeHVACFunction.roomSheetName_new = "房间（不通过＞10m）";

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomsString = FakeHVACFunction.GetRooms_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetHighestStoryNoOfRoomRoom = FakeHVACFunction.getHighestStoryNoOfRoom_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetWindowsInRoomRoom = FakeHVACFunction.GetWindowsInRoom_new;

                //arrange
                globalData.buildingHeight = 11;
                globalData.buildingType = "住宅";
                string comment = "可开启外窗设置不满足规范GB51251_2017中第3.2.1条条文规定。请专家复核楼梯间中是否有其他开口满足规范要求";

                List<ComponentAnnotation> componentViolations = new List<ComponentAnnotation>();
                FakeHVACFunction.ExcelPath_new = @"D:\wangT\HVAC-Checker\UnitTestHVACChecker\测试数据\测试数据_GB51251_2017_3_2_1.xlsx";
                //打开测试数据文件
                string importExcelPath = FakeHVACFunction.ExcelPath_new;
                //打开数据文件
                IWorkbook workbook = WorkbookFactory.Create(importExcelPath);
                //读取数据表格
                ISheet sheet_rooms = workbook.GetSheet(FakeHVACFunction.roomSheetName_new);

                List<Room> rooms = new List<Room>();
                //依次读取数据行，并根据数据内容创建房间，并加入房间集合中
                for (int index = 1; index <= sheet_rooms.LastRowNum; ++index)
                {
                    IRow row = (IRow)sheet_rooms.GetRow(index);
                    if (!row.GetCell(sheet_rooms.getColNumber("是否通过")).BooleanCellValue)
                    {
                        long roomId = Convert.ToInt64(row.GetCell(sheet_rooms.getColNumber("ID")).ToString());
                        String type = row.GetCell(sheet_rooms.getColNumber("房间类型")).ToString();
                        ComponentAnnotation componentAnnotation = new ComponentAnnotation();
                        componentAnnotation.Id = roomId;
                        componentAnnotation.type = type;
                        componentAnnotation.remark = row.GetCell(sheet_rooms.getColNumber("批注")).ToString(); ;
                        componentViolations.Add(componentAnnotation);
                    }
                }


                //act
                BimReview result = new BimReview();
                result = HVACChecker.GB51251_2017_3_2_1();


                //assert
                Assert.AreEqual(comment, result.comment);
                Assert.IsFalse(result.isPassCheck);
                Custom_Assert.AreComponentViolationListEqual(componentViolations, result.violationComponents);
            }
        }
    }

    [TestClass]
    public class GB51251_2017_3_3_1_Test
    {
        [TestMethod]
        public void test_pass()
        {
            using (ShimsContext.Create())
            {
                FakeHVACFunction.inital();

                FakeHVACFunction.roomSheetName_new = "房间(通过)";

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetHighestStoryNoOfRoomRoom = FakeHVACFunction.getHighestStoryNoOfRoom_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetFloors = FakeHVACFunction.GetAllFLoorsOfBuilding_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomContainAirTerminalRoom = FakeHVACFunction.GetRoomContainAirTerminal_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomsString = FakeHVACFunction.GetRooms_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetOutletsOfFanFan = FakeHVACFunction.GetOutputLetsOfFan_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetFanConnectingAirterminalAirTerminal = FakeHVACFunction.GetFanConnectingAirterminal_new;
                //arrange

                string comment = "设计满足规范GB51251_2017中第3.3.1条条文规定。";

                List<ComponentAnnotation> componentViolations = new List<ComponentAnnotation>();
                FakeHVACFunction.ExcelPath_new = @"D:\wangT\HVAC-Checker\UnitTestHVACChecker\测试数据\测试数据_GB51251_2017_3_3_1.xlsx";
                //打开测试数据文件
                string importExcelPath = FakeHVACFunction.ExcelPath_new;
                //打开数据文件
                IWorkbook workbook = WorkbookFactory.Create(importExcelPath);
                //读取数据表格
                ISheet sheet_rooms = workbook.GetSheet(FakeHVACFunction.roomSheetName_new);

                List<Room> rooms = new List<Room>();
                //依次读取数据行，并根据数据内容创建房间，并加入房间集合中
                for (int index = 1; index <= sheet_rooms.LastRowNum; ++index)
                {
                    IRow row = (IRow)sheet_rooms.GetRow(index);
                    if (!row.GetCell(sheet_rooms.getColNumber("是否通过")).BooleanCellValue)
                    {
                        long roomId = Convert.ToInt64(row.GetCell(sheet_rooms.getColNumber("ID")).ToString());
                        String type = row.GetCell(sheet_rooms.getColNumber("房间类型")).ToString();
                        ComponentAnnotation componentAnnotation = new ComponentAnnotation();
                        componentAnnotation.Id = roomId;
                        componentAnnotation.type = type;
                        componentAnnotation.remark = string.Empty;
                        componentViolations.Add(componentAnnotation);
                    }

                }


                //act
                BimReview result = new BimReview();
                result = HVACChecker.GB51251_2017_3_3_1();


                //assert
                Assert.AreEqual(comment, result.comment);
                Assert.IsTrue(result.isPassCheck);
                Custom_Assert.AreComponentViolationListEqual(componentViolations, result.violationComponents);
            }
        }

        [TestMethod]
        public void test_unpass()
        {
            using (ShimsContext.Create())
            {
                FakeHVACFunction.inital();

                FakeHVACFunction.roomSheetName_new = "房间(不通过)";

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetHighestStoryNoOfRoomRoom = FakeHVACFunction.getHighestStoryNoOfRoom_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetFloors = FakeHVACFunction.GetAllFLoorsOfBuilding_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomContainAirTerminalRoom = FakeHVACFunction.GetRoomContainAirTerminal_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomsString = FakeHVACFunction.GetRooms_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetOutletsOfFanFan = FakeHVACFunction.GetOutputLetsOfFan_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetFanConnectingAirterminalAirTerminal = FakeHVACFunction.GetFanConnectingAirterminal_new;
                //arrange

                string comment = "设计不满足规范GB51251_2017中第3.3.1条条文规定。";

                List<ComponentAnnotation> componentViolations = new List<ComponentAnnotation>();
                FakeHVACFunction.ExcelPath_new = @"D:\wangT\HVAC-Checker\UnitTestHVACChecker\测试数据\测试数据_GB51251_2017_3_3_1.xlsx";
                //打开测试数据文件
                string importExcelPath = FakeHVACFunction.ExcelPath_new;
                //打开数据文件
                IWorkbook workbook = WorkbookFactory.Create(importExcelPath);
                //读取数据表格
                ISheet sheet_rooms = workbook.GetSheet(FakeHVACFunction.roomSheetName_new);

                List<Room> rooms = new List<Room>();
                //依次读取数据行，并根据数据内容创建房间，并加入房间集合中
                for (int index = 1; index <= sheet_rooms.LastRowNum; ++index)
                {
                    IRow row = (IRow)sheet_rooms.GetRow(index);
                    if (!row.GetCell(sheet_rooms.getColNumber("是否通过")).BooleanCellValue)
                    {
                        long roomId = Convert.ToInt64(row.GetCell(sheet_rooms.getColNumber("ID")).ToString());
                        String type = row.GetCell(sheet_rooms.getColNumber("房间类型")).ToString();
                        ComponentAnnotation componentAnnotation = new ComponentAnnotation();
                        componentAnnotation.Id = roomId;
                        componentAnnotation.type = type;
                        componentAnnotation.remark = string.Empty;
                        componentViolations.Add(componentAnnotation);
                    }

                }


                //act
                BimReview result = new BimReview();
                result = HVACChecker.GB51251_2017_3_3_1();


                //assert
                Assert.AreEqual(comment, result.comment);
                Assert.IsFalse(result.isPassCheck);
                Custom_Assert.AreComponentViolationListEqual(componentViolations, result.violationComponents);
            }
        }
    }

    [TestClass]
    public class GB51251_2017_3_3_11_Test
    {
        [TestMethod]
        public void test_pass()
        {
            using (ShimsContext.Create())
            {
                FakeHVACFunction.inital();

                FakeHVACFunction.roomSheetName_new = "房间(通过)";

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetHighestStoryNoOfRoomRoom = FakeHVACFunction.getHighestStoryNoOfRoom_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomsString = FakeHVACFunction.GetRooms_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetWindowsInRoomRoom = FakeHVACFunction.GetWindowsInRoom_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetFanConnectingAirterminalAirTerminal = FakeHVACFunction.GetFanConnectingAirterminal_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomContainAirTerminalRoom = FakeHVACFunction.GetRoomContainAirTerminal_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetAllWallsOfRoomRoom = FakeHVACFunction.GetWallsOfRoom_new;
                //arrange

                string comment = "设计满足规范GB51251_2017中第3.3.11条条文规定。";

                List<ComponentAnnotation> componentViolations = new List<ComponentAnnotation>();
                FakeHVACFunction.ExcelPath_new = @"D:\wangT\HVAC-Checker\UnitTestHVACChecker\测试数据\测试数据_GB51251_2017_3_3_11.xlsx";
                //打开测试数据文件
                string importExcelPath = FakeHVACFunction.ExcelPath_new;
                //打开数据文件
                IWorkbook workbook = WorkbookFactory.Create(importExcelPath);
                //读取数据表格
                ISheet sheet_rooms = workbook.GetSheet(FakeHVACFunction.roomSheetName_new);

                List<Room> rooms = new List<Room>();
                //依次读取数据行，并根据数据内容创建房间，并加入房间集合中
                for (int index = 1; index <= sheet_rooms.LastRowNum; ++index)
                {
                    IRow row = (IRow)sheet_rooms.GetRow(index);
                    if (!row.GetCell(sheet_rooms.getColNumber("是否通过")).BooleanCellValue)
                    {
                        long roomId = Convert.ToInt64(row.GetCell(sheet_rooms.getColNumber("ID")).ToString());
                        String type = row.GetCell(sheet_rooms.getColNumber("房间类型")).ToString();
                        ComponentAnnotation componentAnnotation = new ComponentAnnotation();
                        componentAnnotation.Id = roomId;
                        componentAnnotation.type = type;
                        componentAnnotation.remark = string.Empty;
                        componentViolations.Add(componentAnnotation);
                    }

                }


                //act
                BimReview result = new BimReview();
                result = HVACChecker.GB51251_2017_3_3_11();


                //assert
                Assert.AreEqual(comment, result.comment);
                Assert.IsTrue(result.isPassCheck);
                Custom_Assert.AreComponentViolationListEqual(componentViolations, result.violationComponents);
            }
        }

        [TestMethod]
        public void test_unpass()
        {
            using (ShimsContext.Create())
            {
                FakeHVACFunction.inital();

                FakeHVACFunction.roomSheetName_new = "房间(不通过)";

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetHighestStoryNoOfRoomRoom = FakeHVACFunction.getHighestStoryNoOfRoom_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomsString = FakeHVACFunction.GetRooms_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetWindowsInRoomRoom = FakeHVACFunction.GetWindowsInRoom_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetFanConnectingAirterminalAirTerminal = FakeHVACFunction.GetFanConnectingAirterminal_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomContainAirTerminalRoom = FakeHVACFunction.GetRoomContainAirTerminal_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetAllWallsOfRoomRoom= FakeHVACFunction.GetWallsOfRoom_new;
                //arrange

                string comment = "设计不满足规范GB51251_2017中第3.3.11条条文规定。";

                List<ComponentAnnotation> componentViolations = new List<ComponentAnnotation>();
                FakeHVACFunction.ExcelPath_new = @"D:\wangT\HVAC-Checker\UnitTestHVACChecker\测试数据\测试数据_GB51251_2017_3_3_11.xlsx";
                //打开测试数据文件
                string importExcelPath = FakeHVACFunction.ExcelPath_new;
                //打开数据文件
                IWorkbook workbook = WorkbookFactory.Create(importExcelPath);
                //读取数据表格
                ISheet sheet_rooms = workbook.GetSheet(FakeHVACFunction.roomSheetName_new);

                List<Room> rooms = new List<Room>();
                //依次读取数据行，并根据数据内容创建房间，并加入房间集合中
                for (int index = 1; index <= sheet_rooms.LastRowNum; ++index)
                {
                    IRow row = (IRow)sheet_rooms.GetRow(index);
                    if (!row.GetCell(sheet_rooms.getColNumber("是否通过")).BooleanCellValue)
                    {
                        long roomId = Convert.ToInt64(row.GetCell(sheet_rooms.getColNumber("ID")).ToString());
                        String type = row.GetCell(sheet_rooms.getColNumber("房间类型")).ToString();
                        ComponentAnnotation componentAnnotation = new ComponentAnnotation();
                        componentAnnotation.Id = roomId;
                        componentAnnotation.type = type;
                        componentAnnotation.remark = string.Empty;
                        componentViolations.Add(componentAnnotation);
                    }

                }


                //act
                BimReview result = new BimReview();
                result = HVACChecker.GB51251_2017_3_3_11();


                //assert
                Assert.AreEqual(comment, result.comment);
                Assert.IsFalse(result.isPassCheck);
                Custom_Assert.AreComponentViolationListEqual(componentViolations, result.violationComponents);
            }
        }
    }

    [TestClass]
    public class GB51251_2017_4_4_1_Test
    {
        [TestMethod]
        public void test_pass()
        {
            using (ShimsContext.Create())
            {
                FakeHVACFunction.inital();

                FakeHVACFunction.fireDistrictSheetName_new = "防火分区(通过)";

                FakeHVACFunction.fanSheetName_new = "风机(通过)";

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetFireCompartmentContainAirTerminalAirTerminal= FakeHVACFunction.getFireDistrictContainAirTerminal_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetFanConnectingAirterminalAirTerminal = FakeHVACFunction.GetFanConnectingAirterminal_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetInletsOfFanFan = FakeHVACFunction.GetInputLetsOfFan_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetAllFans = FakeHVACFunction.GetAllFans_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.isAirTerminalInFireCompartmentAirTerminalFireCompartment = FakeHVACFunction.isAirTerminalInFireDistrict_new;
                //arrange

                string comment = "设计满足规范GB51251_2017中第4.4.1条条文规定。";

                List<ComponentAnnotation> componentViolations = new List<ComponentAnnotation>();
                FakeHVACFunction.ExcelPath_new = @"D:\wangT\HVAC-Checker\UnitTestHVACChecker\测试数据\测试数据_GB51251_2017_4_4_1.xlsx";
                //打开测试数据文件
                string importExcelPath = FakeHVACFunction.ExcelPath_new;
                //打开数据文件
                IWorkbook workbook = WorkbookFactory.Create(importExcelPath);
                //读取数据表格
                ISheet sheet_Fans = workbook.GetSheet(FakeHVACFunction.fanSheetName_new);

                List<Fan> fans = new List<Fan>();
                //依次读取数据行，并根据数据内容创建房间，并加入房间集合中
                for (int index = 1; index <= sheet_Fans.LastRowNum; ++index)
                {
                    IRow row = (IRow)sheet_Fans.GetRow(index);
                    if (!row.GetCell(sheet_Fans.getColNumber("是否通过")).BooleanCellValue)
                    {
                        long fanId = Convert.ToInt64(row.GetCell(sheet_Fans.getColNumber("ID")).ToString());
                        ComponentAnnotation componentAnnotation = new ComponentAnnotation();
                        componentAnnotation.Id = fanId;
                        componentAnnotation.type = "风机";
                        componentAnnotation.remark = "风机所在的排烟系统跨越了防火分区设置";
                        componentViolations.Add(componentAnnotation);
                    }

                }


                //act
                BimReview result = new BimReview();
                result = HVACChecker.GB51251_2017_4_4_1();


                //assert
                Assert.AreEqual(comment, result.comment);
                Assert.IsTrue(result.isPassCheck);
                Custom_Assert.AreComponentViolationListEqual(componentViolations, result.violationComponents);
            }
        }

        [TestMethod]
        public void test_unpass()
        {
            using (ShimsContext.Create())
            {
                FakeHVACFunction.inital();

                FakeHVACFunction.fireDistrictSheetName_new = "防火分区(不通过)";

                FakeHVACFunction.fanSheetName_new = "风机(不通过)";

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetFireCompartmentContainAirTerminalAirTerminal = FakeHVACFunction.getFireDistrictContainAirTerminal_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetFanConnectingAirterminalAirTerminal = FakeHVACFunction.GetFanConnectingAirterminal_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetInletsOfFanFan = FakeHVACFunction.GetInputLetsOfFan_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetAllFans = FakeHVACFunction.GetAllFans_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.isAirTerminalInFireCompartmentAirTerminalFireCompartment = FakeHVACFunction.isAirTerminalInFireDistrict_new;
                //arrange

                string comment = "设计不满足规范GB51251_2017中第4.4.1条条文规定。";

                List<ComponentAnnotation> componentViolations = new List<ComponentAnnotation>();
                FakeHVACFunction.ExcelPath_new = @"D:\wangT\HVAC-Checker\UnitTestHVACChecker\测试数据\测试数据_GB51251_2017_4_4_1.xlsx";
                //打开测试数据文件
                string importExcelPath = FakeHVACFunction.ExcelPath_new;
                //打开数据文件
                IWorkbook workbook = WorkbookFactory.Create(importExcelPath);
                //读取数据表格
                ISheet sheet_Fans = workbook.GetSheet(FakeHVACFunction.fanSheetName_new);

                List<Fan> fans = new List<Fan>();
                //依次读取数据行，并根据数据内容创建房间，并加入房间集合中
                for (int index = 1; index <= sheet_Fans.LastRowNum; ++index)
                {
                    IRow row = (IRow)sheet_Fans.GetRow(index);
                    if (!row.GetCell(sheet_Fans.getColNumber("是否通过")).BooleanCellValue)
                    {
                        long fanId = Convert.ToInt64(row.GetCell(sheet_Fans.getColNumber("ID")).ToString());
                        ComponentAnnotation componentAnnotation = new ComponentAnnotation();
                        componentAnnotation.Id = fanId;
                        componentAnnotation.type = "风机";
                        componentAnnotation.remark = "风机所在的排烟系统跨越了防火分区设置";
                        componentViolations.Add(componentAnnotation);
                    }

                }


                //act
                BimReview result = new BimReview();
                result = HVACChecker.GB51251_2017_4_4_1();


                //assert
                Assert.AreEqual(comment, result.comment);
                Assert.IsFalse(result.isPassCheck);
                Custom_Assert.AreComponentViolationListEqual(componentViolations, result.violationComponents);
            }
        }
    }

    [TestClass]
    public class GB51251_2017_4_4_2_Test
    {
        [TestMethod]
        public void test_public_higherThan50_pass()
        {
            using (ShimsContext.Create())
            {
                FakeHVACFunction.inital();

                FakeHVACFunction.fanSheetName_new = "风机(公共建筑>50通过)";


                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetFanConnectingAirterminalAirTerminal = FakeHVACFunction.GetFanConnectingAirterminal_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetInletsOfFanFan = FakeHVACFunction.GetInputLetsOfFan_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetAllFans = FakeHVACFunction.GetAllFans_new;

                //arrange

                string comment = "设计满足规范GB51251_2017中第4.4.2条条文规定。";

                globalData.buildingType = "公共建筑";
                globalData.buildingHeight = 51;

                List<ComponentAnnotation> componentViolations = new List<ComponentAnnotation>();
                FakeHVACFunction.ExcelPath_new = @"D:\wangT\HVAC-Checker\UnitTestHVACChecker\测试数据\测试数据_GB51251_2017_4_4_2.xlsx";
                //打开测试数据文件
                string importExcelPath = FakeHVACFunction.ExcelPath_new;
                //打开数据文件
                IWorkbook workbook = WorkbookFactory.Create(importExcelPath);
                //读取数据表格
                ISheet sheet_Fans = workbook.GetSheet(FakeHVACFunction.fanSheetName_new);

                List<Fan> fans = new List<Fan>();
                //依次读取数据行，并根据数据内容创建风机，并加入风机集合中
                for (int index = 1; index <= sheet_Fans.LastRowNum; ++index)
                {
                    IRow row = (IRow)sheet_Fans.GetRow(index);
                    if (!row.GetCell(sheet_Fans.getColNumber("是否通过")).BooleanCellValue)
                    {
                        long fanId = Convert.ToInt64(row.GetCell(sheet_Fans.getColNumber("ID")).ToString());
                        ComponentAnnotation componentAnnotation = new ComponentAnnotation();
                        componentAnnotation.Id = fanId;
                        componentAnnotation.type = "风机";
                        componentAnnotation.remark = "风机所在的排烟系统设置高度超过规范要求";
                        componentViolations.Add(componentAnnotation);
                    }

                }


                //act
                BimReview result = new BimReview();
                result = HVACChecker.GB51251_2017_4_4_2();


                //assert
                Assert.AreEqual(comment, result.comment);
                Assert.IsTrue(result.isPassCheck);
                Custom_Assert.AreComponentViolationListEqual(componentViolations, result.violationComponents);
            }
        }


        [TestMethod]
        public void test_public_LowerThan50_pass()
        {
            using (ShimsContext.Create())
            {
                FakeHVACFunction.inital();

                FakeHVACFunction.fanSheetName_new = "风机(公共建筑<=50通过)";


                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetFanConnectingAirterminalAirTerminal = FakeHVACFunction.GetFanConnectingAirterminal_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetInletsOfFanFan = FakeHVACFunction.GetInputLetsOfFan_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetAllFans = FakeHVACFunction.GetAllFans_new;

                //arrange

                string comment = "设计满足规范GB51251_2017中第4.4.2条条文规定。";

                globalData.buildingType = "公共建筑";
                globalData.buildingHeight = 50;

                List<ComponentAnnotation> componentViolations = new List<ComponentAnnotation>();
                FakeHVACFunction.ExcelPath_new = @"D:\wangT\HVAC-Checker\UnitTestHVACChecker\测试数据\测试数据_GB51251_2017_4_4_2.xlsx";
                //打开测试数据文件
                string importExcelPath = FakeHVACFunction.ExcelPath_new;
                //打开数据文件
                IWorkbook workbook = WorkbookFactory.Create(importExcelPath);
                //读取数据表格
                ISheet sheet_Fans = workbook.GetSheet(FakeHVACFunction.fanSheetName_new);

                List<Fan> fans = new List<Fan>();
                //依次读取数据行，并根据数据内容创建风机，并加入风机集合中
                for (int index = 1; index <= sheet_Fans.LastRowNum; ++index)
                {
                    IRow row = (IRow)sheet_Fans.GetRow(index);
                    if (!row.GetCell(sheet_Fans.getColNumber("是否通过")).BooleanCellValue)
                    {
                        long fanId = Convert.ToInt64(row.GetCell(sheet_Fans.getColNumber("ID")).ToString());
                        ComponentAnnotation componentAnnotation = new ComponentAnnotation();
                        componentAnnotation.Id = fanId;
                        componentAnnotation.type = "风机";
                        componentAnnotation.remark = "风机所在的排烟系统设置高度超过规范要求";
                        componentViolations.Add(componentAnnotation);
                    }

                }


                //act
                BimReview result = new BimReview();
                result = HVACChecker.GB51251_2017_4_4_2();


                //assert
                Assert.AreEqual(comment, result.comment);
                Assert.IsTrue(result.isPassCheck);
                Custom_Assert.AreComponentViolationListEqual(componentViolations, result.violationComponents);
            }
        }


        [TestMethod]
        public void test_residence_LowerThan100_pass()
        {
            using (ShimsContext.Create())
            {
                FakeHVACFunction.inital();

                FakeHVACFunction.fanSheetName_new = "风机(住宅<=100通过)";


                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetFanConnectingAirterminalAirTerminal = FakeHVACFunction.GetFanConnectingAirterminal_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetInletsOfFanFan = FakeHVACFunction.GetInputLetsOfFan_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetAllFans = FakeHVACFunction.GetAllFans_new;

                //arrange

                string comment = "设计满足规范GB51251_2017中第4.4.2条条文规定。";

                globalData.buildingType = "住宅";
                globalData.buildingHeight = 100;

                List<ComponentAnnotation> componentViolations = new List<ComponentAnnotation>();
                FakeHVACFunction.ExcelPath_new = @"D:\wangT\HVAC-Checker\UnitTestHVACChecker\测试数据\测试数据_GB51251_2017_4_4_2.xlsx";
                //打开测试数据文件
                string importExcelPath = FakeHVACFunction.ExcelPath_new;
                //打开数据文件
                IWorkbook workbook = WorkbookFactory.Create(importExcelPath);
                //读取数据表格
                ISheet sheet_Fans = workbook.GetSheet(FakeHVACFunction.fanSheetName_new);

                List<Fan> fans = new List<Fan>();
                //依次读取数据行，并根据数据内容创建风机，并加入风机集合中
                for (int index = 1; index <= sheet_Fans.LastRowNum; ++index)
                {
                    IRow row = (IRow)sheet_Fans.GetRow(index);
                    if (!row.GetCell(sheet_Fans.getColNumber("是否通过")).BooleanCellValue)
                    {
                        long fanId = Convert.ToInt64(row.GetCell(sheet_Fans.getColNumber("ID")).ToString());
                        ComponentAnnotation componentAnnotation = new ComponentAnnotation();
                        componentAnnotation.Id = fanId;
                        componentAnnotation.type = "风机";
                        componentAnnotation.remark = "风机所在的排烟系统设置高度超过规范要求";
                        componentViolations.Add(componentAnnotation);
                    }

                }


                //act
                BimReview result = new BimReview();
                result = HVACChecker.GB51251_2017_4_4_2();


                //assert
                Assert.AreEqual(comment, result.comment);
                Assert.IsTrue(result.isPassCheck);
                Custom_Assert.AreComponentViolationListEqual(componentViolations, result.violationComponents);
            }
        }

        [TestMethod]
        public void test_residence_HigherThan100_pass()
        {
            using (ShimsContext.Create())
            {
                FakeHVACFunction.inital();

                FakeHVACFunction.fanSheetName_new = "风机(住宅>100通过)";


                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetFanConnectingAirterminalAirTerminal = FakeHVACFunction.GetFanConnectingAirterminal_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetInletsOfFanFan = FakeHVACFunction.GetInputLetsOfFan_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetAllFans = FakeHVACFunction.GetAllFans_new;

                //arrange

                string comment = "设计满足规范GB51251_2017中第4.4.2条条文规定。";


                globalData.buildingType = "住宅";
                globalData.buildingHeight = 101;


                List<ComponentAnnotation> componentViolations = new List<ComponentAnnotation>();
                FakeHVACFunction.ExcelPath_new = @"D:\wangT\HVAC-Checker\UnitTestHVACChecker\测试数据\测试数据_GB51251_2017_4_4_2.xlsx";
                //打开测试数据文件
                string importExcelPath = FakeHVACFunction.ExcelPath_new;
                //打开数据文件
                IWorkbook workbook = WorkbookFactory.Create(importExcelPath);
                //读取数据表格
                ISheet sheet_Fans = workbook.GetSheet(FakeHVACFunction.fanSheetName_new);

                List<Fan> fans = new List<Fan>();
                //依次读取数据行，并根据数据内容创建风机，并加入风机集合中
                for (int index = 1; index <= sheet_Fans.LastRowNum; ++index)
                {
                    IRow row = (IRow)sheet_Fans.GetRow(index);
                    if (!row.GetCell(sheet_Fans.getColNumber("是否通过")).BooleanCellValue)
                    {
                        long fanId = Convert.ToInt64(row.GetCell(sheet_Fans.getColNumber("ID")).ToString());
                        ComponentAnnotation componentAnnotation = new ComponentAnnotation();
                        componentAnnotation.Id = fanId;
                        componentAnnotation.type = "风机";
                        componentAnnotation.remark = "风机所在的排烟系统设置高度超过规范要求";
                        componentViolations.Add(componentAnnotation);
                    }

                }


                //act
                BimReview result = new BimReview();
                result = HVACChecker.GB51251_2017_4_4_2();


                //assert
                Assert.AreEqual(comment, result.comment);
                Assert.IsTrue(result.isPassCheck);
                Custom_Assert.AreComponentViolationListEqual(componentViolations, result.violationComponents);
            }
        }



        [TestMethod]
        public void test_public_higherThan50_unpass()
        {
            using (ShimsContext.Create())
            {
                FakeHVACFunction.inital();

                FakeHVACFunction.fanSheetName_new = "风机(公共建筑>50不通过)";


                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetFanConnectingAirterminalAirTerminal = FakeHVACFunction.GetFanConnectingAirterminal_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetInletsOfFanFan = FakeHVACFunction.GetInputLetsOfFan_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetAllFans = FakeHVACFunction.GetAllFans_new;

                //arrange

                string comment = "设计不满足规范GB51251_2017中第4.4.2条条文规定。";

                globalData.buildingType = "公共建筑";
                globalData.buildingHeight = 51;

                List<ComponentAnnotation> componentViolations = new List<ComponentAnnotation>();
                FakeHVACFunction.ExcelPath_new = @"D:\wangT\HVAC-Checker\UnitTestHVACChecker\测试数据\测试数据_GB51251_2017_4_4_2.xlsx";
                //打开测试数据文件
                string importExcelPath = FakeHVACFunction.ExcelPath_new;
                //打开数据文件
                IWorkbook workbook = WorkbookFactory.Create(importExcelPath);
                //读取数据表格
                ISheet sheet_Fans = workbook.GetSheet(FakeHVACFunction.fanSheetName_new);

                List<Fan> fans = new List<Fan>();
                //依次读取数据行，并根据数据内容创建风机，并加入风机集合中
                for (int index = 1; index <= sheet_Fans.LastRowNum; ++index)
                {
                    IRow row = (IRow)sheet_Fans.GetRow(index);
                    if (!row.GetCell(sheet_Fans.getColNumber("是否通过")).BooleanCellValue)
                    {
                        long fanId = Convert.ToInt64(row.GetCell(sheet_Fans.getColNumber("ID")).ToString());
                        ComponentAnnotation componentAnnotation = new ComponentAnnotation();
                        componentAnnotation.Id = fanId;
                        componentAnnotation.type = "风机";
                        componentAnnotation.remark = "风机所在的排烟系统设置高度超过规范要求";
                        componentViolations.Add(componentAnnotation);
                    }

                }


                //act
                BimReview result = new BimReview();
                result = HVACChecker.GB51251_2017_4_4_2();


                //assert
                Assert.AreEqual(comment, result.comment);
                Assert.IsFalse(result.isPassCheck);
                Custom_Assert.AreComponentViolationListEqual(componentViolations, result.violationComponents);
            }
        }

        [TestMethod]
        public void test_residence_higherThan100_unpass()
        {
            using (ShimsContext.Create())
            {
                FakeHVACFunction.inital();

                FakeHVACFunction.fanSheetName_new = "风机(住宅>100不通过)";


                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetFanConnectingAirterminalAirTerminal = FakeHVACFunction.GetFanConnectingAirterminal_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetInletsOfFanFan = FakeHVACFunction.GetInputLetsOfFan_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetAllFans = FakeHVACFunction.GetAllFans_new;

                //arrange

                string comment = "设计不满足规范GB51251_2017中第4.4.2条条文规定。";

                globalData.buildingType = "住宅";
                globalData.buildingHeight = 101;

                List<ComponentAnnotation> componentViolations = new List<ComponentAnnotation>();
                FakeHVACFunction.ExcelPath_new = @"D:\wangT\HVAC-Checker\UnitTestHVACChecker\测试数据\测试数据_GB51251_2017_4_4_2.xlsx";
                //打开测试数据文件
                string importExcelPath = FakeHVACFunction.ExcelPath_new;
                //打开数据文件
                IWorkbook workbook = WorkbookFactory.Create(importExcelPath);
                //读取数据表格
                ISheet sheet_Fans = workbook.GetSheet(FakeHVACFunction.fanSheetName_new);

                List<Fan> fans = new List<Fan>();
                //依次读取数据行，并根据数据内容创建风机，并加入风机集合中
                for (int index = 1; index <= sheet_Fans.LastRowNum; ++index)
                {
                    IRow row = (IRow)sheet_Fans.GetRow(index);
                    if (!row.GetCell(sheet_Fans.getColNumber("是否通过")).BooleanCellValue)
                    {
                        long fanId = Convert.ToInt64(row.GetCell(sheet_Fans.getColNumber("ID")).ToString());
                        ComponentAnnotation componentAnnotation = new ComponentAnnotation();
                        componentAnnotation.Id = fanId;
                        componentAnnotation.type = "风机";
                        componentAnnotation.remark = "风机所在的排烟系统设置高度超过规范要求";
                        componentViolations.Add(componentAnnotation);
                    }

                }


                //act
                BimReview result = new BimReview();
                result = HVACChecker.GB51251_2017_4_4_2();


                //assert
                Assert.AreEqual(comment, result.comment);
                Assert.IsFalse(result.isPassCheck);
                Custom_Assert.AreComponentViolationListEqual(componentViolations, result.violationComponents);
            }
        }
    }



    [TestClass]
    public class GB51251_2017_4_5_1_Test
    {
        [TestMethod]
        public void test_pass()
        {
            using (ShimsContext.Create())
            {
                FakeHVACFunction.inital();

                FakeHVACFunction.roomSheetName_new = "房间(通过)";

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomsString = FakeHVACFunction.GetRooms_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetWindowsInRoomRoom = FakeHVACFunction.GetWindowsInRoom_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetFanConnectingAirterminalAirTerminal = FakeHVACFunction.GetFanConnectingAirterminal_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomContainAirTerminalRoom = FakeHVACFunction.GetRoomContainAirTerminal_new;


                //arrange

                string comment = "设计满足规范GB51251_2017中第4.5.1条条文规定。请专家核对采用自然排烟方式的房间是否满足补风要求。";

                List<ComponentAnnotation> componentViolations = new List<ComponentAnnotation>();
                FakeHVACFunction.ExcelPath_new = @"D:\wangT\HVAC-Checker\UnitTestHVACChecker\测试数据\测试数据_GB51251_2017_4_5_1.xlsx";
                //打开测试数据文件
                string importExcelPath = FakeHVACFunction.ExcelPath_new;
                //打开数据文件
                IWorkbook workbook = WorkbookFactory.Create(importExcelPath);
                //读取数据表格
                ISheet sheet_rooms = workbook.GetSheet(FakeHVACFunction.roomSheetName_new);

                List<Room> rooms = new List<Room>();
                //依次读取数据行，并根据数据内容创建房间，并加入房间集合中
                for (int index = 1; index <= sheet_rooms.LastRowNum; ++index)
                {
                    IRow row = (IRow)sheet_rooms.GetRow(index);
                    if (!row.GetCell(sheet_rooms.getColNumber("是否通过")).BooleanCellValue)
                    {
                        long roomId = Convert.ToInt64(row.GetCell(sheet_rooms.getColNumber("ID")).ToString());
                        String type = row.GetCell(sheet_rooms.getColNumber("房间类型")).ToString();
                        ComponentAnnotation componentAnnotation = new ComponentAnnotation();
                        componentAnnotation.Id = roomId;
                        componentAnnotation.type = type;
                        componentAnnotation.remark = string.Empty;
                        componentViolations.Add(componentAnnotation);
                    }
                    if(row.GetCell(sheet_rooms.getColNumber("是否自然排烟")).BooleanCellValue)
                    {
                        long roomId = Convert.ToInt64(row.GetCell(sheet_rooms.getColNumber("ID")).ToString());
                        String type = row.GetCell(sheet_rooms.getColNumber("房间类型")).ToString();
                        ComponentAnnotation componentAnnotation = new ComponentAnnotation();
                        componentAnnotation.Id = roomId;
                        componentAnnotation.type = type;
                        componentAnnotation.remark = "此房间采用自然排烟方式，请专家核对此房间补风系统是否满足要求";
                        componentViolations.Add(componentAnnotation);
                    }

                }


                //act
                BimReview result = new BimReview();
                result = HVACChecker.GB51251_2017_4_5_1();


                //assert
                Assert.AreEqual(comment, result.comment);
                Assert.IsTrue(result.isPassCheck);
                Custom_Assert.AreComponentViolationListEqual(componentViolations, result.violationComponents);
            }
        }

         [TestMethod]
        public void test_unpass()
        {
            using (ShimsContext.Create())
            {
                FakeHVACFunction.inital();

                FakeHVACFunction.roomSheetName_new = "房间(不通过)";

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomsString = FakeHVACFunction.GetRooms_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetWindowsInRoomRoom = FakeHVACFunction.GetWindowsInRoom_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetFanConnectingAirterminalAirTerminal = FakeHVACFunction.GetFanConnectingAirterminal_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomContainAirTerminalRoom = FakeHVACFunction.GetRoomContainAirTerminal_new;


                //arrange

                string comment = "设计不满足规范GB51251_2017中第4.5.1条条文规定。请专家核对采用自然排烟方式的房间是否满足补风要求。";

                List<ComponentAnnotation> componentViolations = new List<ComponentAnnotation>();
                FakeHVACFunction.ExcelPath_new = @"D:\wangT\HVAC-Checker\UnitTestHVACChecker\测试数据\测试数据_GB51251_2017_4_5_1.xlsx";
                //打开测试数据文件
                string importExcelPath = FakeHVACFunction.ExcelPath_new;
                //打开数据文件
                IWorkbook workbook = WorkbookFactory.Create(importExcelPath);
                //读取数据表格
                ISheet sheet_rooms = workbook.GetSheet(FakeHVACFunction.roomSheetName_new);

                List<Room> rooms = new List<Room>();
                //依次读取数据行，并根据数据内容创建房间，并加入房间集合中
                for (int index = 1; index <= sheet_rooms.LastRowNum; ++index)
                {
                    IRow row = (IRow)sheet_rooms.GetRow(index);
                    if (!row.GetCell(sheet_rooms.getColNumber("是否通过")).BooleanCellValue)
                    {
                        long roomId = Convert.ToInt64(row.GetCell(sheet_rooms.getColNumber("ID")).ToString());
                        String type = row.GetCell(sheet_rooms.getColNumber("房间类型")).ToString();
                        ComponentAnnotation componentAnnotation = new ComponentAnnotation();
                        componentAnnotation.Id = roomId;
                        componentAnnotation.type = type;
                        componentAnnotation.remark = "此房间未设补风系统";
                        componentViolations.Add(componentAnnotation);
                    }
                    if (row.GetCell(sheet_rooms.getColNumber("是否自然排烟")).BooleanCellValue)
                    {
                        long roomId = Convert.ToInt64(row.GetCell(sheet_rooms.getColNumber("ID")).ToString());
                        String type = row.GetCell(sheet_rooms.getColNumber("房间类型")).ToString();
                        ComponentAnnotation componentAnnotation = new ComponentAnnotation();
                        componentAnnotation.Id = roomId;
                        componentAnnotation.type = type;
                        componentAnnotation.remark = "此房间采用自然排烟方式，请专家核对此房间补风系统是否满足要求";
                        componentViolations.Add(componentAnnotation);
                    }

                }


                //act
                BimReview result = new BimReview();
                result = HVACChecker.GB51251_2017_4_5_1();


                //assert
                Assert.AreEqual(comment, result.comment);
                Assert.IsFalse(result.isPassCheck);
                Custom_Assert.AreComponentViolationListEqual(componentViolations, result.violationComponents);
            }
        }

        [TestMethod]
        public void test_emptyRoom_pass()
        {
            using (ShimsContext.Create())
            {
                FakeHVACFunction.inital();

                FakeHVACFunction.roomSheetName_new = "房间(空房间) ";

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomsString = FakeHVACFunction.GetRooms_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetWindowsInRoomRoom = FakeHVACFunction.GetWindowsInRoom_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetFanConnectingAirterminalAirTerminal = FakeHVACFunction.GetFanConnectingAirterminal_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomContainAirTerminalRoom = FakeHVACFunction.GetRoomContainAirTerminal_new;


                //arrange

                string comment = "设计满足规范GB51251_2017中第4.5.1条条文规定。";

                List<ComponentAnnotation> componentViolations = new List<ComponentAnnotation>();
                FakeHVACFunction.ExcelPath_new = @"D:\wangT\HVAC-Checker\UnitTestHVACChecker\测试数据\测试数据_GB51251_2017_4_5_1.xlsx";
                //打开测试数据文件
                string importExcelPath = FakeHVACFunction.ExcelPath_new;
                //打开数据文件
                IWorkbook workbook = WorkbookFactory.Create(importExcelPath);
                //读取数据表格
                ISheet sheet_rooms = workbook.GetSheet(FakeHVACFunction.roomSheetName_new);

                List<Room> rooms = new List<Room>();
                //依次读取数据行，并根据数据内容创建房间，并加入房间集合中
                for (int index = 1; index <= sheet_rooms.LastRowNum; ++index)
                {
                    IRow row = (IRow)sheet_rooms.GetRow(index);
                    if (!row.GetCell(sheet_rooms.getColNumber("是否通过")).BooleanCellValue)
                    {
                        long roomId = Convert.ToInt64(row.GetCell(sheet_rooms.getColNumber("ID")).ToString());
                        String type = row.GetCell(sheet_rooms.getColNumber("房间类型")).ToString();
                        ComponentAnnotation componentAnnotation = new ComponentAnnotation();
                        componentAnnotation.Id = roomId;
                        componentAnnotation.type = type;
                        componentAnnotation.remark = "此房间未设补风系统";
                        componentViolations.Add(componentAnnotation);
                    }
                    if (row.GetCell(sheet_rooms.getColNumber("是否自然排烟")).BooleanCellValue)
                    {
                        long roomId = Convert.ToInt64(row.GetCell(sheet_rooms.getColNumber("ID")).ToString());
                        String type = row.GetCell(sheet_rooms.getColNumber("房间类型")).ToString();
                        ComponentAnnotation componentAnnotation = new ComponentAnnotation();
                        componentAnnotation.Id = roomId;
                        componentAnnotation.type = type;
                        componentAnnotation.remark = "此房间采用自然排烟方式，请专家核对此房间补风系统是否满足要求";
                        componentViolations.Add(componentAnnotation);
                    }

                }


                //act
                BimReview result = new BimReview();
                result = HVACChecker.GB51251_2017_4_5_1();


                //assert
                Assert.AreEqual(comment, result.comment);
                Assert.IsTrue(result.isPassCheck);
                Custom_Assert.AreComponentViolationListEqual(componentViolations, result.violationComponents);
            }
        }
    }

    [TestClass]
    public class GB51251_2017_4_5_2_Test
    {
        [TestMethod]
        public void test_pass()
        {
            using (ShimsContext.Create())
            {
                FakeHVACFunction.inital();

                FakeHVACFunction.roomSheetName_new = "房间(通过)";

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomsString = FakeHVACFunction.GetRooms_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetWindowsInRoomRoom = FakeHVACFunction.GetWindowsInRoom_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetFanConnectingAirterminalAirTerminal = FakeHVACFunction.GetFanConnectingAirterminal_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomContainAirTerminalRoom = FakeHVACFunction.GetRoomContainAirTerminal_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetInletsOfFanFan = FakeHVACFunction.GetInputLetsOfFan_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.isOuterAirTerminalAirTerminal = FakeHVACFunction.isOuterAirTerminal_new;


                //arrange

                string comment = "设计满足规范GB51251_2017中第4.5.2条条文规定。";

                List<ComponentAnnotation> componentViolations = new List<ComponentAnnotation>();
                FakeHVACFunction.ExcelPath_new = @"D:\wangT\HVAC-Checker\UnitTestHVACChecker\测试数据\测试数据_GB51251_2017_4_5_2.xlsx";
                //打开测试数据文件
                string importExcelPath = FakeHVACFunction.ExcelPath_new;
                //打开数据文件
                IWorkbook workbook = WorkbookFactory.Create(importExcelPath);
                //读取数据表格
                ISheet sheet_rooms = workbook.GetSheet(FakeHVACFunction.roomSheetName_new);

                List<Room> rooms = new List<Room>();
                //依次读取数据行，并根据数据内容创建房间，并加入房间集合中
                for (int index = 1; index <= sheet_rooms.LastRowNum; ++index)
                {
                    IRow row = (IRow)sheet_rooms.GetRow(index);
                    if (!row.GetCell(sheet_rooms.getColNumber("是否通过")).BooleanCellValue)
                    {
                        long roomId = Convert.ToInt64(row.GetCell(sheet_rooms.getColNumber("ID")).ToString());
                        String type = row.GetCell(sheet_rooms.getColNumber("房间类型")).ToString();
                        ComponentAnnotation componentAnnotation = new ComponentAnnotation();
                        componentAnnotation.Id = roomId;
                        componentAnnotation.type = type;
                        int commentType = Convert.ToInt32(row.GetCell(sheet_rooms.getColNumber("批注类型")).ToString());
                        if(commentType==1)
                            componentAnnotation.remark = "此房间补风系统未从室外引入空气";
                        else if(commentType==2)
                            componentAnnotation.remark = "此房间补风量小于排烟量的50%";
                        componentViolations.Add(componentAnnotation);
                    }
                }


                //act
                BimReview result = new BimReview();
                result = HVACChecker.GB51251_2017_4_5_2();


                //assert
                Assert.AreEqual(comment, result.comment);
                Assert.IsTrue(result.isPassCheck);
                Custom_Assert.AreComponentViolationListEqual(componentViolations, result.violationComponents);
            }
        }

        [TestMethod]
        public void test_unpass()
        {
            using (ShimsContext.Create())
            {
                FakeHVACFunction.inital();

                FakeHVACFunction.roomSheetName_new = "房间(不通过)";

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomsString = FakeHVACFunction.GetRooms_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetWindowsInRoomRoom = FakeHVACFunction.GetWindowsInRoom_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetFanConnectingAirterminalAirTerminal = FakeHVACFunction.GetFanConnectingAirterminal_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomContainAirTerminalRoom = FakeHVACFunction.GetRoomContainAirTerminal_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetInletsOfFanFan = FakeHVACFunction.GetInputLetsOfFan_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.isOuterAirTerminalAirTerminal = FakeHVACFunction.isOuterAirTerminal_new;
                //arrange

                string comment = "设计不满足规范GB51251_2017中第4.5.2条条文规定。";

                List<ComponentAnnotation> componentViolations = new List<ComponentAnnotation>();
                FakeHVACFunction.ExcelPath_new = @"D:\wangT\HVAC-Checker\UnitTestHVACChecker\测试数据\测试数据_GB51251_2017_4_5_2.xlsx";
                //打开测试数据文件
                string importExcelPath = FakeHVACFunction.ExcelPath_new;
                //打开数据文件
                IWorkbook workbook = WorkbookFactory.Create(importExcelPath);
                //读取数据表格
                ISheet sheet_rooms = workbook.GetSheet(FakeHVACFunction.roomSheetName_new);

                List<Room> rooms = new List<Room>();
                //依次读取数据行，并根据数据内容创建房间，并加入房间集合中
                for (int index = 1; index <= sheet_rooms.LastRowNum; ++index)
                {
                    IRow row = (IRow)sheet_rooms.GetRow(index);
                    if (!row.GetCell(sheet_rooms.getColNumber("是否通过")).BooleanCellValue)
                    {
                        long roomId = Convert.ToInt64(row.GetCell(sheet_rooms.getColNumber("ID")).ToString());
                        String type = row.GetCell(sheet_rooms.getColNumber("房间类型")).ToString();
                        ComponentAnnotation componentAnnotation = new ComponentAnnotation();
                        componentAnnotation.Id = roomId;
                        componentAnnotation.type = type;
                        int commentType = Convert.ToInt32(row.GetCell(sheet_rooms.getColNumber("批注类型")).ToString());
                        if (commentType == 1)
                            componentAnnotation.remark = "此房间补风系统未从室外引入空气";
                        else if (commentType == 2)
                            componentAnnotation.remark = "此房间补风量小于排烟量的50%";
                        componentViolations.Add(componentAnnotation);
                    }
                }


                //act
                BimReview result = new BimReview();
                result = HVACChecker.GB51251_2017_4_5_2();


                //assert
                Assert.AreEqual(comment, result.comment);
                Assert.IsFalse(result.isPassCheck);
                Custom_Assert.AreComponentViolationListEqual(componentViolations, result.violationComponents);
            }
        }
    }

    [TestClass]
    public class GB50067_2014_8_2_1_Test
    {
        [TestMethod]
        public void test_pass()
        {
            using (ShimsContext.Create())
            {
                FakeHVACFunction.inital();

                FakeHVACFunction.roomSheetName_new = "房间(通过)";

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomsString = FakeHVACFunction.GetRooms_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetWindowsInRoomRoom = FakeHVACFunction.GetWindowsInRoom_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetFanConnectingAirterminalAirTerminal = FakeHVACFunction.GetFanConnectingAirterminal_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomContainAirTerminalRoom = FakeHVACFunction.GetRoomContainAirTerminal_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetSmokeCompartmentsInRoomRoom = FakeHVACFunction.GetSmokeCompartmentsInRoom_new;

                //arrange

                string comment = "设计满足规范GB50067_2014中第8.2.1条条文规定。";

                List<ComponentAnnotation> componentViolations = new List<ComponentAnnotation>();
                FakeHVACFunction.ExcelPath_new = @"D:\wangT\HVAC-Checker\UnitTestHVACChecker\测试数据\测试数据_GB50067_2014_8_2_1.xlsx";
                //打开测试数据文件
                string importExcelPath = FakeHVACFunction.ExcelPath_new;
                //打开数据文件
                IWorkbook workbook = WorkbookFactory.Create(importExcelPath);
                //读取数据表格
                ISheet sheet_rooms = workbook.GetSheet(FakeHVACFunction.roomSheetName_new);

                List<Room> rooms = new List<Room>();
                //依次读取数据行，并根据数据内容创建房间，并加入房间集合中
                for (int index = 1; index <= sheet_rooms.LastRowNum; ++index)
                {
                    IRow row = (IRow)sheet_rooms.GetRow(index);
                    if (!row.GetCell(sheet_rooms.getColNumber("是否通过")).BooleanCellValue)
                    {
                        long roomId = Convert.ToInt64(row.GetCell(sheet_rooms.getColNumber("ID")).ToString());
                        String type = row.GetCell(sheet_rooms.getColNumber("房间类型")).ToString();
                        ComponentAnnotation componentAnnotation = new ComponentAnnotation();
                        componentAnnotation.Id = roomId;
                        componentAnnotation.type = type;
                        int commentType = Convert.ToInt32(row.GetCell(sheet_rooms.getColNumber("批注类型")).ToString());
                        if (commentType == 1)
                            componentAnnotation.remark = "此车库未设置排烟系统";
                        else if (commentType == 2)
                            componentAnnotation.remark = "此车库未设置防烟分区";
                        componentViolations.Add(componentAnnotation);
                    }
                }


                //act
                BimReview result = new BimReview();
                result = HVACChecker.GB50067_2014_8_2_1();


                //assert
                Assert.AreEqual(comment, result.comment);
                Assert.IsTrue(result.isPassCheck);
                Custom_Assert.AreComponentViolationListEqual(componentViolations, result.violationComponents);
            }
        }

        [TestMethod]
        public void test_unpass()
        {
            using (ShimsContext.Create())
            {
                FakeHVACFunction.inital();

                FakeHVACFunction.roomSheetName_new = "房间(不通过)";

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomsString = FakeHVACFunction.GetRooms_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetWindowsInRoomRoom = FakeHVACFunction.GetWindowsInRoom_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetFanConnectingAirterminalAirTerminal = FakeHVACFunction.GetFanConnectingAirterminal_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomContainAirTerminalRoom = FakeHVACFunction.GetRoomContainAirTerminal_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetSmokeCompartmentsInRoomRoom = FakeHVACFunction.GetSmokeCompartmentsInRoom_new;

                //arrange

                string comment = "设计不满足规范GB50067_2014中第8.2.1条条文规定。";

                List<ComponentAnnotation> componentViolations = new List<ComponentAnnotation>();
                FakeHVACFunction.ExcelPath_new = @"D:\wangT\HVAC-Checker\UnitTestHVACChecker\测试数据\测试数据_GB50067_2014_8_2_1.xlsx";
                //打开测试数据文件
                string importExcelPath = FakeHVACFunction.ExcelPath_new;
                //打开数据文件
                IWorkbook workbook = WorkbookFactory.Create(importExcelPath);
                //读取数据表格
                ISheet sheet_rooms = workbook.GetSheet(FakeHVACFunction.roomSheetName_new);

                List<Room> rooms = new List<Room>();
                //依次读取数据行，并根据数据内容创建房间，并加入房间集合中
                for (int index = 1; index <= sheet_rooms.LastRowNum; ++index)
                {
                    IRow row = (IRow)sheet_rooms.GetRow(index);
                    if (!row.GetCell(sheet_rooms.getColNumber("是否通过")).BooleanCellValue)
                    {
                        long roomId = Convert.ToInt64(row.GetCell(sheet_rooms.getColNumber("ID")).ToString());
                        String type = row.GetCell(sheet_rooms.getColNumber("房间类型")).ToString();
                        ComponentAnnotation componentAnnotation = new ComponentAnnotation();
                        componentAnnotation.Id = roomId;
                        componentAnnotation.type = type;
                        int commentType = Convert.ToInt32(row.GetCell(sheet_rooms.getColNumber("批注类型")).ToString());
                        if (commentType == 1)
                            componentAnnotation.remark = "此车库未设置排烟系统";
                        else if (commentType == 2)
                            componentAnnotation.remark = "此车库未设置防烟分区";
                        componentViolations.Add(componentAnnotation);
                    }
                }


                //act
                BimReview result = new BimReview();
                result = HVACChecker.GB50067_2014_8_2_1();


                //assert
                Assert.AreEqual(comment, result.comment);
                Assert.IsFalse(result.isPassCheck);
                Custom_Assert.AreComponentViolationListEqual(componentViolations, result.violationComponents);
            }
        }

        [TestMethod]
        public void test_emptyRoom_pass()
        {
            using (ShimsContext.Create())
            {
                FakeHVACFunction.inital();

                FakeHVACFunction.roomSheetName_new = "房间(空房间)";

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomsString = FakeHVACFunction.GetRooms_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetWindowsInRoomRoom = FakeHVACFunction.GetWindowsInRoom_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetFanConnectingAirterminalAirTerminal = FakeHVACFunction.GetFanConnectingAirterminal_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomContainAirTerminalRoom = FakeHVACFunction.GetRoomContainAirTerminal_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetSmokeCompartmentsInRoomRoom = FakeHVACFunction.GetSmokeCompartmentsInRoom_new;

                //arrange

                string comment = "设计满足规范GB50067_2014中第8.2.1条条文规定。";

                List<ComponentAnnotation> componentViolations = new List<ComponentAnnotation>();
                FakeHVACFunction.ExcelPath_new = @"D:\wangT\HVAC-Checker\UnitTestHVACChecker\测试数据\测试数据_GB50067_2014_8_2_1.xlsx";
                //打开测试数据文件
                string importExcelPath = FakeHVACFunction.ExcelPath_new;
                //打开数据文件
                IWorkbook workbook = WorkbookFactory.Create(importExcelPath);
                //读取数据表格
                ISheet sheet_rooms = workbook.GetSheet(FakeHVACFunction.roomSheetName_new);

                List<Room> rooms = new List<Room>();
                //依次读取数据行，并根据数据内容创建房间，并加入房间集合中
                for (int index = 1; index <= sheet_rooms.LastRowNum; ++index)
                {
                    IRow row = (IRow)sheet_rooms.GetRow(index);
                    if (!row.GetCell(sheet_rooms.getColNumber("是否通过")).BooleanCellValue)
                    {
                        long roomId = Convert.ToInt64(row.GetCell(sheet_rooms.getColNumber("ID")).ToString());
                        String type = row.GetCell(sheet_rooms.getColNumber("房间类型")).ToString();
                        ComponentAnnotation componentAnnotation = new ComponentAnnotation();
                        componentAnnotation.Id = roomId;
                        componentAnnotation.type = type;
                        int commentType = Convert.ToInt32(row.GetCell(sheet_rooms.getColNumber("批注类型")).ToString());
                        if (commentType == 1)
                            componentAnnotation.remark = "此车库未设置排烟系统";
                        else if (commentType == 2)
                            componentAnnotation.remark = "此车库未设置防烟分区";
                        componentViolations.Add(componentAnnotation);
                    }
                }


                //act
                BimReview result = new BimReview();
                result = HVACChecker.GB50067_2014_8_2_1();


                //assert
                Assert.AreEqual(comment, result.comment);
                Assert.IsTrue(result.isPassCheck);
                Custom_Assert.AreComponentViolationListEqual(componentViolations, result.violationComponents);
            }
        }
    }

    [TestClass]
    public class GB50067_2014_8_2_2_Test
    {
        [TestMethod]
        public void test_pass()
        {
            using (ShimsContext.Create())
            {
                FakeHVACFunction.inital();

                FakeHVACFunction.roomSheetName_new = "房间(通过)";

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomsString = FakeHVACFunction.GetRooms_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetWindowsInRoomRoom = FakeHVACFunction.GetWindowsInRoom_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetFanConnectingAirterminalAirTerminal = FakeHVACFunction.GetFanConnectingAirterminal_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomContainAirTerminalRoom = FakeHVACFunction.GetRoomContainAirTerminal_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetSmokeCompartmentsInRoomRoom = FakeHVACFunction.GetSmokeCompartmentsInRoom_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.isSmokeCompartmentIntersectFireCompartmentSmokeCompartmentFireCompartment = FakeHVACFunction.isSmokeCompartmentIntersectFireCompartment_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetHighestStoryNoOfRoomRoom = FakeHVACFunction.getHighestStoryNoOfSmokeCompartment_new;
                //arrange

                string comment = "设计满足规范GB50067_2014中第8.2.2条条文规定。请专家复核防烟分区是否采用挡烟垂壁、隔墙或从顶棚下突出不小于0．5m的梁划分";

                List<ComponentAnnotation> componentViolations = new List<ComponentAnnotation>();
                FakeHVACFunction.ExcelPath_new = @"D:\wangT\HVAC-Checker\UnitTestHVACChecker\测试数据\测试数据_GB50067_2014_8_2_2.xlsx";
                //打开测试数据文件
                string importExcelPath = FakeHVACFunction.ExcelPath_new;
                //打开数据文件
                IWorkbook workbook = WorkbookFactory.Create(importExcelPath);
                //读取数据表格
                ISheet sheet_rooms = workbook.GetSheet(FakeHVACFunction.roomSheetName_new);

                List<Room> rooms = new List<Room>();
                //依次读取数据行，并根据数据内容创建房间，并加入房间集合中
                for (int index = 1; index <= sheet_rooms.LastRowNum; ++index)
                {
                    IRow row = (IRow)sheet_rooms.GetRow(index);
                    if (!row.GetCell(sheet_rooms.getColNumber("是否通过")).BooleanCellValue)
                    {
                        long roomId = Convert.ToInt64(row.GetCell(sheet_rooms.getColNumber("ID")).ToString());
                        String type = row.GetCell(sheet_rooms.getColNumber("房间类型")).ToString();
                        ComponentAnnotation componentAnnotation = new ComponentAnnotation();
                        componentAnnotation.Id = roomId;
                        componentAnnotation.type = type;
                        int commentType = Convert.ToInt32(row.GetCell(sheet_rooms.getColNumber("批注类型")).ToString());
                        if (commentType == 1)
                            componentAnnotation.remark = "此车库防烟分区大于2000㎡";
                        else if (commentType == 2)
                            componentAnnotation.remark = "此车库防烟分区跨越防火分区";
                        componentViolations.Add(componentAnnotation);
                    }
                }


                //act
                BimReview result = new BimReview();
                result = HVACChecker.GB50067_2014_8_2_2();


                //assert
                Assert.AreEqual(comment, result.comment);
                Assert.IsTrue(result.isPassCheck);
                Custom_Assert.AreComponentViolationListEqual(componentViolations, result.violationComponents);
            }
        }

        [TestMethod]
        public void test_unpass()
        {
            using (ShimsContext.Create())
            {
                FakeHVACFunction.inital();

                FakeHVACFunction.roomSheetName_new = "房间(不通过)";

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomsString = FakeHVACFunction.GetRooms_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetWindowsInRoomRoom = FakeHVACFunction.GetWindowsInRoom_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetFanConnectingAirterminalAirTerminal = FakeHVACFunction.GetFanConnectingAirterminal_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomContainAirTerminalRoom = FakeHVACFunction.GetRoomContainAirTerminal_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetSmokeCompartmentsInRoomRoom = FakeHVACFunction.GetSmokeCompartmentsInRoom_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.isSmokeCompartmentIntersectFireCompartmentSmokeCompartmentFireCompartment = FakeHVACFunction.isSmokeCompartmentIntersectFireCompartment_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetFireCompartmentString = FakeHVACFunction.GetFireCompartment_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetHighestStoryNoOfRoomRoom = FakeHVACFunction.getHighestStoryNoOfSmokeCompartment_new;
                //arrange

                string comment = "设计不满足规范GB50067_2014中第8.2.2条条文规定。请专家复核防烟分区是否采用挡烟垂壁、隔墙或从顶棚下突出不小于0．5m的梁划分";

                List<ComponentAnnotation> componentViolations = new List<ComponentAnnotation>();
                FakeHVACFunction.ExcelPath_new = @"D:\wangT\HVAC-Checker\UnitTestHVACChecker\测试数据\测试数据_GB50067_2014_8_2_2.xlsx";
                //打开测试数据文件
                string importExcelPath = FakeHVACFunction.ExcelPath_new;
                //打开数据文件
                IWorkbook workbook = WorkbookFactory.Create(importExcelPath);
                //读取数据表格
                ISheet sheet_rooms = workbook.GetSheet(FakeHVACFunction.roomSheetName_new);

                List<Room> rooms = new List<Room>();
                //依次读取数据行，并根据数据内容创建房间，并加入房间集合中
                for (int index = 1; index <= sheet_rooms.LastRowNum; ++index)
                {
                    IRow row = (IRow)sheet_rooms.GetRow(index);
                    if (!row.GetCell(sheet_rooms.getColNumber("是否通过")).BooleanCellValue)
                    {
                        long roomId = Convert.ToInt64(row.GetCell(sheet_rooms.getColNumber("ID")).ToString());
                        String type = row.GetCell(sheet_rooms.getColNumber("房间类型")).ToString();
                        ComponentAnnotation componentAnnotation = new ComponentAnnotation();
                        componentAnnotation.Id = roomId;
                        componentAnnotation.type = type;
                        int commentType = Convert.ToInt32(row.GetCell(sheet_rooms.getColNumber("批注类型")).ToString());
                        if (commentType == 1)
                            componentAnnotation.remark = "此车库防烟分区大于2000㎡";
                        else if (commentType == 2)
                            componentAnnotation.remark = "此车库防烟分区跨越防火分区";
                        componentViolations.Add(componentAnnotation);
                    }
                }


                //act
                BimReview result = new BimReview();
                result = HVACChecker.GB50067_2014_8_2_2();


                //assert
                Assert.AreEqual(comment, result.comment);
                Assert.IsFalse(result.isPassCheck);
                Custom_Assert.AreComponentViolationListEqual(componentViolations, result.violationComponents);
            }
        }

        [TestMethod]
        public void test_emptyRoom_pass()
        {
            using (ShimsContext.Create())
            {
                FakeHVACFunction.inital();

                FakeHVACFunction.roomSheetName_new = "房间(空房间)";

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomsString = FakeHVACFunction.GetRooms_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetWindowsInRoomRoom = FakeHVACFunction.GetWindowsInRoom_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetFanConnectingAirterminalAirTerminal = FakeHVACFunction.GetFanConnectingAirterminal_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomContainAirTerminalRoom = FakeHVACFunction.GetRoomContainAirTerminal_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetSmokeCompartmentsInRoomRoom = FakeHVACFunction.GetSmokeCompartmentsInRoom_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetHighestStoryNoOfRoomRoom = FakeHVACFunction.getHighestStoryNoOfSmokeCompartment_new;
                //arrange

                string comment = "设计满足规范GB50067_2014中第8.2.2条条文规定。请专家复核防烟分区是否采用挡烟垂壁、隔墙或从顶棚下突出不小于0．5m的梁划分";

                List<ComponentAnnotation> componentViolations = new List<ComponentAnnotation>();
                FakeHVACFunction.ExcelPath_new = @"D:\wangT\HVAC-Checker\UnitTestHVACChecker\测试数据\测试数据_GB50067_2014_8_2_2.xlsx";
                //打开测试数据文件
                string importExcelPath = FakeHVACFunction.ExcelPath_new;
                //打开数据文件
                IWorkbook workbook = WorkbookFactory.Create(importExcelPath);
                //读取数据表格
                ISheet sheet_rooms = workbook.GetSheet(FakeHVACFunction.roomSheetName_new);

                List<Room> rooms = new List<Room>();
                //依次读取数据行，并根据数据内容创建房间，并加入房间集合中
                for (int index = 1; index <= sheet_rooms.LastRowNum; ++index)
                {
                    IRow row = (IRow)sheet_rooms.GetRow(index);
                    if (!row.GetCell(sheet_rooms.getColNumber("是否通过")).BooleanCellValue)
                    {
                        long roomId = Convert.ToInt64(row.GetCell(sheet_rooms.getColNumber("ID")).ToString());
                        String type = row.GetCell(sheet_rooms.getColNumber("房间类型")).ToString();
                        ComponentAnnotation componentAnnotation = new ComponentAnnotation();
                        componentAnnotation.Id = roomId;
                        componentAnnotation.type = type;
                        int commentType = Convert.ToInt32(row.GetCell(sheet_rooms.getColNumber("批注类型")).ToString());
                        if (commentType == 1)
                            componentAnnotation.remark = "此车库未设置排烟系统";
                        else if (commentType == 2)
                            componentAnnotation.remark = "此车库未设置防烟分区";
                        componentViolations.Add(componentAnnotation);
                    }
                }


                //act
                BimReview result = new BimReview();
                result = HVACChecker.GB50067_2014_8_2_2();


                //assert
                Assert.AreEqual(comment, result.comment);
                Assert.IsTrue(result.isPassCheck);
                Custom_Assert.AreComponentViolationListEqual(componentViolations, result.violationComponents);
            }
        }
    }

    [TestClass]
    public class GB50157_2013_28_4_2_Test
    {
        [TestMethod]
        public void test_pass()
        {
            using (ShimsContext.Create())
            {
                FakeHVACFunction.inital();

                FakeHVACFunction.roomSheetName_new = "房间(通过)";

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomsString = FakeHVACFunction.GetRooms_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomsStringStringDoubleRoomPosition = FakeHVACFunction.GetRoomsMutiArgu_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetFanConnectingAirterminalAirTerminal = FakeHVACFunction.GetFanConnectingAirterminal_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomContainAirTerminalRoom = FakeHVACFunction.GetRoomContainAirTerminal_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomsMoreThanStringDouble = FakeHVACFunction.GetRoomsMoreThan_new;

                //arrange

                globalData.buildingType = "地铁建筑";

                string comment = "设计满足规范GB50157_2013中第28.4.2条条文规定。";

                List<ComponentAnnotation> componentViolations = new List<ComponentAnnotation>();
                FakeHVACFunction.ExcelPath_new = @"D:\wangT\HVAC-Checker\UnitTestHVACChecker\测试数据\测试数据_GB50157_2013_28_4_2.xlsx";
                //打开测试数据文件
                string importExcelPath = FakeHVACFunction.ExcelPath_new;
                //打开数据文件
                IWorkbook workbook = WorkbookFactory.Create(importExcelPath);
                //读取数据表格
                ISheet sheet_rooms = workbook.GetSheet(FakeHVACFunction.roomSheetName_new);

                List<Room> rooms = new List<Room>();
                //依次读取数据行，并根据数据内容创建房间，并加入房间集合中
                for (int index = 1; index <= sheet_rooms.LastRowNum; ++index)
                {
                    IRow row = (IRow)sheet_rooms.GetRow(index);
                    if (!row.GetCell(sheet_rooms.getColNumber("是否通过")).BooleanCellValue)
                    {
                        long roomId = Convert.ToInt64(row.GetCell(sheet_rooms.getColNumber("ID")).ToString());
                        String type = row.GetCell(sheet_rooms.getColNumber("房间类型")).ToString();
                        ComponentAnnotation componentAnnotation = new ComponentAnnotation();
                        componentAnnotation.Id = roomId;
                        componentAnnotation.type = type;
                        int commentType = Convert.ToInt32(row.GetCell(sheet_rooms.getColNumber("批注类型")).ToString());
                        if (commentType == 1)
                            componentAnnotation.remark = "此房间没有设置排烟系统";
                        else if (commentType == 2)
                            componentAnnotation.remark = "此房间没有设置加压送风系统";
                        componentViolations.Add(componentAnnotation);
                    }
                }


                //act
                BimReview result = new BimReview();
                result = HVACChecker.GB50157_2013_28_4_2();


                //assert
                Assert.AreEqual(comment, result.comment);
                Assert.IsTrue(result.isPassCheck);
                Custom_Assert.AreComponentViolationListEqual(componentViolations, result.violationComponents);
            }
        }

        [TestMethod]
        public void test_unpass()
        {
            using (ShimsContext.Create())
            {
                FakeHVACFunction.inital();

                FakeHVACFunction.roomSheetName_new = "房间(不通过)";

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomsString = FakeHVACFunction.GetRooms_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomsStringStringDoubleRoomPosition = FakeHVACFunction.GetRoomsMutiArgu_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetFanConnectingAirterminalAirTerminal = FakeHVACFunction.GetFanConnectingAirterminal_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomContainAirTerminalRoom = FakeHVACFunction.GetRoomContainAirTerminal_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomsMoreThanStringDouble = FakeHVACFunction.GetRoomsMoreThan_new;

                //arrange
                globalData.buildingType = "地铁建筑";

                string comment = "设计不满足规范GB50157_2013中第28.4.2条条文规定。";

                List<ComponentAnnotation> componentViolations = new List<ComponentAnnotation>();
                FakeHVACFunction.ExcelPath_new = @"D:\wangT\HVAC-Checker\UnitTestHVACChecker\测试数据\测试数据_GB50157_2013_28_4_2.xlsx";
                //打开测试数据文件
                string importExcelPath = FakeHVACFunction.ExcelPath_new;
                //打开数据文件
                IWorkbook workbook = WorkbookFactory.Create(importExcelPath);
                //读取数据表格
                ISheet sheet_rooms = workbook.GetSheet(FakeHVACFunction.roomSheetName_new);

                List<Room> rooms = new List<Room>();
                //依次读取数据行，并根据数据内容创建房间，并加入房间集合中
                for (int index = 1; index <= sheet_rooms.LastRowNum; ++index)
                {
                    IRow row = (IRow)sheet_rooms.GetRow(index);
                    if (!row.GetCell(sheet_rooms.getColNumber("是否通过")).BooleanCellValue)
                    {
                        long roomId = Convert.ToInt64(row.GetCell(sheet_rooms.getColNumber("ID")).ToString());
                        String type = row.GetCell(sheet_rooms.getColNumber("房间类型")).ToString();
                        ComponentAnnotation componentAnnotation = new ComponentAnnotation();
                        componentAnnotation.Id = roomId;
                        componentAnnotation.type = type;
                        int commentType = Convert.ToInt32(row.GetCell(sheet_rooms.getColNumber("批注类型")).ToString());
                        if (commentType == 1)
                            componentAnnotation.remark = "此房间没有设置排烟系统";
                        else if (commentType == 2)
                            componentAnnotation.remark = "此房间没有设置加压送风系统";
                        componentViolations.Add(componentAnnotation);
                    }
                }


                //act
                BimReview result = new BimReview();
                result = HVACChecker.GB50157_2013_28_4_2();


                //assert
                Assert.AreEqual(comment, result.comment);
                Assert.IsFalse(result.isPassCheck);
                Custom_Assert.AreComponentViolationListEqual(componentViolations, result.violationComponents);
            }
        }

        [TestMethod]
        public void test_emptyRoom_pass()
        {
            using (ShimsContext.Create())
            {
                FakeHVACFunction.inital();

                FakeHVACFunction.roomSheetName_new = "房间(空房间)";

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomsString = FakeHVACFunction.GetRooms_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomsStringStringDoubleRoomPosition = FakeHVACFunction.GetRoomsMutiArgu_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetFanConnectingAirterminalAirTerminal = FakeHVACFunction.GetFanConnectingAirterminal_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomContainAirTerminalRoom = FakeHVACFunction.GetRoomContainAirTerminal_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomsMoreThanStringDouble = FakeHVACFunction.GetRoomsMoreThan_new;

                //arrange

                globalData.buildingType = "地铁建筑";

                string comment = "设计满足规范GB50157_2013中第28.4.2条条文规定。";

                List<ComponentAnnotation> componentViolations = new List<ComponentAnnotation>();
                FakeHVACFunction.ExcelPath_new = @"D:\wangT\HVAC-Checker\UnitTestHVACChecker\测试数据\测试数据_GB50157_2013_28_4_2.xlsx";
                //打开测试数据文件
                string importExcelPath = FakeHVACFunction.ExcelPath_new;
                //打开数据文件
                IWorkbook workbook = WorkbookFactory.Create(importExcelPath);
                //读取数据表格
                ISheet sheet_rooms = workbook.GetSheet(FakeHVACFunction.roomSheetName_new);

                List<Room> rooms = new List<Room>();
                //依次读取数据行，并根据数据内容创建房间，并加入房间集合中
                for (int index = 1; index <= sheet_rooms.LastRowNum; ++index)
                {
                    IRow row = (IRow)sheet_rooms.GetRow(index);
                    if (!row.GetCell(sheet_rooms.getColNumber("是否通过")).BooleanCellValue)
                    {
                        long roomId = Convert.ToInt64(row.GetCell(sheet_rooms.getColNumber("ID")).ToString());
                        String type = row.GetCell(sheet_rooms.getColNumber("房间类型")).ToString();
                        ComponentAnnotation componentAnnotation = new ComponentAnnotation();
                        componentAnnotation.Id = roomId;
                        componentAnnotation.type = type;
                        int commentType = Convert.ToInt32(row.GetCell(sheet_rooms.getColNumber("批注类型")).ToString());
                        if (commentType == 1)
                            componentAnnotation.remark = "此房间没有设置排烟系统";
                        else if (commentType == 2)
                            componentAnnotation.remark = "此房间没有设置加压送风系统";
                        componentViolations.Add(componentAnnotation);
                    }
                }


                //act
                BimReview result = new BimReview();
                result = HVACChecker.GB50157_2013_28_4_2();


                //assert
                Assert.AreEqual(comment, result.comment);
                Assert.IsTrue(result.isPassCheck);
                Custom_Assert.AreComponentViolationListEqual(componentViolations, result.violationComponents);
            }
        }
    }

    [TestClass]
    public class GB50490_2009_8_4_19_Test
    {
        [TestMethod]
        public void test_pass()
        {
            using (ShimsContext.Create())
            {
                FakeHVACFunction.inital();

                FakeHVACFunction.roomSheetName_new = "房间(通过)";

                FakeHVACFunction.fanSheetName_new = "风机(通过)";

                FakeHVACFunction.smokeCompartmentSheetName_new = "防烟分区(通过)";


                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomsStringStringDoubleRoomPosition = FakeHVACFunction.GetRoomsMutiArgu_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetFanConnectingAirterminalAirTerminal = FakeHVACFunction.GetFanConnectingAirterminal_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomContainAirTerminalRoom = FakeHVACFunction.GetRoomContainAirTerminal_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetSmokeCompartmentsInRoomRoom = FakeHVACFunction.GetSmokeCompartmentsInRoom_new;


                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetWindowsInRoomRoom = FakeHVACFunction.GetWindowsInRoom_new;
                //arrange

                globalData.buildingType = "城市轨道交通建筑";

                string comment = "设计满足规范GB50490_2009中第8.4.19条条文规定。";

                List<ComponentAnnotation> componentViolations = new List<ComponentAnnotation>();
                FakeHVACFunction.ExcelPath_new = @"D:\wangT\HVAC-Checker\UnitTestHVACChecker\测试数据\测试数据_GB50490_2009_8_4_19.xlsx";
                //打开测试数据文件
                string importExcelPath = FakeHVACFunction.ExcelPath_new;
                //打开数据文件
                IWorkbook workbook = WorkbookFactory.Create(importExcelPath);
                //读取数据表格
                ISheet sheet_smokeCompartments = workbook.GetSheet(FakeHVACFunction.smokeCompartmentSheetName_new);

                ISheet sheet_fans = workbook.GetSheet(FakeHVACFunction.fanSheetName_new);

                //依次读取数据行，并根据数据内容创建房间，并加入房间集合中
                for (int index = 1; index <= sheet_smokeCompartments.LastRowNum; ++index)
                {
                    IRow row = (IRow)sheet_smokeCompartments.GetRow(index);
                    if (!row.GetCell(sheet_smokeCompartments.getColNumber("是否通过")).BooleanCellValue)
                    {
                        long smokeCompartmentId = Convert.ToInt64(row.GetCell(sheet_smokeCompartments.getColNumber("ID")).ToString());
                        String type = "防烟分区";
                        ComponentAnnotation componentAnnotation = new ComponentAnnotation();
                        componentAnnotation.Id = smokeCompartmentId;
                        componentAnnotation.type = type;
                        int commentType = Convert.ToInt32(row.GetCell(sheet_smokeCompartments.getColNumber("批注类型")).ToString());
                            componentAnnotation.remark = "防烟分区排烟量不满足规范要求";
                        componentViolations.Add(componentAnnotation);
                    }
                }

                //依次读取数据行，并根据数据内容创建房间，并加入房间集合中
                for (int index = 1; index <= sheet_fans.LastRowNum; ++index)
                {
                    IRow row = (IRow)sheet_fans.GetRow(index);
                    if (!row.GetCell(sheet_fans.getColNumber("是否通过")).BooleanCellValue)
                    {
                        long fanId = Convert.ToInt64(row.GetCell(sheet_fans.getColNumber("ID")).ToString());
                        String type = "风机";
                        ComponentAnnotation componentAnnotation = new ComponentAnnotation();
                        componentAnnotation.Id = fanId;
                        componentAnnotation.type = type;
                        int commentType = Convert.ToInt32(row.GetCell(sheet_smokeCompartments.getColNumber("批注类型")).ToString());
                        componentAnnotation.remark = "此风机排烟量不满足规范要求";
                        componentViolations.Add(componentAnnotation);
                    }
                }

                //act
                BimReview result = new BimReview();
                result = HVACChecker.GB50490_2009_8_4_19();

                //assert
                Assert.AreEqual(comment, result.comment);
                Assert.IsTrue(result.isPassCheck);
                Custom_Assert.AreComponentViolationListEqual(componentViolations, result.violationComponents);
            }
        }

        [TestMethod]
        public void test_unpass()
        {
            using (ShimsContext.Create())
            {
                FakeHVACFunction.inital();

                FakeHVACFunction.roomSheetName_new = "房间(不通过)";

                FakeHVACFunction.fanSheetName_new = "风机(不通过)";

                FakeHVACFunction.smokeCompartmentSheetName_new = "防烟分区(不通过)";


                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomsStringStringDoubleRoomPosition = FakeHVACFunction.GetRoomsMutiArgu_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetFanConnectingAirterminalAirTerminal = FakeHVACFunction.GetFanConnectingAirterminal_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomContainAirTerminalRoom = FakeHVACFunction.GetRoomContainAirTerminal_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetSmokeCompartmentsInRoomRoom = FakeHVACFunction.GetSmokeCompartmentsInRoom_new;


                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetWindowsInRoomRoom = FakeHVACFunction.GetWindowsInRoom_new;
                //arrange

                globalData.buildingType = "城市轨道交通建筑";

                string comment = "设计不满足规范GB50490_2009中第8.4.19条条文规定。";

                List<ComponentAnnotation> componentViolations = new List<ComponentAnnotation>();
                FakeHVACFunction.ExcelPath_new = @"D:\wangT\HVAC-Checker\UnitTestHVACChecker\测试数据\测试数据_GB50490_2009_8_4_19.xlsx";
                //打开测试数据文件
                string importExcelPath = FakeHVACFunction.ExcelPath_new;
                //打开数据文件
                IWorkbook workbook = WorkbookFactory.Create(importExcelPath);
                //读取数据表格
                ISheet sheet_smokeCompartments = workbook.GetSheet(FakeHVACFunction.smokeCompartmentSheetName_new);

                ISheet sheet_fans = workbook.GetSheet(FakeHVACFunction.fanSheetName_new);

                //依次读取数据行，并根据数据内容创建房间，并加入房间集合中
                for (int index = 1; index <= sheet_smokeCompartments.LastRowNum; ++index)
                {
                    IRow row = (IRow)sheet_smokeCompartments.GetRow(index);
                    if (!row.GetCell(sheet_smokeCompartments.getColNumber("是否通过")).BooleanCellValue)
                    {
                        long smokeCompartmentId = Convert.ToInt64(row.GetCell(sheet_smokeCompartments.getColNumber("ID")).ToString());
                        String type = "防烟分区";
                        ComponentAnnotation componentAnnotation = new ComponentAnnotation();
                        componentAnnotation.Id = smokeCompartmentId;
                        componentAnnotation.type = type;
                        componentAnnotation.remark = "防烟分区排烟量不满足规范要求";
                        componentViolations.Add(componentAnnotation);
                    }
                }

                //依次读取数据行，并根据数据内容创建房间，并加入房间集合中
                for (int index = 1; index <= sheet_fans.LastRowNum; ++index)
                {
                    IRow row = (IRow)sheet_fans.GetRow(index);
                    if (!row.GetCell(sheet_fans.getColNumber("是否通过")).BooleanCellValue)
                    {
                        long fanId = Convert.ToInt64(row.GetCell(sheet_fans.getColNumber("ID")).ToString());
                        String type = "风机";
                        ComponentAnnotation componentAnnotation = new ComponentAnnotation();
                        componentAnnotation.Id = fanId;
                        componentAnnotation.type = type;
                        componentAnnotation.remark = "此风机排烟量不满足规范要求";
                        componentViolations.Add(componentAnnotation);
                    }
                }

                //act
                BimReview result = new BimReview();
                result = HVACChecker.GB50490_2009_8_4_19();


                //assert
                Assert.AreEqual(comment, result.comment);
                Assert.IsFalse(result.isPassCheck);
                Custom_Assert.AreComponentViolationListEqual(componentViolations, result.violationComponents);
            }
        }

        [TestMethod]
        public void test_emptyRoom_pass()
        {
            using (ShimsContext.Create())
            {
                FakeHVACFunction.inital();

                FakeHVACFunction.roomSheetName_new = "房间(空房间)";

                FakeHVACFunction.fanSheetName_new = "风机(通过)";

                FakeHVACFunction.smokeCompartmentSheetName_new = "防烟分区(通过)";


                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomsStringStringDoubleRoomPosition = FakeHVACFunction.GetRoomsMutiArgu_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetFanConnectingAirterminalAirTerminal = FakeHVACFunction.GetFanConnectingAirterminal_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomContainAirTerminalRoom = FakeHVACFunction.GetRoomContainAirTerminal_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetSmokeCompartmentsInRoomRoom = FakeHVACFunction.GetSmokeCompartmentsInRoom_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetWindowsInRoomRoom = FakeHVACFunction.GetWindowsInRoom_new;
                //arrange

                globalData.buildingType = "城市轨道交通建筑";

                string comment = "设计满足规范GB50490_2009中第8.4.19条条文规定。";

                List<ComponentAnnotation> componentViolations = new List<ComponentAnnotation>();
                FakeHVACFunction.ExcelPath_new = @"D:\wangT\HVAC-Checker\UnitTestHVACChecker\测试数据\测试数据_GB50490_2009_8_4_19.xlsx";
                //打开测试数据文件
                string importExcelPath = FakeHVACFunction.ExcelPath_new;
                //打开数据文件
                IWorkbook workbook = WorkbookFactory.Create(importExcelPath);
                //读取数据表格
                ISheet sheet_smokeCompartments = workbook.GetSheet(FakeHVACFunction.smokeCompartmentSheetName_new);

                ISheet sheet_fans = workbook.GetSheet(FakeHVACFunction.fanSheetName_new);

                //依次读取数据行，并根据数据内容创建房间，并加入房间集合中
                for (int index = 1; index <= sheet_smokeCompartments.LastRowNum; ++index)
                {
                    IRow row = (IRow)sheet_smokeCompartments.GetRow(index);
                    if (!row.GetCell(sheet_smokeCompartments.getColNumber("是否通过")).BooleanCellValue)
                    {
                        long smokeCompartmentId = Convert.ToInt64(row.GetCell(sheet_smokeCompartments.getColNumber("ID")).ToString());
                        String type = "防烟分区";
                        ComponentAnnotation componentAnnotation = new ComponentAnnotation();
                        componentAnnotation.Id = smokeCompartmentId;
                        componentAnnotation.type = type;
                        int commentType = Convert.ToInt32(row.GetCell(sheet_smokeCompartments.getColNumber("批注类型")).ToString());
                        componentAnnotation.remark = "防烟分区排烟量不满足规范要求";
                        componentViolations.Add(componentAnnotation);
                    }
                }

                //依次读取数据行，并根据数据内容创建房间，并加入房间集合中
                for (int index = 1; index <= sheet_fans.LastRowNum; ++index)
                {
                    IRow row = (IRow)sheet_fans.GetRow(index);
                    if (!row.GetCell(sheet_fans.getColNumber("是否通过")).BooleanCellValue)
                    {
                        long fanId = Convert.ToInt64(row.GetCell(sheet_fans.getColNumber("ID")).ToString());
                        String type = "风机";
                        ComponentAnnotation componentAnnotation = new ComponentAnnotation();
                        componentAnnotation.Id = fanId;
                        componentAnnotation.type = type;
                        int commentType = Convert.ToInt32(row.GetCell(sheet_smokeCompartments.getColNumber("批注类型")).ToString());
                        componentAnnotation.remark = "此风机排烟量不满足规范要求";
                        componentViolations.Add(componentAnnotation);
                    }
                }

                //act
                BimReview result = new BimReview();
                result = HVACChecker.GB50490_2009_8_4_19();

                //assert
                Assert.AreEqual(comment, result.comment);
                Assert.IsTrue(result.isPassCheck);
                Custom_Assert.AreComponentViolationListEqual(componentViolations, result.violationComponents);
            }
        }
    }


    [TestClass]
    public class GB50736_2012_6_3_6_Test
    {
        [TestMethod]
        public void test_pass()
        {
            using (ShimsContext.Create())
            {
                FakeHVACFunction.inital();

                FakeHVACFunction.roomSheetName_new = "房间(通过)";

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomsString= FakeHVACFunction.GetRooms_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetFanConnectingAirterminalAirTerminal = FakeHVACFunction.GetFanConnectingAirterminal_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomContainAirTerminalRoom = FakeHVACFunction.GetRoomContainAirTerminal_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetWindowsInRoomRoom = FakeHVACFunction.GetWindowsInRoom_new;
                //arrange

                string comment = "设计满足规范GB50736_2012中第6.3.6条条文规定。";

                List<ComponentAnnotation> componentViolations = new List<ComponentAnnotation>();
                FakeHVACFunction.ExcelPath_new = @"D:\wangT\HVAC-Checker\UnitTestHVACChecker\测试数据\测试数据_GB50736_2012_6_3_6.xlsx";
                //打开测试数据文件
                string importExcelPath = FakeHVACFunction.ExcelPath_new;
                //打开数据文件
                IWorkbook workbook = WorkbookFactory.Create(importExcelPath);

                //读取数据表格
                ISheet sheet_rooms = workbook.GetSheet(FakeHVACFunction.roomSheetName_new);

                List<Room> rooms = new List<Room>();
                //依次读取数据行，并根据数据内容创建房间，并加入房间集合中
                for (int index = 1; index <= sheet_rooms.LastRowNum; ++index)
                {
                    IRow row = (IRow)sheet_rooms.GetRow(index);
                    if (!row.GetCell(sheet_rooms.getColNumber("是否通过")).BooleanCellValue)
                    {
                        long roomId = Convert.ToInt64(row.GetCell(sheet_rooms.getColNumber("ID")).ToString());
                        String type = row.GetCell(sheet_rooms.getColNumber("房间类型")).ToString();
                        ComponentAnnotation componentAnnotation = new ComponentAnnotation();
                        componentAnnotation.Id = roomId;
                        componentAnnotation.type = type;
                        int commentType = Convert.ToInt32(row.GetCell(sheet_rooms.getColNumber("批注类型")).ToString());
                        if (commentType == 1)
                            componentAnnotation.remark = "公共卫生间没有设置机械排风系统";
                        else if (commentType == 2)
                            componentAnnotation.remark = "公共卫生间未保持负压";
                        else if(commentType==3)
                            componentAnnotation.remark = "公共卫生间换气次数不满足规范要求";
                        else if (commentType == 4)
                            componentAnnotation.remark = "公共浴室没有设置排风系统";
                        else if (commentType == 5)
                            componentAnnotation.remark = "公共浴室未保持负压";
                        else if (commentType == 6)
                            componentAnnotation.remark = "公共浴室换气次数不满足规范要求";
                        componentViolations.Add(componentAnnotation);
                    }
                }

                //act
                BimReview result = new BimReview();
                result = HVACChecker.GB50736_2012_6_3_6();

                //assert
                Assert.AreEqual(comment, result.comment);
                Assert.IsTrue(result.isPassCheck);
                Custom_Assert.AreComponentViolationListEqual(componentViolations, result.violationComponents);
            }
        }

        [TestMethod]
        public void test_unpass()
        {
            using (ShimsContext.Create())
            {
                FakeHVACFunction.inital();

                FakeHVACFunction.roomSheetName_new = "房间(不通过)";

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomsString = FakeHVACFunction.GetRooms_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetFanConnectingAirterminalAirTerminal = FakeHVACFunction.GetFanConnectingAirterminal_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomContainAirTerminalRoom = FakeHVACFunction.GetRoomContainAirTerminal_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetWindowsInRoomRoom = FakeHVACFunction.GetWindowsInRoom_new;
                //arrange

                string comment = "设计不满足规范GB50736_2012中第6.3.6条条文规定。";

                List<ComponentAnnotation> componentViolations = new List<ComponentAnnotation>();
                FakeHVACFunction.ExcelPath_new = @"D:\wangT\HVAC-Checker\UnitTestHVACChecker\测试数据\测试数据_GB50736_2012_6_3_6.xlsx";
                //打开测试数据文件
                string importExcelPath = FakeHVACFunction.ExcelPath_new;
                //打开数据文件
                IWorkbook workbook = WorkbookFactory.Create(importExcelPath);

                //读取数据表格
                ISheet sheet_rooms = workbook.GetSheet(FakeHVACFunction.roomSheetName_new);

                List<Room> rooms = new List<Room>();
                //依次读取数据行，并根据数据内容创建房间，并加入房间集合中
                for (int index = 1; index <= sheet_rooms.LastRowNum; ++index)
                {
                    IRow row = (IRow)sheet_rooms.GetRow(index);
                    if (!row.GetCell(sheet_rooms.getColNumber("是否通过")).BooleanCellValue)
                    {
                        long roomId = Convert.ToInt64(row.GetCell(sheet_rooms.getColNumber("ID")).ToString());
                        String type = row.GetCell(sheet_rooms.getColNumber("房间类型")).ToString();
                        ComponentAnnotation componentAnnotation = new ComponentAnnotation();
                        componentAnnotation.Id = roomId;
                        componentAnnotation.type = type;
                        int commentType = Convert.ToInt32(row.GetCell(sheet_rooms.getColNumber("批注类型")).ToString());
                        if (commentType == 1)
                            componentAnnotation.remark = "公共卫生间没有设置机械排风系统";
                        else if (commentType == 2)
                            componentAnnotation.remark = "公共卫生间未保持负压";
                        else if (commentType == 3)
                            componentAnnotation.remark = "公共卫生间换气次数不满足规范要求";
                        else if (commentType == 4)
                            componentAnnotation.remark = "公共浴室没有设置排风系统";
                        else if (commentType == 5)
                            componentAnnotation.remark = "公共浴室未保持负压";
                        else if (commentType == 6)
                            componentAnnotation.remark = "公共浴室换气次数不满足规范要求";
                        componentViolations.Add(componentAnnotation);
                    }
                }

                //act
                BimReview result = new BimReview();
                result = HVACChecker.GB50736_2012_6_3_6();

                //assert
                Assert.AreEqual(comment, result.comment);
                Assert.IsFalse(result.isPassCheck);
                Custom_Assert.AreComponentViolationListEqual(componentViolations, result.violationComponents);
            }
        }

        [TestMethod]
        public void test_emptyRoom_pass()
        {
            using (ShimsContext.Create())
            {
                FakeHVACFunction.inital();

                FakeHVACFunction.roomSheetName_new = "房间(空房间)";

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomsString = FakeHVACFunction.GetRooms_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetFanConnectingAirterminalAirTerminal = FakeHVACFunction.GetFanConnectingAirterminal_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomContainAirTerminalRoom = FakeHVACFunction.GetRoomContainAirTerminal_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetWindowsInRoomRoom = FakeHVACFunction.GetWindowsInRoom_new;

                //arrange

                string comment = "设计满足规范GB50736_2012中第6.3.6条条文规定。";

                List<ComponentAnnotation> componentViolations = new List<ComponentAnnotation>();
                FakeHVACFunction.ExcelPath_new = @"D:\wangT\HVAC-Checker\UnitTestHVACChecker\测试数据\测试数据_GB50490_2009_8_4_19.xlsx";
                //打开测试数据文件
                string importExcelPath = FakeHVACFunction.ExcelPath_new;
                //打开数据文件
                IWorkbook workbook = WorkbookFactory.Create(importExcelPath);

                //读取数据表格
                ISheet sheet_rooms = workbook.GetSheet(FakeHVACFunction.roomSheetName_new);

                List<Room> rooms = new List<Room>();
                //依次读取数据行，并根据数据内容创建房间，并加入房间集合中
                for (int index = 1; index <= sheet_rooms.LastRowNum; ++index)
                {
                    IRow row = (IRow)sheet_rooms.GetRow(index);
                    if (!row.GetCell(sheet_rooms.getColNumber("是否通过")).BooleanCellValue)
                    {
                        long roomId = Convert.ToInt64(row.GetCell(sheet_rooms.getColNumber("ID")).ToString());
                        String type = row.GetCell(sheet_rooms.getColNumber("房间类型")).ToString();
                        ComponentAnnotation componentAnnotation = new ComponentAnnotation();
                        componentAnnotation.Id = roomId;
                        componentAnnotation.type = type;
                        int commentType = Convert.ToInt32(row.GetCell(sheet_rooms.getColNumber("批注类型")).ToString());
                        if (commentType == 1)
                            componentAnnotation.remark = "公共卫生间没有设置机械排风系统";
                        else if (commentType == 2)
                            componentAnnotation.remark = "公共卫生间未保持负压";
                        else if (commentType == 3)
                            componentAnnotation.remark = "公共卫生间换气次数不满足规范要求";
                        else if (commentType == 4)
                            componentAnnotation.remark = "公共浴室没有设置排风系统";
                        else if (commentType == 5)
                            componentAnnotation.remark = "公共浴室未保持负压";
                        else if (commentType == 6)
                            componentAnnotation.remark = "公共浴室换气次数不满足规范要求";
                        componentViolations.Add(componentAnnotation);
                    }
                }

                //act
                BimReview result = new BimReview();
                result = HVACChecker.GB50736_2012_6_3_6();

                //assert
                Assert.AreEqual(comment, result.comment);
                Assert.IsTrue(result.isPassCheck);
                Custom_Assert.AreComponentViolationListEqual(componentViolations, result.violationComponents);
            }
        }
    }

    [TestClass]
    public class GB50157_2013_28_4_22_Test
    {
        [TestMethod]
        public void test_pass()
        {
            using (ShimsContext.Create())
            {
                FakeHVACFunction.inital();

                FakeHVACFunction.ductSheetName_new = "风管(通过)";

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomsString = FakeHVACFunction.GetRooms_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetFireCompartmentString = FakeHVACFunction.GetFireCompartment_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetDuctsCrossFireDistrictFireCompartment = FakeHVACFunction.GetDuctsCrossFireDistrict_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.getAllDuctsInRoomRoom = FakeHVACFunction.getAllDuctsInRoom_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetDuctsCrossSpaceRoom = FakeHVACFunction.GetDuctsCrossSpace_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.getAllVerticalDuctConnectedToDuctDuct = FakeHVACFunction.getAllVerticalDuctConnectedToDuct_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.getFireDamperOfDuctDuct = FakeHVACFunction.getFireDamperOfDuct_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetDuctsCrossMovementJointAndFireSide = FakeHVACFunction.GetDuctsCrossMovementJointAndFireSide_new;
                //arrange



                string comment = "设计满足规范GB50157_2013中第28.4.22条条文规定。";

                List<ComponentAnnotation> componentViolations = new List<ComponentAnnotation>();
                FakeHVACFunction.ExcelPath_new = @"D:\wangT\HVAC-Checker\UnitTestHVACChecker\测试数据\测试数据_GB50157_2013_28_4_22.xlsx";
                //打开测试数据文件
                string importExcelPath = FakeHVACFunction.ExcelPath_new;
                //打开数据文件
                IWorkbook workbook = WorkbookFactory.Create(importExcelPath);
                //读取数据表格
                ISheet sheet_ducts = workbook.GetSheet(FakeHVACFunction.ductSheetName_new);

                List<Duct> ducts = new List<Duct>();
                //依次读取数据行，并根据数据内容创建房间，并加入房间集合中
                for (int index = 1; index <= sheet_ducts.LastRowNum; ++index)
                {
                    IRow row = (IRow)sheet_ducts.GetRow(index);
                    if (!row.GetCell(sheet_ducts.getColNumber("是否通过")).BooleanCellValue)
                    {
                        long ductId = Convert.ToInt64(row.GetCell(sheet_ducts.getColNumber("ID")).ToString());

                        ComponentAnnotation componentAnnotation = new ComponentAnnotation();
                        componentAnnotation.Id = ductId;
                        componentAnnotation.type = "风管";
                        int commentType = Convert.ToInt32(row.GetCell(sheet_ducts.getColNumber("批注类型")).ToString());
                        componentAnnotation.remark = "此风管未设置防火阀";
                        componentViolations.Add(componentAnnotation);
                    }
                }


                //act
                BimReview result = new BimReview();
                result = HVACChecker.GB50157_2013_28_4_22();


                //assert
                Assert.AreEqual(comment, result.comment);
                Assert.IsTrue(result.isPassCheck);
                Custom_Assert.AreComponentViolationListEqual(componentViolations, result.violationComponents);
            }
        }

        [TestMethod]
        public void test_unpass()
        {
            using (ShimsContext.Create())
            {
                FakeHVACFunction.inital();

                FakeHVACFunction.ductSheetName_new = "风管(不通过)";

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomsString = FakeHVACFunction.GetRooms_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetFireCompartmentString = FakeHVACFunction.GetFireCompartment_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetDuctsCrossFireDistrictFireCompartment = FakeHVACFunction.GetDuctsCrossFireDistrict_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.getAllDuctsInRoomRoom = FakeHVACFunction.getAllDuctsInRoom_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetDuctsCrossSpaceRoom = FakeHVACFunction.GetDuctsCrossSpace_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.getAllVerticalDuctConnectedToDuctDuct = FakeHVACFunction.getAllVerticalDuctConnectedToDuct_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.getFireDamperOfDuctDuct = FakeHVACFunction.getFireDamperOfDuct_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetDuctsCrossMovementJointAndFireSide = FakeHVACFunction.GetDuctsCrossMovementJointAndFireSide_new;
                //arrange



                string comment = "设计不满足规范GB50157_2013中第28.4.22条条文规定。";

                List<ComponentAnnotation> componentViolations = new List<ComponentAnnotation>();
                FakeHVACFunction.ExcelPath_new = @"D:\wangT\HVAC-Checker\UnitTestHVACChecker\测试数据\测试数据_GB50157_2013_28_4_22.xlsx";
                //打开测试数据文件
                string importExcelPath = FakeHVACFunction.ExcelPath_new;
                //打开数据文件
                IWorkbook workbook = WorkbookFactory.Create(importExcelPath);
                //读取数据表格
                ISheet sheet_ducts = workbook.GetSheet(FakeHVACFunction.ductSheetName_new);

                List<Duct> ducts = new List<Duct>();
                //依次读取数据行，并根据数据内容创建房间，并加入房间集合中
                for (int index = 1; index <= sheet_ducts.LastRowNum; ++index)
                {
                    IRow row = (IRow)sheet_ducts.GetRow(index);
                    if (!row.GetCell(sheet_ducts.getColNumber("是否通过")).BooleanCellValue)
                    {
                        long ductId = Convert.ToInt64(row.GetCell(sheet_ducts.getColNumber("ID")).ToString());

                        ComponentAnnotation componentAnnotation = new ComponentAnnotation();
                        componentAnnotation.Id = ductId;
                        componentAnnotation.type = "风管";
                        int commentType = Convert.ToInt32(row.GetCell(sheet_ducts.getColNumber("批注类型")).ToString());
                        componentAnnotation.remark = "此风管未设置防火阀";
                        componentViolations.Add(componentAnnotation);
                    }
                }


                //act
                BimReview result = new BimReview();
                result = HVACChecker.GB50157_2013_28_4_22();

                //assert
                Assert.AreEqual(comment, result.comment);
                Assert.IsFalse(result.isPassCheck);
                Custom_Assert.AreComponentViolationListEqual(componentViolations, result.violationComponents);
            }
        }
    }

    public static class FakeHVACFunction
    {
        public static string testDataTableName { get; set; }

        private static string ExcelPath = @"D:\wangT\HVAC-Checker\UnitTestHVACChecker\测试数据\测试数据.xlsx";

        public static string ExcelPath_new = @"D:\wangT\HVAC-Checker\UnitTestHVACChecker\测试数据\测试数据_new.xlsx";


        public static string roomSheetName_new = "房间";

        public static string smokeCompartmentSheetName_new = "防烟分区";

        public static string fireDistrictSheetName_new = "防火分区";

        public static string fanSheetName_new = "风机";

        public static string ductSheetName_new = "风管";

        public static string movementJointName_new = "变形缝";
        public static void inital()
        {
            roomSheetName_new = "房间";

            smokeCompartmentSheetName_new = "防烟分区";

            fireDistrictSheetName_new = "防火分区";

            fanSheetName_new = "风机";

            ductSheetName_new = "风管";

            movementJointName_new = "变形缝";
        }
        public static string systemType { get; set; }
        public static List<AirTerminal> GetRoomContainAirTerminal(Room room)
        {
            string importExcelPath = ExcelPath;
            //打开测试数据文件
            IWorkbook workbook = WorkbookFactory.Create(importExcelPath);
            //读取测试数据表
            ISheet sheet = workbook.GetSheet(testDataTableName);
            List<AirTerminal> airTerminals = new List<AirTerminal>();
            //用房间ID找到测试表对应的行
            long roomId = room.Id.Value;
            IRow row = (IRow)sheet.GetRow((int)roomId);
            //从对应行中读取房间中是否有正压送风口数据
            bool isHasPressureTerminal = row.GetCell(2).BooleanCellValue;
            //如果有正压送风口则新建一个系统类型为正压送风口的对象并加入风口集合中
            if (isHasPressureTerminal)
            {
                AirTerminal airTerminal = new AirTerminal(roomId);
                airTerminal.systemType = systemType;
                airTerminals.Add(airTerminal);
            }
            return airTerminals;
        }

        public static int getHighestStoryNoOfRoom_new(Room room)
        {
            string importExcelPath = ExcelPath_new;
            //打开测试数据文件
            IWorkbook workbook = WorkbookFactory.Create(importExcelPath);
            //读取测试数据表
            ISheet sheet_rooms = workbook.GetSheet(roomSheetName_new);

            long roomId = room.Id.Value;
            IRow row = (IRow)sheet_rooms.GetRow((int)roomId);

            int storyNo = (int)row.GetCell(sheet_rooms.getColNumber("房间最高楼层编号")).NumericCellValue;
            return storyNo;
        }

        public static int getHighestStoryNoOfSmokeCompartment_new(Room smokeCompartment)
        {
            string importExcelPath = ExcelPath_new;
            //打开测试数据文件
            IWorkbook workbook = WorkbookFactory.Create(importExcelPath);
            //读取测试数据表
            ISheet sheet_rooms = workbook.GetSheet("防烟分区");

            long roomId = smokeCompartment.Id.Value;
            IRow row = (IRow)sheet_rooms.GetRow((int)roomId);

            int storyNo = (int)row.GetCell(sheet_rooms.getColNumber("房间最高楼层编号")).NumericCellValue;
            return storyNo;
        }
        public static List<AirTerminal> GetRoomContainAirTerminal_new(Room room)
        {
            string importExcelPath = ExcelPath_new;
            //打开测试数据文件
            IWorkbook workbook = WorkbookFactory.Create(importExcelPath);
            //读取测试数据表
            ISheet sheet_rooms = null;
            if (room is SmokeCompartment)
            {
                sheet_rooms = workbook.GetSheet(smokeCompartmentSheetName_new);
            }
            else
            {
                sheet_rooms = workbook.GetSheet(roomSheetName_new);
            }
            ISheet sheet_airTerminals = workbook.GetSheet("风口");
            ISheet sheet_fans = workbook.GetSheet(fanSheetName_new);
            List<AirTerminal> airTerminals = new List<AirTerminal>();
            //用房间ID找到测试表对应的行
            long roomId = room.Id.Value;
            IRow row = (IRow)sheet_rooms.GetRow((int)roomId);
            //从对应行中读取房间中所含的所有风口的集合
            string idString= row.GetCell(sheet_rooms.getColNumber("包含的风口")).ToString();
            airTerminals = getAllTerminalsByIdString(idString);
            return airTerminals;
        }

        public static List<Fan> GetFanConnectingAirterminal(AirTerminal airTerminal)
        {
            string importExcelPath = ExcelPath;
            //打开数据文件
            IWorkbook workbook = WorkbookFactory.Create(importExcelPath);
            //读取数据表格
            ISheet sheet = workbook.GetSheet(testDataTableName);
            //找到与风口id对应的数据行
            long airTerminalId = airTerminal.Id.Value;
            IRow row = (IRow)sheet.GetRow((int)airTerminalId);
            //查表确定此风口是否连接了风机
            bool isAirTerminalLinkedFan = row.GetCell(3).BooleanCellValue;
            //如果连接了风机，则创建一个风机对象，并加入风机集合中
            List<Fan> fans = new List<Fan>();
            if (isAirTerminalLinkedFan)
            {
                Fan fan = new Fan(airTerminalId);
                fans.Add(fan);
            }
            return fans;
        }

        public static List<Fan> GetFanConnectingAirterminal_new(AirTerminal aimAirTerminal)
        {
            string importExcelPath = ExcelPath_new;
            //打开数据文件
            IWorkbook workbook = WorkbookFactory.Create(importExcelPath);
            //读取数据表格
            List<Fan> fans = new List<Fan>();
            ISheet sheet_fans = workbook.GetSheet(fanSheetName_new);
            for (int index = 1; index <= sheet_fans.LastRowNum; ++index)
            {
                IRow row = (IRow)sheet_fans.GetRow(index);
                //获得风机的连接的送风口的ID字符串
                string idList = row.GetCell(sheet_fans.getColNumber("连接的送风口ID")).ToString();
                //获得风机所连的所有送风口集合
                List<AirTerminal> airTerminals = getAllTerminalsByIdString(idList);
                //获得风机的连接的排风口的ID字符串
                idList = row.GetCell(sheet_fans.getColNumber("连接的排风口ID"))?.ToString();
                airTerminals.AddRange(getAllTerminalsByIdString(idList));
                //查看风口集合中是否有aimAirTerminal
                if(airTerminals.findItem(aimAirTerminal)!=null)
                {
                    Fan fan= getFanById(index);
                    fans = new List<Fan>();
                    fans.Add(fan);
                    return fans;
                }
            }
            return fans;
        }
        public static bool isSmokeCompartmentIntersectFireCompartment_new(SmokeCompartment smokeCompartment, FireCompartment fireCompartment)
        {
            string importExcelPath = ExcelPath_new;
            //打开数据文件
            IWorkbook workbook = WorkbookFactory.Create(importExcelPath);
            //读取数据表格
            List<Fan> fans = new List<Fan>();
            ISheet sheet_fireCompartments = workbook.GetSheet("防火分区");
           
            IRow row = (IRow)sheet_fireCompartments.GetRow((int)fireCompartment.Id.Value);
            string idStrings = row.GetCell(sheet_fireCompartments.getColNumber("相交的防烟分区")).ToString();
            List<SmokeCompartment> smokeCompartments = getAllSmokeCompartmentsByIdString(idStrings);
            if (smokeCompartments.findItem(smokeCompartment)!=null)
                return true;
            else
                return false;
        }

        public static List<Window> GetWindowsInRoom(Room room)
        {
            string importExcelPath = ExcelPath;
            //打开数据文件
            IWorkbook workbook = WorkbookFactory.Create(importExcelPath);
            //读取数据表格
            ISheet sheet = workbook.GetSheet(testDataTableName);
            //找到与房间id对应的数据行
            long windowId = room.Id.Value;
            IRow row = (IRow)sheet.GetRow((int)windowId);
            //查表确定此房间是否有可开启外窗
            bool hasOpenableOuterWindow = row.GetCell(4).BooleanCellValue;
            //如果有可开启外窗，则创建一个可开启外窗对象，并加入房间集合中
            List<Window> windows = new List<Window>();

            if (hasOpenableOuterWindow)
            {
                Window window = new Window(windowId);
                window.effectiveArea = 1;
                window.isExternalWindow = true;
                window.isSmokeExhaustWindow= true;
                windows.Add(window);
            }
            return windows;
        }

        public static List<Window> GetWindowsInRoom_new(Room room)
        {
            string importExcelPath = ExcelPath_new;
            //打开数据文件
            IWorkbook workbook = WorkbookFactory.Create(importExcelPath);
            //读取数据表格
            ISheet sheet_rooms = workbook.GetSheet(roomSheetName_new);
            ISheet sheet_windows = workbook.GetSheet("窗户");
            //找到与房间id对应的数据行
            
            IRow row = (IRow)sheet_rooms.GetRow((int)room.Id.Value);
      
            //获得房间所含窗户的id字符串
            string idList= row.GetCell(sheet_rooms.getColNumber("房间包含窗户的ID")).ToString();
            List<Window> windows = getAllWindowsByIdString(idList);

            return windows;
        }

        public static List<Wall> GetWallsOfRoom_new(Room room)
        {
            string importExcelPath = ExcelPath_new;
            //打开数据文件
            IWorkbook workbook = WorkbookFactory.Create(importExcelPath);
            //读取数据表格
            ISheet sheet_rooms = workbook.GetSheet(roomSheetName_new);
            ISheet sheet_Walls = workbook.GetSheet("墙");
            //找到与房间id对应的数据行

            IRow row = (IRow)sheet_rooms.GetRow((int)room.Id.Value);

            //获得房间所含窗户的id字符串
            string idList = row.GetCell(sheet_rooms.getColNumber("包含的墙")).ToString();
            List<Wall> walls = getAllWallsByIdString(idList);

            return walls;
        }

        public static List<AirTerminal> GetOutputLetsOfFan_new(Fan fan)
        {
            string importExcelPath = ExcelPath_new;
            //打开数据文件
            IWorkbook workbook = WorkbookFactory.Create(importExcelPath);
            //读取数据表格
            ISheet sheet_fans = workbook.GetSheet(fanSheetName_new);
            ISheet sheet_airTerminals = workbook.GetSheet("风口");
            //找到与风机id对应的数据行

            IRow row = (IRow)sheet_fans.GetRow((int)fan.Id.Value);

            //获得风机的送风口连接id字符串
            string idString = row.GetCell(sheet_fans.getColNumber("连接的送风口ID"))?.ToString();
            List<AirTerminal> airTerminals = getAllTerminalsByIdString(idString);

            return airTerminals;
        }

        public static List<AirTerminal> GetInputLetsOfFan_new(Fan fan)
        {
            string importExcelPath = ExcelPath_new;
            //打开数据文件
            IWorkbook workbook = WorkbookFactory.Create(importExcelPath);
            //读取数据表格
            ISheet sheet_fans = workbook.GetSheet(fanSheetName_new);
            ISheet sheet_airTerminals = workbook.GetSheet("风口");
            //找到与风机id对应的数据行

            IRow row = (IRow)sheet_fans.GetRow((int)fan.Id.Value);

            //获得风机的送风口连接id字符串
            string idString = row.GetCell(sheet_fans.getColNumber("连接的排风口ID"))?.ToString();
            List<AirTerminal> airTerminals = getAllTerminalsByIdString(idString);

            return airTerminals;
        }


        public static Room GetRoomOfAirterminal_new(AirTerminal airTerminal)
        {
            string importExcelPath = ExcelPath_new;
            //打开数据文件
            IWorkbook workbook = WorkbookFactory.Create(importExcelPath);
            //读取数据表格
            ISheet sheet_rooms = workbook.GetSheet(roomSheetName_new);

            //依次读取数据行，并根据数据内容创建房间，并加入房间集合中
            for (int index = 1; index <= sheet_rooms.LastRowNum; ++index)
            {
                IRow row = (IRow)sheet_rooms.GetRow(index);
                string idString = row.GetCell(sheet_rooms.getColNumber("包含的风口")).ToString();
                List<AirTerminal> airTerminals = getAllTerminalsByIdString(idString);
                if(airTerminals.findItem(airTerminal)!=null)
                {
                    return getRoomById(index);
                }
            }
            return null;
        }

        public static List<Room> GetRooms(string roomType)
        {
           string importExcelPath = ExcelPath;
           //打开测试数据文件
           IWorkbook workbook = WorkbookFactory.Create(importExcelPath);
           //读取测试数据表
           ISheet sheet = workbook.GetSheet(testDataTableName);
           List<Room> Rooms = new List<Room>();
           //依次读取数据行，并根据数据内容创建房间，并加入房间集合中
           for (int index = 1; index<=sheet.LastRowNum; ++index)
           {
             IRow row = (IRow)sheet.GetRow(index);
             string Type = row.GetCell(1).StringCellValue;
             if(Type.Contains(roomType))
             {
                long Id = Convert.ToInt64(row.GetCell(0).ToString());
                Room room = new Room(Id);
                room.type = Type;
                room.name = row.GetCell(13).StringCellValue;
                room.area = row.GetCell(10).NumericCellValue;
                room.roomPosition=(RoomPosition)row.GetCell(12).NumericCellValue;
                room.numberOfPeople= (int)row.GetCell(14).NumericCellValue;
                room.storyNo = (int)row.GetCell(15).NumericCellValue;
                Rooms.Add(room);
             }

           }
           return Rooms;
        }

        public static List<Room> GetRooms_new(string roomType)
        {
            string importExcelPath = ExcelPath_new;
            //打开测试数据文件
            IWorkbook workbook = WorkbookFactory.Create(importExcelPath);
            //读取测试数据表
            ISheet sheet_rooms = workbook.GetSheet(roomSheetName_new);
            List<Room> rooms = new List<Room>();
            //依次读取数据行，并根据数据内容创建房间，并加入房间集合中
            for (int index = 1; index <= sheet_rooms.LastRowNum; ++index)
            {
                IRow row = (IRow)sheet_rooms.GetRow(index);
                string Type = row.GetCell(1).StringCellValue;
                if (Type.Contains(roomType))
                {
                    long Id = Convert.ToInt64(row.GetCell(sheet_rooms.getColNumber("ID")).ToString());
                    Room room = new Room(Id);
                    room.type = Type;
                    room.name = row.GetCell(sheet_rooms.getColNumber("房间名称")).StringCellValue;
                    room.area = row.GetCell(sheet_rooms.getColNumber("房间面积")).NumericCellValue;
                    room.roomPosition = (RoomPosition)row.GetCell(sheet_rooms.getColNumber("房间位置")).NumericCellValue;
                    room.numberOfPeople = (int)row.GetCell(sheet_rooms.getColNumber("房间人数")).NumericCellValue;
                    room.storyNo = (int)row.GetCell(sheet_rooms.getColNumber("房间楼层编号")).NumericCellValue;
                    room.height = (int)row.GetCell(sheet_rooms.getColNumber("房间高度")).NumericCellValue;
                    rooms.Add(room);
                }

            }
            return rooms;
        }

        public static List<Room> GetRoomsMutiArgu_new(string roomType, string roomName, double roomArea, RoomPosition roomPosition)
        {
            string importExcelPath = ExcelPath_new;
            //打开测试数据文件
            IWorkbook workbook = WorkbookFactory.Create(importExcelPath);
            //读取测试数据表
            ISheet sheet_rooms = workbook.GetSheet(roomSheetName_new);
            List<Room> rooms = new List<Room>();
            //依次读取数据行，并根据数据内容创建房间，并加入房间集合中
            for (int index = 1; index <= sheet_rooms.LastRowNum; ++index)
            {
                IRow row = (IRow)sheet_rooms.GetRow(index);
                string type = row.GetCell(sheet_rooms.getColNumber("房间类型")).StringCellValue;
                string name = row.GetCell(sheet_rooms.getColNumber("房间名称")).StringCellValue;
                double area = row.GetCell(sheet_rooms.getColNumber("房间面积")).NumericCellValue;
                RoomPosition position = (RoomPosition)row.GetCell(sheet_rooms.getColNumber("房间位置")).NumericCellValue;

                if (type.Contains(roomType)&&name.Contains(roomName)&&area>roomArea&&roomPosition==position)
                {
                    long Id = Convert.ToInt64(row.GetCell(sheet_rooms.getColNumber("ID")).ToString());
                    Room room = new Room(Id);
                    room.type = type;
                    room.name = name;
                    room.area = area;
                    room.roomPosition = position;
                    room.numberOfPeople = (int)row.GetCell(sheet_rooms.getColNumber("房间人数")).NumericCellValue;
                    room.storyNo = (int)row.GetCell(sheet_rooms.getColNumber("房间楼层编号")).NumericCellValue;
                    rooms.Add(room);
                }

            }
            return rooms;
        }

        public static FireCompartment getFireDistrictContainAirTerminal_new(AirTerminal airTerminal)
        {
            string importExcelPath = ExcelPath_new;
            //打开测试数据文件
            IWorkbook workbook = WorkbookFactory.Create(importExcelPath);
            //读取测试数据表
            ISheet sheet_FireDistricts = workbook.GetSheet(fireDistrictSheetName_new);
            ISheet sheet_airTerminals = workbook.GetSheet("风口");
           
            //依次读取数据行，并根据数据内容创建房间，并加入房间集合中
            for (int index = 1; index <= sheet_FireDistricts.LastRowNum; ++index)
            {
                IRow row = (IRow)sheet_FireDistricts.GetRow(index);
                long Id = Convert.ToInt64(row.GetCell(sheet_FireDistricts.getColNumber("ID")).ToString());
                FireCompartment fireDistrict = new FireCompartment(Id);
                string idString = row.GetCell(sheet_FireDistricts.getColNumber("包含的风口")).ToString();

                List<AirTerminal> airTerminals = getAllTerminalsByIdString(idString);
                if (airTerminals.findItem(airTerminal) != null)
                    return fireDistrict;
            }
            return null;
        }

        public static bool isOuterAirTerminal_new(AirTerminal airTerminal)
        {
            string importExcelPath = ExcelPath_new;
            //打开测试数据文件
            IWorkbook workbook = WorkbookFactory.Create(importExcelPath);
            //读取测试数据表
            ISheet sheet_airTerminals = workbook.GetSheet("风口");

            //依次读取数据行，并根据数据内容创建房间，并加入房间集合中

            IRow row = (IRow)sheet_airTerminals.GetRow((int)airTerminal.Id.Value);
            bool isOuterAirTerminal= row.GetCell(sheet_airTerminals.getColNumber("是否为室外风口")).BooleanCellValue;
            return isOuterAirTerminal;   
        }

        public static bool isAirTerminalInFireDistrict_new(AirTerminal airTerminal, FireCompartment fireDistrict)
        {
            string importExcelPath = ExcelPath_new;
            //打开测试数据文件
            IWorkbook workbook = WorkbookFactory.Create(importExcelPath);
            //读取测试数据表
            ISheet sheet_FireDistricts = workbook.GetSheet(fireDistrictSheetName_new);
            ISheet sheet_airTerminals = workbook.GetSheet("风口");

            //依次读取数据行，并根据数据内容创建房间，并加入房间集合中
           
             IRow row = (IRow)sheet_FireDistricts.GetRow((int)fireDistrict.Id.Value);

             string idString = row.GetCell(sheet_FireDistricts.getColNumber("包含的风口")).ToString();

             List<AirTerminal> airTerminals = getAllTerminalsByIdString(idString);
             if (airTerminals.findItem(airTerminal) != null)
                 return true;
            return false;
        }

        public static List<Room> GetRoomsMutiArgu(string roomType, string roomName, double roomArea, RoomPosition roomPosition)
        {
                    string importExcelPath = ExcelPath;
                    //打开测试数据文件
                    IWorkbook workbook = WorkbookFactory.Create(importExcelPath);
                    //读取测试数据表
                    ISheet sheet = workbook.GetSheet(testDataTableName);
                    List<Room> Rooms = new List<Room>();
                    //依次读取数据行，并根据数据内容创建房间，并加入房间集合中
                    for (int index = 1; index<=sheet.LastRowNum; ++index)
                    {
                        IRow row = (IRow)sheet.GetRow(index);
                        string Type = row.GetCell(1).StringCellValue;
                        string name= row.GetCell(13).StringCellValue;
                        double area= row.GetCell(10).NumericCellValue;
                        RoomPosition position =(RoomPosition)int.Parse(row.GetCell(12).ToString());
                        if (Type.Contains(roomType)&&name.Contains(roomName)&& ((int)(roomPosition & position) != 0) && area>=roomArea)
                        {
                            long Id = Convert.ToInt64(row.GetCell(0).ToString());
                            Room room = new Room(Id);
                            room.type = row.GetCell(1).StringCellValue;
                            room.name= row.GetCell(13).StringCellValue;
                            room.area = area;
                            room.roomPosition = (RoomPosition)row.GetCell(12).NumericCellValue;
                            room.numberOfPeople = (int)row.GetCell(14).NumericCellValue;
                            room.storyNo = (int)row.GetCell(15).NumericCellValue;
                            Rooms.Add(room);

                        }

                    }
                    return Rooms;
        }

        public static List<Region> GetConnectedRegions()
        {
            List<Region> connectedRegions = new List<Region>();
            string importExcelPath = ExcelPath;
            //打开测试数据文件
            IWorkbook workbook = WorkbookFactory.Create(importExcelPath);
            //读取测试数据表
            ISheet sheet = workbook.GetSheet(testDataTableName);
            int currentRegionNo = 0;
            Region region = null;
            //依次读取数据行，并根据数据内容创建区域，并加入区域集合中
            for (int index = 1; index <= sheet.LastRowNum; ++index)
            { 
                IRow row = (IRow)sheet.GetRow(index);
                int regionNo = (int)row.GetCell(17).NumericCellValue;
                
                if(regionNo!=currentRegionNo)
                {
                    if(region!=null)
                        connectedRegions.Add(region);
                    region = new Region();
                    region.rooms = new List<Room>();
                    currentRegionNo = regionNo;
                }

                long Id = Convert.ToInt64(row.GetCell(0).ToString());
                Room room = new Room(Id);
                room.type = row.GetCell(1).StringCellValue;
                room.name = row.GetCell(13).StringCellValue;
                room.area = row.GetCell(10).NumericCellValue;
                room.roomPosition = (RoomPosition)row.GetCell(12).NumericCellValue;
                room.numberOfPeople = (int)row.GetCell(14).NumericCellValue;
                room.storyNo = (int)row.GetCell(15).NumericCellValue;
                region.rooms.Add(room);
            }
            if (region != null)
                connectedRegions.Add(region);
            return connectedRegions;
        }

        public static List<Room> getConnectedRooms_new(Room room)
        {
            string importExcelPath = ExcelPath_new;
            //打开测试数据文件
            IWorkbook workbook = WorkbookFactory.Create(importExcelPath);
            //读取测试数据表
            ISheet sheet_rooms = workbook.GetSheet(roomSheetName_new);
            //获得房间的门ID字符串
            IRow row = (IRow)sheet_rooms.GetRow((int)room.Id);
            string idString = row.GetCell(sheet_rooms.getColNumber("包含的门ID")).ToString();
            //获得房间的所有门的集合
            List<Door> doors = getAllDoorsByIdString(idString);
            List<Room> rooms = new List<Room>();
            //依次遍历所有房间，查找包含集合doors中的门的房间
            for(int index=1;index<=sheet_rooms.LastRowNum;++index)
            {
                row = (IRow)sheet_rooms.GetRow(index);
                idString = row.GetCell(sheet_rooms.getColNumber("包含的门ID")).ToString();
                //获得房间的所有门的集合
                List<Door> doorsOfCurrentRoom = getAllDoorsByIdString(idString);
                if(doorsOfCurrentRoom.getCommonItems(doors).Count>0&&index!=room.Id.Value)
                {
                    Room currentRoom = getRoomById(index);
                    rooms.Add(currentRoom);
                }
            }
            
            return rooms;
        }


        public static List<Door> getDoorsBetweenTwoRooms_new(Room firstRoom,Room secondRoom)
        {
            string importExcelPath = ExcelPath_new;
            //打开测试数据文件
            IWorkbook workbook = WorkbookFactory.Create(importExcelPath);
            //读取测试数据表
            ISheet sheet_rooms = workbook.GetSheet(roomSheetName_new);
            //读取第一个房间的门ID字符串
            IRow row = (IRow)sheet_rooms.GetRow((int)firstRoom.Id);
            string idString= row.GetCell(sheet_rooms.getColNumber("包含的门ID")).ToString();
            //根据ID字符串获得第一个房间的门的集合doorsOfFirstRoom
            List<Door> doorsOfFirstRoom = getAllDoorsByIdString(idString);

            //读取第二个房间的门ID字符串
            row = (IRow)sheet_rooms.GetRow((int)secondRoom.Id);
            idString = row.GetCell(sheet_rooms.getColNumber("包含的门ID")).ToString();
            //根据ID字符串获得第二个房间的门的集合doorsOfSecondRoom
            List<Door> doorsOfSecondRoom = getAllDoorsByIdString(idString);
            //获取集合doorsOfFirstRoom与集合doorsOfSecondRoom共有门的集合DoorsBetweenTwoRooms
            List<Door> DoorsBetweenTwoRooms = doorsOfFirstRoom.getCommonItems(doorsOfSecondRoom);
            //返回集合DoorsToCorridor
            return DoorsBetweenTwoRooms;
        }

        public static List<Floor> GetAllFLoorsOfBuilding_new()
        {
            string importExcelPath = ExcelPath_new;
            //打开测试数据文件
            IWorkbook workbook = WorkbookFactory.Create(importExcelPath);
            //读取测试数据表
            ISheet sheet_floors = workbook.GetSheet("楼层");
           
            List<Floor> floors = new List<Floor>();

            //依次遍历所有楼层
            for (int index = 1; index <= sheet_floors.LastRowNum; ++index)
            {
                IRow row = (IRow)sheet_floors.GetRow(index);
                Floor floor = new Floor((long)row.GetCell(sheet_floors.getColNumber("ID")).NumericCellValue);
                floor.storyNo = (int)row.GetCell(sheet_floors.getColNumber("楼层编号")).NumericCellValue;
                floor.height = row.GetCell(sheet_floors.getColNumber("楼层高度")).NumericCellValue;
                floor.elevation = row.GetCell(sheet_floors.getColNumber("楼层标高")).NumericCellValue;
                floors.Add(floor);
            }

            return floors;
        }
        public static List<SmokeCompartment> GetSmokeCompartmentsInRoom_new(Room room)
        {
            string importExcelPath = ExcelPath_new;
            //打开测试数据文件
            IWorkbook workbook = WorkbookFactory.Create(importExcelPath);
            //读取测试数据表
            ISheet sheet_rooms = workbook.GetSheet(roomSheetName_new);

            IRow row = (IRow)sheet_rooms.GetRow((int)room.Id.Value);
            string IdString = row.GetCell(sheet_rooms.getColNumber("包含的防烟分区")).ToString();

            List<SmokeCompartment> smokeCompartments = getAllSmokeCompartmentsByIdString(IdString);
            return smokeCompartments;
        }

        public static List<Fan> GetAllFans_new()
        {
            string importExcelPath = ExcelPath_new;
            //打开测试数据文件
            IWorkbook workbook = WorkbookFactory.Create(importExcelPath);
            //读取测试数据表
            ISheet sheet_fans = workbook.GetSheet(fanSheetName_new);

            List<Fan> fans = new List<Fan>();
            //依次遍历所有楼层
            for (int index = 1; index <= sheet_fans.LastRowNum; ++index)
            {
                IRow row = (IRow)sheet_fans.GetRow(index);
                Fan fan = new Fan((long)row.GetCell(sheet_fans.getColNumber("ID")).NumericCellValue);
                fans.Add(fan);
            }

            return fans;
        }

        public static List<FireCompartment> GetFireCompartment_new(string sName)
        {
            string importExcelPath = ExcelPath_new;
            //打开测试数据文件
            IWorkbook workbook = WorkbookFactory.Create(importExcelPath);
            //读取测试数据表
            ISheet sheet_fireCompartments = workbook.GetSheet("防火分区");

            List<FireCompartment> fireCompartments = new List<FireCompartment>();
            //依次遍历所有楼层
            for (int index = 1; index <= sheet_fireCompartments.LastRowNum; ++index)
            {
                IRow row = (IRow)sheet_fireCompartments.GetRow(index);
                string name = row.GetCell(sheet_fireCompartments.getColNumber("名称")).ToString();
                if(name.Contains(sName))
                {
                    FireCompartment fireCompartment = new FireCompartment(index);
                    fireCompartment.storyNo= (int)row.GetCell(sheet_fireCompartments.getColNumber("楼层编号")).NumericCellValue;
                    fireCompartments.Add(fireCompartment);
                }
            }

            return fireCompartments;
        }

        public static List<Room> GetRoomsMoreThan(string type,double dLength)
        {
            string importExcelPath = ExcelPath;
            //打开测试数据文件
            IWorkbook workbook = WorkbookFactory.Create(importExcelPath);
            //读取测试数据表
            ISheet sheet = workbook.GetSheet(testDataTableName);
            List<Room> Rooms = new List<Room>();
            //依次读取数据行，并根据数据内容创建房间，并加入房间集合中
            for (int index = 1; index <= sheet.LastRowNum; ++index)
            {
                IRow row = (IRow)sheet.GetRow(index);
                string roomType = row.GetCell(1).StringCellValue;
                if (roomType.Contains(type))
                {
                    double corridorLength = double.Parse(row.GetCell(11).ToString());
                    if (corridorLength >dLength)
                    {
                        long Id = Convert.ToInt64(row.GetCell(0).ToString());
                        Room room = new Room(Id);
                        room.type = roomType;
                        room.name = row.GetCell(13).StringCellValue;
                        room.area = row.GetCell(10).NumericCellValue;
                        room.roomPosition = (RoomPosition)row.GetCell(12).NumericCellValue;
                        room.numberOfPeople = (int)row.GetCell(14).NumericCellValue;
                        room.storyNo = (int)row.GetCell(15).NumericCellValue;
                        Rooms.Add(room);
                    }
                }
            }
            return Rooms;
        }


        public static List<Room> GetRoomsMoreThan_new(string type, double dLength)
        {
            string importExcelPath = ExcelPath_new;
            //打开测试数据文件
            IWorkbook workbook = WorkbookFactory.Create(importExcelPath);
            //读取测试数据表
            ISheet sheet_rooms = workbook.GetSheet(roomSheetName_new);

            List<Room> rooms = new List<Room>();
            //依次读取数据行，并根据数据内容创建房间，并加入房间集合中
            for (int index = 1; index <= sheet_rooms.LastRowNum; ++index)
            {
                IRow row = (IRow)sheet_rooms.GetRow(index);
                string roomType = row.GetCell(sheet_rooms.getColNumber("房间类型")).StringCellValue;
                double corridorLength = double.Parse(row.GetCell(sheet_rooms.getColNumber("房间长度")).ToString());
                if (roomType.Contains(type)&& corridorLength > dLength)
                {
                    long Id = Convert.ToInt64(row.GetCell(sheet_rooms.getColNumber("ID")).ToString());
                    Room room = new Room(Id);
                    room.type = roomType;
                    room.name = row.GetCell(sheet_rooms.getColNumber("房间名称")).StringCellValue;
                    room.area = row.GetCell(sheet_rooms.getColNumber("房间面积")).NumericCellValue;
                    room.roomPosition = (RoomPosition)row.GetCell(sheet_rooms.getColNumber("房间位置")).NumericCellValue;
                    room.numberOfPeople = (int)row.GetCell(sheet_rooms.getColNumber("房间人数")).NumericCellValue;
                    room.storyNo = (int)row.GetCell(sheet_rooms.getColNumber("房间楼层编号")).NumericCellValue;
                    rooms.Add(room);
                }
            }
            return rooms;
        }
            


        private static Room getRoomById(long id)
        {
            string importExcelPath = ExcelPath_new;
            //打开测试数据文件
            IWorkbook workbook = WorkbookFactory.Create(importExcelPath);
            //读取测试数据表
            ISheet sheet_rooms = workbook.GetSheet(roomSheetName_new);
            IRow row = (IRow)sheet_rooms.GetRow((int)id);
           
            Room room = new Room(id);
            room.type = row.GetCell(sheet_rooms.getColNumber("房间类型")).StringCellValue;
            room.name = row.GetCell(sheet_rooms.getColNumber("房间名称")).StringCellValue;
            room.area = row.GetCell(sheet_rooms.getColNumber("房间面积")).NumericCellValue;
            room.roomPosition = (RoomPosition)row.GetCell(sheet_rooms.getColNumber("房间位置")).NumericCellValue;
            room.numberOfPeople = (int)row.GetCell(sheet_rooms.getColNumber("房间人数")).NumericCellValue;
            room.storyNo = (int)row.GetCell(sheet_rooms.getColNumber("房间楼层编号")).NumericCellValue;

            return room;
        }

        private static Fan getFanById(long id)
        {
            string importExcelPath = ExcelPath_new;
            //打开测试数据文件
            IWorkbook workbook = WorkbookFactory.Create(importExcelPath);
            //读取测试数据表
            ISheet sheet_fans = workbook.GetSheet(fanSheetName_new);
            IRow row = (IRow)sheet_fans.GetRow((int)id);

            Fan fan = new Fan(id);

            fan.m_flowRate= row.GetCell(sheet_fans.getColNumber("风量")).NumericCellValue;

            return fan;
        }

        private static List<Room> getAllRoomsByIdString(string IdString)
        {
            string importExcelPath = ExcelPath_new;
            //打开测试数据文件
            IWorkbook workbook = WorkbookFactory.Create(importExcelPath);
            //读取测试数据表
            ISheet sheet_rooms = workbook.GetSheet(roomSheetName_new);

            List<long> idList = getIdList(IdString);
            List<Room> rooms = new List<Room>();
            foreach (long id in idList)
            {
                IRow row = (IRow)sheet_rooms.GetRow((int)id);
                Room room = new Room(id);
                room.type = row.GetCell(sheet_rooms.getColNumber("房间类型")).StringCellValue; ;
                room.name = row.GetCell(sheet_rooms.getColNumber("房间名称")).StringCellValue;
                room.area = row.GetCell(sheet_rooms.getColNumber("房间面积")).NumericCellValue;
                room.roomPosition = (RoomPosition)row.GetCell(sheet_rooms.getColNumber("房间位置")).NumericCellValue;
                room.numberOfPeople = (int)row.GetCell(sheet_rooms.getColNumber("房间人数")).NumericCellValue;
                room.storyNo = (int)row.GetCell(sheet_rooms.getColNumber("房间楼层编号")).NumericCellValue;
                rooms.Add(room);
            }
            return rooms;
        }

        private static List<AirTerminal> getAllTerminalsByIdString(string IdString)
        {
            string importExcelPath = ExcelPath_new;
            //打开测试数据文件
            IWorkbook workbook = WorkbookFactory.Create(importExcelPath);
            //读取测试数据表
            ISheet sheet_airTerminals = workbook.GetSheet("风口");

            List<long> idList = getIdList(IdString);
            List<AirTerminal> airTerminals = new List<AirTerminal>();
            foreach(long id in idList)
            {
                IRow row = (IRow)sheet_airTerminals.GetRow((int)id);
                AirTerminal airTerminal = new AirTerminal(id);
                airTerminal.systemType= row.GetCell(sheet_airTerminals.getColNumber("系统类型")).ToString();
                airTerminal.storyNo= (int)row.GetCell(sheet_airTerminals.getColNumber("楼层编号")).NumericCellValue;
                airTerminal.elevation= row.GetCell(sheet_airTerminals.getColNumber("风口标高")).NumericCellValue;
                airTerminal.airFlowRate = row.GetCell(sheet_airTerminals.getColNumber("风量")).NumericCellValue;
                airTerminals.Add(airTerminal);
            }
            return airTerminals;
        }

        private static List<Fan> getAllFansByIdString(string IdString)
        {
            List<long> idList = getIdList(IdString);
            List<Fan> fans = new List<Fan>();
            foreach (long id in idList)
            {
                Fan fan = new Fan(id);
                fans.Add(fan);
            }
            return fans;
        }


        private static List<SmokeCompartment> getAllSmokeCompartmentsByIdString(string IdString)
        {
            string importExcelPath = ExcelPath_new;
            //打开测试数据文件
            IWorkbook workbook = WorkbookFactory.Create(importExcelPath);
            //读取测试数据表
            ISheet sheet_smokeCompartments = workbook.GetSheet(smokeCompartmentSheetName_new);

            List<long> idList = getIdList(IdString);
            List<SmokeCompartment> smokeCompartments = new List<SmokeCompartment>();
            foreach (long id in idList)
            {
                IRow row = (IRow)sheet_smokeCompartments.GetRow((int)id);
                SmokeCompartment smokeCompartment = new SmokeCompartment(id);
                smokeCompartment.area= row.GetCell(sheet_smokeCompartments.getColNumber("面积")).NumericCellValue;
                smokeCompartment.storyNo = (int)row.GetCell(sheet_smokeCompartments.getColNumber("楼层编号")).NumericCellValue;

                smokeCompartments.Add(smokeCompartment);
            }
            return smokeCompartments;
        }
       


        private static List<Door> getAllDoorsByIdString(string IdString)
        {
            List<long> idList = getIdList(IdString);
            List<Door> doors = new List<Door>();
            foreach (long id in idList)
            {
                Door door = new Door(id);
                doors.Add(door);
            }
            return doors;
        }

        private static List<Window> getAllWindowsByIdString(string IdString)
        {
            List<long> idList = getIdList(IdString);
            List<Window> windows = new List<Window>();

            string importExcelPath = ExcelPath_new;
            //打开数据文件
            IWorkbook workbook = WorkbookFactory.Create(importExcelPath);
            //读取数据表格

            ISheet sheet_windows = workbook.GetSheet("窗户");

            foreach (long id in idList)
            {
                Window window = new Window(id);
                IRow row = (IRow)sheet_windows.GetRow((int)id);
                window.area= row.GetCell(sheet_windows.getColNumber("窗户面积")).NumericCellValue;
                window.storyNo= (int)row.GetCell(sheet_windows.getColNumber("楼层编号")).NumericCellValue;
                window.isExternalWindow= row.GetCell(sheet_windows.getColNumber("是否是外窗")).BooleanCellValue;
                window.effectiveArea= row.GetCell(sheet_windows.getColNumber("窗户有效面积")).NumericCellValue;
                window.isSmokeExhaustWindow= row.GetCell(sheet_windows.getColNumber("是否为排烟窗")).BooleanCellValue;
                windows.Add(window);
            }
            return windows;
        }

        private static List<Wall> getAllWallsByIdString(string IdString)
        {
            List<long> idList = getIdList(IdString);
            List<Wall> walls = new List<Wall>();

            string importExcelPath = ExcelPath_new;
            //打开数据文件
            IWorkbook workbook = WorkbookFactory.Create(importExcelPath);
            //读取数据表格

            ISheet sheet_Walls = workbook.GetSheet("墙");

            foreach (long id in idList)
            {
               Wall wall = new Wall(id);
                IRow row = (IRow)sheet_Walls.GetRow((int)id);
                wall.isOuterWall= row.GetCell(sheet_Walls.getColNumber("是否为外墙")).BooleanCellValue;
                walls.Add(wall);
            }
            return walls;
        }

        public static List<Floor> getAllFloorsByIdString(string idString)
        {
            List<long> idList = getIdList(idString);
            List<Floor> floors = new List<Floor>();

            string importExcelPath = ExcelPath_new;
            //打开数据文件
            IWorkbook workbook = WorkbookFactory.Create(importExcelPath);
            //读取数据表格

            ISheet sheet_floors = workbook.GetSheet("楼层");

            foreach (long id in idList)
            {
                Floor floor = new Floor(id);
                IRow row = (IRow)sheet_floors.GetRow((int)id);
                floor.storyNo = (int)row.GetCell(sheet_floors.getColNumber("楼层编号")).NumericCellValue;
                floor.height = row.GetCell(sheet_floors.getColNumber("楼层高度")).NumericCellValue;
                floor.elevation= row.GetCell(sheet_floors.getColNumber("楼层标高")).NumericCellValue;
                floors.Add(floor);
            }
            return floors;
        }


        public static Dictionary<Duct, List<HVACFunction.Point>> getAllDuctsByIdString_dictionary(string idString)
        {
            List<long> idList = getIdList(idString);
           
            string importExcelPath = ExcelPath_new;
            //打开数据文件
            IWorkbook workbook = WorkbookFactory.Create(importExcelPath);
            //读取数据表格

            ISheet sheet_ducts = workbook.GetSheet(ductSheetName_new);

            Dictionary<Duct, List<HVACFunction.Point>> ducts = new Dictionary<Duct, List<HVACFunction.Point>>();

            foreach (long id in idList)
            {
                Duct duct = new Duct(id);
                IRow row = (IRow)sheet_ducts.GetRow((int)id);
                duct.systemType = row.GetCell(sheet_ducts.getColNumber("风管类型")).StringCellValue;
                List<HVACFunction.Point> points = new List<HVACFunction.Point>();
                points.Add(new HVACFunction.Point());
                ducts.Add(duct, points);
            }
            return ducts;
        }


        public static List<Duct> getAllDuctsByIdString_List(string idString)
        {
            List<long> idList = getIdList(idString);

            string importExcelPath = ExcelPath_new;
            //打开数据文件
            IWorkbook workbook = WorkbookFactory.Create(importExcelPath);
            //读取数据表格

            ISheet sheet_ducts = workbook.GetSheet(ductSheetName_new);

            List<Duct> ducts = new List<Duct>();

            foreach (long id in idList)
            {
                Duct duct = new Duct(id);
                IRow row = (IRow)sheet_ducts.GetRow((int)id);
                duct.systemType = row.GetCell(sheet_ducts.getColNumber("风管类型")).StringCellValue;
                ducts.Add(duct);
            }
            return ducts;
        }

        private static List<long> getIdList(string IdString)
        {
            List<long> idList = new List<long>();
            if (IdString == null)
                return idList;
            string str = "";
            foreach (char c in IdString)
            {
               
                if (c == ',')
                {
                    if (str.Length > 0)
                    {
                        long id = long.Parse(str);
                        idList.Add(id);
                        str = "";
                    }
                        continue;
                }
                str += c;
            }
            if (str.Length > 0)
            {
                long id = long.Parse(str);
                idList.Add(id);
            }
            return idList;
        }

        public static int getColNumber(this ISheet sheet,string colName)
        {
            if (sheet.LastRowNum < 0)
                throw new ArgumentException("表格为空");
            IRow row = (IRow)sheet.GetRow(0);
           for(int index=0;index<= row.LastCellNum;++index)
            {
                ICell cell= row.GetCell(index);
                if (cell.StringCellValue == colName)
                    return index;
            }
            return -1;
        }

        public static List<Duct> GetDuctsCrossSpace_new(Room room)
        {
            string importExcelPath = ExcelPath_new;
            //打开测试数据文件
            IWorkbook workbook = WorkbookFactory.Create(importExcelPath);
            //读取测试数据表
            ISheet sheet_rooms = workbook.GetSheet(roomSheetName_new);

            List<Duct> ducts = new List<Duct>();


            IRow row = (IRow)sheet_rooms.GetRow((int)room.Id.Value);
            string idString = row.GetCell(sheet_rooms.getColNumber("穿越的风管"))?.ToString();
            ducts = getAllDuctsByIdString_List(idString);


            return ducts;
        }

        public static List<Duct> GetDuctsCrossFireDistrict_new(FireCompartment fireDistrict)
        {
            string importExcelPath = ExcelPath_new;
            //打开测试数据文件
            IWorkbook workbook = WorkbookFactory.Create(importExcelPath);
            //读取测试数据表
            ISheet sheet_fireCompartments = workbook.GetSheet("防火分区");

            List<Duct> ducts = new List<Duct>();


            IRow row = (IRow)sheet_fireCompartments.GetRow((int)fireDistrict.Id.Value);
            string idString = row.GetCell(sheet_fireCompartments.getColNumber("穿越的风管"))?.ToString();
            ducts = getAllDuctsByIdString_List(idString);

            return ducts;
        }

        public static List<Duct> getAllDuctsInRoom_new(Room room)
        {
            string importExcelPath = ExcelPath_new;
            //打开测试数据文件
            IWorkbook workbook = WorkbookFactory.Create(importExcelPath);
            //读取测试数据表
            ISheet sheet_rooms = workbook.GetSheet(roomSheetName_new);

            List<Duct> ducts = new List<Duct>();

            IRow row = (IRow)sheet_rooms.GetRow((int)room.Id.Value);
            string idString = row.GetCell(sheet_rooms.getColNumber("包含的风管"))?.ToString();
            ducts = getAllDuctsByIdString_List(idString);
            return ducts;
        }

        public static List<FireDamper> getAllFireDamperByIdString(string idString)
        {
            List<long> idList = getIdList(idString);

            string importExcelPath = ExcelPath_new;
            //打开数据文件
            IWorkbook workbook = WorkbookFactory.Create(importExcelPath);
            //读取数据表格

            ISheet sheet_fireDampers = workbook.GetSheet("防火阀");

            List<FireDamper> fireDampers = new List<FireDamper>();

            foreach (long id in idList)
            {
                FireDamper firedamper = new FireDamper(id);
                fireDampers.Add(firedamper);
            }
            return fireDampers;
        }

        public static List<Duct> getAllVerticalDuctConnectedToDuct_new(Duct duct)
        {
            string importExcelPath = ExcelPath_new;
            //打开测试数据文件
            IWorkbook workbook = WorkbookFactory.Create(importExcelPath);
            //读取测试数据表
            ISheet sheet_ducts = workbook.GetSheet(ductSheetName_new);

            List<Duct> ducts = new List<Duct>();

            IRow row = (IRow)sheet_ducts.GetRow((int)duct.Id.Value);
            string idString = row.GetCell(sheet_ducts.getColNumber("所连接的立管"))?.ToString();
            ducts = getAllDuctsByIdString_List(idString);
            return ducts;
        }

        public static List<Duct> GetDuctsCrossMovementJointAndFireSide_new()
        {
            string importExcelPath = ExcelPath_new;
            //打开测试数据文件
            IWorkbook workbook = WorkbookFactory.Create(importExcelPath);
            //读取测试数据表
            ISheet sheet_movementJoint = workbook.GetSheet(movementJointName_new );

            List<Duct> ducts = new List<Duct>();

            for (int index = 1; index <= sheet_movementJoint.LastRowNum; ++index)
            {
                IRow row = (IRow)sheet_movementJoint.GetRow(index);
                string idString = row.GetCell(sheet_movementJoint.getColNumber("穿越的风管"))?.ToString();
                ducts.addDuctsToList(getAllDuctsByIdString_List(idString));
            }
            return ducts;
        }

        public static List<FireDamper> getFireDamperOfDuct_new(Duct duct)
        {
            string importExcelPath = ExcelPath_new;
            //打开测试数据文件
            IWorkbook workbook = WorkbookFactory.Create(importExcelPath);
            //读取测试数据表
            ISheet sheet_ducts = workbook.GetSheet(ductSheetName_new);

            List<FireDamper> fireDampers = new List<FireDamper>();

            IRow row = (IRow)sheet_ducts.GetRow((int)duct.Id.Value);
            string idString = row.GetCell(sheet_ducts.getColNumber("连接的防火阀"))?.ToString();
            fireDampers = getAllFireDamperByIdString(idString);
            return fireDampers;
        }

        public static bool isElementNearPoint_new(Element element,HVACFunction.Point point)
        {
            string importExcelPath = ExcelPath_new;
            //打开测试数据文件
            IWorkbook workbook = WorkbookFactory.Create(importExcelPath);
            //读取测试数据表
            ISheet sheet_fireDampers = workbook.GetSheet("防火阀");

            IRow row = (IRow)sheet_fireDampers.GetRow((int)element.Id.Value);
            return row.GetCell(sheet_fireDampers.getColNumber("是否靠近穿越点")).BooleanCellValue;
        }

        public static List<Room> getAllRoomsHaveFireDoor_new()
        {
            string importExcelPath = ExcelPath_new;
            //打开测试数据文件
            IWorkbook workbook = WorkbookFactory.Create(importExcelPath);
            //读取测试数据表
            ISheet sheet_rooms = workbook.GetSheet(roomSheetName_new);

            List<Room> rooms = new List<Room>();
            for (int index = 1; index <= sheet_rooms.LastRowNum; ++index)
            {
                IRow row = (IRow)sheet_rooms.GetRow(index);
                if(row.GetCell(sheet_rooms.getColNumber("包含防火门")).BooleanCellValue)
                {
                    Room room = new Room(index);
                    room.type = row.GetCell(sheet_rooms.getColNumber("房间类型")).StringCellValue; ;
                    room.name = row.GetCell(sheet_rooms.getColNumber("房间名称")).StringCellValue;
                    room.area = row.GetCell(sheet_rooms.getColNumber("房间面积")).NumericCellValue;
                    room.roomPosition = (RoomPosition)row.GetCell(sheet_rooms.getColNumber("房间位置")).NumericCellValue;
                    room.numberOfPeople = (int)row.GetCell(sheet_rooms.getColNumber("房间人数")).NumericCellValue;
                    room.storyNo = (int)row.GetCell(sheet_rooms.getColNumber("房间楼层编号")).NumericCellValue;
                    rooms.Add(room);
                }
               
            }
            return rooms;
        }

        private static double error = 0.0001;
    }
       
   
    

    public class Custom_Assert
    {
        public static void AreComponentViolationListEqual(List<ComponentAnnotation> firstList, List<ComponentAnnotation> secondList)
        {
            //return firstList.Equals(secondList);
            Assert.IsNotNull(firstList);
            Assert.IsNotNull(secondList);
            Assert.AreEqual(firstList.Count, secondList.Count);
            int minCountOfList = Math.Min(firstList.Count, secondList.Count);
            for (int index = 0; index < minCountOfList; ++index)
            {
                Assert.IsTrue(firstList.Exists(x => x.Equals(secondList[index])));

            }
        }
       public static void AreListsEqual<T>(List<T> firstList, List<T> secondList)where T:Element
        {
            //return firstList.Equals(secondList);
            Assert.IsNotNull(firstList);
            Assert.IsNotNull(secondList);
            Assert.AreEqual(firstList.Count, secondList.Count);
            int minCountOfList = Math.Min(firstList.Count, secondList.Count);
            for (int index = 0; index < minCountOfList; ++index)
            {
                Assert.IsTrue(firstList.Exists(x => x.Id==secondList[index].Id));
            }
        }


        public static void AreDictionaryEqual<T1,T2>(Dictionary<T1,List<T2>> firstDictionary, Dictionary<T1, List<T2>> secondDictionary) where T1 : Element
                                                                                                                             where T2:Element
        {
            foreach(KeyValuePair<T1, List<T2>> pair in firstDictionary)
            {
                List<T2> floors = secondDictionary.getValueAccordingToKey(pair.Key);
                if (floors == null)
                    Assert.Fail("两字典不等");
                Custom_Assert.AreListsEqual(pair.Value, floors);
            }
        }
    }

}
