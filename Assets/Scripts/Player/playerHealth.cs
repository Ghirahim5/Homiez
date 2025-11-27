using System.ComponentModel;
using TMPro.EditorUtilities;
using UnityEngine;

public class PlayerHealth
{
    private playerController _pc;

    public PlayerHealth(playerController controller)
    {
        _pc = controller;
    }

    public void InitializeHearts()
    {
        for (int i = 0; i < _pc.hearts.Length; i++)
        {
            if (i < _pc.maxHealth)
            {
                _pc.hearts[i].enabled = true;
            }
            else
            {
                _pc.hearts[i].enabled = false;
            }
        }
    }
    public void UpdateCurrentHealth()
    {
        for (int i = 0; i < _pc.hearts.Length; i++)
        {
            if (i < _pc.currentHealth/5)
            {
                _pc.hearts[i].enabled = true;
            }
            else
            {
                _pc.hearts[i].enabled = false;
            }
        }
    }

    public void HandleDamage(DamageCollider dmgcollider)
    {
        _pc.currentHealth -= Mathf.RoundToInt(dmgcollider.damage);
    }
    
}