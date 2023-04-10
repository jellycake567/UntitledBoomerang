using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
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
}
