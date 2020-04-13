﻿#define DEBUG
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
        public int? iType { get; set; } = null;//风口 0 3t：3，4T：4
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

        }



    }


   

    public class HVACFunction
    {
        public static string m_archXdbPath { set; get; }
        public static string m_hvacXdbPath { set; get; }

        private static string m_strLastId;
        private static List<string> m_listStrLastId;

        public HVACFunction(string Archxdb, string HVACxdb)
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
            sql = sql + " and dArea > ";
            sql = sql + area;
            SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                Room room = new Room(Convert.ToInt64(reader["Id"].ToString()));
                room.name = reader["name"].ToString();
                room.m_dHeight = Convert.ToDouble(reader["dHeight"].ToString());
                room.m_dArea = Convert.ToDouble(reader["dArea"].ToString());
                room.m_iNumberOfPeople = Convert.ToInt32(reader["nNumberOfPeople"].ToString());
                //room.m_dMaxlength
                //     room.m_dVolume
                //    room.m_eRoomPosition
                //    room.type
                sql = "select * from Storeys where  Id =  ";
                sql = sql + reader["storeyId"].ToString();
                SQLiteCommand command1 = new SQLiteCommand(sql, dbConnection);
                SQLiteDataReader reader1 = command1.ExecuteReader();

                if (reader1.Read())
                {
                    room.m_iStoryNo = Convert.ToInt32(reader1["storeyNo"].ToString());
                }
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
                    if (room.m_iStoryNo == Convert.ToInt32(readerAirTerminals["StoreyNo"].ToString()))
                    {
                        AirTerminal airTerminal = new AirTerminal(Convert.ToInt64(readerAirTerminals["Id"].ToString()));
                        airTerminal.airVelocity = Convert.ToDouble(readerAirTerminals["AirVelocity"].ToString());
                        airTerminal.systemType = readerAirTerminals["SystemType"].ToString();
                        AABB aabbAirTerminal = GetAABB(readerAirTerminals, dbConnectionHVAC);


                        PointInt pt = aabbAirTerminal.Center();
                        if (Geometry_Utils_BBox.IsPointInBBox2D(poly, aabbAirTerminal.Center())
                         || Geometry_Utils_BBox.IsPointInBBox2D(aabbAirTerminal, poly.Center())
                         || Geometry_Utils_BBox.IsPointInBBox2D(poly, aabbAirTerminal.Min)
                         || Geometry_Utils_BBox.IsPointInBBox2D(poly, aabbAirTerminal.Max))
                        {
                            airterminals.Add(airTerminal);
                        }
                        else if (Geometry_Utils_BBox.IsBBoxIntersectsBBox3D(poly, aabbAirTerminal))
                        {
                            string strVector = readerAirTerminals["NormalVector"].ToString();
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
                            dX = poly.Center().X - aabbAirTerminal.Center().X;
                            dY = poly.Center().Y - aabbAirTerminal.Center().Y;
                            dZ = poly.Center().Z - aabbAirTerminal.Center().Z;
                            listVectorAirterminalToRoom.Add(dX);
                            listVectorAirterminalToRoom.Add(dY);
                            listVectorAirterminalToRoom.Add(dZ);

                            double dDotProduct = listVector[0] * listVectorAirterminalToRoom[0] + listVector[1] * listVectorAirterminalToRoom[1] + listVector[2] * listVectorAirterminalToRoom[2];
                            if (dDotProduct < 0)
                            {
                                airterminals.Add(airTerminal);
                            }


                        }

                    }
                }
                //关闭连接
                dbConnection.Close();

                return airterminals;
            }
            return airterminals;
        }



        public static List<Window> GetWindowsInRoom(Room room)
        {
            List<Window> windows = new List<Window>();
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
                AABB aabbRoom = GetAABB(reader, dbConnection);
                sql = "select * from Windows";
                SQLiteCommand commandWindows = new SQLiteCommand(sql, dbConnection);
                SQLiteDataReader readerWindows = commandWindows.ExecuteReader();
                while (readerWindows.Read())
                {
                    AABB aabbAirTerminal = GetAABB(readerWindows, dbConnection);

                    if (aabbRoom.IsAABBContainsAABB3D(aabbAirTerminal))
                    {
                        Window window = new Window(Convert.ToInt64(readerWindows["Id"].ToString()));
                        window.isExternalWindow = Convert.ToBoolean(readerWindows["IsOutsideComponent"].ToString());
                        window.area = Convert.ToDouble(readerWindows["Area"].ToString());
                        windows.Add(window);
                    }
                    else if (Geometry_Utils_BBox.IsBBoxIntersectsBBox3D(aabbRoom, aabbAirTerminal))
                    {
                        Window window = new Window(Convert.ToInt64(readerWindows["Id"].ToString()));
                        window.isExternalWindow = Convert.ToBoolean(readerWindows["IsOutsideComponent"].ToString());
                        window.area = Convert.ToDouble(readerWindows["Area"].ToString());
                        windows.Add(window);
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
                if (reader["linkElementId"].ToString() != m_strLastId)
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
                        inlet.airVelocity = Convert.ToDouble(readerDucts["AirFlowRate"].ToString());
                        inlet.systemType = readerDucts["SystemType"].ToString();
                        inlets.Add(inlet);
                    }
                    else
                    {
                        m_listStrLastId.Add(strId);
                        FindOutLets(dbConnection, reader["linkElementId"].ToString(), inlets);
                    }
                }
            }
        }

        //5找到大于一定长度的走道对象  double  “走道、走廊”    长度清华引擎 计算学院  张荷花// 表里加type
        public static List<Room> GetRoomsMoreThan(string roomType, double dLength)
        {
            List<Room> rooms = new List<Room>();

            if (!System.IO.File.Exists(m_archXdbPath))
                return rooms;

            //创建一个连接
            string connectionstr = @"data source =" + m_archXdbPath;
            SQLiteConnection dbConnection = new SQLiteConnection(connectionstr);
            dbConnection.Open();
            string sql = "select * from Spaces Where CHARINDEX(";
            sql = sql + "'" + roomType + "'";
            sql = sql + ",userLabel)> 0";
            SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                Room room = new Room(Convert.ToInt64(reader["Id"].ToString()));
                room.boundaryLoops = reader["boundaryLoops"].ToString();
                room.name = reader["name"].ToString();
                room.m_dHeight = Convert.ToDouble(reader["dHeight"].ToString());
                room.m_dArea = Convert.ToDouble(reader["dArea"].ToString());
                room.m_iNumberOfPeople = Convert.ToInt32(reader["nNumberOfPeople"].ToString());
                //room.m_dMaxlength
                //     room.m_dVolume
                //    room.m_eRoomPosition
                //    room.type
                sql = "select * from Storeys where  Id =  ";
                sql = sql + reader["storeyId"].ToString();
                SQLiteCommand command1 = new SQLiteCommand(sql, dbConnection);
                SQLiteDataReader reader1 = command1.ExecuteReader();

                if (reader1.Read())
                {
                    room.m_iStoryNo = Convert.ToInt32(reader1["storeyNo"].ToString());
                }
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
                        inlet.airVelocity = Convert.ToDouble(readerAirTerminal["AirFlowRate"].ToString());
                        inlet.systemType = readerAirTerminal["SystemType"].ToString();
                        inlets.Add(inlet);
                    }
                    else
                    {
                        m_listStrLastId.Add(strId);
                        FindInlets(dbConnection, reader["linkElementId"].ToString(), inlets);
                    }
                }
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
                //room.name = readerSpace["name"].ToString();
                //room.boundaryLoops = readerSpace["boundaryLoops"].ToString();
                //Polygon2D poly = GetSpaceBBox(room.boundaryLoops, room.Id.ToString());

                AABB aabbRoom = GetAABB(readerSpace, dbConnection);


                connectionstr = @"data source =" + m_hvacXdbPath;
                SQLiteConnection dbConnectionHVAC = new SQLiteConnection(connectionstr);
                dbConnectionHVAC.Open();
                sql = "select * from Ducts";
                SQLiteCommand commandHVAC = new SQLiteCommand(sql, dbConnectionHVAC);
                SQLiteDataReader readerDucts = commandHVAC.ExecuteReader();
                while (readerDucts.Read())
                {

                    AABB aabbDuct = GetAABB(readerDucts, dbConnectionHVAC);

                    if (Geometry_Utils_BBox.IsBBoxIntersectsBBox3D(aabbRoom, aabbDuct))
                    {

                        Duct duct = new Duct(Convert.ToInt64(readerDucts["Id"].ToString()));
                        duct.airVelocity = Convert.ToDouble(readerDucts["Velocity"].ToString());
                        ducts.Add(duct);
                    }
                }
            }

            //关闭连接
            dbConnection.Close();

            return ducts;
        }
        //8找到穿越防火分区的风管对象集合  userlable
        public static List<Duct> GetDuctsCrossFireDistrict(FireCompartment fireDistrict)
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
                //fireDistrict.name = readerSpace["name"].ToString();
                //fireDistrict.boundaryLoops = readerSpace["boundaryLoops"].ToString();
                //Polygon2D poly = GetSpaceBBox(fireDistrict.boundaryLoops, fireDistrict.Id.ToString());
                AABB aabbFireDistrict = GetAABB(readerSpace, dbConnection);

                //创建一个连接
                connectionstr = @"data source =" + m_hvacXdbPath;
                SQLiteConnection dbConnectionHVAC = new SQLiteConnection(connectionstr);
                dbConnectionHVAC.Open();
                sql = "select * from Ducts";
                SQLiteCommand commandHVAC = new SQLiteCommand(sql, dbConnectionHVAC);
                SQLiteDataReader readerDucts = commandHVAC.ExecuteReader();
                while (readerDucts.Read())
                {
                    AABB aabbDuct = GetAABB(readerDucts, dbConnectionHVAC);
                    if (Geometry_Utils_BBox.IsBBoxIntersectsBBox3D(aabbFireDistrict, aabbDuct))
                    {

                        Duct duct = new Duct(Convert.ToInt64(readerDucts["Id"].ToString()));
                        duct.airVelocity = Convert.ToDouble(readerDucts["Velocity"].ToString());
                        ducts.Add(duct);
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

            List<FireCompartment> fireDistricts = GetALLFireCompartment();

            foreach (FireCompartment fireDistrict in fireDistricts)
            {
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
                    //fireDistrict.name = readerSpace["name"].ToString();
                    //fireDistrict.boundaryLoops = readerSpace["boundaryLoops"].ToString();
                    //Polygon2D poly = GetSpaceBBox(fireDistrict.boundaryLoops, fireDistrict.Id.ToString());
                    AABB aabbFireDistrict = GetAABB(readerSpace, dbConnection);

                    //创建一个连接
                    connectionstr = @"data source =" + m_hvacXdbPath;
                    SQLiteConnection dbConnectionHVAC = new SQLiteConnection(connectionstr);
                    dbConnectionHVAC.Open();
                    sql = "select * from Ducts";
                    SQLiteCommand commandHVAC = new SQLiteCommand(sql, dbConnectionHVAC);
                    SQLiteDataReader readerDucts = commandHVAC.ExecuteReader();
                    while (readerDucts.Read())
                    {
                        AABB aabbDuct = GetAABB(readerDucts, dbConnectionHVAC);
                        if (Geometry_Utils_BBox.IsBBoxIntersectsBBox3D(aabbFireDistrict, aabbDuct))
                        {

                            AABB aabbjoint = GetAABB(readerDucts, dbConnectionHVAC);
                            if (Geometry_Utils_BBox.IsBBoxIntersectsBBox3D(aabbjoint, aabbDuct))
                            {

                                Duct duct = new Duct(Convert.ToInt64(readerDucts["Id"].ToString()));
                                duct.airVelocity = Convert.ToDouble(readerDucts["Velocity"].ToString());
                                ducts.Add(duct);
                            }

                        }
                    }
                }

                //关闭连接
                dbConnection.Close();

            }


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
                    AABB aabbRoom = GetAABB(readerRoom, dbConnectionArch);
                    if (aabbRoom.IsAABBContainsAABB3D(aabbTerminal))
                    {
                        room = new Room(Convert.ToInt64(readerRoom["Id"].ToString()));
                        room.name = reader["name"].ToString();
                        room.m_dHeight = Convert.ToDouble(reader["dHeight"].ToString());
                        room.m_dArea = Convert.ToDouble(reader["dArea"].ToString());
                        room.m_iNumberOfPeople = Convert.ToInt32(reader["nNumberOfPeople"].ToString());
                        //room.m_dMaxlength
                        //     room.m_dVolume
                        //    room.m_eRoomPosition
                        room.type = readerRoom["userLabel"].ToString();
                        sql = "select * from Storeys where  Id =  ";
                        sql = sql + reader["storeyId"].ToString();
                        SQLiteCommand command1 = new SQLiteCommand(sql, dbConnection);
                        SQLiteDataReader reader1 = command1.ExecuteReader();

                        if (reader1.Read())
                        {
                            room.m_iStoryNo = Convert.ToInt32(reader1["storeyNo"].ToString());
                        }
                    }
                    else if (Geometry_Utils_BBox.IsBBoxIntersectsBBox3D(aabbTerminal, aabbRoom))
                    {
                        string strVector = reader["NormalVector"].ToString();
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
                        dX = aabbRoom.Center().X - aabbTerminal.Center().X;
                        dY = aabbRoom.Center().Y - aabbTerminal.Center().Y;
                        dZ = aabbRoom.Center().Z - aabbTerminal.Center().Z;
                        listVectorAirterminalToRoom.Add(dX);
                        listVectorAirterminalToRoom.Add(dY);
                        listVectorAirterminalToRoom.Add(dZ);

                        double dDotProduct = listVector[0] * listVectorAirterminalToRoom[0] + listVector[1] * listVectorAirterminalToRoom[1] + listVector[2] * listVectorAirterminalToRoom[2];
                        if (dDotProduct < 0)
                        {
                            room = new Room(Convert.ToInt64(readerRoom["Id"].ToString()));
                            room.name = readerRoom["name"].ToString();
                            room.m_dHeight = Convert.ToDouble(readerRoom["dHeight"].ToString());
                            room.m_dArea = Convert.ToDouble(readerRoom["dArea"].ToString());
                            room.m_iNumberOfPeople = Convert.ToInt32(readerRoom["nNumberOfPeople"].ToString());
                            //room.m_dMaxlength
                            //     room.m_dVolume
                            //    room.m_eRoomPosition
                            room.type = readerRoom["userLabel"].ToString();
                            sql = "select * from Storeys where  Id =  ";
                            sql = sql + readerRoom["storeyId"].ToString();
                            SQLiteCommand command1 = new SQLiteCommand(sql, dbConnection);
                            SQLiteDataReader reader1 = command1.ExecuteReader();

                            if (reader1.Read())
                            {
                                room.m_iStoryNo = Convert.ToInt32(reader1["storeyNo"].ToString());
                            }
                        }

                    }
                }
            }
            //关闭连接               
            return room;
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
            string sql = "select * from Spaces Where CHARINDEX(";
            sql = sql + "'" + roomType + "'";

            sql = sql + ",userLabel)> 0";

            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                Room room = new Room(Convert.ToInt64(reader["Id"].ToString()));
                room.name = reader["name"].ToString();
                room.m_dHeight = Convert.ToDouble(reader["dHeight"].ToString());
                room.m_dArea = Convert.ToDouble(reader["dArea"].ToString());
                room.m_iNumberOfPeople = Convert.ToInt32(reader["nNumberOfPeople"].ToString());
                //room.m_dMaxlength
                //     room.m_dVolume
                //    room.m_eRoomPosition
                room.type = reader["userLabel"].ToString();
                sql = "select * from Storeys where  Id =  ";
                sql = sql + reader["storeyId"].ToString();
                SQLiteCommand command1 = new SQLiteCommand(sql, m_dbConnection);
                SQLiteDataReader reader1 = command1.ExecuteReader();

                if (reader1.Read())
                {
                    room.m_iStoryNo = Convert.ToInt32(reader1["storeyNo"].ToString());
                }
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
            sql = sql + "'" + containedString + "'";
            sql = sql + ",name)> 0";

            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                Room room = new Room(Convert.ToInt64(reader["Id"].ToString()));
                room.name = reader["name"].ToString();
                room.m_dHeight = Convert.ToDouble(reader["dHeight"].ToString());
                room.m_dArea = Convert.ToDouble(reader["dArea"].ToString());
                room.m_iNumberOfPeople = Convert.ToInt32(reader["nNumberOfPeople"].ToString());
                //room.m_dMaxlength
                //     room.m_dVolume
                //    room.m_eRoomPosition
                room.type = reader["userLabel"].ToString();
                sql = "select * from Storeys where  Id =  ";
                sql = sql + reader["storeyId"].ToString();
                SQLiteCommand command1 = new SQLiteCommand(sql, m_dbConnection);
                SQLiteDataReader reader1 = command1.ExecuteReader();

                if (reader1.Read())
                {
                    room.m_iStoryNo = Convert.ToInt32(reader1["storeyNo"].ToString());
                }
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
                floor.FloorNumber = Convert.ToInt32(reader["storeyNo"].ToString());
                floor.elevation = Convert.ToDouble(reader["elevation"].ToString());
                floor.height = Convert.ToDouble(reader["height"].ToString());
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
                        duct.airVelocity = Convert.ToDouble(readerDucts["Velocity"].ToString());
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

        //18获得防烟分区长边长度
        public static double GetFireDistrictLength(FireCompartment fireDistrict)
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
            for (int i = 0; i < corridors.Count(); i++)
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
                    List<Room> rooms = new List<Room>();
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
                rooms.Add(roomConnect);
            }
            //关闭连接
            dbConnection.Close();
            return rooms;
        }

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
            sql = sql + firstRoom.Id + "and ToRoomId = ";
            sql = sql + SecondRoom.Id;

            SQLiteCommand commandDoors = new SQLiteCommand(sql, dbConnection);
            SQLiteDataReader readerDoors = commandDoors.ExecuteReader();
            List<string> listStrLastId = new List<string>();
            while (readerDoors.Read())
            {
                Door door = new Door(Convert.ToInt64(readerDoors["ToRoomId"].ToString()));
                listStrLastId.Add(readerDoors["ToRoomId"].ToString());
                doors.Add(door);
            }

            sql = "select * from Doors Where FromRoomId = ";
            sql = sql + SecondRoom.Id + "and ToRoomId = ";
            sql = sql + firstRoom.Id;

            SQLiteCommand commandDoorsTo = new SQLiteCommand(sql, dbConnection);
            SQLiteDataReader readerDoorsTo = commandDoorsTo.ExecuteReader();
            while (readerDoorsTo.Read())
            {
                Door door = new Door(Convert.ToInt64(readerDoors["ToRoomId"].ToString()));
                if (!listStrLastId.Exists(x => x == readerDoors["linkElementId"].ToString()))
                {
                    doors.Add(door);
                }
            }


            //关闭连接
            dbConnection.Close();
            return doors;
        }


        public static List<Door> getDoorsBetweenTwoRooms(Room firstRoom, Room secondRoom)
        {
            List<Door> doors = new List<Door>();
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
                pipe.airVelocity = Convert.ToDouble(reader["AirFlowRate"].ToString());
                pipe.systemType = reader["SystemType"].ToString();
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
            string connectionArchstr = @"data source =" + m_archXdbPath;
            SQLiteConnection dbConnectionArch = new SQLiteConnection(connectionArchstr);
            dbConnectionArch.Open();
            string sql = "select * from Spaces Where Id = ";
            sql = sql + room.Id;
            SQLiteCommand commandRoom = new SQLiteCommand(sql, dbConnectionArch);
            SQLiteDataReader readerRoom = commandRoom.ExecuteReader();

            if (readerRoom.Read())
            {
                room.name = readerRoom["name"].ToString();
                room.boundaryLoops = readerRoom["boundaryLoops"].ToString();
                poly = GetSpaceBBox(room.boundaryLoops, room.Id.ToString());
            }
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
                room.name = reader["name"].ToString();
                room.m_dHeight = Convert.ToDouble(reader["dHeight"].ToString());
                room.m_dArea = Convert.ToDouble(reader["dArea"].ToString());
                room.m_iNumberOfPeople = Convert.ToInt32(reader["nNumberOfPeople"].ToString());
                //room.m_dMaxlength
                //     room.m_dVolume
                //    room.m_eRoomPosition
                room.type = reader["userLabel"].ToString();
                sql = "select * from Storeys where  Id =  ";
                sql = sql + reader["storeyId"].ToString();
                SQLiteCommand command1 = new SQLiteCommand(sql, m_dbConnection);
                SQLiteDataReader reader1 = command1.ExecuteReader();

                if (reader1.Read())
                {
                    room.m_iStoryNo = Convert.ToInt32(reader1["storeyNo"].ToString());
                }
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
            string sql = "select * from Ducts Where CHARINDEX(";
            sql = sql + "'" + strSystemName + "'";

            sql = sql + ",SystemName)> 0";

            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                Duct room = new Duct(Convert.ToInt64(reader["Id"].ToString()));
                room.airVelocity = Convert.ToDouble(reader["Velocity"].ToString());
                ducts.Add(room);
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

            string sql = "select * from AirTerminals Where CHARINDEX(";
            sql = sql + "'" + strSystemName + "'";

            sql = sql + ",SystemName)> 0";


            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                AirTerminal pipe = new AirTerminal(Convert.ToInt64(reader["Id"].ToString()));
                pipe.airVelocity = Convert.ToDouble(reader["AirFlowRate"].ToString());
                pipe.systemType = reader["SystemType"].ToString();
                pipes.Add(pipe);
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
            string sql = "select * from Spaces where CHARINDEX(";
            sql = sql + "'" + sName + "'";
            sql = sql + ",name)> 0 and  userLabel =  ";
            sql = sql + "'" + csSmokeCompartment + "'";
            SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                SmokeCompartment room = new SmokeCompartment(Convert.ToInt64(reader["Id"].ToString()));
                room.name = reader["name"].ToString();
                room.m_dHeight = Convert.ToDouble(reader["dHeight"].ToString());
                room.m_dArea = Convert.ToDouble(reader["dArea"].ToString());
                room.m_iNumberOfPeople = Convert.ToInt32(reader["nNumberOfPeople"].ToString());
                //room.m_dMaxlength
                //     room.m_dVolume
                //    room.m_eRoomPosition
                room.type = reader["userLabel"].ToString();
                sql = "select * from Storeys where  Id =  ";
                sql = sql + reader["storeyId"].ToString();
                SQLiteCommand command1 = new SQLiteCommand(sql, dbConnection);
                SQLiteDataReader reader1 = command1.ExecuteReader();

                if (reader1.Read())
                {
                    room.m_iStoryNo = Convert.ToInt32(reader1["storeyNo"].ToString());
                }
                smokeCompartments.Add(room);
            }
            //关闭连接
            dbConnection.Close();

            return smokeCompartments;
        }



        public static List<FireCompartment> GetFireCompartment(string sName)
        {
            List<FireCompartment> smokeCompartments = new List<FireCompartment>();
            if (!System.IO.File.Exists(m_archXdbPath))
                return smokeCompartments;

            //创建一个连接
            string connectionstr = @"data source =" + m_archXdbPath;
            SQLiteConnection dbConnection = new SQLiteConnection(connectionstr);
            dbConnection.Open();
            string csSmokeCompartment = "防火分区";
            string sql = "select * from Spaces where CHARINDEX(";
            sql = sql + "'" + sName + "'";
            sql = sql + ",name)> 0 and  userLabel = csSmokeCompartment ";

            SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                FireCompartment room = new FireCompartment(Convert.ToInt64(reader["Id"].ToString()));
                room.name = reader["name"].ToString();
                room.m_dHeight = Convert.ToDouble(reader["dHeight"].ToString());
                room.m_dArea = Convert.ToDouble(reader["dArea"].ToString());
                room.m_iNumberOfPeople = Convert.ToInt32(reader["nNumberOfPeople"].ToString());
                //room.m_dMaxlength
                //     room.m_dVolume
                //    room.m_eRoomPosition
                room.type = reader["userLabel"].ToString();
                sql = "select * from Storeys where  Id =  ";
                sql = sql + reader["storeyId"].ToString();
                SQLiteCommand command1 = new SQLiteCommand(sql, dbConnection);
                SQLiteDataReader reader1 = command1.ExecuteReader();

                if (reader1.Read())
                {
                    room.m_iStoryNo = Convert.ToInt32(reader1["storeyNo"].ToString());
                }
                smokeCompartments.Add(room);
            }
            //关闭连接
            dbConnection.Close();

            return smokeCompartments;
        }


        public static List<FireCompartment> GetALLFireCompartment()
        {
            List<FireCompartment> smokeCompartments = new List<FireCompartment>();
            if (!System.IO.File.Exists(m_archXdbPath))
                return smokeCompartments;

            //创建一个连接
            string connectionstr = @"data source =" + m_archXdbPath;
            SQLiteConnection dbConnection = new SQLiteConnection(connectionstr);
            dbConnection.Open();
            string csSmokeCompartment = "防火分区";
            string sql = "select * from Spaces where userLabel = ";

            sql += csSmokeCompartment;

            SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                FireCompartment room = new FireCompartment(Convert.ToInt64(reader["Id"].ToString()));
                room.name = reader["name"].ToString();
                room.m_dHeight = Convert.ToDouble(reader["dHeight"].ToString());
                room.m_dArea = Convert.ToDouble(reader["dArea"].ToString());
                room.m_iNumberOfPeople = Convert.ToInt32(reader["nNumberOfPeople"].ToString());
                //room.m_dMaxlength
                //     room.m_dVolume
                //    room.m_eRoomPosition
                room.type = reader["userLabel"].ToString();
                sql = "select * from Storeys where  Id =  ";
                sql = sql + reader["storeyId"].ToString();
                SQLiteCommand command1 = new SQLiteCommand(sql, dbConnection);
                SQLiteDataReader reader1 = command1.ExecuteReader();

                if (reader1.Read())
                {
                    room.m_iStoryNo = Convert.ToInt32(reader1["storeyNo"].ToString());
                }
                smokeCompartments.Add(room);
            }
            //关闭连接
            dbConnection.Close();

            return smokeCompartments;
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

        public static int GetHighestStoryNoOfRoom(Room room)
        {
            List<Wall> walls = new List<Wall>();
            int iStoryNo = -18;
            if (!System.IO.File.Exists(m_archXdbPath))
                return iStoryNo;

            //创建一个连接
            string connectionstr = @"data source =" + m_archXdbPath;
            SQLiteConnection dbConnection = new SQLiteConnection(connectionstr);
            dbConnection.Open();
            string sql = "select* from Spaces where Id = ";
            sql = sql + room.Id;

            SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();

            if (reader.Read())
            {
                sql = "select * from Storeys where  Id =  ";
                sql = sql + reader["storeyId"].ToString();

                SQLiteCommand command1 = new SQLiteCommand(sql, dbConnection);
                SQLiteDataReader reader1 = command1.ExecuteReader();

                if (reader1.Read())
                {
                    iStoryNo = Convert.ToInt32(reader1["storeyNo"].ToString());

                    double dHighestElevation = Convert.ToDouble(reader1["elevation"].ToString()) + Convert.ToDouble(reader["dHeight"].ToString());
                    Floor floor = new Floor(0);
                    floor.elevation = dHighestElevation;
                    List<Floor> floors = GetFloors();
                    floors.Add(floor);
                    floors.Sort(new Icp());
                    int imatch = floors.FindIndex(a => a.Id == 0);
                    iStoryNo = floors[imatch + 1].FloorNumber;
                }
            }
            //关闭连接
            dbConnection.Close();
            return iStoryNo;
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
            string sql = "select * from Spaces where userLabel = csSmokeCompartment ";
            SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {

                fireCompartment.boundaryLoops = reader["boundaryLoops"].ToString();
                fireCompartment.name = reader["name"].ToString();
                fireCompartment.m_dHeight = Convert.ToDouble(reader["dHeight"].ToString());
                fireCompartment.m_dArea = Convert.ToDouble(reader["dArea"].ToString());
                fireCompartment.m_iNumberOfPeople = Convert.ToInt32(reader["nNumberOfPeople"].ToString());
                //room.m_dMaxlength
                //     room.m_dVolume
                //    room.m_eRoomPosition
                fireCompartment.type = reader["userLabel"].ToString();
                sql = "select * from Storeys where  Id =  ";
                sql = sql + reader["storeyId"].ToString();
                SQLiteCommand command1 = new SQLiteCommand(sql, dbConnection);
                SQLiteDataReader reader1 = command1.ExecuteReader();

                if (reader1.Read())
                {
                    fireCompartment.m_iStoryNo = Convert.ToInt32(reader1["storeyNo"].ToString());
                }
                //Polygon2D poly = GetSpaceBBox(fireCompartment.boundaryLoops, fireCompartment.Id.ToString());
                AABB aabbRoom = GetAABB(reader, dbConnection);
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
                    AABB aabbAirtermianl = GetAABB(reader, dbConnection);

                    if (aabbRoom.IsAABBContainsAABB3D(aabbAirtermianl))
                    {
                        return fireCompartment;
                    }
                    else if (Geometry_Utils_BBox.IsBBoxIntersectsBBox3D(aabbRoom, aabbAirtermianl))
                    {

                        return fireCompartment;
                    }
                }
            }

            //关闭连接
            dbConnection.Close();
            return fireCompartment;
        }

        public static bool IsAirTermianlInFireDistrict(AirTerminal airTerminal, FireCompartment fireDistrict)
        {
            if (!System.IO.File.Exists(m_archXdbPath))
                return false;

            //创建一个连接
            string connectionstr = @"data source =" + m_archXdbPath;
            SQLiteConnection dbConnection = new SQLiteConnection(connectionstr);
            dbConnection.Open();
            string sql = "select * from Spaces Where Id = ";
            sql = sql + fireDistrict.Id;
            SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();

            if (reader.Read())
            {
                // fireDistrict.name = reader["name"].ToString();
                // fireDistrict.boundaryLoops = reader["boundaryLoops"].ToString();
                // Polygon2D poly = GetSpaceBBox(fireDistrict.boundaryLoops, fireDistrict.Id.ToString());

                AABB aabbFireDistrict = GetAABB(reader, dbConnection);

                //创建一个连接
                connectionstr = @"data source =" + m_hvacXdbPath;
                SQLiteConnection dbConnectionHVAC = new SQLiteConnection(connectionstr);
                dbConnectionHVAC.Open();
                sql = "select * from AirTerminals Where Id =";
                sql = sql + airTerminal.Id;
                SQLiteCommand commandHVAC = new SQLiteCommand(sql, dbConnectionHVAC);
                SQLiteDataReader readerAirTerminals = commandHVAC.ExecuteReader();
                while (readerAirTerminals.Read())
                {

                    AABB aabbAirterminal = GetAABB(readerAirTerminals, dbConnectionHVAC);
                    //string sTransformer = readerAirTerminals["transformer"].ToString();

                    //sql = "select * from LODRelations where graphicElementId = ";
                    //sql = sql + readerAirTerminals["Id"].ToString();

                    //SQLiteCommand commandHVAC1 = new SQLiteCommand(sql, dbConnectionHVAC);
                    //SQLiteDataReader readerHVAC1 = commandHVAC1.ExecuteReader();
                    //while (readerHVAC1.Read())
                    //{
                    //    sql = "select * from Geometrys where Id = ";
                    //    sql = sql + readerHVAC1["geometryId"].ToString();
                    //    SQLiteCommand commandHVACGeo = new SQLiteCommand(sql, dbConnectionHVAC);
                    //    SQLiteDataReader readerHVACGeo = commandHVACGeo.ExecuteReader();
                    //    while (readerHVACGeo.Read())
                    //    {
                    //        readerHVACGeo["Id"].ToString();

                    //        Geometry geo = new Geometry();
                    //        geo.Id = Convert.ToInt64(readerHVACGeo["Id"].ToString());
                    //        geo.vertices = readerHVACGeo["vertices"].ToString();
                    //        geo.vertexIndexes = readerHVACGeo["vertexIndexes"].ToString();
                    //        geo.normals = readerHVACGeo["normals"].ToString();
                    //        geo.normalIndexes = readerHVACGeo["normalIndexes"].ToString();
                    //        geo.textrueCoords = readerHVACGeo["textrueCoords"].ToString();
                    //        geo.textrueCoordIndexes = readerHVACGeo["textrueCoordIndexes"].ToString();
                    //        geo.materialIds = readerHVACGeo["materialIds"].ToString();

                    //AABB aabb = GeometryFunction.GetGeometryBBox(geo, sTransformer);

                    //PointInt pt = aabb.Center();

                    if (aabbFireDistrict.IsAABBContainsAABB3D(aabbAirterminal))
                    {
                        return true;
                    }
                    else if (Geometry_Utils_BBox.IsBBoxIntersectsBBox3D(aabbFireDistrict, aabbAirterminal))
                    {

                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            //关闭连接
            dbConnection.Close();
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
                // room.name = reader["name"].ToString();
                // room.boundaryLoops = reader["boundaryLoops"].ToString();
                //   Polygon2D poly = GetSpaceBBox(room.boundaryLoops, room.Id.ToString());

                AABB aabbRoom = GetAABB(reader, dbConnection);

                //创建一个连接
                connectionstr = @"data source =" + m_hvacXdbPath;
                SQLiteConnection dbConnectionHVAC = new SQLiteConnection(connectionstr);
                dbConnectionHVAC.Open();
                string csSmokeCompartment = "防烟分区";
                sql = "select * from Spaces where userLabel = csSmokeCompartment ";
                SQLiteCommand commandHVAC = new SQLiteCommand(sql, dbConnectionHVAC);
                SQLiteDataReader readerAirTerminals = commandHVAC.ExecuteReader();
                while (readerAirTerminals.Read())
                {
                    // room.name = readerAirTerminals["name"].ToString();
                    // room.boundaryLoops = readerAirTerminals["boundaryLoops"].ToString();
                    //   Polygon2D polySmokeCompartment = GetSpaceBBox(room.boundaryLoops, room.Id.ToString());
                    AABB polySmokeCompartment = GetAABB(readerAirTerminals, dbConnection);


                    if (aabbRoom.IsAABBContainsAABB3D(polySmokeCompartment))
                    {
                        SmokeCompartment fireCompartment = new SmokeCompartment(Convert.ToInt64(readerAirTerminals["Id"].ToString()));
                        fireCompartment.boundaryLoops = reader["boundaryLoops"].ToString();
                        fireCompartment.name = reader["name"].ToString();
                        fireCompartment.m_dHeight = Convert.ToDouble(reader["dHeight"].ToString());
                        fireCompartment.m_dArea = Convert.ToDouble(reader["dArea"].ToString());
                        fireCompartment.m_iNumberOfPeople = Convert.ToInt32(reader["nNumberOfPeople"].ToString());
                        //room.m_dMaxlength
                        //     room.m_dVolume
                        //    room.m_eRoomPosition
                        fireCompartment.type = reader["userLabel"].ToString();
                        sql = "select * from Storeys where  Id =  ";
                        sql = sql + reader["storeyId"].ToString();
                        SQLiteCommand command1 = new SQLiteCommand(sql, dbConnection);
                        SQLiteDataReader reader1 = command1.ExecuteReader();

                        if (reader1.Read())
                        {
                            fireCompartment.m_iStoryNo = Convert.ToInt32(reader1["storeyNo"].ToString());
                        }
                        smokeCompartments.Add(fireCompartment);
                    }
                    else if (Geometry_Utils_BBox.IsBBoxIntersectsBBox3D(aabbRoom, polySmokeCompartment))
                    {
                        SmokeCompartment fireCompartment = new SmokeCompartment(Convert.ToInt64(readerAirTerminals["Id"].ToString()));
                        fireCompartment.boundaryLoops = reader["boundaryLoops"].ToString();
                        fireCompartment.name = reader["name"].ToString();
                        fireCompartment.m_dHeight = Convert.ToDouble(reader["dHeight"].ToString());
                        fireCompartment.m_dArea = Convert.ToDouble(reader["dArea"].ToString());
                        fireCompartment.m_iNumberOfPeople = Convert.ToInt32(reader["nNumberOfPeople"].ToString());
                        //room.m_dMaxlength
                        //     room.m_dVolume
                        //    room.m_eRoomPosition
                        fireCompartment.type = reader["userLabel"].ToString();
                        sql = "select * from Storeys where  Id =  ";
                        sql = sql + reader["storeyId"].ToString();
                        SQLiteCommand command1 = new SQLiteCommand(sql, dbConnection);
                        SQLiteDataReader reader1 = command1.ExecuteReader();

                        if (reader1.Read())
                        {
                            fireCompartment.m_iStoryNo = Convert.ToInt32(reader1["storeyNo"].ToString());
                        }
                        smokeCompartments.Add(fireCompartment);
                    }

                }
            }
            //关闭连接
            dbConnection.Close();

            return smokeCompartments;
        }

        public static bool IsSmokeCompartmentIntersectFireCompartment(SmokeCompartment smokeCompartment, FireCompartment fireCompartment)
        {
            if (!System.IO.File.Exists(m_archXdbPath))
                return false;

            //创建一个连接
            string connectionstr = @"data source =" + m_archXdbPath;
            SQLiteConnection dbConnection = new SQLiteConnection(connectionstr);
            dbConnection.Open();
            string sql = "select * from Spaces Where Id = ";
            sql = sql + smokeCompartment.Id;
            SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();

            if (reader.Read())
            {

                AABB aabbSmokeCompartment = GetAABB(reader, dbConnection);
                //创建一个连接
                connectionstr = @"data source =" + m_hvacXdbPath;
                SQLiteConnection dbConnectionHVAC = new SQLiteConnection(connectionstr);
                dbConnectionHVAC.Open();
                sql = "select * from Spaces Where Id = ";
                sql = sql + fireCompartment.Id;
                SQLiteCommand commandHVAC = new SQLiteCommand(sql, dbConnectionHVAC);
                SQLiteDataReader readerAirTerminals = commandHVAC.ExecuteReader();
                while (readerAirTerminals.Read())
                {

                    AABB aabbFireCompartment = GetAABB(reader, dbConnection);


                    if (Geometry_Utils_BBox.IsBBoxIntersectsBBox3D(aabbSmokeCompartment, aabbFireCompartment))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                }
            }
            //关闭连接
            dbConnection.Close();
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
            FindFireDampers(dbConnection, duct.Id.ToString(), fireDampers);
            //关闭连接
            dbConnection.Close();

            return fireDampers;
        }


        public static void FindFireDampers(SQLiteConnection dbConnection, String strId, List<FireDamper> inlets)
        {
            string sql = "select * from MepConnectionRelations Where mainElementId = ";
            sql += strId;
            SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                if (!m_listStrLastId.Exists(x => x == reader["linkElementId"].ToString()))
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
                    else
                    {
                        m_listStrLastId.Add(strId);
                        FindFireDampers(dbConnection, reader["linkElementId"].ToString(), inlets);
                    }
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
            string sql = "select * from Doors";
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
            if (!System.IO.File.Exists(m_archXdbPath))
                return ducts;

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
                AABB aabbRoom = GetAABB(reader, dbConnection);

                sql = "select * from Ducts";
                SQLiteCommand commandDucts = new SQLiteCommand(sql, dbConnection);
                SQLiteDataReader readerDucts = commandDucts.ExecuteReader();
                while (readerDucts.Read())
                {
                    AABB aabbAirTerminal = GetAABB(readerDucts, dbConnection);
                    if (Geometry_Utils_BBox.IsBBoxIntersectsBBox3D(aabbRoom, aabbAirTerminal))
                    {
                        Duct duct = new Duct(Convert.ToInt64(readerDucts["Id"].ToString()));
                        duct.airVelocity = Convert.ToDouble(readerDucts["Velocity"].ToString());
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

                        duct.airVelocity = Convert.ToDouble(readerDucts["Velocity"].ToString());

                        AABB aabbDuct = GetAABB(readerDucts, dbConnection);


                        ducts.Add(duct);
                        m_listStrLastId.Add(strId);
                        FindVerticalDucts(dbConnection, readerDucts["Id"].ToString(), ducts);
                    }
                    else
                    {
                        m_listStrLastId.Add(strId);
                        FindVerticalDucts(dbConnection, reader["linkElementId"].ToString(), ducts);
                    }
                }
            }
        }



        public List<Duct> GetBranchDamperDucts()
        {
            List<Duct> ducts = new List<Duct>();
            List<AirTerminal> airtermials = GetAirterminals("排烟");
            //建立樹結構
            TreeNode lastNode = new TreeNode();
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

                string sql = "select * from MepConnectionRelations Where mainElementId = ";
                sql += strId;
                SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
                SQLiteDataReader reader = command.ExecuteReader();        
                while (reader.Read())
                {
                    newNode = new TreeNode();
                    //lastNode.addFather(newNode);     
                    if (reader["linkElementId"].ToString() != m_strLastId)
                    {
                        Duct duct = new Duct(-1);
                        DuctElbow ductElbow = new DuctElbow(-1);
                        DuctReducer ductReducer = new DuctReducer(-1);
                        DuctDamper ductDamper = new DuctDamper(-1);
                        DuctSoft ductSoft = new DuctSoft(-1);
                        FlexibleShortTube flexibleShortTube = new FlexibleShortTube(-1);
                        Duct3T duct3t = new Duct3T(-1);
                        Duct4T duct4t = new Duct4T(-1);

                        if (GetDuct(reader, dbConnection, duct))
                        {
                            newNode.Id = duct.Id;
                            newNode.DirectNode = lastNode;
                        }
                        else if (GetDuctElbow(reader, dbConnection, ductElbow))
                        {
                            newNode.DirectNode = lastNode;
                            newNode.strfireAirea = GetSmokeCompartmentOfDuctElbow(ductElbow).Id;
                        }
                        else if (GetDuctReducer(reader, dbConnection, ductReducer))
                        {
                            newNode.DirectNode = lastNode;
                        }
                        else if (GetDuctDamper(reader, dbConnection, ductDamper))
                        {
                            newNode.DirectNode = lastNode;
                        }
                        else if (GetDuctSoft(reader, dbConnection, ductSoft))
                        {
                            newNode.DirectNode = lastNode;
                        }
                        else if (GetFlexibleShortTube(reader, dbConnection, flexibleShortTube))
                        {
                            newNode.DirectNode = lastNode;
                        }
                        else if (GetDuct3T(reader, dbConnection, duct3t))
                        {
                            sql = "select * from MepConnectionRelations Where MainElementId = ";
                            strId = Convert.ToString(newNode.Id);
                            sql += strId;

                            sql += "and = linkElementId = ";
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
                            }
                            GetSmokeCompartmentOfDuct3T(duct3t);
                        }
                        else if (GetDuct4T(reader, dbConnection, duct4t))
                        {
                            sql = "select * from MepConnectionRelations Where MainElementId = ";
                            strId = Convert.ToString(newNode.Id);
                            sql += strId;

                            sql += "and = linkElementId = ";
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
                            GetSmokeCompartmentOfDuct4T(duct4t);
                        }


                        lastNode = newNode;


                    }
                }

            }
                            





            PreOrderAddFireArea(lastNode);


            List<TreeNode> airTerminalNodes = PreOrderAirterminalNode(lastNode);
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

            return ducts;
        }
        long lastFireid;
        bool PreOrderAddFireArea(TreeNode node)
        {
            if (node.Id < 0) return false;

            if (lastFireid != node.strfireAirea)
            {
                node.StrfireAireas.Add(lastFireid);
            }
           // lastFireid = node.strfireAirea;
            if (!PreOrder(node.LeftNode))
            {
                return false;
            }
            if (!PreOrder(node.DirectNode))
            {
                return false;
            }
            if (!PreOrder(node.RightNode))
            {
                return false;
            }
            return true;

        }




        bool PreOrder(TreeNode node)
        {
            if (node.Id < 0) return false;
            if (node.iType == 3 && node.iType == 4)
            {
                return false;
            }
            if (!PreOrder(node.LeftNode))
            {
                return false;
            }
            if (!PreOrder(node.DirectNode))
            {
                return false;
            }
            if (!PreOrder(node.RightNode))
            {
                return false;
            }
            return true;

        }



        bool PreOrderFind(TreeNode node,AirTerminal airterminal)
        {
            if (node.Id < 0) return false;
            if (node.Id == airterminal.Id)
            {
                return true;
            }
            if (PreOrder(node.LeftNode))
            {
                return true;
            }
            if (PreOrder(node.DirectNode))
            {
                return true;
            }
            if (PreOrder(node.RightNode))
            {
                return true;
            }
            return false;

        }

        List<TreeNode> PreOrderAirterminalNode(TreeNode node)
        {
            List<TreeNode> airTerminalNodes = new List<TreeNode>();
            if (node.Id < 0) return airTerminalNodes;
            if (node.iType == 0)
            {
                airTerminalNodes.Add(node);
            }
            PreOrderAirterminalNode(node.LeftNode);
            PreOrderAirterminalNode(node.DirectNode);
            PreOrderAirterminalNode(node.RightNode);
            return airTerminalNodes;
        }

        TreeNode GetParentEquel3T4T(TreeNode node, List<TreeNode> ducts)
        {
            if (node.Parent == null)
                return null;
            if (node.Parent.iType != 3 && node.Parent.iType != 4)
            {
                if (node.Parent.iType == 1)
                {
                    ducts.Add(node);
                }

                GetParentEquel3T4T(node, ducts);
            }
            else if (node.Parent.iType == 3 || node.Parent.iType == 4)
            {
                return node.Parent;
            }
            return null;
        }
        public static Room GetSmokeCompartmentOfDuctElbow(DuctElbow airTerminal)
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
                    AABB aabbRoom = GetAABB(readerRoom, dbConnectionArch);
                    if (aabbRoom.IsAABBContainsAABB3D(aabbTerminal))
                    {
                        room = new Room(Convert.ToInt64(readerRoom["Id"].ToString()));
                        room.name = reader["name"].ToString();
                        room.m_dHeight = Convert.ToDouble(reader["dHeight"].ToString());
                        room.m_dArea = Convert.ToDouble(reader["dArea"].ToString());
                        room.m_iNumberOfPeople = Convert.ToInt32(reader["nNumberOfPeople"].ToString());
                        //room.m_dMaxlength
                        //     room.m_dVolume
                        //    room.m_eRoomPosition
                        room.type = readerRoom["userLabel"].ToString();
                        sql = "select * from Storeys where  Id =  ";
                        sql = sql + reader["storeyId"].ToString();
                        SQLiteCommand command1 = new SQLiteCommand(sql, dbConnection);
                        SQLiteDataReader reader1 = command1.ExecuteReader();

                        if (reader1.Read())
                        {
                            room.m_iStoryNo = Convert.ToInt32(reader1["storeyNo"].ToString());
                        }
                    }
                    else if (Geometry_Utils_BBox.IsBBoxIntersectsBBox3D(aabbTerminal, aabbRoom))
                    {
                        string strVector = reader["NormalVector"].ToString();
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
                        dX = aabbRoom.Center().X - aabbTerminal.Center().X;
                        dY = aabbRoom.Center().Y - aabbTerminal.Center().Y;
                        dZ = aabbRoom.Center().Z - aabbTerminal.Center().Z;
                        listVectorAirterminalToRoom.Add(dX);
                        listVectorAirterminalToRoom.Add(dY);
                        listVectorAirterminalToRoom.Add(dZ);

                        double dDotProduct = listVector[0] * listVectorAirterminalToRoom[0] + listVector[1] * listVectorAirterminalToRoom[1] + listVector[2] * listVectorAirterminalToRoom[2];
                        if (dDotProduct < 0)
                        {
                            room = new Room(Convert.ToInt64(readerRoom["Id"].ToString()));
                            room.name = readerRoom["name"].ToString();
                            room.m_dHeight = Convert.ToDouble(readerRoom["dHeight"].ToString());
                            room.m_dArea = Convert.ToDouble(readerRoom["dArea"].ToString());
                            room.m_iNumberOfPeople = Convert.ToInt32(readerRoom["nNumberOfPeople"].ToString());
                            //room.m_dMaxlength
                            //     room.m_dVolume
                            //    room.m_eRoomPosition
                            room.type = readerRoom["userLabel"].ToString();
                            sql = "select * from Storeys where  Id =  ";
                            sql = sql + readerRoom["storeyId"].ToString();
                            SQLiteCommand command1 = new SQLiteCommand(sql, dbConnection);
                            SQLiteDataReader reader1 = command1.ExecuteReader();

                            if (reader1.Read())
                            {
                                room.m_iStoryNo = Convert.ToInt32(reader1["storeyNo"].ToString());
                            }
                        }

                    }
                }
            }
            //关闭连接               
            return room;
        }

        public static Room GetSmokeCompartmentOfDuct3T(Duct3T airTerminal)
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
                    AABB aabbRoom = GetAABB(readerRoom, dbConnectionArch);
                    if (aabbRoom.IsAABBContainsAABB3D(aabbTerminal))
                    {
                        room = new Room(Convert.ToInt64(readerRoom["Id"].ToString()));
                        room.name = reader["name"].ToString();
                        room.m_dHeight = Convert.ToDouble(reader["dHeight"].ToString());
                        room.m_dArea = Convert.ToDouble(reader["dArea"].ToString());
                        room.m_iNumberOfPeople = Convert.ToInt32(reader["nNumberOfPeople"].ToString());
                        //room.m_dMaxlength
                        //     room.m_dVolume
                        //    room.m_eRoomPosition
                        room.type = readerRoom["userLabel"].ToString();
                        sql = "select * from Storeys where  Id =  ";
                        sql = sql + reader["storeyId"].ToString();
                        SQLiteCommand command1 = new SQLiteCommand(sql, dbConnection);
                        SQLiteDataReader reader1 = command1.ExecuteReader();

                        if (reader1.Read())
                        {
                            room.m_iStoryNo = Convert.ToInt32(reader1["storeyNo"].ToString());
                        }
                    }
                    else if (Geometry_Utils_BBox.IsBBoxIntersectsBBox3D(aabbTerminal, aabbRoom))
                    {
                        string strVector = reader["NormalVector"].ToString();
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
                        dX = aabbRoom.Center().X - aabbTerminal.Center().X;
                        dY = aabbRoom.Center().Y - aabbTerminal.Center().Y;
                        dZ = aabbRoom.Center().Z - aabbTerminal.Center().Z;
                        listVectorAirterminalToRoom.Add(dX);
                        listVectorAirterminalToRoom.Add(dY);
                        listVectorAirterminalToRoom.Add(dZ);

                        double dDotProduct = listVector[0] * listVectorAirterminalToRoom[0] + listVector[1] * listVectorAirterminalToRoom[1] + listVector[2] * listVectorAirterminalToRoom[2];
                        if (dDotProduct < 0)
                        {
                            room = new Room(Convert.ToInt64(readerRoom["Id"].ToString()));
                            room.name = readerRoom["name"].ToString();
                            room.m_dHeight = Convert.ToDouble(readerRoom["dHeight"].ToString());
                            room.m_dArea = Convert.ToDouble(readerRoom["dArea"].ToString());
                            room.m_iNumberOfPeople = Convert.ToInt32(readerRoom["nNumberOfPeople"].ToString());
                            //room.m_dMaxlength
                            //     room.m_dVolume
                            //    room.m_eRoomPosition
                            room.type = readerRoom["userLabel"].ToString();
                            sql = "select * from Storeys where  Id =  ";
                            sql = sql + readerRoom["storeyId"].ToString();
                            SQLiteCommand command1 = new SQLiteCommand(sql, dbConnection);
                            SQLiteDataReader reader1 = command1.ExecuteReader();

                            if (reader1.Read())
                            {
                                room.m_iStoryNo = Convert.ToInt32(reader1["storeyNo"].ToString());
                            }
                        }

                    }
                }
            }
            //关闭连接               
            return room;
        }

        public static Room GetSmokeCompartmentOfDuct4T(Duct4T airTerminal)
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
                    AABB aabbRoom = GetAABB(readerRoom, dbConnectionArch);
                    if (aabbRoom.IsAABBContainsAABB3D(aabbTerminal))
                    {
                        room = new Room(Convert.ToInt64(readerRoom["Id"].ToString()));
                        room.name = reader["name"].ToString();
                        room.m_dHeight = Convert.ToDouble(reader["dHeight"].ToString());
                        room.m_dArea = Convert.ToDouble(reader["dArea"].ToString());
                        room.m_iNumberOfPeople = Convert.ToInt32(reader["nNumberOfPeople"].ToString());
                        //room.m_dMaxlength
                        //     room.m_dVolume
                        //    room.m_eRoomPosition
                        room.type = readerRoom["userLabel"].ToString();
                        sql = "select * from Storeys where  Id =  ";
                        sql = sql + reader["storeyId"].ToString();
                        SQLiteCommand command1 = new SQLiteCommand(sql, dbConnection);
                        SQLiteDataReader reader1 = command1.ExecuteReader();

                        if (reader1.Read())
                        {
                            room.m_iStoryNo = Convert.ToInt32(reader1["storeyNo"].ToString());
                        }
                    }
                    else if (Geometry_Utils_BBox.IsBBoxIntersectsBBox3D(aabbTerminal, aabbRoom))
                    {
                        string strVector = reader["NormalVector"].ToString();
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
                        dX = aabbRoom.Center().X - aabbTerminal.Center().X;
                        dY = aabbRoom.Center().Y - aabbTerminal.Center().Y;
                        dZ = aabbRoom.Center().Z - aabbTerminal.Center().Z;
                        listVectorAirterminalToRoom.Add(dX);
                        listVectorAirterminalToRoom.Add(dY);
                        listVectorAirterminalToRoom.Add(dZ);

                        double dDotProduct = listVector[0] * listVectorAirterminalToRoom[0] + listVector[1] * listVectorAirterminalToRoom[1] + listVector[2] * listVectorAirterminalToRoom[2];
                        if (dDotProduct < 0)
                        {
                            room = new Room(Convert.ToInt64(readerRoom["Id"].ToString()));
                            room.name = readerRoom["name"].ToString();
                            room.m_dHeight = Convert.ToDouble(readerRoom["dHeight"].ToString());
                            room.m_dArea = Convert.ToDouble(readerRoom["dArea"].ToString());
                            room.m_iNumberOfPeople = Convert.ToInt32(readerRoom["nNumberOfPeople"].ToString());
                            //room.m_dMaxlength
                            //     room.m_dVolume
                            //    room.m_eRoomPosition
                            room.type = readerRoom["userLabel"].ToString();
                            sql = "select * from Storeys where  Id =  ";
                            sql = sql + readerRoom["storeyId"].ToString();
                            SQLiteCommand command1 = new SQLiteCommand(sql, dbConnection);
                            SQLiteDataReader reader1 = command1.ExecuteReader();

                            if (reader1.Read())
                            {
                                room.m_iStoryNo = Convert.ToInt32(reader1["storeyNo"].ToString());
                            }
                        }

                    }
                }
            }
            //关闭连接               
            return room;
        }

        private bool GetDuct(SQLiteDataReader reader, SQLiteConnection dbConnection, Duct duct)
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


        private bool GetDuctReducer(SQLiteDataReader reader, SQLiteConnection dbConnection, DuctReducer duct)
        {
            string sql = "select * from Ducts Where Id = ";
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

        private bool GetDuctDamper(SQLiteDataReader reader, SQLiteConnection dbConnection, DuctDamper duct)
        {
            string sql = "select * from Ducts Where Id = ";
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


        private bool GetDuctSoft(SQLiteDataReader reader, SQLiteConnection dbConnection, DuctSoft duct)
        {
            string sql = "select * from Ducts Where Id = ";
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

        private bool GetFlexibleShortTube(SQLiteDataReader reader, SQLiteConnection dbConnection, FlexibleShortTube duct)
        {
            string sql = "select * from Ducts Where Id = ";
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

        private bool GetDuctElbow(SQLiteDataReader reader, SQLiteConnection dbConnection, DuctElbow duct)
        {
            string sql = "select * from Ducts Where Id = ";
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

        private bool GetDuct3T(SQLiteDataReader reader, SQLiteConnection dbConnection, Duct3T duct)
        {
            string sql = "select * from Ducts Where Id = ";
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

        private bool GetDuct4T(SQLiteDataReader reader, SQLiteConnection dbConnection, Duct4T duct)
        {
            string sql = "select * from Ducts Where Id = ";
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
     
      








    }
    [Flags]
    public enum RoomPosition { overground = 1, underground = 2, semi_underground = 4 }
}
#endif