using System.Collections.Generic;
using CrossingLears;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public abstract class ObjectDictionary<T> : MonoBehaviour
{
    protected abstract string NameGetter(T inp);

    [SerializeField] protected List<T> Lister = new List<T>();
    public static Dictionary<string, T> Dict = new Dictionary<string, T>();
    public static T Get(string inp) => Dict.GetValueOrDefault(inp);

    protected void Awake()
    {
        if(ResetOnUnload)
        {
            Dict.Clear();
        }

        if(Dict.Count > 0) return; 
        foreach(T item in Lister)
        {
            if(item != null)
                Dict[NameGetter(item)] = item;
        }
    }
    public bool ResetOnUnload = true;
    
    #if UNITY_EDITOR
    [Button("Relaunch dictionary")]
    public void Relaunch()
    {
        Dict.Clear();
        Awake();
    }

    [Button("Clear Duplicates")]
    public void KillDupes()
    {
        Lister = new List<T>(new HashSet<T>(Lister));
    }
    #endif
}

public abstract class ObjectLibrary<T> : ObjectDictionary<T> where T : Object
{
    #if UNITY_EDITOR
    [CrossingLears.Button]
    public void FillViaResourcesLoadAll()
    {
        Lister.Clear();

        T[] loaded = Resources.LoadAll<T>("");

        foreach (var item in loaded)
        {
            if (item != null && !Lister.Contains(item))
            {
                Lister.Add(item);
            }
        }

        EditorUtility.SetDirty(this);
    }
    
    [CrossingLears.Button]
    public void FillViaAssetDatabase()
    {
        Lister.Clear();

        bool isMono = typeof(MonoBehaviour).IsAssignableFrom(typeof(T));

        if (isMono)
        {
            // MonoBehaviours live on prefabs
            string[] guids = AssetDatabase.FindAssets("t:Prefab");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                if (prefab == null) continue;

                T comp = prefab.GetComponent<T>();
                if (comp != null && !Lister.Contains(comp))
                {
                    Lister.Add(comp);
                }
            }
        }
        else
        {
            // Normal asset types (ScriptableObject, etc.)
            string filter = $"t:{typeof(T).Name}";
            string[] guids = AssetDatabase.FindAssets(filter);

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                T asset = AssetDatabase.LoadAssetAtPath<T>(path);

                if (asset != null && !Lister.Contains(asset))
                {
                    Lister.Add(asset);
                }
            }
        }
        
        EditorUtility.SetDirty(this);
        Debug.Log($"[ObjectDictionary] Loaded {Lister.Count} items via AssetDatabase");
    }
    #endif
}