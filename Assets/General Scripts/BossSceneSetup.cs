using Cinemachine;
using UnityEngine;
using UnityEngine.AI;

public class BossSceneSetup : MonoBehaviour
{
    public CinemachineVirtualCamera virtualCamera; // Reference to the Virtual Camera

    [SerializeField]
    public GameObject bossHPPanel; // Reference to the Boss HP panel
    public GameObject runePanel; // Reference to the Rune panel

    void Start()
    {
        StartCoroutine(SetupBossScene());
        bossHPPanel.SetActive(true);
        runePanel.SetActive(false);
    }

    private System.Collections.IEnumerator SetupBossScene()
    {
        // Wait a frame to ensure all objects are loaded
        yield return new WaitForEndOfFrame();

        WandererController wanderer = FindObjectOfType<WandererController>();

        if (wanderer != null)
        {
            NavMeshAgent agent = wanderer.GetComponent<NavMeshAgent>();

            // Disable NavMeshAgent temporarily
            if (agent != null)
            {
                agent.enabled = false;
            }

            // Find the spawn point tagged "BossSpawnPoint"
            Transform spawnPoint = GameObject.FindGameObjectWithTag("BossSpawnPoint")?.transform;

            if (spawnPoint != null)
            {
                // Move the Wanderer to the spawn point
                wanderer.transform.position = spawnPoint.position;
                wanderer.transform.rotation = spawnPoint.rotation;
                Debug.Log("Wanderer positioned at Boss Scene spawn point.");
            }
            else
            {
                Debug.LogError("BossSpawnPoint not found in the scene!");
            }

            // Re-enable the NavMeshAgent
            if (agent != null)
            {
                agent.enabled = true;
            }

            // Set the Virtual Camera to follow and look at the active Wanderer
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
        }
        else
        {
            Debug.LogError("WandererController not found! Ensure it is marked as DontDestroyOnLoad.");
        }
    }
}
