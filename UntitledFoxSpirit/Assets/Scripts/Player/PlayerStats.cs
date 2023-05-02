using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour
{
    [Header("Stats")]
    public float health = 100f;
    private float currentHealth;
    public float invulnerableDelay = 1.0f;
    bool isInvulnerable = false;

    [Header("Potion")]
    [SerializeField] int numOfPotions = 3;
    [SerializeField] float potionHeal = 50f;
    private int currentPotions;
    
    [Header("References")]
    public Material defaultStateMat;
    public Material hurtStateMat;
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
        //if (Input.GetKeyDown(KeyCode.N))
        //{
        //    TakeDamage(10f);
        //}

        if (Input.GetKeyDown(KeyCode.Q))
        {
            UsePotion();
        }
    }

    public void TakeDamage(float damage)
    {
        if (!isInvulnerable)
        {
            health -= damage;

            StartCoroutine(DamageEffect());
        }
    }

    IEnumerator DamageEffect()
    {
        isInvulnerable = true;

        //MeshRenderer playerMesh = GetComponent<MeshRenderer>();
        //playerMesh.material = hurtStateMat;

        yield return new WaitForSeconds(invulnerableDelay);
        
        isInvulnerable = false; //does not hit??????????
        //playerMesh.material = defaultStateMat;
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
