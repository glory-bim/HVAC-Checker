#define DEBUG
using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.IO;
using BCGL.Sharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

#if DEBUG
namespace HVAC_CheckEngine
{

  



    public class TreeNode
    {
        public long? Id { get; set; } = null;

        public long? strfireAirea { get; set; } = null;
        public List<long> StrfireAireas { get; set; }
        public int? iType { get; set; } = null;//风口 0 2t:2 3t：3，4T：4
        //父节点
        public TreeNode Parent { get; set; }
        //0左节点
        public TreeNode LeftNode { get; set; }
        //1直通节点
        public TreeNode DirectNode { get; set; }
        //2右节点   
        public TreeNode RightNode { get; set; }

        public TreeNode()
        {
            StrfireAireas = new List<long>();

        }



    }


   

    public class HVACFunction
    {
        public static string m_archXdbPath { set; get; }
        public static string m_hvacXdbPath { set; get; }

        private static string m_strLastId;
        private static List<string> m_listStrLastId;



        public static long? lastFireid { get; set; } = null;

        public HVACFunction(string Archxdb, string HVACxdb)
        {
            m_archXdbPath = Archxdb;
            m_hvacXdbPath = HVACxdb;
            m_strLastId = "";
            m_listStrLastId = new List<string>();
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


        public static bool GetGlobalData()
        {
            if (!System.IO.File.Exists(m_archXdbPath))
                return false;

            //创建一个连接
            string connectionstr = @"data source =" + m_archXdbPath;
            SQLiteConnection m_dbConnection = new SQLiteConnection(connectionstr);
            m_dbConnection.Open();      

            string sql = "select * from BuildingBCs Where key = ";
            sql = sql + "'" + "建筑名称" + "'";
              

            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            if (reader.Read())
            {            
                string strBuildingType  = reader["value"].ToString();          
                string[] chMsg = strBuildingType.Split(new char[] { '+' });
                if(chMsg.Length == 3)
                {
                    if(chMsg[0] == "公共建筑")
                    {
                        globalData.buildingType = chMsg[0];
                    }
                    else if(chMsg[0] == "居住建筑")
                    {
                        if(chMsg[1] == "住宅建筑")
                        {
                            if (chMsg[2] == "公寓")
                            {
                                globalData.buildingType = "公共建筑";
                            }
                            else 
                            {
                                globalData.buildingType = "住宅";
                            }
                        }
                        else if(chMsg[1] == "宿舍建筑")
                        {
                            globalData.buildingType = "公共建筑";
                        }
                    }                   
                }
                
                return true;
            }
            return false;
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


            string sql = "select * from Spaces Where userLabel like ";
            sql += "'%" + type + "%'";
            sql += " and name like ";
            sql += "'%" + name + "%'";
            sql += " and dArea > ";
            sql += area;
            SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                Room room = new Room(Convert.ToInt64(reader["Id"].ToString()));
                SetRoomPara(room);
                rooms.Add(room);
            }
            //关闭连接
            dbConnection.Close();
            return rooms;
        }

        private static AABB GetAABB(SQLiteDataReader readerElement, SQLiteConnection dbConnection)
        {
            string sTransformer = readerElement["transformer"].ToString();

            string sql = "select * from LODRelations where graphicElementId = ";
            sql = sql + readerElement["Id"].ToString();

            SQLiteCommand commandHVAC1 = new SQLiteCommand(sql, dbConnection);
            SQLiteDataReader readerHVAC1 = commandHVAC1.ExecuteReader();
            PointInt ptMin = new PointInt(0, 0, 0);
            PointInt ptMax = new PointInt(0, 0, 0);
            string strId = "";
            AABB aabb = new AABB(ptMin, ptMax, strId);
            if (readerHVAC1.Read())
            {
                sql = "select * from Geometrys where Id = ";
                sql = sql + readerHVAC1["geometryId"].ToString();
                SQLiteCommand commandHVACGeo = new SQLiteCommand(sql, dbConnection);
                SQLiteDataReader readerHVACGeo = commandHVACGeo.ExecuteReader();

                if (readerHVACGeo.Read())
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

                    aabb = GeometryFunction.GetGeometryBBox(geo, sTransformer);
                    return aabb;

                }
            }
            return aabb;
        }



