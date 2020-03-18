using System;
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
            Window window_1 = new Window(1);
            window_1.openMode = (Window.WindowOpenMode)Convert.ToInt32(context.DataRow["第一个外窗类型"].ToString());
            window_1.isExternalWindow = Convert.ToBoolean(context.DataRow["第一个外窗是否为可开启"].ToString());
            Window window_2 = new Window(2);
            window_2.openMode = (Window.WindowOpenMode)Convert.ToInt32(context.DataRow["第二个外窗类型"].ToString());
            window_2.isExternalWindow = Convert.ToBoolean(context.DataRow["第二个外窗是否为可开启"].ToString());
            Window window_3 = new Window(3);
            window_3.openMode = (Window.WindowOpenMode)Convert.ToInt32(context.DataRow["第三个外窗类型"].ToString());
            window_3.isExternalWindow = Convert.ToBoolean(context.DataRow["第三个外窗是否为可开启"].ToString());

            List<Window> windows = new List<Window>();
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
            Window window_1 = new Window(1);
            window_1.openMode = Window.WindowOpenMode.FixWindow;
            window_1.isExternalWindow = true;
            Window window_2 = new Window(2);
            window_2.openMode = Window.WindowOpenMode.PushWindow;
            window_2.isExternalWindow = false;
            Window window_3 = new Window(3);
            window_3.openMode = Window.WindowOpenMode.FixWindow; ;
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
}
