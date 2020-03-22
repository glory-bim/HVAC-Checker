﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HVAC_CheckEngine;
using System.Collections.Generic;
using NPOI;
using NPOI.SS.UserModel;
using Microsoft.QualityTools.Testing.Fakes;
using UnitTestHVACChecker;

namespace UnitTestAssitantFunction
{
    [TestClass]
    public class getAirTerminalOfCertainSystem_Test
    {

        [TestMethod]
        [DeploymentItem(@"D:\wangT\HVAC-Checker\UnitTestHVACChecker\测试数据\测试数据.xlsx")]
        [DataSource("MyExcelDataSource")]
        public void test_differentOrder()
        {
            //arrange
            AirTerminal airTerminal_1 = new AirTerminal(1);
            airTerminal_1.systemType = context.DataRow["第一个风口系统类型"].ToString();
            AirTerminal airTerminal_2 = new AirTerminal(2);
            airTerminal_2.systemType = context.DataRow["第二个风口系统类型"].ToString();
            AirTerminal airTerminal_3 = new AirTerminal(3);
            airTerminal_3.systemType = context.DataRow["第三个风口系统类型"].ToString();

            List<AirTerminal> airTerminals = new List<AirTerminal>();
            airTerminals.Add(airTerminal_1);
            airTerminals.Add(airTerminal_2);
            airTerminals.Add(airTerminal_3);

            string aimSystemType = context.DataRow["目标风口系统类型"].ToString();
            int indexOfAimAirTerminals = Int32.Parse(context.DataRow["目标风口编号"].ToString());

            //act
            AirTerminal aimAirTerminal = assistantFunctions.GetAirTerminalOfCertainSystem(airTerminals, aimSystemType);

            //assert
            Assert.IsNotNull(aimAirTerminal);
            Assert.AreEqual(aimSystemType, aimAirTerminal.systemType);
            Assert.AreEqual(indexOfAimAirTerminals, aimAirTerminal.Id);
        }

        [TestMethod]
        public void test_doNotHaveAimTerminal()
        {
            //arrange
            AirTerminal airTerminal_1 = new AirTerminal(1);
            airTerminal_1.systemType = "空调送风";
            AirTerminal airTerminal_2 = new AirTerminal(2);
            airTerminal_2.systemType = "空调回风";
            AirTerminal airTerminal_3 = new AirTerminal(3);
            airTerminal_3.systemType = "排风";

            List<AirTerminal> airTerminals = new List<AirTerminal>();
            airTerminals.Add(airTerminal_1);
            airTerminals.Add(airTerminal_2);
            airTerminals.Add(airTerminal_3);

            string aimSystemType = "正压送风";


            //act
            AirTerminal aimAirTerminal = assistantFunctions.GetAirTerminalOfCertainSystem(airTerminals, aimSystemType);

            //assert
            Assert.IsNull(aimAirTerminal);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void test_SystemTypeIsNull()
        {
            //arrange
            List<AirTerminal> airTerminals = new List<AirTerminal>();
            //act
            assistantFunctions.GetAirTerminalOfCertainSystem(airTerminals, null);
        }

        [TestMethod]
        public void test_AirTerminalsIsNull()
        {
            //arrange

            //act
            AirTerminal aimAirTerminal = assistantFunctions.GetAirTerminalOfCertainSystem(null, string.Empty);
            //assert
            Assert.IsNull(aimAirTerminal);
        }

        private TestContext context;

        public TestContext TestContext
        {
            get { return context; }
            set { context = value; }
        }
    }

    [TestClass]
    public class getOpenableOuterWindow_Test
    {
        [TestMethod]
        [DeploymentItem(@"D:\wangT\HVAC-Checker\UnitTestHVACChecker\测试数据\测试数据.xlsx")]
        [DataSource("MyExcelDataSource2")]
        public void test_differentOrder()
        {
            //arrange
            Windows window_1 = new Windows(1);
           
            window_1.isExternalWindow = Convert.ToBoolean(context.DataRow["第一个外窗是否为可开启"].ToString());
       
            Windows window_2 = new Windows(2);
            window_2.isExternalWindow = Convert.ToBoolean(context.DataRow["第二个外窗是否为可开启"].ToString());
           

            Windows window_3 = new Windows(3);
            window_3.isExternalWindow = Convert.ToBoolean(context.DataRow["第三个外窗是否为可开启"].ToString());
     

            List<Windows> windows = new List<Windows>();
            windows.Add(window_1);
            windows.Add(window_2);
            windows.Add(window_3);


            int indexOfAimWindow = Int32.Parse(context.DataRow["目标外窗编号"].ToString());

            //act
            Window aimWindow = assistantFunctions.GetOpenableOuterWindow(windows);

            //assert
            Assert.IsNotNull(aimWindow);
            Assert.AreEqual(indexOfAimWindow, aimWindow.Id);
        }
        [TestMethod]

