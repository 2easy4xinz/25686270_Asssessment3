using UnityEngine;

public class CherryController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 4f;
    private Vector3 moveDirection = Vector3.right;
    
    [Header("Screen Boundaries")]
    [SerializeField] private float leftBound = -10f;
    [SerializeField] private float rightBound = 10f;

    private bool isMoving = false;

    void Update()
    {
        if (!isMoving) return;

        transform.position += moveDirection * moveSpeed * Time.deltaTime;

        if (moveDirection.x > 0 && transform.position.x >= rightBound)
        {
            DeactivateCherry();
        }
        else if (moveDirection.x < 0 && transform.position.x <= leftBound)
        {
            DeactivateCherry();
        }
    }

    public void InitializeCherry(Vector3 startPos, Vector3 direction, float leftLimit, float rightLimit)
    {
        transform.position = startPos;
        moveDirection = direction.normalized;
        leftBound = leftLimit;
        rightBound = rightLimit;
        isMoving = true;
        
        gameObject.SetActive(true);
    }

    private void DeactivateCherry()
    {
        isMoving = false;
        gameObject.SetActive(false); 
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            DeactivateCherry();
        }
    }
}