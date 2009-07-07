using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using InTheHand.Net;
using InTheHand.Net.Sockets;
using InTheHand.Net.Bluetooth;
using System.Threading;
using System.Net.Sockets;
using System.Xml;
using System.Windows.Forms;

namespace iBTPC
{
    class Communication
    {
        public struct Frame
        {
            public string MsgType;
            public string DestAddr;
            public string SrcAddr;
            public string SeqNum;
            public string Content;
        }
        const int MAXMESSAGESIZE = 512;
        const int MAXCHANNELNUM = 8;
        const int MAX_PATH = 128;
        const string XMLFILE_PATH = "./BTRouteInfo.xml";

        public static String reciStr = "";

        //Message Type Definition
        const string TYPE_BROADCAST = "101";
        public const string TYPE_UNICAST = "102";
        const string TYPE_ROUTEQUERY = "103";
        const string TYPE_ROUTEREPLY = "104";
        const string TYPE_NODESTATUSQUERY = "105";
        const string TYPE_NODESTATUSREPLY = "106";
        const string TYPE_ROUTEINFOQUERY = "107";
        const string TYPE_ROUTEINFOREPLY = "108";
        const string TYPE_DEVICELIST = "109";
        const string TYPE_CHECKIN = "110";
        const string TYPE_JOINQUERY = "111";
        const string TYPE_JOINREPLY = "112";
        

        private Guid DNservice = BluetoothService.DialupNetworking;
        private BluetoothClient[] bluetoothClient = new BluetoothClient[MAXCHANNELNUM + 1];
        private BluetoothClient[] bluetoothListenClient = new BluetoothClient[MAXCHANNELNUM];
        private BluetoothListener[] bluetoothListener = new BluetoothListener[MAXCHANNELNUM];
        private Guid[] ServiceName = new Guid[MAXCHANNELNUM];
        System.Threading.Thread[] ListenThread = new Thread[MAXCHANNELNUM];
        private static Boolean listening = true;

        private int NodeStatus, QueryNodeStatusResult; // 0 - unconnected node; 1 - leaf node; 2 - cluster header; 3 - root node;
        private bool QueryNodeStatusing, Joining;
        private int JoiningQueryResult;
        private string QueryNodeUnconnectedChannel;
        private int ChannelID;
        private int MessageSeqID;
        private int LocalHop;
        private int TimerCount;
        private string UpstreamDeviceName;
        private BluetoothAddress UpstreamDevice;
        private string LocalAddress, LocalName;
        private BluetoothAddress[] DownstreamDevice = new BluetoothAddress[MAXCHANNELNUM];

        public void threadStart()
        {
            if (!Init())
            {
                return;
            }
            System.Threading.Thread MainListenThread = new System.Threading.Thread(new System.Threading.ThreadStart(receiveLoop));
            MainListenThread.IsBackground = true;
            MainListenThread.Start();
        }

        private Boolean Init()
        {
            if (!BluetoothRadio.IsSupported)
            {
                MessageBox.Show("No bluetooth device is supported!");
                return false;
            }

            LocalAddress = BluetoothRadio.PrimaryRadio.LocalAddress.ToString();
            LocalName = BluetoothRadio.PrimaryRadio.Name;

            NodeStatus = 0;
            MessageSeqID = 1;
            LocalHop = 999;
            ServiceName[0] = new Guid("{A199BEB3-B949-4ef6-89DF-0C60C74A3B99}"); // This channel is reserved for query node status.
            ServiceName[1] = new Guid("{DA4CA591-63E8-4136-B4F8-AEB4C7820EFE}");
            ServiceName[2] = new Guid("{4327076E-2E4A-4dd3-A18E-7653B5DE0EB1}");
            ServiceName[3] = new Guid("{EF06DC58-83ED-4c76-8786-F089DA91463F}");
            ServiceName[4] = new Guid("{897BE2CE-FBE4-4784-B548-3096BD607C48}");
            ServiceName[5] = new Guid("{0AB2FCF0-1596-42db-B14C-452F206743AA}");
            ServiceName[6] = new Guid("{9DB4EAC7-7425-4ded-8A91-CF3AF4F79306}");
            ServiceName[7] = new Guid("{1BABEC20-A0B4-4038-92BF-CB78C2AAA12F}");

            for (int i = 0; i < MAXCHANNELNUM + 1; i++)
            {
                bluetoothClient[i] = new BluetoothClient();
            }

                XmlTextWriter tw = new XmlTextWriter(XMLFILE_PATH, null);
                tw.WriteStartDocument();
                tw.WriteStartElement("Bluetooth_RouteInformation");
                tw.WriteEndElement();
                tw.WriteEndDocument();
                tw.Flush();
                tw.Close();
            BluetoothDeviceInfo deviceInfo = new BluetoothDeviceInfo(BluetoothAddress.Parse(LocalAddress));
            deviceInfo.DeviceName = LocalName;
            UpdateRouteinfo(deviceInfo, LocalAddress);
            return true;
        }

