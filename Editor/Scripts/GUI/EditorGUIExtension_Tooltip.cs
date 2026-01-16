using UnityEngine;
using UnityEditor;

public static partial class CL_GUILayout
{
    private static readonly GUIStyle HelpStyle = new GUIStyle(EditorStyles.label)
    {
        alignment = TextAnchor.MiddleCenter,
        fixedWidth = 20f,
        fixedHeight = 20f
    };

    public static void HelpMarker(string tooltip)
    {
        GUILayout.Label(new GUIContent("?", tooltip), HelpStyle);
    }
}