        private static OBB GetOBB(SQLiteDataReader readerElement, SQLiteConnection dbConnection)
        {
            string sTransformer = readerElement["transformer"].ToString();

            string sql = "select * from LODRelations where graphicElementId = ";
            sql = sql + readerElement["Id"].ToString();

            SQLiteCommand commandHVAC1 = new SQLiteCommand(sql, dbConnection);
            SQLiteDataReader readerHVAC1 = commandHVAC1.ExecuteReader();
            PointInt pt0 = new PointInt(0, 0, 0);
            PointInt ptA = new PointInt(0, 0, 0);
            PointInt ptB = new PointInt(0, 0, 0);
            PointInt ptC = new PointInt(0, 0, 0);
            string strId = "";
            OBB obb = new OBB(pt0,ptA, ptB, ptC,0,0,0, strId);
            if (readerHVAC1.Read())
            {
                sql = "select * from Geometrys where Id = ";
                sql = sql + readerHVAC1["geometryId"].ToString();
                SQLiteCommand commandHVACGeo = new SQLiteCommand(sql, dbConnection);
                SQLiteDataReader readerHVACGeo = commandHVACGeo.ExecuteReader();

                if (readerHVACGeo.Read())
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

                    obb = GeometryFunction.GetGeometryOBB(geo, sTransformer);
                    return obb;

                }
            }
            return obb;
        }




        private static bool  GetRoomPolygon2D(Room room, ref Polygon2D poly)
        {
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
                poly = GetSpaceBBox(room.boundaryLoops, room.Id.ToString());

                sql = "select * from Storeys where  Id =  ";
                sql = sql + reader["storeyId"].ToString();
                SQLiteCommand command1 = new SQLiteCommand(sql, dbConnection);
                SQLiteDataReader reader1 = command1.ExecuteReader();

                if (reader1.Read())
                {
                    room.m_iStoryNo = Convert.ToInt32(reader1["storeyNo"].ToString());
                }
                dbConnection.Close();
                return true;
            }
            return false;
        }

        private static bool GetMovementJointPolygon2D(MovementJoint movementJoint, ref Polygon2D poly)
        {      
             poly = GetSpaceBBox(movementJoint.boundaryLoops, movementJoint.Id.ToString());
             return  poly != null;
        }

        private static void SetAirterminalPara(ref AirTerminal airTerminal, SQLiteDataReader readerAirTerminals)
        {
            airTerminal.airVelocity = Convert.ToDouble(readerAirTerminals["AirVelocity"].ToString());
            airTerminal.systemType = readerAirTerminals["SystemName"].ToString();
        }

        //2 判断房间是否有某种构件并返回构件对象
        //  Room    某种构件  风口
        public static List<AirTerminal> GetRoomContainAirTerminal(Room room)
        {
            List<AirTerminal> airterminals = new List<AirTerminal>();      
                       
            List<PointIntList> PointLists = new List<PointIntList>();
            PointLists.Add(new PointIntList() { new PointInt(0, 0, 0) });
            string sSpaceId = "0";

            Polygon2D poly = new Polygon2D(PointLists, sSpaceId); 
            if(!GetRoomPolygon2D(room, ref poly))
            {
                return airterminals;
            }
            int heighestStoreyNoOfRoom= GetHighestStoryNoOfRoom(room);
                //创建一个连接
            string  connectionstr = @"data source =" + m_hvacXdbPath;
            SQLiteConnection dbConnectionHVAC = new SQLiteConnection(connectionstr);
            dbConnectionHVAC.Open();
            string sql = "select * from AirTerminals where StoreyNo >="+room.m_iStoryNo+ " And StoreyNo <="+ heighestStoreyNoOfRoom;
            SQLiteCommand commandHVAC = new SQLiteCommand(sql, dbConnectionHVAC);
            SQLiteDataReader readerAirTerminals = commandHVAC.ExecuteReader();
            while (readerAirTerminals.Read())
            {
             
                    AirTerminal airTerminal = new AirTerminal(Convert.ToInt64(readerAirTerminals["Id"].ToString()));
                    SetAirterminalPara(ref airTerminal, readerAirTerminals);

                    AABB aabbAirTerminal = GetAABB(readerAirTerminals, dbConnectionHVAC);


                    PointInt pt = aabbAirTerminal.Center();
                    if (poly.Polygon2D_Contains_AABB(aabbAirTerminal))
                    {
                        airterminals.Add(airTerminal);
                    }
                    else if (Geometry_Utils_BBox.IsBBoxIntersectsBBox3D(poly, aabbAirTerminal))
                    {               
                        if(IsSameDirect(readerAirTerminals,poly, aabbAirTerminal))
                        {
                            airterminals.Add(airTerminal);
                        }
                    }


            }
            //关闭连接
            dbConnectionHVAC.Close();

                return airterminals;
       
     
        }

        public static List<GasMeter> GetRoomContainGasMeters(Room room)
        {
            List<GasMeter> GasMeters = new List<GasMeter>();

            List<PointIntList> PointLists = new List<PointIntList>();
            PointLists.Add(new PointIntList() { new PointInt(0, 0, 0) });
            string sSpaceId = "0";

            Polygon2D poly = new Polygon2D(PointLists, sSpaceId);
            if (!GetRoomPolygon2D(room, ref poly))
            {
                return GasMeters;
            }
            //创建一个连接
            string connectionstr = @"data source =" + m_hvacXdbPath;
            SQLiteConnection dbConnectionHVAC = new SQLiteConnection(connectionstr);
            dbConnectionHVAC.Open();
            int heighestStoreyNoOfRoom = GetHighestStoryNoOfRoom(room);
            //创建一个连接

            string sql = "select * from GasMeters where StoreyNo >=" + room.m_iStoryNo + " And StoreyNo <=" + heighestStoreyNoOfRoom;

            SQLiteCommand commandHVAC = new SQLiteCommand(sql, dbConnectionHVAC);
            SQLiteDataReader readerGasMeters = commandHVAC.ExecuteReader();
            while (readerGasMeters.Read())
            {
                 AABB aabbGasMeter = GetAABB(readerGasMeters, dbConnectionHVAC);
                 if (poly.Polygon2D_Contains_AABB(aabbGasMeter))
                 {
                    GasMeter gasMeter = new GasMeter(Convert.ToInt64(readerGasMeters["Id"].ToString()));
                    GasMeters.Add(gasMeter);
                 }
                   

            }
            //关闭连接
            dbConnectionHVAC.Close();
            return GasMeters;
        }


        public static List<HeatMeter> GetRoomContainHeatMeters(Room room)
        {
            List<HeatMeter> heatMeters = new List<HeatMeter>();

            List<PointIntList> PointLists = new List<PointIntList>();
            PointLists.Add(new PointIntList() { new PointInt(0, 0, 0) });
            string sSpaceId = "0";

            Polygon2D poly = new Polygon2D(PointLists, sSpaceId);
            if (!GetRoomPolygon2D(room, ref poly))
            {
                return heatMeters;
            }
            //创建一个连接
            string connectionstr = @"data source =" + m_hvacXdbPath;
            SQLiteConnection dbConnectionHVAC = new SQLiteConnection(connectionstr);
            dbConnectionHVAC.Open();
            int heighestStoreyNoOfRoom = GetHighestStoryNoOfRoom(room);
            //创建一个连接

            string sql = "select * from HeatMeters where StoreyNo >=" + room.m_iStoryNo + " And StoreyNo <=" + heighestStoreyNoOfRoom;

            SQLiteCommand commandHVAC = new SQLiteCommand(sql, dbConnectionHVAC);
            SQLiteDataReader readerHeatMeters = commandHVAC.ExecuteReader();
            while (readerHeatMeters.Read())
            {
                AABB aabbHeatMeter = GetAABB(readerHeatMeters, dbConnectionHVAC);
                if (poly.Polygon2D_Contains_AABB(aabbHeatMeter))
                {
                    HeatMeter heatMeter = new HeatMeter(Convert.ToInt64(readerHeatMeters["Id"].ToString()));
                    heatMeters.Add(heatMeter);
                }
            }
            //关闭连接
            dbConnectionHVAC.Close();
            return heatMeters;
        }


        public static List<WaterMeter> GetRoomContainWaterMeters(Room room)
        {
            List<WaterMeter> waterMeters = new List<WaterMeter>();

            List<PointIntList> PointLists = new List<PointIntList>();
            PointLists.Add(new PointIntList() { new PointInt(0, 0, 0) });
            string sSpaceId = "0";

            Polygon2D poly = new Polygon2D(PointLists, sSpaceId);
            if (!GetRoomPolygon2D(room, ref poly))
            {
                return waterMeters;
            }
            //创建一个连接
            string connectionstr = @"data source =" + m_hvacXdbPath;
            SQLiteConnection dbConnectionHVAC = new SQLiteConnection(connectionstr);
            dbConnectionHVAC.Open();

            //创建一个连接
            int heighestStoreyNoOfRoom = GetHighestStoryNoOfRoom(room);
            string sql = "select * from WaterMeters where StoreyNo >=" + room.m_iStoryNo + " And StoreyNo <=" + heighestStoreyNoOfRoom;


            SQLiteCommand commandHVAC = new SQLiteCommand(sql, dbConnectionHVAC);
            SQLiteDataReader readerWaterMeter = commandHVAC.ExecuteReader();
            while (readerWaterMeter.Read())
            {
                AABB aabbWaterMeter = GetAABB(readerWaterMeter, dbConnectionHVAC);
                if (poly.Polygon2D_Contains_AABB(aabbWaterMeter))
                {
                    WaterMeter waterMeter = new WaterMeter(Convert.ToInt64(readerWaterMeter["Id"].ToString()));
                    waterMeters.Add(waterMeter);
                }
            }
            //关闭连接
            dbConnectionHVAC.Close();
            return waterMeters;
        }

        private static void SetBoilerPara(Boiler boiler, SQLiteDataReader readerBoiler)
        {
           
            boiler.type = readerBoiler["BoilerType"].ToString();
            boiler.thermalPower = Convert.ToDouble(readerBoiler["ThermalPower"].ToString());
            boiler.ThermalEfficiency = Convert.ToDouble(readerBoiler["ThermalEfficiency"].ToString());
            boiler.mediaType = readerBoiler["HeatMediumType"].ToString();
            boiler.fuelType = readerBoiler["FuelType"].ToString();
            boiler.evaporationCapacity = Convert.ToDouble(readerBoiler["Evaporation"].ToString());
        }

        public static List<Boiler> GetRoomContainBoilers(Room room)
        {
            List<Boiler> boilers = new List<Boiler>();

            List<PointIntList> PointLists = new List<PointIntList>();
            PointLists.Add(new PointIntList() { new PointInt(0, 0, 0) });
            string sSpaceId = "0";

            Polygon2D poly = new Polygon2D(PointLists, sSpaceId);
            if (!GetRoomPolygon2D(room, ref poly))
            {
                return boilers;
            }
            //创建一个连接
            string connectionstr = @"data source =" + m_hvacXdbPath;
            SQLiteConnection dbConnectionHVAC = new SQLiteConnection(connectionstr);
            dbConnectionHVAC.Open();
            int heighestStoreyNoOfRoom = GetHighestStoryNoOfRoom(room);
            string sql = "select * from Boilers where StoreyNo >=" + room.m_iStoryNo + " And StoreyNo <=" + heighestStoreyNoOfRoom;

            SQLiteCommand commandHVAC = new SQLiteCommand(sql, dbConnectionHVAC);
            SQLiteDataReader readerBoiler = commandHVAC.ExecuteReader();
            while (readerBoiler.Read())
            {
                AABB aabbBoiler = GetAABB(readerBoiler, dbConnectionHVAC);
                if (poly.Polygon2D_Contains_AABB(aabbBoiler))
                {
                    Boiler boiler = new Boiler(Convert.ToInt64(readerBoiler["Id"].ToString()));
                    SetBoilerPara(boiler, readerBoiler);
                    boilers.Add(boiler);
                }
            }
            //关闭连接
            dbConnectionHVAC.Close();
            return boilers;
        }


        private static void SetWindowPara(Window window, SQLiteDataReader readerWindows)
        {

            window.isExternalWindow = Convert.ToBoolean(readerWindows["IsOutsideComponent"].ToString());
            window.area = Convert.ToDouble(readerWindows["Area"].ToString());
            window.effectiveArea= Convert.ToDouble(readerWindows["EffectiveArea"].ToString());
            window.sFaceOrient = readerWindows["sFacingOrientation"].ToString();


            if (!System.IO.File.Exists(m_archXdbPath))
                return;

            //创建一个连接
            string connectionstr = @"data source =" + m_archXdbPath;
            SQLiteConnection dbConnection = new SQLiteConnection(connectionstr);
            dbConnection.Open();

            string sql = "select * from Storeys where  Id =  ";
            sql = sql + readerWindows["storeyId"].ToString();
            SQLiteCommand command1 = new SQLiteCommand(sql, dbConnection);
            SQLiteDataReader reader1 = command1.ExecuteReader();

            if (reader1.Read())
            {
                window.m_iStoryNo = Convert.ToInt32(reader1["storeyNo"].ToString());
            }

            window.isExternalWindow = Convert.ToBoolean( readerWindows["IsOutsideComponent"].ToString());
            //window.isSmokeExhaustWindow            
           
             


        }

        public static List<Window> GetWindowsInRoom(Room room)
        {
            List<Window> windows = new List<Window>();
            List<PointIntList> PointLists = new List<PointIntList>();
            PointLists.Add(new PointIntList() { new PointInt(0, 0, 0) });
            string sSpaceId = "0";

            Polygon2D poly = new Polygon2D(PointLists, sSpaceId);
            if (!GetRoomPolygon2D(room, ref poly))
            {
                return windows;
            }

            string connectionstr = @"data source =" + m_archXdbPath;
            SQLiteConnection dbConnection = new SQLiteConnection(connectionstr);
            dbConnection.Open();
            int heighestStoreyNoOfRoom = GetHighestStoryNoOfRoom(room);
            string sql = null;
            List<long> storeyIdsOfRoom = new List<long>();
            for (int storeyNo = room.m_iStoryNo.Value; storeyNo <= heighestStoreyNoOfRoom; ++storeyNo)
            {
                sql = "select * from Storeys where  storeyNo =  ";
                sql += storeyNo.ToString();

                SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
                SQLiteDataReader reader = command.ExecuteReader();
                int highestStoryNostoryNo = room.m_iStoryNo.Value;
                if (reader.Read())
                {
                    storeyIdsOfRoom.Add(Convert.ToInt64(reader["Id"].ToString()));
                }
            }

            if (storeyIdsOfRoom.Count == 0)
                throw new ArgumentException();

            long lastStoreyId = storeyIdsOfRoom.Last();

            sql = "select * from Windows where";
            foreach(long storeyId in storeyIdsOfRoom)
            {
                sql += " storeyId=" + storeyId;
                if (storeyId != lastStoreyId)
                    sql += " Or";
            }

            SQLiteCommand commandWindows = new SQLiteCommand(sql, dbConnection);
            SQLiteDataReader readerWindows = commandWindows.ExecuteReader();
            while (readerWindows.Read())
            {
               
                    AABB aabbAirTerminal = GetAABB(readerWindows, dbConnection);


                    if (poly.Polygon2D_Contains_AABB(aabbAirTerminal))
                    {
                        Window window = new Window(Convert.ToInt64(readerWindows["Id"].ToString()));
                        SetWindowPara(window, readerWindows);
                        windows.Add(window);
                    }
                    else if (Geometry_Utils_BBox.IsBBoxIntersectsBBox3D(poly, aabbAirTerminal))
                    {
                        Window window = new Window(Convert.ToInt64(readerWindows["Id"].ToString()));
                        SetWindowPara(window, readerWindows);
                        windows.Add(window);
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
            m_listStrLastId = new List<string>();
            m_listStrLastId.Clear();
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

                if (!m_listStrLastId.Exists(x => x == reader["linkElementId"].ToString()))                  
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
                        m_listStrLastId.Add(strId);
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
            sql = sql +  " and userLabel = '风机出口管道'";         

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
                        SetAirterminalPara(ref inlet, readerDucts);
                        inlets.Add(inlet);
                    }
                    else
                    {
                        m_listStrLastId.Add(strId);
                        FindAirTerminals(dbConnection, reader["linkElementId"].ToString(), inlets);
                    }
                }
            }
        }

        //5找到大于一定长度的走道对象  double  “走道、走廊”    长度清华引擎 计算学院  张荷花// 表里加type//dLength单位为m
        public static List<Room> GetRoomsMoreThan(string roomType, double dLength)
        {
            List<Room> rooms = new List<Room>();

            if (!System.IO.File.Exists(m_archXdbPath))
                return rooms;

            //创建一个连接
            string connectionstr = @"data source =" + m_archXdbPath;
            SQLiteConnection dbConnection = new SQLiteConnection(connectionstr);
            dbConnection.Open();
            string sql = "select * from Spaces Where userLabel like ";
            sql +="'%" + roomType + "'%";

            SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                Room room = new Room(Convert.ToInt64(reader["Id"].ToString()));
                SetRoomPara(room);
               
                OBB obb = GetSpaceOBB(room.boundaryLoops, room.Id.ToString());
                double dLengthOBB = obb.GetLength();
                if (dLengthOBB*0.001 > dLength)
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
            sql = sql + " and userLabel = ";
            string strFanInletPipe = "风机进口管道";
            sql = sql + "'" + strFanInletPipe + "'";
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
                        SetAirterminalPara(ref inlet, readerAirTerminal);
                        inlets.Add(inlet);
                    }
                    else
                    {
                        m_listStrLastId.Add(strId);
                        FindAirTerminals(dbConnection, reader["linkElementId"].ToString(), inlets);
                    }
                }
            }
        }
        public static void FindAirTerminals(SQLiteConnection dbConnection, String strId, List<AirTerminal> inlets)
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
                        SetAirterminalPara(ref inlet, readerAirTerminal);
                        inlets.Add(inlet);
                    }
                    else
                    {
                        m_listStrLastId.Add(strId);
                        FindAirTerminals(dbConnection, reader["linkElementId"].ToString(), inlets);
                    }
                }
            }
        }


        //7找到穿越某些房间的风管对象集合  清华引擎 构件相交  包含
        public static Dictionary<Duct, List<PointInt>> GetDuctsCrossSpace(Room room)
        {
            Dictionary<Duct, List<PointInt>> ducts = new Dictionary<Duct, List<PointInt>>(new ElementEqualityComparer());
            if (!System.IO.File.Exists(m_archXdbPath))
                return ducts;

            List<PointIntList> PointLists = new List<PointIntList>();
            PointLists.Add(new PointIntList() { new PointInt(0, 0, 0) });
            string sSpaceId = "0";

            Polygon2D poly = new Polygon2D(PointLists, sSpaceId);
            if (!GetRoomPolygon2D(room, ref poly))
            {
                return ducts;
            }
            string connectionstr = @"data source =" + m_hvacXdbPath;
            SQLiteConnection dbConnectionHVAC = new SQLiteConnection(connectionstr);
            dbConnectionHVAC.Open();
            int heighestStoreyNoOfRoom = GetHighestStoryNoOfRoom(room);
            string sql = "select * from Ducts where StoreyNo >=" + room.m_iStoryNo + " And StoreyNo <=" + heighestStoreyNoOfRoom;

            SQLiteCommand commandHVAC = new SQLiteCommand(sql, dbConnectionHVAC);
            SQLiteDataReader readerDucts = commandHVAC.ExecuteReader();
            while (readerDucts.Read())
            {
                OBB obbDuct = GetOBB(readerDucts, dbConnectionHVAC);

                if (poly.IsPolygon2DIntersectsOBB(obbDuct))
                {
                    Duct duct = new Duct(Convert.ToInt64(readerDucts["Id"].ToString()));
                    SetDuctPara(readerDucts,duct);
                    List<PointInt> pointList = GetIntersectPoint(poly, duct);
                    if (!ducts.ContainsKey(duct))
                    {
                        ducts.Add(duct, new List<PointInt>());
                    }
                    ducts[duct].AddRange(pointList);
                }

            }

            //关闭连接
            dbConnectionHVAC.Close();

            return ducts;
        }
        //8找到穿越防火分区的风管对象集合  userlable
        public static Dictionary<Duct, List<PointInt>> GetDuctsCrossFireDistrict(FireCompartment fireDistrict)
        {
            Dictionary<Duct, List<PointInt>> ducts = new Dictionary<Duct, List<PointInt>>(new ElementEqualityComparer());
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
                sql = "select * from Storeys where  Id =  ";
                sql = sql + readerSpace["storeyId"].ToString();
                SQLiteCommand command1 = new SQLiteCommand(sql, dbConnection);
                SQLiteDataReader reader1 = command1.ExecuteReader();

                if (reader1.Read())
                {
                    fireDistrict.m_iStoryNo = Convert.ToInt32(reader1["storeyNo"].ToString());
                }

                //创建一个连接
                connectionstr = @"data source =" + m_hvacXdbPath;
                SQLiteConnection dbConnectionHVAC = new SQLiteConnection(connectionstr);
                dbConnectionHVAC.Open();
                double fireDistrictElevation = getRoomElevation(fireDistrict);
                double fireDistrictTopElevation = fireDistrictElevation + Convert.ToDouble(readerSpace["dHeight"].ToString());
                sql = "select * from Ducts where StartElevation<"+fireDistrictElevation + " And EndElevation>" + fireDistrictElevation;
                sql += " Or EndElevation>" + fireDistrictTopElevation+ " And StartElevation<" + fireDistrictTopElevation;
                sql+= "Or EndElevation<" + fireDistrictTopElevation + "And StartElevation > " + fireDistrictElevation;
                SQLiteCommand commandHVAC = new SQLiteCommand(sql, dbConnectionHVAC);
                SQLiteDataReader readerDucts = commandHVAC.ExecuteReader();
                while (readerDucts.Read())
                {
                    Duct duct = new Duct(Convert.ToInt64(readerDucts["Id"].ToString()));
                    SetDuctPara(readerDucts, duct);
                    OBB obbDuct = GetOBB(readerDucts, dbConnectionHVAC);
                    if (Convert.ToBoolean(readerDucts["IsVerticalDuct"].ToString()) && poly.Polygon2D_Contains_OBB(obbDuct))
                    {
                        
                        if (duct.StartElevation>fireDistrictElevation&&duct.EndElevation<fireDistrictElevation)
                            if (!ducts.ContainsKey(duct))
                            {
                                ducts.Add(duct, new List<PointInt>());
                            }
                        double elevationOfFireDistrict = getRoomElevation(fireDistrict);
                        double H_2 = duct.EndElevation.Value - elevationOfFireDistrict;
                        double H_1 = elevationOfFireDistrict - duct.StartElevation.Value;
                        double Zmax = duct.ptStart.Z > duct.ptEnd.Z ? duct.ptStart.Z : duct.ptEnd.Z;
                        double Zmin = duct.ptStart.Z < duct.ptEnd.Z ? duct.ptStart.Z : duct.ptEnd.Z;
                        double Z = (Zmax * H_1 + Zmin * H_2) / (H_1 + H_2);

                        ducts[duct].Add(new PointInt(duct.ptStart.X,duct.ptStart.Y,(int)Z));
                    }
                    else
                    {
                        if (poly.IsPolygon2DIntersectsOBB(obbDuct))
                        {
                            List<PointInt> pointList = GetIntersectPoint(poly, duct);
                            if (!ducts.ContainsKey(duct))
                            {
                                ducts.Add(duct, new List<PointInt>());
                            }
                            ducts[duct].AddRange(pointList);
                        }
                    }
                }
                dbConnectionHVAC.Close();
            }

            //关闭连接
            dbConnection.Close();
            return ducts;
        }

        //8找到穿越防火分区的风管对象集合  userlable
        public static Dictionary<Duct, List<PointInt>> GetAllDuctsCrossMoveJoint(MovementJoint movementJoint)
        {
            Dictionary<Duct, List<PointInt>> ducts = new Dictionary<Duct, List<PointInt>>(new ElementEqualityComparer());
            //如果不存在，则创建一个空的数据库,
            if (!System.IO.File.Exists(m_archXdbPath))
                return ducts;

            //创建一个连接
           
           
                Polygon2D poly = GetSpaceBBox(movementJoint.boundaryLoops, movementJoint.Id.ToString());
                

                //创建一个连接
                string connectionstr = @"data source =" + m_hvacXdbPath;
                SQLiteConnection dbConnectionHVAC = new SQLiteConnection(connectionstr);
                dbConnectionHVAC.Open();
                string sql = "select * from Ducts where StoreyNo=";
                sql += movementJoint.m_iStoryNo.ToString();
                SQLiteCommand commandHVAC = new SQLiteCommand(sql, dbConnectionHVAC);
                SQLiteDataReader readerDucts = commandHVAC.ExecuteReader();
                while (readerDucts.Read())
                {
                    OBB obbDuct = GetOBB(readerDucts, dbConnectionHVAC);
                    if (poly.IsPolygon2DIntersectsOBB(obbDuct))
                    {
                        Duct duct = new Duct(Convert.ToInt64(readerDucts["Id"].ToString()));
                        SetDuctPara(readerDucts, duct);
                        List<PointInt> pointList = GetIntersectPoint(poly, duct);
                        if (!ducts.ContainsKey(duct))
                        {
                            ducts.Add(duct, new List<PointInt>());
                        }
                        ducts[duct].AddRange(pointList);
                    }

                }


            //关闭连接
            dbConnectionHVAC.Close();
            return ducts;
        }
        //9找到穿越防火分隔处的变形缝两侧的风管集合  差变形缝对象

        //获得所有防火分区对象
        //对防火分区对象按楼层进行分类
        //获得所有变形缝对象的集合
        //依次遍历每一个变形缝对象
        //获得穿越变形缝的风管
        //依次遍历每一个风管
        //依次遍历与风管同层的防火分区对象
        //如果风管与防火分区相交
        //求出风管与防火分区的交点
        //如果交点是否在风管穿越变形位置的附近
        //将风管加入到风管集合中
        //返回风管集合。
        public static Dictionary<Duct,List<PointInt>> GetDuctsCrossMovementJointAndFireSide()
        {
            Dictionary<Duct, List<PointInt>> ductsCrossMovementJointAndFireSide = new Dictionary<Duct, List<PointInt>>(new ElementEqualityComparer());
            // 获得所有防火分区对象
            List<FireCompartment> fireCompartments = GetALLFireCompartment();
            //对防火分区对象按楼层进行分类
            Dictionary<int, List<FireCompartment>> fireCompartmentSortByStoreyNo = assistantFunctions.sortElementsByStoryNo(fireCompartments);
            //获得所有变形缝对象的集合
            List<MovementJoint> movementJoints = GetALLMovementJoints();
            //依次遍历每一个变形缝对象
            Dictionary<Duct, List<PointInt>> ductsCrossMoveJoint = new Dictionary<Duct, List<PointInt>>(new ElementEqualityComparer());
            foreach (MovementJoint movementJoint in movementJoints)
            {
                //获得穿越变形缝的风管
                ductsCrossMoveJoint.addDuctsToDictionary(GetAllDuctsCrossMoveJoint(movementJoint));
            }
            //依次遍历每一个风管
            foreach (KeyValuePair<Duct, List<PointInt>> pair in ductsCrossMoveJoint)
            {
                //依次遍历与风管同层的防火分区对象
               foreach (FireCompartment fireCompartment in fireCompartmentSortByStoreyNo[pair.Key.m_iStoryNo.Value])
               {
                    //求解风管与防火分区的交点
                    Polygon2D poly = GetSpaceBBox(fireCompartment.boundaryLoops, fireCompartment.Id.ToString());
                    List<PointInt> pointList = GetIntersectPoint(poly, pair.Key);

                    //依次遍历每一个交点，如果交点在风管穿越变形位置的附近
                    foreach (PointInt point in pointList)
                    {
                        if(pair.Value.Exists(x=>x.X==point.X&&x.Y==point.Y&&x.Z==point.Z))
                        {
                            //将风管加入到风管集合中
                            if (!ductsCrossMovementJointAndFireSide.ContainsKey(pair.Key))
                                ductsCrossMovementJointAndFireSide.Add(pair.Key, new List<PointInt>());
                            ductsCrossMovementJointAndFireSide[pair.Key].Add(point);
                        }
                    }
                }
            }
            //返回风管集合。
            return ductsCrossMovementJointAndFireSide;
        }


        private static void SetDuctPara(SQLiteDataReader ductReader,Duct duct)
        {
            string strVector = ductReader["DuctStartPoint"].ToString();
            int index = strVector.IndexOf(":");
            int index_s = strVector.LastIndexOf(",\"Y");
            string strX = strVector.Substring(index + 1, index_s - index - 1);

            double dX = Convert.ToDouble(strX);
            index = strVector.IndexOf("Y");
            index_s = strVector.LastIndexOf(",\"Z");
            string strY = strVector.Substring(index + 3, index_s - index - 3);
            double dY = Convert.ToDouble(strY);

            index = strVector.IndexOf("Z");

            index_s = strVector.Length;
            string strZ = strVector.Substring(index + 3, index_s - index - 4);
            double dZ = Convert.ToDouble(strY);


            duct.ptStart.X = Convert.ToInt32(dX);
            duct.ptStart.Y = Convert.ToInt32(dY);
            duct.ptStart.Z = Convert.ToInt32(dZ);


            strVector = ductReader["DuctEndPoint"].ToString();
            index = strVector.IndexOf(":");
            index_s = strVector.LastIndexOf(",\"Y");
            strX = strVector.Substring(index + 1, index_s - index - 1);

            dX = Convert.ToDouble(strX);
            index = strVector.IndexOf("Y");
            index_s = strVector.LastIndexOf(",\"Z");
            strY = strVector.Substring(index + 3, index_s - index - 3);
            dY = Convert.ToDouble(strY);

            index = strVector.IndexOf("Z");

            index_s = strVector.Length;
            strZ = strVector.Substring(index + 3, index_s - index - 4);
            dZ = Convert.ToDouble(strY);


            duct.ptEnd.X = Convert.ToInt32(dX);
            duct.ptEnd.Y = Convert.ToInt32(dY);
            duct.ptEnd.Z = Convert.ToInt32(dZ);

            duct.airVelocity = Convert.ToDouble(ductReader["Velocity"].ToString());
            duct.systemType = ductReader["SystemName"].ToString();
            duct.StartElevation= Convert.ToDouble(ductReader["StartElevation"].ToString());
            duct.EndElevation = Convert.ToDouble(ductReader["EndElevation"].ToString());
        }

        static List<PointInt>  GetIntersectPoint(Polygon2D polygon,Duct duct)
        {
            List<PointInt> ptList = new List<PointInt>();
            foreach ( PointIntList list in polygon.Points)
            {
                if(list.Count()>=2)
                {
                   
                    for (int i = 0;i<list.Count()-1;i++)
                    {
                       PointInt ptInter = new PointInt(0,0,0);

                        duct.ptEnd.Z = duct.ptStart.Z;
                        list[i].Z = duct.ptStart.Z;
                        list[i + 1].Z = duct.ptStart.Z;

                        if (PointInt.IsLineIntersectsLine2D(duct.ptStart, duct.ptEnd, list[i], list[i + 1]))
                        {
                            ptInter = PointInt.GetLinesIntersectionPoint_2D(duct.ptStart, duct.ptEnd, list[i], list[i + 1]);
                            ptList.Add(ptInter);
                        }
                        
                    }
                   
                }
               
            }

            return ptList;


        }




        private static bool IsSameDirect(SQLiteDataReader airterminalreader, Polygon2D roomPoly, AABB aabbTerminal )
        {
            string strVector = airterminalreader["NormalVector"].ToString();
            int index = strVector.IndexOf(":");
            int index_s = strVector.LastIndexOf(",\"Y");
            string strX = strVector.Substring(index + 1, index_s - index - 1);

            double dX = Convert.ToDouble(strX);
            index = strVector.IndexOf("Y");
            index_s = strVector.LastIndexOf(",\"Z");
            string strY = strVector.Substring(index + 3, index_s - index - 3);
            double dY = Convert.ToDouble(strY);

            index = strVector.IndexOf("Z");

            index_s = strVector.Length;
            string strZ = strVector.Substring(index + 3, index_s - index - 4);
            double dZ = Convert.ToDouble(strY);
            List<double> listVector = new List<double>();
            listVector.Add(dX);
            listVector.Add(dY);
            listVector.Add(dZ);

            List<double> listVectorAirterminalToRoom = new List<double>();
            dX = roomPoly.Center().X - aabbTerminal.Center().X;
            dY = roomPoly.Center().Y - aabbTerminal.Center().Y;
            dZ = roomPoly.Center().Z - aabbTerminal.Center().Z;
            listVectorAirterminalToRoom.Add(dX);
            listVectorAirterminalToRoom.Add(dY);
            listVectorAirterminalToRoom.Add(dZ);
            //風口方向標註反了所以同向點積為負
            double dDotProduct = listVector[0] * listVectorAirterminalToRoom[0] + listVector[1] * listVectorAirterminalToRoom[1] + listVector[2] * listVectorAirterminalToRoom[2];
            if (dDotProduct < 0)
            {
                return true;
            }
            else
            {
                return false;
            }                
        }



    

        //10获得构建所在房间的对象  几何 包含 遍历表都查

        public static Room GetRoomOfAirterminal(AirTerminal airTerminal)
        {

            List<Room> rooms = GetAllRooms();
            Room roomEmpty = new Room(-1);
            if (!System.IO.File.Exists(m_hvacXdbPath))
                return roomEmpty;

            //创建一个连接
            string connectionstr = @"data source =" + m_hvacXdbPath;
            SQLiteConnection dbConnection = new SQLiteConnection(connectionstr);
            dbConnection.Open();

            for (int i = 0; i < rooms.Count(); i++)
            {
                string sql = "select * from AirTerminals Where Id = ";
                sql = sql + airTerminal.Id;
                SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
                SQLiteDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    AABB aabbTerminal = GetAABB(reader, dbConnection);
                    List<PointIntList> PointLists = new List<PointIntList>();
                    PointLists.Add(new PointIntList() { new PointInt(0, 0, 0) });
                    string sSpaceId = "0";

                    Polygon2D poly = new Polygon2D(PointLists, sSpaceId);
                    if (!GetRoomPolygon2D(rooms[i], ref poly))
                    {
                        return roomEmpty;
                    }
                    if (rooms[i].m_iStoryNo == Convert.ToInt32(reader["StoreyNo"].ToString()))
                    {
                        if (poly.Polygon2D_Contains_AABB(aabbTerminal))
                        {
                            return rooms[i];
                        }
                        else if (Geometry_Utils_BBox.IsBBoxIntersectsBBox3D(poly, aabbTerminal))
                        {
                            if(IsSameDirect(reader, poly, aabbTerminal)) 
                            {
                                return rooms[i];
                            }

                        }
                    }                 

                }

            }

                                                               
            return roomEmpty;
            //关闭连接               
            
        }

        //11获得一个窗户对象的有效面积  差公式 xdb差参数  开启角度  开启方式
        public static double GetArea(Window window)
        {
            double dArea = 0.0;

            if (!System.IO.File.Exists(m_archXdbPath))
                return dArea;

            //创建一个连接
            string connectionstr = @"data source =" + m_archXdbPath;
            SQLiteConnection m_dbConnection = new SQLiteConnection(connectionstr);
            m_dbConnection.Open();
            string sql = "select Area from Windows Where Id = ";
            sql = sql + window.Id;

            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            string sArea = "";
            while (reader.Read())
                sArea = reader["Area"].ToString();

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
          //  string sql = "select * from Spaces Where CHARINDEX(";
           // sql = sql + "'" + roomType + "'";
           // sql = sql + ",name)> 0";


            string sql = "select * from Spaces Where name like";
            sql = sql + "'%" + roomType + "%'";


            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                Room room = new Room(Convert.ToInt64(reader["Id"].ToString()));
                SetRoomPara(room);
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

            string sql = "select * from Spaces Where name like ";
            sql += "'%" + containedString + "%'";


            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                Room room = new Room(Convert.ToInt64(reader["Id"].ToString()));
                SetRoomPara(room);
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

        private static void SetFloorPara(Floor floor, SQLiteDataReader readerFloor)
        {
            floor.m_iStoryNo = Convert.ToInt32(readerFloor["storeyNo"].ToString());
            floor.elevation = Convert.ToDouble(readerFloor["elevation"].ToString());
            floor.height = Convert.ToDouble(readerFloor["height"].ToString());
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
                SetFloorPara(floor, reader);
                floors.Add(floor);
            }
            return floors;
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
            m_listStrLastId = new List<string>();
            m_listStrLastId.Clear();
            //创建一个连接
            FindDucts(dbConnection, fan.Id.ToString(), ducts);

            //关闭连接
            dbConnection.Close();
            return ducts;
        }

        public static void FindDucts(SQLiteConnection dbConnection, String strId, List<Duct> ducts)
        {
            string sql = "select * from MepConnectionRelations Where mainElementId = ";
            sql += strId;
            SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                if (!m_listStrLastId.Exists(x => x == reader["linkElementId"].ToString()))
                {
                    sql = "select * from Ducts Where Id = ";
                    sql += reader["linkElementId"].ToString();

                    SQLiteCommand commandDucts = new SQLiteCommand(sql, dbConnection);
                    SQLiteDataReader readerDucts = commandDucts.ExecuteReader();
                    if (readerDucts.Read())
                    {
                        Duct duct = new Duct(Convert.ToInt64(readerDucts["Id"].ToString()));
                        SetDuctPara(readerDucts,duct);
                        ducts.Add(duct);
                        m_listStrLastId.Add(strId);
                        FindDucts(dbConnection, readerDucts["Id"].ToString(), ducts);
                    }
                    else
                    {
                        m_listStrLastId.Add(strId);
                        FindDucts(dbConnection, reader["linkElementId"].ToString(), ducts);
                    }
                }
            }
        }
        //17判断是否风机所连风系统所有支路都连接了风口  //管堵
        public static bool isAllBranchLinkingAirTerminal(Fan fan)
        {
            return IfFindAirTerminal3t4t(fan.Id.ToString());
        }        

        public static bool IfFindAirTerminal3t4t(string strId)
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
                    Duct3T duct3t = new Duct3T(-1);
                    if (GetDuct3T(reader, dbConnection, ref duct3t))
                    {
                        sql = "select * from MepConnectionRelations Where mainElementId = ";
                        sql += strId;
                        SQLiteCommand command3t = new SQLiteCommand(sql, dbConnection);
                        SQLiteDataReader reader3t = command3t.ExecuteReader();
                        bool bReturn = false;
                        while (reader3t.Read())
                        {
                           if( reader3t["linkElementId"].ToString()!= strId)
                            {
                                bReturn = IfFindAirTerminal(reader3t["linkElementId"].ToString());
                            }
                          
                        }
                        return bReturn;
                    }

                    Duct4T duct4t = new Duct4T(-1);
                    if (GetDuct4T(reader, dbConnection, ref duct4t))
                    {
                        sql = "select * from MepConnectionRelations Where mainElementId = ";
                        sql += strId;
                        SQLiteCommand command4t = new SQLiteCommand(sql, dbConnection);
                        SQLiteDataReader reader4t = command4t.ExecuteReader();
                        bool bReturn = false;
                        while (reader4t.Read())
                        {
                            if (reader4t["linkElementId"].ToString() != strId)
                            {
                                bReturn = IfFindAirTerminal(reader4t["linkElementId"].ToString());
                            }

                        }
                        return bReturn;

                    }
                                       

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

        //18获得防烟分区长边长度
        public static double GetSmokeCompartmentLength(SmokeCompartment smokeCompartment)
        {
            double dLength = 0.0;
            if (!System.IO.File.Exists(m_archXdbPath))
                return dLength;

            //创建一个连接
            string connectionstr = @"data source =" + m_archXdbPath;
            SQLiteConnection dbConnection = new SQLiteConnection(connectionstr);
            dbConnection.Open();
            string sql = "select * from Spaces where Id = ";
            sql += smokeCompartment.Id;
            SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                Room room = new Room(Convert.ToInt64(reader["Id"].ToString()));
                room.name = reader["name"].ToString();
                room.boundaryLoops = reader["boundaryLoops"].ToString();
                // Polygon2D poly = GetSpaceBBox(room.boundaryLoops, room.Id.ToString());

                OBB obb = GetSpaceOBB(room.boundaryLoops, room.Id.ToString());
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

            string sql = "select * from Spaces Where userLabel like ";
            sql += "'%走廊%'";
           
            SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                Room room = new Room(Convert.ToInt64(reader["Id"].ToString()));
                room.name = reader["Id"].ToString();
                corridors.Add(room);
            }
            //每个走廊在门表中找关联房间
            for (int i = 0; i < corridors.Count(); i++)
            {
                sql = "select * from Doors Where FromRoomId = ";
                sql = sql + corridors.ElementAt<Room>(i).Id;

                SQLiteCommand commandDoors = new SQLiteCommand(sql, dbConnection);
                SQLiteDataReader readerDoors = commandDoors.ExecuteReader();
                Region region = new Region();
                while (readerDoors.Read())
                {
                    Room room = new Room(Convert.ToInt64(readerDoors["ToRoomId"].ToString()));
                    SetRoomPara(room);
                    region.rooms.Add(room);                                 
                }
                regions.Add(region);
            }

            //关闭连接
            dbConnection.Close();
            return regions;
        }

        public static List<Room> GetConnectedRooms(Room room)
        {
            List<Room> rooms = new List<Room>();

            if (!System.IO.File.Exists(m_archXdbPath))
                return rooms;

            //创建一个连接
            string connectionstr = @"data source =" + m_archXdbPath;
            SQLiteConnection dbConnection = new SQLiteConnection(connectionstr);
            dbConnection.Open();

            //每个走廊在门表中找关联房间
            string sql = "select * from Doors Where FromRoomId = ";
            sql = sql + room.Id;

            SQLiteCommand commandDoors = new SQLiteCommand(sql, dbConnection);
            SQLiteDataReader readerDoors = commandDoors.ExecuteReader();
            while (readerDoors.Read())
            {
                Room roomConnect = new Room(Convert.ToInt64(readerDoors["ToRoomId"].ToString()));
                SetRoomPara(roomConnect);
                rooms.Add(roomConnect);
            }

            sql = "select * from Doors Where ToRoomId = ";
            sql = sql + room.Id;

            commandDoors = new SQLiteCommand(sql, dbConnection);
            readerDoors = commandDoors.ExecuteReader();
            while (readerDoors.Read())
            {
                Room roomConnect = new Room(Convert.ToInt64(readerDoors["FromRoomId"].ToString()));
                SetRoomPara(roomConnect);
                rooms.Add(roomConnect);
            }
            //关闭连接
            dbConnection.Close();
            return rooms;
        }


        public static void SetRoomPara(Room room)
        {           
            if (!System.IO.File.Exists(m_archXdbPath))
                return ;

            //创建一个连接
            string connectionstr = @"data source =" + m_archXdbPath;
            SQLiteConnection m_dbConnection = new SQLiteConnection(connectionstr);
            m_dbConnection.Open();
            string sql = "select * from Spaces Where Id =";
            sql = sql + room.Id;
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            if (reader.Read())
            {              
                room.name = reader["name"].ToString();
                room.m_dHeight = Convert.ToDouble(reader["dHeight"].ToString());
                room.m_dArea = Convert.ToDouble(reader["dArea"].ToString());
                room.m_dWidth = Convert.ToDouble(reader["dWidth"].ToString());
                room.m_iNumberOfPeople = Convert.ToInt32(reader["nNumberOfPeople"].ToString());
                room.boundaryLoops = reader["boundaryLoops"].ToString();
                if (reader["position"].Equals("地上房间"))
                    room.m_eRoomPosition = RoomPosition.overground;
                else if (reader["position"].Equals("地下室"))
                    room.m_eRoomPosition = RoomPosition.underground;
                else
                    room.m_eRoomPosition = RoomPosition.semi_underground;
                room.m_dVolume = room.m_dArea * room.m_dHeight;
                //room.m_dMaxlength
                //     room.m_dVolume
                //    room.m_eRoomPosition
                room.type = room.name;
                sql = "select * from Storeys where  Id =  ";
                sql = sql + reader["storeyId"].ToString();
                SQLiteCommand command1 = new SQLiteCommand(sql, m_dbConnection);
                SQLiteDataReader reader1 = command1.ExecuteReader();

                if (reader1.Read())
                {
                    room.m_iStoryNo = Convert.ToInt32(reader1["storeyNo"].ToString());
                }

            }

            m_dbConnection.Close();
        }

        public static void SetFireCompartmentPara(FireCompartment fireCompartment)
        {
            if (!System.IO.File.Exists(m_archXdbPath))
                return;

            //创建一个连接
            string connectionstr = @"data source =" + m_archXdbPath;
            SQLiteConnection m_dbConnection = new SQLiteConnection(connectionstr);
            m_dbConnection.Open();
            string sql = "select * from Spaces Where Id =";
            sql = sql + fireCompartment.Id.ToString();
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            if (reader.Read())
            {
               
                fireCompartment.boundaryLoops = reader["boundaryLoops"].ToString();
                
                sql = "select * from Storeys where  Id =  ";
                sql = sql + reader["storeyId"].ToString();
                SQLiteCommand command1 = new SQLiteCommand(sql, m_dbConnection);
                SQLiteDataReader reader1 = command1.ExecuteReader();

                if (reader1.Read())
                {
                    fireCompartment.m_iStoryNo = Convert.ToInt32(reader1["storeyNo"].ToString());
                }

            }

            m_dbConnection.Close();
        }

        public static void SetSmokeCompartmentPara(SmokeCompartment smokeCompartment)
        {
            if (!System.IO.File.Exists(m_archXdbPath))
                return;

            //创建一个连接
            string connectionstr = @"data source =" + m_archXdbPath;
            SQLiteConnection m_dbConnection = new SQLiteConnection(connectionstr);
            m_dbConnection.Open();
            string sql = "select * from Spaces Where Id =";
            sql = sql + smokeCompartment.Id.ToString();
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            if (reader.Read())
            {

                smokeCompartment.boundaryLoops = reader["boundaryLoops"].ToString();

                sql = "select * from Storeys where  Id =  ";
                sql = sql + reader["storeyId"].ToString();
                SQLiteCommand command1 = new SQLiteCommand(sql, m_dbConnection);
                SQLiteDataReader reader1 = command1.ExecuteReader();

                if (reader1.Read())
                {
                    smokeCompartment.m_iStoryNo = Convert.ToInt32(reader1["storeyNo"].ToString());
                }

            }

            m_dbConnection.Close();
        }

        //不知道 FromRoomId 和 ToRoomId 顺序所以  分别查询
        public static List<Door> GetDoorsBetweenTwoRooms(Room firstRoom, Room SecondRoom)
        {
            List<Door> doors = new List<Door>();
            if (!System.IO.File.Exists(m_archXdbPath))
                return doors;

            //创建一个连接
            string connectionstr = @"data source =" + m_archXdbPath;
            SQLiteConnection dbConnection = new SQLiteConnection(connectionstr);
            dbConnection.Open();

            //每个走廊在门表中找关联房间
            string sql = "select * from Doors Where FromRoomId = ";
            sql = sql + firstRoom.Id + " and ToRoomId = ";
            sql = sql + SecondRoom.Id;

            SQLiteCommand commandDoors = new SQLiteCommand(sql, dbConnection);
            SQLiteDataReader readerDoors = commandDoors.ExecuteReader();

            while (readerDoors.Read())
            {
                Door door = new Door(Convert.ToInt64(readerDoors["Id"].ToString()));
                doors.Add(door);
            }

            sql = "select * from Doors Where FromRoomId = ";
            sql = sql + SecondRoom.Id + " and ToRoomId = ";
            sql = sql + firstRoom.Id;

            SQLiteCommand commandDoorsTo = new SQLiteCommand(sql, dbConnection);
            SQLiteDataReader readerDoorsTo = commandDoorsTo.ExecuteReader();
            while (readerDoorsTo.Read())
            {
                Door door = new Door(Convert.ToInt64(readerDoorsTo["Id"].ToString()));
                if (!doors.Exists(x => x.Id == door.Id))
                {
                    doors.Add(door);
                }
            }

            //关闭连接
            dbConnection.Close();
            return doors;
        }




        public static List<Pipe> GetPipes(String systemName)
        {
            List<Pipe> pipes = new List<Pipe>();

            if (!System.IO.File.Exists(m_hvacXdbPath))
                return pipes;

            //创建一个连接
            string connectionstr = @"data source =" + m_archXdbPath;
            SQLiteConnection m_dbConnection = new SQLiteConnection(connectionstr);
            m_dbConnection.Open();
            string sql = "select * from MEPPipes Where SystemName =";
            sql = sql + systemName;

            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                Pipe pipe = new Pipe(Convert.ToInt64(reader["Id"].ToString()));
                pipes.Add(pipe);
            }
            return pipes;
        }

        public static List<AirTerminal> GetAirterminals()
        {
            List<AirTerminal> pipes = new List<AirTerminal>();

            if (!System.IO.File.Exists(m_hvacXdbPath))
                return pipes;

            //创建一个连接
            string connectionstr = @"data source =" + m_hvacXdbPath;
            SQLiteConnection m_dbConnection = new SQLiteConnection(connectionstr);
            m_dbConnection.Open();
            string sql = "select * from AirTerminals";


            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                AirTerminal pipe = new AirTerminal(Convert.ToInt64(reader["Id"].ToString()));
                SetAirterminalPara(ref pipe, reader);
                pipes.Add(pipe);
            }
            return pipes;
        }


        public static List<Wall> GetOutSideWalls()
        {
            List<Wall> pipes = new List<Wall>();

            if (!System.IO.File.Exists(m_archXdbPath))
                return pipes;

            //创建一个连接
            string connectionstr = @"data source =" + m_archXdbPath;
            SQLiteConnection m_dbConnection = new SQLiteConnection(connectionstr);
            m_dbConnection.Open();
            string sql = "select * from Walls where isSideWall = 1";


            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                Wall pipe = new Wall(Convert.ToInt64(reader["Id"].ToString()));
                pipes.Add(pipe);
            }
            return pipes;
        }


        public static void GetAirTerminalAABB(AABB aabb, string Id)
        {
            //创建一个连接
            string connectionstr = @"data source =" + m_hvacXdbPath;
            SQLiteConnection dbConnection = new SQLiteConnection(connectionstr);
            dbConnection.Open();
            string sql = "select * from AirTerminals Where Id = ";
            sql = sql + Id;
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
                    if (readerHVACGeo.Read())
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

                        aabb = GeometryFunction.GetGeometryBBox(geo, sTransformer);

                    }
                }
            }
        }

        public static void GetWallAABB(AABB aabb, string Id)
        {
            //创建一个连接
            string connectionstr = @"data source =" + m_archXdbPath;
            SQLiteConnection dbConnection = new SQLiteConnection(connectionstr);
            dbConnection.Open();
            string sql = "select * from Walls Where Id = ";
            sql = sql + Id;
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
                    if (readerHVACGeo.Read())
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

                        aabb = GeometryFunction.GetGeometryBBox(geo, sTransformer);

                    }
                }
            }
        }

        public static void GetRoomPolygon(Polygon2D poly, Room room)
        {
             poly = GetSpaceBBox(room.boundaryLoops, room.Id.ToString());

        }

        public static List<Room> GetAllRooms()
        {
            List<Room> rooms = new List<Room>();
            if (!System.IO.File.Exists(m_archXdbPath))
                return rooms;

            //创建一个连接
            string connectionstr = @"data source =" + m_archXdbPath;
            SQLiteConnection m_dbConnection = new SQLiteConnection(connectionstr);
            m_dbConnection.Open();
            string sql = "select * from Spaces";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                Room room = new Room(Convert.ToInt64(reader["Id"].ToString()));
                SetRoomPara(room);
                
                rooms.Add(room);
            }
            m_dbConnection.Close();
            return rooms;
        }

        public static List<Room> GetAllRoomsInCertainStorey(int storeyNo)
        {
            List<Room> rooms = new List<Room>();
            if (!System.IO.File.Exists(m_archXdbPath))
                return rooms;

            //创建一个连接
            string connectionstr = @"data source =" + m_archXdbPath;
            SQLiteConnection m_dbConnection = new SQLiteConnection(connectionstr);
            m_dbConnection.Open();

            string sql = "select Id from Storeys where storeyNo=";
            sql += storeyNo.ToString();
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            if (!reader.Read())
                return rooms;
            int storeyId= Convert.ToInt32(reader["Id"].ToString());


            sql = "select * from Spaces where storeyId=";
            sql += storeyId.ToString();

            command = new SQLiteCommand(sql, m_dbConnection);
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                Room room = new Room(Convert.ToInt64(reader["Id"].ToString()));
                SetRoomPara(room);
                rooms.Add(room);
            }

            return rooms;
        }

        public static List<Fan> GetAllFans()
        {
            List<Fan> fans = new List<Fan>();
            if (!System.IO.File.Exists(m_hvacXdbPath))
                return fans;

            //创建一个连接
            string connectionstr = @"data source =" + m_hvacXdbPath;
            SQLiteConnection m_dbConnection = new SQLiteConnection(connectionstr);
            m_dbConnection.Open();
            string sql = "select * from HVACFans";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                Fan room = new Fan(Convert.ToInt64(reader["Id"].ToString()));
                fans.Add(room);
            }

            return fans;
        }

        public static List<Boiler> GetAllBoilers()
        {
            List<Boiler> boilers = new List<Boiler>();
            if (!System.IO.File.Exists(m_hvacXdbPath))
                return boilers;

            //创建一个连接
            string connectionstr = @"data source =" + m_hvacXdbPath;
            SQLiteConnection m_dbConnection = new SQLiteConnection(connectionstr);
            m_dbConnection.Open();
            string sql = "select * from Boilers";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                Boiler boiler = new Boiler(Convert.ToInt64(reader["Id"].ToString()));
                SetBoilerPara(boiler, reader);
                boilers.Add(boiler);
            }

            return boilers;
        }
        private static void SetAbsorptionChillerPara(AbsorptionChiller absorptionChiller, SQLiteDataReader readerAbsorptionChiller)
        {
            absorptionChiller.coolingCoefficient = Convert.ToDouble(readerAbsorptionChiller["PerformanceRate"].ToString());
            absorptionChiller.heatingCoefficient = Convert.ToDouble(readerAbsorptionChiller["HeatingPerformanceRate"].ToString());
            absorptionChiller.m_iStoryNo = Convert.ToInt32(readerAbsorptionChiller["StoreyNo"].ToString());
        }

        public static List<AbsorptionChiller> GetAllAbsorptionChillers()
        {
            List<AbsorptionChiller> absorptionChillers = new List<AbsorptionChiller>();
            if (!System.IO.File.Exists(m_hvacXdbPath))
                return absorptionChillers;

            //创建一个连接
            string connectionstr = @"data source =" + m_hvacXdbPath;
            SQLiteConnection m_dbConnection = new SQLiteConnection(connectionstr);
            m_dbConnection.Open();
            string sql = "select * from AbsorptionChillers";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                AbsorptionChiller absorptionChiller = new AbsorptionChiller(Convert.ToInt64(reader["Id"].ToString()));
                SetAbsorptionChillerPara(absorptionChiller, reader);
                absorptionChillers.Add(absorptionChiller);
            }

            return absorptionChillers;
        }

        private static void SetChillerPara(Chiller chiller, SQLiteDataReader readerChiller)
        {
            chiller.capacity = Convert.ToDouble(readerChiller["CoolingCapacity"].ToString());
            chiller.coolingType = readerChiller["CoolingType"].ToString();
            chiller.COP = Convert.ToDouble(readerChiller["COP"].ToString());
            chiller.isFrequencyConversion = Convert.ToBoolean(readerChiller["IfFrequencyConversion"].ToString());
            chiller.type = readerChiller["ChillerType"].ToString();
            chiller.m_iStoryNo = Convert.ToInt32(readerChiller["StoreyNo"].ToString());
        }

        public static List<Chiller> GetAllChillers()
        {
            List<Chiller> chillers = new List<Chiller>();
            if (!System.IO.File.Exists(m_hvacXdbPath))
                return chillers;

            //创建一个连接
            string connectionstr = @"data source =" + m_hvacXdbPath;
            SQLiteConnection m_dbConnection = new SQLiteConnection(connectionstr);
            m_dbConnection.Open();
            string sql = "select * from Chillers";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                Chiller chiller = new Chiller(Convert.ToInt64(reader["Id"].ToString()));
                SetChillerPara(chiller, reader);
                chillers.Add(chiller);
            }

            return chillers;
        }

        private static void SetRoofTopAHUPara(RoofTopAHU roofTopAHU, SQLiteDataReader readerRoofTopAHU)
        {
            roofTopAHU.capacity = Convert.ToDouble(readerRoofTopAHU["CoolingCapacity"].ToString());
            roofTopAHU.coolingType = readerRoofTopAHU["CoolingType"].ToString();
            roofTopAHU.EER = Convert.ToDouble(readerRoofTopAHU["EER"].ToString());
            roofTopAHU.m_iStoryNo = Convert.ToInt32(readerRoofTopAHU["StoreyNo"].ToString());
        }
        public static List<RoofTopAHU> GetAllRoofTopAHUs()
        {
            List<RoofTopAHU> roofTopAHUs = new List<RoofTopAHU>();
            if (!System.IO.File.Exists(m_hvacXdbPath))
                return roofTopAHUs;

            //创建一个连接
            string connectionstr = @"data source =" + m_hvacXdbPath;
            SQLiteConnection m_dbConnection = new SQLiteConnection(connectionstr);
            m_dbConnection.Open();
            string sql = "select * from RoofTopAHUs";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                RoofTopAHU roofTopAHU  = new RoofTopAHU(Convert.ToInt64(reader["Id"].ToString()));
                SetRoofTopAHUPara(roofTopAHU, reader);
                roofTopAHUs.Add(roofTopAHU);
            }

            return roofTopAHUs;
        }

        private static void SetOutDoorUnitPara(OutDoorUnit outDoorUnit, SQLiteDataReader readerOutDoorUnit)
        {
            outDoorUnit.capacity = Convert.ToDouble(readerOutDoorUnit["CoolingCapacity"].ToString());
            outDoorUnit.coolingType = readerOutDoorUnit["CoolingType"].ToString();
            outDoorUnit.EER = Convert.ToDouble(readerOutDoorUnit["EER"].ToString());
            outDoorUnit.IPLV = Convert.ToDouble(readerOutDoorUnit["IPLV"].ToString());
            outDoorUnit.m_iStoryNo = Convert.ToInt32(readerOutDoorUnit["StoreyNo"].ToString());
        }

        public static List<OutDoorUnit> GetAllOutDoorUnits()
        {
            List<OutDoorUnit> outDoorUnits = new List<OutDoorUnit>();
            if (!System.IO.File.Exists(m_hvacXdbPath))
                return outDoorUnits;

            //创建一个连接
            string connectionstr = @"data source =" + m_hvacXdbPath;
            SQLiteConnection m_dbConnection = new SQLiteConnection(connectionstr);
            m_dbConnection.Open();
            string sql = "select * from OutDoorUnits";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                OutDoorUnit outDoorUnit = new OutDoorUnit(Convert.ToInt64(reader["Id"].ToString()));
                SetOutDoorUnitPara(outDoorUnit, reader);
                outDoorUnits.Add(outDoorUnit);
            }

            return outDoorUnits;
        }
 
        public static List<OutDoorUnit> GetAllVRVOutDoorUnits()
        {
            List<OutDoorUnit> outDoorUnits = new List<OutDoorUnit>();
            if (!System.IO.File.Exists(m_hvacXdbPath))
                return outDoorUnits;

            //创建一个连接
            string connectionstr = @"data source =" + m_hvacXdbPath;
            SQLiteConnection m_dbConnection = new SQLiteConnection(connectionstr);
            m_dbConnection.Open();
            string sql = "select * from OutDoorUnits";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read()&&reader["userLabel"].Equals("VRV"))
            {
                OutDoorUnit outDoorUnit = new OutDoorUnit(Convert.ToInt64(reader["Id"].ToString()));
                SetOutDoorUnitPara(outDoorUnit, reader);
                outDoorUnits.Add(outDoorUnit);
            }

            return outDoorUnits;
        }

  

        public static bool IsEquipmentChimneyHasFlexibleShortTube(Element equipment)
        {
            if (equipment == null)
                throw new ArgumentException("设备为空");

            //如果不存在，则创建一个空的数据库,
            if (!System.IO.File.Exists(m_hvacXdbPath))
                throw new FileNotFoundException();

            if (!(equipment is Boiler)&&!(equipment is AbsorptionChiller))
                throw new ArgumentException("设备不为锅炉或直燃机");

            string connectionstr = @"data source =" + m_hvacXdbPath;
            SQLiteConnection dbConnection = new SQLiteConnection(connectionstr);
            dbConnection.Open();
            string sql = "select * from MepConnectionRelations Where mainElementId = ";
            sql += equipment.Id.ToString();
            SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();

            string ChimneyId = string.Empty;
            while(reader.Read())
            {
                sql = "select * from Ducts Where Id = ";
                sql += reader["linkElementId"].ToString();
                SQLiteCommand commandDucts = new SQLiteCommand(sql, dbConnection);
                SQLiteDataReader readerDucst = commandDucts.ExecuteReader();
                if(readerDucst.Read()&& readerDucst["SystemType"].ToString().Contains("排风"))
                {
                    ChimneyId= reader["linkElementId"].ToString();
                    break;
                }
            }

            if (ChimneyId == string.Empty)
                return false;

            m_listStrLastId = new List<string>();
            m_listStrLastId.Add(equipment.Id.ToString());

            if (ChimneyId!=null)
            {
                if (FindFlexibleShortTube(dbConnection, ChimneyId) != null)
                    return true;
                
            }
            return false;
        }

        private static FlexibleShortTube FindFlexibleShortTube(SQLiteConnection dbConnection, String strId)
        {
            string sql = "select * from FlexibleShortTubes Where Id = "+strId.ToString();
            SQLiteCommand commandShortTubes = new SQLiteCommand(sql, dbConnection);
            SQLiteDataReader readerShortTubes = commandShortTubes.ExecuteReader();
            if (readerShortTubes.Read())
            {
                FlexibleShortTube flexibleShortTube = new FlexibleShortTube(Convert.ToInt64(readerShortTubes["Id"].ToString()));
                return flexibleShortTube;
            }
            else
            {
                m_listStrLastId.Add(strId);
                sql = "select * from MepConnectionRelations Where mainElementId = ";
                sql += strId;
                SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
                SQLiteDataReader reader = command.ExecuteReader();
                while (reader.Read() )
                {
                    if(!m_listStrLastId.Exists(x => x == reader["linkElementId"].ToString()))
                        return FindFlexibleShortTube(dbConnection, reader["linkElementId"].ToString());
                }
            }
            return null;
        }


        public static List<AssemblyAHU> GetAllAssemblyAHUs()
        {
            List<AssemblyAHU> assemblyAHUs = new List<AssemblyAHU>();
            if (!System.IO.File.Exists(m_hvacXdbPath))
                return assemblyAHUs;

            //创建一个连接
            string connectionstr = @"data source =" + m_hvacXdbPath;
            SQLiteConnection m_dbConnection = new SQLiteConnection(connectionstr);
            m_dbConnection.Open();
            string sql = "select * from AssemblyAHUs";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                AssemblyAHU assemblyAHU = new AssemblyAHU(Convert.ToInt64(reader["Id"].ToString()));
                // assemblyAHU.type = new AssemblyAHU(Convert.ToInt64(reader["Id"].ToString()));
                assemblyAHUs.Add(assemblyAHU);
            }

            return assemblyAHUs;
        }

        public static List<GasMeter> GetGasMeters()
        {
            List<GasMeter> fans = new List<GasMeter>();
            if (!System.IO.File.Exists(m_hvacXdbPath))
                return fans;

            //创建一个连接
            string connectionstr = @"data source =" + m_hvacXdbPath;
            SQLiteConnection m_dbConnection = new SQLiteConnection(connectionstr);
            m_dbConnection.Open();
            string sql = "select * from GasMeters";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                GasMeter room = new GasMeter(Convert.ToInt64(reader["Id"].ToString()));
                fans.Add(room);
            }

            return fans;
        }

        public static List<HeatMeter> GetHeatMeters()
        {
            List<HeatMeter> fans = new List<HeatMeter>();
            if (!System.IO.File.Exists(m_hvacXdbPath))
                return fans;

            //创建一个连接
            string connectionstr = @"data source =" + m_hvacXdbPath;
            SQLiteConnection m_dbConnection = new SQLiteConnection(connectionstr);
            m_dbConnection.Open();
            string sql = "select * from HeatMeters";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                HeatMeter room = new HeatMeter(Convert.ToInt64(reader["Id"].ToString()));
                fans.Add(room);
            }

            return fans;
        }

        public static List<WaterMeter> GetWaterMeters()
        {
            List<WaterMeter> fans = new List<WaterMeter>();
            if (!System.IO.File.Exists(m_hvacXdbPath))
                return fans;

            //创建一个连接
            string connectionstr = @"data source =" + m_hvacXdbPath;
            SQLiteConnection m_dbConnection = new SQLiteConnection(connectionstr);
            m_dbConnection.Open();
            string sql = "select * from WaterMeters";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                WaterMeter room = new WaterMeter(Convert.ToInt64(reader["Id"].ToString()));
                fans.Add(room);
            }

            return fans;
        }

        public static List<FlexibleShortTube> GetFlexibleShortTubesOfFan(Fan fan)
        {
            List<FlexibleShortTube> flexibleShortTubes = new List<FlexibleShortTube>();
            //如果不存在，则创建一个空的数据库,
            if (!System.IO.File.Exists(m_hvacXdbPath))
                return flexibleShortTubes;
            string connectionstr = @"data source =" + m_hvacXdbPath;
            SQLiteConnection dbConnection = new SQLiteConnection(connectionstr);
            dbConnection.Open();
            //创建一个连接



            string sql = "select * from MepConnectionRelations Where mainElementId = ";
            sql += fan.Id.ToString();
            SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {

                sql = "select * from FlexibleShortTubes Where Id = ";
                sql += reader["linkElementId"].ToString();

                SQLiteCommand commandDucts = new SQLiteCommand(sql, dbConnection);
                SQLiteDataReader readerDucts = commandDucts.ExecuteReader();
                if (readerDucts.Read())
                {
                    FlexibleShortTube duct = new FlexibleShortTube(Convert.ToInt64(readerDucts["Id"].ToString()));
                    flexibleShortTubes.Add(duct);
                }
            }

            //关闭连接
            dbConnection.Close();
            return flexibleShortTubes;
        }

        public static List<FlexibleShortTube> GetFlexibleShortTubesOfAssemblyAHUs(AssemblyAHU fan)
        {
            List<FlexibleShortTube> flexibleShortTubes = new List<FlexibleShortTube>();
            //如果不存在，则创建一个空的数据库,
            if (!System.IO.File.Exists(m_hvacXdbPath))
                return flexibleShortTubes;
            string connectionstr = @"data source =" + m_hvacXdbPath;
            SQLiteConnection dbConnection = new SQLiteConnection(connectionstr);
            dbConnection.Open();
            //创建一个连接
            string sql = "select * from MepConnectionRelations Where mainElementId = ";
            sql += fan.Id.ToString();
            SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {

                sql = "select * from FlexibleShortTubes Where Id = ";
                sql += reader["linkElementId"].ToString();

                SQLiteCommand commandDucts = new SQLiteCommand(sql, dbConnection);
                SQLiteDataReader readerDucts = commandDucts.ExecuteReader();
                if (readerDucts.Read())
                {
                    FlexibleShortTube duct = new FlexibleShortTube(Convert.ToInt64(readerDucts["Id"].ToString()));
                    flexibleShortTubes.Add(duct);
                }
            }


            //关闭连接
            dbConnection.Close();
            return flexibleShortTubes;
        }

        public static List<Duct> GetDucts(string strSystemName)
        {
            List<Duct> ducts = new List<Duct>();
            if (!System.IO.File.Exists(m_hvacXdbPath))
                return ducts;

            //创建一个连接
            string connectionstr = @"data source =" + m_hvacXdbPath;
            SQLiteConnection m_dbConnection = new SQLiteConnection(connectionstr);
            m_dbConnection.Open();
            string sql = "select * from Ducts Where SystemName like ";
            sql += "'%" + strSystemName + "%'";

            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                Duct duct = new Duct(Convert.ToInt64(reader["Id"].ToString()));
                SetDuctPara(reader, duct);
                ducts.Add(duct);
            }

            return ducts;
        }

        public static List<AirTerminal> GetAirterminals(string strSystemName)
        {
            List<AirTerminal> pipes = new List<AirTerminal>();

            if (!System.IO.File.Exists(m_hvacXdbPath))
                return pipes;

            //创建一个连接
            string connectionstr = @"data source =" + m_hvacXdbPath;
            SQLiteConnection m_dbConnection = new SQLiteConnection(connectionstr);
            m_dbConnection.Open();

            string sql = "select * from AirTerminals Where SystemName like ";
            sql +="'%" + strSystemName + "%'";

            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                AirTerminal airTerminal = new AirTerminal(Convert.ToInt64(reader["Id"].ToString()));
                SetAirterminalPara(ref airTerminal, reader);
                pipes.Add(airTerminal);
            }
            return pipes;
        }

        //获取所有防烟分区
        public static List<SmokeCompartment> GetSmokeCompartment(string sName)
        {
            List<SmokeCompartment> smokeCompartments = new List<SmokeCompartment>();
            if (!System.IO.File.Exists(m_archXdbPath))
                return smokeCompartments;

            //创建一个连接
            string connectionstr = @"data source =" + m_archXdbPath;
            SQLiteConnection dbConnection = new SQLiteConnection(connectionstr);
            dbConnection.Open();
            string csSmokeCompartment = "防烟分区";
            string sql = "select * from Spaces where name like ";
            sql += "'%" + sName + "%'";
            sql += " and  userLabel =  "+ "'" + csSmokeCompartment + "'";
            SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                SmokeCompartment smokeCompartment = new SmokeCompartment(Convert.ToInt64(reader["Id"].ToString()));
                SetSmokeCompartmentPara(smokeCompartment);
                smokeCompartments.Add(smokeCompartment);
            }
            //关闭连接
            dbConnection.Close();

            return smokeCompartments;
        }

        public static List<FireCompartment> GetFireCompartment(string sName)
        {
            List<FireCompartment> fireCompartments = new List<FireCompartment>();
            if (!System.IO.File.Exists(m_archXdbPath))
                return fireCompartments;

            //创建一个连接
            string connectionstr = @"data source =" + m_archXdbPath;
            SQLiteConnection dbConnection = new SQLiteConnection(connectionstr);
            dbConnection.Open();

            string sql = "select * from Spaces where name like ";
            sql += "'%" + sName + "%'";
            sql += "and  userLabel = '防火分区'";

            SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                FireCompartment room = new FireCompartment(Convert.ToInt64(reader["Id"].ToString()));
                SetFireCompartmentPara(room);
                fireCompartments.Add(room);
            }
            //关闭连接
            dbConnection.Close();

            return fireCompartments;
        }

        public static List<FireCompartment> GetALLFireCompartment()
        {
            List<FireCompartment> fireCompartments = new List<FireCompartment>();
            if (!System.IO.File.Exists(m_archXdbPath))
                return fireCompartments;

            //创建一个连接
            string connectionstr = @"data source =" + m_archXdbPath;
            SQLiteConnection dbConnection = new SQLiteConnection(connectionstr);
            dbConnection.Open();

            string sql = "select * from Spaces where userLabel = '防火分区'";

            SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                FireCompartment room = new FireCompartment(Convert.ToInt64(reader["Id"].ToString()));
                SetFireCompartmentPara(room);
                fireCompartments.Add(room);
            }
            //关闭连接
            dbConnection.Close();

            return fireCompartments;
        }

        private static void SetMovementJointPara(MovementJoint movementJoint, SQLiteDataReader reader)
        {
            movementJoint.boundaryLoops = reader["extendProperty"].ToString();
        }

        public static List<MovementJoint> GetALLMovementJoints()
        {
            List<MovementJoint> moveJoints = new List<MovementJoint>();
            if (!System.IO.File.Exists(m_archXdbPath))
                return moveJoints;

            string connectionstr = @"data source =" + m_archXdbPath;
            SQLiteConnection dbConnectionArch = new SQLiteConnection(connectionstr);
            dbConnectionArch.Open();
            string strBianxing = "变形缝";
            string sql = "select * from Proxys Where userLabel like ";
            sql += "'%" + strBianxing + "%'";
            SQLiteCommand commandArch = new SQLiteCommand(sql, dbConnectionArch);
            SQLiteDataReader readerProxys = commandArch.ExecuteReader();
            while (readerProxys.Read())
            {
                MovementJoint moveJoint = new MovementJoint(Convert.ToInt64(readerProxys["Id"].ToString()));
                SetMovementJointPara(moveJoint, readerProxys);
                moveJoints.Add(moveJoint);
            }
            //关闭连接
            dbConnectionArch.Close();

            return moveJoints;
        }

        public static List<Wall> GetAllWallsOfRoom(Room room)
        {
            List<Wall> walls = new List<Wall>();
            if (!System.IO.File.Exists(m_archXdbPath))
                return walls;

            //创建一个连接
            string connectionstr = @"data source =" + m_archXdbPath;
            SQLiteConnection dbConnection = new SQLiteConnection(connectionstr);
            dbConnection.Open();
            string sql = "select * from WallOfSpaceRelations where spaceId = ";
            sql = sql + room.Id;

            SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                Wall wall = new Wall(Convert.ToInt64(reader["wallId"].ToString()));
                walls.Add(wall);
            }
            //关闭连接
            dbConnection.Close();
            return walls;
        }
        
        public class Icp : IComparer<Floor>
        {        //按书名排序  
            public int Compare(Floor x, Floor y)
            {
                if (x.elevation > y.elevation)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
        }

        private static double getRoomElevation(Room room)
        {
            if (!System.IO.File.Exists(m_archXdbPath))
                throw new FileNotFoundException();

            //创建一个连接
            string connectionstr = @"data source =" + m_archXdbPath;
            SQLiteConnection dbConnection = new SQLiteConnection(connectionstr);
            dbConnection.Open();

            string sql = "select * from Storeys where  storeyNo =  ";
            sql += room.m_iStoryNo.ToString();

            SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
         

            if (reader.Read())
            {
                return Convert.ToDouble(reader["elevation"].ToString());
            }
            else
                throw new ArgumentException("房间楼层编号有误！");
        }

        public static int GetHighestStoryNoOfRoom(Room room)
        {
            if (!System.IO.File.Exists(m_archXdbPath))
                return room.m_iStoryNo.Value;

            //创建一个连接
            string connectionstr = @"data source =" + m_archXdbPath;
            SQLiteConnection dbConnection = new SQLiteConnection(connectionstr);
            dbConnection.Open();



   
            string sql = "select * from Storeys where  storeyNo =  ";
            sql += room.m_iStoryNo.ToString();

            SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            int highestStoryNostoryNo = room.m_iStoryNo.Value;

            double topElevationOfRoom = getRoomElevation(room) + room.m_dHeight.Value;
            double minHeightDiff = double.MaxValue;
                    
            List<Floor> floors = GetFloors();
            foreach(Floor floor in floors)
            {
               double HeightDiff = topElevationOfRoom - floor.elevation.Value;
               if (HeightDiff <= 0)
                   continue;
               else if(HeightDiff<minHeightDiff)
               {
                  minHeightDiff = HeightDiff;
                  highestStoryNostoryNo = floor.m_iStoryNo.Value;
               }
            }

    
            //关闭连接
            dbConnection.Close();
            return highestStoryNostoryNo;
        }


        public static FireCompartment GetFireCompartmentContainAirTerminal(AirTerminal airTerminal)
        {
            FireCompartment fireCompartment = new FireCompartment(1);

            if (!System.IO.File.Exists(m_archXdbPath))
                return fireCompartment;

            //创建一个连接
            string connectionstr = @"data source =" + m_archXdbPath;
            SQLiteConnection dbConnection = new SQLiteConnection(connectionstr);
            dbConnection.Open();
            string csSmokeCompartment = "防火分区";
            string sql = "select * from Spaces where userLabel =  ";
            sql += csSmokeCompartment;
            SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                fireCompartment.Id = Convert.ToInt64(reader["Id"].ToString());
                SetFireCompartmentPara(fireCompartment);
                Polygon2D poly = GetSpaceBBox(fireCompartment.boundaryLoops, fireCompartment.Id.ToString());
                // AABB aabbRoom = GetAABB(reader, dbConnection);
                //创建一个连接
                connectionstr = @"data source =" + m_hvacXdbPath;
                SQLiteConnection dbConnectionHVAC = new SQLiteConnection(connectionstr);
                dbConnectionHVAC.Open();
                sql = "select * from AirTerminals Where Id =";
                sql = sql + airTerminal.Id;
                SQLiteCommand commandHVAC = new SQLiteCommand(sql, dbConnectionHVAC);
                SQLiteDataReader readerAirTerminals = commandHVAC.ExecuteReader();
                if (readerAirTerminals.Read())
                {
                    if (fireCompartment.m_iStoryNo == Convert.ToInt32(readerAirTerminals["StoreyNo"].ToString()))
                    {
                        AABB aabbAirTerminal = GetAABB(reader, dbConnection);

                        PointInt pt = aabbAirTerminal.Center();
                        if (poly.Polygon2D_Contains_AABB(aabbAirTerminal))
                        {
                            return fireCompartment;
                        }
                        else if (Geometry_Utils_BBox.IsBBoxIntersectsBBox3D(poly, aabbAirTerminal))
                        {
                            if(IsSameDirect(readerAirTerminals,poly, aabbAirTerminal))
                                return fireCompartment;
                        }
                    }
                }                        
            }
            dbConnection.Close();
            return fireCompartment;
        }

        public static bool IsAirTermianlInFireDistrict(AirTerminal airTerminal, FireCompartment fireDistrict)
        {
            if (!System.IO.File.Exists(m_archXdbPath))
                return false;

 
                Polygon2D poly = GetSpaceBBox(fireDistrict.boundaryLoops, fireDistrict.Id.ToString());

                // AABB aabbFireDistrict = GetAABB(reader, dbConnection);

                //创建一个连接
                string connectionstr = @"data source =" + m_hvacXdbPath;
                SQLiteConnection dbConnectionHVAC = new SQLiteConnection(connectionstr);
                dbConnectionHVAC.Open();
                string sql = "select * from AirTerminals Where Id =";
                sql = sql + airTerminal.Id;
                SQLiteCommand commandHVAC = new SQLiteCommand(sql, dbConnectionHVAC);
                SQLiteDataReader readerAirTerminals = commandHVAC.ExecuteReader();
                while (readerAirTerminals.Read())
                {

                    if (fireDistrict.m_iStoryNo == Convert.ToInt32(readerAirTerminals["StoreyNo"].ToString()))
                    {
                        AABB aabbAirTerminal = GetAABB(readerAirTerminals, dbConnectionHVAC);

                        PointInt pt = aabbAirTerminal.Center();
                        if (poly.Polygon2D_Contains_AABB(aabbAirTerminal))
                        {
                            return true;
                        }
                        else if (Geometry_Utils_BBox.IsBBoxIntersectsBBox3D(poly, aabbAirTerminal))
                        {

                            if (IsSameDirect(readerAirTerminals, poly, aabbAirTerminal))
                                return true;
                            else
                                return false;

                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            dbConnectionHVAC.Close();
            return true;
        }
        public static List<SmokeCompartment> GetSmokeCompartmentsInRoom(Room room)
        {
            List<SmokeCompartment> smokeCompartments = new List<SmokeCompartment>();

            if (!System.IO.File.Exists(m_archXdbPath))
                return smokeCompartments;

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

                sql = "select * from Storeys where  Id =  ";
                sql = sql + reader["storeyId"].ToString();
                SQLiteCommand command1 = new SQLiteCommand(sql, dbConnection);
                SQLiteDataReader reader1 = command1.ExecuteReader();

                if (reader1.Read())
                {
                    room.m_iStoryNo = Convert.ToInt32(reader1["storeyNo"].ToString());
                }

                //AABB aabbRoom = GetAABB(reader, dbConnection);

                //创建一个连接
                connectionstr = @"data source =" + m_hvacXdbPath;
                SQLiteConnection dbConnectionHVAC = new SQLiteConnection(connectionstr);
                dbConnectionHVAC.Open();
                sql = "select * from Spaces where userLabel = 防烟分区 ";
                SQLiteCommand commandHVAC = new SQLiteCommand(sql, dbConnectionHVAC);
                SQLiteDataReader readerAirTerminals = commandHVAC.ExecuteReader();
                while (readerAirTerminals.Read())
                {
                    Room room1 = new Room(0);
                     room1.name = readerAirTerminals["name"].ToString();
                     room1.boundaryLoops = readerAirTerminals["boundaryLoops"].ToString();
                       Polygon2D polySmokeCompartment = GetSpaceBBox(room.boundaryLoops, room.Id.ToString());
                    sql = "select * from Storeys where  Id =  ";
                    sql = sql + reader["storeyId"].ToString();
                    SQLiteCommand command2 = new SQLiteCommand(sql, dbConnection);
                    SQLiteDataReader reader2 = command2.ExecuteReader();

                    if (reader2.Read())
                    {
                        room.m_iStoryNo = Convert.ToInt32(reader1["storeyNo"].ToString());
                    }
                    // AABB polySmokeCompartment = GetAABB(readerAirTerminals, dbConnection);
                    if(room.m_iStoryNo == room1.m_iStoryNo)
                    {                   
                        if (poly.Polygon2D_Contains_Polygon2D(polySmokeCompartment))
                        {
                            SmokeCompartment smokeCompartment = new SmokeCompartment(Convert.ToInt64(readerAirTerminals["Id"].ToString()));
                            SetSmokeCompartmentPara(smokeCompartment);
                            smokeCompartments.Add(smokeCompartment);
                        }
                        else if (Geometry_Utils_BBox.IsBBoxIntersectsBBox3D(poly, polySmokeCompartment))
                        {
                            SmokeCompartment smokeCompartment = new SmokeCompartment(Convert.ToInt64(readerAirTerminals["Id"].ToString()));
                            SetSmokeCompartmentPara(smokeCompartment);
                            smokeCompartments.Add(smokeCompartment);
                        }

                    }

                

                }
            }
            //关闭连接
            dbConnection.Close();

            return smokeCompartments;
        }

        public static bool IsSmokeCompartmentIntersectFireCompartment(SmokeCompartment smokeCompartment, FireCompartment fireCompartment)
        {  

            List<PointIntList> PointLists = new List<PointIntList>();
            PointLists.Add(new PointIntList() { new PointInt(0, 0, 0) });
            string sSpaceId = "0";

            Polygon2D polySmokeCompartment = new Polygon2D(PointLists, sSpaceId);

            Polygon2D polyfireCompartment = new Polygon2D(PointLists, sSpaceId);

            if (!GetRoomPolygon2D(smokeCompartment, ref polySmokeCompartment))
                {
                    return false;
                }

            if (!GetRoomPolygon2D(fireCompartment, ref polyfireCompartment))
            {
                return false;
            }
            //创建一个连接
            if(smokeCompartment.m_iStoryNo == fireCompartment.m_iStoryNo)
            {
                if (Geometry_Utils_BBox.IsBBoxIntersectsBBox3D(polySmokeCompartment, polyfireCompartment))            
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
         
            return false;
        }

        public static List<FireDamper> GetFireDamperOfDuct(Duct duct)
        {
            List<FireDamper> fireDampers = new List<FireDamper>();
            if (!System.IO.File.Exists(m_hvacXdbPath))
                return fireDampers;
            string connectionstr = @"data source =" + m_hvacXdbPath;
            SQLiteConnection dbConnection = new SQLiteConnection(connectionstr);
            dbConnection.Open();
            m_listStrLastId = new List<string>();
            FindFireDampersDirect(dbConnection, duct.Id.ToString(), fireDampers);
            //关闭连接
            dbConnection.Close();

            return fireDampers;
        }

        public static void FindFireDampersDirect(SQLiteConnection dbConnection, String strId, List<FireDamper> inlets)
        {
            string sql = "select * from MepConnectionRelations Where mainElementId = ";
            sql += strId;
            SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {               
                sql = "select * from DuctDampers Where Id = ";
                sql += reader["linkElementId"].ToString();

                SQLiteCommand commandAirterminal = new SQLiteCommand(sql, dbConnection);
                SQLiteDataReader readerAirTerminal = commandAirterminal.ExecuteReader();
                if (readerAirTerminal.Read())
                {
                    FireDamper inlet = new FireDamper(Convert.ToInt64(readerAirTerminal["Id"].ToString()));
                    inlets.Add(inlet);
                }                
            }
        }  

        public static List<Room> GetALLRoomsHaveFireDoor()
        {
            List<Room> rooms = new List<Room>();

            if (!System.IO.File.Exists(m_archXdbPath))
                return rooms;

            string connectionArchstr = @"data source =" + m_archXdbPath;
            SQLiteConnection dbConnectionArch = new SQLiteConnection(connectionArchstr);
            dbConnectionArch.Open();          
            string strUserLabel = "防火";
            string sql = "select * from Doors Where userLabel like ";
            sql += "'%" + strUserLabel + "%'";

            SQLiteCommand commandDoor = new SQLiteCommand(sql, dbConnectionArch);
            SQLiteDataReader readerDoor = commandDoor.ExecuteReader();

            while (readerDoor.Read())
            {
                Room room = new Room(Convert.ToInt64(readerDoor["FromRoomId"].ToString()));

                sql = "select * from Space where  Id =  ";
                sql = sql + readerDoor["FromRoomId"].ToString();
                SQLiteCommand commandSpace = new SQLiteCommand(sql, dbConnectionArch);
                SQLiteDataReader readerSpace = commandSpace.ExecuteReader();
                if (readerSpace.Read())
                {
                    room.name = readerSpace["name"].ToString();
                    room.m_dHeight = Convert.ToDouble(readerSpace["dHeight"].ToString());
                    room.m_dArea = Convert.ToDouble(readerSpace["dArea"].ToString());
                    room.m_iNumberOfPeople = Convert.ToInt32(readerSpace["nNumberOfPeople"].ToString());
                    //room.m_dMaxlength
                    //     room.m_dVolume
                    //    room.m_eRoomPosition
                    room.type = readerSpace["userLabel"].ToString();
                    sql = "select * from Storeys where  Id =  ";
                    sql = sql + readerSpace["storeyId"].ToString();
                    SQLiteCommand command1 = new SQLiteCommand(sql, dbConnectionArch);
                    SQLiteDataReader reader1 = command1.ExecuteReader();

                    if (reader1.Read())
                    {
                        room.m_iStoryNo = Convert.ToInt32(reader1["storeyNo"].ToString());
                    }

                }

                rooms.Add(room);

            }
            return rooms;
        }
       
        public static List<Duct> GetAllDuctsInRoom(Room room)
        {
            List<Duct> ducts = new List<Duct>();
            List<PointIntList> PointLists = new List<PointIntList>();
            PointLists.Add(new PointIntList() { new PointInt(0, 0, 0) });
            string sSpaceId = "0";

            Polygon2D poly = new Polygon2D(PointLists, sSpaceId);           

            if (!GetRoomPolygon2D(room, ref poly))
            {
                return ducts;
            }

           string sql = "select * from Ducts";
            string connectionstr = @"data source =" + m_hvacXdbPath;
            SQLiteConnection dbConnection = new SQLiteConnection(connectionstr);
            dbConnection.Open();
            SQLiteCommand commandDucts = new SQLiteCommand(sql, dbConnection);
                SQLiteDataReader readerDucts = commandDucts.ExecuteReader();
                while (readerDucts.Read())
                {

                if(room.m_iStoryNo == Convert.ToInt32(readerDucts["storeyNo"].ToString()))
                {
                    AABB aabbAirTerminal = GetAABB(readerDucts, dbConnection);
                    if (Geometry_Utils_BBox.IsBBoxIntersectsBBox3D(poly, aabbAirTerminal))
                    {
                        Duct duct = new Duct(Convert.ToInt64(readerDucts["Id"].ToString()));                     
                        SetDuctPara(readerDucts, duct);
                        ducts.Add(duct);
                    }
                }
            }
              
       
            //关闭连接
            dbConnection.Close();
            return ducts;
        }
               
        public static List<Duct> GetAllVerticalDuctConnectedToDuct(Duct duct)
        {
            List<Duct> ducts = new List<Duct>();
            //如果不存在，则创建一个空的数据库,
            if (!System.IO.File.Exists(m_hvacXdbPath))
                return ducts;
            string connectionstr = @"data source =" + m_hvacXdbPath;
            SQLiteConnection dbConnection = new SQLiteConnection(connectionstr);
            dbConnection.Open();
            m_listStrLastId = new List<string>();
            m_listStrLastId.Clear();
            //创建一个连接
            FindVerticalDucts(dbConnection, duct.Id.ToString(), ducts);

            //关闭连接
            dbConnection.Close();
            return ducts;
        }

        public static void FindVerticalDucts(SQLiteConnection dbConnection, String strId, List<Duct> ducts)
        {
            string sql = "select * from MepConnectionRelations Where mainElementId = ";
            sql += strId;
            SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                if (!m_listStrLastId.Exists(x => x == reader["linkElementId"].ToString()))
                {
                    sql = "select * from Ducts Where Id = ";
                    sql += reader["linkElementId"].ToString();

                    SQLiteCommand commandDucts = new SQLiteCommand(sql, dbConnection);
                    SQLiteDataReader readerDucts = commandDucts.ExecuteReader();
                    if (readerDucts.Read())
                    {
                        Duct duct = new Duct(Convert.ToInt64(readerDucts["Id"].ToString()));
                        SetDuctPara(readerDucts, duct);
                        if(Convert.ToBoolean(readerDucts["IsVerticalDuct"].ToString()))
                        {
                            ducts.Add(duct);
                            m_listStrLastId.Add(strId);
                            FindVerticalDucts(dbConnection, readerDucts["Id"].ToString(), ducts);
                        }                        
                    }
                    else
                    {
                        m_listStrLastId.Add(strId);
                        FindVerticalDucts(dbConnection, reader["linkElementId"].ToString(), ducts);
                    }
                }
            }
        }

      
       
        public static List<Duct> GetBranchDamperDucts()
        {
            List<Duct> ducts = new List<Duct>();
            List<AirTerminal> airtermials = GetAirterminals("排烟");
            //建立樹結構
            TreeNode lastNode = new TreeNode();
            List<TreeNode> LastNodes = new List<TreeNode>();
            foreach (AirTerminal airterminal in airtermials)
            {
                //如果這個樹有這個風口節點，下一個風口向上構建樹
                if (PreOrderFind(lastNode, airterminal)) continue;
                TreeNode newNode = new TreeNode();
                newNode.Id = airterminal.Id;
                newNode.iType = 0;

                string strId = Convert.ToString(airtermials[0].Id);

                if (!System.IO.File.Exists(m_hvacXdbPath))
                    return ducts;
                string connectionstr = @"data source =" + m_hvacXdbPath;
                SQLiteConnection dbConnection = new SQLiteConnection(connectionstr);
                dbConnection.Open();
                //创建一个连接                         
                LastNodes.Add(newNode);               

                StructPareTree(strId, dbConnection, ref newNode, LastNodes);
            }
            //找到根節點
            TreeNode root = new TreeNode();
           if( FindRoot(LastNodes, ref root))
            {
                //从根节点 标记子节点防火分区
                PreOrderAddFireArea(root);

                //从根节点 找到子节点为风口的所有节点

                List<TreeNode> airTerminalNodes = new List<TreeNode>();
                PreOrderAirterminalNode(airTerminalNodes, root);
                foreach (TreeNode airterminal in airTerminalNodes)
                {
                    List<TreeNode> ductNodes = new List<TreeNode>();
                    TreeNode node3T4T = GetParentEquel3T4T(airterminal, ductNodes);

                    if (node3T4T != null)
                    {
                        if (node3T4T.StrfireAireas.Count() > node3T4T.StrfireAireas.Count())
                        {
                            foreach (TreeNode ductnode in ductNodes)
                            {
                                Duct duct = new Duct((long)ductnode.Id);
                                ducts.Add(duct);
                            }
                        }
                    }
                }
            }

         

            return ducts;
        }



        private static void StructPareTree(string strId, SQLiteConnection dbConnection, ref TreeNode newNode, List<TreeNode> LastNodes)
        {            
            string sql = "select * from MepConnectionRelations Where mainElementId = ";
            sql += strId;
            SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {                       
                if (!LastNodes.Exists(x => x.Id == Convert.ToInt64(reader["linkElementId"].ToString())))
                { 
                    int index = sql.IndexOf("=");                                     
                    string strMainId = sql.Substring(index + 1, (sql.Length- index - 1));
                    TreeNode lastNode = LastNodes.Find(x => x.Id == Convert.ToInt64(strMainId));
                    newNode = new TreeNode();
                    AirTerminal airterminal = new AirTerminal(-1);
                    Duct duct = new Duct(-1);
                    DuctElbow ductElbow = new DuctElbow(-1);
                    DuctReducer ductReducer = new DuctReducer(-1);
                    DuctDamper ductDamper = new DuctDamper(-1);
                    DuctSoft ductSoft = new DuctSoft(-1);
                    FlexibleShortTube flexibleShortTube = new FlexibleShortTube(-1);
                    Duct3T duct3t = new Duct3T(-1);
                    Duct4T duct4t = new Duct4T(-1);

                    bool bGet = false;
                    if (GetDuct(reader, dbConnection, ref duct))
                    {
                        bGet = true;
                        newNode.Id = duct.Id;

                        if (lastNode.iType == 3)
                        {
                            string sqll = "select * from MepConnectionRelations Where MainElementId = ";
                            strId = Convert.ToString(lastNode.Id);
                            sqll += strId;

                            sqll += " and  linkElementId = ";
                            strId = Convert.ToString(newNode.Id);
                            sqll += strId;

                            SQLiteCommand commandUserLabel = new SQLiteCommand(sqll, dbConnection);
                            SQLiteDataReader readerUserLabel = commandUserLabel.ExecuteReader();
                            string strMain = "";
                            if (readerUserLabel.Read())
                            {
                                strMain = readerUserLabel["userLabel"].ToString();
                            }
                            switch (strMain)
                            {
                                case "直管":
                                    lastNode.DirectNode = newNode;
                                    newNode.Parent = lastNode;
                                    if (newNode != null && newNode.Id != null)
                                    {

                                    
                                        LastNodes.Add(newNode);
                                        bool bGett = false;
                                        StructSonTree(Convert.ToString(newNode.Id), dbConnection, ref newNode,  LastNodes);
                                        if(bGett)
                                            lastNode = newNode;
                                        continue;
                                    }
                                    break;
                                case "主管":
                                    lastNode.Parent = newNode;
                                    newNode.DirectNode = lastNode;
                                    break;
                                case "支管":
                                    lastNode.LeftNode = newNode;
                                    newNode.Parent = lastNode;
                                    if (newNode != null && newNode.Id != null)
                                    {
                                        LastNodes.Add(newNode);
                                        bool bGettt = false;
                                        StructSonTree(Convert.ToString(newNode.Id), dbConnection, ref newNode, LastNodes);
                                        if (bGettt)
                                            lastNode = newNode;
                                        continue;
                                    }
                                    break;
                            }

                        }
                        else
                        {
                            newNode.DirectNode = lastNode;
                            lastNode.Parent = newNode;
                        }

                        newNode.iType = 2;
                        long longId = (long)duct.Id;
                        newNode.strfireAirea = GetSmokeCompartmentOfElement(longId, "Ducts").Id;
                    }
                    else if (GetDuctElbow(reader, dbConnection, ref ductElbow))
                    {
                        bGet = true;
                        newNode.Id = ductElbow.Id;
                        newNode.DirectNode = lastNode;
                        lastNode.Parent = newNode;
                        newNode.iType = 2;
                        long longId = (long)ductElbow.Id;
                        newNode.strfireAirea = GetSmokeCompartmentOfElement(longId, "DuctElbows").Id;
                    }
                    else if (GetDuctReducer(reader, dbConnection, ref ductReducer))
                    {
                        bGet = true;
                        newNode.Id = ductReducer.Id;
                        newNode.DirectNode = lastNode;
                        lastNode.Parent = newNode;
                        newNode.iType = 2;
                        long longId = (long)ductReducer.Id;
                        newNode.strfireAirea = GetSmokeCompartmentOfElement(longId, "DuctReducers").Id;
                    }
                    else if (GetDuctDamper(reader, dbConnection, ref ductDamper))
                    {
                        bGet = true;
                        newNode.Id = ductDamper.Id;
                        newNode.DirectNode = lastNode;
                        lastNode.Parent = newNode;
                        newNode.iType = 2;
                        long longId = (long)ductDamper.Id;
                        newNode.strfireAirea = GetSmokeCompartmentOfElement(longId, "DuctDampers").Id;
                    }
                    else if (GetDuctSoft(reader, dbConnection, ref ductSoft))
                    {
                        bGet = true;
                        newNode.Id = ductSoft.Id;
                        newNode.DirectNode = lastNode;
                        lastNode.Parent = newNode;
                        newNode.iType = 2;
                        long longId = (long)ductSoft.Id;
                        newNode.strfireAirea = GetSmokeCompartmentOfElement(longId, "DuctSofts").Id;
                    }
                    else if (GetFlexibleShortTube(reader, dbConnection, ref flexibleShortTube))
                    {
                        bGet = true;
                        newNode.Id = flexibleShortTube.Id;
                        newNode.DirectNode = lastNode;
                        lastNode.Parent = newNode;
                        newNode.iType = 2;
                        long longId = (long)flexibleShortTube.Id;
                        newNode.strfireAirea = GetSmokeCompartmentOfElement(longId, "FlexibleShortTubes").Id;
                    }
                    else if (GetDuct3T(reader, dbConnection, ref duct3t))
                    {
                        bGet = true;
                        newNode.iType = 3;
                        newNode.Id = duct3t.Id;
                        long longId = (long)duct3t.Id;
                        newNode.strfireAirea = GetSmokeCompartmentOfElement(longId, "Duct3Ts").Id;
                        lastNode.Parent = newNode;
                        sql = "select * from MepConnectionRelations Where MainElementId = ";
                        strId = Convert.ToString(newNode.Id);
                        sql += strId;

                        sql += " and  linkElementId = ";
                        strId = Convert.ToString(lastNode.Id);
                        sql += strId;

                        SQLiteCommand commandUserLabel = new SQLiteCommand(sql, dbConnection);
                        SQLiteDataReader readerUserLabel = commandUserLabel.ExecuteReader();
                        string strMain = "";
                        if (readerUserLabel.Read())
                        {
                            strMain = readerUserLabel["userLabel"].ToString();
                        }
                        switch (strMain)
                        {
                            case "直管":
                                newNode.DirectNode = lastNode;

                                break;
                            case "主管":
                                newNode.Parent = lastNode.LeftNode;
                                break;
                            case "支管":
                                newNode.LeftNode = lastNode;

                                break;
                        }

                    }
                    else if (GetDuct4T(reader, dbConnection, ref duct4t))
                    {
                        bGet = true;
                        newNode.iType = 4;
                        newNode.Id = duct4t.Id;
                        lastNode.Parent = newNode;
                        sql = "select * from MepConnectionRelations Where MainElementId = ";
                        strId = Convert.ToString(newNode.Id);
                        sql += strId;

                        sql += "and linkElementId = ";
                        strId = Convert.ToString(lastNode.Id);
                        sql += strId;

                        SQLiteCommand commandUserLabel = new SQLiteCommand(sql, dbConnection);
                        SQLiteDataReader readerUserLabel = commandUserLabel.ExecuteReader();
                        string strMain = "";
                        if (readerUserLabel.Read())
                        {
                            strMain = readerUserLabel["userLabel"].ToString();
                        }
                        switch (strMain)
                        {
                            case "直管":
                                newNode.DirectNode = lastNode;
                                break;
                            case "主管":
                                newNode.LeftNode = lastNode;
                                break;
                            case "支管":
                                newNode.RightNode = lastNode;
                                break;
                            case "支管1":
                                newNode.LeftNode = lastNode;
                                break;
                        }

                        long longId = (long)duct4t.Id;
                        newNode.strfireAirea = GetSmokeCompartmentOfElement(longId, "Duct4Ts").Id;
                    }

                    if (newNode != null && newNode.Id != null)
                    {                                      
                        LastNodes.Add(newNode);                   
                        StructPareTree(Convert.ToString(newNode.Id), dbConnection, ref newNode, LastNodes);                            
                    }
                }
            }

            
        }

        private static void StructSonTree(string strId, SQLiteConnection dbConnection, ref TreeNode newNode, List<TreeNode> LastNodes)
        {
            string sql = "select * from MepConnectionRelations Where mainElementId = ";
            sql += strId;
            SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                newNode = new TreeNode();
                //lastNode.addFather(newNode);     
                //if (reader["linkElementId"].ToString() != m_strLastId)
                if (!LastNodes.Exists(x => x.Id == Convert.ToInt64(reader["linkElementId"].ToString())))
                {

                    int index = sql.IndexOf("=");
                    string strMainId = sql.Substring(index + 1, (sql.Length - index - 1));
                    TreeNode lastNode = LastNodes.Find(x => x.Id == Convert.ToInt64(strMainId));
                    AirTerminal airterminal = new AirTerminal(-1);
                    Duct duct = new Duct(-1);
                    DuctElbow ductElbow = new DuctElbow(-1);
                    DuctReducer ductReducer = new DuctReducer(-1);
                    DuctDamper ductDamper = new DuctDamper(-1);
                    DuctSoft ductSoft = new DuctSoft(-1);
                    FlexibleShortTube flexibleShortTube = new FlexibleShortTube(-1);
                    Duct3T duct3t = new Duct3T(-1);
                    Duct4T duct4t = new Duct4T(-1);
                    bool bGet = false;
                    if (GetAirterminal(reader, dbConnection, ref airterminal))
                    {
                        bGet = true;
                        newNode.Id = airterminal.Id;
                        newNode.Parent = lastNode;
                        newNode.iType = 0;
                        long longId = (long)airterminal.Id;
                        newNode.strfireAirea = GetSmokeCompartmentOfElement(longId, "AirTerminals").Id;
                    }
                    else if (GetDuct(reader, dbConnection, ref duct))
                    {
                        bGet = true;
                        newNode.Id = duct.Id;
                        newNode.Parent = lastNode;
                        if (lastNode.iType == 3)
                        {
                            string sqlduct = "select * from MepConnectionRelations Where MainElementId = ";
                            strId = Convert.ToString(lastNode.Id);
                            sqlduct += strId;

                            sqlduct += " and  linkElementId = ";
                            strId = Convert.ToString(newNode.Id);
                            sqlduct += strId;

                            SQLiteCommand commandUserLabel = new SQLiteCommand(sqlduct, dbConnection);
                            SQLiteDataReader readerUserLabel = commandUserLabel.ExecuteReader();
                            string strMain = "";
                            if (readerUserLabel.Read())
                            {
                                strMain = readerUserLabel["userLabel"].ToString();
                            }
                            switch (strMain)
                            {
                                case "直管":
                                    lastNode.DirectNode = newNode;
                                    break;
                                case "主管":
                                    lastNode.LeftNode = newNode;
                                    break;
                                case "支管":
                                    lastNode.RightNode = newNode;
                                    break;
                            }

                        }

                        newNode.iType = 2;
                        long longId = (long)duct.Id;
                        newNode.strfireAirea = GetSmokeCompartmentOfElement(longId, "Ducts").Id;
                    }
                    else if (GetDuctElbow(reader, dbConnection, ref ductElbow))
                    {
                        bGet = true;
                        newNode.Id = ductElbow.Id;
                        newNode.Parent = lastNode;
                        newNode.iType = 2;
                        long longId = (long)ductElbow.Id;
                        newNode.strfireAirea = GetSmokeCompartmentOfElement(longId, "DuctElbows").Id;
                    }
                    else if (GetDuctReducer(reader, dbConnection, ref ductReducer))
                    {
                        bGet = true;
                        newNode.Id = ductReducer.Id;
                        newNode.Parent = lastNode;
                        newNode.iType = 2;
                        long longId = (long)ductReducer.Id;
                        newNode.strfireAirea = GetSmokeCompartmentOfElement(longId, "DuctReducers").Id;
                    }
                    else if (GetDuctDamper(reader, dbConnection, ref ductDamper))
                    {
                        bGet = true;
                        newNode.Id = ductDamper.Id;
                        newNode.Parent = lastNode;
                        newNode.iType = 2;
                        long longId = (long)ductDamper.Id;
                        newNode.strfireAirea = GetSmokeCompartmentOfElement(longId, "DuctDampers").Id;
                    }
                    else if (GetDuctSoft(reader, dbConnection, ref ductSoft))
                    {
                        bGet = true;
                        newNode.Id = ductSoft.Id;
                        newNode.Parent = lastNode;
                        newNode.iType = 2;
                        long longId = (long)ductSoft.Id;
                        newNode.strfireAirea = GetSmokeCompartmentOfElement(longId, "DuctSofts").Id;
                    }
                    else if (GetFlexibleShortTube(reader, dbConnection, ref flexibleShortTube))
                    {
                        bGet = true;
                        newNode.Id = flexibleShortTube.Id;
                        newNode.Parent = lastNode;
                        newNode.iType = 2;
                        long longId = (long)flexibleShortTube.Id;
                        newNode.strfireAirea = GetSmokeCompartmentOfElement(longId, "FlexibleShortTubes").Id;
                    }
                    else if (GetDuct3T(reader, dbConnection, ref duct3t))
                    {
                        bGet = true;
                        newNode.iType = 3;
                        newNode.Id = duct3t.Id;
                        long longId = (long)duct3t.Id;
                        newNode.strfireAirea = GetSmokeCompartmentOfElement(longId, "Duct3Ts").Id;

                        string sqlduct3t = "select * from MepConnectionRelations Where MainElementId = ";
                        strId = Convert.ToString(newNode.Id);
                        sqlduct3t += strId;

                        sqlduct3t += " and  linkElementId = ";
                        strId = Convert.ToString(lastNode.Id);
                        sqlduct3t += strId;

                        SQLiteCommand commandUserLabel = new SQLiteCommand(sqlduct3t, dbConnection);
                        SQLiteDataReader readerUserLabel = commandUserLabel.ExecuteReader();
                        string strMain = "";
                        if (readerUserLabel.Read())
                        {
                            strMain = readerUserLabel["userLabel"].ToString();
                        }

                        newNode.Parent = lastNode;


                    }
                    else if (GetDuct4T(reader, dbConnection, ref duct4t))
                    {
                        bGet = true;
                        newNode.iType = 4;
                        newNode.Id = duct4t.Id;
                        string sql4t = "select * from MepConnectionRelations Where MainElementId = ";
                        strId = Convert.ToString(newNode.Id);
                        sql4t += strId;

                        sql4t += "and linkElementId = ";
                        strId = Convert.ToString(lastNode.Id);
                        sql4t += strId;

                        SQLiteCommand commandUserLabel = new SQLiteCommand(sql4t, dbConnection);
                        SQLiteDataReader readerUserLabel = commandUserLabel.ExecuteReader();
                        string strMain = "";
                        if (readerUserLabel.Read())
                        {
                            strMain = readerUserLabel["userLabel"].ToString();
                        }
                        newNode.Parent = lastNode;

                        long longId = (long)duct4t.Id;
                        newNode.strfireAirea = GetSmokeCompartmentOfElement(longId, "Duct4Ts").Id;
                    }               


                    if (newNode != null && newNode.Id != null)
                    {
                        LastNodes.Add(newNode);
                     
                        StructSonTree(Convert.ToString(newNode.Id), dbConnection, ref newNode,  LastNodes);
                    
                    }
                }
            }
        }




        private static void AddArea(TreeNode nodeArea, TreeNode node)
        {
            if(node.LeftNode!=null)
            {
                if ((!nodeArea.StrfireAireas.Exists(x => x == node.LeftNode.strfireAirea)) && (node.LeftNode.strfireAirea != null))
                {
                    nodeArea.StrfireAireas.Add((long)node.LeftNode.strfireAirea);
                    AddArea(nodeArea, node.LeftNode.LeftNode);
                    AddArea(nodeArea, node.LeftNode.DirectNode);
                    AddArea(nodeArea, node.LeftNode.RightNode);
                }
            }

            if (node.DirectNode != null)
            {
                if ((!nodeArea.StrfireAireas.Exists(x => x == node.DirectNode.strfireAirea))&& (node.DirectNode.strfireAirea!=null))
                {
                    nodeArea.StrfireAireas.Add((long)node.DirectNode.strfireAirea);
                    AddArea(nodeArea, node.DirectNode.LeftNode);
                    AddArea(nodeArea, node.DirectNode.DirectNode);
                    AddArea(nodeArea, node.DirectNode.RightNode);
                }
            }


            if (node.RightNode != null)
            {
                if ((!nodeArea.StrfireAireas.Exists(x => x == node.RightNode.strfireAirea)) && (node.RightNode.strfireAirea != null))
                {
                    nodeArea.StrfireAireas.Add((long)node.RightNode.strfireAirea);
                    AddArea(nodeArea, node.RightNode.LeftNode);
                    AddArea(nodeArea, node.RightNode.DirectNode);
                    AddArea(nodeArea, node.RightNode.RightNode);
                }
            }
         
        }

        static bool PreOrderAddFireArea(TreeNode node)
        {        
            if (node != null)
            {
                if (node.Id < 0) return false;
                if (node.StrfireAireas == null && node.strfireAirea != null)
                {
                    node.StrfireAireas.Add((long)node.strfireAirea);
                }
                else if ((!node.StrfireAireas.Exists(x => x == node.strfireAirea)) && node.strfireAirea != null)
                {
                    node.StrfireAireas.Add((long)node.strfireAirea);
                }
            }



            if(node.LeftNode!=null)
            {
                if (node.StrfireAireas == null && node.LeftNode.strfireAirea != null)
                {
                    node.StrfireAireas.Add((long)node.LeftNode.strfireAirea);

                    AddArea(node, node.LeftNode);
                }
                else if ((!node.StrfireAireas.Exists(x => x == node.LeftNode.strfireAirea)) && node.LeftNode.strfireAirea != null)
                {
                    node.StrfireAireas.Add((long)node.LeftNode.strfireAirea);

                    AddArea(node, node.LeftNode);
                }
            }


            if (node.DirectNode != null)
            {
                if (node.StrfireAireas == null && node.DirectNode.strfireAirea != null)
                {
                    node.StrfireAireas.Add((long)node.DirectNode.strfireAirea);

                    AddArea(node, node.DirectNode);
                }
                else if ((!node.StrfireAireas.Exists(x => x == node.DirectNode.strfireAirea)) && node.DirectNode.strfireAirea != null)
                {
                    node.StrfireAireas.Add((long)node.DirectNode.strfireAirea);
                    AddArea(node, node.DirectNode);
                }
            }

            if (node.RightNode != null)
            {
                if (node.StrfireAireas == null && node.RightNode.strfireAirea != null)
                {
                    node.StrfireAireas.Add((long)node.RightNode.strfireAirea);

                    AddArea(node, node.RightNode);
                }
                else if ((!node.StrfireAireas.Exists(x => x == node.RightNode.strfireAirea)) && node.RightNode.strfireAirea != null)
                {
                    node.StrfireAireas.Add((long)node.RightNode.strfireAirea);
                    AddArea(node, node.RightNode);
                }
            }


            if(node.LeftNode!=null)
            {
                PreOrderAddFireArea(node.LeftNode);
            }
            if (node.LeftNode != null)
            {
                PreOrderAddFireArea(node.DirectNode);
            }
            if (node.RightNode != null)
            {
                PreOrderAddFireArea(node.RightNode);
            }
                
            return true;
        }
                        
        static bool PreOrderFind(TreeNode node,AirTerminal airterminal)
        {
            if(node == null) return false;
            if (node.Id < 0) return false;
            if (node.Id == airterminal.Id)
            {
                return true;
            }
            if (PreOrderFind(node.LeftNode, airterminal))
            {
                return true;
            }
            if (PreOrderFind(node.DirectNode, airterminal))
            {
                return true;
            }
            if (PreOrderFind(node.RightNode, airterminal))
            {
                return true;
            }
            return false;

        }


        static bool  FindRoot(List<TreeNode> LastNodes,ref TreeNode root)
        {

            foreach(TreeNode node in LastNodes)
            {
                if (node.Parent == null)
                {
                    root = node;
                    return true;
                }
                  
            }

            return false;

        }

        static  void PreOrderAirterminalNode(List<TreeNode> airTerminalNodes ,TreeNode node)
        {
            
            if(node == null) return ;
            if (node.Id == null) return ;
            if (node.iType == 0)
            {
                airTerminalNodes.Add(node);
            }
            if(node.LeftNode!=null)
            {
                PreOrderAirterminalNode(airTerminalNodes,node.LeftNode);
            }
            if (node.DirectNode != null)
            {
                PreOrderAirterminalNode(airTerminalNodes,node.DirectNode);
            }
            if (node.RightNode != null)
            {
                PreOrderAirterminalNode(airTerminalNodes,node.RightNode);
            }          
          
        }

        static TreeNode GetParentEquel3T4T(TreeNode node, List<TreeNode> ducts)
        {
            if (node.Parent == null)
                return null;
            if (node.Parent.iType != 3 && node.Parent.iType != 4)
            {
                if (node.Parent.iType == 1)
                {
                    ducts.Add(node);
                }

                GetParentEquel3T4T(node.Parent, ducts);
            }
            else if (node.Parent.iType == 3 || node.Parent.iType == 4)
            {
                return node.Parent;
            }
            return null;
        }
        public static Room GetSmokeCompartmentOfElement(long Id,string tableName)
        {
            long lid = 0;//id為空的對象
            Room room = new Room(lid);
            if (!System.IO.File.Exists(m_hvacXdbPath))
                return room;
            //创建一个连接
            string connectionstr = @"data source =" + m_hvacXdbPath;
            SQLiteConnection dbConnection = new SQLiteConnection(connectionstr);
            dbConnection.Open();
            string sql = "select * from ";
            sql += tableName +" "+"Where Id =" ;
        
            sql = sql + Id;
            SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            if (reader.Read())
            {
                AABB aabbTerminal = GetAABB(reader, dbConnection);
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
                    room = new Room(Convert.ToInt64(readerRoom["Id"].ToString()));
                    room.type = readerRoom["userLabel"].ToString();
                    sql = "select * from Storeys where  Id =  ";
                    sql = sql + readerRoom["storeyId"].ToString();
                    SQLiteCommand command1 = new SQLiteCommand(sql, dbConnectionArch);
                    SQLiteDataReader reader1 = command1.ExecuteReader();

                    if (reader1.Read())
                    {
                        room.m_iStoryNo = Convert.ToInt32(reader1["storeyNo"].ToString());
                    }

                    if(room.m_iStoryNo == Convert.ToInt32( reader["StoreyNo"].ToString() ))
                    {

                        List<PointIntList> PointLists = new List<PointIntList>();
                        PointLists.Add(new PointIntList() { new PointInt(0, 0, 0) });
                        string sSpaceId = "0";
                        Polygon2D poly = new Polygon2D(PointLists, sSpaceId);
                        room.Id = Convert.ToInt64(readerRoom["Id"].ToString());
                        if (!GetRoomPolygon2D(room, ref poly))
                        {
                            return room;
                        }
                        if (poly.Polygon2D_Contains_AABB(aabbTerminal))
                        {                           
                            room.name = reader["name"].ToString();
                            room.m_dHeight = Convert.ToDouble(readerRoom["dHeight"].ToString());
                            room.m_dArea = Convert.ToDouble(readerRoom["dArea"].ToString());
                            room.m_iNumberOfPeople = Convert.ToInt32(readerRoom["nNumberOfPeople"].ToString());
                            //room.m_dMaxlength
                            //     room.m_dVolume
                            //    room.m_eRoomPosition
                            return room;
                           
                        }
                        else if (Geometry_Utils_BBox.IsBBoxIntersectsBBox3D(poly, aabbTerminal))
                        {                        
                            room.name = readerRoom["name"].ToString();
                            room.m_dHeight = Convert.ToDouble(readerRoom["dHeight"].ToString());
                            room.m_dArea = Convert.ToDouble(readerRoom["dArea"].ToString());
                            room.m_iNumberOfPeople = Convert.ToInt32(readerRoom["nNumberOfPeople"].ToString());
                            //room.m_dMaxlength
                            //     room.m_dVolume
                            //    room.m_eRoomPosition
                            return room;

                        }

                    }

          
                }
            }
            //关闭连接  
            room.Id = null;
            return room;
        }

        private static bool GetAirterminal(SQLiteDataReader reader, SQLiteConnection dbConnection, ref AirTerminal duct)
        {
            string sql = "select * from AirTerminals Where Id = ";
            sql += reader["linkElementId"].ToString();

            SQLiteCommand commandFans = new SQLiteCommand(sql, dbConnection);
            SQLiteDataReader readerFans = commandFans.ExecuteReader();
            if (readerFans.Read())
            {
                duct = new AirTerminal(Convert.ToInt64(readerFans["Id"].ToString()));
                SetAirterminalPara(ref duct, readerFans);
                return true;
            }
            return false;
        }

        private static bool GetDuct(SQLiteDataReader reader, SQLiteConnection dbConnection, ref Duct duct)
        {
            string sql = "select * from Ducts Where Id = ";
            sql += reader["linkElementId"].ToString();

            SQLiteCommand commandFans = new SQLiteCommand(sql, dbConnection);
            SQLiteDataReader readerFans = commandFans.ExecuteReader();
            if (readerFans.Read())
            {
                duct = new Duct(Convert.ToInt64(readerFans["Id"].ToString()));
                return true;
            }
            return false;
        }
        
        private static bool GetDuctReducer(SQLiteDataReader reader, SQLiteConnection dbConnection, ref DuctReducer duct)
        {
            string sql = "select * from DuctReducers Where Id = ";
            sql += reader["linkElementId"].ToString();

            SQLiteCommand commandFans = new SQLiteCommand(sql, dbConnection);
            SQLiteDataReader readerFans = commandFans.ExecuteReader();
            if (readerFans.Read())
            {
                duct = new DuctReducer(Convert.ToInt64(readerFans["Id"].ToString()));
                return true;
            }
            return false;
        }

        private static  bool GetDuctDamper(SQLiteDataReader reader, SQLiteConnection dbConnection, ref DuctDamper duct)
        {
            string sql = "select * from DuctDampers Where Id = ";
            sql += reader["linkElementId"].ToString();

            SQLiteCommand commandFans = new SQLiteCommand(sql, dbConnection);
            SQLiteDataReader readerFans = commandFans.ExecuteReader();
            if (readerFans.Read())
            {
                duct = new DuctDamper(Convert.ToInt64(readerFans["Id"].ToString()));
                return true;
            }
            return false;
        }
        
        private static bool GetDuctSoft(SQLiteDataReader reader, SQLiteConnection dbConnection, ref DuctSoft duct)
        {
            string sql = "select * from DuctSofts Where Id = ";
            sql += reader["linkElementId"].ToString();

            SQLiteCommand commandFans = new SQLiteCommand(sql, dbConnection);
            SQLiteDataReader readerFans = commandFans.ExecuteReader();
            if (readerFans.Read())
            {
                duct = new DuctSoft(Convert.ToInt64(readerFans["Id"].ToString()));
                return true;
            }
            return false;
        }

        private static bool GetFlexibleShortTube(SQLiteDataReader reader, SQLiteConnection dbConnection, ref FlexibleShortTube duct)
        {
            string sql = "select * from FlexibleShortTubes Where Id = ";
            sql += reader["linkElementId"].ToString();

            SQLiteCommand commandFans = new SQLiteCommand(sql, dbConnection);
            SQLiteDataReader readerFans = commandFans.ExecuteReader();
            if (readerFans.Read())
            {
                duct = new FlexibleShortTube(Convert.ToInt64(readerFans["Id"].ToString()));
                return true;
            }
            return false;
        }

        private static bool GetDuctElbow(SQLiteDataReader reader, SQLiteConnection dbConnection, ref  DuctElbow duct)
        {
            string sql = "select * from DuctElbows Where Id = ";
            sql += reader["linkElementId"].ToString();

            SQLiteCommand commandFans = new SQLiteCommand(sql, dbConnection);
            SQLiteDataReader readerFans = commandFans.ExecuteReader();
            if (readerFans.Read())
            {
                duct = new DuctElbow(Convert.ToInt64(readerFans["Id"].ToString()));
                return true;
            }
            return false;
        }

        private static bool GetDuct3T(SQLiteDataReader reader, SQLiteConnection dbConnection, ref Duct3T duct)
        {
            string sql = "select * from Duct3Ts Where Id = ";
            sql += reader["linkElementId"].ToString();

            SQLiteCommand commandFans = new SQLiteCommand(sql, dbConnection);
            SQLiteDataReader readerFans = commandFans.ExecuteReader();
            if (readerFans.Read())
            {
                duct = new Duct3T(Convert.ToInt64(readerFans["Id"].ToString()));
                return true;
            }
            return false;
        }

        private static bool GetDuct4T(SQLiteDataReader reader, SQLiteConnection dbConnection, ref Duct4T duct)
        {
            string sql = "select * from Duct4Ts Where Id = ";
            sql += reader["linkElementId"].ToString();

            SQLiteCommand commandFans = new SQLiteCommand(sql, dbConnection);
            SQLiteDataReader readerFans = commandFans.ExecuteReader();
            if (readerFans.Read())
            {
                duct = new Duct4T(Convert.ToInt64(readerFans["Id"].ToString()));
                return true;
            }
            return false;
        }

        public static bool isAirTerminalInRoom(AirTerminal airTerminal, Room room)
        {
            List<PointIntList> PointLists = new List<PointIntList>();
            PointLists.Add(new PointIntList() { new PointInt(0, 0, 0) });
            string sSpaceId = "0";
            Polygon2D poly = new Polygon2D(PointLists, sSpaceId);

            if (!GetRoomPolygon2D(room, ref poly))
            {
                return false;
            }

            string connectionstr = @"data source =" + m_hvacXdbPath;
            SQLiteConnection dbConnectionHVAC = new SQLiteConnection(connectionstr);
            dbConnectionHVAC.Open();
            string sql = "select * from AirTerminals Where Id = ";
            sql += airTerminal.Id;
            SQLiteCommand commandHVAC = new SQLiteCommand(sql, dbConnectionHVAC);
            SQLiteDataReader readerAirTerminals = commandHVAC.ExecuteReader();
            PointInt ptMin = new PointInt(0, 0, 0);
            PointInt ptMax = new PointInt(0, 0, 0);
            string strId = "";
            AABB aabbAirTerminal = new AABB(ptMin, ptMax, strId);
            if (readerAirTerminals.Read())
            {
                airTerminal.airVelocity = Convert.ToDouble(readerAirTerminals["AirVelocity"].ToString());
                airTerminal.systemType = readerAirTerminals["SystemType"].ToString();
                airTerminal.m_iStoryNo = Convert.ToInt32(readerAirTerminals["StoreyNo"].ToString());
                aabbAirTerminal = GetAABB(readerAirTerminals, dbConnectionHVAC);
            }
            else
                return false;

            return poly.Polygon2D_Contains_AABB(aabbAirTerminal);
        }
  
        
        public static bool isOuterAirTerminal(AirTerminal airTerminal)
        {
            //创建一个连接
            string connectionstr = @"data source =" + m_hvacXdbPath;
            SQLiteConnection dbConnectionHVAC = new SQLiteConnection(connectionstr);
            dbConnectionHVAC.Open();
            string sql = "select * from AirTerminals Where Id = ";
            sql += airTerminal.Id;
            SQLiteCommand commandHVAC = new SQLiteCommand(sql, dbConnectionHVAC);
            SQLiteDataReader readerAirTerminals = commandHVAC.ExecuteReader();
            PointInt ptMin = new PointInt(0, 0, 0);
            PointInt ptMax = new PointInt(0, 0, 0);
            string strId = "";           
            AABB aabbAirTerminal = new AABB(ptMin, ptMax, strId);
            if (readerAirTerminals.Read())
            {
                SetAirterminalPara(ref airTerminal, readerAirTerminals);       
                aabbAirTerminal = GetAABB(readerAirTerminals, dbConnectionHVAC);
            }

            List<Room> rooms = new List<Room>();
            rooms = GetAllRoomsInCertainStorey(airTerminal.m_iStoryNo.Value);
            List<PointIntList> PointLists = new List<PointIntList>();
            PointLists.Add(new PointIntList() { new PointInt(0, 0, 0) });
            string sSpaceId = "0";
            Polygon2D poly = new Polygon2D(PointLists, sSpaceId);
            foreach (Room room in rooms)
            {             
                if (!GetRoomPolygon2D(room, ref poly))
                {
                    return false;
                }


                if (poly.Polygon2D_Contains_AABB(aabbAirTerminal))
                {
                    return false;
                }
                else if (poly.IsPolygon2DIntersectsAABB(aabbAirTerminal))
                {
                    List<AABB> sideWallAABBsOfRoom = GetAllSideWallsAABBOfRoom(room);
                    foreach(AABB sideWallAABB in sideWallAABBsOfRoom)
                    {

                        if (aabbAirTerminal.IsBBoxIntersectsBBox3D(sideWallAABB))
                            return true;
                    }
                    return false;
                }

            }             
            //关闭连接
            dbConnectionHVAC.Close();       
            return true;
        }


        private static List<AABB> GetAllSideWallsAABBOfRoom(Room room)
        {
            List<AABB> AABBs = new List<AABB>();

            string sql = "select * from WallOfSpaceRelations where spaceId=";
            sql += room.Id.ToString();
            string connectionstr = @"data source =" + m_archXdbPath;
            SQLiteConnection dbConnection = new SQLiteConnection(connectionstr);
            dbConnection.Open();
            SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();

            List<int> wallIds = new List<int>();
            while (reader.Read())
            {
                wallIds.Add(Convert.ToInt32(reader["wallId"].ToString()));
            }

            foreach(int Id in wallIds)
            {
                sql = "select * from Walls where Id=";
                sql += Id.ToString();
                sql += " And isSideWall=1";
                command = new SQLiteCommand(sql, dbConnection);
                reader = command.ExecuteReader();
                if (reader.Read())   
                    AABBs.Add(GetAABB(reader, dbConnection));
            }
            return AABBs;
        }

    }

    public class ElementEqualityComparer : IEqualityComparer<Element>
    {
        public bool Equals(Element element_1, Element element_2)
        {
            if (element_1 == null && element_2 == null)
                return true;
            else if (element_1 == null || element_2 == null)
                return false;
            else if (element_1.Id==element_2.Id)
                return true;
            else
                return false;
        }

        public int GetHashCode(Element element)
        {
            long hCode = element.Id.Value;
            return hCode.GetHashCode();
        }
    }

    [Flags]
    public enum RoomPosition { overground = 1, underground = 2, semi_underground = 4 }
}
#endif