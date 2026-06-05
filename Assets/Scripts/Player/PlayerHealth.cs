using UnityEngine;
using System;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth;
    [SerializeField] private int currentHealth;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    public event Action<int, int> OnHealthChanged;
    public event Action OnDeath;

    private void Awake()
    {
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
}