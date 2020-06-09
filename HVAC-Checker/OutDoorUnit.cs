using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HVAC_CheckEngine
{
    public class OutDoorUnit:UnitAircondition
    {

        public OutDoorUnit(long id) : base(id)
        {

        }
        public override string ToString()
        {
            return "OutDoorUnits";
        }

        public double? IPLV { get; set; }

        public override void setParameter(SQLiteDataReader readerOutDoorUnit)
        {
            base.setParameter(readerOutDoorUnit);

            revitId = Convert.ToInt64(readerOutDoorUnit["extendProperty"].ToString());
            capacity = Convert.ToDouble(readerOutDoorUnit["CoolingCapacity"].ToString());
            coolingType = readerOutDoorUnit["CoolingType"].ToString();
            EER = Convert.ToDouble(readerOutDoorUnit["EER"].ToString());
            IPLV = Convert.ToDouble(readerOutDoorUnit["IPLV"].ToString());
            m_iStoryNo = Convert.ToInt32(readerOutDoorUnit["StoreyNo"].ToString());
        }
    }
}
