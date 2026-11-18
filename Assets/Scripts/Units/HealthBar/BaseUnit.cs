using UnityEngine;

public class BaseUnit : MonoBehaviour
{
    public string unitName;
    public Tile occupiedTile;
    public Faction faction;

    [Header("Health")]
    public int maxHealth = 10;
    public int currentHealth;
    public int attackDamage = 3;

    protected virtual void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        currentHealth = Mathf.Max(currentHealth - amount, 0);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
    }

    protected virtual void Die()
    {
        if (occupiedTile != null) occupiedTile.occupiedUnit = null;
        Destroy(gameObject);
    }
}