        public void test_doNotHaveAimWindow()
        {
            Windows window_1 = new Windows(1);
            
            window_1.isExternalWindow = true;
            Windows window_2 = new Windows(2);
            
            window_2.isExternalWindow = false;
            Windows window_3 = new Windows(3);
           
            window_3.isExternalWindow = false;

            List<Window> windows = new List<Window>();
            windows.Add(window_1);
            windows.Add(window_2);
            windows.Add(window_3);


            //act
            Window aimWindow = assistantFunctions.GetOpenableOuterWindow(windows);

            //assert
            Assert.IsNull(aimWindow);

        }

        [TestMethod]
        public void test_WindowsIsNull()
        {

            //act
            Window aimWindow = assistantFunctions.GetOpenableOuterWindow(null);

            //assert
            Assert.IsNull(aimWindow);

        }

        private TestContext context;

        public TestContext TestContext
        {
            get { return context; }
            set { context = value; }
        }
    }

    [TestClass]
    public class getSumOfAllRoomsAreaOfRooms_test
    {
        [TestMethod]
        public void test_getSumOfAllRoomsAreaOfRooms()
        {
            //打开测试数据文件
            IWorkbook Workbook = WorkbookFactory.Create(ExcelPath);
            //读取测试数据表
            ISheet Sheet = Workbook.GetSheet("测试区域面积计算");

            //arrang
            List<Room> rooms = new List<Room>();
            IRow row = null;
            //依次读取数据行，并根据数据内容创建房间，并加入房间集合中
            for (int index = 1; index <= Sheet.LastRowNum - 1; ++index)
            {

                row = (IRow)Sheet.GetRow(index);
                long roomId = Convert.ToInt64(row.GetCell(0).ToString());
                Room room = new Room(roomId);
                room.type = row.GetCell(1).ToString();
                room.area = row.GetCell(2).NumericCellValue;
                rooms.Add(room);
            }
            row = (IRow)Sheet.GetRow(9);
            double aimSum = row.GetCell(2).NumericCellValue;

            //act
            double sum = assistantFunctions.getSumOfAllRoomsAreaOfRooms(rooms);

            //assert

            Assert.AreEqual(aimSum, sum);
        }

        private static string ExcelPath = @"D:\wangT\HVAC-Checker\UnitTestHVACChecker\测试数据\测试数据_assitantFunction.xlsx";

        private static double error = 0.0001;
    }

    [TestClass]
    public class isStairPressureAirSystemIndependent_test
    {
        [TestMethod]
        public void test_isStairPressureAirSystemIndependent_pass()
        {
            using (ShimsContext.Create())
            {

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomContainAirTerminalRoom = FakeHVACFunction.GetRoomContainAirTerminal_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetFanConnectingAirterminalAirTerminal = FakeHVACFunction.GetFanConnectingAirterminal_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetOutletsOfFanFan = FakeHVACFunction.GetOutputLetsOfFan_new;

                //arrange
                FakeHVACFunction.ExcelPath_new = @"D:\wangT\HVAC-Checker\UnitTestHVACChecker\测试数据\测试数据_GB51251_2017_3_1_5.xlsx";
                Room stairCase = new Room(1);
                //打开测试数据文件

                //act
                bool isIndependent = assistantFunctions.isStairPressureAirSystemIndependent(stairCase);

                //assert

                Assert.IsTrue(isIndependent);

            }
        }

