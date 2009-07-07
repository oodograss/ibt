using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using InTheHand.Net;
using InTheHand.Net.Sockets;
using InTheHand.Net.Bluetooth;
using System.Threading;
using System.Net.Sockets;
using System.Xml;
using System.Windows.Forms;

namespace iBTMobile
{
    public partial class mainForm : Form
    {
        Communication c = new Communication();
        Net net = new Net();

        delegate void deleInvokee(string s);//创建一个代理
        private void pri(string t)//这个就是我们的函数，我们把要对控件进行的操作放在这里
        {
            if (t == "")
                return;
            if (!textBox2.InvokeRequired)//判断是否需要进行唤醒的请求，如果控件与主线程在一个线程内，可以写成if(!InvokeRequired)
            {
                textBox2.Text = t;
            }
            else
            {
                try
                {
                    deleInvokee a1 = new deleInvokee(pri);
                    Invoke(a1, new object[] { t });//执行唤醒操作
                }
                catch (Exception ex)
                { }
            }
        }

        public mainForm()
        {
            InitializeComponent();
            c.threadStart();
            
        }

        private void panel4_GotFocus(object sender, EventArgs e)
        {
        }

        private void menuItem1_Click(object sender, EventArgs e)
        {
            string DestAddr = "2C3D4F04093C";
            string content = "";
            if (checkBox1.Checked)
                content = "A";
            if (checkBox2.Checked)
                content += "B";
            if (checkBox3.Checked)
                content += "C";
            if (checkBox4.Checked)
                content += "D";

            Net.Content newContent = new Net.Content();
            newContent.exerID = comboBox1.Text;
            newContent.title = "";
            newContent.answer = new string[1] {content};
            Net.Packet packet = new Net.Packet();
            packet.type = "117";
            packet.content = net.EncodeContent(newContent);
            c.Send(DestAddr,net.EncodePacket(packet));
        }


        BluetoothDeviceInfo[] bluetoothDeviceInfo = { };

        private void button1_Click(object sender, EventArgs e)
        {
            String s = "正在搜寻中...";
            label1.Text = s;
            Cursor.Current = Cursors.WaitCursor;
            int searchTimes = 0;
            while (bluetoothDeviceInfo.Length == 0 && searchTimes < 4)
            {
                bluetoothDeviceInfo = c.Search();
                searchTimes++;
            }  
            if (bluetoothDeviceInfo == null)
            {
                s = "未找到任何蓝牙设备";
                label1.Text = s;
                return;
            }


            for (int i = 0; i < bluetoothDeviceInfo.Length; i++)
            {
                if (bluetoothDeviceInfo[i].DeviceAddress.ToString() != "2C3D4F04093C")
                {
                    continue;
                }

                c.UpdateRoute(bluetoothDeviceInfo[i], bluetoothDeviceInfo[i].DeviceAddress.ToString());
                BluetoothAddress Addr = (BluetoothAddress.Parse(bluetoothDeviceInfo[i].DeviceAddress.ToString()));
                s = bluetoothDeviceInfo[i].DeviceName + c.Connect(Addr) + " - OK";
                label1.Text = s;
            }
            Cursor.Current = Cursors.Default;
        }

        string sTemp = "";
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (Communication.reciStr != sTemp)
            {
                sTemp = Communication.reciStr;
                Net.Content exer = net.getContent(sTemp);
                comboBox1.Text = exer.exerID;
                textBox2.Text = exer.title;
                textBox6.Text = exer.answer[0];
                textBox7.Text = exer.answer[1];
                textBox8.Text = exer.answer[2];
                textBox9.Text = exer.answer[3];
            }
        }
    }
}