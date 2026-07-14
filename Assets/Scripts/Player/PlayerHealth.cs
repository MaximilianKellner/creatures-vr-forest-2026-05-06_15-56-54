using UnityEngine;
using System;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 150;
    [SerializeField] private int currentHealth = 150;

    private int baseMaxHealth;
    private int appliedMaxHealthBonus;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    public event Action<int, int> OnHealthChanged;
    public event Action OnDeath;

    private void Awake()
    {
        baseMaxHealth = maxHealth;
        currentHealth = maxHealth;
    }

    public void Heal(int amount)
{
    if (currentHealth <= 0) return; // Wenn der Spieler tot ist, bringt Heilung nichts mehr

    currentHealth += amount;
    currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); // Verhindert, dass das Leben über das Maximum steigt

    // UI informieren, dass sich das Leben geändert hat
    OnHealthChanged?.Invoke(currentHealth, maxHealth);
}

    public void TakeDamage(int amount)
    {
        if (currentHealth <= 0) return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        OnDeath?.Invoke();
        Time.timeScale = 0f;
        Debug.Log("GAME OVER");
    }
    public void Kill()
    {
        if (currentHealth <= 0) return;

        currentHealth = 0;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        Die();
    }

    public void ApplyMaxHealthBonus(int bonus)
    {
        bonus = Mathf.Max(0, bonus);

        if (bonus <= appliedMaxHealthBonus)
            return;

        int gainedHealth = bonus - appliedMaxHealthBonus;
        appliedMaxHealthBonus = bonus;
        maxHealth = baseMaxHealth + appliedMaxHealthBonus;
        currentHealth = Mathf.Clamp(currentHealth + gainedHealth, 0, maxHealth);

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
}
