using CrossingLears;
using UnityEditor;
using UnityEngine;
using System;

public class VersioningCommands
{
    private static DateTime lastRunTime = DateTime.MinValue;
    private const int cooldownSeconds = 10;

    public static void PatchFix()
    {
        if (IsOnCooldown()) return;
        PlayerSettings.Android.bundleVersionCode++;
        UpdateVersion(2); // patch
    }

    public static void MinorFix()
    {
        if (IsOnCooldown()) return;
        PlayerSettings.Android.bundleVersionCode++;
        UpdateVersion(1); // minor
    }

    private static bool IsOnCooldown()
    {
        if ((DateTime.Now - lastRunTime).TotalSeconds < cooldownSeconds)
        {
            Debug.LogWarning("Command is on cooldown. Please wait.");
            return true;
        }
        lastRunTime = DateTime.Now;
        return false;
    }

    private static void UpdateVersion(int index)
    {
        string[] parts = PlayerSettings.bundleVersion.Split('.');
        int[] version = new int[] { 0, 0, 0 };
        for (int i = 0; i < parts.Length && i < 3; i++)
            int.TryParse(parts[i], out version[i]);

        version[index]++;
        for (int i = index + 1; i < 3; i++)
            version[i] = 0;

        PlayerSettings.bundleVersion = $"{version[0]}.{version[1]}.{version[2]}";
        Debug.Log($"New bundleVersion: {PlayerSettings.bundleVersion} ({PlayerSettings.Android.bundleVersionCode})");
    }
}