using Microsoft.VisualStudio.TestTools.UnitTesting;
using HVAC_CheckEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using System.IO;




namespace HVAC_CheckEngine.Tests
{
    [TestClass()]
    public class HVACFunctionTests
    {
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
            Room room = new Room(362159);
            string strArchPath = "D://Users//zheny//Source//Repos//HVAC-Checker//HVAC-Checker//6.2.2-ARCH.GDB";
            string strHVACPath = "D://Users//zheny//Source//Repos//HVAC-Checker//HVAC-Checker//6.2.2-HVAC.GDB";
            HVACFunction hvacFunction = new HVACFunction(strArchPath, strHVACPath);
            List<AirTerminal> airTerminals = HVACFunction.GetRoomContainAirTerminal(room);
            Assert.IsTrue(airTerminals.Count() > 0);
        }

        [TestMethod()]
        public void HVACFunctionTest()
        {
            HVACFunction hvacFunction = new HVACFunction(" / 建筑.GDB", "/机电.GDB");
            Assert.IsNotNull(hvacFunction);
        }

        [TestMethod()]
        public void GetWindowsInRoomTest()
        {
            string strArchPath = "D://Users//zheny//Source//Repos//HVAC-Checker//HVAC-Checker//8.1.3.GDB";
            HVACFunction.m_archXdbPath = strArchPath;
            Room room = new Room(362135);
            int iCount = HVACFunction.GetWindowsInRoom(room).Count();
            Assert.IsTrue(HVACFunction.GetWindowsInRoom(room).Count() > 0);
        }

        [TestMethod()]
        public void GetFanConnectingAirterminalTest()
        {
            string strhvacXdbPath = "D://Users//zheny//Source//Repos//HVAC-Checker//HVAC-Checker//测试hvac.GDB";
            HVACFunction.m_hvacXdbPath = strhvacXdbPath;
            AirTerminal airTerminal = new AirTerminal(1230487613874372609);
            Assert.IsTrue(HVACFunction.GetFanConnectingAirterminal(airTerminal).Count() > 0);
        }

        [TestMethod()]
        public void GetOutLetsOfFanTest()
        {
            string strhvacXdbPath = "D://Users//zheny//Source//Repos//HVAC-Checker//HVAC-Checker//测试hvac.GDB";
            HVACFunction.m_hvacXdbPath = strhvacXdbPath;
            Fan fan = new Fan(1230487612968402944);
            Assert.IsTrue(HVACFunction.GetOutletsOfFan(fan).Count() > 0);
            //Assert.IsTrue(false);
        }

        [TestMethod()]
        public void GetRoomsMoreThanTest()
        {
            string strArchPath = "D://Users//zheny//Source//Repos//HVAC-Checker//HVAC-Checker//建筑.GDB";
            HVACFunction.m_archXdbPath = strArchPath;
            Assert.IsTrue(HVACFunction.GetRoomsMoreThan("走廊", 10.0).Count() > 0);
        }

        [TestMethod()]
        public void GetInletOfFanTest()
        {
            string strhvacXdbPath = "D://Users//zheny//Source//Repos//HVAC-Checker//HVAC-Checker//测试hvac.GDB";
            HVACFunction.m_hvacXdbPath = strhvacXdbPath;
            Fan fan = new Fan(1230487612968402944);
            Assert.IsTrue(HVACFunction.GetInletsOfFan(fan).Count() > 0);
            //Assert.IsTrue(false);
        }

        [TestMethod()]
        public void GetDuctsCrossSpaceTest()
        {
            string strArchPath = "D://Users//zheny//Source//Repos//HVAC-Checker//HVAC-Checker//建筑.GDB";
            string strHVACPath = "D://Users//zheny//Source//Repos//HVAC-Checker//HVAC-Checker//机电.GDB";
            HVACFunction hvacFunction = new HVACFunction(strArchPath, strHVACPath);
            Room room = new Room(573789);
            Assert.IsTrue(HVACFunction.GetDuctsCrossSpace(room).Count() > 0);
        }

        [TestMethod()]
        public void GetDuctsCrossFireDistrictTest()
        {
            string strArchPath = "D://Users//zheny//Source//Repos//HVAC-Checker//HVAC-Checker//建筑.GDB";
            string strHVACPath = "D://Users//zheny//Source//Repos//HVAC-Checker//HVAC-Checker//机电.GDB";
            HVACFunction hvacFunction = new HVACFunction(strArchPath, strHVACPath);
            FireCompartment fireDistrict = new FireCompartment(573789);
            Assert.IsTrue(HVACFunction.GetDuctsCrossFireDistrict(fireDistrict).Count() > 0);
        }

        //    [TestMethod()]
        //    public void GetDuctsCrossFireSideTest()
        //    {
        //        Assert.Fail();
        //    }

