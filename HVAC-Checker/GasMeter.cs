﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HVAC_CheckEngine
{
  
    public class GasMeter : Element
    {
        public GasMeter(long id) : base(id)
        {

        }

        public override string ToString()
        {
            return "GasMeters";
        }

    }
}
