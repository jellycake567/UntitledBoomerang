using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponDetect : MonoBehaviour
{
    [SerializeField] PlayerStats playerStatScript;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Enemy"))
            return;


        //playerStatScript.TakeDamage(10f);
        Debug.Log("hit");
    }
}
