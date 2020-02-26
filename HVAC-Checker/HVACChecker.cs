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
        
        private BimReview GB50016_2014_8_5_1()
        {
            BimReview result=new BimReview();
            result.compulsory = "GB50016_2014";
            result.standardCode = "8.5.1";
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
                AirTerminal pressureAirTerminal = assistantFunctions.GetAirTerminalOfCertainSystem(airTerminals, "正压送风");
                if(pressureAirTerminal!=null)
                {
                    //      如果正压送风口未连接了风机，则在审查结果中注明审查不通过，并将当前房间信息加到违规构建列表中
                    List<Fan> fans = HVACFunction.GetFanConnectingAirterminal(pressureAirTerminal);
                    if(fans==null)
                    {
                        result.isPassCheck = false;
     
                        result.AddViolationComponent(room.Id.Value, room.type);
                    }
                }
                //如果房间中没有正压送风口
                else
                {
                    //      如果房间中没有可开启外窗，则在审查结果中注明审查不通过，并将当前房间信息加到违规构建列表中
                    List<Windows> windows = HVACFunction.GetWindowsInRoom(room);
                    Windows aimWindow = assistantFunctions.GetOpenableOuterWindow(windows);
                    if(aimWindow==null)
                    {
                        result.isPassCheck = false;
                        result.AddViolationComponent(room.Id.Value, room.type);
                    }
                }


            }
            //经过以上操作后，如果审查通过，则在审查结果中注明审查通过
            if (result.isPassCheck)
            {
                result.comment = "设计满足规范GB50016_2014中第8.5.1条条文规定";
            }
            //                如果审查不通过，则在审查结果中注明审查未通过，并写明原因
            else
            {
                result.comment = build_GB50016_2014_8_5_1_ViolationComment(ref result);
            }
            return result;
        }
        private string build_GB50016_2014_8_5_1_ViolationComment(ref BimReview result)
        {
            string comment = "设计不满足规范GB50016_2014中第8.5.1条条文规定";
            //如果公共建筑、厂房、仓库的建筑高度不大于50m的或住宅建筑的建筑高度不大于100m，则检查违规构件中是否有楼梯间
            if ((globalData.buildingType.Contains("公共建筑") || globalData.buildingType.Contains("厂房") || globalData.buildingType.Contains("仓库") && globalData.buildingHeight <= 50) ||
                    globalData.buildingType.Contains("住宅") && globalData.buildingHeight <= 100)
            {
                foreach (ComponentAnnotation component in result.ViolationComponents)
                {
                    //如果有楼梯间则在审查结果批注中加入请专家复核提示
                    if (component.type.Contains("楼梯间"))
                    {
                        comment += "请专家复核：未设置防烟设施的楼梯间前室或合用前室是否采用敞开的阳台、凹廊，或者前室或合用前室是否具有不同朝向的可开启外窗，且可开启外窗的面积满足自然排烟口的面积要求。";
                        break;
                    }
                }
            }
            return comment;
        }
    }
}