        // Search all the bluetooth device and return the device infomation
        public BluetoothDeviceInfo[] Search()
        {
            BluetoothRadio.PrimaryRadio.Mode = RadioMode.Discoverable;
            BluetoothDeviceInfo[] bluetoothDeviceInfo = { };
            bluetoothDeviceInfo = bluetoothClient[MAXCHANNELNUM].DiscoverDevices(20);
            return bluetoothDeviceInfo;
        }

#region communication

        /*
         * decode a frame received from netstream
         * revert all field of a frame
         * */
        private Frame DecodeMsg(string text)
        {
            Frame result = new Frame();
            try
            {
                result.MsgType = text.Substring(1, 3);
                result.DestAddr = text.Substring(6, 12);
                result.SrcAddr = text.Substring(20, 12);
                result.SeqNum = text.Substring(34, 3);
                result.Content = text.Substring(38, text.Length - 39);
            }
            catch (Exception ex)
            {
                result.MsgType = "";
                result.DestAddr = "";
                result.SrcAddr = "";
                result.SeqNum = "";
                result.Content = "";
            }
            return result;
        }

        /* 
         * Encode a frame to formated string to send
         * */
        private string EncodeMsg(Frame fr)
        {
            string text = "";
            text = "<" + fr.MsgType + "><" + fr.DestAddr + "><" + fr.SrcAddr + "><" + fr.SeqNum + ">" + fr.Content + "#";
            return text;
        }

        // just for test
        public String ConnectTest(BluetoothAddress Addr)
        {
            int i;
            for (i = 0; i < MAXCHANNELNUM; i++)
            {
                try
                {
                    if (!bluetoothClient[0].Connected)
                    {
                        bluetoothClient[0].Connect(new BluetoothEndPoint(Addr, ServiceName[i]));
                        break;
                    }
                }
                catch (Exception ex)
                {
                }
            }
            if (i >= MAXCHANNELNUM)
            {
                //bluetoothClient[0] = new BluetoothClient();
                //bluetoothClient[0].Connect(new BluetoothEndPoint(Addr, ServiceName[i]));                
                return "Connect Fail!";
            }
            else
            {
                return "Connected at Channel: " + (i).ToString();
            }
        }

        public String Connect(BluetoothAddress Addr)
        {            
            int i;
            for (i = 0; i < MAXCHANNELNUM; i++)
            {
                try
                {
                    if (!bluetoothClient[i].Connected)
                    {
                        bluetoothClient[i].Connect(new BluetoothEndPoint(Addr, ServiceName[i]));
                        break;
                    }
                }
                catch (Exception ex)
                {
                }
            }
            if (i >= MAXCHANNELNUM)
            {
                //bluetoothClient[0] = new BluetoothClient();
                //bluetoothClient[0].Connect(new BluetoothEndPoint(Addr, ServiceName[i]));                
                return "Connect Fail!";
            }
            else
            {
                return "Connected at Channel: " + (i).ToString();
            }
        }

        private void BroadcastMessage(Frame fr)
        {
            for (int i = 0; i < MAXCHANNELNUM; i++)
            {
                if (bluetoothClient[i].Connected)
                {
                    BluetoothEndPoint EP = (BluetoothEndPoint)bluetoothClient[i].Client.RemoteEndPoint;
                    fr.DestAddr = EP.Address.ToString();
                    string Msg = EncodeMsg(fr);
                    if (!SendMessage(Msg, bluetoothClient[i]))
                        MessageBox.Show("SendMsg Fail!");
                }
            }
        }

