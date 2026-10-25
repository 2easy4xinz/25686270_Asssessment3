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

    
    private Vector3Int currentGridPos;
    private Vector3Int targetGridPos;
    private Vector3Int direction = Vector3Int.right;

    private bool isLerping = false;
    private float lerpProgress = 0f;

    
    private KeyCode lastInput = KeyCode.D;
    private KeyCode currentInput = KeyCode.D;

    void Start()
    {
        
        currentGridPos = new Vector3Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y), 0);
        targetGridPos = currentGridPos;
        
        if (dustParticleSystem != null) dustParticleSystem.Stop();
    }

    void Update()
    {
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
            Vector3Int currentDir = GetDirectionFromKey(currentInput);
            candidateTarget = currentGridPos + currentDir;

            if (IsWalkable(candidateTarget))
            {
                direction = currentDir;
                StartLerp(candidateTarget);
            }
            else
            {
                StopMovementEffects();
            }
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
}