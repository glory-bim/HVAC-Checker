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
                Assert.IsFalse(isPassCheck);
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
                    if(systemType==type)
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
                bool isPassCheck =Boolean.Parse(context.DataRow["是否通过"].ToString());
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
                Assert.IsTrue(isPassCheck);
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

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomsMoreThanDouble = FakeHVACFunction.GetRoomsMoreThan;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomContainAirTerminalRoom = FakeHVACFunction.GetRoomContainAirTerminal;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetFanConnectingAirterminalAirTerminal = FakeHVACFunction.GetFanConnectingAirterminal;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetWindowsInRoomRoom = FakeHVACFunction.GetWindowsInRoom;

                //arrange
                globalData.buildingHeight = 32;
                globalData.buildingType = "丙类厂房";
                string comment = "设计不满足规范GB50016_2014中第8.5.2条条文规定。请专家复核：相关违规房间是否人员长期停留或可燃物较多";
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
                        if (isNeedReCheck)
                            componentAnnotation.remark = "此房间需专家核对是否为人员长期停留或可燃物较多";
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
                Assert.IsFalse(isPassCheck);
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
                     if (Type.Contains(roomType) && name.Contains(roomName) && Math.Abs(roomArea - area) < error && roomPosition == position)
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

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomsMoreThanDouble = (double dLength) =>
                  {
                      List<Room> Rooms = new List<Room>();
                      string roomType = context.DataRow["房间类型"].ToString();
                      double corridorLength = Convert.ToDouble(context.DataRow["走廊长度"].ToString());
                      if (roomType.Contains("走廊") && (corridorLength > dLength || Math.Abs(corridorLength - dLength) < error))
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
                    if (isNeedReCheck)
                        componentAnnotation.remark = "此房间需专家核对是否为人员长期停留或可燃物较多";
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

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomsMoreThanDouble=(double dLength) =>
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
                bool isPassCheck = true;
                List<ComponentAnnotation> componentViolations = new List<ComponentAnnotation>();

                //act
                BimReview result = new BimReview();
                result = HVACChecker.GB50016_2014_8_5_2();

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

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomsMoreThanDouble = FakeHVACFunction.GetRoomsMoreThan;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomContainAirTerminalRoom = FakeHVACFunction.GetRoomContainAirTerminal;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetFanConnectingAirterminalAirTerminal = FakeHVACFunction.GetFanConnectingAirterminal;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetWindowsInRoomRoom = FakeHVACFunction.GetWindowsInRoom;

                //arrange
                globalData.buildingHeight = 32;
                globalData.buildingType = "公共建筑";
                string comment = "设计不满足规范GB50016_2014中第8.5.3条条文规定。请专家复核：相关违规房间是否人员长期停留或可燃物较多";
                bool isPassCheck = false;
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
                Assert.IsFalse(isPassCheck);
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
                        room.storyNo= Convert.ToInt32(context.DataRow["房间楼层编号"].ToString());
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
                    if (Type.Contains(roomType) && name.Contains(roomName) && area>=roomArea && roomPosition == position)
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

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomsMoreThanDouble = (double dLength) =>
                {
                    List<Room> Rooms = new List<Room>();
                    string roomType = context.DataRow["房间类型"].ToString();
                    double corridorLength = Convert.ToDouble(context.DataRow["走廊长度"].ToString());
                    if (roomType.Contains("走廊") && (corridorLength > dLength || Math.Abs(corridorLength - dLength) < error))
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
                        if(remarkType==1)
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

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomsMoreThanDouble = (double dLength) =>
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
    public class FakeHVACFunction
    {
        public static string testDataTableName { get; set; }

        private static string ExcelPath = @"D:\wangT\HVAC-Checker\UnitTestHVACChecker\测试数据\测试数据.xlsx";

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

        public static List<Windows> GetWindowsInRoom(Room room)
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
            List<Windows> windows = new List<Windows>();

            if (hasOpenableOuterWindow)
            {
                Windows window = new Windows(windowId);
                window.openMode = Windows.WindowOpenMode.PushWindow;
                window.isExternalWindow = true;
                windows.Add(window);
            }
            return windows;
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
                room.numberOfPeople= row.GetCell(14).NumericCellValue;
                room.storyNo = (int)row.GetCell(15).NumericCellValue;
                Rooms.Add(room);
             }

           }
           return Rooms;
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
                        if (Type.Contains(roomType)&&name.Contains(roomName)&& position ==roomPosition&&area>=roomArea)
                        {
                            long Id = Convert.ToInt64(row.GetCell(0).ToString());
                            Room room = new Room(Id);
                            room.type = row.GetCell(1).StringCellValue;
                            room.name= row.GetCell(13).StringCellValue;
                            room.area = area;
                            room.roomPosition = (RoomPosition)row.GetCell(12).NumericCellValue;
                            room.numberOfPeople = row.GetCell(14).NumericCellValue;
                            room.storyNo = (int)row.GetCell(15).NumericCellValue;
                            Rooms.Add(room);

                        }

                    }
                    return Rooms;
        }

        public List<Room> GetRooms_single(string roomType)
        {
            List<Room> Rooms = new List<Room>();
            string Type = context.DataRow["房间类型"].ToString();
            if (Type.Contains(roomType))
            {
                long Id = Convert.ToInt64(context.DataRow["房间ID"].ToString());
                Room room = new Room(Id);
                room.type = systemType;
                room.area =Convert.ToDouble(context.DataRow["房间面积"].ToString());
                room.name = context.DataRow["房间名称"].ToString();
                room.numberOfPeople= Convert.ToInt32(context.DataRow["房间人数"].ToString());
                room.roomPosition = (RoomPosition)Convert.ToInt32(context.DataRow["房间位置"].ToString());
                Rooms.Add(room);
            }
            return Rooms;
        }

        public List<Room> GetRoomsMutiArgu_single(string roomType, string roomName, double roomArea, RoomPosition roomPosition)
        {
            List<Room> Rooms = new List<Room>();
            string Type = context.DataRow["房间类型"].ToString();
            double area= Convert.ToDouble(context.DataRow["房间面积"].ToString()); 
            string name= context.DataRow["房间名称"].ToString();
            RoomPosition position= (RoomPosition)Convert.ToInt32(context.DataRow["房间位置"].ToString());
            if (Type.Contains(roomType)&&name.Contains(roomName)&&Math.Abs(roomArea-area)<error&&roomPosition==position)
            {
                long Id = Convert.ToInt64(context.DataRow["房间ID"].ToString());
                Room room = new Room(Id);
                room.type = Type;
                room.area =area;
                room.name = name;
                room.numberOfPeople = Convert.ToInt32(context.DataRow["房间人数"].ToString());
                room.roomPosition = position;
                Rooms.Add(room);
            }
            return Rooms;
        }

        public static List<Room> GetRoomsMoreThan(double dLength)
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
                if (roomType.Contains("走廊"))
                {
                    double corridorLength = double.Parse(row.GetCell(11).ToString());
                    if (corridorLength >dLength||Math.Abs(corridorLength-dLength)<error)
                    {
                        long Id = Convert.ToInt64(row.GetCell(0).ToString());
                        Room room = new Room(Id);
                        room.type = roomType;
                        room.name = row.GetCell(13).StringCellValue;
                        room.area = row.GetCell(10).NumericCellValue;
                        room.roomPosition = (RoomPosition)row.GetCell(12).NumericCellValue;
                        room.numberOfPeople = row.GetCell(14).NumericCellValue;
                        room.storyNo = (int)row.GetCell(15).NumericCellValue;
                        Rooms.Add(room);
                    }
                }
            }
            return Rooms;
        }
        private  TestContext context;
        public  TestContext TestContext
        {
            get { return context; }
            set { context = value; }
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
           for(int index=0;index< minCountOfList; ++index)
           {
                AreComponentViolationEqual(firstList[index], secondList[index]);
           }
        }
        private static void AreComponentViolationEqual(ComponentAnnotation firstComponent, ComponentAnnotation secondComponent)
        {
            Assert.AreEqual(firstComponent.remark, secondComponent.remark);
            Assert.AreEqual(firstComponent.type, secondComponent.type);
            Assert.AreEqual(firstComponent.Id, secondComponent.Id);
        }
    }

}
