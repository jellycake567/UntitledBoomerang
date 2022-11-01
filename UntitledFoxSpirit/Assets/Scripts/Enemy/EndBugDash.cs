using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;

public class EndBugDash : MonoBehaviour
{
    EnemyNavigation enemyNav;

    void Start()
    {
        //enemyNav = GetComponentInParent<EnemyNavigation>();
    }

    public void EndAttack()
    {
        //enemyNav.attackAnim.Stop();

        //enemyNav.isAttacking = false;

        transform.parent.position = transform.position;
        transform.localPosition = new Vector3(0, 0, 0);
    }

    void OnTriggerStay(Collider other)
    {
        // Get dir from AI to player
        //Vector3 facingDir = (other.ClosestPointOnBounds(enemyNav.target.position) - transform.position).IgnoreYAxis();
        //Vector3 dir = enemyNav.CalculatePathFacingDir(transform.position, facingDir);

        //enemyNav.target.gameObject.GetComponent<PlayerMovement>().TakeDamage(dir.normalized);
    }
}
