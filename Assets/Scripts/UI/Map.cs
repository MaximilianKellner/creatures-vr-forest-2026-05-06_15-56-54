using UnityEngine;

public class Map : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private GameObject mapContainer; // Ziehe hier dein Map-Objekt rein
    
    private bool isMap = false;

    private void Start()
    {
        XRVisualRuntimeAdapter.EnsureSceneVisuals();

        // Map am Anfang direkt verstecken
        if (mapContainer != null)
        {
            mapContainer.SetActive(false);
        }
    }

    // Diese Methode wird später von deinem Zungen-Skript EINMAL aufgerufen
    public void ShowMap()
    {
        if (!isMap && mapContainer != null)
        {
            mapContainer.SetActive(true);
            isMap = true;
            Debug.Log("Map wurde freigeschaltet!");
        }
    }

    public void HideMap()
    {
        if (mapContainer == null)
            return;

        mapContainer.SetActive(false);
        isMap = false;
    }

    public void ToggleMap()
    {
        if (mapContainer == null)
            return;

        isMap = !isMap;
        mapContainer.SetActive(isMap);
    }
}

