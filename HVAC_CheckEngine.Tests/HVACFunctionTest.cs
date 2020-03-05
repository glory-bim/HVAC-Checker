// <copyright file="HVACFunctionTest.cs">Copyright ©  2020</copyright>
using System;
using System.Collections.Generic;
using HVAC_CheckEngine;
using Microsoft.Pex.Framework;
using Microsoft.Pex.Framework.Validation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HVAC_CheckEngine.Tests
{
    /// <summary>此类包含 HVACFunction 的参数化单元测试</summary>
    [PexClass(typeof(HVACFunction))]
    [PexAllowedExceptionFromTypeUnderTest(typeof(InvalidOperationException))]
    [PexAllowedExceptionFromTypeUnderTest(typeof(ArgumentException), AcceptExceptionSubtypes = true)]
    [TestClass]
    public partial class HVACFunctionTest
    {
        /// <summary>测试 GetRoomsMoreThan(Double) 的存根</summary>
        [PexMethod]
        public List<Room> GetRoomsMoreThanTest(double dLength)
        {
            List<Room> result = HVACFunction.GetRoomsMoreThan(dLength);

            // TODO: 将断言添加到 方法 HVACFunctionTest.GetRoomsMoreThanTest(Double)
            string strArchPath = "D://Users//zheny//Source//Repos//HVAC-Checker//HVAC-Checker//建筑.GDB";
            HVACFunction.m_archXdbPath = strArchPath;
            Assert.IsTrue(true);
            return result;
        }
    }
}
