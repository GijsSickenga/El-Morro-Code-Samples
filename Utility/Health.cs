// (c) Gijs Sickenga, 2018 //

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Allows an object to take damage.
/// </summary>
public class Health : MonoBehaviour
{
    [SerializeField]
    private IntReference _currentHealth;
    public int CurrentHealth
    {
        get
        {
            return _currentHealth.Value;
        }

        private set
        {
            // Clamp value so health falls in [0, max] range.
            _currentHealth.Value = Mathf.Clamp(value, 0, MaxHealth);
        }
    }

    [SerializeField]
    private IntReference _maxHealth;
    public int MaxHealth
    {
        get
        {
            return _maxHealth.Value;
        }
    }

    [Tooltip("Event that fires when health is changed.")]
    public IntEvent OnHealthChanged = new IntEvent();

    [Tooltip("Event that fires when damage is applied.")]
    public DamageEvent OnDamage = new DamageEvent();

    [Tooltip("Event that fires on a heal.")]
    public IntEvent OnHeal = new IntEvent();

    [Tooltip("Event that fires when health reaches 0.")]
    public UnityEvent OnHealthDepleted = new UnityEvent();

    /// <summary>
    /// Whether health has reached 0.
    /// </summary>
    private bool _healthDepleted = false;

    private void Start()
    {
        SetHealthToMax();
    }
    
    public void Damage(Damage damage)
    {
        // Clamp to prevent negative damage.
        Damage clampedDamage = new Damage(Mathf.Clamp(damage.amount, 0, int.MaxValue), damage.type);
        SetHealth(CurrentHealth - clampedDamage.amount);

        // Fire OnDamage event with clamped damage value, even if less actual damage was done.
        OnDamage.Invoke(clampedDamage);
    }

    public void Heal(int amount)
    {
        // Only heal if not dead.
        if (!_healthDepleted)
        {
            // Clamp to prevent negative healing.
            int clampedHealAmount = Mathf.Clamp(amount, 0, int.MaxValue);
            int deltaHealth = SetHealth(CurrentHealth + clampedHealAmount);

            // Fire OnHeal event with actual amount of healing.
            OnHeal.Invoke(deltaHealth);
        }
    }

    /// <summary>
    /// Sets health to the given amount.
    /// </summary>
    /// /// <param name="newHealth">What value to set the health to.</param>
    /// <returns>Returns by how much the health changed.</returns>
    public int SetHealth(int newHealth)
    {
        // Cache previous health.
        int previousHealth = CurrentHealth;

        // Set health to given amount.
        CurrentHealth = newHealth;

        if (CurrentHealth == 0)
        {
            // Check if health hit 0 just now.
            if (previousHealth > 0)
            {
                _healthDepleted = true;
                OnHealthDepleted.Invoke();
            }
        }
        else
        {
            _healthDepleted = false;
        }

        // Calculate actual health difference.
        int deltaHealth = CurrentHealth - previousHealth;

        if (deltaHealth != 0)
        {
            OnHealthChanged.Invoke(deltaHealth);
        }
        return deltaHealth;
    }

    /// <summary>
    /// Sets health back to max health.
    /// </summary>
    public void SetHealthToMax()
    {
        SetHealth(MaxHealth);
    }

    private void OnDestroy()
    {
        // Unsubscribe all non-persistent listeners.
        OnHealthChanged.RemoveAllListeners();
        OnDamage.RemoveAllListeners();
        OnHeal.RemoveAllListeners();
        OnHealthDepleted.RemoveAllListeners();
    }
}
