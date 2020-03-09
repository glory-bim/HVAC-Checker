//#define RELEASE
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.IO;
using BCGL.Sharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HVAC_CheckEngine
{

    public class HVACFunction
    {

        public static string m_archXdbPath { set; get; }
        public static string m_hvacXdbPath { set; get; }

        private static string m_strLastId;
        private static List<string> m_listStrLastId;

        public HVACFunction(string Archxdb,string HVACxdb)
        {
            m_archXdbPath = Archxdb;
            m_hvacXdbPath = HVACxdb;
        }

        public static string GetCurrentPath(string dbName)
        {
            dynamic ProgramType = (new Program()).GetType();
            string currentDirectory = Path.GetDirectoryName(ProgramType.Assembly.Location);
            int iSub = currentDirectory.IndexOf("\\bin");
            currentDirectory = currentDirectory.Substring(0, iSub);
            string strPath = new DirectoryInfo(currentDirectory + dbName).FullName;
            return strPath;
        }

        //1获取指定类型、指定名称、大于一定面积的地上或地下房间对象集合
        public static List<Room> GetRooms(string type, string name, double area, RoomPosition roomPosition)
        {
            List<Room> rooms = new List<Room>();         
            //如果不存在，则创建一个空的数据库,
            if (!System.IO.File.Exists(m_archXdbPath))
                return rooms;   

            //创建一个连接
            string connectionstr = @"data source =" + m_archXdbPath;
            SQLiteConnection dbConnection = new SQLiteConnection(connectionstr);            
            dbConnection.Open();          
           // string sql = "select * from Spaces Where userLabel = ";


            string sql = "select * from Spaces Where CHARINDEX(";
            sql = sql + "'" + type + "'";

            sql = sql + ",userLabel)> 0";

            sql = sql + " and CHARINDEX(";

            sql = sql + "'" + name + "'";

            sql = sql + ",name)> 0";


            // sql = sql + "'"+type+"'";
            //  sql = sql + "and name = " ;
           // sql = sql + "'" + name + "'";
            sql = sql + " and dArea > ";
            sql = sql + area;
            SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                Room room = new Room(Convert.ToInt64(reader["Id"].ToString()));
                room.name = reader["name"].ToString();
                rooms.Add(room);
            }             
            //关闭连接
            dbConnection.Close();                       
            return rooms;
        }

        //2 判断房间是否有某种构件并返回构件对象
        //  Room    某种构件  风口
        public static List<AirTerminal> GetRoomContainAirTerminal(Room room)
        {
            List<AirTerminal> airterminals = new List<AirTerminal>();           
     
            //如果不存在，则创建一个空的数据库,
            if (!System.IO.File.Exists(m_archXdbPath))
                return airterminals;

            //创建一个连接
            string connectionstr = @"data source =" + m_archXdbPath;
            SQLiteConnection dbConnection = new SQLiteConnection(connectionstr);
            dbConnection.Open();
            string sql = "select * from Spaces Where Id = ";
            sql = sql + room.Id;          
            SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
   
            if(reader.Read())
            {                
                room.name = reader["name"].ToString();
                room.boundaryLoops = reader["boundaryLoops"].ToString();
                Polygon2D poly = GetSpaceBBox(room.boundaryLoops, room.Id.ToString());


       
                //创建一个连接
                connectionstr = @"data source =" + m_hvacXdbPath;
                SQLiteConnection dbConnectionHVAC = new SQLiteConnection(connectionstr);
                dbConnectionHVAC.Open();
                sql = "select * from AirTerminals";
                SQLiteCommand commandHVAC = new SQLiteCommand(sql, dbConnectionHVAC);
                SQLiteDataReader readerAirTerminals = commandHVAC.ExecuteReader();
                while (readerAirTerminals.Read())
                {

                    string sTransformer = readerAirTerminals["transformer"].ToString();

                    sql = "select * from LODRelations where graphicElementId = ";
                    sql = sql + readerAirTerminals["Id"].ToString();

                    SQLiteCommand commandHVAC1 = new SQLiteCommand(sql, dbConnectionHVAC);
                    SQLiteDataReader readerHVAC1 = commandHVAC1.ExecuteReader();
                    while (readerHVAC1.Read())
                    {
                        sql = "select * from Geometrys where Id = ";
                        sql = sql + readerHVAC1["geometryId"].ToString();
                        SQLiteCommand commandHVACGeo = new SQLiteCommand(sql, dbConnectionHVAC);
                        SQLiteDataReader readerHVACGeo = commandHVACGeo.ExecuteReader();
                        while (readerHVACGeo.Read())
                        {
                            readerHVACGeo["Id"].ToString();

                            Geometry geo = new Geometry();
                            geo.Id = Convert.ToInt64(readerHVACGeo["Id"].ToString());
                            geo.vertices = readerHVACGeo["vertices"].ToString();
                            geo.vertexIndexes = readerHVACGeo["vertexIndexes"].ToString();
                            geo.normals = readerHVACGeo["normals"].ToString();
                            geo.normalIndexes = readerHVACGeo["normalIndexes"].ToString();
                            geo.textrueCoords = readerHVACGeo["textrueCoords"].ToString();
                            geo.textrueCoordIndexes = readerHVACGeo["textrueCoordIndexes"].ToString();
                            geo.materialIds = readerHVACGeo["materialIds"].ToString();

                            AABB aabb = GeometryFunction.GetGeometryBBox(geo, sTransformer);



                            PointInt pt = aabb.Center();
                            if (Geometry_Utils_BBox.IsPointInBBox2D(poly, aabb.Center())
                                || Geometry_Utils_BBox.IsBBoxIntersectsBBox3D(poly, aabb)
                                || Geometry_Utils_BBox.IsPointInBBox2D(aabb, poly.Center())
                                || Geometry_Utils_BBox.IsPointInBBox2D(poly, aabb.Min)
                                || Geometry_Utils_BBox.IsPointInBBox2D(poly, aabb.Max))
                            {

                                AirTerminal airTerminal = new AirTerminal(Convert.ToInt64(readerAirTerminals["Id"].ToString()));
                            
                                airterminals.Add(airTerminal);
                            }
                        }
                    }
                }
            }
            //关闭连接
            dbConnection.Close();        
                         
            return airterminals;
        }

        public static List<Windows> GetWindowsInRoom(Room room)
        {
            List<Windows> windows = new List<Windows>();       
            if (!System.IO.File.Exists(m_archXdbPath))
                return windows;

            //创建一个连接
            string connectionstr = @"data source =" + m_archXdbPath;
            SQLiteConnection dbConnection = new SQLiteConnection(connectionstr);
            dbConnection.Open();
            string sql = "select * from Spaces Where Id = ";
            sql = sql + room.Id;
            SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();

            if (reader.Read())
            {
                room.name = reader["name"].ToString();
                room.boundaryLoops = reader["boundaryLoops"].ToString();
                Polygon2D poly = GetSpaceBBox(room.boundaryLoops, room.Id.ToString());
            
                sql = "select * from Windows";
                SQLiteCommand commandWindows = new SQLiteCommand(sql, dbConnection);
                SQLiteDataReader readerWindows = commandWindows.ExecuteReader();
                while (readerWindows.Read())
                {

                    string sTransformer = readerWindows["transformer"].ToString();

                    sql = "select * from LODRelations where graphicElementId = ";
                    sql = sql + readerWindows["Id"].ToString();

                    SQLiteCommand commandHVAC1 = new SQLiteCommand(sql, dbConnection);
                    SQLiteDataReader readerHVAC1 = commandHVAC1.ExecuteReader();
                    while (readerHVAC1.Read())
                    {
                        sql = "select * from Geometrys where Id = ";
                        sql = sql + readerHVAC1["geometryId"].ToString();
                        SQLiteCommand commandHVACGeo = new SQLiteCommand(sql, dbConnection);
                        SQLiteDataReader readerHVACGeo = commandHVACGeo.ExecuteReader();
                        while (readerHVACGeo.Read())
                        {
                            readerHVACGeo["Id"].ToString();

                            Geometry geo = new Geometry();
                            geo.Id = Convert.ToInt64(readerHVACGeo["Id"].ToString());
                            geo.vertices = readerHVACGeo["vertices"].ToString();
                            geo.vertexIndexes = readerHVACGeo["vertexIndexes"].ToString();
                            geo.normals = readerHVACGeo["normals"].ToString();
                            geo.normalIndexes = readerHVACGeo["normalIndexes"].ToString();
                            geo.textrueCoords = readerHVACGeo["textrueCoords"].ToString();
                            geo.textrueCoordIndexes = readerHVACGeo["textrueCoordIndexes"].ToString();
                            geo.materialIds = readerHVACGeo["materialIds"].ToString();

                            AABB aabb = GeometryFunction.GetGeometryBBox(geo, sTransformer);

                            PointInt pt = aabb.Center();
                            if (Geometry_Utils_BBox.IsPointInBBox2D(poly, aabb.Center())
                                || Geometry_Utils_BBox.IsBBoxIntersectsBBox3D(poly, aabb)
                                || Geometry_Utils_BBox.IsPointInBBox2D(aabb, poly.Center())
                                || Geometry_Utils_BBox.IsPointInBBox2D(poly, aabb.Min)
                                || Geometry_Utils_BBox.IsPointInBBox2D(poly, aabb.Max))
                            {

                                Windows window = new Windows(Convert.ToInt64(readerWindows["Id"].ToString()));

                                windows.Add(window);
                            }
                        }
                    }
                }
            }
            //关闭连接
            dbConnection.Close();

            return windows;
        }     
        /// <summary>
        /// 将json反序列化为List&lt;List&lt;XDBCurve&gt;&gt;
        /// </summary>
        /// <param name="json">要反序列化的json字符串</param>
        /// <returns>返回一个List&lt;List&lt;XDBCurve&gt;&gt;值</returns>
        public static List<List<XDBCurve>> ConvertJsonToBoundaryLoops(String json)
        {
            if (String.IsNullOrEmpty(json))
                return null;
            if (json.ToLower() == "null")
                return null;
            List<List<XDBCurve>> xdbCurveLoops = new List<List<XDBCurve>>();
            JArray boundaryLoopsJArry = (JArray)JsonConvert.DeserializeObject(json);
            for (int i = 0; i < boundaryLoopsJArry.Count; i++)
            {
                List<XDBCurve> xdbCurveLoop = new List<XDBCurve>();

                JArray boundaryLoopJArry = (JArray)boundaryLoopsJArry[i];
                for (int j = 0; j < boundaryLoopJArry.Count; j++)
                {
                    JToken curveJToken = (JToken)boundaryLoopJArry[j];

                    string curveType = curveJToken["curveType"].ToString();

                    XDBCurve curve = null;
                    if (curveType == typeof(XDBBoundLine).Name)
                    {
                        curve = JsonConvert.DeserializeObject<XDBBoundLine>(curveJToken.ToString());
                    }
                    else if (curveType == typeof(XDBEllipse).Name)
                    {
                        curve = JsonConvert.DeserializeObject<XDBEllipse>(curveJToken.ToString());
                    }
                    else if (curveType == typeof(XDB2DPolyLine).Name)
                    {
                        curve = JsonConvert.DeserializeObject<XDB2DPolyLine>(curveJToken.ToString());
                    }
                    else if (curveType == typeof(XDB3DPolyLine).Name)
                    {
                        curve = JsonConvert.DeserializeObject<XDB3DPolyLine>(curveJToken.ToString());
                    }

                    xdbCurveLoop.Add(curve);
                }

                xdbCurveLoops.Add(xdbCurveLoop);
            }
            return xdbCurveLoops;
        }

        /// <summary>
        /// 根据房间边界曲线，获得房间的包围盒（默认AABB）
        /// </summary>
        /// <param name="boundaryLoops"></param>
        /// <param name="sSpaceId"></param>
        /// <returns>Polygon2D包围盒数据</returns>
        public static Polygon2D GetSpaceBBox(string boundaryLoops, string sSpaceId)
        {
            List<List<XDBCurve>> xdbCurveLoops = ConvertJsonToBoundaryLoops(boundaryLoops);
            List<PointIntList> PointLists = new List<PointIntList>();
            foreach (List<XDBCurve> xdbCurveLoop in xdbCurveLoops)
            {
                PointIntList pointList = new PointIntList();

                foreach (XDBCurve xdbCurve in xdbCurveLoop)
                {
                    if (xdbCurve is XDBBoundLine)
                    {
                        XDBBoundLine line = xdbCurve as XDBBoundLine;
                        PointInt ptS = new PointInt((int)line.startPoint.x, (int)line.startPoint.y, (int)line.startPoint.z);
                        pointList.Add(ptS);
                        PointInt ptE = new PointInt((int)line.endPoint.x, (int)line.endPoint.y, (int)line.endPoint.z);
                        pointList.Add(ptE);
                    }
                    else if (xdbCurve is XDBEllipse)
                    {
                        // 椭圆曲线进行离散，将离散点加入房间包围盒数据
                        XDBEllipse xdbcurve = xdbCurve as XDBEllipse;
                        int nSplitCount = 6; // 将椭圆曲线离散的点个数
                        double dSplitAngle = xdbcurve.sweepAngle / (nSplitCount - 1);
                        for (int i = 0; i < nSplitCount; i++)
                        {
                            PointInt ptInt = getEllipsePoint(xdbcurve, xdbcurve.startAngle + i * dSplitAngle);
                            pointList.Add(ptInt);
                        }
                    }
                    else
                    {
                        // 目前只考虑区域边界为直线，圆弧(椭圆曲线)两种情况
                    }
                }

                if (pointList.Count > 0)
                    PointLists.Add(pointList);
            }

            if (PointLists.Count == 0)
            {
                PointLists.Add(new PointIntList() { new PointInt(0, 0, 0) });
            }

            Polygon2D poly = new Polygon2D(PointLists, sSpaceId);
            return poly;
        }


        public static OBB GetSpaceOBB(string boundaryLoops, string sSpaceId)
        {
            List<List<XDBCurve>> xdbCurveLoops = ConvertJsonToBoundaryLoops(boundaryLoops);
            List<PointIntList> PointLists = new List<PointIntList>();
            PointIntList pointList = new PointIntList();
            foreach (List<XDBCurve> xdbCurveLoop in xdbCurveLoops)
            {
               // PointIntList pointList = new PointIntList();

                foreach (XDBCurve xdbCurve in xdbCurveLoop)
                {
                    if (xdbCurve is XDBBoundLine)
                    {
                        XDBBoundLine line = xdbCurve as XDBBoundLine;
                        PointInt ptS = new PointInt((int)line.startPoint.x, (int)line.startPoint.y, (int)line.startPoint.z);
                        pointList.Add(ptS);
                        PointInt ptE = new PointInt((int)line.endPoint.x, (int)line.endPoint.y, (int)line.endPoint.z);
                        pointList.Add(ptE);
                    }
                    else if (xdbCurve is XDBEllipse)
                    {
                        // 椭圆曲线进行离散，将离散点加入房间包围盒数据
                        XDBEllipse xdbcurve = xdbCurve as XDBEllipse;
                        int nSplitCount = 6; // 将椭圆曲线离散的点个数
                        double dSplitAngle = xdbcurve.sweepAngle / (nSplitCount - 1);
                        for (int i = 0; i < nSplitCount; i++)
                        {
                            PointInt ptInt = getEllipsePoint(xdbcurve, xdbcurve.startAngle + i * dSplitAngle);
                            pointList.Add(ptInt);
                        }
                    }
                    else
                    {
                        // 目前只考虑区域边界为直线，圆弧(椭圆曲线)两种情况
                    }
                }

                if (pointList.Count > 0)
                    PointLists.Add(pointList);
            }

            if (PointLists.Count == 0)
            {
                PointLists.Add(new PointIntList() { new PointInt(0, 0, 0) });
            }
            OBB obb = pointList.GetOBB(sSpaceId);
     
            return obb;
        }

        public static PointInt getEllipsePoint(XDBEllipse xdbellipse, Double angle)
        {
            double a = Math.Pow(xdbellipse.vector0.x, 2);
            double b = Math.Pow(xdbellipse.vector90.y, 2);
            double c = Math.Pow(Math.Tan(angle), 2);
            double d = 1 / a + c / b;
            double dx;
            if ((0 <= angle && angle <= (Math.PI / 2)) || (3 * Math.PI < angle && angle <= 2 * Math.PI))
                dx = Math.Abs(Math.Sqrt(1 / d));
            else
                dx = -Math.Abs(Math.Sqrt(1 / d));
            double dy = dx * Math.Tan(angle);
            PointInt pt = new PointInt((int)(dx + xdbellipse.center.x), (int)(dy + xdbellipse.center.y), (int)(xdbellipse.center.z));
            return pt;
        }

        //3找到与风口相连的风机对象//一条路径 三通、四通只记录了一个id
        public static List<Fan> GetFanConnectingAirterminal(AirTerminal airterminal)
        {
            List<Fan> fans = new List<Fan>();       

            if (!System.IO.File.Exists(m_hvacXdbPath))
                return fans;
            string connectionstr = @"data source =" + m_hvacXdbPath;
            SQLiteConnection dbConnection = new SQLiteConnection(connectionstr);
            dbConnection.Open();
            //创建一个连接
            FindFansByLinkId(dbConnection, airterminal.Id.ToString(), fans);
            //关闭连接
            dbConnection.Close();

            return fans;      
        }

        public static void FindFansByLinkId(SQLiteConnection dbConnection, String strId, List<Fan> inlets)
        {
            string sql = "select * from MepConnectionRelations Where mainElementId = ";
            sql += strId;
            SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                if(reader["linkElementId"].ToString() != m_strLastId)
                {
                    sql = "select * from HVACFans Where Id = ";
                    sql += reader["linkElementId"].ToString();

                    SQLiteCommand commandFans = new SQLiteCommand(sql, dbConnection);
                    SQLiteDataReader readerFans = commandFans.ExecuteReader();
                    if (readerFans.Read())
                    {
                        Fan inlet = new Fan(Convert.ToInt64(readerFans["Id"].ToString()));
                        inlets.Add(inlet);
                    }
                    else
                    {
                        m_strLastId = strId;
                        FindFansByLinkId(dbConnection, reader["linkElementId"].ToString(), inlets);
                    }
                }                          
            }                                    
        }

        //4找到风机的排风口对象//怎么判断风口前端后端
        public static List<AirTerminal> GetOutletsOfFan(Fan fan)
        {
            List<AirTerminal> airTerminals = new List<AirTerminal>();        
            if (!System.IO.File.Exists(m_hvacXdbPath))
                return airTerminals;
            string connectionstr = @"data source =" + m_hvacXdbPath;
            SQLiteConnection dbConnection = new SQLiteConnection(connectionstr);
            dbConnection.Open();
            //创建一个连接
            m_listStrLastId = new List<string>();
            FindOutLets(dbConnection, fan.Id.ToString(), airTerminals);
            //关闭连接
            dbConnection.Close();
         
            return airTerminals; 
        }

        public static void FindOutLets(SQLiteConnection dbConnection, String strId, List<AirTerminal> inlets)
        {
            string sql = "select * from MepConnectionRelations Where mainElementId = ";
            sql += strId;
            SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                if (!m_listStrLastId.Exists(x => x == reader["linkElementId"].ToString()))
                {
                    sql = "select * from AirTerminals Where Id = ";
                    sql += reader["linkElementId"].ToString();

                    SQLiteCommand commandDucts = new SQLiteCommand(sql, dbConnection);
                    SQLiteDataReader readerDucts = commandDucts.ExecuteReader();
                    if (readerDucts.Read())
                    {
                        AirTerminal inlet = new AirTerminal(Convert.ToInt64(readerDucts["Id"].ToString()));
                        inlets.Add(inlet);
                    }
                    else
                    {
                        string strLastId = reader["linkElementId"].ToString();
                        m_listStrLastId.Add(strLastId);
                        FindOutLets(dbConnection, reader["linkElementId"].ToString(), inlets);
                    }
                }               
            }
        }

        public static void FindOutletsByDuct(SQLiteConnection dbConnection, String strId, List<AirTerminal> ducts)
        {
            string sql = "select * from Ducts Where Id = ";
            sql += strId;

            SQLiteCommand commandDuct3T = new SQLiteCommand(sql, dbConnection);
            SQLiteDataReader readerDuct3T = commandDuct3T.ExecuteReader();
            if (readerDuct3T.Read())
            {
                FindOutLets(dbConnection, readerDuct3T["Id"].ToString(), ducts);
            }
        }
        public static void FindOutletsByDuct3t(SQLiteConnection dbConnection, String strId, List<AirTerminal> ducts)
        {
            string sql = "select * from Duct3Ts Where Id = ";
            sql += strId;

            SQLiteCommand commandDuct3T = new SQLiteCommand(sql, dbConnection);
            SQLiteDataReader readerDuct3T = commandDuct3T.ExecuteReader();
            if (readerDuct3T.Read())
            {
                FindOutLets(dbConnection, readerDuct3T["Id"].ToString(), ducts);
            }
        }

        public static void FindOutletsByDuct4t(SQLiteConnection dbConnection, String strId, List<AirTerminal> ducts)
        {
            string sql = "select * from Duct4Ts Where Id = ";
            sql += strId;

            SQLiteCommand commandDuct4T = new SQLiteCommand(sql, dbConnection);
            SQLiteDataReader readerDuct4T = commandDuct4T.ExecuteReader();
            if (readerDuct4T.Read())
            {
                FindOutLets(dbConnection, readerDuct4T["Id"].ToString(), ducts);
            }
        }

        public static void FindOutletsByDuctDampers(SQLiteConnection dbConnection, String strId, List<AirTerminal> ducts)
        {
            string sql = "select * from DuctDampers Where Id = ";
            sql += strId;

            SQLiteCommand commandDuctDampers = new SQLiteCommand(sql, dbConnection);
            SQLiteDataReader readerDampers = commandDuctDampers.ExecuteReader();
            if (readerDampers.Read())
            {
                FindOutLets(dbConnection, readerDampers["Id"].ToString(), ducts);
            }
        }
             

        //5找到大于一定长度的走道对象  double  “走道、走廊”    长度清华引擎 计算学院  张荷花
        public static List<Room> GetRoomsMoreThan(double dLength)
        {
            List<Room> rooms = new List<Room>();            
        
            if (!System.IO.File.Exists(m_archXdbPath))
                return rooms;

            //创建一个连接
            string connectionstr = @"data source =" + m_archXdbPath;
            SQLiteConnection dbConnection = new SQLiteConnection(connectionstr);
            dbConnection.Open();
            string sql = "select * from Spaces";          
            SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                Room room = new Room(Convert.ToInt64(reader["Id"].ToString()));
                room.name = reader["name"].ToString();
                room.boundaryLoops = reader["boundaryLoops"].ToString();
               // Polygon2D poly = GetSpaceBBox(room.boundaryLoops, room.Id.ToString());

                OBB obb = GetSpaceOBB(room.boundaryLoops, room.Id.ToString());
                double dLengthOBB = obb.GetLength();
                if (dLengthOBB > dLength)
                    rooms.Add(room);
            }
            //关闭连接
            dbConnection.Close();
            return rooms;
        }
        //6找到一个风机的进风口对象集合
        public static List<AirTerminal> GetInletsOfFan(Fan fan)
        {
            List<AirTerminal> inlets = new List<AirTerminal>();                
            if (!System.IO.File.Exists(m_hvacXdbPath))
                return inlets;
            string connectionstr = @"data source =" + m_hvacXdbPath;
            SQLiteConnection dbConnection = new SQLiteConnection(connectionstr);
            dbConnection.Open();
            m_listStrLastId = new List<string>();
            FindInlets(dbConnection, fan.Id.ToString(), inlets);
            //关闭连接
            dbConnection.Close();           
            return inlets;
        }
        public static void FindInlets(SQLiteConnection dbConnection, String strId, List<AirTerminal> inlets)
        {
            string sql = "select * from MepConnectionRelations Where mainElementId = ";
            sql += strId;
            SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {                    
                if (!m_listStrLastId.Exists(x => x == reader["linkElementId"].ToString()))
                {
                    sql = "select * from AirTerminals Where Id = ";
                    sql += reader["linkElementId"].ToString();

                    SQLiteCommand commandAirterminal = new SQLiteCommand(sql, dbConnection);
                    SQLiteDataReader readerAirTerminal = commandAirterminal.ExecuteReader();
                    if (readerAirTerminal.Read())
                    {
                        AirTerminal inlet = new AirTerminal(Convert.ToInt64(readerAirTerminal["Id"].ToString()));
                        inlets.Add(inlet);
                    }
                    else
                    {
                        string strLastId = reader["linkElementId"].ToString();
                        m_listStrLastId.Add(strLastId);
                        FindInlets(dbConnection, reader["linkElementId"].ToString(), inlets);
                    }
                }             
            }
        }

        public static void FindInletsByDuct(SQLiteConnection dbConnection, String strId, List<AirTerminal> ducts)
        {
            string sql = "select * from Ducts Where Id = ";
            sql += strId;

            SQLiteCommand commandDuct3T = new SQLiteCommand(sql, dbConnection);
            SQLiteDataReader readerDuct3T = commandDuct3T.ExecuteReader();
            if (readerDuct3T.Read())
            {
                FindInlets(dbConnection, readerDuct3T["Id"].ToString(), ducts);
            }
        }
        public static void FindInletsByDuct3t(SQLiteConnection dbConnection, String strId, List<AirTerminal> ducts)
        {
            string sql = "select * from Duct3Ts Where Id = ";
            sql += strId;

            SQLiteCommand commandDuct3T = new SQLiteCommand(sql, dbConnection);
            SQLiteDataReader readerDuct3T = commandDuct3T.ExecuteReader();
            if (readerDuct3T.Read())
            {
                FindInlets(dbConnection, readerDuct3T["Id"].ToString(), ducts);
            }
        }

        public static void FindInletsByDuct4t(SQLiteConnection dbConnection, String strId, List<AirTerminal> ducts)
        {
            string sql = "select * from Duct4Ts Where Id = ";
            sql += strId;

            SQLiteCommand commandDuct4T = new SQLiteCommand(sql, dbConnection);
            SQLiteDataReader readerDuct4T = commandDuct4T.ExecuteReader();
            if (readerDuct4T.Read())
            {
                FindInlets(dbConnection, readerDuct4T["Id"].ToString(), ducts);
            }
        }

        public static void FindInletsByDuctDampers(SQLiteConnection dbConnection, String strId, List<AirTerminal> ducts)
        {
            string sql = "select * from DuctDampers Where Id = ";
            sql += strId;

            SQLiteCommand commandDuctDampers = new SQLiteCommand(sql, dbConnection);
            SQLiteDataReader readerDampers = commandDuctDampers.ExecuteReader();
            if (readerDampers.Read())
            {
                FindInlets(dbConnection, readerDampers["Id"].ToString(), ducts);
            }
        }


        //7找到穿越某些房间的风管对象集合  清华引擎 构件相交  包含
        public static List<Duct> GetDuctsCrossSpace(Room room)
        {
            List<Duct> ducts = new List<Duct>();
            if (!System.IO.File.Exists(m_archXdbPath))
                return ducts;

            //创建一个连接
            string connectionstr = @"data source =" + m_archXdbPath;
            SQLiteConnection dbConnection = new SQLiteConnection(connectionstr);
            dbConnection.Open();
            string sql = "select * from Spaces Where Id = ";
            sql = sql + room.Id;
            SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
            SQLiteDataReader readerSpace = command.ExecuteReader();

            if (readerSpace.Read())
            {
                room.name = readerSpace["name"].ToString();
                room.boundaryLoops = readerSpace["boundaryLoops"].ToString();
                Polygon2D poly = GetSpaceBBox(room.boundaryLoops, room.Id.ToString());                                          
                
                connectionstr = @"data source =" + m_hvacXdbPath;
                SQLiteConnection dbConnectionHVAC = new SQLiteConnection(connectionstr);
                dbConnectionHVAC.Open();
                sql = "select * from Ducts";
                SQLiteCommand commandHVAC = new SQLiteCommand(sql, dbConnectionHVAC);
                SQLiteDataReader readerDucts = commandHVAC.ExecuteReader();
                while (readerDucts.Read())
                {
                    string sTransformer = readerDucts["transformer"].ToString();
                    sql = "select * from LODRelations where graphicElementId = ";
                    sql = sql + readerDucts["Id"].ToString();

                    SQLiteCommand commandHVAC1 = new SQLiteCommand(sql, dbConnectionHVAC);
                    SQLiteDataReader readerHVAC1 = commandHVAC1.ExecuteReader();
                    while (readerHVAC1.Read())
                    {
                        sql = "select * from Geometrys where Id = ";
                        sql = sql + readerHVAC1["geometryId"].ToString();
                        SQLiteCommand commandHVACGeo = new SQLiteCommand(sql, dbConnectionHVAC);
                        SQLiteDataReader readerHVACGeo = commandHVACGeo.ExecuteReader();
                        while (readerHVACGeo.Read())
                        {
                            readerHVACGeo["Id"].ToString();

                            Geometry geo = new Geometry();
                            geo.Id = Convert.ToInt64(readerHVACGeo["Id"].ToString());
                            geo.vertices = readerHVACGeo["vertices"].ToString();
                            geo.vertexIndexes = readerHVACGeo["vertexIndexes"].ToString();
                            geo.normals = readerHVACGeo["normals"].ToString();
                            geo.normalIndexes = readerHVACGeo["normalIndexes"].ToString();
                            geo.textrueCoords = readerHVACGeo["textrueCoords"].ToString();
                            geo.textrueCoordIndexes = readerHVACGeo["textrueCoordIndexes"].ToString();
                            geo.materialIds = readerHVACGeo["materialIds"].ToString();

                            AABB aabb = GeometryFunction.GetGeometryBBox(geo, sTransformer);

                            

                            PointInt pt = aabb.Center();
                            if (Geometry_Utils_BBox.IsPointInBBox2D(poly, aabb.Center())
                                || Geometry_Utils_BBox.IsBBoxIntersectsBBox3D(poly, aabb)
                                || Geometry_Utils_BBox.IsPointInBBox2D(aabb, poly.Center())
                                || Geometry_Utils_BBox.IsPointInBBox2D(poly, aabb.Min)
                                || Geometry_Utils_BBox.IsPointInBBox2D(poly, aabb.Max))
                            {

                                Duct duct = new Duct(Convert.ToInt64(readerDucts["Id"].ToString()));

                                ducts.Add(duct);
                            }
                        }
                    }
                }
            }
            //关闭连接
            dbConnection.Close();                               

            return ducts;
        }
        //8找到穿越防火分区的风管对象集合  userlable
        public static List<Duct> GetDuctsCrossFireDistrict(FireDistrict fireDistrict)
        {          
            List<Duct> ducts = new List<Duct>();    
            //如果不存在，则创建一个空的数据库,
            if (!System.IO.File.Exists(m_archXdbPath))
                return ducts;

            //创建一个连接
            string connectionstr = @"data source =" + m_archXdbPath;
            SQLiteConnection dbConnection = new SQLiteConnection(connectionstr);
            dbConnection.Open();
            string sql = "select * from Spaces Where Id = ";
            sql = sql + fireDistrict.Id;
            SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
            SQLiteDataReader readerSpace = command.ExecuteReader();

            if (readerSpace.Read())
            {
                fireDistrict.name = readerSpace["name"].ToString();
                fireDistrict.boundaryLoops = readerSpace["boundaryLoops"].ToString();
                Polygon2D poly = GetSpaceBBox(fireDistrict.boundaryLoops, fireDistrict.Id.ToString());
                
          
                //创建一个连接
                connectionstr = @"data source =" + m_hvacXdbPath;
                SQLiteConnection dbConnectionHVAC = new SQLiteConnection(connectionstr);
                dbConnectionHVAC.Open();
                sql = "select * from Ducts";
                SQLiteCommand commandHVAC = new SQLiteCommand(sql, dbConnectionHVAC);
                SQLiteDataReader readerDucts = commandHVAC.ExecuteReader();
                while (readerDucts.Read())
                {
                    string sTransformer = readerDucts["transformer"].ToString();
                    sql = "select * from LODRelations where graphicElementId = ";
                    sql = sql + readerDucts["Id"].ToString();

                    SQLiteCommand commandHVAC1 = new SQLiteCommand(sql, dbConnectionHVAC);
                    SQLiteDataReader readerHVAC1 = commandHVAC1.ExecuteReader();
                    while (readerHVAC1.Read())
                    {
                        sql = "select * from Geometrys where Id = ";
                        sql = sql + readerHVAC1["geometryId"].ToString();
                        SQLiteCommand commandHVACGeo = new SQLiteCommand(sql, dbConnectionHVAC);
                        SQLiteDataReader readerHVACGeo = commandHVACGeo.ExecuteReader();
                        while (readerHVACGeo.Read())
                        {
                            readerHVACGeo["Id"].ToString();

                            Geometry geo = new Geometry();
                            geo.Id = Convert.ToInt64(readerHVACGeo["Id"].ToString());
                            geo.vertices = readerHVACGeo["vertices"].ToString();
                            geo.vertexIndexes = readerHVACGeo["vertexIndexes"].ToString();
                            geo.normals = readerHVACGeo["normals"].ToString();
                            geo.normalIndexes = readerHVACGeo["normalIndexes"].ToString();
                            geo.textrueCoords = readerHVACGeo["textrueCoords"].ToString();
                            geo.textrueCoordIndexes = readerHVACGeo["textrueCoordIndexes"].ToString();
                            geo.materialIds = readerHVACGeo["materialIds"].ToString();

                            AABB aabb = GeometryFunction.GetGeometryBBox(geo, sTransformer);                            

                            PointInt pt = aabb.Center();
                            if (Geometry_Utils_BBox.IsPointInBBox2D(poly, aabb.Center())
                                || Geometry_Utils_BBox.IsBBoxIntersectsBBox3D(poly, aabb)
                                || Geometry_Utils_BBox.IsPointInBBox2D(aabb, poly.Center())
                                || Geometry_Utils_BBox.IsPointInBBox2D(poly, aabb.Min)
                                || Geometry_Utils_BBox.IsPointInBBox2D(poly, aabb.Max))
                            {

                                Duct duct = new Duct(Convert.ToInt64(readerDucts["Id"].ToString()));

                                ducts.Add(duct);
                            }
                        }
                    }
                }
            }
            //关闭连接
            dbConnection.Close();


            return ducts;
        }
        //9找到穿越防火分隔处的变形缝两侧的风管集合  差变形缝对象
        public static List<Duct> GetDuctsCrossFireSide()
        {
            List<Duct> ducts = new List<Duct>();

            return ducts;
        }


        //10获得构建所在房间的对象  几何 包含 遍历表都查

        public static Room GetRoomOfAirterminal(AirTerminal airTerminal)
        {
            long lid = 0;//id為空的對象
            Room room = new Room(lid);     
            if (!System.IO.File.Exists(m_hvacXdbPath))
                return room;

            //创建一个连接
            string connectionstr = @"data source =" + m_hvacXdbPath;
            SQLiteConnection dbConnection = new SQLiteConnection(connectionstr);
            dbConnection.Open();
            string sql = "select * from AirTerminals Where Id = ";
            sql = sql + airTerminal.Id;
            SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            if (reader.Read())
            {
                string sTransformer = reader["transformer"].ToString();
                sql = "select * from LODRelations where graphicElementId = ";
                sql = sql + reader["Id"].ToString();

                SQLiteCommand commandHVAC1 = new SQLiteCommand(sql, dbConnection);
                SQLiteDataReader readerHVAC1 = commandHVAC1.ExecuteReader();
                if (readerHVAC1.Read())
                {
                    sql = "select * from Geometrys where Id = ";
                    sql = sql + readerHVAC1["geometryId"].ToString();
                    SQLiteCommand commandHVACGeo = new SQLiteCommand(sql, dbConnection);
                    SQLiteDataReader readerHVACGeo = commandHVACGeo.ExecuteReader();
                    if(readerHVACGeo.Read())
                    {
                        readerHVACGeo["Id"].ToString();

                        Geometry geo = new Geometry();
                        geo.Id = Convert.ToInt64(readerHVACGeo["Id"].ToString());
                        geo.vertices = readerHVACGeo["vertices"].ToString();
                        geo.vertexIndexes = readerHVACGeo["vertexIndexes"].ToString();
                        geo.normals = readerHVACGeo["normals"].ToString();
                        geo.normalIndexes = readerHVACGeo["normalIndexes"].ToString();
                        geo.textrueCoords = readerHVACGeo["textrueCoords"].ToString();
                        geo.textrueCoordIndexes = readerHVACGeo["textrueCoordIndexes"].ToString();
                        geo.materialIds = readerHVACGeo["materialIds"].ToString();

                        AABB aabb = GeometryFunction.GetGeometryBBox(geo, sTransformer);

                    
                        //如果不存在，则创建一个空的数据库,
                        if (!System.IO.File.Exists(m_archXdbPath))
                            return room;

                        //创建一个连接
                        string connectionArchstr = @"data source =" + m_archXdbPath;
                        SQLiteConnection dbConnectionArch = new SQLiteConnection(connectionArchstr);
                        dbConnectionArch.Open();
                        sql = "select * from Spaces";
                        SQLiteCommand commandRoom = new SQLiteCommand(sql, dbConnectionArch);
                        SQLiteDataReader readerRoom = commandRoom.ExecuteReader();

                        while (readerRoom.Read())
                        {
                            room.name = readerRoom["name"].ToString();
                            room.boundaryLoops = readerRoom["boundaryLoops"].ToString();
                            Polygon2D poly = GetSpaceBBox(room.boundaryLoops, room.Id.ToString());

                            PointInt pt = aabb.Center();
                            if (Geometry_Utils_BBox.IsPointInBBox2D(poly, aabb.Center())
                                || Geometry_Utils_BBox.IsBBoxIntersectsBBox3D(poly, aabb)
                                || Geometry_Utils_BBox.IsPointInBBox2D(aabb, poly.Center())
                                || Geometry_Utils_BBox.IsPointInBBox2D(poly, aabb.Min)
                                || Geometry_Utils_BBox.IsPointInBBox2D(poly, aabb.Max))
                            {

                                room = new Room(Convert.ToInt64(readerRoom["Id"].ToString()));
                               
                            }
                        }
                    }
                }
            }                                
        //关闭连接               
                return room;
        }

        //11获得一个窗户对象的有效面积  差公式 xdb差参数  开启角度  开启方式
        public static double GetArea(Windows window)  
        {
            double dArea = 0.0;  

            if (!System.IO.File.Exists(m_archXdbPath))
                return dArea;

            //创建一个连接
            string connectionstr = @"data source =" + m_archXdbPath;
            SQLiteConnection m_dbConnection = new SQLiteConnection(connectionstr);
            m_dbConnection.Open();
            string sql = "select Area from Windows Where Id = ";
            sql = sql + window.Id ;   

            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();          
            string sArea = "";
            while (reader.Read())
                sArea= reader["Area"].ToString();   
            
            dArea = Convert.ToDouble(sArea);
            //关闭连接
            m_dbConnection.Close();
       
            return dArea;
        }

        //12找到属于某种类型的房间对象集合
        public static List<Room> GetRooms(string roomType)
        {
            List<Room> rooms = new List<Room>();                
            if (!System.IO.File.Exists(m_archXdbPath))
                return rooms;

            //创建一个连接
            string connectionstr = @"data source =" + m_archXdbPath;
            SQLiteConnection m_dbConnection = new SQLiteConnection(connectionstr);
            m_dbConnection.Open();    
            string sql = "select * from Spaces Where CHARINDEX(";
            sql = sql +"'"+ roomType +"'" ;

            sql = sql + ",userLabel)> 0";

            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                Room room = new Room(Convert.ToInt64(reader["Id"].ToString()));
                rooms.Add(room);
            }
               
        return rooms;
    }
    //13找到名称包含某一字段的房间对象集合    
        public static List<Room> GetRoomsContainingString(string containedString)
        {
            List<Room> rooms = new List<Room>(); 
     
            if (!System.IO.File.Exists(m_archXdbPath))
                return rooms;

            //创建一个连接
            string connectionstr = @"data source =" + m_archXdbPath;
            SQLiteConnection m_dbConnection = new SQLiteConnection(connectionstr);
            m_dbConnection.Open();

            string sql = "select * from Spaces Where CHARINDEX(";
            sql = sql +"'"+ containedString + "'" ;
            sql = sql + ",name)> 0";       

            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                Room room =  new Room(Convert.ToInt64(reader["Id"].ToString()));                         
                room.name = reader["name"].ToString();
                rooms.Add(room);
            }
            
            return rooms;

        }

        // 14判断房间属于地上房间或地下房间  //差参数
        public static bool isOvergroundRoom(Room room)
        {       
            if (!System.IO.File.Exists(m_archXdbPath))
                return false;

            //创建一个连接
            string connectionstr = @"data source =" + m_archXdbPath;
            SQLiteConnection m_dbConnection = new SQLiteConnection(connectionstr);
            m_dbConnection.Open();
            string sql = "select userLabel from Spaces Where Id =";
            sql += room.Id;           

            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            if (reader.Read())
            {
                if (reader["UserLabel"].ToString() == "地上房间")
                    return true;
                else
                    return false;
            }  
            else
            {
                return false;
            }
        }
        //15获取所有楼层对象集合
        public static List<Floor> GetFloors()
        {
            List<Floor> floors = new List<Floor>();         
                    
            if (!System.IO.File.Exists(m_archXdbPath))
                return floors;

            //创建一个连接
            string connectionstr = @"data source =" + m_archXdbPath;
            SQLiteConnection m_dbConnection = new SQLiteConnection(connectionstr);
            m_dbConnection.Open();
            string sql = "select * from Storeys  ";
     
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
          
            while (reader.Read())
            {
                Floor floor = new Floor(Convert.ToInt64(reader["Id"].ToString()));       
                floor.storeyName = (reader["storeyName"].ToString());
                floors.Add(floor);
            }                       
            return floors;
        }

        public static void FindDucts(SQLiteConnection dbConnection, String strId, List<Duct> ducts)
        {
           
            string sql = "select * from MepConnectionRelations Where mainElementId = ";
            sql += strId;
            SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {

                sql = "select * from Ducts Where Id = ";
                sql += reader["linkElementId"].ToString();

                SQLiteCommand commandDucts = new SQLiteCommand(sql, dbConnection);
                SQLiteDataReader readerDucts = commandDucts.ExecuteReader();
                if (readerDucts.Read())
                {                                  
                    Duct duct = new Duct(Convert.ToInt64(readerDucts["Id"].ToString()));
                    
                    ducts.Add(duct);
                    FindDucts(dbConnection, readerDucts["Id"].ToString(), ducts);
                }
                else
                {
                    FindDuctsByDuct3t(dbConnection, reader["linkElementId"].ToString(), ducts);
                    FindDuctsByDuct4t(dbConnection, reader["linkElementId"].ToString(), ducts);
                    FindDuctsByDuctDuctDampers(dbConnection, reader["linkElementId"].ToString(), ducts);                    
                }

            }
        }

        public static void FindDuctsByDuct3t(SQLiteConnection dbConnection, String strId, List<Duct> ducts)
        {                  
                string  sql = "select * from Duct3Ts Where Id = ";
                sql += strId;

                SQLiteCommand commandDuct3T = new SQLiteCommand(sql, dbConnection);
                SQLiteDataReader readerDuct3T = commandDuct3T.ExecuteReader();
                if (readerDuct3T.Read())
                {
                    FindDucts(dbConnection, readerDuct3T["Id"].ToString(), ducts);
                }          
        }

        public static void FindDuctsByDuct4t(SQLiteConnection dbConnection, String strId, List<Duct> ducts)
        {
            string sql = "select * from Duct4Ts Where Id = ";
            sql += strId;

            SQLiteCommand commandDuct4T = new SQLiteCommand(sql, dbConnection);
            SQLiteDataReader readerDuct4T = commandDuct4T.ExecuteReader();
            if (readerDuct4T.Read())
            {
                FindDucts(dbConnection, readerDuct4T["Id"].ToString(), ducts);
            }
        }

        public static void FindDuctsByDuctDuctDampers(SQLiteConnection dbConnection, String strId, List<Duct> ducts)
        {
            string sql = "select * from DuctDampers Where Id = ";
            sql += strId;

            SQLiteCommand commandDuctDampers = new SQLiteCommand(sql, dbConnection);
            SQLiteDataReader readerDampers = commandDuctDampers.ExecuteReader();
            if (readerDampers.Read())
            {
                FindDucts(dbConnection, readerDampers["Id"].ToString(), ducts);
            }
        }

        //16获得风机所连接的所有风管集合  支路 干管  风口到末端 
        public static List<Duct> GetDuctsOfFan(Fan fan)
    {
            List<Duct> ducts = new List<Duct>(); 
            //如果不存在，则创建一个空的数据库,
            if (!System.IO.File.Exists(m_hvacXdbPath))
                return ducts;
            string connectionstr = @"data source =" + m_hvacXdbPath;
            SQLiteConnection dbConnection = new SQLiteConnection(connectionstr);
            dbConnection.Open();
            //创建一个连接
            FindDucts(dbConnection,fan.Id.ToString(),ducts);

            //关闭连接
            dbConnection.Close();          
        return ducts;
    }
        //17判断是否风机所连风系统所有支路都连接了风口  //管堵
        public static bool isAllBranchLinkingAirTerminal(Fan fan)
        {                   
            return IfFindAirTerminal(fan.Id.ToString());                  
        }

        public static bool IfFindAirTerminal(string strId)
        {        
            //如果不存在，则创建一个空的数据库,
            if (!System.IO.File.Exists(m_hvacXdbPath))
                return false;
            string connectionstr = @"data source =" + m_hvacXdbPath;
            SQLiteConnection dbConnection = new SQLiteConnection(connectionstr);
            dbConnection.Open();
            string sql = "select * from MepConnectionRelations Where mainElementId = ";
            sql += strId;
            SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                if (reader["linkElementId"].ToString() != m_strLastId)
                {
                    sql = "select * from AirTerminals Where Id = ";
                    sql += reader["linkElementId"].ToString();

                    SQLiteCommand commandAirTerminals = new SQLiteCommand(sql, dbConnection);
                    SQLiteDataReader readerAirTerminals = commandAirTerminals.ExecuteReader();
                    if (readerAirTerminals.Read())
                    {
                        return true;
                    }
                    else
                    {
                        m_strLastId = strId;
                        if (IfFindAirTerminal(reader["linkElementId"].ToString()))
                            return true;
                    }
                }             
            }
            return false;
         
        }

        public static bool IfFindAirterminalByDuct(SQLiteConnection dbConnection, String strId)
        {
            string sql = "select * from MepConnectionRelations Where mainElementId = ";
            sql += strId;
            SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                sql = "select * from AirTerminals Where Id = ";
                sql += reader["linkElementId"].ToString();

                SQLiteCommand commandDucts = new SQLiteCommand(sql, dbConnection);
                SQLiteDataReader readerDucts = commandDucts.ExecuteReader();
                if (readerDucts.Read())
                {
                    return true;
                }
                else
                {
                   if (IfFindAirterminalByDuct3t(dbConnection, reader["linkElementId"].ToString())||
                        IfFindAirterminalByDuct4t(dbConnection, reader["linkElementId"].ToString())||
                        IfFindAirterminalByDuctDampers(dbConnection, reader["linkElementId"].ToString())
                        )
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }                               
                }

            }

            return false;
        }

        public static bool IfFindAirterminalByDuct3t(SQLiteConnection dbConnection, String strId)
        {
            string sql = "select * from AirTerminals Where Id = ";
            sql += strId;

            SQLiteCommand commandDuct3T = new SQLiteCommand(sql, dbConnection);
            SQLiteDataReader readerDuct3T = commandDuct3T.ExecuteReader();
            if (readerDuct3T.Read())         
            {
                return true;
            }
            else
            {
                return IfFindAirTerminal(readerDuct3T["Id"].ToString());
            }
        }

        public static bool IfFindAirterminalByDuct4t(SQLiteConnection dbConnection, String strId)
        {
            string sql = "select * from AirTerminals Where Id = ";
            sql += strId;

            SQLiteCommand commandDuct4T = new SQLiteCommand(sql, dbConnection);
            SQLiteDataReader readerDuct4T = commandDuct4T.ExecuteReader();
            if (readerDuct4T.Read())         
            {
                return true;
            }
            else
            {
                return IfFindAirTerminal(readerDuct4T["Id"].ToString());
            }
        }

        public static bool IfFindAirterminalByDuctDampers(SQLiteConnection dbConnection, String strId)
        {
            string sql = "select * from AirTerminals Where Id = ";
            sql += strId;

            SQLiteCommand commandDuctDampers = new SQLiteCommand(sql, dbConnection);
            SQLiteDataReader readerDampers = commandDuctDampers.ExecuteReader();
            if (readerDampers.Read())
            {
                return true;               
            }
            else
            {
                return IfFindAirTerminal(readerDampers["Id"].ToString());
            }
        }
        //18获得防烟分区长边长度
        public static double GetFireDistrictLength(FireDistrict fireDistrict)
        {
            double dLength = 0.0;              
            if (!System.IO.File.Exists(m_archXdbPath))
                return dLength;

            //创建一个连接
            string connectionstr = @"data source =" + m_archXdbPath;
            SQLiteConnection dbConnection = new SQLiteConnection(connectionstr);
            dbConnection.Open();
            string sql = "select * from Spaces where Id = ";
            sql += fireDistrict.Id;
            SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                Room room = new Room(Convert.ToInt64(reader["Id"].ToString()));
                room.name = reader["name"].ToString();
                room.boundaryLoops = reader["boundaryLoops"].ToString();
                // Polygon2D poly = GetSpaceBBox(room.boundaryLoops, room.Id.ToString());

                OBB  obb = GetSpaceOBB(room.boundaryLoops, room.Id.ToString());
                dLength = obb.GetLength();           

            }
            //关闭连接
            dbConnection.Close();    
                       
            return dLength;
        }
        //19获得所有联通区域的集合 （联通区域是指与同一个走廊相连的所有房间的集合）
        public static List<Region> GetConnectedRegion()
        {
            List<Region> regions = new List<Region>();
            //找出所有走廊
            List<Room> corridors = new List<Room>();


            if (!System.IO.File.Exists(m_archXdbPath))
                return regions;

            //创建一个连接
            string connectionstr = @"data source =" + m_archXdbPath;
            SQLiteConnection dbConnection = new SQLiteConnection(connectionstr);
            dbConnection.Open();       

            string sql = "select * from Spaces Where CHARINDEX(";
            sql = sql + "'" + "走廊" + "'";
            sql = sql + ",userLabel)> 0";


            SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                Room room = new Room(Convert.ToInt64(reader["Id"].ToString()));
                room.name = reader["Id"].ToString();
                corridors.Add(room);
            }
            //每个走廊在门表中找关联房间
            for (int i =0;i < corridors.Count();i++)
            {               
                sql = "select * from Doors Where FromRoomId = ";
                sql = sql + corridors.ElementAt<Room>(i).Id;            
                
                SQLiteCommand commandDoors = new SQLiteCommand(sql, dbConnection);
                SQLiteDataReader readerDoors = commandDoors.ExecuteReader();
                while (readerDoors.Read())
                {              
                    Room room = new Room(Convert.ToInt64(readerDoors["ToRoomId"].ToString()));
                   // room.name = readerDoors["ToRoomId"].ToString();
                    Region region = new Region();
                    List < Room > rooms = new List<Room>();
                    rooms.Add(room);
                    region.rooms = rooms;
                    regions.Add(region);
                    //region.rooms.Add(corridors.ElementAt<Room>(i));
                }
            }      
                                      
            //关闭连接
            dbConnection.Close();
            return regions;
        }

    }
    public enum RoomPosition { overground = 1, underground = 2, semi_underground = 4 }
}

