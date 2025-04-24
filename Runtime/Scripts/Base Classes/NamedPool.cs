using System.Collections.Generic;
using UnityEngine;

namespace CrossingLears
{
    public class NamedPool<T> : MonoBehaviour where T : Component
    {
        public T[] originals;
        private Dictionary<string, Queue<T>> pools = new Dictionary<string, Queue<T>>();

        public static NamedPool<T> Instance;

        void Awake()
        {
            Instance = this;

            foreach (var item in originals)
            {
                string key = item.name;
                if (!pools.ContainsKey(key))
                    pools[key] = new Queue<T>();

                Return(key, item);
                item.close();
            }
        }

        public T Get(string key)
        {
            if (pools.TryGetValue(key, out var queue) && queue.Count > 0)
            {
                T obj = queue.Dequeue();
                obj.gameObject.SetActive(true);
                return obj;
            }
            else if (TryGetOriginal(key, out var original))
            {
                T obj = Instantiate(original, transform, true);
                obj.gameObject.SetActive(true);
                return obj;
            }
            else
            {
                Debug.LogError($"No original found for key '{key}'");
                return null;
            }
        }

        public static void Return(string key, T obj)
        {
            obj.close();
            if (!Instance.pools.TryGetValue(key, out var queue))
            {
                queue = new Queue<T>();
                Instance.pools[key] = queue;
            }
            queue.Enqueue(obj);
        }

        private bool TryGetOriginal(string key, out T original)
        {
            foreach (var item in originals)
            {
                if (item.name == key)
                {
                    original = item;
                    return true;
                }
            }
            original = null;
            return false;
        }
    }
}
