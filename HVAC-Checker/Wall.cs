using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HVAC_CheckEngine
{
  
    public class Wall : Element
    {
        public Wall(long id) : base(id)
        {
        }
      
        public string boundaryLoops { get; set; } = null;

     
    }
}
