using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HVAC_CheckEngine
{
    public class RoofTopAHU:UnitAircondition
    {
        public RoofTopAHU(long id) : base(id)
        {

        }
        public override string ToString()
        {
            return "RoofTopAHUs";
        }

        public override void setParameter(SQLiteDataReader readerRoofTopAHU)
        {
            base.setParameter(readerRoofTopAHU);

            revitId = Convert.ToInt64(readerRoofTopAHU["extendProperty"].ToString());
            capacity = Convert.ToDouble(readerRoofTopAHU["CoolingCapacity"].ToString());
            coolingType = readerRoofTopAHU["CoolingType"].ToString();
            EER = Convert.ToDouble(readerRoofTopAHU["EER"].ToString());
            m_iStoryNo = Convert.ToInt32(readerRoofTopAHU["StoreyNo"].ToString());
        }
    }
}
