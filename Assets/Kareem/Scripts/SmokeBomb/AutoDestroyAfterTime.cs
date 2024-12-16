using UnityEngine;

public class AutoDestroyAfterTime : MonoBehaviour
{
    [SerializeField] private float destroyDelay = 3f; // Time to destroy the GameObject

    void Start()
    {
        Destroy(gameObject, destroyDelay);
    }
}
