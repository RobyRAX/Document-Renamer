using SFB;
using TMPro;
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    public TMP_InputField libreInputField;
    public TMP_InputField ghostscriptInputField;

    void Start()
    {
        libreInputField.text = LibreBridge.Instance.libreOfficePath;
        ghostscriptInputField.text = GhostscriptBridge.Instance.ghostscriptPath;
    }

    public void SetGhostscriptBridgePath(string path)
    {
        ghostscriptInputField.text = path;
        GhostscriptBridge.Instance.SetGhostscriptPath(path);
    }

    public void SetLibreBridgePath(string path)
    {
        libreInputField.text = path;
        LibreBridge.Instance.SetLibreOfficePath(path);
    }

    public void OnClick_FindLibre()
    {
        var extensions = new[] { new ExtensionFilter("Image Files", "png", "jpg", "jpeg") };
        var paths = StandaloneFileBrowser.OpenFilePanel("Select Executable", "", extensions, false);

        if (paths.Length > 0)
        {
            SetLibreBridgePath(paths[0]);
        }
    }

    public void OnClick_FindGhostscript()
    {
        var extensions = new[] { new ExtensionFilter("Image Files", "png", "jpg", "jpeg") };
        var paths = StandaloneFileBrowser.OpenFilePanel("Select Executable", "", extensions, false);

        if (paths.Length > 0)
        {
            SetGhostscriptBridgePath(paths[0]);
        }
    }
}
