using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using InTheHand.Net;
using InTheHand.Net.Sockets;
using InTheHand.Net.Bluetooth;
using System.Threading;
using System.Net.Sockets;
using System.Xml;
using System.Windows.Forms;


namespace iBTPC
{
    public partial class mainForm : Form
    {

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
        Communication commu = new Communication();
        Net net = new Net();

        public mainForm()
        {
            InitializeComponent();
            commu.threadStart();
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
                groupBox8.Visible = true;
            }
            else
            {
                dataGridView1.Visible = true;
                groupBox8.Visible = false;
            }
        }

        private void button14_Click(object sender, EventArgs e)
        {
            panel2.Visible = true;
            panel3.Visible = false;
        }

        private void button15_Click(object sender, EventArgs e)
        {
            panel2.Visible = false;
            panel3.Visible = true;
        }

        private void button18_Click(object sender, EventArgs e)
        {
            button19.Location = new Point(6, 370);
            button20.Location = new Point(6, 405);
            panel_StuInfo.Visible = true;
            panel_CourseInfo.Visible = false;
            panel_QList.Visible = false;
            panel4.Visible = true;
            panel6.Visible = false;
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
            panel4.Visible = false;
            panel6.Visible = true;
        }

        private void button22_Click(object sender, EventArgs e)
        {
            panel4.Visible = true;
            panel5.Visible = false;
        }

        private void button23_Click(object sender, EventArgs e)
        {
            panel4.Visible = false;
            panel5.Visible = true;
            groupBox12.Text = "出勤记录";
        }

        private void button24_Click(object sender, EventArgs e)
        {
            panel4.Visible = false;
            panel5.Visible = true;
            groupBox12.Text = "课堂表现";
        }
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

    }
}
