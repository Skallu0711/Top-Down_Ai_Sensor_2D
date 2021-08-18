using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldOfView : MonoBehaviour
{
    [Header("Field of view parameters")]
    [Range(0, 10)]
    [SerializeField] internal float viewRadius = 5f;
    
    [Range(0, 360)]
    [SerializeField] internal float viewAngle = 90f; // in degrees
    
    [Header("Debug handles modifiers")]
    [SerializeField] internal Color fovHandlesColor; // view radius and view angle handles color
    [SerializeField] internal Color spottedTargetHandlesColor;
    [SerializeField] internal float handlesThickness = 2.5f;

    [Header("Layer masks references")]
    [SerializeField] internal LayerMask targetLayerMask;
    [SerializeField] internal LayerMask obstacleLayerMask;
    
    internal List<Transform> spottedTargets = new List<Transform>();
    private bool targetSpotted;

    private void Start()
    {
        StartCoroutine(VisibleTargetsCheckDelay());
    }

    private void CheckForVisibleTargets()
    {
        spottedTargets.Clear(); // clear list of spotted targets, to prevent duplicating
        
        Collider2D[] targetsInViewRadius = Physics2D.OverlapCircleAll(transform.position, viewRadius, targetLayerMask); // an array which stores targets inside view radius

        if (targetsInViewRadius.Length >= 1) // if there is at least one target inside view radius
        {
            foreach (Collider2D col in targetsInViewRadius)
            {
                Transform target = col.transform;
                Vector3 directionToTarget = (target.position - transform.position).normalized;

                if (Vector3.Angle(transform.right, directionToTarget) < viewAngle * 0.5f) // if the target is inside view angle
                {
                    float distanceToTarget = Vector3.Distance(transform.position, target.position);

                    // when character don't collide with any object from obstacle layer mask during raycasting, it means, that target is spotted
                    if (!Physics2D.Raycast(transform.position, directionToTarget, distanceToTarget, obstacleLayerMask))
                    {
                        spottedTargets.Add(target);
                        targetSpotted = true;
                    }
                    else
                        targetSpotted = false;
                }
            }
        }
    }

    // checks for visible targets, with 0.2 sec delay for the better performance
    private IEnumerator VisibleTargetsCheckDelay()
    {
        float delayTime = 0.2f;
        
        while (true)
        {
            yield return new WaitForSeconds(delayTime);
            CheckForVisibleTargets();
        }
    }

}