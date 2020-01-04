using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using JsonReference;
using Newtonsoft.Json.Linq;

namespace Runner
{
    public class Runner{

        static void Main(string[] args)
        {
            JObject dataJson = new JObject();
            dataJson["student"] = new JObject();

            SchoolDatabase data = new SchoolDatabase();
            JsonReferenceTable<Student, SchoolDatabase> studentTable = data.AddTable<Student,SchoolDatabase>(new StudentTableFactory());
            data.Update(dataJson);
        }
    }

    public class SchoolDatabase: JsonReferenceTable<SchoolDatabase>
    {
        public SchoolDatabase() : base()
        {

        }
    }

    public class Student : JsonReferenceTableElement
    {
        public override void LoadRefrences(JObject tableJson)
        {
            throw new NotImplementedException();
        }
    }

    public class StudentTableFactory : JsonReferenceTableFactory<Student, SchoolDatabase>
    {
        public StudentTableFactory()
        {

        }

        public string GetName()
        {
            return "Student";
        }

        public Student LoadJson(int i, JObject jToken)
        {
            throw new NotImplementedException();
        }

        public void LoadRefrences(Student tElement, JObject jToken)
        {
            throw new NotImplementedException();
        }
    }
}
