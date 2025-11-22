using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using SFB;
using TMPro;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class ToolManagerBase : MonoBehaviour
{
    [Header("Prefab")]
    public SelectedFileUI selectedFileUiPrefab;

    [Header("UI Ref")]
    public Transform selectedFileParent;

    protected List<SelectedFileUI> spawnedFileUI;
    public static List<string> files;

    void OnEnable()
    {
        RefreshSelectedFiles();
    }

    async Task PickMultipleDocsAsync()
    {
        var extensions = new[]
        {
            new ExtensionFilter("Documents", "docx", "doc", "xlsx")
        };

        string[] selectedFiles = StandaloneFileBrowser.OpenFilePanel(
            "Select Files",
            "",
            extensions,
            true
        );

        if (selectedFiles.Length == 0)
        {
            Debug.Log("No file selected");
            return;
        }

        files = new List<string>();

        foreach (var f in selectedFiles)
        {
            string ext = Path.GetExtension(f).ToLower();
            if (ext == ".doc")
            {
                if (LoadingBlocker.Instance)
                {
                    LoadingBlocker.Instance.Open();
                }

                await Task.Delay(100);

                string converted = LibreBridge.Instance.ConvertDocToDocx(f);;
                if (!string.IsNullOrEmpty(converted))
                    files.Add(converted);
                
                await Task.Delay(100);

                if (LoadingBlocker.Instance)
                {
                    LoadingBlocker.Instance.Close();
                }
            }
            else
            {
                files.Add(f); // .docx langsung
            }
        }
    }

    public void OnClick_SelectFile()
    {
        _ = OnClick_SelectFile_Async();
    }

    async Task OnClick_SelectFile_Async()
    {
        await PickMultipleDocsAsync();

        RefreshSelectedFiles();
    }

    void RefreshSelectedFiles()
    {
        // Clear old UI
        foreach (Transform child in selectedFileParent)
        {
            Destroy(child.gameObject);
        }
        spawnedFileUI = new List<SelectedFileUI>();

        if (files == null)
            return;

        // Spawn new UI
        foreach (var file in files)
        {
            SelectedFileUI newClone = Instantiate(selectedFileUiPrefab, selectedFileParent);
            spawnedFileUI.Add(newClone);

            newClone.Setup(file);
        }
    }
}
