using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(SphereCollider))] // Гарантує наявність колайдера для тригера
public class Enemy_AI : MonoBehaviour
{
    private NavMeshAgent myNavMeshAgent;
    [SerializeField] private Transform _goal;
    [SerializeField] private float pathUpdateDelay = 0.2f;
    [SerializeField] private float climbSpeed = 3.5f;

    [Header("Combat Settings")]
    [SerializeField] private EnemyStatsSO enemyStats; // Посилання на ScriptableObject
    private SphereCollider attackTrigger;
    private float lastAttackTime;

    private float nextUpdateTime;
    private bool isClimbing = false;

    void Start()
    {
        myNavMeshAgent = GetComponent<NavMeshAgent>();
        myNavMeshAgent.autoTraverseOffMeshLink = false;

        // Налаштування Trigger Area на основі SO
        attackTrigger = GetComponent<SphereCollider>();
        attackTrigger.isTrigger = true;

        if (enemyStats != null)
        {
            // Радіус тригера дорівнює range з SO
            attackTrigger.radius = enemyStats.attackRange;
        }
        else
        {
            Debug.LogWarning("EnemyStatsSO is missing on " + gameObject.name);
        }
    }

    void Update()
    {
        if (isClimbing) return;

        if (myNavMeshAgent.isOnOffMeshLink)
        {
            StartCoroutine(ClimbRoutine());
            return;
        }

        if (_goal != null && Time.time >= nextUpdateTime)
        {
            myNavMeshAgent.SetDestination(_goal.position);
            nextUpdateTime = Time.time + pathUpdateDelay;
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (enemyStats == null) return;
        if (other.CompareTag("Player"))
        {
            if (Time.time >= lastAttackTime + enemyStats.attackCooldown)
            {
                PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(enemyStats.attackDamage);
                    lastAttackTime = Time.time;
                }
            }
        }
    }

    private IEnumerator ClimbRoutine()
    {
        isClimbing = true;
        OffMeshLinkData linkData = myNavMeshAgent.currentOffMeshLinkData;

        bool isClimbingUp = linkData.endPos.y > linkData.startPos.y;
        float pivotOffset = myNavMeshAgent.baseOffset;

        Vector3 floorPoint = (isClimbingUp ? linkData.startPos : linkData.endPos) + (Vector3.up * pivotOffset);
        Vector3 roofPoint = (isClimbingUp ? linkData.endPos : linkData.startPos) + (Vector3.up * pivotOffset);

        float verticalClearance = 0.15f;
        Vector3 edgeAirPoint = new Vector3(floorPoint.x, roofPoint.y + verticalClearance, floorPoint.z);

        if (isClimbingUp)
        {
            yield return MoveToPosition(floorPoint);
            yield return MoveToPosition(edgeAirPoint);
            yield return MoveToPosition(roofPoint);
        }
        else
        {
            Vector3 liftedRoofPoint = new Vector3(roofPoint.x, roofPoint.y + verticalClearance, roofPoint.z);

            yield return MoveToPosition(liftedRoofPoint);
            yield return MoveToPosition(edgeAirPoint);
            yield return MoveToPosition(floorPoint);
        }

        myNavMeshAgent.CompleteOffMeshLink();
        isClimbing = false;
    }

    private IEnumerator MoveToPosition(Vector3 target)
    {
        Vector3 lookDir = new Vector3(target.x - transform.position.x, 0, target.z - transform.position.z);
        if (lookDir.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(lookDir);
        }

        while (transform.position != target)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, climbSpeed * Time.deltaTime);
            yield return null;
        }
    }
}