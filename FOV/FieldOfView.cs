using System.Collections;
using SkalluUtils.PropertyAttributes; // download package via package manager https://github.com/Skallu0711/Skallu-Utils.git
using UnityEngine;
using Random = UnityEngine.Random;

public class FieldOfView : MonoBehaviour
{
    #region FIELD OF VIEW STATES
    [ReadOnlyInspector] public bool targetInsideViewOuterRadius;
    [ReadOnlyInspector] public bool targetSpotted;
    [ReadOnlyInspector] public bool targetInsideSafeZone;
    [ReadOnlyInspector] public bool targetInsideAttackRange;
    #endregion

    # region FIELD OF VIEW PARAMETERS
    public float viewOuterRadius; // Area that determines the ability to detect target within it, provided that it is also within the viewing angle cone
    public float viewInnerRadius; // The minimum area that determines the ability to detect target within it
    public float viewAngle; // Angle (in degrees), which determines the ability to spot objects within its area
    
    public float safeZoneRadius; // Radius of an optional safe zone area
    public float attackRangeRadius; // Radius of an optional attack range area
    
    public float zoneCheckInterval = 0.02f; // Time interval between zone checks (i.e. fov update)
    public float castRadius = 0.1f; // CircleCast radius value
    public LayerMask obstacleLayerMask; // Layer with all obstacles, which is used during circle cast. Enemy cannot see through obstacles
    # endregion

    [HideInInspector] public GameObject target; // you can display this in inspector if You want to choose target manually or set in awake
    private Vector3 randomLookOffset;

    private void Awake()
    {
        // set target here
        // target = 
    }

    private void Start() => StartCoroutine(PerformZonesChecksWithDelay());

    /// <summary>
    /// Performs all zone checks with some delay for the better performance.
    /// </summary>
    private IEnumerator PerformZonesChecksWithDelay()
    {
        var delayTime = zoneCheckInterval;

        while (true)
        {
            yield return new WaitForSeconds(delayTime);
            
            CheckForVisibleTarget();
        
            // do not perform "safe zone" and "attack range" checks if they aren't used
            if (safeZoneRadius > 0) 
                CheckSafeZone();
            if (attackRangeRadius > 0)
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
            targetInsideViewOuterRadius = true;
            
            var directionToTarget = (target.transform.position + randomLookOffset - transform.position).normalized;
            var distanceToTarget = Vector3.Distance(transform.position, target.transform.position);

            if (Vector2.SqrMagnitude(target.transform.position - transform.position) <= viewInnerRadius * viewInnerRadius) // when target is inside inner view radius
            {
                // when circle cast doesn't collide with any object from obstacle mask, it means, that target is spotted
                targetSpotted = !Physics2D.CircleCast(transform.position, castRadius, directionToTarget, distanceToTarget, obstacleLayerMask);
            }
            else
            {
                if (Vector3.Angle(transform.right, directionToTarget) < viewAngle * 0.5f) // when the target is inside view angle
                {
                    // when circle cast doesn't collide with any object from obstacle mask, it means, that target is spotted
                    targetSpotted = !Physics2D.CircleCast(transform.position, castRadius, directionToTarget, distanceToTarget, obstacleLayerMask);
                }
                else
                {
                    targetSpotted = false;
                }
            }
        }
        else
        {
            targetInsideViewOuterRadius = false;
            targetSpotted = false;
        }
    }
    
    /// <summary>
    /// Checks if player is inside specific radius
    /// </summary>
    /// <param name="radius"> float value considered as "radius" to check (e.g. "safe zone" or "attack range") </param>
    /// <param name="obstacleMask"> LayerMask considered as obstacle layer, which is used during circle cast </param>
    /// <returns> bool value which specifies if target is inside specified radius </returns>
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
    /// Checks if player is inside character's "safe zone". Value of "target inside safe zone" flag is the result of check action.
    /// </summary>
    private void CheckSafeZone() => targetInsideSafeZone = CheckIfTargetIsInsideSpecificRadius(safeZoneRadius, obstacleLayerMask);

    /// <summary>
    /// Checks if player is inside character's "attack range". Value of "target inside attack range" flag is the result of check action
    /// </summary>
    private void CheckAttackRange() => targetInsideAttackRange = CheckIfTargetIsInsideSpecificRadius(attackRangeRadius, obstacleLayerMask);
    
}