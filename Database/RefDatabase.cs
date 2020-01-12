using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace JsonReference
{
    public abstract class RefDatabase<D>: IEnumerable<IRefTable<D,RefElement<D>>>, IHasJson<JObject> where D : RefDatabase<D>
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

        public RefTable<D, E> AddTable<E>(D database, IRefTableFactory<D,E> tableFactory) where E : RefElement<D>
        {
            RefTable<D,E> table = new RefTable<D,E>(database, tableFactory);
            IHasName name = table;
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
                String tableName = ((IHasName)table).ToPascalCase();
                JObject tableJson  = (JObject) databaseJson[tableName];
                table.LoadReferences(tableJson);
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

        public JObject ToJson()
        {
            JObject databaseJson = new JObject();
            foreach (KeyValuePair<string, IRefTable<D, RefElement<D>>> pair in Tables)
            {
                databaseJson[pair.Key] = pair.Value.ToJson();
            }

            return databaseJson;
        }
    }

    public sealed class RefTable<D,E> : IRefTable<D,E> where E : IRefElement<D> where D : RefDatabase<D>
    {
        public D Database { get; }
        private Dictionary<int, E> Elements;
        private IRefTableFactory<D,E> TableFactory;
        public RefTable(D database, IRefTableFactory<D,E> tableFactory)
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

        public int getNextId(int i)
        {
            if (Elements.ContainsKey(i))
            {
                return getNextId(i+1);
            }
            return i;
        }

        public int getNextId()
        {
            return getNextId(0);
        }

        public List<int> getNextIds(int size)
        {
            List<int> list = new List<int>();
            int curr = 0;
            for (int index = 0; index < size; index++)
            {
                curr = getNextId(curr+1);
                list.Add(curr);
            }

            return list;
        }

        public void LoadJson(JObject tableJson)
        {
            foreach(KeyValuePair<string, JToken> elementPair in tableJson)
            {
                int id = int.Parse(elementPair.Key);
                JObject elementJson = (JObject) elementPair.Value;
                Elements[id] = LoadElement(Database,id, elementJson);
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

        public E LoadElement(D database, int id, JObject elementJson)
        {
            return TableFactory.LoadElement(database, id, elementJson);
        }

        public IEnumerator<E> GetEnumerator()
        {
            return Elements.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Elements.Values.GetEnumerator();
        }

        public JObject ToJson()
        {
            JObject tableJson = new JObject();
            foreach (KeyValuePair<int,E> pair in Elements)
            {
                tableJson[pair.Key.ToString()] = pair.Value.ToJson();
            }

            return tableJson;
        }

        public void LoadReferences(JObject tableJson)
        {
            foreach (E element in Elements.Values)
            {
                int id = element.Id;
                JObject elementJson = (JObject)tableJson[id.ToString()];
                element.LoadReference(elementJson);
            }
        }
    }

    public interface IRefTable<D, out E>: IRefTableFactory<D,E>, IHasJson<JObject>, IEnumerable<E> where E : IRefElement<D> where D: RefDatabase<D>
    {
        public D Database { get; }

        public List<int> getNextIds(int size);

        public int getNextId();

        public void LoadJson(JObject tableJson);
        public void LoadReferences(JObject tableJson);
    }

    public interface IRefTableFactory<D, out E> : IHasName where E : IRefElement<D> where D : RefDatabase<D>
    {
        public E LoadElement(D database, int id, JObject elementJson);
    }

    public abstract class RefElement<D>: IRefElement<D> where D: RefDatabase<D>
    {
        public D Database { get;  }

        public int Id { get; }

        public RefElement(D database, int id, JObject elementJson){
            this.Database = database;
            this.Id = id;
        }

        public abstract void LoadReference(JObject elementJson);

        public JObject ToJson()
        {
            JObject tableJson = new JObject();
            CreateJson(tableJson);
            return tableJson;
        }

        public abstract void CreateJson(JObject tableJson);
    }

    public interface IRefElement<out D>: IHasJson<JObject> where D: RefDatabase<D>
    {
        public int Id { get; }

        public void LoadReference(JObject elementJson);
    }

    public interface IHasName
    {
        public string[] GetName();

        public string ToPascalCase()
        {
            return ToPascalCase("");
        }

        public string ToPascalCase(String seperator)
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

        public string ToCamelCase() {
            return ToCamelCase("");
        }

        public string ToCamelCase(String seperator)
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

    public interface IHasJson<J> where J: JToken
    {
        public J ToJson();
    }

}