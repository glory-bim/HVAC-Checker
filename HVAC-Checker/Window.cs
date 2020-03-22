using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HVAC_CheckEngine
{
    public class Window:Element
    {
        public Window(long id):base(id)
        {
        }
        public bool? isExternalWindow { get; set; } = null;//是否为外窗
        public double? area { get; set; } = null;

        public double? effectiveArea { get; set; } 

    }
}
