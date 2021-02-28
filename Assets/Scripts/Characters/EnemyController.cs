using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyStates { GUARD, PATROL, CHASE, DEAD }

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(CharacterStates))]
public class EnemyController : MonoBehaviour, IEndGameObserver
{
    private EnemyStates enemyStates;
    private NavMeshAgent agent;
    private Animator anim;
    private CharacterStates characterStates;

    [Header("Basic Settings")]
    public float sightRadius;

    public bool isGuard;
    public float lookAtTime;
    private float speed;
    protected GameObject attackTarget;
    private float remainLookAtTime;
    private float lastAttackTime;
    private Quaternion guardRotation;
    private Collider coll;

    [Header("Patrol State")]
    public float patrolRange;

    private Vector3 wayPoint;
    private Vector3 guardPos;

    private bool isWalk;
    private bool isChase;
    private bool isFollow;
    private bool isDead;
    private bool playerDead;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        characterStates = GetComponent<CharacterStates>();
        coll = GetComponent<Collider>();
        speed = agent.speed;
        guardPos = transform.position;
        guardRotation = transform.rotation;

        remainLookAtTime = lookAtTime;
    }

    private void Start()
    {
        if (isGuard)
        {
            enemyStates = EnemyStates.GUARD;
        }
        else
        {
            enemyStates = EnemyStates.PATROL;
            GetNewWayPoint();
        }

        GameManager.Instance.AddObserver(this);
    }

    //切换场景加载时启用
    //private void OnEnable()
    //{
    //    GameManager.Instance.AddObserver(this);
    //}

    private void OnDisable()
    {
        if (!GameManager.IsInitialized)
        {
            return;
        }
        GameManager.Instance.RemoveObserver(this);
    }

    private void Update()
    {
        if (characterStates.CurrentHealth == 0)
        {
            isDead = true;
        }

        if (!playerDead)
        {
            SwitchStates();
            SwitchAnimation();
            lastAttackTime -= Time.deltaTime;
        }
    }

    private void SwitchAnimation()
    {
        anim.SetBool("Walk", isWalk);
        anim.SetBool("Chase", isChase);
        anim.SetBool("Follow", isFollow);
        anim.SetBool("Critical", characterStates.isCritical);
        anim.SetBool("Death", isDead);
    }

    private void SwitchStates()
    {
        if (isDead)
        {
            enemyStates = EnemyStates.DEAD;
        }

        //如果发现Player
        else if (FoundPlayer())
        {
            enemyStates = EnemyStates.CHASE;
        }

        switch (enemyStates)
        {
            case EnemyStates.GUARD:
                Debug.Log("test333");
                isChase = false;
                if (transform.position != guardPos)
                {
                    Debug.Log("test1");
                    isWalk = true;
                    agent.isStopped = false;
                    agent.destination = guardPos;
                    //if (Vector3.SqrMagnitude(guardPos - transform.position) <= agent.stoppingDistance)
                    if (Vector3.Distance(guardPos, transform.position) <= agent.stoppingDistance)
                    {
                        //慢慢转向
                        transform.rotation = Quaternion.Lerp(transform.rotation, guardRotation, 0.02f);
                        isWalk = false;
                        Debug.Log("test2");
                    }
                }
                break;

            case EnemyStates.PATROL:
                isChase = false;
                agent.speed = speed * 0.5f;

                if (Vector3.Distance(wayPoint, transform.position) <= agent.stoppingDistance)
                {
                    isWalk = false;
                    if (remainLookAtTime > 0)
                    {
                        remainLookAtTime -= Time.deltaTime;
                    }
                    else
                    {
                        GetNewWayPoint();
                    }
                }
                else
                {
                    isWalk = true;
                    agent.destination = wayPoint;
                }
                break;

            case EnemyStates.CHASE:
                //配合动画
                isWalk = false;
                isChase = true;
                agent.speed = speed;
                if (!FoundPlayer())
                {
                    isFollow = false;
                    if (remainLookAtTime > 0)
                    {
                        agent.destination = transform.position;
                        remainLookAtTime -= Time.deltaTime;
                    }
                    else if (isGuard)
                    {
                        enemyStates = EnemyStates.GUARD;
                        remainLookAtTime = lookAtTime;
                    }
                    else
                    {
                        enemyStates = EnemyStates.PATROL;
                        remainLookAtTime = lookAtTime;
                    }
                }
                else
                {
                    isFollow = true;
                    agent.isStopped = false;
                    agent.destination = attackTarget.transform.position;
                }

                //在攻击范围内则攻击
                if (TargetInAttackRange() || TargetInSkillRange())
                {
                    isFollow = false;
                    agent.isStopped = true;
                    if (lastAttackTime <= 0)
                    {
                        lastAttackTime = characterStates.attackData.coolDown;

                        //暴击判断
                        characterStates.isCritical = Random.value < characterStates.attackData.criticalChance;
                        //执行攻击
                        Attack();
                    }
                }
                break;

            case EnemyStates.DEAD:
                coll.enabled = false;
                //agent.enabled = false;
                agent.radius = 0;
                Destroy(gameObject, 3f);
                break;

            default:
                break;
        }
    }

    private void Attack()
    {
        transform.LookAt(attackTarget.transform);
        if (TargetInAttackRange())
        {
            //近身攻击动画
            anim.SetTrigger("Attack");
        }
        if (TargetInSkillRange())
        {
            //技能攻击动画
            anim.SetTrigger("Skill");
        }
    }

    private bool FoundPlayer()
    {
        var colliders = Physics.OverlapSphere(transform.position, sightRadius);

        foreach (var target in colliders)
        {
            if (target.CompareTag("Player"))
            {
                attackTarget = target.gameObject;
                return true;
            }
        }
        attackTarget = null;
        return false;
    }

    private bool TargetInAttackRange()
    {
        if (attackTarget != null)
        {
            return Vector3.Distance(attackTarget.transform.position, transform.position) <= characterStates.attackData.attackRange;
        }
        else
        {
            return false;
        }
    }

    private bool TargetInSkillRange()
    {
        if (attackTarget != null)
        {
            return Vector3.Distance(attackTarget.transform.position, transform.position) <= characterStates.attackData.skillRange;
        }
        else
        {
            return false;
        }
    }

    private void GetNewWayPoint()
    {
        remainLookAtTime = lookAtTime;

        float randomX = Random.Range(-patrolRange, patrolRange);
        float randomZ = Random.Range(-patrolRange, patrolRange);

        Vector3 randomPoint = new Vector3(guardPos.x + randomX, transform.position.y, guardPos.z + randomZ);

        NavMeshHit hit;
        wayPoint = NavMesh.SamplePosition(randomPoint, out hit, patrolRange, 1) ? hit.position : transform.position;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, sightRadius);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, patrolRange);
    }

    //Animation Event
    private void Hit()
    {
        if (attackTarget != null && transform.IsFacingTarget(attackTarget.transform))
        {
            var targetStates = attackTarget.GetComponent<CharacterStates>();
            targetStates.TakeDamage(characterStates, targetStates);
        }
    }

    public void EndNotify()
    {
        //获胜动画
        anim.SetBool("Win", true);
        playerDead = true;
        //停止所有移动
        //停止Agent
        isChase = false;
        isWalk = false;
        attackTarget = null;
    }
}