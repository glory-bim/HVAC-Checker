using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HVAC_CheckEngine
{
    public class FireCompartment : Room
    {
        public FireCompartment(long id):base(id)
        {

        }

        public override void setParameter(SQLiteDataReader reader)
        {
            base.setParameter(reader);
            boundaryLoops = reader["boundaryLoops"].ToString();
            if (!System.IO.File.Exists(HVACFunction.m_archXdbPath))
                return;
            revitId = Id;
            //创建一个连接
            string connectionstr = @"data source =" + HVACFunction.m_archXdbPath;
            SQLiteConnection m_dbConnection = new SQLiteConnection(connectionstr);
            m_dbConnection.Open();
            string sql = "select * from Storeys where  Id =  ";
            sql = sql + reader["storeyId"].ToString();
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            SQLiteDataReader readerStorey = command.ExecuteReader();

            if (readerStorey.Read())
            {
                m_iStoryNo = Convert.ToInt32(readerStorey["storeyNo"].ToString());
                m_dHeight = Convert.ToDouble(readerStorey["height"].ToString());
            }
            m_dbConnection.Close();
        }
    }
}
