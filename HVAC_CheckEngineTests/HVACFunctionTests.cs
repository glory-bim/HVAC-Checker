using Microsoft.VisualStudio.TestTools.UnitTesting;
using HVAC_CheckEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HVAC_CheckEngine.Tests
{
    [TestClass()]
    public class HVACFunctionTests
    {
        [TestMethod()]
        public void HVACFunctionTest()
        {
            HVACFunction hvacFunction = new HVACFunction(" / 建筑.GDB", "/机电.GDB");
            Assert.IsNotNull(hvacFunction);
        }

        [TestMethod()]
        public void GetCurrentPathTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetRoomsTest()
        {
            string type = "间";
            string name = "CH/PH Stock 1";
            double area = 10.0;
            RoomPosition roomPosition = RoomPosition.overground;

            string strArchPath = "D://Users//zheny//Source//Repos//HVAC-Checker//HVAC-Checker//建筑.GDB";
            string strHVACPath = "D://Users//zheny//Source//Repos//HVAC-Checker//HVAC-Checker//机电.GDB";
            HVACFunction hvacFunction = new HVACFunction(strArchPath, strHVACPath);
            HVACFunction.GetRooms(type, name, area, roomPosition);
        }

        [TestMethod()]
        public void GetRoomContainAirTerminalTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetWindowsInRoomTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void ConvertJsonToBoundaryLoopsTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetSpaceBBoxTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetSpaceOBBTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void getEllipsePointTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetFanConnectingAirterminalTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void FindFansByAirterminalTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void FindFansByDuctTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void FindFansByDuct3tTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void FindFansByDuct4tTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void FindFansByDuctDampersTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetOutletsOfFanTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void FindOutLetsTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void FindOutletsByDuctTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void FindOutletsByDuct3tTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void FindOutletsByDuct4tTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void FindOutletsByDuctDampersTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetRoomsMoreThanTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetInletsOfFanTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void FindInletsTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void FindInletsByDuctTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void FindInletsByDuct3tTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void FindInletsByDuct4tTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void FindInletsByDuctDampersTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetDuctsCrossSpaceTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetDuctsCrossFireDistrictTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetDuctsCrossFireSideTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetRoomOfAirterminalTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetAreaTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetRoomsTest1()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetRoomsContainingStringTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void isOvergroundRoomTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetFloorsTest()
        {
            string strArchPath = "D://Users//zheny//Source//Repos//HVAC-Checker//HVAC-Checker//建筑.GDB";
            string strHVACPath = "D://Users//zheny//Source//Repos//HVAC-Checker//HVAC-Checker//6.2.2-HVAC.GDB";
            HVACFunction hvacFunction = new HVACFunction(strArchPath, strHVACPath);

            List<Floor> floors = HVACFunction.GetFloors();
            Assert.IsTrue(floors.Count > 0);
        }

        [TestMethod()]
        public void FindDuctsTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void FindDuctsByDuct3tTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void FindDuctsByDuct4tTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void FindDuctsByDuctDuctDampersTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetDuctsOfFanTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void isAllBranchLinkingAirTerminalTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void IfFindAirTerminalTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void IfFindAirterminalByDuctTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void IfFindAirterminalByDuct3tTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void IfFindAirterminalByDuct4tTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void IfFindAirterminalByDuctDampersTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetFireDistrictLengthTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetConnectedRegionTest()
        {
            Assert.Fail();
        }
    }
}