using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HVAC_CheckEngine
{
    class Floor
    {

        private string m_storeyName;
        public string GetStoreyName()
        {
            return m_storeyName;
        }
        public void SetStoreyName(string storeyName)
        {
            m_storeyName = storeyName;
        }
    }
}
