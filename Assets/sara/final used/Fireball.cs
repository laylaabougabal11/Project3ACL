using UnityEngine;

public class Fireball : MonoBehaviour
{
    public float speed = 10f;  // Fireball speed
    public int damage = 5;     // Damage dealt by Fireball
    public float maxTravelDistance = 50f;  // Maximum distance before the Fireball is destroyed

    [Tooltip("The particle system for the fireball effect.")]
    public ParticleSystem fireballParticle;

    [Tooltip("The collider for the fireball.")]
    public GameObject fireballCollider;

    [Tooltip("The particle system to play upon collision.")]
    public ParticleSystem explosionParticle;

    [Tooltip("Audio source for the explosion sound.")]
    public AudioSource explosionAudioSource;

    private Vector3 startPosition; // The position where the Fireball was spawned
    private Vector3 target; // The target position of the Fireball

    // Set the target position when the Fireball is spawned
    public void Initialize(Vector3 targetPosition)
    {
        target = targetPosition;
        startPosition = transform.position; // Save the starting position
        transform.LookAt(target); // Orient the Fireball toward the target

        // Activate the fireball particle effect
        if (fireballParticle != null)
        {
            fireballParticle.Play();
        }

        // Activate the collider
        if (fireballCollider != null)
        {
            fireballCollider.SetActive(true);
        }
    }

    void Update()
    {
        // Move the Fireball toward the target
        transform.position += transform.forward * speed * Time.deltaTime;

        // Destroy the Fireball if it exceeds the maximum travel distance
        if (Vector3.Distance(startPosition, transform.position) >= maxTravelDistance)
        {
            DestroyFireball();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Fireball collided with: {other.gameObject.name}");

        // Check if the Fireball hits an enemy
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage); // Apply damage to the enemy
        }

        // Stop the fireball particle effect
        if (fireballParticle != null)
        {
            fireballParticle.Stop(); // Stop fireball particle effect
            fireballParticle.transform.parent = null; // Detach for cleanup
            Destroy(fireballParticle.gameObject);
        }

        // Play the explosion particle system
        if (explosionParticle != null)
        {
            Debug.Log("Playing explosion effect...");
            explosionParticle.transform.position = transform.position; // Set explosion position
            explosionParticle.transform.parent = null; // Detach from Fireball
            explosionParticle.Play(); // Play the explosion effect

            // Destroy the explosion particle system after it finishes playing
            Destroy(explosionParticle.gameObject, explosionParticle.main.duration);
        }

        // Play the explosion sound
        if (explosionAudioSource != null)
        {
            Debug.Log("Playing explosion sound...");
            explosionAudioSource.transform.position = transform.position; // Ensure sound is at the explosion position
            explosionAudioSource.transform.parent = null; // Detach from Fireball
            explosionAudioSource.Play();

            // Destroy the explosion audio source after it finishes playing
            Destroy(explosionAudioSource.gameObject, explosionAudioSource.clip.length);
        }

        // Destroy the Fireball after collision
        DestroyFireball();
    }

    private void DestroyFireball()
    {
        // Stop and clean up the fireball particle effect if not already done
        if (fireballParticle != null)
        {
            fireballParticle.Stop();
            fireballParticle.transform.parent = null; // Detach to allow cleanup
            Destroy(fireballParticle.gameObject, fireballParticle.main.duration);
        }

        // Destroy the Fireball GameObject
        Destroy(gameObject);
    }
}
