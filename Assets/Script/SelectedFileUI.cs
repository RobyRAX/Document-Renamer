using System;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class SelectedFileUI : MonoBehaviour, IPointerClickHandler
{
    public TextMeshProUGUI titleTmp;   // Nama file
    public TextMeshProUGUI countTmp;   // Text untuk jumlah ditemukan

    string path;

    // Double click detection
    private float lastClickTime;
    private const float doubleClickThreshold = 0.3f; // detik

    // Setup nama file
    public void Setup(string filePath)
    {
        path = filePath;

        string fileName = Path.GetFileName(filePath);
        titleTmp.text = fileName;
        countTmp.text = "";
    }

    // Update jumlah ditemukan
    public void SetCount(int count)
    {
        countTmp.text = count.ToString();
    }

    // Dipanggil saat pointer click
    public void OnPointerClick(PointerEventData eventData)
    {
        float timeSinceLastClick = Time.time - lastClickTime;

        if (timeSinceLastClick <= doubleClickThreshold)
        {
            // Double click terdeteksi
            OpenFile();
        }

        lastClickTime = Time.time;
    }

    private void OpenFile()
    {
        if (File.Exists(path))
        {
            GlobalUtility.OpenFile(path); // pake fungsi yang udah kamu punya
        }
        else
        {
            Debug.LogError("File not found: " + path);
        }
    }
}
