using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;

namespace JsonReference
{
    public abstract class RefDatabase<D> where D: RefDatabase<D>
    {
        private Dictionary<string, RefDatabase<RefElement, D>> Tables;

        public RefDatabase()
        {
            Tables = new Dictionary<string, RefDatabase<RefElement, D>>();
        }

        public RefDatabase<E,D> AddTable<E,F>(JsonReferenceTableFactory<E, D> tableFactory) where E : RefElement
        {
            RefDatabase<E,D> table = new RefDatabase<E,D>((D)this, tableFactory);
            return AddTable<E>(table);
        }

        private RefDatabase<E,D> AddTable<E>(RefDatabase<E,D> table) where E : RefElement
        {
            Tables.Add(table.ToString(), (RefDatabase<RefElement, D>) table);
            return table;
        }

        public void Update(JObject databaseJson)
        {
            foreach(KeyValuePair<string, JToken> keyValuePair in databaseJson)
            {
                String tableName = keyValuePair.Key;
                if(typeof(JObject) != keyValuePair.Value.GetType())
                {
                    continue;
                }
                JObject tableJson = (JObject) keyValuePair.Value;
                Tables[tableName].LoadJson(tableJson);
            }
        }

        public static explicit operator D(RefDatabase<D> database)
        {
            return (D)database;
        }

    }

    public class RefDatabase<E,D> where E : RefElement where D: RefDatabase<D>
    {
        private JsonReferenceTableFactory<E,D> TableFactory;
        public D Database { get;  }

        private Dictionary<int, E> Data;

        public string Name { 
            get
            {
                return this.GetName();
            } 
        }

        public RefDatabase(D database, JsonReferenceTableFactory<E,D> tableFactory)
        {
            Database = database;
            TableFactory = tableFactory;
            Data = new Dictionary<int, E>();
        }

        public string GetName()
        {
            return TableFactory.GetName();
        }

        public void LoadJson(JObject tableJson)
        {
            foreach (KeyValuePair<string, JToken> keyValuePair in tableJson)
            {
                int id = int.Parse(keyValuePair.Key);
                if (typeof(JObject) != keyValuePair.Value.GetType())
                {
                    continue;
                }
                JObject tableJObject = (JObject)tableJson;
                E element = TableFactory.LoadJson(id, tableJObject);
            }
        }

        private void Add(E element)
        {
            Data.Add(element.Id, element);
        }

        public void LoadRefrences(JObject tableJson)
        {
            foreach (KeyValuePair<string, JToken> keyValuePair in tableJson)
            {
                int id = int.Parse(keyValuePair.Key);
                E element = Data[id];
                element.LoadRefrences(tableJson);
            }
        }

        public static int count = 0;

        public static explicit operator RefDatabase<RefElement, D>(RefDatabase<E, D> table)
        {
            Console.WriteLine(count++);
            return (RefDatabase<RefElement, D>)table;
        }
        
    }

    public abstract class RefElement
    {
        public int Id { get;  }

        public RefElement()
        {

        }

        public abstract void LoadRefrences(JObject tableJson);

    }


    public interface JsonReferenceTableFactory<E,D> : HasName where E : RefElement where D : RefDatabase<D>
    {
        public E LoadJson(int i, JObject jToken);
        public void LoadRefrences(E tElement, JObject jToken);

    }

    public interface HasName
    {
        public String GetName();
    }
}



