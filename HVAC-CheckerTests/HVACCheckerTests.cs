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

        [TestMethod()]
        public void GB50736_2012_6_6_7Test()
        {
            string strArchPath = "D://Users//zheny//Source//Repos//HVAC-Checker//HVAC-Checker//6.2.2-ARCH.GDB";
            string strHVACPath = "D://Users//zheny//Source//Repos//HVAC-Checker//HVAC-Checker//6.2.2-HVAC.GDB";
            HVACFunction hvacFunction = new HVACFunction(strArchPath, strHVACPath);
            BimReview result = new BimReview();
            result = HVACChecker.GB50736_2012_6_6_7();

            //assert
            // Assert.AreEqual(comment, result.comment);
            Assert.IsFalse(result.isPassCheck);
        }

        [TestMethod()]
        public void GB50736_2012_9_1_5Test()
        {
            string strArchPath = "D://Users//zheny//Source//Repos//HVAC-Checker//HVAC-Checker//6.2.2-ARCH.GDB";
            string strHVACPath = "D://Users//zheny//Source//Repos//HVAC-Checker//HVAC-Checker//6.2.2-HVAC.GDB";
            HVACFunction hvacFunction = new HVACFunction(strArchPath, strHVACPath);
            BimReview result = new BimReview();
            result = HVACChecker.GB50736_2012_9_1_5();

            //assert
            // Assert.AreEqual(comment, result.comment);
            Assert.IsFalse(result.isPassCheck);
        }

        [TestMethod()]
        public void GB50189_2015_4_5_2Test()
        {
            string strArchPath = "D://Users//zheny//Source//Repos//HVAC-Checker//HVAC-Checker//6.2.2-ARCH.GDB";
            string strHVACPath = "D://Users//zheny//Source//Repos//HVAC-Checker//HVAC-Checker//6.2.2-HVAC.GDB";
            HVACFunction hvacFunction = new HVACFunction(strArchPath, strHVACPath);
            BimReview result = new BimReview();
            result = HVACChecker.GB50189_2015_4_5_2();

            //assert
            // Assert.AreEqual(comment, result.comment);
            Assert.IsFalse(result.isPassCheck);
        }

        [TestMethod()]
        public void GB50016_2014_9_3_16Test()
        {
            string strArchPath = "D://Users//zheny//Source//Repos//HVAC-Checker//HVAC-Checker//6.2.2-ARCH.GDB";
            string strHVACPath = "D://Users//zheny//Source//Repos//HVAC-Checker//HVAC-Checker//6.2.2-HVAC.GDB";
            HVACFunction hvacFunction = new HVACFunction(strArchPath, strHVACPath);
            BimReview result = new BimReview();
            result = HVACChecker.GB50016_2014_9_3_16();

            //assert
            // Assert.AreEqual(comment, result.comment);
            Assert.IsFalse(result.isPassCheck);
        }

        [TestMethod()]
        public void GB51251_2017_3_2_2Test()
        {
            string strArchPath = "D://Users//zheny//Source//Repos//HVAC-Checker//HVAC-Checker//6.2.2-ARCH.GDB";
            string strHVACPath = "D://Users//zheny//Source//Repos//HVAC-Checker//HVAC-Checker//6.2.2-HVAC.GDB";
            HVACFunction hvacFunction = new HVACFunction(strArchPath, strHVACPath);
            BimReview result = new BimReview();
            result = HVACChecker.GB51251_2017_3_2_2();

            //assert
            // Assert.AreEqual(comment, result.comment);
            Assert.IsFalse(result.isPassCheck);
        }

        [TestMethod()]
        public void GB51251_2017_3_2_3Test()
        {
            string strArchPath = "D://Users//zheny//Source//Repos//HVAC-Checker//HVAC-Checker//6.2.2-ARCH.GDB";
            string strHVACPath = "D://Users//zheny//Source//Repos//HVAC-Checker//HVAC-Checker//6.2.2-HVAC.GDB";
            HVACFunction hvacFunction = new HVACFunction(strArchPath, strHVACPath);
            BimReview result = new BimReview();
            result = HVACChecker.GB51251_2017_3_2_3();

            //assert
            // Assert.AreEqual(comment, result.comment);
            Assert.IsFalse(result.isPassCheck);
        }

        [TestMethod()]
        public void GB51251_2017_3_3_7Test()
        {
            string strArchPath = "D://Users//zheny//Source//Repos//HVAC-Checker//HVAC-Checker//6.2.2-ARCH.GDB";
            string strHVACPath = "D://Users//zheny//Source//Repos//HVAC-Checker//HVAC-Checker//6.2.2-HVAC.GDB";
            HVACFunction hvacFunction = new HVACFunction(strArchPath, strHVACPath);
            BimReview result = new BimReview();
            result = HVACChecker.GB51251_2017_3_3_7();

            //assert
            // Assert.AreEqual(comment, result.comment);
            Assert.IsFalse(result.isPassCheck);
        }
    }
}