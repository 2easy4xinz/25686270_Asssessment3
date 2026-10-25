using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;

public class PacStudentController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    
    [Header("Level References")]
    [SerializeField] private Tilemap wallTilemap; 

    [Header("Components & Audio")]
    [SerializeField] private Animator animator;
    [SerializeField] private AudioSource movementAudioSource;
    [SerializeField] private AudioClip normalMoveClip;
    [SerializeField] private AudioClip eatPelletClip;
    [SerializeField] private ParticleSystem dustParticleSystem;

    [Header("Pellet & Score References")]
    [SerializeField] private Tilemap coinTilemap; 
    [SerializeField] private HUDManager hudManager;  

    [Header("Wall Collision Feedback")]
    [SerializeField] private AudioClip wallHitClip;
    [SerializeField] private ParticleSystem wallCollisionParticleSystem;

    [Header("Teleporter Settings")]
    [SerializeField] private Vector3Int leftTeleportExit = new Vector3Int(1, 15, 0); 
    [SerializeField] private Vector3Int rightTeleportExit = new Vector3Int(26, 15, 0); 
    [SerializeField] private int leftBoundaryX = 0;   
    [SerializeField] private int rightBoundaryX = 27;

    [Header("Power Pill References")]
    [SerializeField] private Tilemap powerPillTilemap; 

    [Header("Ghost References")]
    [SerializeField] private GhostController[] ghosts;
    [SerializeField] private AudioSource backgroundAudioSource;
    [SerializeField] private AudioClip normalBgmClip;
    [SerializeField] private AudioClip scaredBgmClip;

    [Header("Lives & Health Settings")]
    [SerializeField] private int maxLives = 3;

    private int currentLives;

    private float ghostScaredTimer = 0f;
    private bool isGhostScaredActive = false;

    private Vector3Int currentGridPos;
    private Vector3Int targetGridPos;
    private Vector3Int direction = Vector3Int.right;

    private bool isLerping = false;
    private float lerpProgress = 0f;

    
    private KeyCode lastInput = KeyCode.D;
    private KeyCode currentInput = KeyCode.D;

    void Start()
    {
        
        currentLives = maxLives;
        currentGridPos = new Vector3Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y), 0);
        targetGridPos = currentGridPos;
        
        if (dustParticleSystem != null) dustParticleSystem.Stop();
    }

    void Update()
    {
        if (hudManager != null && !hudManager.IsGameActive) return;

        if (isGhostScaredActive)
        {
            ghostScaredTimer -= Time.deltaTime;
            if (hudManager != null) hudManager.UpdateScaredTimer(ghostScaredTimer);

            if (ghostScaredTimer <= 3f && ghostScaredTimer > 0f)
            {
                SetAllGhostStates(GhostController.GhostState.Recovering);
            }

            if (ghostScaredTimer <= 0f)
            {
                isGhostScaredActive = false;
                SetAllGhostStates(GhostController.GhostState.Normal);

                if (backgroundAudioSource != null && normalBgmClip != null)
                {
                    backgroundAudioSource.clip = normalBgmClip;
                    backgroundAudioSource.loop = true;
                    backgroundAudioSource.Play();
                }
            }
        }

        GatherInput();

        if (!isLerping)
        {
            TryMove();
        }
        else
        {
            ExecuteLerp();
        }
    }

    private void GatherInput()
    {
        
        if (Keyboard.current == null) return;

        if (Keyboard.current.wKey.wasPressedThisFrame || Keyboard.current.upArrowKey.wasPressedThisFrame)
        {
            lastInput = KeyCode.W;
        }
        else if (Keyboard.current.sKey.wasPressedThisFrame || Keyboard.current.downArrowKey.wasPressedThisFrame)
        {
            lastInput = KeyCode.S;
        }
        else if (Keyboard.current.aKey.wasPressedThisFrame || Keyboard.current.leftArrowKey.wasPressedThisFrame)
        {
            lastInput = KeyCode.A;
        }
        else if (Keyboard.current.dKey.wasPressedThisFrame || Keyboard.current.rightArrowKey.wasPressedThisFrame)
        {
            lastInput = KeyCode.D;
        }
    }

    private void TryMove()
    {
        Vector3Int nextDir = GetDirectionFromKey(lastInput);
        Vector3Int candidateTarget = currentGridPos + nextDir;

        if (IsWalkable(candidateTarget))
        {
            currentInput = lastInput;
            direction = nextDir;
            StartLerp(candidateTarget);
        }
        else
        {
            TriggerWallCollisionEffects();

            Vector3Int currentDir = GetDirectionFromKey(currentInput);
            candidateTarget = currentGridPos + currentDir;

            if (IsWalkable(candidateTarget))
            {
                direction = currentDir;
                StartLerp(candidateTarget);
            }
            else
            {
                TriggerWallCollisionEffects();
                StopMovementEffects();
            }
        }
    }

    private void TriggerWallCollisionEffects()
    {
        if (movementAudioSource != null && wallHitClip != null && !movementAudioSource.isPlaying)
        {
            movementAudioSource.PlayOneShot(wallHitClip);
        }

        if (wallCollisionParticleSystem != null && !wallCollisionParticleSystem.isPlaying)
        {
            wallCollisionParticleSystem.Play();
        }
    }

    private void StartLerp(Vector3Int target)
    {
        targetGridPos = target;
        lerpProgress = 0f;
        isLerping = true;

        if (animator != null) 
        {
            animator.SetBool("IsMoving", true);
        }

        if (dustParticleSystem != null && !dustParticleSystem.isPlaying) dustParticleSystem.Play();

        PlayMovementAudio();
    }

    private void ExecuteLerp()
    {
        lerpProgress += Time.deltaTime * moveSpeed;
        transform.position = Vector3.Lerp(currentGridPos, targetGridPos, lerpProgress);

        UpdateFacingDirection();

        if (lerpProgress >= 1f)
        {
            transform.position = targetGridPos;
            currentGridPos = targetGridPos;
            isLerping = false;

            CheckAndEatPellet();
            CheckTeleporters(); 
        }
    }

    private void CheckTeleporters()
    {
    
        if (currentGridPos.x <= leftBoundaryX)
        {
          
            currentGridPos = new Vector3Int(26, currentGridPos.y, currentGridPos.z);
            targetGridPos = currentGridPos + Vector3Int.left; 
            transform.position = currentGridPos;

            direction = Vector3Int.left;
            currentInput = KeyCode.A;
            lastInput = KeyCode.A;
            StartLerp(targetGridPos);
        }
       
        else if (currentGridPos.x >= rightBoundaryX)
        {
            
            currentGridPos = new Vector3Int(1, currentGridPos.y, currentGridPos.z);
            targetGridPos = currentGridPos + Vector3Int.right;
            transform.position = currentGridPos;

            direction = Vector3Int.right;
            currentInput = KeyCode.D;
            lastInput = KeyCode.D;
            StartLerp(targetGridPos);
        }
    }

    private void CheckAndEatPellet()
    {
        if (coinTilemap != null && coinTilemap.HasTile(currentGridPos))
        {
            coinTilemap.SetTile(currentGridPos, null);

            if (movementAudioSource != null && eatPelletClip != null)
            {
                movementAudioSource.PlayOneShot(eatPelletClip);
            }

            if (hudManager != null)
            {
                hudManager.AddScore(10);
            }
        }

        if (powerPillTilemap != null && powerPillTilemap.HasTile(currentGridPos))
        {
            powerPillTilemap.SetTile(currentGridPos, null);

            if (movementAudioSource != null && eatPelletClip != null)
            {
                movementAudioSource.PlayOneShot(eatPelletClip);
            }

            if (hudManager != null)
            {
                hudManager.AddScore(50);
            }

            ghostScaredTimer = 10f;
            isGhostScaredActive = true;
            SetAllGhostStates(GhostController.GhostState.Scared);

            if (backgroundAudioSource != null && scaredBgmClip != null)
            {
                backgroundAudioSource.clip = scaredBgmClip;
                backgroundAudioSource.loop = true;
                backgroundAudioSource.Play();
            }
        }
    }

    private bool IsWalkable(Vector3Int gridPos)
    {
        if (wallTilemap == null) return true;

        return !wallTilemap.HasTile(gridPos);
    }

    private Vector3Int GetDirectionFromKey(KeyCode key)
    {
        switch (key)
        {
            case KeyCode.W: return Vector3Int.up;
            case KeyCode.S: return Vector3Int.down;
            case KeyCode.A: return Vector3Int.left;
            case KeyCode.D: return Vector3Int.right;
            default: return direction;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ghost"))
        {
            GhostController ghost = collision.GetComponent<GhostController>();

            if (ghost != null)
            {
                if (ghost.CurrentState == GhostController.GhostState.Normal)
                {
                    LoseLife();
                }
                else if (ghost.CurrentState == GhostController.GhostState.Scared || ghost.CurrentState == GhostController.GhostState.Recovering)
                {
                    ghost.SetState(GhostController.GhostState.Dead);
                    if (hudManager != null) hudManager.AddScore(200);
                }
            }
        }
    }

    private void LoseLife()
    {
        currentLives--;
        Debug.Log("PacStudent lost a life! Lives remaining: " + currentLives);

        if (currentLives <= 0)
        {
            
            if (hudManager != null)
            {
                hudManager.ShowGameOver();
            }
            enabled = false;
        }
        // else
        // {
        //     // TODO: Optional reset position to spawn point when losing a life
        // }
    }

    private void PlayMovementAudio()
    {
        if (movementAudioSource == null) return;

        if (movementAudioSource.clip != normalMoveClip || !movementAudioSource.isPlaying)
        {
            movementAudioSource.clip = normalMoveClip;
            movementAudioSource.loop = true;
            movementAudioSource.Play();
        }
    }

    private void StopMovementEffects()
    {
        if (animator != null) 
        {
            animator.SetBool("IsMoving", false);
        }
        if (movementAudioSource != null && movementAudioSource.isPlaying) movementAudioSource.Stop();
        if (dustParticleSystem != null) dustParticleSystem.Stop();
    }

    private void UpdateFacingDirection()
    {
        if (animator == null) return;

    if (direction == Vector3Int.right) 
    {
        animator.SetInteger("Direction", 0);
    }
    else if (direction == Vector3Int.down) 
    {
        animator.SetInteger("Direction", 1);
    }
    else if (direction == Vector3Int.left) 
    {
        animator.SetInteger("Direction", 2);
    }
    else if (direction == Vector3Int.up) 
    {
        animator.SetInteger("Direction", 3);
    }
    }

    private void SetAllGhostStates(GhostController.GhostState state)
    {
        if (ghosts == null) return;
        foreach (var ghost in ghosts)
        {
            if (ghost != null) ghost.SetState(state);
        }
    }

}