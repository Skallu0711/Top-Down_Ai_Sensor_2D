/*
 * This is a simple editor tool, which visualises character's field of view in 2D top-down perspective
 */

using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FieldOfView))]
public class FieldOfViewVisualiser : Editor
{ 
    private void OnSceneGUI()
    {
        FieldOfView fieldOfView = (FieldOfView) target; // field of view script reference

        // draws view radius
        Handles.color = fieldOfView.fovHandlesColor;
        Handles.DrawWireArc(fieldOfView.transform.position, Vector3.forward, Vector3.up, 360f, fieldOfView.viewRadius, fieldOfView.handlesThickness);
        
        // draws view angle cone
        Vector3 viewAngleLeft = CalculateDirectionFromAngle((-fieldOfView.viewAngle + 180) * 0.5f, fieldOfView.transform.eulerAngles.z); // left view angle: \|
        Vector3 viewAngleRight = CalculateDirectionFromAngle((fieldOfView.viewAngle + 180) * 0.5f, fieldOfView.transform.eulerAngles.z); // right view angle: |/

        Handles.color = fieldOfView.fovHandlesColor;
        Handles.DrawLine(fieldOfView.transform.position, fieldOfView.transform.position + viewAngleLeft * fieldOfView.viewRadius, fieldOfView.handlesThickness);
        Handles.DrawLine(fieldOfView.transform.position, fieldOfView.transform.position + viewAngleRight * fieldOfView.viewRadius, fieldOfView.handlesThickness);
        
        // draws ray from character to every spotted target
        Handles.color = fieldOfView.spottedTargetHandlesColor;

        foreach (Transform spottedTarget in fieldOfView.spottedTargets)
            Handles.DrawLine(fieldOfView.transform.position, spottedTarget.position, fieldOfView.handlesThickness);
    }
    
    private Vector3 CalculateDirectionFromAngle(float inputAngle, float eulerAngleZ)
    {
        inputAngle -= eulerAngleZ;

        Vector3 calculatedAngle = new Vector3(Mathf.Sin(inputAngle * Mathf.Deg2Rad), Mathf.Cos(inputAngle * Mathf.Deg2Rad), 0f);
        return calculatedAngle;
    }

}