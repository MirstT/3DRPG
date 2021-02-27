using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grunt : EnemyController
{
    [Header("Skill")]
    public float kickForce = 10;

    public void KickOff()
    {
        if (attackTarget != null)
        {
            transform.LookAt(attackTarget.transform);
        }
    }
}