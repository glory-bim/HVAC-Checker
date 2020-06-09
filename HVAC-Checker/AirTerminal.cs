using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HVAC_CheckEngine
{
    public class AirTerminal:Element
    {
        public string systemType { get; set; }= null;
        public AirTerminal(long id):base(id)
        {
            
        }
        public double? airVelocity { get; set; } = null;

        public double? elevation { get; set; } = null;

        public double? airFlowRate { get; set; } = null;

        public double? height { get; set; } = null;

        public double? width { get; set; } = null;

        public double? ventilationEfficiency { get; set; } = null;

        public override void setParameter(SQLiteDataReader readerAirTerminals)
        {
            base.setParameter(readerAirTerminals);
            revitId = Convert.ToInt64(readerAirTerminals["extendProperty"].ToString());
            airVelocity = Convert.ToDouble(readerAirTerminals["AirVelocity"].ToString());
            systemType = readerAirTerminals["SystemName"].ToString();
            width = Convert.ToDouble(readerAirTerminals["AirTerminalWidth"].ToString());
            height = Convert.ToDouble(readerAirTerminals["AirTerminalHeight"].ToString());
            airFlowRate = Convert.ToDouble(readerAirTerminals["AirFlowRate"].ToString());
            ventilationEfficiency = Convert.ToDouble(readerAirTerminals["VentilationRate"].ToString());
            m_iStoryNo = Convert.ToInt32(readerAirTerminals["StoreyNo"].ToString());
        }

    }
}
