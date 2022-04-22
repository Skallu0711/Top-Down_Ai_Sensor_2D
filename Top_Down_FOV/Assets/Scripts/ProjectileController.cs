using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    [SerializeField] private float flySpeed = 30;
    private float timeToDespawn = 2;

    private GameObject target;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        target = PlayerController.self.gameObject;
    }

    private void Start()
    {
        rb.velocity = (target.transform.position - transform.position).normalized * flySpeed;
        
        Destroy(gameObject, timeToDespawn);
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.CompareTag("Player") && col.gameObject == PlayerController.self.gameObject)
            Destroy(gameObject);
    }

}