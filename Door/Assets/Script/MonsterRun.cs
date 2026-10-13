
using UnityEngine;

public class MonsterRun : MonoBehaviour
{
    [Header("Monster Movement")]
    public Transform target;
    public float runSpeed = 5f;
    public float stopDistance = 1.5f;

    [Header("Animation")]
    public Animator monsterAnimator;

    private Vector3 startPosition;
    private Quaternion startRotation;
    private bool isRunning = false;

    void Awake()
    {
        startPosition = transform.position;
        startRotation = transform.rotation;

        if (monsterAnimator == null)
            monsterAnimator = GetComponent<Animator>();
    }

    void OnEnable()
    {
        ResetMonster();
    }

    void Update()
    {
        if (!isRunning || target == null)
            return;

        Vector3 targetPosition = new Vector3(
            target.position.x,
            transform.position.y,
            target.position.z
        );

        float distance = Vector3.Distance(
            transform.position,
            targetPosition
        );

        if (distance <= stopDistance)
        {
            StopRunning();
            return;
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            runSpeed * Time.deltaTime
        );
    }

    public void StartRunning()
    {
        if (target == null)
        {
            Debug.LogWarning("Monster Target is missing!");
            return;
        }

        isRunning = true;

        if (monsterAnimator != null)
        {
            monsterAnimator.SetTrigger("RunTrigger");
        }

        Debug.Log("MONSTER STARTED RUNNING!");
    }

    public void StopRunning()
    {
        isRunning = false;
    }

    public void ResetMonster()
    {
        isRunning = false;

        transform.position = startPosition;
        transform.rotation = startRotation;

        if (monsterAnimator != null)
        {
            monsterAnimator.ResetTrigger("RunTrigger");
            monsterAnimator.Play("animation_howler_idle", 0, 0f);
        }
    }
}
