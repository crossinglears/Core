using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using System.Collections.Generic;
using System.Linq;

namespace CrossingLearsEditor
{
    public class ScriptingTab : CL_WindowTab
    {
        public override string TabName => "Scripting";

        private List<string> ExtraScriptingDefineSymbols = new();
        private string newSymbol = "";

        public override void DrawContent()
        {
            GUILayout.Label("Scripting Define Symbols", EditorStyles.boldLabel);

            // Define the NamedBuildTarget for Standalone platforms (you can change this to other platforms as needed)
            NamedBuildTarget buildTarget = NamedBuildTarget.Standalone;

            // Get existing scripting define symbols for the selected build target
            string defines = PlayerSettings.GetScriptingDefineSymbols(buildTarget);
            List<string> existingSymbols = defines.Split(';').ToList();

            // Ensure ExtraScriptingDefineSymbols reflects the current symbols
            foreach (var symbol in existingSymbols)
            {
                if (!ExtraScriptingDefineSymbols.Contains(symbol))
                {
                    ExtraScriptingDefineSymbols.Add(symbol);
                }
            }

            // Display each symbol with a checkbox
            for (int i = 0; i < ExtraScriptingDefineSymbols.Count; i++)
            {
                bool enabled = existingSymbols.Contains(ExtraScriptingDefineSymbols[i]);
                bool newState = GUILayout.Toggle(enabled, ExtraScriptingDefineSymbols[i]);

                // Update symbol inclusion based on the checkbox state
                if (newState && !enabled)
                {
                    existingSymbols.Add(ExtraScriptingDefineSymbols[i]);
                }
                else if (!newState && enabled)
                {
                    existingSymbols.Remove(ExtraScriptingDefineSymbols[i]);
                }
            }

            // Add new symbol input
            GUILayout.BeginHorizontal();
            newSymbol = GUILayout.TextField(newSymbol);
            if (GUILayout.Button("Add") && !string.IsNullOrWhiteSpace(newSymbol))
            {
                if (!ExtraScriptingDefineSymbols.Contains(newSymbol))
                {
                    ExtraScriptingDefineSymbols.Add(newSymbol);
                    existingSymbols.Add(newSymbol); // Add the new symbol to the list for the first time
                }
                newSymbol = "";
            }
            GUILayout.EndHorizontal();

            // Save and apply changes to the scripting define symbols
            if (GUILayout.Button("Save and Reload Scripts"))
            {
                // Update the define symbols for the build target
                string updatedDefines = string.Join(";", existingSymbols);
                PlayerSettings.SetScriptingDefineSymbols(buildTarget, updatedDefines);

                // Refresh scripts to apply changes
                AssetDatabase.Refresh(); 
            }
        }
    }
}
