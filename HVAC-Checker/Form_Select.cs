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
    public partial class Form_Select : Form
    {
        public Form_Select()
        {
            InitializeComponent();
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
                Close();
                
        }
    }
}
