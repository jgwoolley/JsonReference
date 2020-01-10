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
            JObject studentJson = new JObject();
            for (int i = 0; i < 10; i++)
            {
                JObject studentObject = new JObject();
                studentObject["school"] = i % 2;
                studentJson[i.ToString()] = studentObject;

            }

            JObject schoolJson = new JObject();
            for (int i = 0; i < 2; i++)
            {
                schoolJson[i.ToString()] = new JObject();
            }

            RefDatabase database = new RefDatabase();
            RefTable<Student> studentTable = database.AddTable<Student>(new StudentFactory());                                         
            Name studentTableName = studentTable;
            RefTable<School> schoolTable = database.AddTable<School>(new SchoolFactory());
            Name schoolTableName = schoolTable;

            JObject dataJson = new JObject();
            dataJson[studentTableName.ToPascalCase()] = studentJson;
            dataJson[schoolTableName.ToPascalCase()] = schoolJson;

            Console.WriteLine(dataJson);
            database.LoadJson(dataJson);

            Console.WriteLine("Writing Students:");
            foreach (Student student in studentTable)
            {
                Console.WriteLine("\t"+student.Id);
            }
            Console.WriteLine("Finished Writing Students.");

        }
    }

    public class StudentFactory : IRefTableFactory<Student>
    {
        public string[] GetName()
        {
            return new string[] { "Student", "Table" };
        }

        public Student LoadElement(int id, JObject elementJson)
        {
            return new Student(id, elementJson);
        }
    }

    public class Student : RefElement
    {
        public Student(int id, JObject elementJson) : base(id, elementJson)
        {
            
        }

        public override void LoadReference(JObject elementJson)
        {
            
        }

    }

    public class SchoolFactory : IRefTableFactory<School> {
        public string[] GetName()
        {
            return new string[] { "School", "Table" };
        }

        public School LoadElement(int id, JObject elementJson)
        {
            return new School(id, elementJson);
        }
    }

    public class School: RefElement
    {
        public School(int id, JObject elementJson) : base(id, elementJson)
        {

        }

        public override void LoadReference(JObject elementJson)
        {

        }
    }
}