        private void BroadcastMessage2(Frame fr) // Broadcast messages to the downstream devices.
        {
            for (int i = 1; i < MAXCHANNELNUM; i++) //0 is reserved
            {
                if (DownstreamDevice[i] != null)
                {
                    fr.DestAddr = DownstreamDevice[i].ToString();
                    string Msg = EncodeMsg(fr);
                    if (!SendMessage(Msg))
                        MessageBox.Show("SendMsg Fail!");
                }
            }
        }

        private bool SendMessage(string text, BluetoothClient client)
        {
            NetworkStream textStr;
            try
            {
                textStr = client.GetStream();
                textStr.Write(System.Text.Encoding.UTF8.GetBytes(text), 0, text.Length);
                textStr.Close();
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }

        private bool SendMessage(string text)
        {
            string DestAddr = text.Substring(6, 12);
            string NextAddr = "";
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(XMLFILE_PATH);
            }
            catch (XmlException ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
            bool flag = true;

            XmlNodeList DeviceList = doc.GetElementsByTagName("Device");
            foreach (XmlElement node in DeviceList)
            {
                XmlNodeList subList = node.ChildNodes;
                for (int i = 0; i < subList.Count; i++)
                {
                    if (subList[i].Name.CompareTo("Address") == 0 && subList[i].InnerText == DestAddr)
                    {
                        flag = false;
                    }
                    if (!flag && subList[i].Name.CompareTo("NextHopAddr") == 0)
                    {
                        NextAddr = subList[i].InnerText;
                        break;
                    }
                }
                if (!flag)
                    break;
            }
            if (flag)
            {
                NextAddr = UpstreamDevice.ToString();
            }
            int j;
            for (j = 0; j < MAXCHANNELNUM; j++)
            {
                if (bluetoothClient[j].Connected)
                {
                    BluetoothEndPoint EP = (BluetoothEndPoint)bluetoothClient[j].Client.RemoteEndPoint;
                    if (EP.Address.ToString() == NextAddr)
                    {
                        NetworkStream textStr;
                        try
                        {
                            textStr = bluetoothClient[j].GetStream();
                            byte[] bytes = Encoding.UTF8.GetBytes(text);
                            textStr.Write(bytes, 0, bytes.Length);
                            textStr.Close();
                            return true;
                        }
                        catch (Exception ex)
                        {
                            bluetoothClient[j] = new BluetoothClient();
                        }
                        finally
                        {

                        }
                    }
                }
            }
            return false;
        }

        private string RecvMessage(BluetoothListener BTListener, int ChanID, int bufferlen)
        {
            string str = "";
            Frame fr = new Frame();
            try
            {
                int bytesRead;
                Byte[] buffer = new Byte[MAXMESSAGESIZE];
                if (bluetoothListenClient[ChanID] == null)
                    bluetoothListenClient[ChanID] = BTListener.AcceptBluetoothClient();
                NetworkStream textStr = bluetoothListenClient[ChanID].GetStream();
                bytesRead = textStr.Read(buffer, 0, bufferlen);
                textStr.Close();
                string text = System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);
                fr = DecodeMsg(text);
                switch (fr.MsgType)
                {
                    case TYPE_BROADCAST:
                        {
                            //if (fr.SrcAddr != LocalAddress) // Have not deal with the scenario: same Broadcast from two neighbors
                            //{
                            //    BroadcastMessage(fr);
                            //    str = fr.Content;
                            //}
                            //break;
                            BroadcastMessage2(fr);
                            str = fr.Content;
                            break;
                        }
                    case TYPE_UNICAST:
                        {
                            if (LocalAddress == fr.DestAddr)
                            {
                                str = fr.Content;
                            }
                            else
                            {
                                if (!SendMessage(text))
                                {
                                    MessageBox.Show("ForwardMsg Fail!");
                                }
                            }
                            break;
                        }
                    case TYPE_ROUTEQUERY:
                        {
                            if (Int32.Parse(fr.SeqNum) < LocalHop)
                            {
                                LocalHop = Int32.Parse(fr.SeqNum) + 1;
                                UpstreamDevice = BluetoothAddress.Parse(fr.SrcAddr);
                                UpstreamDeviceName = fr.Content;
                                // Timer do not invoke the Timer_Tick func; Maybe the problem is different threads!
                                //if (!this.timer1.Enabled)
                                //{                                
                                //    this.timer1.Enabled = true;
                                //}
                                Thread clusterformThread = new Thread(new ThreadStart(ClusteFormation));
                                clusterformThread.Start();
                            }
                            break;
                        }
                    case TYPE_ROUTEREPLY:
                        {
                            if (NodeStatus != 3)
                                NodeStatus = 2;
                            for (int i = 1; i < MAXCHANNELNUM; i++)
                            {
                                BluetoothEndPoint EP = (BluetoothEndPoint)bluetoothListenClient[i].Client.RemoteEndPoint;
                                if (fr.SrcAddr == EP.Address.ToString())
                                {
                                    DownstreamDevice[i] = new BluetoothAddress(EP.Address.ToByteArray());
                                    BluetoothDeviceInfo deviceInfo = new BluetoothDeviceInfo(EP.Address);
                                    deviceInfo.DeviceName = fr.Content;
                                    UpdateRouteinfo(deviceInfo, fr.SrcAddr);
                                    break;
                                }
                            }
                            break;
                        }
                    case TYPE_NODESTATUSQUERY:
                        {
                            Frame newfr = new Frame();
                            newfr.MsgType = TYPE_NODESTATUSREPLY;
                            newfr.DestAddr = fr.SrcAddr;
                            newfr.SrcAddr = fr.DestAddr;
                            newfr.SeqNum = String.Format("{0:D3}", 0);
                            string unconnectedChannel = "(";
                            for (int k = 1; k < MAXCHANNELNUM; k++)
                            {
                                if (bluetoothListenClient[k] == null) // TO be test                                
                                {
                                    unconnectedChannel = unconnectedChannel + k.ToString();
                                }
                            }
                            unconnectedChannel = unconnectedChannel + ")";
                            newfr.Content = NodeStatus.ToString() + unconnectedChannel;
                            string newtext = EncodeMsg(newfr);
                            try
                            {
                                bluetoothClient[0].Close();
                                bluetoothClient[0].Dispose();
                                bluetoothClient[0] = new BluetoothClient();
                                bluetoothClient[0].Connect(new BluetoothEndPoint(BluetoothAddress.Parse(fr.SrcAddr), ServiceName[0]));
                                SendMessage(newtext, bluetoothClient[0]);
                            }
                            catch (Exception ex)
                            { }
                            bluetoothListenClient[0] = null;
                            break;
                        }
                    case TYPE_NODESTATUSREPLY:
                        {
                            QueryNodeStatusResult = Int32.Parse(fr.Content.Substring(0, 1));
                            QueryNodeUnconnectedChannel = fr.Content.Substring(1, fr.Content.Length - 1);
                            QueryNodeStatusing = false;
                            //str = fr.Content;
                            bluetoothClient[0].Close();
                            bluetoothClient[0].Dispose();
                            break;
                        }
                    case TYPE_ROUTEINFOQUERY:
                        {
                            if (NodeStatus == 1)
                            {
                                Frame newfr = new Frame();
                                newfr.MsgType = TYPE_ROUTEINFOREPLY;
                                newfr.DestAddr = UpstreamDevice.ToString();
                                newfr.SrcAddr = LocalAddress;
                                newfr.SeqNum = String.Format("{0:D3}", 0);
                                newfr.Content = "1(" + LocalName + ")<" + LocalAddress + ">;";
                                string newtext = EncodeMsg(newfr);
                                SendMessage(newtext);
                            }
                            else if (NodeStatus == 2)
                            {
                                Frame newfr = new Frame();
                                newfr.MsgType = TYPE_ROUTEINFOQUERY;
                                newfr.DestAddr = "000000000000";
                                newfr.SrcAddr = LocalAddress;
                                newfr.SeqNum = String.Format("{0:D3}", 0);
                                newfr.Content = "";
                                BroadcastMessage2(newfr);
                            }
                            break;
                        }
                    case TYPE_ROUTEINFOREPLY:
                        {
                            if (fr.Content.Substring(0, 1) == "2")
                            {
                                int pos1 = fr.Content.IndexOf("<");
                                int pos2 = fr.Content.IndexOf(">");
                                string NextHopAddr, DeviceName, DeviceAddr;
                                NextHopAddr = fr.Content.Substring(pos1 + 1, pos2 - pos1 - 1);
                                BluetoothDeviceInfo deviceInfo;
                                pos1 = fr.Content.IndexOf("(", pos2);
                                pos2 = fr.Content.IndexOf(")", pos2);
                                while (pos1 >= 0 && pos2 >= 0)
                                {
                                    DeviceName = fr.Content.Substring(pos1 + 1, pos2 - pos1 - 1);
                                    pos1 = fr.Content.IndexOf("<", pos2);
                                    pos2 = fr.Content.IndexOf(">", pos2);
                                    DeviceAddr = fr.Content.Substring(pos1 + 1, pos2 - pos1 - 1);
                                    deviceInfo = new BluetoothDeviceInfo(BluetoothAddress.Parse(DeviceAddr));
                                    deviceInfo.DeviceName = DeviceName;
                                    UpdateRouteinfo(deviceInfo, NextHopAddr);
                                    pos1 = fr.Content.IndexOf("(", pos2);
                                    pos2 = fr.Content.IndexOf(")", pos2);
                                }
                            }
                            if (NodeStatus != 3)
                            {
                                Frame newfr = new Frame();
                                newfr.MsgType = TYPE_ROUTEINFOREPLY;
                                newfr.DestAddr = UpstreamDevice.ToString();
                                newfr.SrcAddr = LocalAddress;
                                newfr.SeqNum = String.Format("{0:D3}", 0);
                                newfr.Content = "2(" + LocalName + ")<" + LocalAddress + ">" + fr.Content;
                                string newtext = EncodeMsg(newfr);
                                SendMessage(newtext);
                            }
                            else
                            {
                                int pos1 = fr.Content.IndexOf("(");
                                int pos2 = fr.Content.IndexOf(")");
                                string devicename = fr.Content.Substring(pos1 + 1, pos2 - pos1 - 1);
                                //priLable1(devicename + " - OK");
                            }
                            break;
                        }
                    case TYPE_DEVICELIST:
                        {
                            GetDeviceList(fr.Content);
                            BroadcastMessage2(fr);
                            break;
                        }
                    case TYPE_CHECKIN:
                        {
                            if (NodeStatus != 3)
                            {
                                fr.DestAddr = UpstreamDevice.ToString();
                                fr.SrcAddr = LocalAddress;
                                string newtext = EncodeMsg(fr);
                                SendMessage(newtext);
                            }
                            else
                            {

                                string checkinstr = fr.Content + " has checked in!";
                                //ListBoxAdd(checkinstr);
                            }
                            break;
                        }
                    case TYPE_JOINQUERY:
                        {
                            int x;
                            for (x = 1; x < MAXCHANNELNUM; x++)
                            {
                                if (DownstreamDevice[x] == null)
                                    break;
                            }
                            if (x < MAXCHANNELNUM)
                            {
                                DownstreamDevice[x] = new BluetoothAddress(BluetoothAddress.Parse(fr.SrcAddr).ToByteArray());
                                bluetoothClient[x].Close();
                                bluetoothClient[x].Dispose();
                                bluetoothClient[x] = new BluetoothClient();
                                bluetoothClient[x].Connect(new BluetoothEndPoint(BluetoothAddress.Parse(fr.SrcAddr), ServiceName[x]));
                                Frame newfr = new Frame();
                                newfr.MsgType = TYPE_JOINREPLY;
                                newfr.DestAddr = fr.SrcAddr;
                                newfr.SrcAddr = LocalAddress;
                                newfr.SeqNum = String.Format("{0:D3}", 0);
                                newfr.Content = x.ToString();
                                string newtext = EncodeMsg(newfr);
                                try
                                {
                                    bluetoothClient[0].Close();
                                    bluetoothClient[0].Dispose();
                                    bluetoothClient[0] = new BluetoothClient();
                                    bluetoothClient[0].Connect(new BluetoothEndPoint(BluetoothAddress.Parse(fr.SrcAddr), ServiceName[0]));
                                    SendMessage(newtext, bluetoothClient[0]);
                                }
                                catch (Exception ex)
                                { }
                                bluetoothListenClient[0] = null;
                            }
                            else
                            {
                                Frame newfr = new Frame();
                                newfr.MsgType = TYPE_JOINREPLY;
                                newfr.DestAddr = fr.SrcAddr;
                                newfr.SrcAddr = LocalAddress;
                                newfr.SeqNum = String.Format("{0:D3}", 0);
                                newfr.Content = "0" + LocalName;
                                string newtext = EncodeMsg(newfr);
                                try
                                {
                                    bluetoothClient[0].Close();
                                    bluetoothClient[0].Dispose();
                                    bluetoothClient[0] = new BluetoothClient();
                                    bluetoothClient[0].Connect(new BluetoothEndPoint(BluetoothAddress.Parse(fr.SrcAddr), ServiceName[0]));
                                    SendMessage(newtext, bluetoothClient[0]);
                                }
                                catch (Exception ex)
                                { }
                                bluetoothListenClient[0] = null;
                            }
                            break;
                        }
                    case TYPE_JOINREPLY:
                        {
                            try
                            {
                                JoiningQueryResult = Int32.Parse(fr.Content.Substring(0, 1));
                                if (JoiningQueryResult > 0)
                                {
                                    UpstreamDevice = BluetoothAddress.Parse(fr.SrcAddr);
                                    UpstreamDeviceName = fr.Content.Substring(1, fr.Content.Length - 1);
                                }
                                Joining = false;
                            }
                            catch (Exception ex)
                            { }
                            break;
                        }
                }

            }
            catch (Exception ex)
            {
            }
            finally
            {
            }
            return str;
        }

