using UnityEngine;

public class Fireball : MonoBehaviour
{
    public float speed = 10f;  // Fireball speed
    public int damage = 5;    // Damage dealt by Fireball

    private Vector3 target;

    // Set the target position when the Fireball is spawned
    public void Initialize(Vector3 targetPosition)
    {
        target = targetPosition;
        transform.LookAt(target); // Orient the Fireball toward the target
    }

    void Update()
    {
        // Move the Fireball toward the target
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Check if the Fireball hits an enemy
        Enemy enemy = collision.gameObject.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage); // Apply damage to the enemy
        }

        // Destroy the Fireball on collision
        Destroy(gameObject);
    }
}
