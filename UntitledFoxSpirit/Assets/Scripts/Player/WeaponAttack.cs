﻿using System.Collections;
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
    List<List<(Vector3, bool)>> debugHardPoints = new List<List<(Vector3, bool)>>(); // Stores all the hardPoints
    List<Vector3> preHardPoints = new List<Vector3>(); // Stores the hardPoint position after raycasting

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
            // Loop through the list of hardPoints
            for (int i = 1; i < debugHardPoints.Count; i++)
            {
                // Loop through hardpoints in the list
                for (int j = 0; j < debugHardPoints[i].Count; j++)
                {
                    // If weapon has hit the enemy make line red
                    if (debugHardPoints[i][j].Item2)
                    {
                        Gizmos.color = Color.red;
                    }
                    else
                    {
                        Gizmos.color = Color.blue;
                    }
                    
                    Gizmos.DrawLine(debugHardPoints[i][j].Item1, debugHardPoints[i - 1][j].Item1);
                }
            }
        }
    }

    void AttackDetection()
    {
        if (attackDetected)
        {
            #region Get HardPoints in weapon

            List<Vector3> hardPoints = new List<Vector3>();

            // Get children(hardPoint) position of the weapon
            for (int i = 0; i < transform.childCount; i++)
            {
                hardPoints.Add(transform.GetChild(i).position);
            }

            #endregion

            // Debug
            List<(Vector3, bool)> storeHardPoints = new List<(Vector3, bool)>();

            // If previous hardPoint does exist (Need previous hardpoint to raycast)
            if (preHardPoints.Count > 0)
            {
                // Loops through hardpoints
                for (int i = 0; i < hardPoints.Count; i++)
                {
                    #region Calculate Raycasting Weapon Detection

                    // Distance between current weapon position and last weapon position
                    float distance = Vector3.Distance(hardPoints[i], preHardPoints[i]);

                    // Direction from previous to current position
                    Vector3 direction = (hardPoints[i] - preHardPoints[i]).normalized;

                    #region Raycast

                    RaycastHit hit;
                    if (Physics.Raycast(preHardPoints[i], direction, out hit, distance))
                    {
                        // If it is an enemy
                        if (hit.collider.tag == "Enemy")
                        {
                            #region Hit Enemy

                            // Show where in the swing the attack is detected
                            if (showDebugLines)
                                storeHardPoints.Add((hardPoints[i], true)); // Adding a tuple

                            // Enemy Take Damage
                            Debug.Log("hit enemy");
                            hit.collider.gameObject.GetComponent<EnemyStats>().TakeDamage(attackDamage);

                            #endregion
                        }
                        else
                        {
                            storeHardPoints.Add((hardPoints[i], false)); // Adding a tuple
                        }
                    }
                    else
                    {
                        storeHardPoints.Add((hardPoints[i], false)); // Adding a tuple
                    }

                    #endregion

                    #endregion
                }
            }

            // Show swing lines
            if (preHardPoints.Count > 0 && showDebugLines)
                debugHardPoints.Add(storeHardPoints);


            // Store current hardpoint to previous for next raycasting check
            preHardPoints = hardPoints;
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