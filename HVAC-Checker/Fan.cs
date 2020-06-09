using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HVAC_CheckEngine
{
    public class Fan:Element
    {
        public Fan(long id):base(id)
        {

        }
        public double? m_flowRate { get; set; } = null;
        public string type { get; set; } = null;

        public override void setParameter(SQLiteDataReader reader)
        {
            base.setParameter(reader);

            m_iStoryNo = Convert.ToInt32(reader["StoreyNo"].ToString());
            revitId = Convert.ToInt64(reader["extendProperty"].ToString());
        }
        public override string ToString()
        {
            return "HVACFans";
        }
    }
}
