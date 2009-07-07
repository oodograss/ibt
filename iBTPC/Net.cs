using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iBTPC
{
    public class Net
    {
        // 使用第1、2个ACSII字符作为字符串的分隔符
        static string FLAG = ((char)001).ToString() + ((char)002).ToString();

        // 接收到的消息
        string recvMsg = "";

        // Content Type Definition       
        const string TYPE_NOTEXERCISE = "113";
        const string TYPE_CHOICE = "114";
        const string TYPE_BLANKING = "115";
        const string TYPE_TOPICS = "116";
        const string TYPE_ANSWER = "117";
        const string TYPE_WELCOMEINFO = "118";
        const string TYPE_MESSAGE = "119";
        const string TYPE_ACK = "120";

        public struct Packet
        {
            public string type;
            public string content;
        }

        // 题目分组的内容域
        public struct Content 
        {            
            public string exerID;
            public string title;
            public string[] choice; // 若非选择题 | 讨论题，则此域为空
        }

        //答案分组的内容域
        public struct Answer
        {
            public string exerID;
            public string userID;
            public string ansr;
        }

        // 消息分组的内容域
        public struct Message 
        {
            public string userID;
            public string msg;
        }

        // 定义Communication 类的实例，供方法调用
        Communication commu = new Communication();

#region 成帧
        /*
         * 将题目选项以特殊分隔符FLAG分隔并合并到一个字符串中,仅被EncodeContent 调用
         * */
        public string EncodeChoice(string[] choice)
        {
            string allChoice = "";
            for (int i = 0; i < choice.Length-1;i++ )
            {
                allChoice += choice[i] + FLAG;
            }
            allChoice += choice[choice.Length - 1];
            return allChoice;
        }

        public string EncodePacket(Packet packet)
        {
            string pck = "<" + packet.type +">"+ packet.content;
            return pck;
        }
        
        public Packet DecodePacket(string packet)
        {
            Packet pck = new Packet();
            pck.type = packet.Substring(1, 3);
            pck.content = packet.Substring(5, packet.Length - 5);
            return pck;
        }

        public string EncodeContent(Content content)
        {
            string newContent = content.exerID + FLAG + content.title + FLAG + EncodeChoice(content.choice);
            return newContent;    
        }

        public Content DecodeContent(string content)
        {
            Content newContent = new Content();
            string[] s = new string[1];
            s[0] = FLAG;
            string[] temp = content.Split(s, 3, System.StringSplitOptions.RemoveEmptyEntries);
            newContent.exerID = temp[0];
            newContent.title = temp[1];
            newContent.choice = temp[1].Split(s, System.StringSplitOptions.RemoveEmptyEntries);

            return newContent;
        }
#endregion
        private void SendPacket(Packet packet)
        {
        }

        // 发送欢迎消息
        private void Welcome(string DestAddr, string welInfo)
        {
            Packet packet = new Packet();
            packet.type = TYPE_WELCOMEINFO;
            packet.content = welInfo;
            commu.Send(DestAddr,EncodePacket(packet));
        }

        // 发送确认帧
        public bool SendAck(string DestAddr, string ackInfo)
        {
            Packet packet = new Packet();
            packet.type = TYPE_ACK;
            packet.content = ackInfo;
            commu.Send(DestAddr, EncodePacket(packet));
            return false;
        }

        /*
         * 从接收到的数据包中解析出题目内容
         * */
        public Content getContent(string pck)
        {
            Content content = new Content();
            Packet packet = DecodePacket(pck);
            content = DecodeContent(packet.content);
            return content;
        }


        public void RecvMessage()
        {
            if (recvMsg == Communication.reciStr)
            {
                return;
            }
            recvMsg = Communication.reciStr;
            Packet packet = DecodePacket(recvMsg);
            switch (packet.type)
            {
            case TYPE_CHOICE:
                    Content content = DecodeContent(packet.content);
            	break;
                case TYPE_TOPICS:

                break;
            }
        }

        public string makeExer(string exer)
        {
            char[] c = new char[1] { '\n' };
            Content content = new Content();
            string[] temp = exer.Split(c, 2);
            content.exerID = "001";
            content.title = temp[0];
            content.choice = temp[1].Split(c, StringSplitOptions.RemoveEmptyEntries);
            Packet packet = new Packet();
            packet.type = TYPE_CHOICE;
            packet.content = EncodeContent(content);
            return EncodePacket(packet);
        }
        /*
         * 发送一道题目，exer中包含了题目的编号、题干、候选答案
         * */
    }
}
