using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HVAC_CheckEngine
{
    public class AbsorptionChiller : Element
    {
        public AbsorptionChiller(long id) : base(id)
        {

        }
        public override string ToString()
        {
            return "AbsorptionChillers";
        }

        public double? coolingCoefficient { get; set; }

        public double? heatingCoefficient { get; set; }

        public override void setParameter(SQLiteDataReader readerAbsorptionChiller)
        {
            base.setParameter(readerAbsorptionChiller);
            revitId = Convert.ToInt64(readerAbsorptionChiller["extendProperty"].ToString());
            coolingCoefficient = Convert.ToDouble(readerAbsorptionChiller["PerformanceRate"].ToString());
            heatingCoefficient = Convert.ToDouble(readerAbsorptionChiller["HeatingPerformanceRate"].ToString());
           m_iStoryNo = Convert.ToInt32(readerAbsorptionChiller["StoreyNo"].ToString());
        }

    }
}
