using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HVAC_CheckEngine
{
    public partial class Form_showResult : Form
    {
        public Form_showResult()
        {
            InitializeComponent();
            
        }

       public void showResult(BimReview result)
       {
            string text = null;
            int index = 0;
            text = "规范编号：" + result.standardCode+ "\n";
            text += "条文编号：" + result.compulsory + "\n";
            text +="审查结论："+result.comment + "\n";
            text += "违规构件如下：\n";
            foreach (ComponentAnnotation annotation in result.violationComponents)
            {
                ++index;
                text += "构件"+ index+":\n";
                text += "       构件ID：" + annotation.Id + "\n";
                text += "       批注：" + annotation.remark+ "\n\n";
            }
            richTextBox.Text = text;
        }
    }
}
