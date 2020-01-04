using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;

namespace JsonReference
{
    public abstract class JsonReference<D> where D: JsonReference<D>
    {
        private Dictionary<string, Table<TableElement, D>> Tables;

        public JsonReference()
        {
            Tables = new Dictionary<string, Table<TableElement, D>>();
        }

        public Table<E,D> AddTable<E,F>(TableFactory<E, D> tableFactory) where E : TableElement
        {
            Table<E,D> table = new Table<E,D>((D)this, tableFactory);
            return AddTable<E>(table);
        }

        private Table<E,D> AddTable<E>(Table<E,D> table) where E : TableElement
        {
            Tables.Add(table.ToString(), (Table<TableElement, D>) table);
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

        public static explicit operator D(JsonReference<D> database)
        {
            return (D)database;
        }

    }

    public class Table<E,D> where E : TableElement where D: JsonReference<D>
    {
        private TableFactory<E,D> TableFactory;
        public D Database { get;  }

        private Dictionary<int, E> Data;

        public string Name { 
            get
            {
                return this.GetName();
            } 
        }

        public Table(D database, TableFactory<E,D> tableFactory)
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

        
        public static explicit operator Table<TableElement, D>(Table<E, D> table)
        {
            return (Table<TableElement, D>)table;
        }
        
    }

    public abstract class TableElement
    {
        public int Id { get;  }

        public TableElement()
        {

        }

        public abstract void LoadRefrences(JObject tableJson);

    }


    public interface TableFactory<E,D> : HasName where E : TableElement where D : JsonReference<D>
    {
        public E LoadJson(int i, JObject jToken);
        public void LoadRefrences(E tElement, JObject jToken);

    }

    public interface HasName
    {
        public String GetName();
    }
}



