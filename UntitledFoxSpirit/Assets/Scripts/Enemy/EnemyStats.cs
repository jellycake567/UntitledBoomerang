using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    [Header("Stats")]
    public float maxHealth = 100f;
    public float currentHealth;
    public float invulnerableDelay = 1.0f;
    bool isInvulnerable = false;

    [Header("References")]
    public Material defaultStateMat;
    public Material hurtStateMat;
    void Start()
    {
        currentHealth = maxHealth;
        //healthBar.value = currentHealth / health;

       
    }
    public void TakeDamage(float damage)
    {
        if (!isInvulnerable)
        {
            currentHealth -= damage;

            StartCoroutine(DamageEffect());
        }

      
        this.GetComponent<EnemyHuman>().isStaggered = true;
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
