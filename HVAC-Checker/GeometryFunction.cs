using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BCGL.Sharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HVAC_CheckEngine
{
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
    /// 构件类型
    /// </summary>
    public enum ComponentCategory
    {
        /// <summary>
        /// 其它
        /// </summary>
        OTHER = 0,

        /// <summary>
        /// 墙体
        /// </summary>
        WALL = 1,

        /// <summary>
        /// 梁
        /// </summary>
        BEAM = 2,

        /// <summary>
        /// 板
        /// </summary>
        SLAB = 3,

        /// <summary>
        /// 柱
        /// </summary>
        COLUMN = 4,

        /// <summary>
        /// 区域
        /// </summary>
        SPACE = 5,

        /// <summary>
        /// 门
        /// </summary>
        DOOR = 6,

        /// <summary>
        /// 窗户
        /// </summary>
        WINDOW = 7,

        /// <summary>
        /// 坡屋顶
        /// </summary>
        SLOPINGROOF = 8,

        /// <summary>
        /// 平屋顶
        /// </summary>
        FLATROOF = 9,

        /// <summary>
        /// 场地出入口
        /// </summary>
        PASSAGEWAY = 10,

        /// <summary>
        /// 道路
        /// </summary>
        ROAD = 11,

        /// <summary>
        /// 室外场地
        /// </summary>
        OUTDOORSPACE = 12,

        /// <summary>
        /// 停车位
        /// </summary>
        PARKING = 13,

        /// <summary>
        /// 绿地
        /// </summary>
        GREENLAND = 14,

        /// <summary>
        /// 乔木
        /// </summary>
        TREE = 15,

        /// <summary>
        /// 用地界线
        /// </summary>
        LANDLINE = 16,

        /// <summary>
        /// 阳台
        /// </summary>
        BALCONY = 17,

        /// <summary>
        /// 飘窗
        /// </summary>
        BAYWINDOW = 18,

        /// <summary>
        /// 栏杆/栏板
        /// </summary>
        HANDRAILBREASTBOARD = 19,

        /// <summary>
        /// 雨棚
        /// </summary>
        PLATFORMAWNING = 20,

        /// <summary>
        /// 楼梯
        /// </summary>
        STAIR = 21,

        /// <summary>
        /// 勒脚
        /// </summary>
        PLINTH = 22,

        /// <summary>
        /// 室外台阶
        /// </summary>
        OUTDOORSTEPS = 23,

        /// <summary>
        /// 室外墙垛
        /// </summary>
        OUTDOORWALLBATTLEMENTS = 24,

        /// <summary>
        /// 操作平台
        /// </summary>
        OPERATIONPLATFORM = 25,

        /// <summary>
        /// 上料平台
        /// </summary>
        MATERIALPUTTINGPLATFORM = 26,

        /// <summary>
        /// 安装箱
        /// </summary>
        INSTALLATIONBOX = 27,

        /// <summary>
        /// 罐体平台
        /// </summary>
        TANKPLATFORM = 28,

        /// <summary>
        /// 屋顶水箱
        /// </summary>
        ATTICTANK = 29,

        /// <summary>
        /// 花架
        /// </summary>
        PERGOLA = 30,

        /// <summary>
        /// 独立烟囱
        /// </summary>
        DETACHEDCHIMNEY = 31,

        /// <summary>
        /// 烟道
        /// </summary>
        FLUEPIPE = 32,

        /// <summary>
        /// 地沟
        /// </summary>
        TRENCH = 33,

        /// <summary>
        /// 油(水)罐
        /// </summary>
        OILTANK = 34,

        /// <summary>
        /// 气柜
        /// </summary>
        GASHOLDER = 35,

        /// <summary>
        /// 水塔
        /// </summary>
        WATERTOWER = 36,

        /// <summary>
        /// 贮水(油)池
        /// </summary>
        WATERSTORAGETANK = 37,

        /// <summary>
        /// 贮仓
        /// </summary>
        SILO = 38,

        /// <summary>
        /// 栈桥
        /// </summary>
        TRESTLE = 39,

        /// <summary>
        /// 地下人防通道
        /// </summary>
        UNDERGROUNDTUNNEL = 40,

        /// <summary>
        /// 天桥（舞台及后台悬挂幕布、布景）
        /// </summary>
        OVERHESDBRIDGE = 41,

        /// <summary>
        /// 挑台（舞台及后台悬挂幕布、布景）
        /// </summary>
        COMPONENTCATEGORY_BALCONY = 42,

        /// <summary>
        /// 变形缝
        /// </summary>
        DEFORMATIONJOINT = 43,

        /// <summary>
        /// 室外构件
        /// </summary>
        OUTDOORCOMPONENTS = 44,

        /// <summary>
        /// 室外配件
        /// </summary>
        OUTDOORFITTINGS = 45,

        /// <summary>
        /// 散水
        /// </summary>
        APROLL = 46,

        /// <summary>
        /// 结构基础
        /// </summary>
        STRUCTUREFOUNDATION = 47,

        /// <summary>
        /// 避雷针
        /// </summary>
        LIGHTENINGROD = 48,

        /// <summary>
        /// 太阳能板
        /// </summary>
        SOLARPANEL = 49,

        /// <summary>
        /// 卫星锅
        /// </summary>
        SATELLITEDISHES = 50,

        /// <summary>
        /// 排风井
        /// </summary>
        EXHUSTSHAFT = 51,

        /// <summary>
        /// 进风井
        /// </summary>
        DOWNCASTSHAFT = 52,

        /// <summary>
        /// 电梯机房
        /// </summary>
        ELEVATORMAHINEROOM = 53,

        /// <summary>
        /// 风机
        /// </summary>
        DRAUGHTFAN = 54,

        /// <summary>
        /// 冷却塔
        /// </summary>
        COOLINGTOWER = 55,

        /// <summary>
        /// 配电箱
        /// </summary>
        DISTRIBUTIONBOX = 56,

        /// <summary>
        /// 管线
        /// </summary>
        PIPE = 57,

        /// <summary>
        /// 连接件
        /// </summary>
        FITTING = 58,

        /// <summary>
        /// 市政管线接口
        /// </summary>
        CITYPIPEINTERFACE = 59,

        /// <summary>
        /// 井
        /// </summary>
        WELL = 60,

        /// <summary>
        /// 场地地形
        /// </summary>
        SITETOPOGRAPHY = 61,

        /// <summary>
        /// 水池
        /// </summary>
        POOL = 62,

        /// <summary>
        /// 主体
        /// </summary>
        MainBody = 300,

        /// <summary>
        /// 桥墩
        /// </summary>
        BridgePier = 301,

        /// <summary>
        /// 场地围墙
        /// </summary>
        VenueWall,
    };

    public class Geometry 
    {
        public long Id { get; set; }
        /// <summary>
        /// 构件类别
        /// </summary>
        public ComponentCategory componentCategory { get; set; }

        /// <summary>
        /// 顶点坐标 存储的为List&lt;double&gt;的json串,
        /// 注意，顶点坐标为构件几何离散后描述三角面片的顶点，要求该顶点去重唯一
        /// </summary>
        public string vertices { get; set; }

        /// <summary>
        /// 顶点索引 存储的为List&lt;int&gt;的json串
        /// 索引值从0开始
        /// </summary>
        public string vertexIndexes { get; set; }

        /// <summary>
        /// 法向量 存储的为List&lt;float&gt;的json串
        /// 法向量坐标为三角面片三个顶点的法向，如果三个顶点的法向相同请全部输出，若一个三角面片只有一个法向（三个顶点法向相同），
        /// 则也按此方式进行输出，法向坐标也要求不重复唯一性
        /// </summary>
        public string normals { get; set; }

        /// <summary>
        /// 法向量索引 存储的为List&lt;int&gt;的json串
        /// 索引值从0开始
        /// </summary>
        public string normalIndexes { get; set; }

        /// <summary>
        /// 纹理坐标索引 存储的为List&lt;int&gt;的json串
        /// 纹理坐标是一个二维点，用于纹理贴图使用
        /// </summary>
        public string textrueCoordIndexes { get; set; }

        /// <summary>
        /// 纹理坐标索引 存储的为List&lt;float&gt;的json串
        /// </summary>
        public string textrueCoords { get; set; }

        /// <summary>
        /// 材质ID 存储的为List&lt;long&gt;的json串
        /// 一个三角面片要求有一个材料id
        /// </summary>
        public string materialIds { get; set; }
    };
    public class GeometryFunction
    {
        /// <summary>
        /// 根据房间边界曲线，获得房间的包围盒（默认AABB）
        /// </summary>
        /// <param name="boundaryLoops"></param>
        /// <param name="sSpaceId"></param>
        /// <returns>Polygon2D包围盒数据</returns>
        public static AABB GetGeometryBBox(Geometry geo, string transformer)
        {
            PointIntList ptlist = new PointIntList();
            ConvertVerticesToPointIntList(geo.vertices, transformer, ref ptlist);

            // 暂时全部采用A包围盒，O.S.P后续视情况添加
            AABB aabb = ptlist.GetAABB(geo.Id.ToString());
            //PointInt ptCenter = aabb.Center();
            return aabb;

            //OBB obb = ptlist.GetOBB(geo.Id.ToString());
           // obb.ToDataList_OBB();
        }


        public static OBB GetGeometryOBB(Geometry geo, string transformer)
        {
            PointIntList ptlist = new PointIntList();
            ConvertVerticesToPointIntList(geo.vertices, transformer, ref ptlist);

            // 暂时全部采用A包围盒，O.S.P后续视情况添加
          //  AABB aabb = ptlist.GetAABB(geo.Id.ToString());
            //PointInt ptCenter = aabb.Center();
           // return aabb;

            OBB obb = ptlist.GetOBB(geo.Id.ToString());
           // obb.ToDataList_OBB();
            return obb;
        }


        /// <summary>
        /// 根据element的几何图形的顶点数据和转换矩阵，获取其位置点集
        /// </summary>
        /// <param name="sVertices">顶点字符串</param>
        /// <param name="transformer">矩阵信息字符串</param>
        /// <param name="ptlist">返回的位置点集</param>
        public static void ConvertVerticesToPointIntList(string sVertices, string transformer, ref PointIntList ptlist)
        {
            bool bConverted = ConvertFromJsonString(sVertices, out List<double> vertices);
            if (!bConverted)
            {
                return;
            }

            int vertexCount = vertices.Count / 3;
            for (var i = 0; i < vertexCount; i++)
            {
                double x = vertices[3 * i];
                double y = vertices[3 * i + 1];
                double z = vertices[3 * i + 2];
                XYZ ptXYZ = new XYZ(x, y, z);
                XYZ ptChange = TransformPoint(ptXYZ, transformer);
                PointInt pt = new PointInt((int)ptChange.X, (int)ptChange.Y, (int)ptChange.Z);
                ptlist.Add(pt);
            }
        }

        /// <summary>
        /// 将点进行矩阵变换
        /// </summary>
        /// <param name="point">要转换的点</param>
        /// <param name="transformer">XDB中的转换矩阵字符串</param>
        /// <returns>转换后的点</returns>
        public static XYZ TransformPoint(XYZ point, string transformer)
        {
            List<double> output = JsonConvert.DeserializeObject<List<double>>(transformer);
            if (output.Count != 16)
            {
                return new XYZ(0, 0, 0);
            }

            // 列表内容如下
            //basicX.X,basicX.Y,basicX.Z,0,
            //basicY.X,basicY.Y,basicY.Z,0,
            //basicZ.X,basicZ.Y,basicZ.Z,0,
            //dOrigonX,dOrigonY,dOrigonZ,1,
            XYZ b0 = new XYZ(output[0], output[1], output[2]);
            XYZ b1 = new XYZ(output[4], output[5], output[6]);
            XYZ b2 = new XYZ(output[8], output[9], output[10]);
            XYZ origin = new XYZ(output[12], output[13], output[14]);

            double x = point.X;
            double y = point.Y;
            double z = point.Z;

            double xTemp = x * b0.X + y * b1.X + z * b2.X + origin.X;
            double yTemp = x * b0.Y + y * b1.Y + z * b2.Y + origin.Y;
            double zTemp = x * b0.Z + y * b1.Z + z * b2.Z + origin.Z;

            return new XYZ(xTemp, yTemp, zTemp);
        }
        /// <summary>
        /// json反序列化
        /// </summary>
        /// <typeparam name="T">反序列化时源数据的类型</typeparam>
        /// <param name="jsonString">json字符串</param>
        /// <param name="output">返回反序列化后的结果</param>
        /// <returns>true代表反序列化成功，false代表反序列化失败</returns>    
        public static bool ConvertFromJsonString<T>(string jsonString, out T output)
        {
            try
            {
                output = JsonConvert.DeserializeObject<T>(jsonString);
                return true;
            }
            catch (Exception ex)
            {
                output = default(T);
                Console.WriteLine(ex);
                return false;
            }
        }

    }
}
