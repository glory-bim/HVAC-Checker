﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HVAC_CheckEngine
{
    public class Window:Element
    {
        public Window(long id):base(id)
        {
        }
        public bool? isExternalWindow { get; set; } = null;//是否为外窗

        public bool? isSmokeExhaustWindow { get; set; } = null;//是否为排烟窗
        public double? area { get; set; } = null;

        public double? effectiveArea //有效面积在这里计算
        {
            get;set;
        }

       public WindowOpenMode? openMode { get; set; } = null;

        double? openingAngle { get; set; } = null;//开启角度

        public string sFaceOrient { get; set; } = null;

        public enum WindowOpenMode{HangWindow,SashWindow,BlindWindow,CasementWindow,PushWindow,FixWindow }//悬窗，推拉窗，百叶窗，平开窗，平推窗，固定窗
    }
}
