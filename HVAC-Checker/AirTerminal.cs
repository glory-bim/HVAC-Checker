using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HVAC_CheckEngine
{
    class AirTerminal
    {
        public string systemType { get; set; }= null;
        public AirTerminal(long id)
        {
            Id = id;
        }
        public long? Id { get; } = null;
    }
}
