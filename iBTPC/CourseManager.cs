using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;
using System.Xml;

namespace iBTPC
{
    class CourseManager
    {

        const string CONFIGFILE_PATH = "iBT.conf";

        private XmlControl xControl;
        private string courseXmlFilePath = "course/";

        private string courseName;
        //private int courseWeeks; //课程周次(课时)
        //private int courseId;

        List<string> courseList; //所有课程列表
        

        public CourseManager()
        {
            xControl = new XmlControl(courseXmlFilePath);
            courseList = new List<string>();

            string[] cXmlFiles = Directory.GetFiles(courseXmlFilePath);
            string cName;
            foreach (string cXmlFile in cXmlFiles) //get courseList
            {
                if(!cXmlFile.Contains(".xml")) continue;

                cName = cXmlFile.Remove(cXmlFile.IndexOf(".xml"));
                cName = cName.Replace(courseXmlFilePath, "");
                courseList.Add(cName);
                cName = "";
            }


            //StreamReader sr = new StreamReader(CONFIGFILE_PATH);
        }

        public CourseManager(string path)
        {
            courseXmlFilePath = path;

            xControl = new XmlControl(courseXmlFilePath);
            courseList = new List<string>();

            string[] cXmlFiles = Directory.GetFiles(courseXmlFilePath);
            string cName;
            foreach (string cXmlFile in cXmlFiles) //get courseList
            {
                if (!cXmlFile.Contains(".xml")) continue;

                cName = cXmlFile.Remove(cXmlFile.IndexOf(".xml"));
                cName = cName.Replace(courseXmlFilePath, "");
                courseList.Add(cName);
                cName = "";
            }
        }

        /*
         新建课程
         */
        public bool creatCourse(ComDefs.courseInfo cinfo)
        {
            courseName = cinfo.name;
            //courseWeeks = cinfo.weeks;

            //检查重复
            foreach (string cName in courseList)
            {
                if (cName == courseName)
                    return false;
            }

            //TODO:验证信息

            xControl.newCourseXml(cinfo);

            
            return true;
        }

        //public void selectCourse(string cName)
        //{ 
        
        
        
        //}

        public ComDefs.courseInfo getCourseInfo(string name)
        { 
            ComDefs.courseInfo cinfo;

            cinfo = xControl.getCourseInfo(name);

            return cinfo;
        }

        public bool deleteCourse(string name)
        {
            foreach (string cName in courseList)
            {
                if (cName == name)
                {
                    courseList.Remove(name);
                    File.Delete(courseXmlFilePath+name+".xml");
                    return true;
                }

            }

            return false;
        }

        public bool modifyCourseInfo(ComDefs.courseInfo cinfo)
        {
            xControl.modifyCourseInfo(cinfo);

            return true;
        }

        public List<string> getCourseList() //获取课程列表
        {
            return courseList;
        }

    }
}
