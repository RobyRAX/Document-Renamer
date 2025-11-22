using System.Diagnostics;
using System.IO;
using UnityEngine;

using Debug = UnityEngine.Debug;

public class GhostscriptBridge : MonoBehaviour
{
    public static GhostscriptBridge Instance { get; set; }

    [Header("Path to Ghostscript Executable")]
    public string ghostscriptPath = @"C:\Program Files\gs\gs10.06.0\bin\gswin64.exe";

    void Awake()
    {
        Instance = this;
    }

    public void SetGhostscriptPath(string path)
    {
        ghostscriptPath = path;
    }

    /// <summary>
    /// Run any Ghostscript command and return stdout+stderr.
    /// </summary>
    public string RunGhostscript(string arguments)
    {
        if (!File.Exists(ghostscriptPath))
        {
            Debug.LogError("Ghostscript not found: " + ghostscriptPath);
            return null;
        }

        Process p = new Process();
        p.StartInfo.FileName = ghostscriptPath;
        p.StartInfo.Arguments = arguments;

        p.StartInfo.CreateNoWindow = true;
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.RedirectStandardOutput = true;
        p.StartInfo.RedirectStandardError = true;

        p.Start();

        string stdout = p.StandardOutput.ReadToEnd();
        string stderr = p.StandardError.ReadToEnd();
        p.WaitForExit();

        if (!string.IsNullOrEmpty(stderr))
        {
            Debug.LogWarning("Ghostscript stderr: " + stderr);
        }

        return stdout;
    }

    /// <summary>
    /// Merge multiple PDFs into 1 output PDF
    /// </summary>
    public string MergePdf(string[] inputFiles, string outputFile)
    {
        if (inputFiles == null || inputFiles.Length == 0)
        {
            Debug.LogError("No input files for merge.");
            return null;
        }

        string inputArgs = "";
        foreach (var pdf in inputFiles)
        {
            if (File.Exists(pdf))
                inputArgs += $" \"{pdf}\"";
            else
                Debug.LogWarning("PDF not found: " + pdf);
        }

        string args =
            $"-dBATCH -dNOPAUSE -q -sDEVICE=pdfwrite -sOutputFile=\"{outputFile}\" {inputArgs}";

        Debug.Log("Running Ghostscript merge: " + args);

        RunGhostscript(args);

        if (File.Exists(outputFile))
        {
            Debug.Log("PDF merged successfully → " + outputFile);
            return outputFile;
        }

        Debug.LogError("Merge failed.");
        return null;
    }
}
