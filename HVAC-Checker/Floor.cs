using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HVAC_CheckEngine
{
    class Floor
    {
        public Floor(long id)
        {
            Id = id;
        }
        public string storeyName { get; set; } = null;
        public double? elevation { get; set; } = null;
        public double? height { get; set; } = null;

        public int? FloorNumber { get; set; } = null;
        public long? Id { get; set; } = null;
    }
}
