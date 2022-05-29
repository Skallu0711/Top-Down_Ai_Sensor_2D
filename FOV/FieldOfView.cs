using System;
using System.Collections;
using Skallu.Utils.PropertyAttributes.ReadOnlyInspector;
using UnityEngine;
using Random = UnityEngine.Random;

public class FieldOfView : MonoBehaviour
{
    [Serializable]
    public struct HandlesParameters
    {
        public Color mainFovColor; // view radius and view angle handles color
        public Color safeZoneColor;
        public Color attackRangeColor;
        [Range(0.5f, 2)] public float thickness; // handles thickness
        [HideInInspector] public Color targetSpottedColor;
        [HideInInspector] public Color targetHiddenColor;
    }

    [Serializable]
    public struct FovState
    {
        [HideInInspector] public bool targetInsideViewOuterRadius;
        [ReadOnlyInspector] public bool targetSpotted;
        [ReadOnlyInspector] public bool targetInsideSafeZone;
        [ReadOnlyInspector] public bool targetInsideAttackRange;
    }

    # region fovParameters
    [Header("Field of View Parameters")]
    
    [Tooltip("Area that determines the ability to detect target within it, provided that they are also within the viewing angle cone.")]
    [Range(0, 20)] public float viewOuterRadius;
    
    [Tooltip("The minimum area that determines the ability to detect target within it.")]
    [Range(0, 10)] public float viewInnerRadius;
    
    [Tooltip("Angle (in degrees), which determines the ability to spot objects within its area.")]
    [Range(0, 360)] public float viewAngle;
    
    [Space]
    
    [Tooltip("An optional area that can be used, e.g. to retreating, when target is too close.")]
    [Range(0, 10)] public float safeZoneRadius;
    
    [Tooltip("An optional area that can be used, e.g. to detect if a character is close enough to attack.")]
    [Range(0, 10)] public float attackRangeRadius;
    
    [Space]
    
    [Tooltip("Time interval between zone checks (i.e. fov update).")]
    [Range(0.001f, 1)] [SerializeField] private float zoneCheckInterval = 0.02f;
    
    [Tooltip("Value of the circle cast radius.")]
    [Range(0, 10)] public float castRadius = 0.1f;
    
    [Tooltip("Layer with all obstacles, which is used during circle cast. Enemy cannot see through obstacles.")]
    public LayerMask obstacleLayerMask;
    # endregion
    
    [Space]
    public HandlesParameters handlesParameters;
    [Space]
    public FovState fovState;
    
    [HideInInspector] public GameObject target; // you can display this in inspector if You want to choose target manually
    private Vector3 randomLookOffset;

    private void Awake()
    {
        // set target here
        target = PlayerController.self.gameObject; // get rid of this, if You are setting this via inspector
    }

    private void Start()
    {
        handlesParameters.targetSpottedColor = Color.green;
        handlesParameters.targetHiddenColor = Color.red;
        
        StartCoroutine(PerformZonesChecksWithDelay());
    }
    
    /// <summary>
    /// Performs all zone checks with some delay for the better performance.
    /// If You want to get rid of delay, better use Update method instead.
    /// </summary>
    /// <returns></returns>
    private IEnumerator PerformZonesChecksWithDelay()
    {
        var delayTime = zoneCheckInterval;

        while (true)
        {
            yield return new WaitForSeconds(delayTime);
            
            CheckForVisibleTarget();
        
            // do not perform "safe zone" and "attack range" checks if they aren't used
            if (safeZoneRadius > 0.1f) 
                CheckSafeZone();
            if (attackRangeRadius > 0.1f)
                CheckAttackRange();
        }
    }

    /// <summary>
    /// Checks for visible targets inside view range and angle.
    /// </summary>
    private void CheckForVisibleTarget()
    {
        randomLookOffset = new Vector3(Random.Range(-castRadius, castRadius), Random.Range(-castRadius, castRadius));
        
        if (Vector2.SqrMagnitude(target.transform.position - transform.position) <= viewOuterRadius * viewOuterRadius) // when target is inside outer view radius
        {
            fovState.targetInsideViewOuterRadius = true;
            
            var directionToTarget = (target.transform.position + randomLookOffset - transform.position).normalized;
            var distanceToTarget = Vector3.Distance(transform.position, target.transform.position);

            if (Vector2.SqrMagnitude(target.transform.position - transform.position) <= viewInnerRadius * viewInnerRadius) // when target is inside inner view radius
            {
                // when circle cast doesn't collide with any object from obstacle mask, it means, that target is spotted
                fovState.targetSpotted = !Physics2D.CircleCast(transform.position, castRadius, directionToTarget, distanceToTarget, obstacleLayerMask);
            }
            else
            {
                if (Vector3.Angle(transform.right, directionToTarget) < viewAngle * 0.5f) // when the target is inside view angle
                {
                    // when circle cast doesn't collide with any object from obstacle mask, it means, that target is spotted
                    fovState.targetSpotted = !Physics2D.CircleCast(transform.position, castRadius, directionToTarget, distanceToTarget, obstacleLayerMask);
                }
                else
                {
                    fovState.targetSpotted = false;
                }
            }
        }
        else
        {
            fovState.targetInsideViewOuterRadius = false;
            fovState.targetSpotted = false;
        }
    }
    
    /// <summary>
    /// Checks if player is inside specific radius.
    /// </summary>
    /// <param name="radius">
    /// Float value considered as "radius" to check (e.g. "safe zone" or "attack range").
    /// </param>
    /// <param name="obstacleMask">
    /// LayerMask considered as obstacle layer, which is used during circle cast.
    /// </param>
    /// <returns></returns>
    private bool CheckIfTargetIsInsideSpecificRadius(float radius, LayerMask obstacleMask)
    {
        if (Vector2.SqrMagnitude(target.transform.position - transform.position) <= radius * radius) // when target is inside inner view radius
        {
            var directionToTarget = (target.transform.position + randomLookOffset - transform.position).normalized;
            var distanceToTarget = Vector3.Distance(transform.position, target.transform.position);
            
            if (!Physics2D.CircleCast(transform.position, castRadius, directionToTarget, distanceToTarget, obstacleMask))
                return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Checks if player is inside character's "safe zone".
    /// Value of "target inside safe zone" flag is the result of check action.
    /// </summary>
    private void CheckSafeZone()
    {
        fovState.targetInsideSafeZone = CheckIfTargetIsInsideSpecificRadius(safeZoneRadius, obstacleLayerMask);
    }
    
    /// <summary>
    /// Checks if player is inside character's "attack range"
    /// Value of "target inside attack range" flag is the result of check action.
    /// </summary>
    private void CheckAttackRange()
    {
        fovState.targetInsideAttackRange = CheckIfTargetIsInsideSpecificRadius(attackRangeRadius, obstacleLayerMask);
    }

}