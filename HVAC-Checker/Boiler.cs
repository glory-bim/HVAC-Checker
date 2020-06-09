using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HVAC_CheckEngine
{
    public class Boiler : Element
    {
        public Boiler(long id) : base(id)
        {

        }

        public double thermalPower { get; set; }

        public double ThermalEfficiency { get; set; }

        public double evaporationCapacity { get; set; }

        public string type { get; set; }

        public string fuelType { get; set; }

        public string mediaType { get; set; }

        public override string ToString()
        {
            return "Boilers";
        }

        public override void setParameter(SQLiteDataReader readerBoiler)
        {
            base.setParameter(readerBoiler);

            revitId = Convert.ToInt64(readerBoiler["extendProperty"].ToString());
            type = readerBoiler["BoilerType"].ToString();
            thermalPower = Convert.ToDouble(readerBoiler["ThermalPower"].ToString());
            ThermalEfficiency = Convert.ToDouble(readerBoiler["ThermalEfficiency"].ToString());
            mediaType = readerBoiler["HeatMediumType"].ToString();
            fuelType = readerBoiler["FuelType"].ToString();
            evaporationCapacity = Convert.ToDouble(readerBoiler["Evaporation"].ToString());
            m_iStoryNo = Convert.ToInt32(readerBoiler["StoreyNo"].ToString());
        }
    }
}
