﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;

namespace HVAC_CheckEngine
{
    public class Element
    {
        public Element(long id)
        {
            Id = id;
        }

        public long? Id { get; set; } = null;

        public long? revitId { get; set; } = null;
        public int? m_iStoryNo { get; set; } = null;

        public virtual void setParameter(SQLiteDataReader reader) 
        {
            Id= Convert.ToInt64(reader["Id"].ToString());
        }
    }
}
