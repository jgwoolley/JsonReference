using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;

namespace JsonReference
{
    public abstract class JsonReferenceTable<D> where D: JsonReferenceTable<D>
    {
        private Dictionary<string, JsonReferenceTable<JsonReferenceTableElement, D>> Tables;

        public JsonReferenceTable()
        {
            Tables = new Dictionary<string, JsonReferenceTable<JsonReferenceTableElement, D>>();
        }

        public JsonReferenceTable<E,D> AddTable<E,F>(JsonReferenceTableFactory<E, D> tableFactory) where E : JsonReferenceTableElement
        {
            JsonReferenceTable<E,D> table = new JsonReferenceTable<E,D>((D)this, tableFactory);
            return AddTable<E>(table);
        }

        private JsonReferenceTable<E,D> AddTable<E>(JsonReferenceTable<E,D> table) where E : JsonReferenceTableElement
        {
            Tables.Add(table.ToString(), (JsonReferenceTable<JsonReferenceTableElement, D>) table);
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

        public static explicit operator D(JsonReferenceTable<D> database)
        {
            return (D)database;
        }

    }

    public class JsonReferenceTable<E,D> where E : JsonReferenceTableElement where D: JsonReferenceTable<D>
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

        public JsonReferenceTable(D database, JsonReferenceTableFactory<E,D> tableFactory)
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

        
        public static explicit operator JsonReferenceTable<JsonReferenceTableElement, D>(JsonReferenceTable<E, D> table)
        {
            return (JsonReferenceTable<JsonReferenceTableElement, D>)table;
        }
        
    }

    public abstract class JsonReferenceTableElement
    {
        public int Id { get;  }

        public JsonReferenceTableElement()
        {

        }

        public abstract void LoadRefrences(JObject tableJson);

    }


    public interface JsonReferenceTableFactory<E,D> : HasName where E : JsonReferenceTableElement where D : JsonReferenceTable<D>
    {
        public E LoadJson(int i, JObject jToken);
        public void LoadRefrences(E tElement, JObject jToken);

    }

    public interface HasName
    {
        public String GetName();
    }
}



