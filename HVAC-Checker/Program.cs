using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.IO;


namespace HVAC_CheckEngine
{
    class Program
    {
        static void Main(string[] args)
        {
            string strArchPath = @"C:\Users\wangt\Desktop\测试XDB\测试.GDB";
            string strHVACPath = @"C:\Users\wangt\Desktop\测试XDB\机电.GDB";
            HVACFunction hvacFunction = new HVACFunction(strArchPath, strHVACPath);
            globalData.buildingHeight = 50;
            globalData.buildingType = "公共建筑";
            BimReview result = HVACChecker.GB50016_2014_8_5_3();
        }
    }          
}
