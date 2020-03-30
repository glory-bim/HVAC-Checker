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

            //    HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomsMoreThanDouble = FakeHVACFunction.GetRoomsMoreThan;

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
                            if(remarkType==1)
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
                              room.m_dArea = Convert.ToDouble(context.DataRow["房间面积"].ToString());
                              room.name = context.DataRow["房间名称"].ToString();
                              room.m_iNumberOfPeople = Convert.ToInt32(context.DataRow["房间人数"].ToString());
                             room.m_eRoomPosition = (RoomPosition)Convert.ToInt32(context.DataRow["房间位置"].ToString());
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
                     if (Type.Contains(roomType) && name.Contains(roomName) &&area>= roomArea &&((int)(roomPosition&position)!=0))
                     {
                         long Id = Convert.ToInt64(context.DataRow["房间ID"].ToString());
                         Room room = new Room(Id);
                         room.type = Type;
                         room.m_dArea = area;
                         room.name = name;
                         room.m_iNumberOfPeople = Convert.ToInt32(context.DataRow["房间人数"].ToString());
                        room.m_eRoomPosition = position;
                         Rooms.Add(room);
                     }
                     return Rooms;
                 };

                //HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomsMoreThanDouble = (double dLength) =>
                //  {
                //      List<Room> Rooms = new List<Room>();
                //      string roomType = context.DataRow["房间类型"].ToString();
                //      double corridorLength = Convert.ToDouble(context.DataRow["走廊长度"].ToString());
                //      if (roomType.Contains("走廊") && (corridorLength > dLength || Math.Abs(corridorLength - dLength) < error))
                //      {


                //          long Id = Convert.ToInt64(context.DataRow["房间ID"].ToString());
                //          Room room = new Room(Id);
                //          room.type = roomType;
                //          room.name = context.DataRow["房间名称"].ToString();
                //          room.m_dArea = Convert.ToDouble(context.DataRow["房间面积"].ToString());
                //         room.m_eRoomPosition = (RoomPosition)Convert.ToInt32(context.DataRow["房间位置"].ToString());
                //          room.m_iNumberOfPeople = Convert.ToInt32(context.DataRow["房间人数"].ToString());
                //          Rooms.Add(room);

                //      }
                //      return Rooms;
                //  };

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
                    int remarkType= Convert.ToInt32(context.DataRow["复审类型"].ToString());
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

                //HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomsMoreThanDouble=(double dLength) =>
                //{
                //    return new List<Room>();
                //};
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

             //   HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomsMoreThanDouble = FakeHVACFunction.GetRoomsMoreThan;

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
                        room.m_dArea = Convert.ToDouble(context.DataRow["房间面积"].ToString());
                        room.name = context.DataRow["房间名称"].ToString();
                        room.m_iNumberOfPeople = Convert.ToInt32(context.DataRow["房间人数"].ToString());
                       room.m_eRoomPosition = (RoomPosition)Convert.ToInt32(context.DataRow["房间位置"].ToString());
                       room.m_iStoryNo= Convert.ToInt32(context.DataRow["房间楼层编号"].ToString());
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
                    if (Type.Contains(roomType) && name.Contains(roomName) && area>=roomArea && ((int)(roomPosition & position) != 0))
                    {
                        long Id = Convert.ToInt64(context.DataRow["房间ID"].ToString());
                        Room room = new Room(Id);
                        room.type = Type;
                        room.m_dArea = area;
                        room.name = name;
                        room.m_iNumberOfPeople = Convert.ToInt32(context.DataRow["房间人数"].ToString());
                       room.m_eRoomPosition = position;
                       room.m_iStoryNo = Convert.ToInt32(context.DataRow["房间楼层编号"].ToString());
                        Rooms.Add(room);
                    }
                    return Rooms;
                };

                //HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomsMoreThanDouble = (double dLength) =>
                //{
                //    List<Room> Rooms = new List<Room>();
                //    string roomType = context.DataRow["房间类型"].ToString();
                //    double corridorLength = Convert.ToDouble(context.DataRow["走廊长度"].ToString());
                //    if (roomType.Contains("走廊") && (corridorLength > dLength || Math.Abs(corridorLength - dLength) < error))
                //    {


                //        long Id = Convert.ToInt64(context.DataRow["房间ID"].ToString());
                //        Room room = new Room(Id);
                //        room.type = roomType;
                //        room.name = context.DataRow["房间名称"].ToString();
                //        room.m_dArea = Convert.ToDouble(context.DataRow["房间面积"].ToString());
                //       room.m_eRoomPosition = (RoomPosition)Convert.ToInt32(context.DataRow["房间位置"].ToString());
                //        room.m_iNumberOfPeople = Convert.ToInt32(context.DataRow["房间人数"].ToString());
                //       room.m_iStoryNo = Convert.ToInt32(context.DataRow["房间楼层编号"].ToString());
                //        Rooms.Add(room);

                //    }
                //    return Rooms;
                //};

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

                //HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomsMoreThanDouble = (double dLength) =>
                //{
                //    return new List<Room>();
                //};
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

               // HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomsMoreThanDouble = FakeHVACFunction.GetRoomsMoreThan;

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

               // HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomsMoreThanDouble = FakeHVACFunction.GetRoomsMoreThan;

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

               // HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomsMoreThanDouble = FakeHVACFunction.GetRoomsMoreThan;

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
                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomContainAirTerminalRoom = FakeHVACFunction.GetRoomContainAirTerminal_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetFanConnectingAirterminalAirTerminal = FakeHVACFunction.GetFanConnectingAirterminal_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetOutletsOfFanFan = FakeHVACFunction.GetOutputLetsOfFan_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomOfAirterminalAirTerminal = FakeHVACFunction.GetRoomOfAirterminal_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.getDoorsBetweenTwoRoomsRoomRoom = FakeHVACFunction.getDoorsBetweenTwoRooms_new;

                //HVAC_CheckEngine.Fakes.ShimHVACFunction.getConnectedRoomsRoom = FakeHVACFunction.getConnectedRooms_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomsString = FakeHVACFunction.GetRooms_new;
                //arrange
                globalData.buildingHeight =50;
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
        public void test_publicBuilding_unPass()
        {

            using (ShimsContext.Create())
            {
                FakeHVACFunction.roomSheetName_new = "房间(通过测试)";
                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomContainAirTerminalRoom = FakeHVACFunction.GetRoomContainAirTerminal_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetFanConnectingAirterminalAirTerminal = FakeHVACFunction.GetFanConnectingAirterminal_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetOutletsOfFanFan = FakeHVACFunction.GetOutputLetsOfFan_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomOfAirterminalAirTerminal = FakeHVACFunction.GetRoomOfAirterminal_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.getDoorsBetweenTwoRoomsRoomRoom = FakeHVACFunction.getDoorsBetweenTwoRooms_new;

              //  HVAC_CheckEngine.Fakes.ShimHVACFunction.getConnectedRoomsRoom = FakeHVACFunction.getConnectedRooms_new;

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

    }

        public static class FakeHVACFunction
    {
        public static string testDataTableName { get; set; }

        private static string ExcelPath = @"D:\wangT\HVAC-Checker\UnitTestHVACChecker\测试数据\测试数据.xlsx";

        public static string ExcelPath_new = @"D:\wangT\HVAC-Checker\UnitTestHVACChecker\测试数据\测试数据_new.xlsx";

        public static string roomSheetName_new = "房间";

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

        public static List<AirTerminal> GetRoomContainAirTerminal_new(Room room)
        {
            string importExcelPath = ExcelPath_new;
            //打开测试数据文件
            IWorkbook workbook = WorkbookFactory.Create(importExcelPath);
            //读取测试数据表
            ISheet sheet_rooms = workbook.GetSheet(roomSheetName_new);
            ISheet sheet_airTerminals = workbook.GetSheet("风口");
            ISheet sheet_fans = workbook.GetSheet("风机");
            List<AirTerminal> airTerminals = new List<AirTerminal>();
            //用房间ID找到测试表对应的行
            long roomId = room.Id.Value;
            IRow row = (IRow)sheet_rooms.GetRow((int)roomId);
            //从对应行中读取房间中所含的所有风口的集合
            string idString= row.GetCell(2).StringCellValue;
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
            ISheet sheet_fans = workbook.GetSheet("风机");
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
                window.openMode = Window.WindowOpenMode.PushWindow;
                window.isExternalWindow = true;
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
            string idList= row.GetCell(4).StringCellValue;
            List<Window> windows = getAllWindowsByIdString(idList);

            return windows;
        }

        public static List<AirTerminal> GetOutputLetsOfFan_new(Fan fan)
        {
            string importExcelPath = ExcelPath_new;
            //打开数据文件
            IWorkbook workbook = WorkbookFactory.Create(importExcelPath);
            //读取数据表格
            ISheet sheet_fans = workbook.GetSheet("风机");
            ISheet sheet_airTerminals = workbook.GetSheet("风口");
            //找到与风机id对应的数据行

            IRow row = (IRow)sheet_fans.GetRow((int)fan.Id.Value);

            //获得风机的送风口连接id字符串
            string idString = row.GetCell(sheet_fans.getColNumber("连接的送风口ID"))?.ToString();
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
                string idString = row.GetCell(sheet_rooms.getColNumber("包含的风口")).StringCellValue;
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
                room.m_dArea = row.GetCell(10).NumericCellValue;
               room.m_eRoomPosition=(RoomPosition)row.GetCell(12).NumericCellValue;
                room.m_iNumberOfPeople= (int)row.GetCell(14).NumericCellValue;
               room.m_iStoryNo = (int)row.GetCell(15).NumericCellValue;
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
                    room.m_dArea = row.GetCell(sheet_rooms.getColNumber("房间面积")).NumericCellValue;
                    room.m_eRoomPosition = (RoomPosition)row.GetCell(sheet_rooms.getColNumber("房间位置")).NumericCellValue;
                    room.m_iNumberOfPeople = (int)row.GetCell(sheet_rooms.getColNumber("房间人数")).NumericCellValue;
                    room.m_iStoryNo = (int)row.GetCell(sheet_rooms.getColNumber("房间楼层编号")).NumericCellValue;
                    rooms.Add(room);
                }

            }
            return rooms;
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
                            room.m_dArea = area;
                           room.m_eRoomPosition = (RoomPosition)row.GetCell(12).NumericCellValue;
                            room.m_iNumberOfPeople = (int)row.GetCell(14).NumericCellValue;
                           room.m_iStoryNo = (int)row.GetCell(15).NumericCellValue;
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
                room.m_dArea = row.GetCell(10).NumericCellValue;
               room.m_eRoomPosition = (RoomPosition)row.GetCell(12).NumericCellValue;
                room.m_iNumberOfPeople = (int)row.GetCell(14).NumericCellValue;
               room.m_iStoryNo = (int)row.GetCell(15).NumericCellValue;
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
            string idString = row.GetCell(sheet_rooms.getColNumber("包含的门ID")).StringCellValue;
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
                        room.m_dArea = row.GetCell(10).NumericCellValue;
                       room.m_eRoomPosition = (RoomPosition)row.GetCell(12).NumericCellValue;
                        room.m_iNumberOfPeople = (int)row.GetCell(14).NumericCellValue;
                       room.m_iStoryNo = (int)row.GetCell(15).NumericCellValue;
                        Rooms.Add(room);
                    }
                }
            }
            return Rooms;
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
            room.m_dArea = row.GetCell(sheet_rooms.getColNumber("房间面积")).NumericCellValue;
           room.m_eRoomPosition = (RoomPosition)row.GetCell(sheet_rooms.getColNumber("房间位置")).NumericCellValue;
            room.m_iNumberOfPeople = (int)row.GetCell(sheet_rooms.getColNumber("房间人数")).NumericCellValue;
           room.m_iStoryNo = (int)row.GetCell(sheet_rooms.getColNumber("房间楼层编号")).NumericCellValue;

            return room;
        }

        private static Fan getFanById(long id)
        {
            string importExcelPath = ExcelPath_new;
            //打开测试数据文件
            IWorkbook workbook = WorkbookFactory.Create(importExcelPath);
            //读取测试数据表
            ISheet sheet_rooms = workbook.GetSheet("风机");
            IRow row = (IRow)sheet_rooms.GetRow((int)id);

            Fan fan = new Fan(id);

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
                room.m_dArea = row.GetCell(sheet_rooms.getColNumber("房间面积")).NumericCellValue;
               room.m_eRoomPosition = (RoomPosition)row.GetCell(sheet_rooms.getColNumber("房间位置")).NumericCellValue;
                room.m_iNumberOfPeople = (int)row.GetCell(sheet_rooms.getColNumber("房间人数")).NumericCellValue;
               room.m_iStoryNo = (int)row.GetCell(sheet_rooms.getColNumber("房间楼层编号")).NumericCellValue;
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
            foreach (long id in idList)
            {
                Window window = new Window(id);
                windows.Add(window);
            }
            return windows;
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
    }

}
