using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HVAC_CheckEngine
{
    public class Floor:Element
    {
        public Floor(long id):base(id)
        {
            
        }
        public double? elevation { get; set; } = null;
        public double? height { get; set; } = null;

        public override void setParameter(SQLiteDataReader readerFloor)
        {
            base.setParameter(readerFloor);

            m_iStoryNo = Convert.ToInt32(readerFloor["storeyNo"].ToString());
            elevation = Convert.ToDouble(readerFloor["elevation"].ToString());
            height = Convert.ToDouble(readerFloor["height"].ToString());
        }

    }
}
