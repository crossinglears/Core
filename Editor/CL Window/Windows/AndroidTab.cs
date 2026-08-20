using UnityEditor;
using UnityEngine;

namespace CrossingLears.Editor
{
    public class AndroidTab : CL_WindowTab
    {
        private string KEYSTORE_NAME_KEY => Application.productName + "_CL_Android_KeystoreName";
        private string KEYSTORE_PASS_KEY => Application.productName + "_CL_Android_KeystorePass";
        private string KEYALIAS_NAME_KEY => Application.productName + "_CL_Android_KeyAliasName";
        private string KEYALIAS_PASS_KEY => Application.productName + "_CL_Android_KeyAliasPass";
        private string USE_CUSTOM_KEYSTORE_KEY => Application.productName + "_CL_Android_UseCustomKeystore";

        private string keystoreName;
        private string keystorePass;
        private string keyAliasName;
        private string keyAliasPass;
        private bool useCustomKeystore;

        public override string TabName => "Android";

        public override void Awake()
        {
            base.Awake();

            keystoreName = EditorPrefs.GetString(KEYSTORE_NAME_KEY, PlayerSettings.Android.keystoreName);
            keystorePass = EditorPrefs.GetString(KEYSTORE_PASS_KEY, PlayerSettings.Android.keystorePass);
            keyAliasName = EditorPrefs.GetString(KEYALIAS_NAME_KEY, PlayerSettings.Android.keyaliasName);
            keyAliasPass = EditorPrefs.GetString(KEYALIAS_PASS_KEY, PlayerSettings.Android.keyaliasPass);
            useCustomKeystore = EditorPrefs.GetBool(USE_CUSTOM_KEYSTORE_KEY, PlayerSettings.Android.useCustomKeystore);

            ApplySettings();
        }

        public override void DrawContent()
        {
            EditorGUI.BeginChangeCheck();

            useCustomKeystore = EditorGUILayout.Toggle("Use Custom Keystore", useCustomKeystore);
            keystoreName = EditorGUILayout.TextField("Keystore", keystoreName);
            keystorePass = EditorGUILayout.PasswordField("Keystore Password", keystorePass);
            keyAliasName = EditorGUILayout.TextField("Key Alias", keyAliasName);
            keyAliasPass = EditorGUILayout.PasswordField("Key Alias Password", keyAliasPass);

            if (EditorGUI.EndChangeCheck())
            {
                EditorPrefs.SetBool(USE_CUSTOM_KEYSTORE_KEY, useCustomKeystore);
                EditorPrefs.SetString(KEYSTORE_NAME_KEY, keystoreName);
                EditorPrefs.SetString(KEYSTORE_PASS_KEY, keystorePass);
                EditorPrefs.SetString(KEYALIAS_NAME_KEY, keyAliasName);
                EditorPrefs.SetString(KEYALIAS_PASS_KEY, keyAliasPass);

                ApplySettings();
            }

            if(GUILayout.Button("Fill From Current Setting"))
            {
                useCustomKeystore = PlayerSettings.Android.useCustomKeystore;
                keystoreName = PlayerSettings.Android.keystoreName;
                keystorePass = PlayerSettings.Android.keystorePass;
                keyAliasName = PlayerSettings.Android.keyaliasName;
                keyAliasPass = PlayerSettings.Android.keyaliasPass;

                EditorPrefs.SetBool(USE_CUSTOM_KEYSTORE_KEY, useCustomKeystore);
                EditorPrefs.SetString(KEYSTORE_NAME_KEY, keystoreName);
                EditorPrefs.SetString(KEYSTORE_PASS_KEY, keystorePass);
                EditorPrefs.SetString(KEYALIAS_NAME_KEY, keyAliasName);
                EditorPrefs.SetString(KEYALIAS_PASS_KEY, keyAliasPass);
            }
        }

        private void ApplySettings()
        {
            PlayerSettings.Android.useCustomKeystore = useCustomKeystore;
            PlayerSettings.Android.keystoreName = keystoreName;
            PlayerSettings.Android.keystorePass = keystorePass;
            PlayerSettings.Android.keyaliasName = keyAliasName;
            PlayerSettings.Android.keyaliasPass = keyAliasPass;
        }
    }
}