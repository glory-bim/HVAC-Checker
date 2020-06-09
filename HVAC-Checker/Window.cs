using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HVAC_CheckEngine
{
    public class Window:Element
    {
        public Window(long id):base(id)
        {
        }
        public bool? isExternalWindow { get; set; } = null;//是否为外窗

        public bool? isSmokeExhaustWindow { get; set; } = null;//是否为排烟窗
        public double? area { get; set; } = null;

        public double? effectiveArea //有效面积在这里计算
        {
            get;set;
        }

       public WindowOpenMode? openMode { get; set; } = null;

        double? openingAngle { get; set; } = null;//开启角度

        public string sFaceOrient { get; set; } = null;

        public enum WindowOpenMode{HangWindow,SashWindow,BlindWindow,CasementWindow,PushWindow,FixWindow }//悬窗，推拉窗，百叶窗，平开窗，平推窗，固定窗

        public override void setParameter(SQLiteDataReader readerWindows)
        {
            base.setParameter(readerWindows);
            revitId = Id;
            isExternalWindow = Convert.ToBoolean(readerWindows["IsOutsideComponent"].ToString());
            area = Convert.ToDouble(readerWindows["Area"].ToString());
            effectiveArea = Convert.ToDouble(readerWindows["EffectiveArea"].ToString());
            sFaceOrient = readerWindows["sFacingOrientation"].ToString();
            isExternalWindow = Convert.ToBoolean(readerWindows["IsOutsideComponent"].ToString());

            if (!System.IO.File.Exists(HVACFunction. m_archXdbPath))
                return;

            //创建一个连接
            string connectionstr = @"data source =" + HVACFunction.m_archXdbPath;
            SQLiteConnection dbConnection = new SQLiteConnection(connectionstr);
            dbConnection.Open();

            string sql = "select * from Storeys where  Id =  ";
            sql = sql + readerWindows["storeyId"].ToString();
            SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
            SQLiteDataReader readerStorey = command.ExecuteReader();

            if (readerStorey.Read())
            {
               m_iStoryNo = Convert.ToInt32(readerStorey["storeyNo"].ToString());
            }

            
        }
    }
}
