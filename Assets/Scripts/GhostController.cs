using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class GhostController : MonoBehaviour
{
    public static bool GlobalCanMove = false;

    public enum GhostState
    {
        Normal,
        Scared,
        Recovering,
        Dead
    }

    [Header("Components")]
    [SerializeField] private Animator animator;
    [SerializeField] private Tilemap wallTilemap;

    [Header("Ghost Identification & References")]
    [SerializeField] private int ghostID = 1; 
    [SerializeField] private Transform pacStudentTransform;
    [SerializeField] private float pacStudentSpeed = 5f;
    [SerializeField] private Vector3Int spawnCenter = new Vector3Int(13, 14, 0); 

    [Header("Spawn Area Exit Settings")]
    [SerializeField] private RectInt spawnAreaRect; 
    [SerializeField] private Vector3Int topExitNode;    
    [SerializeField] private Vector3Int bottomExitNode; 
    private bool hasExitedSpawn = false;

    private float NormalSpeed => pacStudentSpeed * 0.9f;
    private float SpecialSpeed => NormalSpeed * 0.5f;

    private Vector3Int currentGridPos;
    private Vector3Int targetGridPos;
    private Vector3Int direction = Vector3Int.down;
    private bool isLerping = false;
    private float lerpProgress = 0f;

    public GhostState CurrentState { get; private set; } = GhostState.Normal;

    void Start()
    {
        currentGridPos = new Vector3Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y), 0);
        targetGridPos = currentGridPos;
    }

    void Update()
    {
        if (!GlobalCanMove) return;

        if (!isLerping)
        {
            ChooseNextDirection();
            StartLerp(targetGridPos);
        }
        else
        {
            ExecuteLerp();
        }
    }

    private void ChooseNextDirection()
    {
        if (CurrentState == GhostState.Dead)
        {
            ChooseDeadMovement();
            return;
        }

        if (!hasExitedSpawn && IsInSpawnArea())
        {
            ChooseSpawnExitMovement();
            return;
        }

        Vector3Int[] possibleDirections = { Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right };
        List<Vector3Int> validDirs = new List<Vector3Int>();

        foreach (var dir in possibleDirections)
        {
            if (dir == -direction && possibleDirections.Length > 1) continue;

            Vector3Int candidate = currentGridPos + dir;
            if (IsValidGhostTile(candidate))
            {
                validDirs.Add(dir);
            }
        }

        if (validDirs.Count == 0)
        {
            validDirs.Add(-direction);
        }

        if (CurrentState == GhostState.Scared || CurrentState == GhostState.Recovering || ghostID == 1)
        {
            direction = GetDistanceFilteredDirection(validDirs, true);
        }
        else if (ghostID == 2)
        {
            direction = GetDistanceFilteredDirection(validDirs, false);
        }
        else if (ghostID == 3)
        {
            direction = validDirs[Random.Range(0, validDirs.Count)];
        }
        else if (ghostID == 4)
        {
            direction = GetClockwiseDirection(validDirs);
        }

        targetGridPos = currentGridPos + direction;
        UpdateAnimatorDirection();
    }

    private void ChooseDeadMovement()
    {
        if (currentGridPos == spawnCenter)
        {
            SetState(GhostState.Normal);
            return;
        }

        Vector3Int diff = spawnCenter - currentGridPos;
        if (Mathf.Abs(diff.x) > Mathf.Abs(diff.y))
        {
            direction = new Vector3Int(System.Math.Sign(diff.x), 0, 0);
        }
        else if (Mathf.Abs(diff.y) != 0)
        {
            direction = new Vector3Int(0, System.Math.Sign(diff.y), 0);
        }
        else
        {
            direction = Vector3Int.zero;
        }

        targetGridPos = currentGridPos + direction;
    }

    private bool IsInSpawnArea()
    {
        return spawnAreaRect.Contains(new Vector2Int(currentGridPos.x, currentGridPos.y));
    }

    private void ChooseSpawnExitMovement()
    {

        Vector3Int targetExit = (ghostID == 1 || ghostID == 3) ? topExitNode : bottomExitNode;

        if (currentGridPos == targetExit || !IsInSpawnArea())
        {
            hasExitedSpawn = true;
            return;
        }

        Vector3Int diff = targetExit - currentGridPos;
        if (Mathf.Abs(diff.x) > Mathf.Abs(diff.y))
        {
            direction = new Vector3Int(System.Math.Sign(diff.x), 0, 0);
        }
        else if (diff.y != 0)
        {
            direction = new Vector3Int(0, System.Math.Sign(diff.y), 0);
        }
        else
        {
            direction = Vector3Int.up;
        }

        targetGridPos = currentGridPos + direction;
    }

    private void StartLerp(Vector3Int target)
    {
        targetGridPos = target;
        lerpProgress = 0f;
        isLerping = true;
    }

    private void ExecuteLerp()
    {
        float currentActiveSpeed = (CurrentState == GhostState.Normal) ? NormalSpeed : SpecialSpeed;

        lerpProgress += Time.deltaTime * currentActiveSpeed;
        transform.position = Vector3.Lerp(currentGridPos, targetGridPos, lerpProgress);

        if (lerpProgress >= 1f)
        {
            transform.position = targetGridPos;
            currentGridPos = targetGridPos;
            isLerping = false;
        }
    }

    private bool IsValidGhostTile(Vector3Int candidatePos)
    {
        if (wallTilemap != null && wallTilemap.HasTile(candidatePos))
        {
            return false;
        }

        if (candidatePos.x <= 0 || candidatePos.x >= 27)
        {
            return false;
        }

        if (hasExitedSpawn && CurrentState != GhostState.Dead && spawnAreaRect.Contains(new Vector2Int(candidatePos.x, candidatePos.y)))
        {
            return false;
        }

        return true;
    }

    private Vector3Int GetDistanceFilteredDirection(List<Vector3Int> validDirs, bool wantFurther)
    {
        if (pacStudentTransform == null) return validDirs[Random.Range(0, validDirs.Count)];

        Vector3Int bestDir = validDirs[0];
        float bestDistance = wantFurther ? float.MinValue : float.MaxValue;

        foreach (var dir in validDirs)
        {
            Vector3 candidateWorldPos = currentGridPos + dir;
            float dist = Vector3.Distance(candidateWorldPos, pacStudentTransform.position);

            if (wantFurther)
            {
                if (dist >= bestDistance)
                {
                    bestDistance = dist;
                    bestDir = dir;
                }
            }
            else
            {
                if (dist <= bestDistance)
                {
                    bestDistance = dist;
                    bestDir = dir;
                }
            }
        }
        return bestDir;
    }

    private Vector3Int GetClockwiseDirection(List<Vector3Int> validDirs)
    {
        Vector3Int[] clockwiseOrder = { Vector3Int.up, Vector3Int.right, Vector3Int.down, Vector3Int.left };
        
        foreach (var dir in clockwiseOrder)
        {
            if (validDirs.Contains(dir))
            {
                return dir;
            }
        }
        return validDirs[0];
    }

    public void SetState(GhostState newState)
    {
        if (CurrentState == GhostState.Dead && newState != GhostState.Normal) return;
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
