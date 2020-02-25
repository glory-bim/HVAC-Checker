using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace HVAC_CheckEngine
{
    public class HVACChecker
    {
        /**
     建筑设计防火规范 GB50016-2014：8.5.1条文：
     建筑的下列场所或部位应设置防烟设施：
     1 防烟楼梯间及其前室；
     2 消防电梯间前室或合用前室；
     3 避难走道的前室、避难层(间)。
     建筑高度不大于50m的公共建筑、厂房、仓库和建筑高度不大于100m的住宅建筑，当其防烟楼梯间的前室或合用前室符合下列条件之一时，楼梯间可不设置防烟系统：
     1 前室或合用前室采用敞开的阳台、凹廊；
     2 前室或合用前室具有不同朝向的可开启外窗，且可开启外窗的面积满足自然排烟口的面积要求。
     */
        //获得建筑中所有防烟楼梯间、前室及避难间的集合
        //依次对以上房间进行如下判断：
        //如果房间中有正压送风口
        //      如果正压送风口未连接了风机，则在审查结果中注明审查不通过，并将当前房间信息加到违规构建列表中
        //如果房间中没有正压送风口
        //      如果房间中没有可开启外窗，则在审查结果中注明审查不通过，并将当前房间信息加到违规构建列表中
        //经过以上操作后，如果审查通过，则在审查结果中注明审查通过
        //                如果审查不通过，则在审查结果中注明审查未通过，并写明原因
        //返回审查结果

        private BimReview GB50016_2014_8_5_1()
        {
            BimReview result;
            result.isPassCheck = true;
            //获得建筑中所有防烟楼梯间、前室及避难间的集合
            List<Room> rooms = new List<Room>();
            rooms.AddRange(HVACFunction.GetRooms("防烟楼梯间"));
            rooms.AddRange(HVACFunction.GetRooms("前室"));
            rooms.AddRange(HVACFunction.GetRooms("避难间"));
            //依次对以上房间进行如下判断：
            foreach (Room room in rooms)
            {
                //如果房间中有正压送风口
                List<AirTerminal>airTerminals= HVACFunction.GetRoomContainAirTerminal(room);
                //      如果正压送风口未连接了风机，则在审查结果中注明审查不通过，并将当前房间信息加到违规构建列表中
                //如果房间中没有正压送风口
                //      如果房间中没有可开启外窗，则在审查结果中注明审查不通过，并将当前房间信息加到违规构建列表中

            }

            //经过以上操作后，如果审查通过，则在审查结果中注明审查通过
            //                如果审查不通过，则在审查结果中注明审查未通过，并写明原因
            //返回审查结果
        }
    }
    
}