        /*
        private void priLable1(string content)
        {
            if (label1.InvokeRequired)
            {
                try
                {
                    deleInvokee a1 = new deleInvokee(priLable1);
                    Invoke(a1, new object[] { content });//执行唤醒操作
                }
                catch (Exception ex)
                { }
            }
            else
            {
                label1.Text = content;
            }
        }
        */

        private void GetDeviceList(string DeviceListStr)
        {
            int pos1 = DeviceListStr.IndexOf("(");
            int pos2 = DeviceListStr.IndexOf(")");
            DataTable DataTable1 = new DataTable();
            DataTable1.Columns.Add("DeviceName");
            DataTable1.Columns.Add("DeviceAddress");
            string DeviceName, DeviceAddr;
            while (pos1 >= 0 && pos2 >= 0)
            {

                DeviceName = DeviceListStr.Substring(pos1 + 1, pos2 - pos1 - 1);
                pos1 = DeviceListStr.IndexOf("<", pos2);
                pos2 = DeviceListStr.IndexOf(">", pos2);
                DeviceAddr = DeviceListStr.Substring(pos1 + 1, pos2 - pos1 - 1);
                DataRow DataRow1 = DataTable1.NewRow();
                DataRow1["DeviceName"] = DeviceName;
                DataRow1["DeviceAddress"] = DeviceAddr;
                if (DeviceAddr != LocalAddress)
                    DataTable1.Rows.Add(DataRow1);
                pos1 = DeviceListStr.IndexOf("(", pos2);
                pos2 = DeviceListStr.IndexOf(")", pos2);
            }
//             if (comboBox1.InvokeRequired)
//             {
//                 try
//                 {
//                     deleInvokee a1 = new deleInvokee(GetDeviceList);
//                     Invoke(a1, new object[] { DeviceListStr });//执行唤醒操作
//                 }
//                 catch (Exception ex)
//                 { }
//             }
//             else
//             {
//                 comboBox1.DataSource = DataTable1;
//                 comboBox1.DisplayMember = "DeviceName";
//                 comboBox1.ValueMember = "DeviceAddress";
//                 comboBox1.Focus();
//             }
        }

