using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace JsonReference
{
    public class RefDatabase: IEnumerable<IRefTable<RefElement>>
    {
        private Dictionary<string, IRefTable<RefElement>> Tables;

        public RefDatabase()
        {
            Tables = new Dictionary<string, IRefTable<RefElement>>();
        }

        public IRefTable<RefElement> this[string key] { get {
                return Tables[key];
            }
        }

        public RefTable<E> AddTable<E>(IRefTableFactory<E> tableFactory) where E : RefElement
        {
            RefTable<E> table = new RefTable<E>(this, tableFactory);
            Name name = table;
            Tables.Add(name.ToPascalCase(), (IRefTable<RefElement>) table);
            return table;
        }

        public void LoadJson(JObject databaseJson)
        {
            foreach (KeyValuePair<string, JToken> keyValuePair in databaseJson)
            {
                String tableName = keyValuePair.Key;
                if (typeof(JObject) != keyValuePair.Value.GetType() || !Tables.ContainsKey(tableName))
                {
                    continue;
                }
                JObject tableJson = (JObject) keyValuePair.Value;
                Tables[tableName].LoadJson(tableJson);
            }

            foreach (IRefTable<RefElement> table in this)
            {
                
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Tables.Values.GetEnumerator();
        }

        public IEnumerator<IRefTable<RefElement>> GetEnumerator()
        {
            return Tables.Values.GetEnumerator();
        }

    }

    public sealed class RefTable<E> : IRefTable<E>, IEnumerable<E> where E : RefElement
    {
        private RefDatabase Database { get; }
        private Dictionary<int, E> Elements;
        private IRefTableFactory<E> TableFactory;
        public RefTable(RefDatabase database, IRefTableFactory<E> tableFactory)
        {
            this.Database = database;
            this.TableFactory = tableFactory;
            Elements = new Dictionary<int, E>();
        }

        public E this[int id]
        {
            get
            {
                return Elements[id];
            }
        }
        
        public void LoadJson(JObject tableJson)
        {
            foreach(KeyValuePair<string, JToken> elementPair in tableJson)
            {
                int id = int.Parse(elementPair.Key);
                JObject elementJson = (JObject) elementPair.Value;
                Elements[id] = LoadElement(id,elementJson);
            }
        }

        public void LoadReference(JObject tableJson)
        {
            foreach (E element in this)
            {
                int id = element.Id;
                JObject elementJson = (JObject) tableJson[id.ToString()];
                element.LoadReference(elementJson);
            }
        }

        public string[] GetName()
        {
            return TableFactory.GetName();
        }

        public E LoadElement(int id, JObject elementJson)
        {
            return TableFactory.LoadElement(id, elementJson);
        }

        public IEnumerator<E> GetEnumerator()
        {
            return Elements.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Elements.Values.GetEnumerator();
        }

    }

    public interface IRefTable<out E>: IRefTableFactory<E> where E : RefElement {
        public void LoadJson(JObject tableJson);

    }

    public interface IRefTableFactory<out E> : Name where E : RefElement
    {
        public E LoadElement(int id, JObject elementJson);
    }

    public abstract class RefElement
    {
        public int Id { get; }

        public RefElement(int id, JObject elementJson){
            this.Id = id;
        }

        public abstract void LoadReference(JObject elementJson);

    }

    public interface Name
    {
        public string[] GetName();

        public virtual string ToPascalCase()
        {
            return ToPascalCase("");
        }

        public virtual string ToPascalCase(String seperator)
        {
            string[] name = GetName();
            if (name.Length == 0)
            {
                return "";
            }
            String curr = name[0];
            if (curr.Length >= 2)
            {
                curr = char.ToUpper(curr[0]) + curr.Substring(1, curr.Length - 1).ToLower();
            } else
            {
                curr = char.ToUpper(curr[0]).ToString();
            }

            StringBuilder buff = new StringBuilder();
            buff.Append(curr);
            for (int i = 1; i < name.Length; i++)
            {
                curr = name[i];
                curr = char.ToUpper(curr[0]) + curr.Substring(1, curr.Length - 1).ToLower();
                buff.Append(seperator+curr);
            }

            return buff.ToString();
        }

        public virtual string ToCamelCase() {
            return ToCamelCase("");
        }

        public virtual string ToCamelCase(String seperator)
        {
            string[] name = GetName();
            if (name.Length == 0)
            {
                return "";
            }
            String curr = name[0];
            if (curr.Length >= 2)
            {
                curr = char.ToLower(curr[0]) + curr.Substring(1, curr.Length - 1).ToLower();
            }
            else
            {
                curr = char.ToUpper(curr[0]).ToString();
            }

            StringBuilder buff = new StringBuilder();
            buff.Append(curr);
            for (int i = 1; i < name.Length; i++)
            {
                curr = name[i];
                curr = char.ToUpper(curr[0]) + curr.Substring(1, curr.Length - 1).ToLower();
                buff.Append(seperator + curr);
            }

            return buff.ToString();
        }
    }

}