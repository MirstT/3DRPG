using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator anim;
    private CharacterStates characterStates;

    private GameObject attackTarget;
    private float lastAttackTime;
    private bool isDead;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        characterStates = GetComponent<CharacterStates>();
    }

    private void Start()
    {
        MouseManager.Instance.OnMouseClicked += MoveToTarget;
        MouseManager.Instance.OnEnemyClicked += EventAttack;
        //characterStates.MaxHealth = 2;
        //characterStates.characterData.maxHealth = 4;
    }

    private void Update()
    {
        isDead = characterStates.CurrentHealth == 0;
        SwitchAnimation();
        lastAttackTime -= Time.deltaTime;
    }

    private void SwitchAnimation()
    {
        anim.SetFloat("Speed", agent.velocity.sqrMagnitude);
        anim.SetBool("Death", isDead);
    }

    public void MoveToTarget(Vector3 target)
    {
        StopAllCoroutines();
        agent.isStopped = false;
        agent.destination = target;
    }

    private void EventAttack(GameObject target)
    {
        if (target != null)
        {
            attackTarget = target;
            //是否暴击计算
            characterStates.isCritical = UnityEngine.Random.value < characterStates.attackData.criticalChance;
            StartCoroutine(MoveToAttackTarget());
        }
    }

    private IEnumerator MoveToAttackTarget()
    {
        agent.isStopped = false;
        transform.LookAt(attackTarget.transform);

        while (Vector3.Distance(attackTarget.transform.position, transform.position) > characterStates.attackData.attackRange)
        {
            agent.destination = attackTarget.transform.position;
            yield return null;
        }

        agent.isStopped = true;

        //Attack
        if (lastAttackTime < 0)
        {
            anim.SetBool("Critical", characterStates.isCritical);
            anim.SetTrigger("Attack");
            //重置冷却时间
            lastAttackTime = characterStates.attackData.coolDown;
        }
    }

    //Animation Event
    private void Hit()
    {
        var targetStates = attackTarget.GetComponent<CharacterStates>();
        targetStates.TakeDamage(characterStates, targetStates);
    }
}