using UnityEngine;

public class GhostController : MonoBehaviour
{
    public enum GhostState
    {
        Normal,
        Scared,
        Recovering,
        Dead
    }

    [Header("Components")]
    [SerializeField] private Animator animator;

    public GhostState CurrentState { get; private set; } = GhostState.Normal;
    private Vector3Int direction = Vector3Int.down;

    public void SetState(GhostState newState)
    {
        if (CurrentState == GhostState.Dead && newState != GhostState.Normal)
        {
            return;
        }

        CurrentState = newState;

        if (animator == null) return;

        switch (newState)
        {
            case GhostState.Normal:
                animator.SetBool("IsScared", false);
                animator.SetBool("IsDead", false);
                break;
            case GhostState.Scared:
            case GhostState.Recovering:
                animator.SetBool("IsScared", true);
                animator.SetBool("IsDead", false);
                break;
            case GhostState.Dead:
                animator.SetBool("IsDead", true);
                break;
        }
    }

    public void SetDirection(Vector3Int newDir)
    {
        direction = newDir;
        UpdateAnimatorDirection();
    }

    private void UpdateAnimatorDirection()
    {
        if (animator == null) return;

        int dirIndex = 1; 
        if (direction == Vector3Int.right) dirIndex = 0;
        else if (direction == Vector3Int.down) dirIndex = 1;
        else if (direction == Vector3Int.left) dirIndex = 2;
        else if (direction == Vector3Int.up) dirIndex = 3;

        animator.SetInteger("Direction", dirIndex);
    }
}
