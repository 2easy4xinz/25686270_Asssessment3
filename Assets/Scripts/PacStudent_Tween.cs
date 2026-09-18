using System.Collections;
using UnityEngine;

public class PacStudent_Tween : MonoBehaviour
{

    [SerializeField] private Vector3[] waypoints = new Vector3[4];
    [SerializeField] private float speed = 3.0f;

    [SerializeField] private AudioSource moveAudio;
    [SerializeField] private Animator animator;

    private int currentWaypointIndex = 0;

    void Start()
    {
        if (moveAudio == null) moveAudio = GetComponent<AudioSource>();
        if (animator == null) animator = GetComponent<Animator>();

        if (waypoints != null && waypoints.Length == 4)
        {
            transform.position = waypoints[0];
            StartCoroutine(MoveClockwiseRoutine());
        }

        if (moveAudio != null && !moveAudio.isPlaying)
        {
            moveAudio.loop = true;
            moveAudio.Play();
        }

    }

    private IEnumerator MoveClockwiseRoutine()
    {
        while (true)
        {
            Vector3 startPos = waypoints[currentWaypointIndex];
            int nextIndex = (currentWaypointIndex + 1) % waypoints.Length;
            Vector3 targetPos = waypoints[nextIndex];

            UpdateAnimation(startPos, targetPos);

            float distance = Vector3.Distance(startPos, targetPos);
            float totalDuration = distance / speed;
            float elapsedTime = 0.0f;

            while (elapsedTime < totalDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / totalDuration;

                transform.position = Vector3.Lerp(startPos, targetPos, t);

                yield return null;
            }

            transform.position = targetPos;
            currentWaypointIndex = nextIndex;

        }

    }

    private void UpdateAnimation(Vector3 from, Vector3 to)
    {
        if (animator == null) return;

        Vector3 dir = (to - from).normalized;

        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        {
            if (dir.x > 0)
                animator.Play("PacStudent_Right");
            else
                animator.Play("PacStudent_Left");
        }
        else
        {
            if (dir.y > 0)
                animator.Play("PacStudent_Up");
            else
                animator.Play("PacStudent_Down");
        }
    }




}
