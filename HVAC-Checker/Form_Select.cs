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
    using Standard = Dictionary<string, itemChecher>;

    public partial class Form_Select : Form
    {
        public Form_Select()
        {
            InitializeComponent();
            initialStandardTable();
        }

        private void button_ArchXDB_Click(object sender, EventArgs e)
        {
            domain = Domain.Arch;
            openFileDialog.ShowDialog();
        }

        public string archXDB_FileName { get; set; } = null;
        public string mechXDB_FileName { get; set; } = null;

        public string standardCode { get; set; } = null;

        public string itemCode { get; set; } = null;

        private Domain domain { get; set; }

        enum Domain {Arch,Mech };

        private void button_MechXBD_Click(object sender, EventArgs e)
        {
            domain = Domain.Mech;
            openFileDialog.ShowDialog();
        }

        private void openFileDialog_FileOk(object sender, CancelEventArgs e)
        {
            if(domain==Domain.Arch)
            {
                archXDB_FileName = openFileDialog.FileName;
                textBox_ArchXDB.Text = openFileDialog.FileName;
            }
            else if(domain==Domain.Mech)
            {
                mechXDB_FileName = openFileDialog.FileName;
                textBox_MechXDB.Text = openFileDialog.FileName;
            }
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (treeView_select.SelectedNode.Parent != null)
            {
                textBox_standardCode.Text = treeView_select.SelectedNode.Parent.Text;
                textBox_itemCode.Text = treeView_select.SelectedNode.Text;
                standardCode = textBox_standardCode.Text;
                itemCode = textBox_itemCode.Text;
            }
        }

        private void button_Ok_Click(object sender, EventArgs e)
        {
            if (archXDB_FileName == null && mechXDB_FileName == null)
                MessageBox.Show("未指定XDB文件路径");
            else if (standardCode == null || itemCode == null)
                MessageBox.Show("未指定要审查的条文");
            else
            {
                Visible = false;
                HVACFunction hvacFunction = new HVACFunction(archXDB_FileName, mechXDB_FileName);
                BimReview result = runChecker(standardCode, itemCode);
                Form_showResult form_showResult = new Form_showResult();
                form_showResult.showResult(result);
                form_showResult.ShowDialog();
                Visible = true;
            }     
        }

        private static void initialStandardTable()
        {
            standards.Add("GB50016_2014", new Standard());
            standards["GB50016_2014"].Add("8_1_9", HVACChecker.GB50016_2014_8_1_9);
            standards["GB50016_2014"].Add("8_5_1", HVACChecker.GB50016_2014_8_5_1);
            standards["GB50016_2014"].Add("8_5_2", HVACChecker.GB50016_2014_8_5_2);
            standards["GB50016_2014"].Add("8_5_3", HVACChecker.GB50016_2014_8_5_3);
            standards["GB50016_2014"].Add("8_5_4", HVACChecker.GB50016_2014_8_5_4);
            standards["GB50016_2014"].Add("9_3_11", HVACChecker.GB50016_2014_9_3_11);
            standards["GB50016_2014"].Add("9_3_16", HVACChecker.GB50016_2014_9_3_16);
            standards.Add("GB50736_2012", new Standard());
            standards["GB50736_2012"].Add("6_3_6", HVACChecker.GB50736_2012_6_3_6);
            standards["GB50736_2012"].Add("6_6_5", HVACChecker.GB50736_2012_6_6_5);
            standards["GB50736_2012"].Add("6_6_7", HVACChecker.GB50736_2012_6_6_7);
            standards["GB50736_2012"].Add("6_6_13", HVACChecker.GB50736_2012_6_6_13);
            standards["GB50736_2012"].Add("7_4_13", HVACChecker.GB50736_2012_7_4_13);
            standards["GB50736_2012"].Add("9_1_5", HVACChecker.GB50736_2012_9_1_5);
            standards.Add("GB50189_2015", new Standard());
            standards["GB50189_2015"].Add("4_2_5", HVACChecker.GB50189_2015_4_2_5);
            standards["GB50189_2015"].Add("4_2_10", HVACChecker.GB50189_2015_4_2_10);
            standards["GB50189_2015"].Add("4_2_14", HVACChecker.GB50189_2015_4_2_14);
            standards["GB50189_2015"].Add("4_2_17", HVACChecker.GB50189_2015_4_2_17);
            standards["GB50189_2015"].Add("4_2_19", HVACChecker.GB50189_2015_4_2_19);
            standards["GB50189_2015"].Add("4_5_2", HVACChecker.GB50189_2015_4_5_2);
            standards.Add("GB51251_2017", new Standard());
            standards["GB51251_2017"].Add("3_1_2", HVACChecker.GB51251_2017_3_1_2);
            standards["GB51251_2017"].Add("3_1_4", HVACChecker.GB51251_2017_3_1_4);
            standards["GB51251_2017"].Add("3_1_5", HVACChecker.GB51251_2017_3_1_5);
            standards["GB51251_2017"].Add("3_2_1", HVACChecker.GB51251_2017_3_2_1);
            standards["GB51251_2017"].Add("3_2_2", HVACChecker.GB51251_2017_3_2_2);
            standards["GB51251_2017"].Add("3_2_3", HVACChecker.GB51251_2017_3_2_3);
            standards["GB51251_2017"].Add("3_3_1", HVACChecker.GB51251_2017_3_3_1);
            standards["GB51251_2017"].Add("3_3_7", HVACChecker.GB51251_2017_3_3_7);
            standards["GB51251_2017"].Add("3_3_11", HVACChecker.GB51251_2017_3_3_11);
            standards["GB51251_2017"].Add("4_2_4", HVACChecker.GB51251_2017_4_2_4);
            standards["GB51251_2017"].Add("4_4_1", HVACChecker.GB51251_2017_4_4_1);
            standards["GB51251_2017"].Add("4_4_2", HVACChecker.GB51251_2017_4_4_2);
            standards["GB51251_2017"].Add("4_4_7", HVACChecker.GB51251_2017_4_4_7);
            standards["GB51251_2017"].Add("4_4_10", HVACChecker.GB51251_2017_4_4_10);
            standards["GB51251_2017"].Add("4_5_1", HVACChecker.GB51251_2017_4_5_1);
            standards["GB51251_2017"].Add("4_5_2", HVACChecker.GB51251_2017_4_5_2);
            standards["GB51251_2017"].Add("4_5_6", HVACChecker.GB51251_2017_4_5_6);
            standards.Add("GB50067_2014", new Standard());
            standards["GB50067_2014"].Add("8_2_1", HVACChecker.GB50067_2014_8_2_1);
            standards["GB50067_2014"].Add("8_2_2", HVACChecker.GB50067_2014_8_2_2);
            standards.Add("GB50157_2013", new Standard());
            standards["GB50157_2013"].Add("28_4_2", HVACChecker.GB50157_2013_28_4_2);
            standards["GB50157_2013"].Add("28_4_22", HVACChecker.GB50157_2013_28_4_22);
            standards.Add("GB50490_2009", new Standard());
            standards["GB50490_2009"].Add("8_4_17", HVACChecker.GB50490_2009_8_4_17);
            standards["GB50490_2009"].Add("8_4_19", HVACChecker.GB50490_2009_8_4_19);
            standards.Add("GB50041_2008", new Standard());
            standards["GB50041_2008"].Add("15_3_7", HVACChecker.GB50041_2008_15_3_7);
        }

        private static BimReview runChecker(string standardCode, string itemCode)
        {
           
            if (!standards.ContainsKey(standardCode))
                throw new ArgumentException("规范编号有误");
            if (!standards[standardCode].ContainsKey(itemCode))
                throw new ArgumentException("条文编号有误");
            return standards[standardCode][itemCode]();
        }

        private static Dictionary<string, Standard> standards = new Dictionary<string, Standard>();

    }
}
