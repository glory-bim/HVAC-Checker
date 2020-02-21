using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HVAC_Checker
{
    class Fan
    {
        public
         long m_longId;
       
     

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
