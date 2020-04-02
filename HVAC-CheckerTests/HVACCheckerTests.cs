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
    public class HVACCheckerTests
    {
        [TestMethod()]
        public void GB50736_2012_5_9_13Test()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GB50736_2012_6_6_5Test()
        {
            string strArchPath = "D://Users//zheny//Source//Repos//HVAC-Checker//HVAC-Checker//6.2.2-ARCH.GDB";
            string strHVACPath = "D://Users//zheny//Source//Repos//HVAC-Checker//HVAC-Checker//6.2.2-HVAC.GDB";
            HVACFunction hvacFunction = new HVACFunction(strArchPath, strHVACPath);
            BimReview result = new BimReview();
            result = HVACChecker.GB50736_2012_6_6_5();

            //assert
           // Assert.AreEqual(comment, result.comment);
            Assert.IsFalse(result.isPassCheck);
        
         
        }
    }
}