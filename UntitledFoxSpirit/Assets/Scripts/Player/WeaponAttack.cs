using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponAttack : MonoBehaviour
{
    [Header("Weapon Values")]
    public float attackDamage;
    public float attackSpeed;

    [Header("Debug")]
    public bool showDebugLines = false;

    #region Internal Variables

    bool isAttacking = false;
    bool isBlocking = false;
    bool attackDetected = false;

    Animation attackAnimation;

    // ## A list of hardPoints will be referred to a singular hardPoint ##

    // Show Debug Lines 
    List<List<Vector3>> debugHardPoints = new List<List<Vector3>>(); // Stores all the hardPoints
    List<Vector3> preHardPoints = new List<Vector3>(); // Stores the hardPoint position after raycasting
    List<Vector3> hardPointHit = new List<Vector3>(); // Store the hardPoint when weapon collides with enemy

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        attackAnimation = GetComponent<Animation>();
    }

    // Update is called once per frame
    void Update()
    {
        AttackDetection();

        StartCoroutine(Attack());
    }

    void OnDrawGizmos()
    {
        // If there is at least 2 hardPoints
        if (debugHardPoints.Count > 1 && showDebugLines)
        {
            // Loop through the hardPoints
            for (int i = 1; i < debugHardPoints.Count; i++)
            {
                // If hardPoint has hit enemy
                if (hardPointHit == debugHardPoints[i])
                    Gizmos.color = Color.red;
                else
                    Gizmos.color = Color.blue;

                // Draw the lines between hardPoints
                for (int j = 0; j < debugHardPoints[i].Count; j++)
                {
                    Gizmos.color = Color.blue;

                    // Distance between current weapon position and last weapon position
                    float distance = Vector3.Distance(debugHardPoints[i][j], debugHardPoints[i - 1][j]);

                    // Direction from previous to current position
                    Vector3 direction = (debugHardPoints[i][j] - debugHardPoints[i - 1][j]).normalized;

                    RaycastHit hit;
                    if (Physics.Raycast(debugHardPoints[i - 1][j], direction, out hit, distance))
                    {
                        // If it is an enemy
                        if (hit.collider.tag == "Enemy")
                        {
                            Gizmos.color = Color.yellow;
                        }
                    }

                    Gizmos.DrawLine(debugHardPoints[i][j], debugHardPoints[i - 1][j]);
                }
            }
        }
    }

    void AttackDetection()
    {
        if (attackDetected)
        {
            List<Vector3> hardPoints = new List<Vector3>();

            // Get children(hardPoint) position of the weapon
            for (int i = 0; i < transform.childCount; i++)
            {
                hardPoints.Add(transform.GetChild(i).position);
            }

            // If previous hardPoint does exist
            if (preHardPoints.Count > 0)
            {
                for (int i = 0; i < hardPoints.Count; i++)
                {
                    // Distance between current weapon position and last weapon position
                    float distance = Vector3.Distance(hardPoints[i], preHardPoints[i]);

                    // Direction from previous to current position
                    Vector3 direction = (hardPoints[i] - preHardPoints[i]).normalized;

                    RaycastHit hit;
                    if (Physics.Raycast(preHardPoints[i], direction, out hit, distance))
                    {
                        // If it is an enemy
                        if (hit.collider.tag == "Enemy")
                        {
                            // Show where in the swing the attack is detected
                            if (showDebugLines)
                                hardPointHit = hardPoints;

                            // Enemy Take Damage
                            Debug.Log("hit enemy");
                            hit.collider.gameObject.GetComponent<EnemyStats>().TakeDamage(attackDamage);                           
                        }
                    }
                }
            }

            // Store current hardpoint to previous for next raycasting check
            preHardPoints = hardPoints;

            // Show swing lines
            if (showDebugLines)
                debugHardPoints.Add(hardPoints);
        }
    }

    IEnumerator Attack()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && !isAttacking)
        {
            // Clear debug lines
            if (showDebugLines)
            {
                debugHardPoints.Clear();
                hardPointHit = new List<Vector3>();
            }

            // Clear previous hardPoint
            preHardPoints = new List<Vector3>();

            // Attack Animation
            attackAnimation["Attack"].speed = attackSpeed;
            attackAnimation.Play();

            isAttacking = true;

            yield return new WaitForSeconds(attackAnimation["Attack"].length);

            isAttacking = false;
        }
    }

    #region Animation Events

    void EnableAttackDetection()
    {
        attackDetected = true;
    }

    void DisableAttackDetection()
    {
        attackDetected = false;
    }

    #endregion
}
