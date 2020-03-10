using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HVAC_CheckEngine
{
   public class Room 
    {
        public Room(long id)
        {
            Id = id;
        }
        public string type { get; set; } = null;
        public string name { get; set; } = null;
        public string boundaryLoops { get; set; } = null;
        
        public double? area { get; set; } = null;
        public RoomPosition? roomPosition { get; set; } = null;
        public int ? storyNo { get; set; } = null;
        public long? Id { get; } = null;
    }


    public class Door
    {
        public Door(long id)
        {
            Id = id;
        }
        public long? Id { get; set; } = null;
    }
}
