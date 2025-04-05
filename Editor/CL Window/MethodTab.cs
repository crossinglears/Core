using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace CrossingLearsEditor
{
    public class MethodTab : CL_WindowTab
    {
        public override string TabName => "Command";

        [Serializable]
        public class MethodIdentifier
        {
            public string TypeName;
            public string MethodName;

            public MethodIdentifier(string typeName, string methodName)
            {
                TypeName = typeName;
                MethodName = methodName;
            }
        }

        private List<(MethodInfo method, CL_CommandAttribute attr)> assetMethods = new();
        private List<(MethodInfo method, CL_CommandAttribute attr)> sceneMethods = new();

        private List<MethodIdentifier> assetMethodIDs = new();
        private List<MethodIdentifier> sceneMethodIDs = new();

        private bool assetMethodsBuilt = false;
        private bool sceneMethodsBuilt = false;

        private bool AutoReload;

        public override void Awake()
        {
            base.Awake();
            AutoReload = EditorPrefs.GetBool("MethodTab.AutoReload");
            FindAllMethods();
        }

        public override void OnFocus()
        {
            base.OnFocus();

            AutoReload = EditorPrefs.GetBool("MethodTab.AutoReload");
            if(AutoReload)
                FindAllMethods();
        }

        private void FindAllMethods()
        {
            assetMethods = FindCommandMethods(MethodType.Asset);
            assetMethodIDs = assetMethods.Select(m => new MethodIdentifier(m.method.DeclaringType.FullName, m.method.Name)).ToList();
            assetMethodsBuilt = true;

            sceneMethods = FindCommandMethods(MethodType.SceneObject);
            sceneMethodIDs = sceneMethods.Select(m => new MethodIdentifier(m.method.DeclaringType.FullName, m.method.Name)).ToList();
            sceneMethodsBuilt = true;
        }

        public override void DrawContent()
        {
            GUILayout.BeginHorizontal();

            bool newAutoReload = EditorGUILayout.Toggle(AutoReload, GUILayout.Width(20));

            if (newAutoReload != AutoReload)
            {
                AutoReload = newAutoReload;
                EditorPrefs.SetBool("MethodTab.AutoReload", AutoReload);
            }

            EditorGUILayout.LabelField("Auto Reload", GUILayout.Width(80));
            
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(new GUIContent("Find Commands", "Finds all method with \"CL_Command\" attribute inside all classes that have \"CL_CommandHolder\" attribute"), GUILayout.Width(150)))
            {
                FindAllMethods();
            }
            GUILayout.EndHorizontal();

            if (!assetMethodsBuilt && assetMethodIDs.Count > 0)
            {
                assetMethods = RebuildMethods(assetMethodIDs, MethodType.Asset);
                assetMethodsBuilt = true;
            }

            if (!sceneMethodsBuilt && sceneMethodIDs.Count > 0)
            {
                sceneMethods = RebuildMethods(sceneMethodIDs, MethodType.SceneObject);
                sceneMethodsBuilt = true;
            }

            GUILayout.Label("Asset Commands", EditorStyles.boldLabel);
            if (assetMethods.Count > 0)
                DrawCommandButtons(assetMethods, MethodType.Asset);
            else GUILayout.Label("\tNone");

            GUILayout.Space(10);

            GUILayout.Label("Scene Commands", EditorStyles.boldLabel);
            if (sceneMethods.Count > 0)
                DrawCommandButtons(sceneMethods, MethodType.SceneObject);
            else GUILayout.Label("\tNone");

            GUILayout.Space(10);
            EditorGUILayout.HelpBox("Finds all method with \"CL_Command\" attribute inside all classes that have \"CL_CommandHolder\" attribute", MessageType.Info, true);
        }

        private List<(MethodInfo, CL_CommandAttribute)> FindCommandMethods(MethodType type)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(asm => asm.GetTypes())
                .Where(t => t.GetCustomAttribute<CL_CommandHolderAttribute>() != null) // Filter classes
                .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                    .SelectMany(m =>
                        m.GetCustomAttributes(typeof(CL_CommandAttribute), false)
                            .Cast<CL_CommandAttribute>()
                            .Where(attr => attr.m_type == type)
                            .Select(attr => (m, attr))))
                .ToList();
        }

        private List<(MethodInfo, CL_CommandAttribute)> RebuildMethods(List<MethodIdentifier> ids, MethodType type)
        {
            var results = new List<(MethodInfo, CL_CommandAttribute)>();
            foreach (var id in ids)
            {
                var typeObj = Type.GetType(id.TypeName);
                if (typeObj == null) continue;
                var method = typeObj.GetMethod(id.MethodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                if (method == null) continue;
                var attr = method.GetCustomAttributes(typeof(CL_CommandAttribute), false)
                                 .Cast<CL_CommandAttribute>()
                                 .FirstOrDefault(a => a.m_type == type);
                if (attr != null)
                    results.Add((method, attr));
            }
            return results;
        }

        private void DrawCommandButtons(List<(MethodInfo method, CL_CommandAttribute attr)> methods, MethodType type)
        {
            foreach (var (method, attr) in methods)
            {
                // Create a horizontal layout to hold the label and the button
                GUILayout.BeginHorizontal();
                
                // Label showing the class name
                // GUILayout.Label(method.DeclaringType.Name, GUILayout.Width(60));
                GUILayout.Label(method.DeclaringType.Name, GUILayout.Width(60), GUILayout.ExpandWidth(false));
                
                // Button for the method
                if (GUILayout.Button(string.IsNullOrEmpty(attr.Key) ? ObjectNames.NicifyVariableName(method.Name) : attr.Key, GUILayout.MinWidth(100)))
                {
                    if (type == MethodType.Asset)
                    {
                        var instances = AssetDatabase.FindAssets($"t:{method.DeclaringType.Name}")
                            .Select(guid => AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid), method.DeclaringType));

                        foreach (var instance in instances)
                        {
                            method.Invoke(instance, null);
                            EditorUtility.SetDirty(instance);
                        }
                    }
                    else if (type == MethodType.SceneObject)
                    {
                        var instances = UnityEngine.Object.FindObjectsOfType(method.DeclaringType);
                        foreach (var instance in instances)
                        {
                            method.Invoke(instance, null);
                            EditorSceneManager.MarkSceneDirty(((Component)instance).gameObject.scene);
                        }
                    }
                }
                
                GUILayout.EndHorizontal(); // End the horizontal layout
            }
        }
    }

    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public class CL_CommandAttribute : Attribute
    {
        public string Key { get; set; }
        public MethodType m_type { get; set; } = MethodType.Asset;

        public CL_CommandAttribute(string key, MethodType methodType)
        {
            Key = key;
            m_type = methodType;
        }

        public CL_CommandAttribute(MethodType methodType)
        {
            m_type = methodType;
        }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
    public class CL_CommandHolderAttribute : Attribute
    {
        public CL_CommandHolderAttribute() { }
    }

    public enum MethodType
    {
        Asset,
        SceneObject,
    }
}

