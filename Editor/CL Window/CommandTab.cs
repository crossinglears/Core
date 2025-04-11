using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using CrossingLears;

namespace CrossingLearsEditor
{
    public class CommandTab : CL_WindowTab
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
        private List<(MethodInfo method, CL_CommandAttribute attr)> staticMethods = new();

        private List<MethodIdentifier> assetMethodIDs = new();
        private List<MethodIdentifier> sceneMethodIDs = new();
        private List<MethodIdentifier> staticMethodIDs = new();

        private bool assetMethodsBuilt = false;
        private bool sceneMethodsBuilt = false;
        private bool staticMethodsBuilt = false;

        private bool AutoReload;

        public void CallAll(string commandName)
        {
            // Call methods from asset methods
            foreach (var (method, attr) in assetMethods)
            {
                if (attr.Key == commandName || string.IsNullOrEmpty(attr.Key) && ObjectNames.NicifyVariableName(method.Name) == commandName)
                {
                    var instances = AssetDatabase.FindAssets($"t:{method.DeclaringType.Name}")
                        .Select(guid => AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid), method.DeclaringType));

                    foreach (var instance in instances)
                    {
                        method.Invoke(instance, null);
                        EditorUtility.SetDirty(instance);
                    }
                }
            }

            // Call methods from scene methods
            foreach (var (method, attr) in sceneMethods)
            {
                if (attr.Key == commandName || string.IsNullOrEmpty(attr.Key) && ObjectNames.NicifyVariableName(method.Name) == commandName)
                {
                    UnityEngine.Object[] sceneInstances = UnityEngine.Object.FindObjectsByType(method.DeclaringType, FindObjectsInactive.Include, FindObjectsSortMode.None);

                    foreach (var instance in sceneInstances)
                    {
                        method.Invoke(instance, null);
                        EditorSceneManager.MarkSceneDirty(((Component)instance).gameObject.scene);
                    }
                }
            }

            // Call static methods
            foreach (var (method, attr) in staticMethods)
            {
                if (attr.Key == commandName || string.IsNullOrEmpty(attr.Key) && ObjectNames.NicifyVariableName(method.Name) == commandName)
                {
                    if (method.IsStatic)
                    {
                        method.Invoke(null, null);
                    }
                    else
                    {
                        Debug.LogError("Method should have static keyword");
                    }
                }
            }
        }

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

            staticMethods = FindCommandMethods(MethodType.Static);
            staticMethodIDs = sceneMethods.Select(m => new MethodIdentifier(m.method.DeclaringType.FullName, m.method.Name)).ToList();
            staticMethodsBuilt = true;
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

            if (!staticMethodsBuilt && staticMethodIDs.Count > 0)
            {
                staticMethods = RebuildMethods(sceneMethodIDs, MethodType.SceneObject);
                staticMethodsBuilt = true;
            }

            GUILayout.Space(10);

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
            GUILayout.Label("Static Commands", EditorStyles.boldLabel);
            if (staticMethods.Count > 0)
                DrawCommandButtons(staticMethods, MethodType.Static);
            else GUILayout.Label("\tNone");

            GUILayout.Space(10);
            EditorGUILayout.HelpBox("Finds all method with \"CL_Command\" attribute inside all classes that have \"CL_CommandHolder\" attribute", MessageType.Info, true);

            GUILayout.Space(40);
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
            List<(MethodInfo, CL_CommandAttribute)> results = new List<(MethodInfo, CL_CommandAttribute)>();
            foreach (var id in ids)
            {
                Type typeObj = Type.GetType(id.TypeName);
                if (typeObj == null) continue;
                MethodInfo method = typeObj.GetMethod(id.MethodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                if (method == null) continue;
                CL_CommandAttribute attr = method.GetCustomAttributes(typeof(CL_CommandAttribute), false)
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
                GUILayout.BeginHorizontal();
                
                GUILayout.Space(10);
                GUILayout.Label(new GUIContent(method.DeclaringType.Name, method.DeclaringType.Name), GUILayout.Width(70), GUILayout.ExpandWidth(false));
                                
                if (LastAll == attr.Key)
                {
                    GUI.contentColor = Color.cyan;
                }

                if (GUILayout.Button(string.IsNullOrEmpty(attr.Key) ? ObjectNames.NicifyVariableName(method.Name) : attr.Key, GUILayout.MinWidth(100)))
                {
                    switch (type)
                    {
                        case MethodType.Asset:
                            var instances = AssetDatabase.FindAssets($"t:{method.DeclaringType.Name}")
                                .Select(guid => AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid), method.DeclaringType));

                            foreach (var instance in instances)
                            {
                                method.Invoke(instance, null);
                                EditorUtility.SetDirty(instance);
                            }
                            break;

                        case MethodType.SceneObject:
                            UnityEngine.Object[] sceneInstances = UnityEngine.Object.FindObjectsByType(method.DeclaringType, FindObjectsInactive.Include, FindObjectsSortMode.None);

                            foreach (var instance in sceneInstances)
                            {
                                method.Invoke(instance, null);
                                EditorSceneManager.MarkSceneDirty(((Component)instance).gameObject.scene);
                            }
                            break;

                        case MethodType.Static:
                            if (method.IsStatic)
                            {
                                method.Invoke(null, null);
                            }
                            else
                            {
                                Debug.LogError("Method should have static keyword");
                            }
                            break;
                    }
                }
                if (GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition) && Event.current.type == EventType.Repaint)
                {
                    LastAll = "CommandTabLastAll";
                }

                if(attr.CanCallAll)
                {
                    if (GUILayout.Button("Call All", GUILayout.Width(60)))
                    {
                        CallAll(attr.Key);
                    }

                    // Set LastAll when the button is hovered
                    if (GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition) && Event.current.type == EventType.Repaint)
                    {
                        if(string.IsNullOrEmpty(attr.Key))
                        {
                            LastAll = ObjectNames.NicifyVariableName(method.Name);
                        }
                        else
                            LastAll = attr.Key;
                    }
                }          
                GUI.contentColor = Color.white; // Set font color to white
                GUILayout.EndHorizontal();
            }
        }
        public string LastAll = "CommandTabLastAll";
    }
}