        //For testing
        public void UpdateRoute(BluetoothDeviceInfo DeviceInfo, string NextHopAddress)
        {
            UpdateRouteinfo(DeviceInfo, NextHopAddress);
        }

        /* 
         * Update the route infomation, which is recorded in the Xml document of root "XMLFILE_PATH"
         * */
        private void UpdateRouteinfo(BluetoothDeviceInfo DeviceInfo, string NextHopAddress)
        {
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(XMLFILE_PATH);
            }
            catch (XmlException ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
            bool Existed = false;
            XmlNodeList nodeList = doc.SelectSingleNode("Bluetooth_RouteInformation").ChildNodes;//获取Bluetooth_RouteInformation节点的所有子节点
            foreach (XmlNode node in nodeList)//遍历所有子节点
            {
                XmlNodeList subList = node.ChildNodes;
                for (int i = 0; i < subList.Count && !Existed; i++)
                {
                    if (subList[i].Name.CompareTo("Address") == 0 && subList[i].InnerText == DeviceInfo.DeviceAddress.ToString())
                    {
                        subList[i - 1].InnerText = DeviceInfo.DeviceName;
                        subList[i + 1].InnerText = NextHopAddress;
                        Existed = true;
                        break;
                    }
                }
            }
            if (!Existed)
            {
                XmlNode root = doc.SelectSingleNode("Bluetooth_RouteInformation");
                XmlElement xe1 = doc.CreateElement("Device");

                XmlElement xesub1 = doc.CreateElement("Name");
                xesub1.InnerText = DeviceInfo.DeviceName;
                xe1.AppendChild(xesub1);
                XmlElement xesub2 = doc.CreateElement("Address");
                xesub2.InnerText = DeviceInfo.DeviceAddress.ToString();
                xe1.AppendChild(xesub2);
                XmlElement xesub3 = doc.CreateElement("NextHopAddr");
                xesub3.InnerText = NextHopAddress;
                xe1.AppendChild(xesub3);

                root.AppendChild(xe1);
            }
            doc.Save(XMLFILE_PATH);

        }


