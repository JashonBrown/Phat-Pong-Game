using System;

public interface IHealth
{
    int StartingHealth { get; }
    int CurrentHealth { get; }

    event Action OnDeath;

    /// <summary>
    /// Called when the attached object takes damage.
    /// </summary>
    /// <param name="damage"></param>
    void TakeDamage(int damage);
}