        [TestMethod()]
        public void GetRoomOfAirterminalTest()
        {
            string strArchPath = "D://Users//zheny//Source//Repos//HVAC-Checker//HVAC-Checker//6.2.2-ARCH.GDB";
            string strHVACPath = "D://Users//zheny//Source//Repos//HVAC-Checker//HVAC-Checker//6.2.2-HVAC.GDB";
            HVACFunction hvacFunction = new HVACFunction(strArchPath, strHVACPath);
            AirTerminal airterminal = new AirTerminal(1245195412197867521);
            Assert.IsTrue(HVACFunction.GetRoomOfAirterminal(airterminal).Id == 362156);
        }

        [TestMethod()]
        public void GetAreaTest()
        {
            Window window = new Window(322);
            string strArchPath = "D://Users//zheny//Source//Repos//HVAC-Checker//HVAC-Checker//建筑.GDB";
            HVACFunction.m_archXdbPath = strArchPath;
            Assert.IsTrue(HVACFunction.GetArea(window) > 0);
        }

        [TestMethod()]
        public void GetRoomsTest1()
        {
            string strArchPath = "D://Users//zheny//Source//Repos//HVAC-Checker//HVAC-Checker//建筑.GDB";
            HVACFunction.m_archXdbPath = strArchPath;
            Assert.IsTrue(HVACFunction.GetRooms("间").Count() > 0);
        }

        [TestMethod()]
        public void GetRoomsContainingStringTest()
        {
            string strArchPath = "D://Users//zheny//Source//Repos//HVAC-Checker//HVAC-Checker//建筑.GDB";
            HVACFunction.m_archXdbPath = strArchPath;
            Assert.IsTrue(HVACFunction.GetRoomsContainingString("CH").Count() > 0);
        }

        [TestMethod()]
        public void isOvergroundRoomTest()
        {
            Room room = new Room(573789);
            string strArchPath = "D://Users//zheny//Source//Repos//HVAC-Checker//HVAC-Checker//建筑.GDB";
            HVACFunction.m_archXdbPath = strArchPath;
            Assert.IsTrue(HVACFunction.isOvergroundRoom(room));
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
        public void GetDuctsOfFanTest()
        {
            Fan fan = new Fan(1244249984593821696);
            string strArchPath = "D://Users//zheny//Source//Repos//HVAC-Checker//HVAC-Checker//6.2.2-arch.GDB";
            string strHVACPath = "D://Users//zheny//Source//Repos//HVAC-Checker//HVAC-Checker//6.2.2-HVAC.GDB";
            HVACFunction hvacFunction = new HVACFunction(strArchPath, strHVACPath);
            List<Duct> ducts = HVACFunction.GetDuctsOfFan(fan);
            Assert.IsTrue(ducts.Count() > 0);
        }

        [TestMethod()]
        public void isAllBranchLinkingAirTerminalTest()
        {
            string strhvacXdbPath = "D://Users//zheny//Source//Repos//HVAC-Checker//HVAC-Checker//测试hvac.GDB";
            HVACFunction.m_hvacXdbPath = strhvacXdbPath;
            Fan fan = new Fan(1230487612968402944);
            Assert.IsTrue(HVACFunction.isAllBranchLinkingAirTerminal(fan));
        }

        [TestMethod()]
        public void GetConnectedRegionTest()
        {
            HVACFunction.m_archXdbPath = "D://Users//zheny//Source//Repos//HVAC-Checker//HVAC-Checker//建筑.GDB";
            List<Region> regions = new List<Region>();
            regions = HVACFunction.GetConnectedRegion();
            Assert.IsTrue(regions.Count() > 0);
        }

        [TestMethod()]
        public void GetFireDistrictLengthTest()
        {
            FireCompartment fireDis = new FireCompartment(573789);
            double dLength = 0.0;
            HVACFunction.m_archXdbPath = "D://Users//zheny//Source//Repos//HVAC-Checker//HVAC-Checker//建筑.GDB";
            dLength = HVACFunction.GetFireDistrictLength(fireDis);
            Assert.IsTrue(dLength > 0);

        }

        [TestMethod()]
        public void GetAllVerticalDuctConnectedToDuctTest()
        {
            Duct duct = new Duct(1244249984656736257);
            string strArchPath = "D://Users//zheny//Source//Repos//HVAC-Checker//HVAC-Checker//6.2.2-arch.GDB";
            string strHVACPath = "D://Users//zheny//Source//Repos//HVAC-Checker//HVAC-Checker//6.2.2-HVAC.GDB";
            HVACFunction hvacFunction = new HVACFunction(strArchPath, strHVACPath);
            List<Duct> ducts = HVACFunction.GetAllVerticalDuctConnectedToDuct(duct);
            Assert.IsTrue(ducts.Count() > 0);
        }
    }
}