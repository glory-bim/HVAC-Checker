using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HVAC_CheckEngine;
using System.Collections.Generic;


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

           string aimSystemType= context.DataRow["目标风口系统类型"].ToString();
           int indexOfAimAirTerminals = Int32.Parse(context.DataRow["目标风口编号"].ToString());

           //act
           AirTerminal aimAirTerminal= assistantFunctions.GetAirTerminalOfCertainSystem(airTerminals, aimSystemType);

           //assert
           Assert.IsNotNull(aimAirTerminal);
           Assert.AreEqual(aimSystemType,aimAirTerminal.systemType);
           Assert.AreEqual(indexOfAimAirTerminals,aimAirTerminal.Id );
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
            List<AirTerminal> airTerminals =new List<AirTerminal>();
            //act
            assistantFunctions.GetAirTerminalOfCertainSystem(airTerminals, null);
        }

        [TestMethod]
        public void test_AirTerminalsIsNull()
        {
            //arrange
          
            //act
            AirTerminal aimAirTerminal= assistantFunctions.GetAirTerminalOfCertainSystem(null, string.Empty);
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
            window_1.openMode =(Windows.WindowOpenMode)Convert.ToInt32(context.DataRow["第一个外窗类型"].ToString());
            window_1.isExternalWindow = Convert.ToBoolean(context.DataRow["第一个外窗是否为可开启"].ToString());
            Windows window_2 = new Windows(2);
            window_2.openMode = (Windows.WindowOpenMode)Convert.ToInt32(context.DataRow["第二个外窗类型"].ToString());
            window_2.isExternalWindow = Convert.ToBoolean(context.DataRow["第二个外窗是否为可开启"].ToString());
            Windows window_3 = new Windows(3);
            window_3.openMode = (Windows.WindowOpenMode)Convert.ToInt32(context.DataRow["第三个外窗类型"].ToString());
            window_3.isExternalWindow = Convert.ToBoolean(context.DataRow["第三个外窗是否为可开启"].ToString());

            List<Windows> windows = new List<Windows>();
            windows.Add(window_1);
            windows.Add(window_2);
            windows.Add(window_3);

          
            int indexOfAimWindow = Int32.Parse(context.DataRow["目标外窗编号"].ToString());

            //act
            Windows aimWindow = assistantFunctions.GetOpenableOuterWindow(windows);

            //assert
            Assert.IsNotNull(aimWindow);
            Assert.AreEqual(indexOfAimWindow, aimWindow.Id);
        }
        [TestMethod]
   
        public void test_doNotHaveAimWindow()
        {
            Windows window_1 = new Windows(1);
            window_1.openMode = Windows.WindowOpenMode.FixWindow;
            window_1.isExternalWindow =true;
            Windows window_2 = new Windows(2);
            window_2.openMode = Windows.WindowOpenMode.PushWindow;
            window_2.isExternalWindow = false;
            Windows window_3 = new Windows(3);
            window_3.openMode = Windows.WindowOpenMode.FixWindow;;
            window_3.isExternalWindow = false;

            List<Windows> windows = new List<Windows>();
            windows.Add(window_1);
            windows.Add(window_2);
            windows.Add(window_3);


            //act
            Windows aimWindow = assistantFunctions.GetOpenableOuterWindow(windows);

            //assert
            Assert.IsNull(aimWindow);

        }

        [TestMethod]
        public void test_WindowsIsNull()
        {
           
            //act
            Windows aimWindow = assistantFunctions.GetOpenableOuterWindow(null);

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
    public class GB50016_2014_8_5_1_Test
    {

    }


}

