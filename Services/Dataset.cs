using shared.Entities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace srvkestrel
{
    public class Dataset<T> where T : class, IEntity
    {
        //TWO storages (dict and array)
        ConcurrentDictionary<uint, T> dict;
        T[] array;

        readonly int ARRAYSIZE;
        public Dataset(IEnumerable<List<T>> batch) 
        {
            ARRAYSIZE = batch.Sum(list => list.Count);
            Console.WriteLine($"data size {typeof(T)} =  {ARRAYSIZE}");
            ARRAYSIZE += 10000;
            array = new T[ARRAYSIZE];
            dict = new ConcurrentDictionary<uint, T>(3, 1000);//initial capacity 1000
            foreach (var x in batch.SelectMany(list => list))
                if (x.id < ARRAYSIZE)
                    array[x.id] = x;
                else
                    dict[x.id] = x;
            foreach (var list in batch) list.Clear();//free some mem
        }


        public T this[uint index]
        {
            get
            {
                if (index < ARRAYSIZE)
                    return array[index];
                else
                    if (dict.TryGetValue(index, out T res)) return res; else return null;
            }
            set
            {
                if (index < ARRAYSIZE)
                    array[index] = value;
                else
                    dict[index] = value;
            }
        }
        //public int Count => dict.Count+array???;

        public IEnumerable<T> Values => array.Where(x=>x!=null).Concat(dict.Values);
    }
}
