using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HVAC_CheckEngine
{
    class Windows
    {
        long m_id;
        double m_dArea;
        public long GetID()
        {
            return m_id;
        }
        public void SetID(long id)
        {
            m_id = id;
        }


    }
}
