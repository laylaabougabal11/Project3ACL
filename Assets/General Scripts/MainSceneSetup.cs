using UnityEngine;

public class MainSceneSetup : MonoBehaviour
{
    public Transform spawnPoint; // SpawnPoint position in the Main Scene

    void Start()
    {
        GameObject player = GameObject.FindWithTag("Wanderer");
        if (player != null)
        {
            player.transform.position = spawnPoint.position;
            player.transform.rotation = spawnPoint.rotation;
        }
        else
        {
            Debug.LogError("Player not found! Ensure the character has been marked with DontDestroyOnLoad.");
        }
    }
}
