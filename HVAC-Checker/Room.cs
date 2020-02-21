﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HVAC_CheckEngine
{
    class Room
    {
        public Room(long id)
        {
            Id = id;
        }
        public string type { get; set; } = null;
        public string name { get; set; } = null;
        public double? area { get; set; } = null;
        public RoomPosition? roomPosition { get; set; } = null;
        public long? Id { get; } = null;
    }
}
