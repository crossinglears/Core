using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Networking;

namespace CrossingLears.Editor
{
    public class ScriptsTab : CL_WindowTab
    {
        public override string TabName => "Script";

        private static readonly string LocalScriptsRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "CrossingLears", "SharedScripts");

        private const bool EnableRemoteRepo = true;
        private const string GitHubOwner = "crossinglears";
        private const string GitHubRepo = "RecyclableScripts";
        private const string GitHubBranch = "main";
        private const string GitHubRootFolder = "";
        private const int RequestTimeout = 8;

        private string searchText = string.Empty;
        private Vector2 scroll;
        private bool searched;
        private List<ScriptEntry> results = new List<ScriptEntry>();

        private GUIStyle toolbarBoxStyle;
        private GUIStyle resultBoxStyle;
        private GUIStyle entryBoxStyle;
        private GUIStyle adaptiveTitleStyle;
        private GUIStyle descriptionStyle;
        private GUIStyle metaLabelStyle;
        private GUIStyle metaValueStyle;
        private GUIStyle sourceBadgeStyle;
        private GUIStyle emptyStateStyle;
        private GUIStyle countStyle;
        private GUIStyle entryTitleStyle;

        public override void DrawTitle()
        {
            EnsureStyles();

            EditorGUILayout.BeginVertical(toolbarBoxStyle);

            EditorGUILayout.BeginHorizontal();
            DrawAdaptiveTabLabel();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("New", GUILayout.Width(80f), GUILayout.Height(24f)))
            {
                OpenCreateWindow();
            }

            if (GUILayout.Button("Open Local Folder", GUILayout.Width(130f), GUILayout.Height(24f)))
            {
                EnsureLocalFolderExists();
                EditorUtility.RevealInFinder(LocalScriptsRoot);
            }

            EditorGUILayout.EndHorizontal();

            GUILayout.Space(6f);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Search", GUILayout.Width(45f));
            searchText = GUILayout.TextField(searchText, GUILayout.ExpandWidth(true), GUILayout.Height(22f));

            if (GUILayout.Button("Search", GUILayout.Width(80f), GUILayout.Height(24f)))
            {
                DoSearch();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
            GUILayout.Space(6f);
        }

