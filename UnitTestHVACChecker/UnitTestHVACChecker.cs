using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HVAC_CheckEngine;
using System.Collections.Generic;
using Microsoft.QualityTools.Testing.Fakes;
using Microsoft.Office.Interop.Excel;

namespace UnitTestHVACChecker
{
    [TestClass]
    public class GB50016_2014_8_5_1_Test
    {
        [TestMethod]
        [DeploymentItem(@"D:\wangT\HVAC-Checker\UnitTestHVACChecker\测试数据\测试数据.xlsx")]
        [DataSource("MyExcelDataSource2")]
        public void test_Correct()
        {
            
            using (ShimsContext.Create())
            {
                
                HVAC_CheckEngine.Fakes.ShimHVACFunction.GetRoomsString = (string type) =>
                 {
                     ApplicationClass ExcelApp = new ApplicationClass();
                     ExcelApp.Workbooks.Open(@"D:\wangT\HVAC-Checker\UnitTestHVACChecker\测试数据\测试数据.xlsx");
                     ExcelApp.Workbooks
                 }



            }

        }
