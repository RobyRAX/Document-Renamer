using System.Diagnostics;
using System.IO;
using UnityEngine;

using Debug = UnityEngine.Debug;

public class LibreBridge : MonoBehaviour
{
    public static LibreBridge Instance { get; set; }

    [Header("Path to LibreOffice")]
    public string libreOfficePath = @"C:\Program Files\LibreOffice\program\soffice.exe";

    void Awake()
    {
        Instance = this;
    }

    public void SetLibreOfficePath(string path)
    {
        libreOfficePath = path;
    }

    public string ConvertDocToDocx(string docPath, string outputFolder = null)
    {
        if (!File.Exists(docPath))
        {
            Debug.LogError("Input file not found: " + docPath);
            return null;
        }

        if (!File.Exists(libreOfficePath))
        {
            Debug.LogError("LibreOffice executable not found: " + libreOfficePath);
            return null;
        }

        if (string.IsNullOrEmpty(outputFolder))
            outputFolder = Path.GetDirectoryName(docPath);

        Process p = new Process();
        p.StartInfo.FileName = libreOfficePath;
        p.StartInfo.WorkingDirectory = Path.GetDirectoryName(libreOfficePath);
        p.StartInfo.Arguments =
            $"--headless --nologo --nodefault --nolockcheck --convert-to docx \"{docPath}\" --outdir \"{outputFolder}\"";
        p.StartInfo.CreateNoWindow = true;
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.RedirectStandardOutput = true;
        p.StartInfo.RedirectStandardError = true;

        p.Start();
        string stdout = p.StandardOutput.ReadToEnd();
        string stderr = p.StandardError.ReadToEnd();
        p.WaitForExit();

        string fileName = Path.GetFileNameWithoutExtension(docPath) + ".docx";
        string outputPath = Path.Combine(outputFolder, fileName);

        if (File.Exists(outputPath))
        {
            Debug.Log($"Converted via LibreOffice: {docPath} → {outputPath}");
            return outputPath;
        }
        else
        {
            Debug.LogError("Conversion failed for: " + docPath + "\nLibreOffice stderr: " + stderr);
            return null;
        }
    }

    public string ConvertToPdf(string inputPath, string outputFolder = null)
    {
        if (!File.Exists(inputPath))
        {
            Debug.LogError("Input file not found: " + inputPath);
            return null;
        }

        if (!File.Exists(libreOfficePath))
        {
            Debug.LogError("LibreOffice executable not found: " + libreOfficePath);
            return null;
        }

        if (string.IsNullOrEmpty(outputFolder))
            outputFolder = Path.GetDirectoryName(inputPath);

        Process p = new Process();
        p.StartInfo.FileName = libreOfficePath;
        p.StartInfo.WorkingDirectory = Path.GetDirectoryName(libreOfficePath);
        p.StartInfo.Arguments =
            $"--headless --nologo --nodefault --nolockcheck --convert-to pdf \"{inputPath}\" --outdir \"{outputFolder}\"";
        p.StartInfo.CreateNoWindow = true;
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.RedirectStandardOutput = true;
        p.StartInfo.RedirectStandardError = true;

        p.Start();
        string stdout = p.StandardOutput.ReadToEnd();
        string stderr = p.StandardError.ReadToEnd();
        p.WaitForExit();

        string fileName = Path.GetFileNameWithoutExtension(inputPath) + ".pdf";
        string outputPath = Path.Combine(outputFolder, fileName);

        if (File.Exists(outputPath))
        {
            Debug.Log($"Converted to PDF via LibreOffice: {inputPath} → {outputPath}");
            return outputPath;
        }
        else
        {
            Debug.LogError("PDF conversion failed for: " + inputPath + "\nLibreOffice stderr: " + stderr);
            return null;
        }
    }

}
