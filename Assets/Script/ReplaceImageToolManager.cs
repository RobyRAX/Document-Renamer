using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using SFB;
using TMPro;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class ReplaceImageToolManager : ToolManagerBase
{
    public TMP_InputField fromTxtInput;
    public TMP_InputField toTxtInput;
    
    const string EXE_NAME = "replace_image_tool.exe";

    string referenceImagePath;
    string newImagePath;

    public void OnClick_SelectReferenceImage()
    {
        var extensions = new[] { new ExtensionFilter("Image Files", "png", "jpg", "jpeg") };
        var paths = StandaloneFileBrowser.OpenFilePanel("Select Reference Image", "", extensions, false);

        if (paths.Length > 0)
        {
            referenceImagePath = paths[0];
            fromTxtInput.text = System.IO.Path.GetFileName(referenceImagePath);
        }
    }

    // Select new image
    public void OnClick_SelectNewImage()
    {
        var extensions = new[] { new ExtensionFilter("Image Files", "png", "jpg", "jpeg") };
        var paths = StandaloneFileBrowser.OpenFilePanel("Select New Image", "", extensions, false);

        if (paths.Length > 0)
        {
            newImagePath = paths[0];
            toTxtInput.text = System.IO.Path.GetFileName(newImagePath);
        }
    }

    public void OnClick_Replace()
    {
        _ = ReplaceAsync();
    }

    async Task ReplaceAsync()
    {
        if (LoadingBlocker.Instance != null)
        {
            LoadingBlocker.Instance.Open();
        }

        await Task.Delay(100);

        if (files == null || spawnedFileUI == null)
        {
            Debug.LogWarning("One of the required references is null.");
            if (LoadingBlocker.Instance != null)
            {
                LoadingBlocker.Instance.Close();
            }
            return;
        }

        string oldText = referenceImagePath ?? string.Empty;
        string newText = newImagePath ?? string.Empty;

        for (int i = 0; i < files.Count; i++)
        {
            if (string.IsNullOrEmpty(files[i]))
                continue;

            ReplaceImage(files[i], oldText, newText);

            int count = GetImageCount(files[i], oldText); // opsional, bisa jadi 0 setelah replace

            if (i < spawnedFileUI.Count && spawnedFileUI[i] != null)
            {
                spawnedFileUI[i].SetCount(count);
            }

            Debug.Log($"[{files[i]}] Replaced '{oldText}' → '{newText}', remaining count: {count}");
        }

        await Task.Delay(100);

        if (LoadingBlocker.Instance != null)
        {
            LoadingBlocker.Instance.Close();
        }
    }

    public void OnClick_Find()
    {
        _ = FindAsync();
    }

    async Task FindAsync()
    {
        if (LoadingBlocker.Instance != null)
        {
            LoadingBlocker.Instance.Open();
        }

        await Task.Delay(100);

        if (files == null || spawnedFileUI == null)
        {
            Debug.LogWarning("One of the required references is null.");
            if (LoadingBlocker.Instance != null)
            {
                LoadingBlocker.Instance.Close();
            }
            return;
        }

        for (int i = 0; i < files.Count; i++)
        {
            if (string.IsNullOrEmpty(files[i]))
                continue;

            int count = GetImageCount(files[i], referenceImagePath ?? string.Empty);

            // Update UI
            if (i < spawnedFileUI.Count && spawnedFileUI[i] != null)
            {
                spawnedFileUI[i].SetCount(count);
            }

            Debug.Log($"[{files[i]}] FOUND: {count}");
        }

        await Task.Delay(100);

        if (LoadingBlocker.Instance != null)
        {
            LoadingBlocker.Instance.Close();
        }
    }

    public void ReplaceImage(string docxPath, string referenceImagePath, string newImagePath)
    {
        string exePath = Path.Combine(Application.streamingAssetsPath, "replace_image_tool.exe");

        if (!File.Exists(exePath))
        {
            Debug.LogError("Python EXE not found: " + exePath);
            return;
        }

        Process p = new Process();
        p.StartInfo.FileName = exePath;
        p.StartInfo.Arguments = $"replace \"{docxPath}\" \"{referenceImagePath}\" \"{newImagePath}\"";
        p.StartInfo.CreateNoWindow = true;
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.RedirectStandardError = true;

        p.Start();

        string error = p.StandardError.ReadToEnd();
        p.WaitForExit();

        if (!string.IsNullOrEmpty(error))
        {
            Debug.LogError("Python error: " + error);
        }
        else
        {
            Debug.Log($"Replaced images in file: {docxPath}");
        }
    }

    public int GetImageCount(string docxPath, string referenceImagePath)
    {
        string exePath = Path.Combine(Application.streamingAssetsPath, EXE_NAME);

        if (!File.Exists(exePath))
        {
            Debug.LogError("Python EXE not found: " + exePath);
            return -1;
        }

        Process p = new Process();
        p.StartInfo.FileName = exePath;

        // ⚡ Tambahkan mode "find"
        p.StartInfo.Arguments = $"find \"{docxPath}\" \"{referenceImagePath}\"";

        p.StartInfo.CreateNoWindow = true;
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.RedirectStandardOutput = true;

        p.Start();

        string output = p.StandardOutput.ReadToEnd();
        p.WaitForExit();

        output = output.Trim();

        if (int.TryParse(output, out int count))
        {
            return count;
        }
        else
        {
            Debug.LogError("Python output invalid: " + output);
            return -1;
        }
    }
}