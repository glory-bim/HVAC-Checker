﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HVAC_CheckEngine
{
    public class Element
    {
        public Element(long id)
        {
            Id = id;
        }

        public long? Id { get; } = null;

        public int? storyNo { get; set; } = null;
    }
}