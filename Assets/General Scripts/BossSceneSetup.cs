using Cinemachine;
using UnityEngine;

public class BossSceneSetup : MonoBehaviour
{

    public CinemachineVirtualCamera virtualCamera; // Reference to the Virtual Camera

    void Start()
    {
        WandererController wanderer = FindObjectOfType<WandererController>();

        if (wanderer != null)
        {
            // Set the Virtual Camera to follow and look at the active character
            if (virtualCamera != null)
            {
                Transform playerTransform = wanderer.transform;
                virtualCamera.Follow = playerTransform;
                virtualCamera.LookAt = playerTransform;
                Debug.Log("Virtual Camera updated to follow the activated character.");
            }
            else
            {
                Debug.LogError("Virtual Camera reference is missing! Assign it in the inspector.");
            }
            // Find the spawn point tagged "BossSpawnPoint"
            Transform spawnPoint = GameObject.FindGameObjectWithTag("BossSpawnPoint")?.transform;

            if (spawnPoint != null)
            {
                wanderer.transform.position = spawnPoint.position;
                wanderer.transform.rotation = spawnPoint.rotation;
                Debug.Log("Wanderer positioned at Boss Scene spawn point.");
            }
            else
            {
                Debug.LogError("BossSpawnPoint not found in the scene!");
            }
        }
        else
        {
            Debug.LogError("WandererController not found!");
        }
    }
}
