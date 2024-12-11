public interface IHealth
{
    int CurrentHealth { get; }
    int MaxHealth { get; }
    void TakeDamage(int damage); // Add this method to handle damage
}
