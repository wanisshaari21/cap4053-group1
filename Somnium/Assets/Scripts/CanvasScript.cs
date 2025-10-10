using UnityEngine;

public class CanvasScript : MonoBehaviour
{
    [SerializeField] private GameObject myCanvas; // Use GameObject, not Canvas

    void Start()
    {
        myCanvas.SetActive(true);
    }
}