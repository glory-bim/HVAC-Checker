using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HVAC_CheckEngine
{
    public class Chiller:Element
    {
        public Chiller(long id) : base(id)
        {

        }
        public override string ToString()
        {
            return "Chillers";
        }

        public string type { get; set; }

        public string coolingType { get; set;}

        public double? capacity { get; set; }

        public double? COP { get; set; }

        public bool? isFrequencyConversion { get; set; }

        public override void setParameter(SQLiteDataReader readerChiller)
        {
            base.setParameter(readerChiller);

            revitId = Convert.ToInt64(readerChiller["extendProperty"].ToString());
            capacity = Convert.ToDouble(readerChiller["CoolingCapacity"].ToString());
            coolingType = readerChiller["CoolingType"].ToString();
            COP = Convert.ToDouble(readerChiller["COP"].ToString());
            isFrequencyConversion = Convert.ToBoolean(readerChiller["IfFrequencyConversion"]);
            type = readerChiller["ChillerType"].ToString();
            m_iStoryNo = Convert.ToInt32(readerChiller["StoreyNo"].ToString());
        }

    }
}
