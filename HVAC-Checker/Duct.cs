using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BCGL.Sharp;

namespace HVAC_CheckEngine
{
    public class Duct:Element
    {
        public Duct(long id):base(id)
        {
            ptStart = new PointInt(0,0,0);
            ptEnd = new PointInt(0, 0, 0);
        }
        public double? airVelocity { get; set; } = null;

        public PointInt ptStart { get; set; } = null;

        public PointInt ptEnd { get; set; } = null;

        public string systemType { get; set; } = null;

        public double? BottomElevation { get; set; } = null;

        public double? TopElevation { get; set; } = null;

        public bool? isVertical { get; set; } = null;

        public override string ToString()
        {
            return "Ducts";
        }

        public override void setParameter(SQLiteDataReader ductReader)
        {
            base.setParameter(ductReader);

            revitId = Convert.ToInt64(ductReader["extendProperty"].ToString());
            string strVector = ductReader["DuctStartPoint"].ToString();
            int index = strVector.IndexOf(":");
            int index_s = strVector.LastIndexOf(",\"Y");
            string strX = strVector.Substring(index + 1, index_s - index - 1);

            double dX = Convert.ToDouble(strX);
            index = strVector.IndexOf("Y");
            index_s = strVector.LastIndexOf(",\"Z");
            string strY = strVector.Substring(index + 3, index_s - index - 3);
            double dY = Convert.ToDouble(strY);

            index = strVector.IndexOf("Z");

            index_s = strVector.Length;
            string strZ = strVector.Substring(index + 3, index_s - index - 4);
            double dZ = Convert.ToDouble(strZ);


            ptStart.X = Convert.ToInt32(dX);
            ptStart.Y = Convert.ToInt32(dY);
            ptStart.Z = Convert.ToInt32(dZ);


            strVector = ductReader["DuctEndPoint"].ToString();
            index = strVector.IndexOf(":");
            index_s = strVector.LastIndexOf(",\"Y");
            strX = strVector.Substring(index + 1, index_s - index - 1);

            dX = Convert.ToDouble(strX);
            index = strVector.IndexOf("Y");
            index_s = strVector.LastIndexOf(",\"Z");
            strY = strVector.Substring(index + 3, index_s - index - 3);
            dY = Convert.ToDouble(strY);

            index = strVector.IndexOf("Z");

            index_s = strVector.Length;
            strZ = strVector.Substring(index + 3, index_s - index - 4);
            dZ = Convert.ToDouble(strZ);


            ptEnd.X = Convert.ToInt32(dX);
            ptEnd.Y = Convert.ToInt32(dY);
            ptEnd.Z = Convert.ToInt32(dZ);

            airVelocity = Convert.ToDouble(ductReader["Velocity"].ToString());
            systemType = ductReader["SystemName"].ToString();
            BottomElevation = Convert.ToDouble(ductReader["BottomElevation"].ToString());
            TopElevation = Convert.ToDouble(ductReader["TopElevation"].ToString());
            isVertical = Convert.ToBoolean(ductReader["IsVerticalDuct"]);
            m_iStoryNo = Convert.ToInt32(ductReader["StoreyNo"].ToString());
        }
    }
}