        public override void DrawContent()
        {
            EnsureStyles();

            if (!Directory.Exists(LocalScriptsRoot))
            {
                EditorGUILayout.BeginVertical(resultBoxStyle);
                EditorGUILayout.HelpBox("Local shared scripts folder does not exist yet.\n" + LocalScriptsRoot, MessageType.Info);

                if (GUILayout.Button("Create Local Shared Folder", GUILayout.Height(28f)))
                {
                    EnsureLocalFolderExists();
                }

                EditorGUILayout.EndVertical();
                return;
            }

            if (!searched)
            {
                EditorGUILayout.BeginVertical(resultBoxStyle);
                GUILayout.Space(8f);
                GUILayout.Label("Search local reusable scripts. GitHub repo results are optional and skipped automatically if unavailable.", emptyStateStyle);
                GUILayout.Space(8f);
                EditorGUILayout.EndVertical();
                return;
            }

            // EditorGUILayout.BeginHorizontal(resultBoxStyle);
            // GUILayout.Label("Results:", EditorStyles.boldLabel);
            // GUILayout.Space(4f);
            // GUILayout.Label(results.Count.ToString(), countStyle);
            // GUILayout.FlexibleSpace();
            // EditorGUILayout.EndHorizontal();

            // GUILayout.Space(4f);

            scroll = EditorGUILayout.BeginScrollView(scroll);

            if (results.Count == 0)
            {
                EditorGUILayout.BeginVertical(resultBoxStyle);
                GUILayout.Space(10f);
                GUILayout.Label("No matching scripts found.", emptyStateStyle);
                GUILayout.Space(10f);
                EditorGUILayout.EndVertical();
            }
            else
            {
                foreach (ScriptEntry entry in results)
                {
                    DrawEntry(entry);
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawAdaptiveTabLabel()
        {
            GUIContent titleContent = new GUIContent(TabName);
            float maxWidth = Mathf.Max(40f, EditorGUIUtility.currentViewWidth - 260f);
            Vector2 size = adaptiveTitleStyle.CalcSize(titleContent);
            float drawWidth = Mathf.Min(size.x, maxWidth);
            GUILayout.Label(titleContent, adaptiveTitleStyle, GUILayout.Width(drawWidth), GUILayout.Height(20f));
        }

        private void DrawEntry(ScriptEntry entry)
        {
            Color previousContentColor = GUI.contentColor;
            Color previousBackgroundColor = GUI.backgroundColor;

            EditorGUILayout.BeginVertical(entryBoxStyle);

            EditorGUILayout.BeginHorizontal(GUILayout.Height(20f));
            GUILayout.Label(entry.Name, entryTitleStyle, GUILayout.ExpandWidth(false), GUILayout.Height(20f));

            GUI.contentColor = entry.Source == ScriptSource.Remote ? new Color(0.35f, 0.85f, 0.35f) : new Color(0.95f, 0.85f, 0.25f);
            GUILayout.Label(entry.SourceLabel, sourceBadgeStyle, GUILayout.Width(60f), GUILayout.Height(20f));
            GUI.contentColor = previousContentColor;

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            if (!string.IsNullOrWhiteSpace(entry.Description))
            {
                GUILayout.Space(4f);
                GUILayout.Label(entry.Description, descriptionStyle);
            }

            GUILayout.Space(6f);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Tags", metaLabelStyle, GUILayout.Width(90f));
            GUILayout.Label(entry.Tags.Count > 0 ? string.Join(", ", entry.Tags) : "-", metaValueStyle);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Dependencies", metaLabelStyle, GUILayout.Width(90f));
            GUILayout.Label(entry.Dependencies.Count > 0 ? string.Join(", ", entry.Dependencies) : "-", metaValueStyle);
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(8f);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("View", GUILayout.Width(70f), GUILayout.Height(24f)))
            {
                ViewEntry(entry);
            }

            if (GUILayout.Button("Insert", GUILayout.Width(70f), GUILayout.Height(24f)))
            {
                InsertEntry(entry);
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
            GUILayout.Space(4f);

            GUI.contentColor = previousContentColor;
            GUI.backgroundColor = previousBackgroundColor;
        }

        private void EnsureStyles()
        {
            if (toolbarBoxStyle == null)
            {
                toolbarBoxStyle = new GUIStyle(EditorStyles.helpBox)
                {
                    padding = new RectOffset(10, 10, 10, 10),
                    margin = new RectOffset(0, 0, 0, 0)
                };
            }

            if (resultBoxStyle == null)
            {
                resultBoxStyle = new GUIStyle(EditorStyles.helpBox)
                {
                    padding = new RectOffset(10, 10, 8, 8),
                    margin = new RectOffset(0, 0, 0, 0)
                };
            }

            if (entryBoxStyle == null)
            {
                entryBoxStyle = new GUIStyle(EditorStyles.helpBox)
                {
                    padding = new RectOffset(10, 10, 10, 10),
                    margin = new RectOffset(0, 0, 0, 0)
                };
            }

            if (adaptiveTitleStyle == null)
            {
                adaptiveTitleStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    fontSize = 12,
                    wordWrap = false,
                    clipping = TextClipping.Clip,
                    alignment = TextAnchor.MiddleLeft
                };
            }

            if (entryTitleStyle == null)
            {
                entryTitleStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    fontSize = 12,
                    wordWrap = false,
                    clipping = TextClipping.Clip,
                    alignment = TextAnchor.MiddleLeft,
                    fixedHeight = 20f,
                    padding = new RectOffset(0, 0, 0, 0),
                    margin = new RectOffset(0, 0, 0, 0)
                };
            }

            if (descriptionStyle == null)
            {
                descriptionStyle = new GUIStyle(EditorStyles.label)
                {
                    fontSize = 11,
                    wordWrap = true,
                    alignment = TextAnchor.UpperLeft
                };
            }

            if (metaLabelStyle == null)
            {
                metaLabelStyle = new GUIStyle(EditorStyles.label)
                {
                    fontSize = 11,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.UpperLeft,
                    wordWrap = false
                };
            }

            if (metaValueStyle == null)
            {
                metaValueStyle = new GUIStyle(EditorStyles.label)
                {
                    fontSize = 11,
                    wordWrap = true,
                    alignment = TextAnchor.UpperLeft
                };
            }

            if (sourceBadgeStyle == null)
            {
                sourceBadgeStyle = new GUIStyle(EditorStyles.label)
                {
                    fontSize = 11,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleLeft,
                    wordWrap = false,
                    fixedHeight = 20f,
                    padding = new RectOffset(0, 0, 0, 0),
                    margin = new RectOffset(6, 0, 0, 0)
                };
            }

            if (emptyStateStyle == null)
            {
                emptyStateStyle = new GUIStyle(EditorStyles.centeredGreyMiniLabel)
                {
                    wordWrap = true,
                    alignment = TextAnchor.MiddleCenter
                };
            }

            if (countStyle == null)
            {
                countStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    fontSize = 12
                };
            }
        }

