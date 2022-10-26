using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KLB_Monitor.window
{
    public partial class FrmErr : Form
    {
        public Action _ClearErrInfo;

        public string TipMsg
        {
            get { return tipLabel.Text; }
            set { tipLabel.Text = value; }
        }

        public FrmErr()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tipLabel_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Left)
            {
                DialogResult result = MessageBox.Show("是否处理完成？", "操作提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    _ClearErrInfo();
                    this.tipLabel.Text = string.Empty;
                    this.Close();
                }
                else if (result == DialogResult.No)
                {
                    //e.Cancel = true;
                }
                else if (result == DialogResult.Cancel)
                {
                    //e.Cancel = true;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FrmErr_FormClosing(object sender, FormClosingEventArgs e)
        {
            
        }

        private void FrmErr_Load(object sender, EventArgs e)
        {
            this.TopMost = true;
            this.WindowState = FormWindowState.Maximized;
            this.FormBorderStyle = FormBorderStyle.None;    //隐藏整个标题栏

            tipLabel.Width = Screen.PrimaryScreen.Bounds.Width;
            tipLabel.Location = new Point(8, Screen.PrimaryScreen.Bounds.Height / 6);
            tipLabel.TextAlign = ContentAlignment.MiddleCenter;
        }
    }
}
