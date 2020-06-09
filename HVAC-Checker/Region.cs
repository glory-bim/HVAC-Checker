using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HVAC_CheckEngine
{
    public  class Region
    {
 
        public Region()
        {
            rooms = new List<Room>();
        }
        public List<Room> rooms{ get; set; } 
    }
}