        [TestMethod]
        public void test_isStairPressureAirSystemIndependent_unPass()
        {
            using (ShimsContext.Create())
            {

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomContainAirTerminalRoom = FakeHVACFunction.GetRoomContainAirTerminal_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetFanConnectingAirterminalAirTerminal = FakeHVACFunction.GetFanConnectingAirterminal_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetOutletsOfFanFan = FakeHVACFunction.GetOutputLetsOfFan_new;

                //arrange
                FakeHVACFunction.ExcelPath_new = @"D:\wangT\HVAC-Checker\UnitTestHVACChecker\测试数据\测试数据_GB51251_2017_3_1_5.xlsx";
                Room stairCase = new Room(2);
                //打开测试数据文件

                //act
                bool isIndependent = assistantFunctions.isStairPressureAirSystemIndependent(stairCase);

                //assert

                Assert.IsFalse(isIndependent);

            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void test_isStairPressureAirSystemIndependent_ArgumentException_noPressureSystem()
        {
            using (ShimsContext.Create())
            {

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomContainAirTerminalRoom = FakeHVACFunction.GetRoomContainAirTerminal_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetFanConnectingAirterminalAirTerminal = FakeHVACFunction.GetFanConnectingAirterminal_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetOutletsOfFanFan = FakeHVACFunction.GetOutputLetsOfFan_new;

                //arrange
                FakeHVACFunction.ExcelPath_new = @"D:\wangT\HVAC-Checker\UnitTestHVACChecker\测试数据\测试数据_GB51251_2017_3_1_5.xlsx";
                Room stairCase = new Room(17);
                //打开测试数据文件

                //act
                bool isIndependent = assistantFunctions.isStairPressureAirSystemIndependent(stairCase);


            }
        }


        [TestMethod]
        [ExpectedException(typeof(modelException))]
        public void test_isStairPressureAirSystemIndependent_modelException()
        {
            using (ShimsContext.Create())
            {

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomContainAirTerminalRoom = FakeHVACFunction.GetRoomContainAirTerminal_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetFanConnectingAirterminalAirTerminal = FakeHVACFunction.GetFanConnectingAirterminal_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetOutletsOfFanFan = FakeHVACFunction.GetOutputLetsOfFan_new;

                //arrange
                FakeHVACFunction.ExcelPath_new = @"D:\wangT\HVAC-Checker\UnitTestHVACChecker\测试数据\测试数据_GB51251_2017_3_1_5.xlsx";
                Room stairCase = new Room(18);
                //打开测试数据文件

                //act
                bool isIndependent = assistantFunctions.isStairPressureAirSystemIndependent(stairCase);

                //assert

            }
        }
    }

    [TestClass]
    public class isAtriaPressureAirSystemIndependent_test
    {
        [TestMethod]
        public void test_isAtriaPressureAirSystemIndependent_pass()
        {
            using (ShimsContext.Create())
            {

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomContainAirTerminalRoom = FakeHVACFunction.GetRoomContainAirTerminal_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetFanConnectingAirterminalAirTerminal = FakeHVACFunction.GetFanConnectingAirterminal_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetOutletsOfFanFan = FakeHVACFunction.GetOutputLetsOfFan_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomOfAirterminalAirTerminal = FakeHVACFunction.GetRoomOfAirterminal_new;
                //arrange
                FakeHVACFunction.ExcelPath_new = @"D:\wangT\HVAC-Checker\UnitTestHVACChecker\测试数据\测试数据_GB51251_2017_3_1_5.xlsx";
                Room atria = new Room(3);
                //打开测试数据文件

                //act
                bool isIndependent = assistantFunctions.isAtriaPressureAirSystemIndependent(atria);

                //assert

                Assert.IsTrue(isIndependent);

            }
        }

        [TestMethod]
        public void test_isAtriaPressureAirSystemIndependent_unpass()
        {
            using (ShimsContext.Create())
            {

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomContainAirTerminalRoom = FakeHVACFunction.GetRoomContainAirTerminal_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetFanConnectingAirterminalAirTerminal = FakeHVACFunction.GetFanConnectingAirterminal_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetOutletsOfFanFan = FakeHVACFunction.GetOutputLetsOfFan_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomOfAirterminalAirTerminal = FakeHVACFunction.GetRoomOfAirterminal_new;
                //arrange
                FakeHVACFunction.ExcelPath_new = @"D:\wangT\HVAC-Checker\UnitTestHVACChecker\测试数据\测试数据_GB51251_2017_3_1_5.xlsx";
                Room atria = new Room(8);
                //打开测试数据文件

                //act
                bool isIndependent = assistantFunctions.isAtriaPressureAirSystemIndependent(atria);

                //assert

                Assert.IsFalse(isIndependent);

            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void test_isAtriaPressureAirSystemIndependent_ArgumentException()
        {
            using (ShimsContext.Create())
            {

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomContainAirTerminalRoom = FakeHVACFunction.GetRoomContainAirTerminal_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetFanConnectingAirterminalAirTerminal = FakeHVACFunction.GetFanConnectingAirterminal_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetOutletsOfFanFan = FakeHVACFunction.GetOutputLetsOfFan_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomOfAirterminalAirTerminal = FakeHVACFunction.GetRoomOfAirterminal_new;
                //arrange
                FakeHVACFunction.ExcelPath_new = @"D:\wangT\HVAC-Checker\UnitTestHVACChecker\测试数据\测试数据_GB51251_2017_3_1_5.xlsx";
                Room atria = new Room(11);
                //打开测试数据文件

                //act
                bool isIndependent = assistantFunctions.isAtriaPressureAirSystemIndependent(atria);

                //assert


            }
        }

        [TestMethod]
        [ExpectedException(typeof(modelException))]
        public void test_isAtriaPressureAirSystemIndependent_modelException()
        {
            using (ShimsContext.Create())
            {

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomContainAirTerminalRoom = FakeHVACFunction.GetRoomContainAirTerminal_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetFanConnectingAirterminalAirTerminal = FakeHVACFunction.GetFanConnectingAirterminal_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetOutletsOfFanFan = FakeHVACFunction.GetOutputLetsOfFan_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomOfAirterminalAirTerminal = FakeHVACFunction.GetRoomOfAirterminal_new;
                //arrange
                FakeHVACFunction.ExcelPath_new = @"D:\wangT\HVAC-Checker\UnitTestHVACChecker\测试数据\测试数据_GB51251_2017_3_1_5.xlsx";
                Room atria = new Room(12);
                //打开测试数据文件

                //act
                bool isIndependent = assistantFunctions.isAtriaPressureAirSystemIndependent(atria);

                //assert


            }
        }
    }

    [TestClass]
    public class getDoorsToCorridorOfAtria_test
    {
        [TestMethod]
        public void test_getDoorsToCorridorOfAtria_pass()
        {
            using (ShimsContext.Create())
            {
                HVAC_CheckEngine.Fakes.ShimHVACFunction.getDoorsBetweenTwoRoomsRoomRoom = FakeHVACFunction.getDoorsBetweenTwoRooms_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.getConnectedRoomsRoom = FakeHVACFunction.getConnectedRooms_new;
                //arrange
                FakeHVACFunction.ExcelPath_new = @"D:\wangT\HVAC-Checker\UnitTestHVACChecker\测试数据\测试数据_GB51251_2017_3_1_5.xlsx";
                Room atria = new Room(21);
                List<Door> expectDoors = new List<Door>();
                expectDoors.Add(new Door(20));
                expectDoors.Add(new Door(21));
                //打开测试数据文件
                //act
                List<Door> doors = assistantFunctions.getDoorsToCorridorOfAtria(atria);

                //assert

                Custom_Assert.AreListsEqual(expectDoors, doors);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(modelException))]
        public void test_getDoorsToCorridorOfAtria_modelException_noLinkOtherRoom()
        {
            using (ShimsContext.Create())
            {
                HVAC_CheckEngine.Fakes.ShimHVACFunction.getDoorsBetweenTwoRoomsRoomRoom = FakeHVACFunction.getDoorsBetweenTwoRooms_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.getConnectedRoomsRoom = FakeHVACFunction.getConnectedRooms_new;
                //arrange
                FakeHVACFunction.ExcelPath_new = @"D:\wangT\HVAC-Checker\UnitTestHVACChecker\测试数据\测试数据_GB51251_2017_3_1_5.xlsx";
                Room atria = new Room(23);

                //打开测试数据文件
                //act
                List<Door> doors = assistantFunctions.getDoorsToCorridorOfAtria(atria);

                //assert
            }
        }


        [TestMethod]
        [ExpectedException(typeof(modelException))]
        public void test_getDoorsToCorridorOfAtria_modelException_OnlyLinkStairCase()
        {
            using (ShimsContext.Create())
            {
                HVAC_CheckEngine.Fakes.ShimHVACFunction.getDoorsBetweenTwoRoomsRoomRoom = FakeHVACFunction.getDoorsBetweenTwoRooms_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.getConnectedRoomsRoom = FakeHVACFunction.getConnectedRooms_new;
                //arrange
                FakeHVACFunction.ExcelPath_new = @"D:\wangT\HVAC-Checker\UnitTestHVACChecker\测试数据\测试数据_GB51251_2017_3_1_5.xlsx";
                Room atria = new Room(24);

                //打开测试数据文件
                //act
                List<Door> doors = assistantFunctions.getDoorsToCorridorOfAtria(atria);

                //assert
            }
        }
    }

    [TestClass]
    public class getFloorDivisionOfAirTerminalsBottomUp_test
    {
        [TestMethod]
        public void test_getFloorDivisionOfAirTerminalsBottomUp_pass()
        {
            using (ShimsContext.Create())
            {
                FakeHVACFunction.ExcelPath_new = @"D:\wangT\HVAC-Checker\UnitTestHVACChecker\测试数据\测试数据_GB51251_2017_3_3_1.xlsx";

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetFloors = FakeHVACFunction.GetAllFLoorsOfBuilding_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomContainAirTerminalRoom = FakeHVACFunction.GetRoomContainAirTerminal_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomsString = FakeHVACFunction.GetRooms_new;
                //arrange
                

                List<Room> rooms = HVACFunction.GetRooms("防烟楼梯间");

                 List<AirTerminal> airTerminals = HVACFunction.GetRoomContainAirTerminal(rooms[2]);

                //打开测试数据文件
                string importExcelPath = FakeHVACFunction.ExcelPath_new;
                //打开数据文件
                IWorkbook workbook = WorkbookFactory.Create(importExcelPath);
                //读取数据表格
                ISheet sheet_airTerminals = workbook.GetSheet("风口");

                Dictionary<AirTerminal, List<Floor>> aimResult = new Dictionary<AirTerminal, List<Floor>>();

                //依次读取数据行，并根据数据内容创建房间，并加入房间集合中
                for (int index = 11; index <= 14; ++index)
                {
                    IRow row = (IRow)sheet_airTerminals.GetRow(index);
                   
                    long airTerminalId = Convert.ToInt64(row.GetCell(sheet_airTerminals.getColNumber("ID")).NumericCellValue);
                    AirTerminal airTerminal = new AirTerminal(airTerminalId);
                    airTerminal.systemType= row.GetCell(sheet_airTerminals.getColNumber("系统类型")).ToString();
                    airTerminal.storyNo= Convert.ToInt32(row.GetCell(sheet_airTerminals.getColNumber("楼层编号")).NumericCellValue);
                    string affordStoryIdString= row.GetCell(sheet_airTerminals.getColNumber("负担的楼层")).ToString();
                    List<Floor> floors = FakeHVACFunction.getAllFloorsByIdString(affordStoryIdString);
                    aimResult[airTerminal] = floors;
                }

                //打开测试数据文件
                //act
                Dictionary<AirTerminal, List<Floor>> result = assistantFunctions.getFloorDivisionOfAirTerminalsBottomUp(HVACFunction.GetFloors(), airTerminals);

                //assert

                Custom_Assert.AreDictionaryEqual(aimResult, result);
            }

        }

       

        

    }

    [TestClass]
    public class getFloorDivisionOfAirTerminalsTopToBottom_test
    {
        [TestMethod]
        public void test_getFloorDivisionOfAirTerminalsTopToBottom_pass()
        {
            using (ShimsContext.Create())
            {
                FakeHVACFunction.roomSheetName_new = "房间";

                FakeHVACFunction.ExcelPath_new = @"D:\wangT\HVAC-Checker\UnitTestHVACChecker\测试数据\测试数据_GB51251_2017_3_3_1.xlsx";

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetFloors = FakeHVACFunction.GetAllFLoorsOfBuilding_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomContainAirTerminalRoom = FakeHVACFunction.GetRoomContainAirTerminal_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomsString = FakeHVACFunction.GetRooms_new;
                //arrange


                List<Room> rooms = HVACFunction.GetRooms("防烟楼梯间");

                List<AirTerminal> airTerminals = HVACFunction.GetRoomContainAirTerminal(rooms[4]);

                //打开测试数据文件
                string importExcelPath = FakeHVACFunction.ExcelPath_new;
                //打开数据文件
                IWorkbook workbook = WorkbookFactory.Create(importExcelPath);
                //读取数据表格
                ISheet sheet_airTerminals = workbook.GetSheet("风口");

                Dictionary<AirTerminal, List<Floor>> aimResult = new Dictionary<AirTerminal, List<Floor>>();

                //依次读取数据行，并根据数据内容创建房间，并加入房间集合中
                for (int index = 8; index <= 10; ++index)
                {
                    IRow row = (IRow)sheet_airTerminals.GetRow(index);

                    long airTerminalId = Convert.ToInt64(row.GetCell(sheet_airTerminals.getColNumber("ID")).NumericCellValue);
                    AirTerminal airTerminal = new AirTerminal(airTerminalId);
                    airTerminal.systemType = row.GetCell(sheet_airTerminals.getColNumber("系统类型")).ToString();
                    airTerminal.storyNo = Convert.ToInt32(row.GetCell(sheet_airTerminals.getColNumber("楼层编号")).NumericCellValue);
                    string affordStoryIdString = row.GetCell(sheet_airTerminals.getColNumber("负担的楼层")).ToString();
                    List<Floor> floors = FakeHVACFunction.getAllFloorsByIdString(affordStoryIdString);
                    aimResult[airTerminal] = floors;
                }

                //打开测试数据文件
                //act
          
                Dictionary<AirTerminal, List<Floor>> result = assistantFunctions.getFloorDivisionOfAirTerminalsTopToBottom(assistantFunctions.filterFloorsBetweenlowestAndHighestStoryNo(1, 15), airTerminals);

                //assert

                Custom_Assert.AreDictionaryEqual(aimResult, result);
            }

        }

    }

    [TestClass]
    public class getAffordHeightOfFanBottomUp_test
    {
        [TestMethod]
        public void test_getAffordHeightOfFanBottomUp_test_pass()
        {
            using (ShimsContext.Create())
            {
                FakeHVACFunction.ExcelPath_new = @"D:\wangT\HVAC-Checker\UnitTestHVACChecker\测试数据\测试数据_GB51251_2017_3_3_1.xlsx";

                FakeHVACFunction.roomSheetName_new = "房间";


                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetFloors = FakeHVACFunction.GetAllFLoorsOfBuilding_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomContainAirTerminalRoom = FakeHVACFunction.GetRoomContainAirTerminal_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomsString = FakeHVACFunction.GetRooms_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetOutletsOfFanFan = FakeHVACFunction.GetOutputLetsOfFan_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.getHighestStoryNoOfRoomRoom = FakeHVACFunction.getHighestStoryNoOfRoom_new;
                //arrange



                //打开测试数据文件
                string importExcelPath = FakeHVACFunction.ExcelPath_new;
                //打开数据文件
                IWorkbook workbook = WorkbookFactory.Create(importExcelPath);
                //读取数据表格
                ISheet sheet_fans = workbook.GetSheet("风机");

                Dictionary<AirTerminal, List<Floor>> aimResult = new Dictionary<AirTerminal, List<Floor>>();

                Fan fan = new Fan(10);

                double aimHight = 70000;
                //打开测试数据文件
                //act
                ISheet sheet_rooms = workbook.GetSheet("房间");
                IRow row = (IRow)sheet_rooms.GetRow(1);
                Room stairCase = new Room(1);
                

                stairCase.type = row.GetCell(sheet_rooms.getColNumber("房间类型")).StringCellValue;
                stairCase.name = row.GetCell(sheet_rooms.getColNumber("房间名称")).StringCellValue;
                stairCase.area = row.GetCell(sheet_rooms.getColNumber("房间面积")).NumericCellValue;
                stairCase.roomPosition = (RoomPosition)row.GetCell(sheet_rooms.getColNumber("房间位置")).NumericCellValue;
                stairCase.numberOfPeople = (int)row.GetCell(sheet_rooms.getColNumber("房间人数")).NumericCellValue;
                stairCase.storyNo = (int)row.GetCell(sheet_rooms.getColNumber("房间楼层编号")).NumericCellValue;
                List<AirTerminal> airTerminals = HVACFunction.GetRoomContainAirTerminal(stairCase);

                int highestStoryNo = HVACFunction.getHighestStoryNoOfRoom(stairCase);
                int lowestStoryNo = stairCase.storyNo.Value;
                List<Floor> floors = assistantFunctions.filterFloorsBetweenlowestAndHighestStoryNo(lowestStoryNo, highestStoryNo);

                double hight = assistantFunctions.getAffordHeightOfFanByFloorDivision(fan,assistantFunctions.getFloorDivisionOfAirTerminalsBottomUp(floors, airTerminals));

                //assert

                Assert.AreEqual(aimHight, hight);
            }

        }


    }

    [TestClass]
    public class getAffordHeightOfFanTopToBottom_test
    {
        [TestMethod]
        public void test_getAffordHeightOfFanTopToBottom_test_pass()
        {
            using (ShimsContext.Create())
            {
                FakeHVACFunction.ExcelPath_new = @"D:\wangT\HVAC-Checker\UnitTestHVACChecker\测试数据\测试数据_GB51251_2017_3_3_1.xlsx";

                FakeHVACFunction.roomSheetName_new = "房间";


                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetFloors = FakeHVACFunction.GetAllFLoorsOfBuilding_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomContainAirTerminalRoom = FakeHVACFunction.GetRoomContainAirTerminal_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomsString = FakeHVACFunction.GetRooms_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetOutletsOfFanFan = FakeHVACFunction.GetOutputLetsOfFan_new;

                HVAC_CheckEngine.Fakes.ShimHVACFunction.getHighestStoryNoOfRoomRoom = FakeHVACFunction.getHighestStoryNoOfRoom_new;
                //arrange



                //打开测试数据文件
                string importExcelPath = FakeHVACFunction.ExcelPath_new;
                //打开数据文件
                IWorkbook workbook = WorkbookFactory.Create(importExcelPath);
                //读取数据表格
                ISheet sheet_fans = workbook.GetSheet("风机");

                Dictionary<AirTerminal, List<Floor>> aimResult = new Dictionary<AirTerminal, List<Floor>>();

                Fan fan = new Fan(10);

                double aimHight = 90000;
                //打开测试数据文件
                //act
                ISheet sheet_rooms = workbook.GetSheet("房间");
                IRow row = (IRow)sheet_rooms.GetRow(4);
                Room stairCase = new Room(4);


                stairCase.type = row.GetCell(sheet_rooms.getColNumber("房间类型")).StringCellValue;
                stairCase.name = row.GetCell(sheet_rooms.getColNumber("房间名称")).StringCellValue;
                stairCase.area = row.GetCell(sheet_rooms.getColNumber("房间面积")).NumericCellValue;
                stairCase.roomPosition = (RoomPosition)row.GetCell(sheet_rooms.getColNumber("房间位置")).NumericCellValue;
                stairCase.numberOfPeople = (int)row.GetCell(sheet_rooms.getColNumber("房间人数")).NumericCellValue;
                stairCase.storyNo = (int)row.GetCell(sheet_rooms.getColNumber("房间楼层编号")).NumericCellValue;
                List<AirTerminal> airTerminals = HVACFunction.GetRoomContainAirTerminal(stairCase);

                int highestStoryNo = HVACFunction.getHighestStoryNoOfRoom(stairCase);
                int lowestStoryNo = stairCase.storyNo.Value;
                List<Floor> floors = assistantFunctions.filterFloorsBetweenlowestAndHighestStoryNo(lowestStoryNo, highestStoryNo);

                double hight = assistantFunctions.getAffordHeightOfFanByFloorDivision(fan, assistantFunctions.getFloorDivisionOfAirTerminalsTopToBottom(floors, airTerminals));

                //assert

                Assert.AreEqual(aimHight, hight);

            }
        }
    }

}
