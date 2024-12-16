using UnityEngine;

public class ArrowScript : MonoBehaviour
{
    public float speed = 20f; // Arrow speed
    public int damage = 5; // Damage value
    private Vector3 target; // Target position
    private Vector3 startPosition; // Start position
    private float maxTravelDistance = 50f; // Maximum travel distance
    private bool targetSet = false; // Ensures the target is set before moving

    public void SetTarget(Vector3 targetPosition, int v)
    {
        target = targetPosition;
        startPosition = transform.position;
        targetSet = true; // Mark the target as set
        transform.LookAt(target); // Align the arrow to face the target
        Debug.Log($"Arrow target set to: {target}, Start Position: {startPosition}");
    }

    void Update()
    {
        if (!targetSet) return;

        Debug.Log($"Arrow moving towards target: {target} from {transform.position}");
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);

        //if arrow hit collided with anything
        if (Vector3.Distance(startPosition, transform.position) >= maxTravelDistance)
        {
            Debug.Log("Arrow reached its maximum travel distance.");
            DestroyArrow();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            Debug.Log($"Hit trigger enemy! Dealing {damage} damage.");
            DestroyArrow(); // Destroy the arrow after hitting
        }
    }

    //private void OnCollisionEnter(Collider other)
    //{
    //    if (other.CompareTag("Enemy"))
    //    {
    //        Debug.Log($"Hit collision enemy! Dealing {damage} damage.");
    //        DestroyArrow(); // Destroy the arrow after hitting
    //    }
    //    //if collided with anythinggggg
    //    else
    //    {
    //        Debug.Log("Arrow hit something else.");
    //        DestroyArrow();
    //    }
    //}

    private void DestroyArrow()
    {
        Destroy(gameObject);
    }
}
