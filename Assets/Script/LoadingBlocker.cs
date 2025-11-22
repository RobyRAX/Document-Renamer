using UnityEngine;

public class LoadingBlocker : MonoBehaviour
{
    public static LoadingBlocker Instance { get; set; }

    void Awake()
    {
        Instance = this;
        Close();
    }

    public void Open()
    {
        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}
