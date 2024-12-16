using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowShowerController : MonoBehaviour
{
    public float damage = 10f;
    public float slowDuration = 3f;
    public float slowAmount = 0.25f;
    public float radius = 5f;

    // Start is called before the first frame update
    void Start()
    {
        ApplyShowerOfArrowsEffects();
    }

    private void ApplyShowerOfArrowsEffects()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, radius);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Enemy"))
            {
                Enemy enemy = hitCollider.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                    enemy.Slow(slowAmount, slowDuration);
                    Destroy(gameObject, 2f); // Destroy the shower of arrows object
                }
            }
        }
    }
    
    // Update is called once per frame
    void Update()
    {
        // Optional: Add any visual effects or updates here
    }
}
