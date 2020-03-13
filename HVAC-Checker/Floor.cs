using System;
using System.Collections.Generic;
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
        public string storeyName { get; set; } = null;
        public double? elevation { get; set; } = null;
        public double? height { get; set; } = null;

        public int? FloorNumber { get; set; } = null;
       
    }
}
