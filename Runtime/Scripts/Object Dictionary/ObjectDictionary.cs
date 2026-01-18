using System.Collections.Generic;
using CrossingLears;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public abstract class ObjectDictionary<T> : MonoBehaviour where T : Object
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

        Debug.Log($"[ObjectDictionary] Loaded {Lister.Count} items via Resources.LoadAll");
    }
    
    [CrossingLears.Button]
    public void FillViaAssetDatabase()
    {
        Lister.Clear();

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

        Debug.Log($"[ObjectDictionary] Loaded {Lister.Count} items via AssetDatabase");
    }
    #endif

}

public abstract class ObjectLibrary<T> : ObjectDictionary<T> where T : Object
{
    #if UNITY_EDITOR
    [Button("ResourceFindAll_Scriptable_Object")]
    public void FindAllSO()
    {
        T[] tee = Resources.LoadAll<T>("");
        foreach(var item in tee)
        {
            if(!Lister.Contains(item))
            {
                Lister.Add(item);
            }
        }
    }
    #endif
}