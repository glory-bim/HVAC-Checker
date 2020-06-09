using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HVAC_CheckEngine
{
	public struct ComponentAnnotation
	{
		public long Id;       //构件id
		public string type;   //构件类型
		public int storeyId;    //楼层id

	};


	//批注管理的结构体，最后转换为json返回
	public struct BimReview
	{
		public string compulsory;        //规范编号 
		public string comment;       //审查意见  【xxx条通过】
		public string standardCode; //规范编号  【3-2-1】
		public bool isPassCheck;    //是否通过审查
		public List<ComponentAnnotation> violationComponents;  // 违规构建【无】	
		public BimReview(string compulsory_input,string standardCode_input)
		{
			compulsory = compulsory_input;
			standardCode = standardCode_input;
			comment = string.Empty;
			isPassCheck = true;
			violationComponents = new List<ComponentAnnotation>();
		}
		public void AddViolationComponent(long Id,string componentType,int storeyId)
		{
			ComponentAnnotation componentAnnotation = new ComponentAnnotation();
			componentAnnotation.Id = Id;
			componentAnnotation.type = componentType;
			componentAnnotation.storeyId = storeyId;

			if (violationComponents == null)
			{
				violationComponents = new List<ComponentAnnotation>();
			}
			if(!violationComponents.Exists(x=>x.Id==componentAnnotation.Id))
				violationComponents.Add(componentAnnotation);
		}

	

		
	};
	class CheckResult
	{
		public int state { get; set; }//记录审查最终状态。1：审查程序运行正常。0：审查程序遇到异常
		public string message { get; set; }//存储审查程序最终状态的文本消息
		public List<BimReview> data;//记录了每个条文的审查结果
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
		

	}
}
