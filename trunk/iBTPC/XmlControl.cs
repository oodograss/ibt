using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Xml;

namespace iBTPC
{
    partial class XmlControl
    {

        private string xmlFilePath = "course/";

        public XmlControl(string path)
        {
            xmlFilePath = path;

        }

        #region 课程管理
        public void newCourseXml(ComDefs.courseInfo cinfo)
        {
            string name = cinfo.name;
            //int weeks, string teacherName, string assistName, string term, string period, string classroom

            xmlFilePath = xmlFilePath.Insert(xmlFilePath.Length, name);
            xmlFilePath = xmlFilePath.Insert(xmlFilePath.Length, ".xml");

            XmlTextWriter tw = new XmlTextWriter(xmlFilePath, null);
            tw.Formatting = Formatting.Indented;  //缩进格式
            tw.Indentation = 4;

            //XmlDocument xDoc = new XmlDocument();
            tw.WriteStartDocument();

            tw.WriteStartElement("Course"); //root

            #region courseInfo

            tw.WriteStartElement("CourseInfo");

            tw.WriteStartAttribute("CourseName");
            tw.WriteString(name);
            tw.WriteEndAttribute();

            tw.WriteStartAttribute("Teacher");
            tw.WriteString(cinfo.teacherName);
            tw.WriteEndAttribute();

            //tw.WriteStartAttribute("Assist");
            //tw.WriteString(assistName);
            //tw.WriteEndAttribute();

            tw.WriteStartAttribute("Term");
            tw.WriteString(cinfo.term);
            tw.WriteEndAttribute();

            tw.WriteStartAttribute("Weeks");
            tw.WriteValue(cinfo.weeks);
            tw.WriteEndAttribute();

            tw.WriteStartAttribute("Classroom");
            tw.WriteString(cinfo.classroom);
            tw.WriteEndAttribute();

            tw.WriteStartAttribute("Time");
            tw.WriteString(cinfo.time);
            tw.WriteEndAttribute();

            tw.WriteEndElement();
            #endregion

            #region student info
            tw.WriteStartElement("StudentInfo");

            tw.WriteStartAttribute("StuCount");
            tw.WriteValue(0);
            //tw.WriteString("0");
            tw.WriteEndAttribute();

            
            tw.WriteEndElement();
            #endregion

            #region exercise info
            tw.WriteStartElement("ExerciseInfo");

            tw.WriteStartAttribute("ExeCount");
            tw.WriteValue(0);
            tw.WriteEndAttribute();

            tw.WriteStartElement("ExeChoice");

            tw.WriteEndElement();

            tw.WriteStartElement("ExeBlanking");

            tw.WriteEndElement();

            tw.WriteStartElement("ExeOpen");

            tw.WriteEndElement();

            tw.WriteEndElement();
            #endregion

            tw.WriteEndElement();

            tw.WriteEndDocument();
            tw.Flush();
            tw.Close();

            
        }

        public ComDefs.courseInfo getCourseInfo(string cname)
        {
            ComDefs.courseInfo cinfo = new ComDefs.courseInfo();

            XmlNodeList list = (XmlNodeList)getNode(cname, "CourseInfo");
            
            foreach (XmlElement e in list)
            {
                cinfo.name = e.Attributes["CourseName"].Value;
                cinfo.teacherName = e.Attributes["Teacher"].Value;
                cinfo.term = e.Attributes["Term"].Value;
                cinfo.weeks = int.Parse(e.Attributes["Weeks"].Value);
                cinfo.time = e.Attributes["Time"].Value;
                cinfo.classroom = e.Attributes["Classroom"].Value;
            } 
            return cinfo;
        }

        public void modifyCourseInfo(ComDefs.courseInfo cinfo)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(xmlFilePath + cinfo.name + ".xml");
            XmlNodeList list = doc.GetElementsByTagName("CourseInfo");

