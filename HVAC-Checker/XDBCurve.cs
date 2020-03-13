using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HVAC_CheckEngine
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
}
