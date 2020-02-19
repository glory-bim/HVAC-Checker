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
        string type;
        string m_name;
        double area;
        RoomPosition roomPosition;

        public string GetName()
        {
            return m_name;
        }
        public void SetName(string Name)
        {
            m_name = Name;
        }

    }
}