            foreach (XmlElement e in list)
            {
                e.SetAttribute("Teacher",cinfo.teacherName);
                e.SetAttribute("Term", cinfo.term);
                e.SetAttribute("Weeks", cinfo.weeks.ToString());
                e.SetAttribute("Time",cinfo.time);
                e.SetAttribute("Classroom",cinfo.classroom);
            }

            doc.Save(xmlFilePath + cinfo.name + ".xml");

        }
        #endregion

        #region 学生管理

        public void newStudent(string cName, ComDefs.studentInfo sinfo)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(xmlFilePath + cName + ".xml");
            XmlNodeList list = doc.GetElementsByTagName("StudentInfo");

            foreach (XmlNode n in list)
            {
                XmlElement stuEle = doc.CreateElement("Student");
                stuEle.SetAttribute("StuName",sinfo.stuName);
                stuEle.SetAttribute("StuID",sinfo.stuID.ToString());
                stuEle.SetAttribute("StuClass",sinfo.stuClass);
                stuEle.SetAttribute("StuAttendence",sinfo.attendence);

                n.AppendChild(stuEle);
                XmlElement e = (XmlElement)n;
                int c = int.Parse(e.Attributes["StuCount"].Value);
                c++;
                e.SetAttribute("StuCount", c.ToString());
            }

            doc.Save(xmlFilePath + cName + ".xml");
            //tw.WriteStartElement("Student"); //student node

            //tw.WriteStartAttribute("StuName");
            //tw.WriteString("");
            //tw.WriteEndAttribute();

            //tw.WriteStartAttribute("StuID");
            //tw.WriteValue(-1);
            //tw.WriteEndAttribute();

            //tw.WriteStartAttribute("StuClass");
            //tw.WriteString(""); ;
            //tw.WriteEndAttribute();

            //tw.WriteStartAttribute("StuAttendence");
            //tw.WriteString(""); ;
            //tw.WriteEndAttribute();

            //tw.WriteStartElement("StuExercise");//student exercise node

            //tw.WriteStartAttribute("StuExerciseID");
            //tw.WriteValue(-1);
            //tw.WriteEndAttribute();

            //tw.WriteStartAttribute("StuExerciseWeek");
            //tw.WriteValue(-1);
            //tw.WriteEndAttribute();

            //tw.WriteString("stuAnswer");

            //tw.WriteEndElement();//student exercise node end

            //tw.WriteEndElement();//student node end

            //    //初始化XML文档操作类

            ////加载XML文件
            //myDoc.Load(newXmlFilePath);

            ////添加元素--UserCode
            //XmlElement ele = myDoc.CreateElement("UserCode");
            //XmlText text = myDoc.CreateTextNode(UserCode);

            ////添加元素--UserName
            //XmlElement ele1 = myDoc.CreateElement("UserName");
            //XmlText text1 = myDoc.CreateTextNode(UserName);

            ////添加元素--UserPwd
            //XmlElement ele2 = myDoc.CreateElement("UserPwd");
            //XmlText text2 = myDoc.CreateTextNode(UserPassword);

            ////添加节点 User要对应我们xml文件中的节点名字
            //XmlNode newElem = xDoc.CreateNode("element", "User", "");

            ////在节点中添加元素
            //newElem.AppendChild(ele);
            //newElem.LastChild.AppendChild(text);
            //newElem.AppendChild(ele1);
            //newElem.LastChild.AppendChild(text1);
            //newElem.AppendChild(ele2);
            //newElem.LastChild.AppendChild(text2);

            ////将节点添加到文档中
            //XmlElement root = myDoc.DocumentElement;
            //root.AppendChild(newElem);

            ////保存
            //myDoc.Save(newXmlFilePath);

        
        }

        public void getStudentList(string cName)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(xmlFilePath + cName + ".xml");  
        
        }

        

        #endregion

        public object getNode(string xmlName ,string nodeName)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(xmlFilePath + xmlName + ".xml");
            XmlNodeList list = doc.GetElementsByTagName("StudentInfo");

            return list;

        }

        public bool addNode(string xmlName)
        {


            return true;
        }

        public void deleteNode()
        {

        }
    }
}