        private void DoSearch()
        {
            searched = true;

            List<ScriptEntry> merged = new List<ScriptEntry>();

            try
            {
                merged.AddRange(FindLocalScripts(searchText));
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[Scripts] Local search failed: " + ex.Message);
            }

            if (EnableRemoteRepo)
            {
                try
                {
                    merged.AddRange(FindRemoteScripts(searchText));
                }
                catch (Exception ex)
                {
                    Debug.Log("[Scripts] Remote repo skipped: " + ex.Message);
                }
            }

            results = merged.GroupBy(x => x.UniqueId, StringComparer.OrdinalIgnoreCase).Select(x => x.First()).OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase).ToList();
        }

        private List<ScriptEntry> FindLocalScripts(string query)
        {
            List<ScriptEntry> found = new List<ScriptEntry>();

            if (!Directory.Exists(LocalScriptsRoot))
            {
                return found;
            }

            string normalized = Normalize(query);
            string[] files = Directory.GetFiles(LocalScriptsRoot, "*.cs", SearchOption.AllDirectories);

            foreach (string file in files)
            {
                try
                {
                    string text = File.ReadAllText(file);
                    ScriptEntry entry = ParseMetadata(text);

                    if (string.IsNullOrWhiteSpace(entry.Name))
                    {
                        entry.Name = Path.GetFileNameWithoutExtension(file);
                    }

                    entry.Source = ScriptSource.Local;
                    entry.SourceLabel = "Local";
                    entry.FilePath = file;
                    entry.UniqueId = "local:" + file.Replace('\\', '/');

                    if (Matches(entry, normalized))
                    {
                        found.Add(entry);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning("[Scripts] Failed reading local script '" + file + "': " + ex.Message);
                }
            }

            return found;
        }

        private List<ScriptEntry> FindRemoteScripts(string query)
        {
            List<ScriptEntry> found = new List<ScriptEntry>();
            string normalized = Normalize(query);
            List<GitHubContentItem> remoteFiles = GetRemoteCsFilesRecursive(GitHubRootFolder);

            foreach (GitHubContentItem file in remoteFiles)
            {
                try
                {
                    string text = DownloadText(file.download_url);

                    if (string.IsNullOrWhiteSpace(text))
                    {
                        continue;
                    }

                    ScriptEntry entry = ParseMetadata(text);

                    if (string.IsNullOrWhiteSpace(entry.Name))
                    {
                        entry.Name = Path.GetFileNameWithoutExtension(file.path);
                    }

                    entry.Source = ScriptSource.Remote;
                    entry.SourceLabel = "GitHub";
                    entry.RawUrl = file.download_url;
                    entry.RepoPath = file.path;
                    entry.ViewUrl = BuildGitHubBlobUrl(file.path);
                    entry.UniqueId = "remote:" + file.path;
                    entry.CachedText = text;

                    if (Matches(entry, normalized))
                    {
                        found.Add(entry);
                    }
                }
                catch (Exception ex)
                {
                    Debug.Log("[Scripts] Skipped remote file '" + file.path + "': " + ex.Message);
                }
            }

            return found;
        }

        private bool Matches(ScriptEntry entry, string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return true;
            }

            if ((entry.Name ?? string.Empty).ToLowerInvariant().Contains(query))
            {
                return true;
            }

            if ((entry.Description ?? string.Empty).ToLowerInvariant().Contains(query))
            {
                return true;
            }

            if (entry.Tags.Any(x => x.ToLowerInvariant().Contains(query)))
            {
                return true;
            }

            if (entry.Dependencies.Any(x => x.ToLowerInvariant().Contains(query)))
            {
                return true;
            }

            if (entry.Packages.Any(x => x.ToLowerInvariant().Contains(query)))
            {
                return true;
            }

            return false;
        }

        private void OpenCreateWindow()
        {
            EnsureLocalFolderExists();
            ScriptCreateWindow.Open(this);
        }

        private void CreateScriptFile(string scriptName, string description, string tagsCsv, string dependenciesCsv, string packagesCsv)
        {
            scriptName = SanitizeClassName(scriptName);

            if (string.IsNullOrWhiteSpace(scriptName))
            {
                EditorUtility.DisplayDialog("Invalid Name", "Script name is required.", "OK");
                return;
            }

            EnsureLocalFolderExists();

            string filePath = Path.Combine(LocalScriptsRoot, scriptName + ".cs");

            if (File.Exists(filePath))
            {
                EditorUtility.DisplayDialog("Already Exists", "Script already exists:\n" + filePath, "OK");
                return;
            }

            string content = BuildScriptTemplate(scriptName, description, tagsCsv, dependenciesCsv, packagesCsv);
            File.WriteAllText(filePath, content, new UTF8Encoding(false));

            AssetDatabase.Refresh();
            InternalEditorUtility.OpenFileAtLineExternal(filePath, 1);

            if (searched)
            {
                DoSearch();
            }
        }

        private string BuildScriptTemplate(string scriptName, string description, string tagsCsv, string dependenciesCsv, string packagesCsv)
        {
            return
$@"/*
Name: {scriptName}
Description: {description}
Tags: {tagsCsv}
Dependencies: {dependenciesCsv}
Packages: {packagesCsv}
*/

using UnityEngine;

public class {scriptName} : MonoBehaviour
{{
    private void Start()
    {{
        
    }}

    private void Update()
    {{
        
    }}
}}
";
        }

        private void ViewEntry(ScriptEntry entry)
        {
            if (entry.Source == ScriptSource.Local)
            {
                if (File.Exists(entry.FilePath))
                {
                    InternalEditorUtility.OpenFileAtLineExternal(entry.FilePath, 1);
                }
                else
                {
                    EditorUtility.DisplayDialog("Missing File", "Local file not found:\n" + entry.FilePath, "OK");
                }

                return;
            }

            if (!string.IsNullOrWhiteSpace(entry.ViewUrl))
            {
                Application.OpenURL(entry.ViewUrl);
            }
            else if (!string.IsNullOrWhiteSpace(entry.RawUrl))
            {
                Application.OpenURL(entry.RawUrl);
            }
        }

        private void InsertEntry(ScriptEntry entry)
        {
            string destinationFolder = EditorUtility.OpenFolderPanel("Select destination folder inside this Unity project", Application.dataPath, "");

            if (string.IsNullOrWhiteSpace(destinationFolder))
            {
                return;
            }

            if (!destinationFolder.StartsWith(Application.dataPath, StringComparison.OrdinalIgnoreCase))
            {
                EditorUtility.DisplayDialog("Invalid Folder", "Destination must be inside this project's Assets folder.", "OK");
                return;
            }

            bool installDependencies = true;

            if (entry.Dependencies.Count > 0)
            {
                installDependencies = EditorUtility.DisplayDialog("Dependencies Found", "This script has dependencies:\n\n" + string.Join("\n", entry.Dependencies) + "\n\nInstall dependencies too?", "Yes", "No");
            }

            try
            {
                WriteEntryToProject(entry, destinationFolder);
                InstallPackages(entry.Packages);

                if (installDependencies)
                {
                    foreach (string dependencyName in entry.Dependencies)
                    {
                        ScriptEntry dependency = FindDependencyByName(dependencyName);

                        if (dependency != null)
                        {
                            WriteEntryToProject(dependency, destinationFolder);
                            InstallPackages(dependency.Packages);
                        }
                        else
                        {
                            Debug.LogWarning("[Scripts] Dependency not found: " + dependencyName);
                        }
                    }
                }

                AssetDatabase.Refresh();

                string relativeFolder = "Assets" + destinationFolder.Substring(Application.dataPath.Length);
                EditorUtility.DisplayDialog("Insert Complete", "Script copied to:\n" + relativeFolder, "OK");
            }
            catch (Exception ex)
            {
                EditorUtility.DisplayDialog("Insert Failed", ex.Message, "OK");
            }
        }

        private void WriteEntryToProject(ScriptEntry entry, string destinationFolder)
        {
            Directory.CreateDirectory(destinationFolder);

            string fileName = entry.Name + ".cs";
            string destinationPath = Path.Combine(destinationFolder, fileName);

            if (File.Exists(destinationPath))
            {
                bool overwrite = EditorUtility.DisplayDialog("File Already Exists", fileName + " already exists.\nOverwrite it?", "Overwrite", "Skip");

                if (!overwrite)
                {
                    return;
                }
            }

            if (entry.Source == ScriptSource.Local)
            {
                if (!File.Exists(entry.FilePath))
                {
                    throw new FileNotFoundException("Local source file not found.", entry.FilePath);
                }

                File.Copy(entry.FilePath, destinationPath, true);
                return;
            }

            string text = !string.IsNullOrWhiteSpace(entry.CachedText) ? entry.CachedText : DownloadText(entry.RawUrl);

            if (string.IsNullOrWhiteSpace(text))
            {
                throw new Exception("Downloaded script was empty: " + entry.Name);
            }

            File.WriteAllText(destinationPath, text, new UTF8Encoding(false));
        }

        private void InstallPackages(List<string> packages)
        {
            if (packages == null || packages.Count == 0)
            {
                return;
            }

            HashSet<string> installed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (string rawPackage in packages)
            {
                string packageId = ExtractInstallablePackageId(rawPackage);

                if (string.IsNullOrWhiteSpace(packageId))
                {
                    continue;
                }

                if (!installed.Add(packageId))
                {
                    continue;
                }

                try
                {
                    Client.Add(packageId);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning("[Scripts] Package install failed '" + packageId + "': " + ex.Message);
                }
            }
        }

        private string ExtractInstallablePackageId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            string trimmed = value.Trim().Trim('"');

            if (trimmed.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || trimmed.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return string.Empty;
            }

            return trimmed;
        }

        private ScriptEntry FindDependencyByName(string dependencyName)
        {
            if (string.IsNullOrWhiteSpace(dependencyName))
            {
                return null;
            }

            string normalized = dependencyName.Trim();

            foreach (ScriptEntry item in results)
            {
                if (string.Equals(item.Name, normalized, StringComparison.OrdinalIgnoreCase))
                {
                    return item;
                }
            }

            try
            {
                List<ScriptEntry> localOnly = FindLocalScripts(normalized);
                ScriptEntry exactLocal = localOnly.FirstOrDefault(x => string.Equals(x.Name, normalized, StringComparison.OrdinalIgnoreCase));

                if (exactLocal != null)
                {
                    return exactLocal;
                }
            }
            catch
            {
            }

            if (EnableRemoteRepo)
            {
                try
                {
                    List<ScriptEntry> remoteOnly = FindRemoteScripts(normalized);
                    ScriptEntry exactRemote = remoteOnly.FirstOrDefault(x => string.Equals(x.Name, normalized, StringComparison.OrdinalIgnoreCase));

                    if (exactRemote != null)
                    {
                        return exactRemote;
                    }
                }
                catch
                {
                }
            }

            return null;
        }

        private List<GitHubContentItem> GetRemoteCsFilesRecursive(string folder)
        {
            List<GitHubContentItem> files = new List<GitHubContentItem>();
            FetchFolderRecursive(folder, files);
            return files;
        }

        private void FetchFolderRecursive(string folder, List<GitHubContentItem> files)
        {
            string apiUrl = BuildGitHubContentsApiUrl(folder);
            string json = DownloadText(apiUrl);

            if (string.IsNullOrWhiteSpace(json))
            {
                return;
            }

            if (!json.TrimStart().StartsWith("["))
            {
                return;
            }

            string wrapped = "{\"items\":" + json + "}";
            GitHubContentsResponse response = JsonUtility.FromJson<GitHubContentsResponse>(wrapped);

            if (response == null || response.items == null)
            {
                return;
            }

            foreach (GitHubContentItem item in response.items)
            {
                if (item == null)
                {
                    continue;
                }

                if (string.Equals(item.type, "file", StringComparison.OrdinalIgnoreCase) && item.name.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
                {
                    files.Add(item);
                }
                else if (string.Equals(item.type, "dir", StringComparison.OrdinalIgnoreCase))
                {
                    FetchFolderRecursive(item.path, files);
                }
            }
        }

        private string BuildGitHubContentsApiUrl(string folder)
        {
            string clean = string.IsNullOrWhiteSpace(folder) ? "" : "/" + folder.Trim('/');
            return "https://api.github.com/repos/" + GitHubOwner + "/" + GitHubRepo + "/contents" + clean + "?ref=" + GitHubBranch;
        }

        private string BuildGitHubBlobUrl(string repoPath)
        {
            return "https://github.com/" + GitHubOwner + "/" + GitHubRepo + "/blob/" + GitHubBranch + "/" + repoPath;
        }

        private string DownloadText(string url)
        {
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.timeout = RequestTimeout;
                request.SetRequestHeader("User-Agent", "UnityEditor-CrossingLears");

                UnityWebRequestAsyncOperation operation = request.SendWebRequest();

                while (!operation.isDone)
                {
                }

#if UNITY_2020_2_OR_NEWER
                if (request.result != UnityWebRequest.Result.Success)
                {
                    throw new Exception(request.error);
                }
#else
                if (request.isHttpError || request.isNetworkError)
                {
                    throw new Exception(request.error);
                }
#endif

                return request.downloadHandler.text;
            }
        }

        private ScriptEntry ParseMetadata(string text)
        {
            ScriptEntry entry = new ScriptEntry();
            entry.Name = string.Empty;
            entry.Description = string.Empty;
            entry.Tags = new List<string>();
            entry.Dependencies = new List<string>();
            entry.Packages = new List<string>();

            int start = text.IndexOf("/*", StringComparison.Ordinal);
            int end = text.IndexOf("*/", StringComparison.Ordinal);

            if (start < 0 || end <= start)
            {
                return entry;
            }

            string header = text.Substring(start + 2, end - start - 2);
            string[] lines = header.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string rawLine in lines)
            {
                string line = rawLine.Trim();

                if (line.StartsWith("Name:", StringComparison.OrdinalIgnoreCase))
                {
                    entry.Name = line.Substring("Name:".Length).Trim();
                }
                else if (line.StartsWith("Description:", StringComparison.OrdinalIgnoreCase))
                {
                    entry.Description = line.Substring("Description:".Length).Trim();
                }
                else if (line.StartsWith("Tags:", StringComparison.OrdinalIgnoreCase))
                {
                    entry.Tags = SplitCsv(line.Substring("Tags:".Length));
                }
                else if (line.StartsWith("Dependencies:", StringComparison.OrdinalIgnoreCase))
                {
                    entry.Dependencies = SplitCsv(line.Substring("Dependencies:".Length));
                }
                else if (line.StartsWith("Packages:", StringComparison.OrdinalIgnoreCase))
                {
                    entry.Packages = SplitCsv(line.Substring("Packages:".Length));
                }
            }

            return entry;
        }

        private List<string> SplitCsv(string value)
        {
            return (value ?? string.Empty).Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }

        private string Normalize(string value)
        {
            return (value ?? string.Empty).Trim().ToLowerInvariant();
        }

        private string SanitizeClassName(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }

            StringBuilder sb = new StringBuilder();

            foreach (char c in input.Trim())
            {
                if (char.IsLetterOrDigit(c) || c == '_')
                {
                    sb.Append(c);
                }
            }

            string result = sb.ToString();

            if (string.IsNullOrWhiteSpace(result))
            {
                return string.Empty;
            }

            if (char.IsDigit(result[0]))
            {
                result = "_" + result;
            }

            return result;
        }

        private void EnsureLocalFolderExists()
        {
            if (!Directory.Exists(LocalScriptsRoot))
            {
                Directory.CreateDirectory(LocalScriptsRoot);
            }
        }

        private enum ScriptSource
        {
            Local,
            Remote
        }

        [Serializable]
        private class ScriptEntry
        {
            public string Name;
            public string Description;
            public List<string> Tags = new List<string>();
            public List<string> Dependencies = new List<string>();
            public List<string> Packages = new List<string>();
            public ScriptSource Source;
            public string SourceLabel;
            public string FilePath;
            public string RepoPath;
            public string RawUrl;
            public string ViewUrl;
            public string CachedText;
            public string UniqueId;
        }

        [Serializable]
        private class GitHubContentItem
        {
            public string name;
            public string path;
            public string type;
            public string download_url;
        }

        [Serializable]
        private class GitHubContentsResponse
        {
            public GitHubContentItem[] items;
        }

        private class ScriptCreateWindow : EditorWindow
        {
            private ScriptsTab owner;
            private string scriptName = "";
            private string description = "";
            private string tags = "";
            private string dependencies = "";
            private string packages = "";

            public static void Open(ScriptsTab owner)
            {
                ScriptCreateWindow window = CreateInstance<ScriptCreateWindow>();
                window.owner = owner;
                window.titleContent = new GUIContent("Create Script");
                window.minSize = new Vector2(420f, 210f);
                window.maxSize = new Vector2(420f, 210f);
                window.ShowUtility();
            }

            private void OnGUI()
            {
                GUILayout.Space(8f);

                EditorGUILayout.LabelField("Create Shared Script", EditorStyles.boldLabel);
                GUILayout.Space(8f);

                scriptName = EditorGUILayout.TextField("Name", scriptName);
                description = EditorGUILayout.TextField("Description", description);
                tags = EditorGUILayout.TextField("Tags (csv)", tags);
                dependencies = EditorGUILayout.TextField("Dependencies (csv)", dependencies);
                packages = EditorGUILayout.TextField("Packages (csv)", packages);

                GUILayout.FlexibleSpace();
                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button("Cancel", GUILayout.Height(26f)))
                {
                    Close();
                }

                if (GUILayout.Button("Create", GUILayout.Height(26f)))
                {
                    owner.CreateScriptFile(scriptName, description, tags, dependencies, packages);
                    Close();
                }

                EditorGUILayout.EndHorizontal();
                GUILayout.Space(8f);
            }
        }
    }
}