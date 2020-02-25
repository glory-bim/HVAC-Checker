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
   
    class HVACFunction
    {
        private
            string m_archXdb;
            string m_hvacXdb;
            static  string m_TestPath;

        public HVACFunction(string Archxdb,string HVACxdb)
        {
            m_archXdb = Archxdb;
            m_hvacXdb = HVACxdb;
        }

        private static string GetCurrentPath(string dbName)
        {
            dynamic ProgramType = (new Program()).GetType();
            string currentDirectory = Path.GetDirectoryName(ProgramType.Assembly.Location);
            int iSub = currentDirectory.IndexOf("\\bin");
            currentDirectory = currentDirectory.Substring(0, iSub);
            m_TestPath = new DirectoryInfo(currentDirectory + dbName).FullName;
            return m_TestPath;
        }

        //1获取指定类型、指定名称、大于一定面积的地上或地下房间对象集合
        public static List<Room> GetRooms(string type, string name, double area, RoomPosition roomPosition)
        {
            List<Room> rooms = new List<Room>();

            string strDbName = "/建筑.GDB";
            string path = GetCurrentPath(strDbName);
            //如果不存在，则创建一个空的数据库,
            if (!System.IO.File.Exists(path))
                return rooms;   

            //创建一个连接
            string connectionstr = @"data source =" + path;
            SQLiteConnection dbConnection = new SQLiteConnection(connectionstr);            
            dbConnection.Open();          
            string sql = "select * from Spaces Where userLabel = ";
            sql = sql + "'"+type+"'";
            sql = sql + "and name = " ;
            sql = sql + "'" + name + "'";
            sql = sql + "and dArea > ";
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
            string strDbName = "/建筑.GDB";
            string path = GetCurrentPath(strDbName);
            //如果不存在，则创建一个空的数据库,
            if (!System.IO.File.Exists(path))
                return airterminals;

            //创建一个连接
            string connectionstr = @"data source =" + path;
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


                strDbName = "/机电.GDB";
                path = GetCurrentPath(strDbName);
                //创建一个连接
                connectionstr = @"data source =" + path;
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
            string strDbName = "/建筑.GDB";
            string path = GetCurrentPath(strDbName);
            //如果不存在，则创建一个空的数据库,
            if (!System.IO.File.Exists(path))
                return windows;

            //创建一个连接
            string connectionstr = @"data source =" + path;
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
        private static List<List<XDBCurve>> ConvertJsonToBoundaryLoops(String json)
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
        private static Polygon2D GetSpaceBBox(string boundaryLoops, string sSpaceId)
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

        private static PointInt getEllipsePoint(XDBEllipse xdbellipse, Double angle)
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
        static List<Fan> GetFanConnectingAirterminal(AirTerminal airterminal)
        {
            List<Fan> Fans = new List<Fan>();
            return Fans;
        }

        //4找到风机的排风口对象//怎么判断风口前端后端
        static List<AirTerminal> GetOutLetsOfFan(Fan fan)
        {
            List<AirTerminal> airTerminals = new List<AirTerminal>();
            string strDbName = "/rme_basic_sample_project.GDB";
            string path = GetCurrentPath(strDbName);
            //如果不存在，则创建一个空的数据库,
            if (!System.IO.File.Exists(path))
                return airTerminals;
            string connectionstr = @"data source =" + path;
            SQLiteConnection dbConnection = new SQLiteConnection(connectionstr);
            dbConnection.Open();
            //创建一个连接
            FindOutLets(dbConnection, fan.Id.ToString(), airTerminals);
            //关闭连接
            dbConnection.Close();
         
            return airTerminals; 
        }

        private static void FindOutLets(SQLiteConnection dbConnection, String strId, List<AirTerminal> inlets)
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
                    AirTerminal inlet = new AirTerminal(Convert.ToInt64(readerDucts["Id"].ToString()));
                    inlets.Add(inlet);
                }
                else
                {
                    FindOutletsByDuct(dbConnection, reader["linkElementId"].ToString(), inlets);
                    FindOutletsByDuct3t(dbConnection, reader["linkElementId"].ToString(), inlets);
                    FindOutletsByDuct4t(dbConnection, reader["linkElementId"].ToString(), inlets);
                    FindOutletsByDuctDampers(dbConnection, reader["linkElementId"].ToString(), inlets);
                }

            }
        }

        private static void FindOutletsByDuct(SQLiteConnection dbConnection, String strId, List<AirTerminal> ducts)
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
        private static void FindOutletsByDuct3t(SQLiteConnection dbConnection, String strId, List<AirTerminal> ducts)
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

        private static void FindOutletsByDuct4t(SQLiteConnection dbConnection, String strId, List<AirTerminal> ducts)
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

        private static void FindOutletsByDuctDampers(SQLiteConnection dbConnection, String strId, List<AirTerminal> ducts)
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

            return rooms;
        }


        //6找到一个风机的进风口对象集合
        public static List<AirTerminal> GetInletOfFan(Fan fan)
        {
            List<AirTerminal> inlets = new List<AirTerminal>();        
            string strDbName = "/rme_basic_sample_project.GDB";
            string path = GetCurrentPath(strDbName);
            //如果不存在，则创建一个空的数据库,
            if (!System.IO.File.Exists(path))
                return inlets;
            string connectionstr = @"data source =" + path;
            SQLiteConnection dbConnection = new SQLiteConnection(connectionstr);
            dbConnection.Open();



                FindInlets(dbConnection, fan.Id.ToString(), inlets);

                //关闭连接
                dbConnection.Close();



                return inlets;
        }
        private static void FindInlets(SQLiteConnection dbConnection, String strId, List<AirTerminal> inlets)
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
                    AirTerminal inlet = new AirTerminal(Convert.ToInt64(readerDucts["Id"].ToString()));
                    inlets.Add(inlet);                
                }
                else
                {
                    FindInletsByDuct(dbConnection, reader["linkElementId"].ToString(), inlets);
                    FindInletsByDuct3t(dbConnection, reader["linkElementId"].ToString(), inlets);
                    FindInletsByDuct4t(dbConnection, reader["linkElementId"].ToString(), inlets);
                    FindInletsByDuctDampers(dbConnection, reader["linkElementId"].ToString(), inlets);
                }

            }
        }

        private static void FindInletsByDuct(SQLiteConnection dbConnection, String strId, List<AirTerminal> ducts)
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
        private static void FindInletsByDuct3t(SQLiteConnection dbConnection, String strId, List<AirTerminal> ducts)
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

        private static void FindInletsByDuct4t(SQLiteConnection dbConnection, String strId, List<AirTerminal> ducts)
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

        private static void FindInletsByDuctDampers(SQLiteConnection dbConnection, String strId, List<AirTerminal> ducts)
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
            string strDbName = "/建筑.GDB";
            string path = GetCurrentPath(strDbName);
            //如果不存在，则创建一个空的数据库,
            if (!System.IO.File.Exists(path))
                return ducts;

            //创建一个连接
            string connectionstr = @"data source =" + path;
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


                strDbName = "/机电.GDB";
                path = GetCurrentPath(strDbName);
                //创建一个连接
                connectionstr = @"data source =" + path;
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
            string strDbName = "机电.GDB";
            string path = GetCurrentPath(strDbName);
            //如果不存在，则创建一个空的数据库,
            if (!System.IO.File.Exists(path))
                return room;

            //创建一个连接
            string connectionstr = @"data source =" + path;
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

                        strDbName = "建筑.GDB";
                        path = GetCurrentPath(strDbName);
                        //如果不存在，则创建一个空的数据库,
                        if (!System.IO.File.Exists(path))
                            return room;

                        //创建一个连接
                        string connectionArchstr = @"data source =" + path;
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
        dynamic ProgramType = (new Program()).GetType();
        string currentDirectory = Path.GetDirectoryName(ProgramType.Assembly.Location);
        int iSub = currentDirectory.IndexOf("\\bin");
        currentDirectory = currentDirectory.Substring(0, iSub);
        string path = new DirectoryInfo(currentDirectory + "/建筑.GDB").FullName;

        //如果不存在，则创建一个空的数据库,
        if (!System.IO.File.Exists(path))
            return dArea;

        //创建一个连接
        string connectionstr = @"data source =" + path;
        SQLiteConnection m_dbConnection = new SQLiteConnection(connectionstr);
        m_dbConnection.Open();
        string sql = "select Area from Windows Where Id = ";
        sql = sql + window.Id ;
        //sql = sql + "'" + window.GetID() + "'";

        SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
        SQLiteDataReader reader = command.ExecuteReader();
        long lArea = 0;
        string sArea = "";
        while (reader.Read())
            sArea= reader["Area"].ToString();   
            
        dArea = Convert.ToDouble(sArea);
        //关闭连接
        m_dbConnection.Close();
       
        return dArea;
    }

        //12找到属于某种类型的房间对象集合
        static List<Room> GetRooms(string roomType)
        {


            List<Room> rooms = new List<Room>();
            dynamic ProgramType = (new Program()).GetType();

            string currentDirectory = Path.GetDirectoryName(ProgramType.Assembly.Location);
            int iSub = currentDirectory.IndexOf("\\bin");
            currentDirectory = currentDirectory.Substring(0, iSub);
            string path = new DirectoryInfo(currentDirectory + "/建筑.GDB").FullName;

            //如果不存在，则创建一个空的数据库,
            if (!System.IO.File.Exists(path))
                return rooms;

            //创建一个连接
            string connectionstr = @"data source =" + path;
            SQLiteConnection m_dbConnection = new SQLiteConnection(connectionstr);
            m_dbConnection.Open();
            string sql = "select * from Spaces Where userLabel = ";
            sql = sql + "'" + roomType + "'";

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
  
            dynamic ProgramType = (new Program()).GetType();
            string currentDirectory = Path.GetDirectoryName(ProgramType.Assembly.Location);
            int iSub = currentDirectory.IndexOf("\\bin");
            currentDirectory = currentDirectory.Substring(0, iSub);
            string path = new DirectoryInfo(currentDirectory + "/建筑.GDB").FullName;

            //如果不存在，则创建一个空的数据库,
            if (!System.IO.File.Exists(path))
                return rooms;

            //创建一个连接
            string connectionstr = @"data source =" + path;
            SQLiteConnection m_dbConnection = new SQLiteConnection(connectionstr);
            m_dbConnection.Open();
            string sql = "select * from Spaces Where name like ";
            sql = sql + "'" + containedString + "'";

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
        static bool isOvergroundRoom(Room room)
        {

            bool isUp = true;

            return isUp;

        }
        //15获取所有楼层对象集合
        public static List<Floor> GetFloors()
        {
            List<Floor> floors = new List<Floor>();
            dynamic ProgramType = (new Program()).GetType();
            string currentDirectory = Path.GetDirectoryName(ProgramType.Assembly.Location);
            int iSub = currentDirectory.IndexOf("\\bin");
            currentDirectory = currentDirectory.Substring(0, iSub);
            string path = new DirectoryInfo(currentDirectory + "/建筑.GDB").FullName;

            //如果不存在，则创建一个空的数据库,
            if (!System.IO.File.Exists(path))
                return floors;

            //创建一个连接
            string connectionstr = @"data source =" + path;
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

        private static void FindDucts(SQLiteConnection dbConnection, String strId, List<Duct> ducts)
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

        private static void FindDuctsByDuct3t(SQLiteConnection dbConnection, String strId, List<Duct> ducts)
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

        private static void FindDuctsByDuct4t(SQLiteConnection dbConnection, String strId, List<Duct> ducts)
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

        private static void FindDuctsByDuctDuctDampers(SQLiteConnection dbConnection, String strId, List<Duct> ducts)
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
            string strDbName = "/测试.GDB";
            string path = GetCurrentPath(strDbName);
            //如果不存在，则创建一个空的数据库,
            if (!System.IO.File.Exists(path))
                return ducts;
            string connectionstr = @"data source =" + path;
            SQLiteConnection dbConnection = new SQLiteConnection(connectionstr);
            dbConnection.Open();
            //创建一个连接
            FindDucts(dbConnection,fan.Id.ToString(),ducts);

            //关闭连接
            dbConnection.Close();          
        return ducts;
    }
        //17判断是否风机所连风系统所有支路都连接了风口  //管堵
        static bool isAllBranchLinkingAirTerminal(Fan fan)
        {

            bool isLink = true;

            return isLink;

            }
        //获得防烟分区长边长度
        static double GetFireDistrictLength(FireDistrict fan)
        {
            double dLength = 0.0;
            return dLength;
        }                  
    }
    
}
enum RoomPosition { overground, underground, semi_underground }
