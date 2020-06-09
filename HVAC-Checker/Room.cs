using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HVAC_CheckEngine
{
   public class Room :Element
    {
        public Room(long id):base(id)
        {
        }
        public string type { get; set; } = null;
        public string name { get; set; } = null;
        public string boundaryLoops { get; set; } = null;

        public int? m_iNumberOfPeople { get; set; } = null;
        
        public double? m_dArea { get; set; } = null;
        public RoomPosition? m_eRoomPosition { get; set; } = null;
      
        public double? m_dVolume { get; set; } = null;

        public double? m_dHeight { get; set; } = null;
        public double? m_dMaxlength { get; set; } = null;

        public double? m_dWidth { get; set; } = null;

        public override void setParameter(SQLiteDataReader reader)
        {
            base.setParameter(reader);

            name = reader["name"].ToString();
            m_dHeight = Convert.ToDouble(reader["dHeight"].ToString()) * 0.001;
            m_dArea = Convert.ToDouble(reader["dArea"].ToString());
            m_dWidth = Convert.ToDouble(reader["dWidth"].ToString()) * 0.001;
            m_iNumberOfPeople = Convert.ToInt32(reader["nNumberOfPeople"].ToString());
            boundaryLoops = reader["boundaryLoops"].ToString();
            m_eRoomPosition = changeRoomPositonStringToRoomPositionType(reader["position"].ToString());

            m_dVolume = m_dArea * m_dHeight;

            type = name;


            if (!System.IO.File.Exists(HVACFunction.m_archXdbPath))
                return;

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
            }

            m_dbConnection.Close();
        }

        public static RoomPosition changeRoomPositonStringToRoomPositionType(string XdbRoomPosition)
        {
            if (XdbRoomPosition.Equals("地上房间"))
                return RoomPosition.overground;
            else if (XdbRoomPosition.Equals("地下室"))
                return RoomPosition.underground;
            else
                return RoomPosition.semi_underground;
        }

        public static List<string> changeRoomPositonTypeToRoomPositionString(RoomPosition positon)
        {
            List<string> roomPositionStrings = new List<string>();
            if ((positon & RoomPosition.overground) != 0)
                roomPositionStrings.Add("地上房间");
            else if ((positon & RoomPosition.underground) != 0)
                roomPositionStrings.Add("地下室");
            else if ((positon & RoomPosition.semi_underground) != 0)
                roomPositionStrings.Add("半地下室");

            return roomPositionStrings;
        }

    }


    //public class Door
    //{
    //    public Door(long id)
    //    {
    //        Id = id;
    //    }
    //    public long? Id { get; set; } = null;
    //}
}
