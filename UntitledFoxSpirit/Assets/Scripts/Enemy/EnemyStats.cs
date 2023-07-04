using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyStats : MonoBehaviour
{
    [Header("Stats")]
    public float maxHealth = 100f;
    public float currentHealth;
    public float maxStaggerValue;
    public float currentStaggerValue;
    public float invulnerableDelay = 1.0f;
    bool isInvulnerable = false;

    [Header("References")]
    public Material defaultStateMat;
    public Material hurtStateMat;

    [SerializeField] Slider healthBar;
    [SerializeField] Slider staggerBar;
    void Start()
    {
        currentHealth = maxHealth;
        healthBar.value = currentHealth / maxHealth;
        currentStaggerValue = maxStaggerValue;
        staggerBar.value = currentStaggerValue / maxStaggerValue;
        
       
    }

    private void Update()
    {
        staggerBar.value = currentStaggerValue / maxStaggerValue;
    }
    public void TakeDamage(float damage)
    {
        if (!isInvulnerable)
        {
            currentHealth -= damage;
            healthBar.value = currentHealth / maxHealth;
            currentStaggerValue -= 1;
            
            StartCoroutine(DamageEffect());
        }

        if(currentStaggerValue <= 0)
        {
            this.GetComponent<EnemyHuman>().isStaggered = true;

        }
        CheckHP();


    }

    IEnumerator DamageEffect()
    {
        isInvulnerable = true;

        //MeshRenderer enemyMesh = GetComponent<MeshRenderer>();
        //enemyMesh.material = hurtStateMat;

        yield return new WaitForSeconds(invulnerableDelay);

        isInvulnerable = false;
        //enemyMesh.material = defaultStateMat;
    }

    void CheckHP()
    {
        if(currentHealth <= 0)
        {
            this.GetComponent<EnemyHuman>().isDead = true;
        }
    }

  
}
