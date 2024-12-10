using UnityEngine;

public class Enemy : MonoBehaviour
{
    public void TakeDamage(int damage)
    {
        Debug.Log($"Enemy took {damage} damage!");
        // Additional logic for handling damage can go here
    }
}
