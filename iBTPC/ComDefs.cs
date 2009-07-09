using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iBTPC
{
    public class ComDefs
    {


        //课程信息
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

        //学生信息
        public struct studentInfo
        {
            public string stuName;
            //public string grender;
            public int stuID;
            public string stuClass;
            public string stuBtID;
            public string attendence; // 0-未上 1-准时到 2-迟到 3-早退 其他-无效
        }

        //

        // 题目
        public struct Excecise
        {
            public string exerID;
            public string title;
            public string[] choice; // 若非选择题 | 讨论题，则此域为空

            public string ansr;
            public string type; //choice tf blanking shortansr discuss
            public string week;

            public string[] author; //若非讨论题，此域为空
            public string[] ratio;
            public string acuracy;
        }

        public enum exerType
        { 
            choice, tf, blanking, shortansr, discuss

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
