using UnityEngine;

namespace CrossingLears
{
    public static class ObjectExtensions
    {
        public static T instantiate<T>(this T go) where T : Object =>
            Object.Instantiate(go);

        public static T instantiate<T>(this T original, Transform parent, bool worldPositionStays = false) where T : Object =>
            (T)Object.Instantiate((Object)original, parent, worldPositionStays);

        public static T instantiate<T>(this T go, Vector3 pos, Quaternion rot = default, Transform parent = null) where T : Object =>
            Object.Instantiate(go, pos, rot, parent);

        public static void open(this GameObject go) => go.SetActive(true);
        public static void close(this GameObject go) => go.SetActive(false);

        public static void open(this Component go) => go.gameObject.SetActive(true);
        public static void close(this Component go) => go.gameObject.SetActive(false);

        public static void destroy(this Object obj)
        {
            #if UNITY_EDITOR
                if (Application.isPlaying)
                    Object.Destroy(obj);
                else
                    Object.DestroyImmediate(obj);
            #else
                Object.Destroy(obj);
            #endif
        }
        
        public static void destroy(this Object obj, bool destroyAssets)
        {
            #if UNITY_EDITOR
                if (Application.isPlaying)
                    Object.Destroy(obj);
                else
                    Object.DestroyImmediate(obj, destroyAssets);
            #else
                Object.Destroy(obj);
            #endif
        }

        public static void destroyChildObjects(this Transform tr)
        {
            for (int i = tr.childCount - 1; i >= 0; i--)
            {
                Transform child = tr.GetChild(i);
                child.gameObject.destroy();
            }
        }

        public static void closeChildObjects(this Transform tr) { foreach (Transform child in tr) child.gameObject.close(); }
        
        public static void alphaChange(this SpriteRenderer spriteRenderer, float alpha)
        {
            Color color = spriteRenderer.color;
            color.a = alpha;
            spriteRenderer.color = color;
        }
    }

    public static class VectorExtensions 
    {        
        public static Vector2Int SnapToVector2(this Vector3 vector3) 
            => new Vector2Int(Mathf.RoundToInt(vector3.x), Mathf.RoundToInt(vector3.y));

        public static Vector3Int SnapToInt(this Vector3 vector3) 
            => new Vector3Int(Mathf.RoundToInt(vector3.x), Mathf.RoundToInt(vector3.y), Mathf.RoundToInt(vector3.z));

        public static Vector2Int SnapToInt(this Vector2 vector2) 
            => new Vector2Int(Mathf.RoundToInt(vector2.x), Mathf.RoundToInt(vector2.y));
            
        public static bool IsInRange(this Vector3 a, Vector3 b, float range) => (a - b).sqrMagnitude <= range * range;
    }
}