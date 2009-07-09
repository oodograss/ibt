using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using InTheHand.Net;
using InTheHand.Net.Sockets;
using InTheHand.Net.Bluetooth;
using System.Threading;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;


namespace iBTPC
{
    public partial class mainForm : Form
    {

        public string currentCourse;
        private CourseManager course;
        private StudentManager sManager;

        Net net = new Net();
        public static ComDefs.Answer answer = new ComDefs.Answer();
        public static ComDefs.Message message = new ComDefs.Message();
        

        public mainForm()
        {
            InitializeComponent();          

            //初始化
            //currentCourse = "test2";

            //course = new CourseManager();
            //sManager = new StudentManager(currentCourse);

            net.CommuThreadStart();
            System.Threading.Thread listenThread = new System.Threading.Thread(new System.Threading.ThreadStart(ListenThreadFunc));
            listenThread.IsBackground = true;
            listenThread.Start();
        }

#region 公共部分 暂时不修改
        public class ListItem : object
        {
            public string name;
            public int id;
            public override string ToString()
            {
                // TODO:  添加 MyItem.ToString 实现
                return name;
            }
        }

        private void button13_Click(object sender, EventArgs e)
        {
            ListItem item = new ListItem();
            item.id = listBox1.Items.Count;
            item.name = "教师："+richTextBox5.Text;
            listBox1.Items.Add(item);
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex == 2 || tabControl1.SelectedIndex == 3)
            {
                dataGridView1.Visible = false;
            }
            else
            {
                dataGridView1.Visible = true;
            }
        }

        private void button14_Click(object sender, EventArgs e)
        {
            panel3.Visible = false;
        }

        private void button15_Click(object sender, EventArgs e)
        {
            panel3.Visible = true;
        }

        private void button18_Click(object sender, EventArgs e)
        {
            button19.Location = new Point(6, 370);
            button20.Location = new Point(6, 405);
            panel_StuInfo.Visible = true;
            panel_CourseInfo.Visible = false;
            panel_QList.Visible = false;
        }

        private void button19_Click(object sender, EventArgs e)
        {
            button19.Location = new Point(6, 55);
            button20.Location = new Point(6, 405);
            panel_CourseInfo.Location = new Point(8,85);
            panel_StuInfo.Visible = false;
            panel_CourseInfo.Visible = true;
            panel_QList.Visible = false;
        }

        private void button20_Click(object sender, EventArgs e)
        {
            button19.Location = new Point(6, 55);
            button20.Location = new Point(6, 90);
            panel_QList.Location = new Point(8, 120);
            panel_StuInfo.Visible = false;
            panel_CourseInfo.Visible = false;
            panel_QList.Visible = true;
            panel6.Visible = true;
        }

        private void button22_Click(object sender, EventArgs e)
        {
            panel4.Visible = true;
            panel5.Visible = false;
            panel6.Visible = false;
        }

        private void button23_Click(object sender, EventArgs e)
        {
            panel4.Visible = false;
            panel5.Visible = true;
            panel6.Visible = false;
            groupBox12.Text = "出勤记录";
        }

        private void button24_Click(object sender, EventArgs e)
        {
            panel4.Visible = false;
            panel5.Visible = true;
            panel6.Visible = false;
            groupBox12.Text = "课堂表现";
        }

