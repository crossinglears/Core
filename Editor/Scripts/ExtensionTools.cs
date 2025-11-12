using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace CrossingLearsEditor
{
    public static class ExtensionTools
    {
        /// <summary>
        /// Attempts to parse a string into a <see cref="Vector3"/>.
        /// </summary>
        /// <param name="input">
        /// A string containing either a single float value (e.g. "3") 
        /// or three comma-separated float values (e.g. "1,2,7").
        /// </param>
        /// <param name="result">
        /// The resulting <see cref="Vector3"/> value if parsing succeeds; otherwise, <see cref="Vector3.zero"/>.
        /// </param>
        /// <returns>
        /// Returns <c>true</c> if parsing succeeds; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// - If the string contains one value, all three components (x, y, z) will use that same value.<br/>
        /// - If the string contains three comma-separated values, they are parsed individually into x, y, and z.<br/>
        /// </remarks>
        public static bool TryParseVector3(string input, out Vector3 result)
        {
            result = Vector3.zero;
            if (string.IsNullOrWhiteSpace(input)) return false;

            string[] parts = input.Split(',');
            if (parts.Length == 1 && float.TryParse(parts[0].Trim(), out float single))
            {
                result = new Vector3(single, single, single);
                return true;
            }
            else if (parts.Length == 3 &&
                    float.TryParse(parts[0].Trim(), out float x) &&
                    float.TryParse(parts[1].Trim(), out float y) &&
                    float.TryParse(parts[2].Trim(), out float z))
            {
                result = new Vector3(x, y, z);
                return true;
            }

            return false;
        }

        public static void SetProjectSearch(string searchText)
        {
            var projectBrowserType = typeof(Editor).Assembly.GetType("UnityEditor.ProjectBrowser");
            var windows = Resources.FindObjectsOfTypeAll(projectBrowserType);

            foreach (var win in windows)
            {
                var searchField = projectBrowserType.GetField("m_SearchFilter", BindingFlags.NonPublic | BindingFlags.Instance);
                var searchFilterType = typeof(Editor).Assembly.GetType("UnityEditor.SearchFilter");
                var searchFilter = searchField.GetValue(win);

                var searchTextField = searchFilterType.GetField("m_NameFilter", BindingFlags.NonPublic | BindingFlags.Instance);
                searchTextField.SetValue(searchFilter, searchText);

                // Refresh
                var refreshMethod = projectBrowserType.GetMethod("RefreshSearchIfNeeded", BindingFlags.NonPublic | BindingFlags.Instance);
                refreshMethod.Invoke(win, null);
            }
        }
    }


}