        /* 
         * Start the listener for channel with the Id of "ChannelID"
         * Cycle to receive message from the channel
         * */
        private void ListenThreadFunc()
        {
            int chanID = ChannelID;
            bluetoothListener[chanID] = new BluetoothListener(ServiceName[chanID]);
            bluetoothListener[chanID].Start();
            reciStr = RecvMessage(bluetoothListener[chanID], chanID, MAXMESSAGESIZE);
            //if (str != "") pri(str);
            while (listening)
            {
                reciStr = RecvMessage(bluetoothListener[chanID], chanID, MAXMESSAGESIZE);
                //if (str != "") pri(str);
            }
        }

        /*
         * Start a listening thread for each channel
         * */
        public void receiveLoop()
        {
            for (ChannelID = 0; ChannelID < MAXCHANNELNUM; ChannelID++)
            {
                ListenThread[ChannelID] = new System.Threading.Thread(new System.Threading.ThreadStart(ListenThreadFunc));
                ListenThread[ChannelID].IsBackground = true;
                ListenThread[ChannelID].Start();
                Thread.Sleep(200);
            }
        }

        private void Disconnect()
        {
            listening = false;
            for (int i = 0; i < MAXCHANNELNUM; i++)
            {
                bluetoothListener[i].Stop();
            }
        }
       
