using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace CrossingLears.Editor
{
    public class FindTab : CL_WindowTab
    {
        public override string TabName => "Find";

        private const int PageSize = 100;

        private readonly List<Object> cachedFoundObjects = new List<Object>();
        private readonly List<Object> foundObjects = new List<Object>();
        private ReorderableList foundObjectsList;
        private FindType findType;
        private FindTypeProject findTypeProject;
        private FindTypeHeirarchy findTypeHeirarchy;
        private int currentPage = 1;
        private bool viewAll;

        public override void OnEnable()
        {
            BuildFoundObjectsList();
        }

        public override void DrawContent()
        {
            if (foundObjectsList == null)
            {
                BuildFoundObjectsList();
            }

            SearchKey = EditorGUILayout.TextField("Search Key", SearchKey);
            findType = (FindType)GUILayout.Toolbar((int)findType, new string[] { "Project", "Heirarchy" });

            if (findType == FindType.Project)
            {
                findTypeProject = (FindTypeProject)EditorGUILayout.EnumPopup("Find Type", findTypeProject);
            }
            else
            {
                findTypeHeirarchy = (FindTypeHeirarchy)EditorGUILayout.EnumPopup("Find Type", findTypeHeirarchy);
            }

            if (GUILayout.Button("Refresh"))
            {
                RefreshFoundObjects();
            }

            DrawPageControls();
            GUILayout.Space(8f);
            foundObjectsList.DoLayoutList();
        }

        public string SearchKey;
        
        public enum FindType
        {
            Project,
            Heirarchy
        }

        public enum FindTypeProject
        {
            ObjectOfType,
            ObjectWithName,
            AssetPath,
            PrefabObjectOfType,
            PrefabObjectWithName,
            Text, // textmeshpro or text legacy
            MaterialShader,
        }

        public enum FindTypeHeirarchy
        {
            ObjectOfType,
            ObjectWithName,
            ComponentOfType,
            ComponentWithName,
            TextWithMatch,
            TextObjectWithName,
        }

        private void BuildFoundObjectsList()
        {
            foundObjectsList = new ReorderableList(foundObjects, typeof(Object), true, true, false, false);
            foundObjectsList.drawHeaderCallback = DrawFoundObjectsHeader;
            foundObjectsList.drawElementCallback = DrawFoundObjectElement;
            foundObjectsList.elementHeight = EditorGUIUtility.singleLineHeight + 4f;
        }

        private void DrawFoundObjectsHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "Found Objects (" + foundObjects.Count + "/" + cachedFoundObjects.Count + ")");
        }

        private void DrawFoundObjectElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            rect.y += 2f;
            rect.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.BeginDisabledGroup(true);
            EditorGUI.ObjectField(rect, foundObjects[index], typeof(Object), true);
            EditorGUI.EndDisabledGroup();
        }

        private void RefreshFoundObjects()
        {
            cachedFoundObjects.Clear();
            foundObjects.Clear();
            currentPage = 1;
            viewAll = false;

            if (findType == FindType.Project)
            {
                RefreshProjectObjects();
            }
            else
            {
                RefreshHeirarchyObjects();
            }

            ApplyFoundObjectsPage();
        }

        private void DrawPageControls()
        {
            if (cachedFoundObjects.Count <= PageSize)
            {
                return;
            }

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();

            GUI.enabled = !viewAll && currentPage > 1;
            if (GUILayout.Button("Previous", GUILayout.Width(80f)))
            {
                currentPage--;
                ApplyFoundObjectsPage();
            }

            GUI.enabled = !viewAll;
            EditorGUI.BeginChangeCheck();
            int newPage = EditorGUILayout.IntField(currentPage, GUILayout.Width(50f));
            if (EditorGUI.EndChangeCheck())
            {
                currentPage = newPage;
                ApplyFoundObjectsPage();
            }

            GUI.enabled = !viewAll && currentPage < GetPageCount();
            if (GUILayout.Button("Next", GUILayout.Width(80f)))
            {
                currentPage++;
                ApplyFoundObjectsPage();
            }

            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(GetPageRangeText());
            if (GUILayout.Button("View All", GUILayout.Width(80f)))
            {
                viewAll = true;
                ApplyFoundObjectsPage();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private void ApplyFoundObjectsPage()
        {
            foundObjects.Clear();

            if (cachedFoundObjects.Count == 0)
            {
                currentPage = 1;
                return;
            }

            if (viewAll)
            {
                for (int i = 0; i < cachedFoundObjects.Count; i++)
                {
                    foundObjects.Add(cachedFoundObjects[i]);
                }

                return;
            }

            int pageCount = GetPageCount();
            currentPage = Mathf.Clamp(currentPage, 1, pageCount);
            int startIndex = (currentPage - 1) * PageSize;
            int endIndex = Mathf.Min(startIndex + PageSize, cachedFoundObjects.Count);

            for (int i = startIndex; i < endIndex; i++)
            {
                foundObjects.Add(cachedFoundObjects[i]);
            }
        }

        private int GetPageCount()
        {
            return Mathf.CeilToInt((float)cachedFoundObjects.Count / PageSize);
        }

        private string GetPageRangeText()
        {
            if (cachedFoundObjects.Count == 0)
            {
                return "0-0 (Total: 0)";
            }

            if (viewAll)
            {
                return "1-" + cachedFoundObjects.Count + " (Total: " + cachedFoundObjects.Count + ")";
            }

            int startIndex = (currentPage - 1) * PageSize + 1;
            int endIndex = Mathf.Min(currentPage * PageSize, cachedFoundObjects.Count);
            return startIndex + "-" + endIndex + " (Total: " + cachedFoundObjects.Count + ")";
        }

        private void RefreshProjectObjects()
        {
            string[] assetPaths = AssetDatabase.GetAllAssetPaths();
            for (int i = 0; i < assetPaths.Length; i++)
            {
                Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPaths[i]);
                for (int j = 0; j < assets.Length; j++)
                {
                    AddProjectObjectIfMatch(assets[j], assetPaths[i]);
                }

                GameObject gameObject = AssetDatabase.LoadAssetAtPath<GameObject>(assetPaths[i]);
                if (gameObject != null)
                {
                    AddProjectGameObjectComponentsIfMatch(gameObject);
                }
            }
        }

        private void AddProjectObjectIfMatch(Object targetObject, string assetPath)
        {
            if (targetObject == null)
            {
                return;
            }

            if (findTypeProject == FindTypeProject.ObjectOfType && TextMatches(targetObject.GetType().Name))
            {
                AddFoundObject(targetObject);
            }

            if (findTypeProject == FindTypeProject.ObjectWithName && TextMatches(targetObject.name))
            {
                AddFoundObject(targetObject);
            }

            if (findTypeProject == FindTypeProject.AssetPath && TextMatches(assetPath))
            {
                AddFoundObject(targetObject);
            }

            if (findTypeProject == FindTypeProject.MaterialShader)
            {
                Material material = targetObject as Material;
                if (material != null && material.shader != null && TextMatches(material.shader.name))
                {
                    AddFoundObject(targetObject);
                }
            }
        }

        private void AddProjectGameObjectComponentsIfMatch(GameObject gameObject)
        {
            if (findTypeProject == FindTypeProject.ObjectOfType || findTypeProject == FindTypeProject.PrefabObjectOfType)
            {
                Component[] components = gameObject.GetComponentsInChildren<Component>(true);
                for (int i = 0; i < components.Length; i++)
                {
                    if (components[i] != null && TextMatches(components[i].GetType().Name))
                    {
                        AddFoundObject(components[i]);
                    }
                }
            }

            if (findTypeProject == FindTypeProject.ObjectWithName || findTypeProject == FindTypeProject.PrefabObjectWithName)
            {
                Transform[] transforms = gameObject.GetComponentsInChildren<Transform>(true);
                for (int i = 0; i < transforms.Length; i++)
                {
                    if (TextMatches(transforms[i].name))
                    {
                        AddFoundObject(transforms[i].gameObject);
                    }
                }
            }

            if (findTypeProject == FindTypeProject.Text)
            {
                Text[] texts = gameObject.GetComponentsInChildren<Text>(true);
                for (int i = 0; i < texts.Length; i++)
                {
                    if (TextMatches(texts[i].text))
                    {
                        AddFoundObject(texts[i]);
                    }
                }

                TMP_Text[] tmpTexts = gameObject.GetComponentsInChildren<TMP_Text>(true);
                for (int i = 0; i < tmpTexts.Length; i++)
                {
                    if (TextMatches(tmpTexts[i].text))
                    {
                        AddFoundObject(tmpTexts[i]);
                    }
                }
            }
        }

        private void RefreshHeirarchyObjects()
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                GameObject[] rootObjects = scene.GetRootGameObjects();
                for (int j = 0; j < rootObjects.Length; j++)
                {
                    AddHeirarchyObjectsIfMatch(rootObjects[j]);
                }
            }
        }

        private void AddHeirarchyObjectsIfMatch(GameObject gameObject)
        {
            if (findTypeHeirarchy == FindTypeHeirarchy.ObjectOfType && TextMatches(typeof(GameObject).Name))
            {
                AddFoundObject(gameObject);
            }

            Transform[] transforms = gameObject.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < transforms.Length; i++)
            {
                if (findTypeHeirarchy == FindTypeHeirarchy.ObjectWithName && TextMatches(transforms[i].name))
                {
                    AddFoundObject(transforms[i].gameObject);
                }
            }

            Component[] components = gameObject.GetComponentsInChildren<Component>(true);
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] == null)
                {
                    continue;
                }

                if (findTypeHeirarchy == FindTypeHeirarchy.ObjectOfType && TextMatches(components[i].GetType().Name))
                {
                    AddFoundObject(components[i]);
                }

                if (findTypeHeirarchy == FindTypeHeirarchy.ComponentOfType && TextMatches(components[i].GetType().Name))
                {
                    AddFoundObject(components[i]);
                }

                if (findTypeHeirarchy == FindTypeHeirarchy.ComponentWithName && TextMatches(components[i].name))
                {
                    AddFoundObject(components[i]);
                }
            }

            AddHeirarchyTextObjectsIfMatch(gameObject);
        }

        private void AddHeirarchyTextObjectsIfMatch(GameObject gameObject)
        {
            Text[] texts = gameObject.GetComponentsInChildren<Text>(true);
            for (int i = 0; i < texts.Length; i++)
            {
                if (findTypeHeirarchy == FindTypeHeirarchy.TextWithMatch && TextMatches(texts[i].text))
                {
                    AddFoundObject(texts[i]);
                }

                if (findTypeHeirarchy == FindTypeHeirarchy.TextObjectWithName && TextMatches(texts[i].name))
                {
                    AddFoundObject(texts[i]);
                }
            }

            TMP_Text[] tmpTexts = gameObject.GetComponentsInChildren<TMP_Text>(true);
            for (int i = 0; i < tmpTexts.Length; i++)
            {
                if (findTypeHeirarchy == FindTypeHeirarchy.TextWithMatch && TextMatches(tmpTexts[i].text))
                {
                    AddFoundObject(tmpTexts[i]);
                }

                if (findTypeHeirarchy == FindTypeHeirarchy.TextObjectWithName && TextMatches(tmpTexts[i].name))
                {
                    AddFoundObject(tmpTexts[i]);
                }
            }
        }

        private bool TextMatches(string value)
        {
            if (string.IsNullOrEmpty(SearchKey))
            {
                return true;
            }

            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            return value.IndexOf(SearchKey, System.StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void AddFoundObject(Object targetObject)
        {
            if (!cachedFoundObjects.Contains(targetObject))
            {
                cachedFoundObjects.Add(targetObject);
            }
        }
    }
}
