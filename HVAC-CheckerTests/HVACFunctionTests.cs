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
        public void GetRoomsTest()
        {
            string type = "房间";
            string name = "CH/PH Stock 1";
            double area = 10.0;
            RoomPosition roomPosition = RoomPosition.overground;
            HVACFunction.GetRooms(type, name, area, roomPosition);
             
        }

        [TestMethod()]
        public void GetRoomContainAirTerminalTest()
        {
            Assert.Fail();
        }
    }
}