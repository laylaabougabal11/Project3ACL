using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmokeBombController : MonoBehaviour
{
    [SerializeField] private GameObject smokeParticlesPrefab; // Smoke effect prefab
    [SerializeField] private float stunRadius = 5f; // Radius of the stun effect
    [SerializeField] private float stunDuration = 5f; // Duration of the stun
    [SerializeField] private LayerMask enemyLayer;

    private void Start()
    {
        enemyLayer = LayerMask.GetMask("enemyLayer");
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
            MinionController minion = hitCollider.GetComponent<MinionController>();
            DemonController demon = hitCollider.GetComponent<DemonController>();

            if (minion != null)
            {
                StartCoroutine(ApplyStun(minion));
            }
            else if (demon != null)
            {
                StartCoroutine(ApplyStun(demon));
            }
            else
            {
                Debug.Log("No compatible enemy script found on detected object.");
            }
        }

        // Destroy the smoke bomb GameObject
        Destroy(gameObject);
    }

    private IEnumerator ApplyStun(MonoBehaviour enemy)
    {
        Debug.Log($"Stunning enemy: {enemy.name} for {stunDuration} seconds.");
        if (enemy is MinionController minion) minion.Stun(stunDuration); // Pass stunDuration
        if (enemy is DemonController demon) demon.Stun(stunDuration); // Pass stunDuration

        yield return new WaitForSeconds(stunDuration);

        if (enemy is MinionController minionUnstun) minionUnstun.Unstun();
        if (enemy is DemonController demonUnstun) demonUnstun.Unstun();
        Debug.Log($"Enemy: {enemy.name} is no longer stunned.");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stunRadius);
    }
}
