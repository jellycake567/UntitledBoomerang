﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    [Header("Stats")]
    public float health = 100f;
    public float invulnerableDelay = 1.0f;
    bool isInvulnerable = false;

    [Header("References")]
    public Material defaultStateMat;
    public Material hurtStateMat;

    public void TakeDamage(float damage)
    {
        if (!isInvulnerable)
        {
            health -= damage;

            StartCoroutine(DamageEffect());
        }

        //transform.parent.gameObject.GetComponent<EnemyHuman>().isStaggered = true;
        this.GetComponent<EnemyHuman>().isStaggered = true;
        this.GetComponent<EnemyHuman>().staggerTimer = this.GetComponent<EnemyHuman>().staggerTimerMax;
        
    }

    IEnumerator DamageEffect()
    {
        isInvulnerable = true;

        MeshRenderer enemyMesh = GetComponent<MeshRenderer>();
        enemyMesh.material = hurtStateMat;

        yield return new WaitForSeconds(invulnerableDelay);

        isInvulnerable = false;
        enemyMesh.material = defaultStateMat;
    }
}
