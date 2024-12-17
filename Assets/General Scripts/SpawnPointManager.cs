using UnityEngine;

public class SpawnPointManager : MonoBehaviour
{
    private static SpawnPointManager instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Keep the spawn point across scenes
        }
        else
        {
            Destroy(gameObject); // Prevent duplicates
        }
    }
}
