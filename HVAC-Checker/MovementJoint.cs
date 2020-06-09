using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HVAC_CheckEngine
{
  

    public class MovementJoint : Element
    {
        public MovementJoint(long id) : base(id)
        {
        }
       
        public string boundaryLoops { get; set; } = null;

        public override void setParameter(SQLiteDataReader reader)
        {
            base.setParameter(reader);
            boundaryLoops = reader["extendProperty"].ToString();
            revitId = Id;

            if (!System.IO.File.Exists(HVACFunction.m_archXdbPath))
                return;
            
            //创建一个连接
            string connectionstr = @"data source =" + HVACFunction.m_archXdbPath;
            SQLiteConnection m_dbConnection = new SQLiteConnection(connectionstr);
            m_dbConnection.Open();

            
            string sql = "select * from Storeys where  Id =  ";
            sql += reader["storeyId"].ToString();
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            SQLiteDataReader readerStorey = command.ExecuteReader();
            
            if (readerStorey.Read())
            {
                m_iStoryNo = Convert.ToInt32(readerStorey["storeyNo"].ToString());

            }
            m_dbConnection.Close();
        }

    }
}
