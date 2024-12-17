using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmokeBombController : MonoBehaviour
{
    [Header("Smoke Bomb Settings")]
    [SerializeField] private GameObject smokeParticlesPrefab; // Prefab for the smoke effect
    [SerializeField] private float stunRadius = 5f; // Radius of the stun effect
    [SerializeField] private float stunDuration = 5f; // Duration of the stun
    [SerializeField] private LayerMask enemyLayer; // Layer for enemies

    private void Start()
    {
        // Debugging to ensure the correct enemy layer is set
        Debug.Log($"Enemy Layer: {enemyLayer.value}");
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Smoke Bomb exploded!");

        // Instantiate the smoke particles at the bomb's position
        if (smokeParticlesPrefab != null)
        {
            Instantiate(smokeParticlesPrefab, transform.position, Quaternion.identity);
        }

        // Find all enemies within the stun radius
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, stunRadius, enemyLayer);
        Debug.Log($"Number of enemies detected in radius: {hitColliders.Length}");

        foreach (Collider hitCollider in hitColliders)
        {
            MinionController minion = hitCollider.GetComponent<MinionController>();
            DemonController demon = hitCollider.GetComponent<DemonController>();

            if (minion != null)
            {
                Debug.Log($"Applying stun to Minion: {minion.name}");
                StartCoroutine(ApplyStun(minion));
            }
            else if (demon != null)
            {
                Debug.Log($"Applying stun to Demon: {demon.name}");
                StartCoroutine(ApplyStun(demon));
            }
            else
            {
                Debug.Log("Detected object is not a valid enemy.");
            }
        }

        // Destroy the smoke bomb after it explodes
        Destroy(gameObject);
    }

    private IEnumerator ApplyStun(MonoBehaviour enemy)
    {
        Debug.Log($"Stunning enemy: {enemy.name} for {stunDuration} seconds.");

        // Apply the stun based on the type of enemy
        if (enemy is MinionController minion) minion.Stun(stunDuration);
        if (enemy is DemonController demon) demon.Stun(stunDuration);

        // Wait for the stun duration to expire
        yield return new WaitForSeconds(stunDuration);

        // Remove the stun effect
        if (enemy is MinionController minionUnstun) minionUnstun.Unstun();
        if (enemy is DemonController demonUnstun) demonUnstun.Unstun();

        Debug.Log($"Enemy: {enemy.name} is no longer stunned.");
    }

    private void OnDrawGizmosSelected()
    {
        // Visualize the stun radius in the Unity Editor
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stunRadius);
    }
}
