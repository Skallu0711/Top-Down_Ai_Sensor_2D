using Skallu.Utils.PropertyAttributes.ReadOnlyInspector;
using Skallu.Utils.Tools;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 250; // speed, with which character rotates
    private GameObject player;
    
    #region FieldOfView
    [HideInInspector] public FieldOfView fieldOfView;

    [Header("Field of View states")]
    [ReadOnlyInspector] public bool playerSpotted;
    [ReadOnlyInspector] public bool playerInsideSafeZone;
    [ReadOnlyInspector] public bool playerInsideAttackRange;
    #endregion

    #region Shooting
    [Header("Shooting related stuff")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private GameObject bulletSpawnObj;
    private ParticleSystem shotParticleSystem;
    
    [SerializeField] private float timeBetweenShots = 1;
    [SerializeField] [ReadOnlyInspector] private float shotTimer = 0;
    #endregion

    private void Awake()
    {
        fieldOfView = GetComponent<FieldOfView>();
        shotParticleSystem = bulletSpawnObj.GetComponent<ParticleSystem>();
        player = PlayerController.self.gameObject;
    }
    
    private void Start()
    {
        shotParticleSystem.Stop();
    }
    
    private void FixedUpdate()
    {
        playerSpotted = fieldOfView.fovState.targetSpotted;
        playerInsideSafeZone = fieldOfView.fovState.targetInsideSafeZone;
        playerInsideAttackRange = fieldOfView.fovState.targetInsideAttackRange;
        
        if (playerSpotted)
        {
            RotateTo(player.transform.position, rotationSpeed);
        
            if (Vector3.Dot(transform.right, (player.transform.position - transform.position).normalized) > 0.95f)
                PerformShooting();
        }
        else
        {
            if (shotTimer > 0f)
                shotTimer = 0f;
            
            // rotates character clockwise around z axis
            transform.localEulerAngles += new Vector3(0f, 0f, -rotationSpeed * Time.deltaTime * 0.3f);
        }
        
        DisplayLineOfSight();
    }

    // rotates character to selected target, with certain speed
    private void RotateTo(Vector3 targetPosition, float rotationSpeed)
    {
        Vector2 direction = (targetPosition - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rotationQuaternion = Quaternion.AngleAxis(angle, Vector3.forward);

        transform.rotation = Quaternion.RotateTowards(transform.rotation, rotationQuaternion, Time.deltaTime * rotationSpeed);
    }

    // shoots bullet at constant time intervals
    private void PerformShooting()
    {
        shotTimer += Time.deltaTime;

        if (shotTimer >= timeBetweenShots)
        {
            Shoot();
            shotTimer = 0f;
        }
    }

    // shoots single bullet
    private void Shoot()
    {
        shotParticleSystem.Play();
        Instantiate(bulletPrefab, (Vector2) bulletSpawnObj.transform.position, Quaternion.identity);
    }

    // displays line of sight
    private void DisplayLineOfSight()
    {
        var hit = Physics2D.Raycast(transform.position, transform.right, fieldOfView.viewOuterRadius, LayerMask.GetMask("Obstacle", "Target"));
        
        if (hit.collider != null)
            Debug.DrawLine(transform.position, hit.point, hit.collider.CompareTag("Player") ? Color.green : Color.red);
        else
            Debug.DrawLine(transform.position, transform.position + transform.right * fieldOfView.viewOuterRadius, Color.red);
    }

}