        private void ClusteFormation()
        {
            Cursor.Current = Cursors.WaitCursor;
            //confirm the connection with Upstreamdevice
            for (int i = 0; i < MAXCHANNELNUM; i++)
            {
                if (bluetoothListenClient[i] == null)
                    continue;
                BluetoothEndPoint EP = (BluetoothEndPoint)bluetoothListenClient[i].Client.RemoteEndPoint;
                if (EP.Address.ToString() == UpstreamDevice.ToString()) // Connect at channel i
                {
                    try
                    {
                        bluetoothClient[i].Close();
                        bluetoothClient[i].Dispose();
                        bluetoothClient[i] = new BluetoothClient();
                        bluetoothClient[i].Connect(new BluetoothEndPoint(UpstreamDevice, ServiceName[i]));
                    }
                    catch (Exception ex)
                    { }
                }
                else
                {
                    bluetoothClient[i].Close();
                    bluetoothClient[i].Dispose();
                    bluetoothClient[i] = new BluetoothClient();
                }
            }

            //update route infos
            BluetoothDeviceInfo deviceInfo = new BluetoothDeviceInfo(UpstreamDevice);
            deviceInfo.DeviceName = UpstreamDeviceName;
            UpdateRouteinfo(deviceInfo, UpstreamDevice.ToString());

            // send back ack to upstreamdevice
            Frame fr = new Frame();
            fr.MsgType = TYPE_ROUTEREPLY;
            fr.DestAddr = UpstreamDevice.ToString();
            fr.SrcAddr = LocalAddress;
            fr.SeqNum = String.Format("{0:D3}", LocalHop);
            fr.Content = LocalName;
            string text = EncodeMsg(fr);
            SendMessage(text);

            //start to form downstream cluster
            NodeStatus = 1; // regular node. If it is CH, this is updated when receieve Queryreply from downstream;
            int maxquerydevicenum = 12;
            bluetoothClient[MAXCHANNELNUM] = new BluetoothClient();
            BluetoothDeviceInfo[] bluetoothDeviceInfo = { };
            bluetoothDeviceInfo = bluetoothClient[MAXCHANNELNUM].DiscoverDevices(maxquerydevicenum);
            for (int i = 0; i < bluetoothDeviceInfo.Length; i++)
            {
                if (bluetoothDeviceInfo[i].DeviceAddress.ToString() == UpstreamDevice.ToString())
                    continue;
                try
                {
                    bluetoothClient[0].Close();
                    bluetoothClient[0].Dispose();
                    bluetoothClient[0] = new BluetoothClient();
                    bluetoothClient[0].Connect(new BluetoothEndPoint(bluetoothDeviceInfo[i].DeviceAddress, ServiceName[0]));
                }
                catch (Exception ex)
                {
                    continue;
                }

                QueryNodeStatusResult = -1;
                QueryNodeStatusing = true;
                QueryNodeUnconnectedChannel = "";

                fr.MsgType = TYPE_NODESTATUSQUERY;
                fr.DestAddr = bluetoothDeviceInfo[i].DeviceAddress.ToString();
                fr.SrcAddr = LocalAddress;
                fr.SeqNum = String.Format("{0:D3}", 0);
                fr.Content = "";
                text = EncodeMsg(fr);
                SendMessage(text, bluetoothClient[0]);

                int Timecount = 0; // timeout parameter
                while (QueryNodeStatusing && Timecount < 10)
                {
                    Thread.Sleep(200);
                    Timecount++;
                }

                if (QueryNodeStatusResult == 0)
                {
                    int j;
                    for (j = 1; j < MAXCHANNELNUM; j++) // channel 0 is reserved.
                    {
                        try
                        {
                            if (!bluetoothClient[j].Connected && QueryNodeUnconnectedChannel.IndexOf(j.ToString()) >= 0) // two nodes have the same unconnected channel.
                            {
                                bluetoothClient[j].Connect(new BluetoothEndPoint(bluetoothDeviceInfo[i].DeviceAddress, ServiceName[j]));
                                break;
                            }
                        }
                        catch (Exception ex)
                        {
                            continue;
                        }
                    }
                    if (j < MAXCHANNELNUM)
                    {
                        fr.MsgType = TYPE_ROUTEQUERY;
                        fr.DestAddr = bluetoothDeviceInfo[i].DeviceAddress.ToString();
                        fr.SrcAddr = LocalAddress;
                        fr.SeqNum = String.Format("{0:D3}", LocalHop);
                        fr.Content = LocalName;
                        text = EncodeMsg(fr);
                        SendMessage(text, bluetoothClient[j]);
                    }
                }
            }
            string testChannel = "";
            for (int y = 0; y < MAXCHANNELNUM; y++)
            {
                if (!bluetoothClient[y].Connected)
                    testChannel = testChannel + y.ToString();
            }
            //pri(testChannel);
            Cursor.Current = Cursors.Default;
        }

        public void Send(string DestAddr, string content)
        {
            Frame fr = new Frame();
            fr.MsgType = TYPE_BROADCAST;
            fr.SrcAddr = LocalAddress;
            fr.DestAddr = DestAddr;
            fr.SeqNum = String.Format("{0:D3}", MessageSeqID);
            fr.Content = content;
            string text = EncodeMsg(fr);
            if (!SendMessage(text))
                MessageBox.Show("SendMsg Fail!");
            else
                MessageSeqID++;
        }
#endregion
    }

}        