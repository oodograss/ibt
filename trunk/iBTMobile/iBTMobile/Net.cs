using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iBTMobile
{
    public class Net
    {
        // 使用第1、2个ACSII字符作为字符串的分隔符
        static string FLAG = ((char)001).ToString() + ((char)002).ToString();

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

        public struct Content 
        {
            public string exerID;
            public string title;
            public string[] answer;
        }
#region 成帧
        /*
         * 将题目选项以特殊分隔符FLAG分隔并合并到一个字符串中
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
            string newContent = content.exerID + FLAG + content.title + FLAG + EncodeChoice(content.answer);
            return newContent;    
        }

        public Content DecodeContent(string content)
        {
            Content newContent = new Content();
            char[] c = FLAG.ToCharArray();
            string[] temp = content.Split(c);
            newContent.exerID = temp[0];
            newContent.title = temp[2];
            newContent.answer = new string[(temp.Length-3) / 2];
            int i=3;
            while (i < temp.Length)
            {
                if (temp[i] != "")
                {
                    newContent.answer[(i-3)/2] = temp[i];                    
                }
                i++;
            }
          
            //newContent.answer = temp[2].Split(c);

            return newContent;
        }
#endregion

        public Content getContent(string pck)
        {
            Content content = new Content();
            Packet packet = DecodePacket(pck);
            content = DecodeContent(packet.content);
            return content;
        }
        
        private void SendPacket(Packet packet)
        {

        }

        public bool SendAck()
        {

            return false;
        }
    }
}
