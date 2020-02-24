using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HVAC_CheckEngine
{
	struct ComponentAnnotation
	{
		public long Id;       //构件id
		public string type;   //构件类型
		public long storeyID; //构件楼层
	};


	//批注管理的结构体，最后转换为json返回
	struct BimReview
	{
		public int compulsory;        //规范编号 
		public string comment;       //审查意见  【xxx条通过】
		public string standardCode; //规范编号  【3-2-1】
		List<ComponentAnnotation> componentAnnotations;  // 构件批注【无】	
	};
	class CheckResult
	{
		public CheckResult()
		{
			state = 1;
			message = null;
			data = new List<BimReview>();
		}
		public void addBimReview(BimReview bimReview)
		{
			data.Add(bimReview);
		}
		public int state { get; set;}
		public string message { get; set; }
		List<BimReview> data;
    }
}
