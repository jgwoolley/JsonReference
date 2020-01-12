using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using JsonReference;
using Newtonsoft.Json.Linq;
using System.CodeDom;

namespace TestRefDatabase
{
    public class Runner{

        static void Main(string[] args)
        {
            Console.WriteLine("studentJson create:");
            JObject studentJson = new JObject();
            for (int i = 0; i < 10; i++)
            {
                JObject studentObject = new JObject();
                studentObject["school"] = i % 2;
                studentJson[i.ToString()] = studentObject;

            }

            for (int i = 13; i < 16; i++)
            {
                JObject studentObject = new JObject();
                studentObject["school"] = i % 2;
                studentJson[i.ToString()] = studentObject;
            }

                Console.WriteLine("schoolJson create:");
            JObject schoolJson = new JObject();
            for (int i = 0; i < 2; i++)
            {
                schoolJson[i.ToString()] = new JObject();
            }

            Console.WriteLine("StudentDatabase create:");
            StudentDatabase database = new StudentDatabase();
            IHasName studentTableName = database.StudentTable;
            IHasName schoolTableName = database.SchoolTable;

            JObject dataJson = new JObject();
            dataJson[studentTableName.ToPascalCase()] = studentJson;
            dataJson[schoolTableName.ToPascalCase()] = schoolJson;

            Console.WriteLine("Load Json");
            database.LoadJson(dataJson);

            Console.WriteLine("Writing Students/Schools:");
            Console.WriteLine(database.ToJson());

            database.StudentTable.getNextIds(4).ForEach(Console.WriteLine);

        }
    }

    public class StudentDatabase: RefDatabase<StudentDatabase>
    {
        public RefTable<StudentDatabase, Student> StudentTable { get; }
        public RefTable<StudentDatabase, School> SchoolTable { get; }

        public StudentDatabase(): base()
        {
            StudentTable = this.AddTable(this, new StudentFactory());
            SchoolTable = this.AddTable(this, new SchoolFactory());
        }
    }

    public class StudentFactory : IRefTableFactory<StudentDatabase,Student>
    {
        public string[] GetName()
        {
            return new string[] { "Student", "Table" };
        }

        public Student LoadElement(StudentDatabase database, int id, JObject elementJson)
        {
            return new Student(database, id, elementJson);
        }
    }

    public class Student : RefElement<StudentDatabase>
    {
        public School School { get; set; }

        public Student(StudentDatabase database, int id, JObject elementJson) : base(database, id, elementJson)
        {
            
        }

        public override void LoadReference(JObject elementJson)
        {
            this.School = this.Database.SchoolTable[(int)elementJson["school"]];
            if (this.School == null) throw new Exception();
        }

        public override void CreateJson(JObject studentJson)
        {
            int schoolId = this.School.Id;
            studentJson["school"] = schoolId;
        }

    }

    public class SchoolFactory : IRefTableFactory<StudentDatabase,School> {
        public string[] GetName()
        {
            return new string[] { "School", "Table" };
        }

        public School LoadElement(StudentDatabase database, int id, JObject elementJson)
        {
            return new School(database, id, elementJson);
        }
    }

    public class School: RefElement<StudentDatabase>
    {
        public School(StudentDatabase database, int id, JObject elementJson) : base(database,id, elementJson)
        {
            
        }

        public override void LoadReference(JObject elementJson)
        {

        }


        public override void CreateJson(JObject tableJson)
        {

        }
    }
}
