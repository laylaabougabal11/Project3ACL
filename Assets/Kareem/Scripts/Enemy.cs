using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
    private bool isStunned = false;
    public float originalSpeed;
    public float speed = 3f; // Enemy movement speed

    private void Start()
    {
        originalSpeed = speed;
    }

    private void Update()
    {
        if (!isStunned)
        {
            // Enemy movement logic here
        }
    }

    public void Stun()
    {
        isStunned = true;
        speed = 0; // Stop enemy movement
        Debug.Log($"{name} is stunned!");
    }

    public void Unstun()
    {
        isStunned = false;
        speed = originalSpeed; // Restore enemy movement
        Debug.Log($"{name} is no longer stunned!");
    }

    public void TakeDamage(float damage)
    {
        Debug.Log($"Enemy took {damage} damage!");
        // Additional logic for handling damage can go here
    }

    public void Slow(float slowAmount, float duration)
    {
        StartCoroutine(SlowCoroutine(slowAmount, duration));
    }

    private IEnumerator SlowCoroutine(float slowAmount, float duration)
    {
        float originalSpeed = speed;
        speed *= slowAmount;
        Debug.Log($"{name} speed reduced to {speed}");
        yield return new WaitForSeconds(duration);
        speed = originalSpeed;
        Debug.Log($"{name} speed reset to {speed}");
    }
}