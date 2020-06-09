using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;

namespace HVAC_CheckEngine
{
    public class Door:Element
    {
        public Door(long id):base(id)
        {
            
        }

        public string name { get; set; } = null;
        public override void setParameter(SQLiteDataReader readerDoor)
        {
            base.setParameter(readerDoor);
            name= readerDoor["name"].ToString();

            string connectionstr = @"data source =" + HVACFunction.m_archXdbPath;
            SQLiteConnection m_dbConnection = new SQLiteConnection(connectionstr);
            m_dbConnection.Open();

            string sql = "select * from Storeys where  Id =  ";
            sql = sql + readerDoor["storeyId"].ToString();
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
