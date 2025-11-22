using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public static class GlobalUtility
{
    public static void OpenFile(string filePath)
    {
        if (System.IO.File.Exists(filePath))
        {
            Process.Start(new ProcessStartInfo(filePath)
            {
                UseShellExecute = true  // penting supaya pakai default app
            });
        }
        else
        {
            Debug.LogError("File not found: " + filePath);
        }
    }
}
