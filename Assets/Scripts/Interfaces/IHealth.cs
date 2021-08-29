namespace Interfaces
{
    public interface IHealth
    {
        int StartingHealth { get; }
        int CurrentHealth { get; }

        /// <summary>
        /// Called when the attached object takes damage.
        /// </summary>
        /// <param name="damage"></param>
        void TakeDamage(int damage);
    }
}