using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iBTPC
{
    public class ComDefs
    {
         public struct courseInfo
        {
            public string name;
            public int weeks; //周数
            public string teacherName;
            //public string assistName;
            public string term; //学期
            public string time; //上课时间
            public string classroom;

        }

        public struct studentInfo
        {
            public string stuName;
            //public string grender;
            public int stuID;
            public string stuClass;
            public string attendence;
        }

        // 题目
        public struct Content
        {
            public string exerID;
            public string title;
            public string[] choice; // 若非选择题 | 讨论题，则此域为空
        }

        //答案
        public struct Answer
        {
            public string exerID;
            public string userID;
            public string ansr;
        }

        // 消息
        public struct Message
        {
            public string userID;
            public string msg;
        }
    }
}
