using UnityEngine;

public class ArrowScript : MonoBehaviour
{
    public float speed = 20f; // Arrow speed
    public int damage = 5;    // Damage value
    private Vector3 target;   // Target position
    private bool targetSet = false;

    [SerializeField] private LayerMask enemyLayer; // LayerMask for enemies

    public void SetTarget(Vector3 targetPosition, int arrowDamage)
    {
        target = targetPosition;
        damage = arrowDamage;
        targetSet = true;

        // Rotate the arrow to face the target at the start
        RotateTowards(target);
    }

    private void Update()
    {
        if (!targetSet) return;

        // Move towards the target
        Vector3 direction = (target - transform.position).normalized;
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);

        // Continuously rotate the arrow to face the movement direction
        if (direction != Vector3.zero)
        {
            RotateTowards(target);
        }

        // Destroy the arrow if it reaches the target
        if (Vector3.Distance(transform.position, target) <= 0.1f)
        {
            DestroyArrow();
        }
    }

    private void RotateTowards(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Only interact with enemies
        if ((1 << other.gameObject.layer & enemyLayer) != 0)
        {
            IHealth targetHealth = other.GetComponent<IHealth>();
            if (targetHealth != null)
            {
                targetHealth.TakeDamage(damage);
                Debug.Log($"Arrow hit {other.name} for {damage} damage.");
            }

            DestroyArrow();
        }
    }

    private void DestroyArrow()
    {
        Destroy(gameObject);
    }
}
