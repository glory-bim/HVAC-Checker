using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;

namespace HVAC_CheckEngine
{
  
    public class Wall : Element
    {
        public Wall(long id) : base(id)
        {

        }
      
        public string boundaryLoops { get; set; } = null;
        public bool? isOuterWall { get; set; } = null;

        public double? fireResistanceRating { get; set; } = null;

        public override void setParameter(SQLiteDataReader readerWall)
        {
            base.setParameter(readerWall);
            isOuterWall= Convert.ToBoolean(readerWall["isSideWall"].ToString());
            fireResistanceRating= Convert.ToDouble(readerWall["fireResistanceRating"].ToString());
        }
    }
}
