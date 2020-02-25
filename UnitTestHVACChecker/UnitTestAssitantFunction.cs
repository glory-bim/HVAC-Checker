using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HVAC_CheckEngine;
using System.Collections.Generic;

namespace UnitTestHVACChecker
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
           AirTerminal aimAirTerminal= assistantFunctions.getAirTerminalOfCertainSystem(airTerminals, aimSystemType);

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
            AirTerminal aimAirTerminal = assistantFunctions.getAirTerminalOfCertainSystem(airTerminals, aimSystemType);

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
            assistantFunctions.getAirTerminalOfCertainSystem(airTerminals, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void test_getAirTerminalOfCertainSystem_AirTerminalsIsNull()
        {
            //arrange
            List<AirTerminal> airTerminals = new List<AirTerminal>();
            //act
            assistantFunctions.getAirTerminalOfCertainSystem(null, string.Empty);
        }

        private TestContext context;

        public TestContext TestContext
        {
            get { return context; }
            set { context = value; }
        }
    }
}
