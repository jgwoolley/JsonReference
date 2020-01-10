using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace JsonReference
{
    public abstract class RefDatabase<D>: IEnumerable<IRefTable<D,RefElement<D>>> where D : RefDatabase<D>
    {
        private Dictionary<string, IRefTable<D, RefElement<D>>> Tables;

        public RefDatabase()
        {
            Tables = new Dictionary<string, IRefTable<D,RefElement<D>>>();
        }

        public IRefTable<D,RefElement<D>> this[string key] { get {
                return Tables[key];
            }
        }

        public RefTable<D, E> AddTable<E>(IRefTableFactory<D,E> tableFactory) where E : RefElement<D>
        {
            RefTable<D,E> table = new RefTable<D,E>(this, tableFactory);
            Name name = table;
            Tables.Add(name.ToPascalCase(), (IRefTable<D, RefElement<D>>) table);
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

            foreach (IRefTable<D, RefElement<D>> table in this)
            {
                
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Tables.Values.GetEnumerator();
        }

        public IEnumerator<IRefTable<D,RefElement<D>>> GetEnumerator()
        {
            return Tables.Values.GetEnumerator();
        }

    }

    public sealed class RefTable<D,E> : IRefTable<D,E>, IEnumerable<E> where E : IRefElement<D> where D : RefDatabase<D>
    {
        private RefDatabase<D> Database { get; }
        private Dictionary<int, E> Elements;
        private IRefTableFactory<D,E> TableFactory;
        public RefTable(RefDatabase<D> database, IRefTableFactory<D,E> tableFactory)
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

    public interface IRefTable<out D, out E>: IRefTableFactory<D,E> where E : IRefElement<D> where D: RefDatabase<D>
    {
        public void LoadJson(JObject tableJson);

    }

    public interface IRefTableFactory<out D, out E> : Name where E : IRefElement<D> where D : RefDatabase<D>
    {
        public E LoadElement(int id, JObject elementJson);
    }

    public abstract class RefElement<D>: IRefElement<D> where D: RefDatabase<D>
    {
        public int Id { get; }

        public RefElement(int id, JObject elementJson){
            this.Id = id;
        }

        public abstract void LoadReference(JObject elementJson);

    }

    public interface IRefElement<out D> where D: RefDatabase<D>
    {
        public int Id { get; }

        public void LoadReference(JObject elementJson);
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