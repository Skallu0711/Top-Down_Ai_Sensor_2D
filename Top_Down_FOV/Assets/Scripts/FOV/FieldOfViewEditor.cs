/*
 * This is a simple editor tool, which visualises character's field of view in 2D top-down perspective
 */

#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FieldOfView))]
public class FieldOfViewEditor : Editor
{
    /*
    // USE THESE VARIABLES WHEN SPRITE IS NOT CENTERED. OTHERWISE KEEP THEM COMMENTED.
    
    private float offset = 0f; // set offset value
    private Vector3 fixedPosition;
    */

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
    }

    private void OnSceneGUI()
    {
        FieldOfView fov = (FieldOfView) target; // field of view script reference

        if (fov != null)
        {
            /*
            // USE THIS VARIABLE AS CENTER OF WIRE ARC (INSTEAD OF FOV POSITION) WHEN SPRITE IS NOT CENTERED. OTHERWISE KEEP IT COMMENTED AND USE FOV POSITION.
            
            fixedPosition = new Vector3(fov.transform.position.x, fov.transform.position.y, 0f) + fov.transform.TransformDirection(new Vector3(0f, offset, 0f));
            */

            // draws max view radius
            Handles.color = fov.handlesParameters.mainFovColor;
            Handles.DrawWireArc(fov.transform.position, Vector3.forward, Vector3.up, 360f, fov.viewOuterRadius, fov.handlesParameters.thickness);
            
            // draws min view radius
            //Handles.color = fov.handlesStuff.mainFovColor;;
            Handles.DrawWireArc(fov.transform.position, Vector3.forward, Vector3.up, 360f, fov.viewInnerRadius, fov.handlesParameters.thickness);
            
            // draws view angle cone
            Vector3 viewAngleLeft = CalculateDirectionFromAngle((-fov.viewAngle + 180) * 0.5f, fov.transform.eulerAngles.z); // left view angle: \|
            Vector3 viewAngleRight = CalculateDirectionFromAngle((fov.viewAngle + 180) * 0.5f, fov.transform.eulerAngles.z); // right view angle: |/

            //Handles.color = fov.handlesStuff.mainFovColor;;
            Handles.DrawLine(fov.transform.position, fov.transform.position + viewAngleLeft * fov.viewOuterRadius, fov.handlesParameters.thickness);
            Handles.DrawLine(fov.transform.position, fov.transform.position + viewAngleRight * fov.viewOuterRadius, fov.handlesParameters.thickness);
            
            // draws safe zone radius
            Handles.color = fov.handlesParameters.safeZoneColor;
            Handles.DrawWireArc(fov.transform.position, Vector3.forward, Vector3.up, 360f, fov.safeZoneRadius, fov.handlesParameters.thickness);

            // draws attack range radius
            Handles.color = fov.handlesParameters.attackRangeColor;;
            Handles.DrawWireArc(fov.transform.position, Vector3.forward, Vector3.up, 360f, fov.attackRangeRadius, fov.handlesParameters.thickness);

            // draws ray from character to spotted target
            if (fov.fovState.targetInsideViewOuterRadius)
            {
                Handles.color = fov.fovState.targetSpotted ? fov.handlesParameters.targetSpottedColor : fov.handlesParameters.targetHiddenColor;
                Handles.DrawLine(fov.transform.position, fov.target.transform.position, fov.handlesParameters.thickness);
            }
        }
    }
    
    private Vector3 CalculateDirectionFromAngle(float inputAngle, float inputEulerAngleZ)
    {
        var newAngle = inputAngle - inputEulerAngleZ;
        var calculatedAngle = new Vector3(Mathf.Sin(newAngle * Mathf.Deg2Rad), Mathf.Cos(newAngle * Mathf.Deg2Rad), 0f);
        
        return calculatedAngle;
    }

}

#endif