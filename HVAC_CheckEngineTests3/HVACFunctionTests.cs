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
        public void GetRoomsMoreThanTest()
        {
            string strArchPath = "D://Users//zheny//Source//Repos//HVAC-Checker//HVAC-Checker//建筑.GDB";
            HVACFunction.m_archXdbPath = strArchPath;
            Assert.IsTrue(HVACFunction.GetRoomsMoreThan(10.0).Count() > 0);
        }
    }
}