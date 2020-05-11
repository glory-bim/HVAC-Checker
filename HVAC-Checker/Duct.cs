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
            ptStart = new PointInt(0,0,0);
            ptEnd = new PointInt(0, 0, 0);
        }
        public double? airVelocity { get; set; } = null;

        public PointInt ptStart { get; set; } = null;

        public PointInt ptEnd { get; set; } = null;

        public string systemType { get; set; } = null;

        public double? StartElevation { get; set; } = null;

        public double? EndElevation { get; set; } = null;

        public bool? isVertical { get; set; } = null;
    }
}
