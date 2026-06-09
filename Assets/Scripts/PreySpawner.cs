using UnityEngine;

public class PreySpawner : MonoBehaviour
{
    public enum PreyType 
    { 
        Fly,               
        FlyPoised,         
        Frog,             
        Crab,              
        ToxicSpider       
    }

    [Header("Spawner Einstellungen")]
    [Tooltip("Welche Art von Beute soll an dieser Position spawnen?")]
    public PreyType beuteArt;

    [Tooltip("Zieh hier das passende Prefab aus dem Projektordner rein")]
    public GameObject preyPrefab;

    [Tooltip("Soll die Beute sofort bei Spielstart spawnen oder verzögert?")]
    public bool sofortSpawnen = false;

    [Tooltip("Falls verzögert: Wie viele Sekunden bis zum Spawn?")]
    public float spawnVerzoegerung = 2f;

    [Header("Erweiterte Spiel-Logik (Tag/Nacht)")]
    [Tooltip("Soll diese Beute NUR nachts spawnen?")]
    public bool nurNachtsSpawnen = false;

    void Start()
    {
        if (sofortSpawnen)
        {
            SpawnPrey();
        }
        else
        {
            Invoke("SpawnPrey", spawnVerzoegerung);
        }
    }

    void SpawnPrey()
    {
        if (nurNachtsSpawnen)
        {
            Debug.Log($"[Evrans Spawner]: {beuteArt} wartet auf die Nachtphase...");
            return; 
        }

        if (preyPrefab != null)
        {
            GameObject neueBeute = Instantiate(preyPrefab, transform.position, transform.rotation);
            neueBeute.name = beuteArt.ToString() + "_Spawned";
            
            // Logik-Check für giftige Beutetiere (FlyPoised und ToxicSpider)
            if (beuteArt == PreyType.FlyPoised || beuteArt == PreyType.ToxicSpider)
            {
                Debug.LogWarning($"[Evrans Spawner]: WARNUNG! Eine giftige {beuteArt} wurde gespawnt!");
            }
            else
            {
                Debug.Log($"[Evrans Spawner]: {beuteArt} erfolgreich in die Welt gesetzt!");
            }
        }
        else
        {
            Debug.LogError($"[Evrans Spawner]: Fehler! Kein Prefab für {beuteArt} im Inspector zugewiesen!");
        }
    }
}