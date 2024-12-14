using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmokeBombController : MonoBehaviour
{
    [SerializeField] private GameObject smokeParticlesPrefab; // Smoke effect prefab
    [SerializeField] private float stunRadius = 5f; // Radius of the stun effect
    [SerializeField] private float stunDuration = 5f; // Duration of the stun
    [SerializeField] private LayerMask enemyLayer ;

    private void Start()
    {
        Debug.Log($"Enemy layer mask: {enemyLayer.value}");
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Smoke bomb exploded!");

        // Instantiate the smoke particles
        if (smokeParticlesPrefab != null)
        {
            Instantiate(smokeParticlesPrefab, transform.position, Quaternion.identity);
        }

        // Apply stun effect to all enemies in smoke
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, stunRadius, enemyLayer);
        Debug.Log($"Number of enemies detected: {hitColliders.Length}");
        foreach (var hitCollider in hitColliders)
        {
            Enemy enemy = hitCollider.GetComponent<Enemy>();
            if (enemy != null)
            {
                Debug.Log($"Enemy detected: {enemy.name}");
                StartCoroutine(ApplyStun(enemy));
            }
            else
            {
                Debug.Log("No Enemy component found on detected object.");
            }
        }

        // Destroy the smoke bomb GameObject
        Destroy(gameObject);
    }

    private IEnumerator ApplyStun(Enemy enemy)
    {
        Debug.Log($"Stunning enemy: {enemy.name} for {stunDuration} seconds.");
        enemy.Stun(); // Call the enemy's stun method
        Debug.Log($"Enemy: {enemy.name} is stunned.");
        yield return new WaitForSeconds(stunDuration);
        enemy.Unstun(); // Call the enemy's unstun method
        Debug.Log($"Enemy: {enemy.name} is no longer stunned.");
    }
    private void OnDrawGizmosSelected()
{
    Gizmos.color = Color.red;
    Gizmos.DrawWireSphere(transform.position, stunRadius);
}

}
