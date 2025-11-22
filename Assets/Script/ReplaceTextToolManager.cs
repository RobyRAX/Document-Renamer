using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using SFB;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

using Debug = UnityEngine.Debug;

public class ReplaceTextToolManager : ToolManagerBase
{
    public TMP_InputField fromTxtInput;
    public TMP_InputField toTxtInput;

    const string EXE_NAME = "replace_text_tool.exe";

    public int GetTextCount(string inputFilePath, string target)
    {
        string exePath = Path.Combine(Application.streamingAssetsPath, EXE_NAME);

        if (!File.Exists(exePath))
        {
            Debug.LogError("Python EXE not found: " + exePath);
            return -1;
        }

        Process p = new Process();
        p.StartInfo.FileName = exePath;
        p.StartInfo.Arguments = $"find \"{inputFilePath}\" \"{target}\"";
        p.StartInfo.CreateNoWindow = true;
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.RedirectStandardOutput = true;
        p.StartInfo.RedirectStandardError = true;

        p.Start();

        string output = p.StandardOutput.ReadToEnd();
        string error = p.StandardError.ReadToEnd();

        p.WaitForExit();

        if (!string.IsNullOrEmpty(error))
            Debug.LogError("Python error: " + error);

        output = output.Trim();

        if (int.TryParse(output, out int count))
        {
            return count;
        }

        Debug.LogError("Invalid output: " + output);
        return -1;
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

        if (files == null || spawnedFileUI == null || fromTxtInput == null)
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

            int count = GetTextCount(files[i], fromTxtInput.text ?? string.Empty);

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

    // public void ReplaceText(string inputFilePath, string oldText, string newText)
    // {
    //     string exePath = Path.Combine(Application.streamingAssetsPath, EXE_NAME);

    //     if (!File.Exists(exePath))
    //     {
    //         Debug.LogError("Python EXE not found: " + exePath);
    //         return;
    //     }

    //     string args = $"replace \"{inputFilePath}\" \"{oldText}\" \"{newText}\"";
    //     // DEBUG LOG
    //     Debug.Log($"[ReplaceText CMD] {exePath} {args}");

    //     Process p = new Process();
    //     p.StartInfo.FileName = exePath;
    //     p.StartInfo.Arguments = args;
    //     p.StartInfo.CreateNoWindow = true;
    //     p.StartInfo.UseShellExecute = false;
    //     p.StartInfo.RedirectStandardError = true;
    //     p.StartInfo.RedirectStandardOutput = true;

    //     p.Start();

    //     string error = p.StandardError.ReadToEnd();
    //     p.WaitForExit();

    //     if (!string.IsNullOrEmpty(error))
    //     {
    //         Debug.LogError("Python error: " + error);
    //     }
    //     else
    //     {
    //         Debug.Log($"Replaced '{oldText}' → '{newText}' in file: {inputFilePath}");
    //     }
    // }

    public async Task<int> ReplaceTextAsync(string inputFilePath, string oldText, string newText)
    {
        string exePath = Path.Combine(Application.streamingAssetsPath, EXE_NAME);

        if (!File.Exists(exePath))
        {
            Debug.LogError("Python EXE not found: " + exePath);
            return -1;
        }

        string args = $"replace \"{inputFilePath}\" \"{oldText}\" \"{newText}\"";
        Debug.Log($"[ReplaceText CMD] {exePath} {args}");

        Process p = new Process();
        p.StartInfo.FileName = exePath;
        p.StartInfo.Arguments = args;
        p.StartInfo.CreateNoWindow = true;
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.RedirectStandardOutput = true;
        p.StartInfo.RedirectStandardError = true;

        p.Start();

        string output = await p.StandardOutput.ReadToEndAsync();
        string error = await p.StandardError.ReadToEndAsync();

        p.WaitForExit();

        if (!string.IsNullOrEmpty(error))
            Debug.LogError("Python error: " + error);

        output = output.Trim();

        if (output.StartsWith("REPLACED_COUNT="))
        {
            string num = output.Replace("REPLACED_COUNT=", "");
            if (int.TryParse(num, out int count))
                return count;
        }

        Debug.LogWarning("Invalid output: " + output);
        return -1;
    }

    public void OnClick_Replace()
    {
        _ = ReplaceAsync();
    }

    async Task ReplaceAsync()
    {
        LoadingBlocker.Instance?.Open();
        await Task.Delay(100);

        if (files == null)
            return;

        string oldText = fromTxtInput.text ?? "";
        string newText = toTxtInput.text ?? "";

        for (int i = 0; i < files.Count; i++)
        {
            string file = files[i];
            if (string.IsNullOrEmpty(file))
                continue;

            // DO THE REPLACE (await!)
            int replaced = await ReplaceTextAsync(file, oldText, newText);

            // COUNT AFTER REPLACE
            int remaining = GetTextCount(file, oldText);

            // Update UI
            spawnedFileUI[i]?.SetCount(remaining);

            Debug.Log($"[{file}] REPLACED: {replaced}, REMAINING: {remaining}");
        }

        await Task.Delay(100);
        LoadingBlocker.Instance?.Close();
    }

}
