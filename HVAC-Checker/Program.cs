using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.IO;
using System.Windows.Forms;
using Newtonsoft.Json;


namespace HVAC_CheckEngine
{
    
    delegate BimReview itemChecher();

    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            //try
            //{
                string pathOfBuildingXDB = args[0];
                string pathOfMechanicalXDB = args[1];
                HVACFunction function = new HVACFunction(pathOfBuildingXDB,pathOfMechanicalXDB);
                string result= runChecher();
                Console.WriteLine(result);
                Console.ReadLine();
            //}
            //catch(Exception e)
            //{
            //    Console.WriteLine(e.Message);
            //    Console.ReadLine();
            //}
        }

        static string runChecher()
        {
            CheckResult checkResult = new CheckResult();
            try
            {
                //checkResult.addBimReview(HVACChecker.GB50016_2014_8_1_9());
               // checkResult.addBimReview(HVACChecker.GB50016_2014_8_5_1());
               // checkResult.addBimReview(HVACChecker.GB50016_2014_8_5_2());
                checkResult.addBimReview(HVACChecker.GB50016_2014_8_5_3());
                checkResult.addBimReview(HVACChecker.GB50016_2014_8_5_4());
               // checkResult.addBimReview(HVACChecker.GB50016_2014_9_3_11());
               // checkResult.addBimReview(HVACChecker.GB50016_2014_9_3_16());
               // checkResult.addBimReview(HVACChecker.GB50736_2012_6_3_6());
               // checkResult.addBimReview(HVACChecker.GB50736_2012_6_6_5());
               // checkResult.addBimReview(HVACChecker.GB50736_2012_6_6_7());
              //  checkResult.addBimReview(HVACChecker.GB50736_2012_6_6_13());
               // checkResult.addBimReview(HVACChecker.GB50736_2012_7_4_13());
              //  checkResult.addBimReview(HVACChecker.GB50736_2012_9_1_5());
              //  checkResult.addBimReview(HVACChecker.GB50189_2015_4_2_5());
             //   checkResult.addBimReview(HVACChecker.GB50189_2015_4_2_10());
             //   checkResult.addBimReview(HVACChecker.GB50189_2015_4_2_14());
             //   checkResult.addBimReview(HVACChecker.GB50189_2015_4_2_17());
             //   checkResult.addBimReview(HVACChecker.GB50189_2015_4_2_19());
              //  checkResult.addBimReview(HVACChecker.GB50189_2015_4_5_2());
             //   checkResult.addBimReview(HVACChecker.GB51251_2017_3_1_2());
              //  checkResult.addBimReview(HVACChecker.GB51251_2017_3_1_4());
              //  checkResult.addBimReview(HVACChecker.GB51251_2017_3_1_5());
             //   checkResult.addBimReview(HVACChecker.GB51251_2017_3_2_1());
                checkResult.addBimReview(HVACChecker.GB51251_2017_3_2_2());
             //   checkResult.addBimReview(HVACChecker.GB51251_2017_3_2_3());
             //   checkResult.addBimReview(HVACChecker.GB51251_2017_3_3_1());
             //   checkResult.addBimReview(HVACChecker.GB51251_2017_3_3_7());
             //   checkResult.addBimReview(HVACChecker.GB51251_2017_3_3_11());
             //   checkResult.addBimReview(HVACChecker.GB51251_2017_4_2_4());
             //   checkResult.addBimReview(HVACChecker.GB51251_2017_4_4_1());
             //   checkResult.addBimReview(HVACChecker.GB51251_2017_4_4_2());
             //   checkResult.addBimReview(HVACChecker.GB51251_2017_4_4_7());
             //   checkResult.addBimReview(HVACChecker.GB51251_2017_4_4_10());
                checkResult.addBimReview(HVACChecker.GB51251_2017_4_4_11());
                checkResult.addBimReview(HVACChecker.GB50738_2011_8_4_2());
                //   checkResult.addBimReview(HVACChecker.GB51251_2017_4_5_1());
                //   checkResult.addBimReview(HVACChecker.GB51251_2017_4_5_2());
                //   checkResult.addBimReview(HVACChecker.GB51251_2017_4_5_6());
                //   checkResult.addBimReview(HVACChecker.GB50067_2014_8_2_1());
                //   checkResult.addBimReview(HVACChecker.GB50067_2014_8_2_2());
                //   checkResult.addBimReview(HVACChecker.GB50157_2013_28_4_2());
                //  checkResult.addBimReview(HVACChecker.GB50157_2013_28_4_22());
                //   checkResult.addBimReview(HVACChecker.GB50490_2009_8_4_17());
                //   checkResult.addBimReview(HVACChecker.GB50490_2009_8_4_19());
                //   checkResult.addBimReview(HVACChecker.GB50041_2008_15_3_7());
                checkResult.state = 1;
                checkResult.message = "succeed";
            }
            catch(ArgumentException e)
            {
                checkResult.message = e.Message;
                checkResult.state = 0;
                checkResult.data.Clear();
            }
            return JsonConvert.SerializeObject(checkResult);
        }  
    }          
}
