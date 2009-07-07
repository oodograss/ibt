using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;


namespace iBTPC
{
    public partial class mainForm : Form
    {

        public string currentCourse;
        private CourseManager course;
        private StudentManager sManager;


        public mainForm()
        {
            InitializeComponent();

            //初始化
            currentCourse = "test2";

            course = new CourseManager();
            sManager = new StudentManager(currentCourse);

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
            if (tabControl1.SelectedIndex == 2)
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




        Communication commu = new Communication();
        Net net = new Net();

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
            int searchTimes = 0;
            while (bluetoothDeviceInfo.Length == 0 && searchTimes < 4)
            {
                bluetoothDeviceInfo = commu.Search();
                searchTimes++;
            }
            if (bluetoothDeviceInfo.Length == 0)
            {
                s = "未找到任何蓝牙设备";
                label9.Text = s;
                return;
            }

            for (int i = 0; i < bluetoothDeviceInfo.Length; i++)
            {
                if (bluetoothDeviceInfo[i].DeviceAddress.ToString() != "0017E5448609")
                {
                    continue;
                }
                commu.UpdateRoute(bluetoothDeviceInfo[i], bluetoothDeviceInfo[i].DeviceAddress.ToString());
                BluetoothAddress Addr = (BluetoothAddress.Parse(bluetoothDeviceInfo[i].DeviceAddress.ToString()));
                s = bluetoothDeviceInfo[i].DeviceName + commu.ConnectTest(Addr) + " - OK";
                label10.Text = label9.Text;
                label9.Text = s;
            }
            Cursor.Current = Cursors.Default;
        }

        /*
         * 发布题目
         * */
        private void button5_Click(object sender, EventArgs e)
        {
            string DestAddr = bluetoothDeviceInfo[0].DeviceAddress.ToString();
            string content = richTextBox1.Text.Trim();

            commu.Send(DestAddr, net.makeExer(content));
        }

        string sTemp = "";
        private void timer1_Tick(object sender, EventArgs e)
        {
            bool flag = true;
            if (!BluetoothRadio.IsSupported && flag)
            {
                MessageBox.Show("No bluetooth device is supported! Please restart the program and make sure your bluetooth device is in service.");
                flag = false;
            }
            else if (!flag)
            {
                flag = true;
            }
            if (Communication.reciStr != sTemp)
            {
                sTemp = Communication.reciStr;
                Net.Content exer = net.getContent(sTemp);
                richTextBox1.Text += "\r" + exer.title;
            }
        }
#endregion


    }
}
