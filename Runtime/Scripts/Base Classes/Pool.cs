using System.Collections.Generic;
using UnityEngine;

namespace CrossingLears
{
    public class Pool<T> : MonoBehaviour where T : Component
    {
        public T original;
        private Queue<T> pool;

        public bool ReturnOriginalAtAwake = true;

        public static Pool<T> Instance;

        void Awake()
        {
            Instance = this;

            pool = new Queue<T>();

            if(ReturnOriginalAtAwake)
            {
                Return(original);
                original.close();
            }
        }

        public T Get()
        {
            if (pool.Count > 0)
            {
                T obj = pool.Dequeue();
                obj.gameObject.SetActive(true);
                return obj;
            }
            else
            {
                T obj = Instantiate(original, transform, true);
                obj.gameObject.SetActive(true);
                return obj;
            }
        }

        public static void Return(T obj)
        {
            obj.close();
            Instance.pool.Enqueue(obj);
        }
    }
}