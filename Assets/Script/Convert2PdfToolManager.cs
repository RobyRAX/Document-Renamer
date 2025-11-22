using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using SFB;
using TMPro;
using UnityEngine;

public class Convert2PdfToolManager : ToolManagerBase
{
    public Transform convertedFileParent;
    public Transform mergedFileParent;

    public TMP_InputField mergedFileFolder;
    public TMP_InputField mergedFileName;

    List<SelectedFileUI> convertedFileUI;
    List<string> converteds;

    SelectedFileUI mergedFileUI;
    string merged;

    public void OnClick_BrowseFolder()
    {
        var paths = StandaloneFileBrowser.OpenFolderPanel("Select Folder", "", false);

        if (paths.Length > 0)
        {
            mergedFileFolder.text = paths[0];
            Debug.Log("Selected Folder: " + paths[0]);
        }
        else
        {
            Debug.Log("No folder selected.");
        }
    }

    public void OnClick_FindPdfToMerge()
    {
        var extensions = new[]
        {
            new ExtensionFilter("Documents", "pdf")
        };

        string[] selectedFiles = StandaloneFileBrowser.OpenFilePanel(
            "Select Files",
            "",
            extensions,
            true
        );

        converteds = new List<string>();

        foreach (var f in selectedFiles)
        {
            converteds.Add(f); // .docx langsung
        }

        RefreshConverteds();
    }

    public void OnClick_Merge()
    {
        _ = MergeAsync();
    }

    async Task MergeAsync()
    {
        try
        {
            if (converteds == null || converteds.Count == 0)
            {
                Debug.LogError("No PDF files selected to merge!");
                return;
            }

            if (string.IsNullOrWhiteSpace(mergedFileFolder.text))
            {
                Debug.LogError("Output folder is empty!");
                return;
            }

            if (string.IsNullOrWhiteSpace(mergedFileName.text))
            {
                Debug.LogError("Output file name is empty!");
                return;
            }

            if (LoadingBlocker.Instance)
                LoadingBlocker.Instance.Open();

            await Task.Delay(100);

            string path = Path.Combine(mergedFileFolder.text, mergedFileName.text);

            if (Path.GetExtension(path).ToLower() != ".pdf")
                path += ".pdf";

            Debug.Log($"[Merge] Output path: {path}");

            merged = GhostscriptBridge.Instance.MergePdf(converteds.ToArray(), path);

            RefreshMerged();
        }
        catch (System.Exception ex)
        {
            Debug.LogError("[MergeAsync] ERROR: " + ex);
        }
        finally
        {
            await Task.Delay(100);

            if (LoadingBlocker.Instance)
                LoadingBlocker.Instance.Close();
        }
    }



    public void OnClick_Convert()
    {
        _ = ConvertAsync();
    }

    async Task ConvertAsync()
    {
        try
        {
            if (LoadingBlocker.Instance)
                LoadingBlocker.Instance.Open();

            await Task.Delay(100);

            if (files == null || files.Count == 0)
            {
                Debug.LogWarning("[Convert] No files to convert.");
                return;
            }

            converteds = new List<string>();

            foreach (var file in files)
            {
                try
                {
                    string converted = LibreBridge.Instance.ConvertToPdf(file);

                    if (!string.IsNullOrEmpty(converted))
                    {
                        converteds.Add(converted);
                        Debug.Log("[Convert] Success: " + converted);
                    }
                    else
                    {
                        Debug.LogError($"[Convert] Failed to convert: {file}");
                    }
                }
                catch (System.Exception exInner)
                {
                    Debug.LogError($"[Convert] Exception converting {file}: {exInner}");
                }
            }

            RefreshConverteds();
        }
        catch (System.Exception ex)
        {
            Debug.LogError("[ConvertAsync] ERROR: " + ex);
        }
        finally
        {
            await Task.Delay(100);

            if (LoadingBlocker.Instance)
                LoadingBlocker.Instance.Close();
        }
    }



    void RefreshConverteds()
    {
        // Clear old UI
        foreach (Transform child in convertedFileParent)
        {
            Destroy(child.gameObject);
        }
        convertedFileUI = new List<SelectedFileUI>();

        if (converteds == null)
            return;

        // Spawn new UI
        foreach (var file in converteds)
        {
            SelectedFileUI newClone = Instantiate(selectedFileUiPrefab, convertedFileParent);
            convertedFileUI.Add(newClone);

            newClone.Setup(file);
        }
    }

    void RefreshMerged()
    {
        // Clear old UI
        foreach (Transform child in mergedFileParent)
        {
            Destroy(child.gameObject);
        }
        mergedFileUI = null;

        if (merged == null)
            return;

        mergedFileUI = Instantiate(selectedFileUiPrefab, mergedFileParent);
        mergedFileUI.Setup(merged);
    }
}
