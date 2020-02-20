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

namespace HVAC_Checker
{
    /// <summary>
    /// 曲线，XDB描述曲线的类
    /// </summary>
    public class XDBCurve
    {
        /// <summary>
        /// 曲线类型名称,如直线XDBBoundLine,曲线XDBEllipse等
        /// </summary>
        public string curveType { get; set; }
        /// <summary>
        /// 构造函数
        /// </summary>
        public XDBCurve()
        {
            curveType = GetType().Name;
        }
    };
    /// <summary>
    /// XDB三维点
    /// </summary>
    public class XDBXYZ
    {
        private static XDBXYZ m_zero = new XDBXYZ(0, 0, 0);
        /// <summary>
        /// Gets the first coordinate
        /// </summary>
        public double x { get; set; }
        /// <summary>
        /// Gets the second coordinate
        /// </summary>
        public double y { get; set; }
        /// <summary>
        /// Gets the third coordinate
        /// </summary>
        public double z { get; set; }

        /// <summary>
        /// 创建一个默认的XDBXYZ使用值0，0，0
        /// </summary>
        public XDBXYZ() { x = 0; y = 0; z = 0; }

        /// <summary>
        /// 创建一个带指定值的XDBXYZ
        /// </summary>
        /// <param name="dx">The first coordinate</param>
        /// <param name="dy">The second coordinate</param>
        /// <param name="dz">The third coordinate</param>
        public XDBXYZ(double dx, double dy, double dz) { x = dx; y = dy; z = dz; }

        /// <summary>
        /// The coordinate origin or zero vector
        /// </summary>
        public static XDBXYZ Zero { get { return m_zero; } }
    };
    /// <summary>
    /// 椭圆
    /// </summary>
    public class XDBEllipse : XDBCurve
    {
        /// <summary>
        /// 中心点
        /// </summary>
        public XDBXYZ center { get; set; }

        /// <summary>
        /// x轴正方向，该值可以是个长度
        /// </summary>
        public XDBXYZ vector0 { get; set; }

        /// <summary>
        /// y轴正方向，该值可以是个长度
        /// </summary>
        public XDBXYZ vector90 { get; set; }

        /// <summary>
        /// 起始角度,X轴正方向按逆时针旋转一定角度作为起点角度
        /// </summary>
        public double startAngle { get; set; }

        /// <summary>
        /// 扫掠角度，与起始角度按逆时针方向旋转了一定角度，扫掠的角度就是该扫掠角度
        /// </summary>
        public double sweepAngle { get; set; }
    };


    /// <summary>
    /// 有界直线，包含起终点
    /// </summary>
    public class XDBBoundLine : XDBCurve
    {
        /// <summary>
        /// 直线起点
        /// </summary>
        public XDBXYZ startPoint { get; set; }

        /// <summary>
        /// 直线终点
        /// </summary>
        public XDBXYZ endPoint { get; set; }
    };
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
                Room room = new Room();
                room.SetName(reader["name"].ToString());
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
            sql = sql + room.GetId();          
            SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
   
