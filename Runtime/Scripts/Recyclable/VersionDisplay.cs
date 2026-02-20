using TMPro;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Displays a version string based on Semantic Versioning (SemVer).
/// 
/// This implementation assumes PlayerSettings.bundleVersion follows:
///     MAJOR.MINOR.PATCH
/// 
/// Example:
///     1.4.2
/// 
/// It parses those three numeric components and injects them into a
/// configurable format string. Any deviation from SemVer (missing parts,
/// extra identifiers like -beta, metadata, etc.) will not be interpreted
/// correctly — only the first three dot-separated numeric segments are used.
/// 
/// Build number is platform-specific and appended separately.
/// </summary>
public class VersionDisplay : MonoBehaviour
{
    /// <summary>
    /// Output format template.
    /// Supported tokens:
    ///     Major  -> SemVer major component
    ///     Minor  -> SemVer minor component
    ///     Patch  -> SemVer patch component
    ///     Build  -> Platform build identifier
    /// 
    /// Default example result:
    ///     v1.2.3 (45)
    /// </summary>
    public string DefaultFormat = "vMajor.Minor.Patch (Build)";

    /// <summary>
    /// Generates formatted version text.
    /// 
    /// Only supports strict semantic version strings.
    /// </summary>
    public string GetVersion(string format)
    {
        string version;
#if UNITY_EDITOR
        version = PlayerSettings.bundleVersion;
#else
        version = Application.version;
#endif
        string[] parts = version.Split('.');

        string major = parts.Length > 0 ? parts[0] : "0";
        string minor = parts.Length > 1 ? parts[1] : "0";
        string patch = parts.Length > 2 ? parts[2] : "0";

        string build = "0";

#if UNITY_EDITOR && UNITY_ANDROID
        build = PlayerSettings.Android.bundleVersionCode.ToString();
#elif UNITY_EDITOR && UNITY_IOS
        build = PlayerSettings.iOS.buildNumber;
#else
        build = version;
#endif

        string result = format;
        result = result.Replace("Major", major);
        result = result.Replace("Minor", minor);
        result = result.Replace("Patch", patch);
        result = result.Replace("Build", build);

        return result;
    }

    void Start()
    {
        GetComponent<TMP_Text>().text = GetVersion(DefaultFormat);
    }
}