using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iBTPC
{
    partial class ComDefs
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

        public ComDefs()
        { 
        
        }
    }
}
