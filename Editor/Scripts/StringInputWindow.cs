using UnityEditor;
using UnityEngine;

public class StringInputWindow : EditorWindow
{
    private static string titleText;
    private static string messageText;
    private static string defaultValue;
    private static System.Action<string> onConfirm;

    private string inputValue;

    public static void Open(string title, string message, string defaultString, System.Action<string> callback)
    {
        var window = GetWindow<StringInputWindow>(true, title, true);
        titleText = title;
        messageText = message;
        defaultValue = defaultString;
        onConfirm = callback;
        window.inputValue = defaultString;
        window.position = new Rect(Screen.width / 2, Screen.height / 2, 350, 120);
        window.ShowUtility();
    }

    private void OnGUI()
    {
        GUILayout.Label(messageText, EditorStyles.wordWrappedLabel);
        GUILayout.Space(5);

        GUI.SetNextControlName("InputField");
        inputValue = EditorGUILayout.TextField(inputValue);

        GUILayout.FlexibleSpace();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Cancel"))
            Close();

        if (GUILayout.Button("OK"))
        {
            onConfirm?.Invoke(inputValue);
            Close();
        }
        GUILayout.EndHorizontal();

        if (Event.current.type == EventType.Repaint)
            EditorGUI.FocusTextInControl("InputField");
    }
}
