using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HVAC_Checker
{
    class Room
    {
        public
            long m_longId;
            string m_strType;
            string m_strName;
            double m_dArea;
            RoomPosition m_eRoomPosition;
        public string boundaryLoops { get; set; }
        public string GetName()
        {
            return m_strName;
        }
        public void SetName(string name)
        {
            m_strName = name;
        }

        public long GetId()
        {
            return m_longId;
        }
        public void SetId(long id)
        {
            m_longId = id;
        }

    }
}
