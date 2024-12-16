using UnityEngine;
using Cinemachine;

public class DynamicCameraController : MonoBehaviour
{
    public CinemachineVirtualCamera virtualCamera;

    void Start()
    {
        UpdateCameraTarget();
    }

    public void UpdateCameraTarget()
    {
        // Find the active Wanderer in the scene
        WandererController currentWanderer = FindObjectOfType<WandererController>();

        if (currentWanderer != null && virtualCamera != null)
        {
            // Set the Follow and LookAt targets to the current Wanderer
            Transform wandererTransform = currentWanderer.transform;
            virtualCamera.Follow = wandererTransform;
            virtualCamera.LookAt = wandererTransform;

            Debug.Log($"Camera is now following and looking at: {wandererTransform.name}");
        }
        else
        {
            Debug.LogWarning("No WandererController or Virtual Camera found in the scene!");
        }
    }
}
