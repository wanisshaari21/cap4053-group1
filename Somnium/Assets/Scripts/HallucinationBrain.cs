using UnityEngine;

public class HallucinationBrain : MonoBehaviour
{
    public Transform player;
    public float speed = 12f;
    private Rigidbody2D rb;

    void Awake() => rb = GetComponent<Rigidbody2D>();

    void Update()
    {
        if (!player) return;

        Vector2 dir = ((Vector2)player.position - rb.position).normalized;
        rb.MovePosition(rb.position + dir * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            // Just disappear
            Destroy(gameObject);
        }
        else if (col.CompareTag("Wall"))
        {
            // optional: destroy if hits wall
            Destroy(gameObject);
        }
    }
}