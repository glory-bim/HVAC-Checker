using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HVAC_CheckEngine
{
   public class Room :Element
    {
        public Room(long id):base(id)
        {
        }
        public string type { get; set; } = null;
        public string name { get; set; } = null;
        public string boundaryLoops { get; set; } = null;

        public int? m_iNumberOfPeople { get; set; } = null;
        
        public double? m_dArea { get; set; } = null;
        public RoomPosition? m_eRoomPosition { get; set; } = null;
      
        public double? m_dVolume { get; set; } = null;

        public double? m_dHeight { get; set; } = null;
        public double? m_dMaxlength { get; set; } = null;

        public double? m_dWidth { get; set; } = null;
        
    }


    //public class Door
    //{
    //    public Door(long id)
    //    {
    //        Id = id;
    //    }
    //    public long? Id { get; set; } = null;
    //}
}
