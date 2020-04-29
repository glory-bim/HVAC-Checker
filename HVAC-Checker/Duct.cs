using System;
using System.Collections.Generic;
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
            
        }
        public double? airVelocity { get; set; } = null;

        public PointInt ptStart { get; set; } = null;

        public PointInt ptEnd { get; set; } = null;

        public string systemType { get; set; } = null;

    }
}
