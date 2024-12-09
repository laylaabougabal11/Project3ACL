using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HPBarBillboard : MonoBehaviour
{
    public Camera mainCamera; // Reference to the main camera

    void Start()
    {
        // If no camera is assigned, find the main camera
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    void LateUpdate()
    {
        // Ensure the object faces the camera
        transform.LookAt(transform.position + mainCamera.transform.forward);
    }
}

