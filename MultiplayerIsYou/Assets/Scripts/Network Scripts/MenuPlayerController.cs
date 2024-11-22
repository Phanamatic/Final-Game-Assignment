using UnityEngine;

public class MenuPlayerController : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float changeDirectionInterval = 2f;

    private Rigidbody2D rb;
    private Vector2 movement;
    private float changeDirectionTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        ChooseRandomDirection();
    }

    void Update()
    {
        changeDirectionTimer -= Time.deltaTime;
        if (changeDirectionTimer <= 0f)
        {
            ChooseRandomDirection();
        }
    }

    void FixedUpdate()
    {
        rb.velocity = movement * moveSpeed;
    }

    void ChooseRandomDirection()
    {
        movement = Random.insideUnitCircle.normalized;
        changeDirectionTimer = Random.Range(1f, changeDirectionInterval);
    }
}
