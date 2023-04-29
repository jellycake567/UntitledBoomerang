using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour
{
    [SerializeField] float health = 100f;
    private float currentHealth;
    [SerializeField] int numOfPotions = 3;
    [SerializeField] float potionHeal = 50f;
    private int currentPotions;

    [SerializeField] Slider healthBar;
    [SerializeField] TMP_Text potionNumDisplay;

    void Start()
    {
        currentHealth = health;
        healthBar.value = currentHealth / health;

        currentPotions = numOfPotions;
        potionNumDisplay.text = currentPotions.ToString();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            TakeDamage(10f);
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            UsePotion();
        }
    }

    void TakeDamage(float damage)
    {
        currentHealth -= damage;

        healthBar.value = currentHealth / health;
    }

    void UsePotion()
    {
        if (currentPotions > 0)
        {
            currentHealth += potionHeal;

            if (currentHealth > health)
                currentHealth = health;

            healthBar.value = currentHealth / health;

            currentPotions--;
            potionNumDisplay.text = currentPotions.ToString();

        }

        
    }
}
