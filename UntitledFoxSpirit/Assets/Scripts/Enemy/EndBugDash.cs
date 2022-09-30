using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;

public class EndBugDash : MonoBehaviour
{
    EnemyNavigation enemyNav;

    void Start()
    {
        enemyNav = GetComponentInParent<EnemyNavigation>();
    }

    public void EndAttack()
    {
        enemyNav.attackAnim.Stop();

        enemyNav.isDashing = false;

        transform.parent.position = transform.position;
        transform.localPosition = new Vector3(0, 0, 0);
    }

}
