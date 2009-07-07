using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iBTPC
{
    
    class StudentManager
    {
        private XmlControl xControl;
        private string courseXmlFilePath = "course/";

        private string courseName;
        List<int> stuIdList;

        public StudentManager(string cname)
        {
            courseName = cname;

            xControl = new XmlControl(courseXmlFilePath);

        }

        public StudentManager(string cname,string path)
        {
            courseName = cname;
            courseXmlFilePath = path;
            xControl = new XmlControl(courseXmlFilePath);

        }

        public void getStudentList()
        {
            xControl.getStudentList(courseName); 
        }

        public void addStudent(ComDefs.studentInfo sinfo)
        {
            xControl.newStudent(courseName,sinfo);
        }

    }
}