        private void button28_Click(object sender, EventArgs e)
        {
            panel4.Visible = false;
            panel5.Visible = false;
            panel6.Visible = true;
        }
#endregion

/************************************************************************/
/* 华丽的分割线                                                         */
/************************************************************************/

#region 课程管理 by LYY
        private void buttonCourse_Click(object sender, EventArgs e)
        {
            //课程管理

            //新建课程


            ComDefs.courseInfo cinfo;

            cinfo.name = "test2";
            cinfo.weeks = 16;
            cinfo.teacherName = "GuMing";
            cinfo.time = "42";
            cinfo.term = "200901";
            cinfo.classroom = "6a101";

            if (!course.creatCourse(cinfo))
            {
                MessageBox.Show(this, "同名课程已存在，请更改课程名");
            }
            else
            {
                currentCourse = cinfo.name;
            }
            
            //选择课程
            //currentCourse = "name";

            //删除课程
            //DialogResult dialogResult = MessageBox.Show("确定删除吗？与课程相关的学生及题库都将删除", "删除课程", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            //if (dialogResult == DialogResult.OK)
            //{
            //    if(currentCourse.Equals("course_todelete"))
            //        MessageBox.Show(this, "不能删除当前课程！");
            //    else if (!course.deleteCourse("course_todelete"))
            //        MessageBox.Show(this, "所选课程未发现！");
            //} 
            
            //获取课程详细信息
            //courseInfo i = course.getCourseInfo("test");

            //修改课程信息
            //ComDefs.courseInfo cinfo;

            //cinfo.name = "test";
            //cinfo.weeks = 8;
            //cinfo.teacherName = "GuMing";
            //cinfo.time = "42";
            //cinfo.term = "200901";
            //cinfo.classroom = "6a103";
            //course.modifyCourseInfo(cinfo);



            //labelCourse.Text = cinfo.name;

        }

        private void button33_Click(object sender, EventArgs e)
        {
            //学生管理

            //添加学生
            

            ComDefs.studentInfo sinfo = new ComDefs.studentInfo();

            sinfo.stuName = "白易元";
            sinfo.stuID = 2006013219;
            sinfo.stuClass = "软件62";
            sinfo.attendence = "10101";

            sManager.addStudent(sinfo);
        }
#endregion

/************************************************************************/
/* 华丽的分割线                                                         */
/************************************************************************/

#region 课堂互动 by wucs32

        BluetoothDeviceInfo[] bluetoothDeviceInfo = { };
        // 连接网络
        private void button1_Click(object sender, EventArgs e)
        {
            String s = "正在搜寻中...";
            label10.Text = s;
            Cursor.Current = Cursors.WaitCursor;
            s = net.Search();
            label9.Text = s;                
            Cursor.Current = Cursors.Default;
        }

        /*
         * 发布题目
         * */
        private void button5_Click(object sender, EventArgs e)
        {
            string content = richTextBox1.Text.Trim();
            ComDefs.Exercise exer = net.makeExer(content);
            net.SendExercise(exer);
        }

        delegate void deleInvokee();
        private void RecvMessage()
        {
            if (this.InvokeRequired)
            {
                deleInvokee callback = new deleInvokee(RecvMessage);
                this.Invoke(callback, new object[] { });
            }
            else
            {
                ListItem item = new ListItem();
                item.id = listBox1.Items.Count;
                item.name = message.userID + ":" + message.msg;
                listBox1.Items.Add(item);
            }
        }

        public void RecvAnswer()
        {
            if (this.InvokeRequired)
            {
                deleInvokee callback = new deleInvokee(RecvAnswer);
                this.Invoke(callback, new object[] { });
            }
            else
            {
                dataGridView1.Rows.Add();
                dataGridView1.Rows[0].Cells[0].Value = dataGridView1.RowCount.ToString();
                dataGridView1.Rows[0].Cells[2].Value = answer.userID;
                dataGridView1.Rows[0].Cells[3].Value = answer.exerID;
                dataGridView1.Rows[0].Cells[4].Value = answer.ansr;                
                richTextBox2.Text += answer.userID + ":" + answer.exerID + "-" + answer.ansr;
            }
        }

        private void ListenThreadFunc()
        {
            int result = -1;
            while (true)
            {
                result = net.RecvMessage();
                switch (result)
                {
                    case 2:
                        RecvAnswer();
                        break;
                    case 3:
                        RecvMessage();
                        break;
                    default:
                        break;
                }
                Thread.Sleep(200);
            } 
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            string s = net.CheckDevice();
            if (s != "")
            {
                MessageBox.Show(s, "蓝牙设备状态", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
            s = label6.Text;
            if (net.nodeExistNum.ToString() != s.Substring(label6.Text.IndexOf(':')))
            {
                label6.Text = s.Substring(0, s.IndexOf(':')+1) + net.nodeExistNum.ToString();
            }
        }

#endregion

    }
}