            if(reader.Read())
            {                
                room.SetName(reader["name"].ToString());
                room.boundaryLoops = reader["boundaryLoops"].ToString();
                Polygon2D poly = GetSpaceBBox(room.boundaryLoops, room.GetId().ToString());


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

                                AirTerminal airTerminal = new AirTerminal();
                                airTerminal.Id = Convert.ToInt64(readerAirTerminals["Id"].ToString());
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
        /// XDB二维点
        /// </summary>
        public class XDBUV
        {
            private static XDBUV m_zero = new XDBUV(0, 0);
            /// <summary>
            /// u
            /// </summary>
            public double u { get; set; }
            /// <summary>
            /// v
            /// </summary>
            public double v { get; set; }

            /// <summary>
            /// 创建一个默认的XDBUV使用值0，0
            /// </summary>
            public XDBUV() { u = 0; v = 0; }

            /// <summary>
            /// 创建一个带有指定值du，dv的XDBUV
            /// </summary>
            /// <param name="du">The first coordinate</param>
            /// <param name="dv">The second coordinate</param>
            public XDBUV(double du, double dv) { u = du; v = dv; }

            /// <summary>
            /// The coordinate origin or zero 2-D vector
            /// </summary>
            public static XDBUV Zero { get { return m_zero; } }
        }
        /// <summary>
        /// 二维多段线
        /// </summary>
        public class XDB2DPolyLine : XDBCurve
        {
            /// <summary>
            /// XDB多段线数据结构点，XDB2DPolyLine内部使用
            /// </summary>
            public class XDBPolyLinePoint2D
            {
                /// <summary>
                /// 点
                /// </summary>
                public XDBUV pt { get; set; }
                /// <summary>
                /// 凸度
                /// </summary>
                public double bulge { get; set; }
            }

            private List<XDBPolyLinePoint2D> m_points = new List<XDBPolyLinePoint2D>();
            private bool m_bIsClosed = false;
            /// <summary>
            /// 多段线点和凸度集合
            /// </summary>
            public List<XDBPolyLinePoint2D> points { get { return m_points; } set { m_points = value; } }

            /// <summary>
            /// 是否闭合
            /// </summary>
            public bool IsClosed { get { return m_bIsClosed; } set { m_bIsClosed = value; } }

            /// <summary>
            /// 添加多段线集合
            /// </summary>
            /// <param name="pt">要添加的多段线的点</param>
            /// <param name="bulge">要添加的多段线的点的凸度，
            /// 凸度是用于指定当前顶点的平滑性，其被定义为：选取顶点与下一个顶点形成的弧之间角度的四分之一的正切值。
            /// 凸度可以用来设置多段线某一段的凸出参数，0表示直线，1表示半圆，介于0～1之间为劣弧，大于1为优弧</param>
            public void addVertex(XDBUV pt, double bulge = 0)
            {
                XDBPolyLinePoint2D point = new XDBPolyLinePoint2D();
                point.pt = new XDBUV(pt.u, pt.v);
                point.bulge = bulge;

                m_points.Add(point);
            }

            /// <summary>
            /// 设置当前多段线是否闭合
            /// </summary>
            /// <param name="bClosed"></param>
            public void SetClosed(bool bClosed)
            {
                m_bIsClosed = bClosed;
            }
        }

        /// <summary>
        /// 三维多段线
        /// </summary>
        public class XDB3DPolyLine : XDBCurve
        {
            /// <summary>
            /// XDB多段线数据结构点，XDB3DPolyLine内部使用
            /// </summary>
            public class XDBPolyLinePoint3D
            {
                /// <summary>
                /// 当前弧形所在多段线的索引值，从0开始
                /// </summary>
                public int hasArcIndex { get; set; }

                /// <summary>
                /// 圆弧上一点
                /// </summary>
                public XDBXYZ ptOnArc { get; set; }
            }

            private List<XDBXYZ> m_points = new List<XDBXYZ>();
            private List<XDBPolyLinePoint3D> m_Index2Arc = new List<XDBPolyLinePoint3D>();

            /// <summary>
            /// 多段线点集合，只包含直线、弧线的起终点
            /// </summary>
            public List<XDBXYZ> points { get { return m_points; } set { m_points = value; } }

            /// <summary>
            /// 对于弧线，该属性记录了该弧线所在多段线的段标识等信息，具体请查看XDBPolyLinePoint3D
            /// </summary>
            public List<XDBPolyLinePoint3D> Index2Arc { get { return m_Index2Arc; } set { m_Index2Arc = value; } }

            /// <summary>
            /// 添加一个点到当前多段线上
            /// </summary>
            /// <param name="nIndex">当前要添加点所在多段线的索引，从0开始</param>
            /// <param name="pt">要添加的坐标值</param>
            /// <param name="ptOnArc">如果当前添加的是弧形段，则该项有值，添加的为弧形上一点的坐标</param>
            public void appendVertex(int nIndex, XDBXYZ pt, XDBXYZ ptOnArc = null)
            {
                m_points.Add(pt);
                if (null != ptOnArc)
                {
                    XDBPolyLinePoint3D point = new XDBPolyLinePoint3D();
                    point.hasArcIndex = nIndex;
                    point.ptOnArc = new XDBXYZ(ptOnArc.x, ptOnArc.y, ptOnArc.z);

                    m_Index2Arc.Add(point);
                }
            }
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

        //4找到风机的取风口或排风口对象//怎么判断风口前端后端
        static List<AirTerminal> GetExhaustAirTerminalsOfFan(Fan fan)
        {
            List<AirTerminal> airTerminals = new List<AirTerminal>();
            return airTerminals; 
        }

    //5找到大于一定长度的走道对象  double  “走道、走廊”    长度清华引擎 计算学院  张荷花




    //6找到一个风机的全部末端风口对象集合
    static List<AirTerminal> GetAirTerminalsOfFan(Fan fan)
    {
        List<AirTerminal> airTerminals = new List<AirTerminal>();

        return airTerminals;
    }



    //7找到穿越某些房间的风管对象集合  清华引擎 构件相交  包含
    static List<Duct> GetDuctsCrossSpace(Room room)
    {
        List<Duct> ducts = new List<Duct>();

        return ducts;
    }
    //8找到穿越防火分区的风管对象集合  userlable
    static List<Duct> GetDuctsCrossFireDistrict(FireDistrict fireDistrict)
    {
        List<Duct> ducts = new List<Duct>();
        return ducts;
    }
    //9找到穿越防火分隔处的变形缝两侧的风管集合  差变形缝对象

    static List<Duct> GetDuctsCrossFireSide()
    {
        List<Duct> ducts = new List<Duct>();

        return ducts;
    }


    //10获得构建所在房间的对象  几何 包含 遍历表都查
         
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
        sql = sql + window.GetID() ;
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
            Console.WriteLine("书名: " + reader["userLabel"]);
            Room room = new Room();
            rooms.Add(room);
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
            Room room = new Room();      
            room.SetName(reader["name"].ToString());
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
            Floor floor = new Floor();
            floor.SetStoreyName(reader["storeyName"].ToString());
            floors.Add(floor);

        }
               



        return floors;
    }
    //16获得风机所连接的所有风管集合  支路 干管  风口到末端 
    static List<Duct> GetDuctsOfFan(Fan fan)
    {
        List<Duct> ducts = new List<Duct>();
        return ducts;
    }
    //17判断是否风机所连风系统所有支路都连接了风口  //管堵
    static bool isAllBranchLinkingAirTerminal(Fan fan)
    {

        bool isLink = true;

        return isLink;

    }
    //18获得防烟分区长边长度  清华引擎  矩形替代弧形 
    static double GetFireDistrictLength(FireDistrict fan)
    {
        double dLength = 0.0;
        return dLength;
    }            
       
    }
    
}
enum RoomPosition { overground, underground, semi_underground }
