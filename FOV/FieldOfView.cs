using System.Collections;
using SkalluUtils.PropertyAttributes; // download package via package manager https://github.com/Skallu0711/Skallu-Utils.git
using UnityEngine;

#if UNITY_EDITOR

using UnityEditor;
using UnityEditorInternal;

[CustomEditor(typeof(FieldOfView))]
public class FieldOfViewEditor : Editor
{
    private FieldOfView fov;
    private Vector3 fovPos;
    
    // use these two variables when character sprite is not centered
    private float offset = 0f;
    private Vector3 fixedFovPos;

    #region INSPECTOR FLAGS
    private bool showFovParameters = true; // is "FOV Parameters" segment unwrapped
    private bool showFovEditorParameters = true; // is "FOV Editor Parameters" segment unwrapped
    private bool showFovChecks; // is "FOV Zone Checks" segment unwrapped
    
    private bool useSpecialZones; // is "use special zones" toggled
    #endregion

    #region FOV EDITOR PARAMETERS
    private Color mainFovColor = Color.white; // view radius and view angle handles color
    private Color safeZoneColor = Color.yellow; // safe zone handles color
    private Color attackRangeColor = Color.red; // attack range handles color
    private float thickness = 0.5f; // handles thickness

    private readonly Color targetSpottedColor = Color.green;
    private readonly Color targetHiddenColor = Color.red;
    #endregion

    private void Awake() => fov = (FieldOfView) target;

    public override void OnInspectorGUI()
    {
        if (fov == null) return;

        // default "script" object field
        EditorGUI.BeginDisabledGroup(true); 
        EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((MonoBehaviour)target), GetType(), false);
        EditorGUI.EndDisabledGroup();

        // shows Field of View Parameters
        showFovParameters = EditorGUILayout.BeginFoldoutHeaderGroup(showFovParameters, "Field of View Parameters");
        if (showFovParameters)
        {
            fov.viewOuterRadius = EditorGUILayout.Slider(
                new GUIContent("Outer view radius", "Area that determines the ability to detect target within it, provided that it is also within the viewing angle cone"), 
                fov.viewOuterRadius, 0, 20);
            
            fov.viewInnerRadius = EditorGUILayout.Slider(
                new GUIContent("Inner view radius", "The minimum area that determines the ability to detect target within it"), 
                fov.viewInnerRadius, 0, 10);
            
            fov.viewAngle = EditorGUILayout.Slider(
                new GUIContent("View angle", "Angle (in degrees), which determines the ability to spot objects within its area"), 
                fov.viewAngle, 0, 360);
            
            EditorGUILayout.Space();
            
            // shows Field of View Special Zones Parameters
            useSpecialZones = EditorGUILayout.Toggle("Use special zones", useSpecialZones);
            if (EditorGUILayout.BeginFadeGroup(useSpecialZones ? 1 : 0))
            {
                EditorGUI.indentLevel++;
                
                EditorGUILayout.Space();
        
                fov.safeZoneRadius = EditorGUILayout.Slider(
                    new GUIContent("Safe zone radius", "Radius of an optional safe zone area"),
                    fov.safeZoneRadius, 0, 10);
                
                fov.attackRangeRadius = EditorGUILayout.Slider(
                    new GUIContent("Attack range radius", "Radius of an optional attack range area"),
                    fov.attackRangeRadius, 0, 10);
                
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFadeGroup();
            
            EditorGUILayout.Space();
            
            fov.zoneCheckInterval = EditorGUILayout.Slider(
                new GUIContent("Update interval", "Time interval between zone checks (i.e. fov update)"), 
                fov.zoneCheckInterval, 0.001f, 1);
            
            // layer mask field
            string[] layerNames = new string[32];
            for (int i = 0; i < layerNames.Length; i++)
            {
                layerNames[i] = LayerMask.LayerToName(i);
            }

            fov.obstacleLayerMask = EditorGUILayout.MaskField(
                new GUIContent("Obstacle layer mask", "Layer with all obstacles, which is used during circle cast. Enemy cannot see through obstacles"),
                fov.obstacleLayerMask, layerNames, EditorStyles.popup);
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        EditorGUILayout.Space();
        
        // shows Field of View Editor Parameters
        showFovEditorParameters = EditorGUILayout.BeginFoldoutHeaderGroup(showFovEditorParameters, "Field of View Editor Parameters");
        if (showFovEditorParameters)
        {
            thickness = EditorGUILayout.Slider(
                new GUIContent("Thickness", "Handles thickness"),
                thickness, 0.5f, 2);
            
            mainFovColor = EditorGUILayout.ColorField(
                new GUIContent("FOV main color", "Handles FOV main color"),
                mainFovColor);
            
            // shows Field of View Special Zones Editor Parameters
            if (EditorGUILayout.BeginFadeGroup(useSpecialZones ? 1 : 0))
            {
                EditorGUI.indentLevel++;
                
                EditorGUILayout.Space();
        
                safeZoneColor = EditorGUILayout.ColorField(
                    new GUIContent("FOV safe zone color", "Handles FOV safe zone color"),
                    safeZoneColor);
            
                attackRangeColor = EditorGUILayout.ColorField(
                    new GUIContent("FOV attack range color", "Handles FOV attack range color"),
                    attackRangeColor);
                
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFadeGroup();
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        EditorGUILayout.Space();
        
        // shows Field of View Zone Checks
        showFovChecks = EditorGUILayout.BeginFoldoutHeaderGroup(showFovChecks, "Field of View Zone Checks");
        if (showFovChecks)
        {
            EditorGUI.BeginDisabledGroup(true);
        
            fov.targetInsideViewOuterRadius = EditorGUILayout.Toggle("Target inside outer view radius", fov.targetInsideViewOuterRadius);
            fov.targetSpotted = EditorGUILayout.Toggle("Target spotted", fov.targetSpotted);
            
            // shows Field of View Special Zones Checks
            if (EditorGUILayout.BeginFadeGroup(useSpecialZones ? 1 : 0))
            {
                EditorGUI.indentLevel++;
                
                EditorGUILayout.Space();
                
                fov.targetInsideSafeZone = EditorGUILayout.Toggle("Target inside safe zone", fov.targetInsideSafeZone);
                fov.targetInsideAttackRange = EditorGUILayout.Toggle("Target inside attack range", fov.targetInsideAttackRange);
                
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFadeGroup();
        
            EditorGUI.EndDisabledGroup();
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        
        // repaint
        if (GUI.changed)
            InternalEditorUtility.RepaintAllViews();
    }

    private void OnSceneGUI()
    {
        if (fov == null) return;
        
        fovPos = fov.transform.position;
        
        /*
        // use this variable as center of wire arc (instead of fovPos) when sprite is not centered, otherwise keep it commented and use fovPos
        fixedFovPos = new Vector3(fovPos.x, fovPos.y, 0f) + fov.transform.TransformDirection(new Vector3(0f, offset, 0f));
        */

        DrawMainFov();
        
        DrawFovZone(fovPos, 360f, fov.safeZoneRadius, thickness, safeZoneColor); // draws safe zone radius
        DrawFovZone(fovPos, 360f, fov.attackRangeRadius, thickness, attackRangeColor); // draws attack range radius
    }

    /// <summary>
    /// Draws Main Field of View such as outer radius, inner radius and view angle cone
    /// </summary>
    private void DrawMainFov()
    {
        // draws outer and inner view radius
        DrawFovZone(fovPos, 360f, fov.viewOuterRadius, thickness, mainFovColor);
        DrawFovZone(fovPos, 360f, fov.viewInnerRadius, thickness, mainFovColor);
            
        // calculates and draws view angle cone
        Vector3 viewAngleLeft = CalculateDirectionFromAngle((-fov.viewAngle + 180) * 0.5f, fov.transform.eulerAngles.z); // left view angle: \|
        Vector3 viewAngleRight = CalculateDirectionFromAngle((fov.viewAngle + 180) * 0.5f, fov.transform.eulerAngles.z); // right view angle: |/
        
        Handles.color = mainFovColor;
        Handles.DrawLine(fovPos, fovPos + viewAngleLeft * fov.viewOuterRadius, thickness);
        Handles.DrawLine(fovPos, fovPos + viewAngleRight * fov.viewOuterRadius, thickness);
        
        // draws line from character to spotted target
        if (!fov.targetInsideViewOuterRadius) return;
        Handles.color = fov.targetSpotted ? targetSpottedColor : targetHiddenColor;
        Handles.DrawLine(fovPos, fov.target.transform.position, thickness);
    }

    /// <summary>
    /// Draws Field of View zone wire arc in certain color
    /// </summary>
    /// <param name="center"> center position </param>
    /// <param name="angle"> angle </param>
    /// <param name="radius"> radius </param>
    /// <param name="lineThickness"> handles thickness </param>
    /// <param name="color"> handles color </param>
    private void DrawFovZone(Vector3 center, float angle, float radius, float lineThickness, Color color)
    {
        Handles.color = color;
        Handles.DrawWireArc(center, Vector3.forward, Vector3.up, angle, radius, lineThickness);
    }

    /// <summary>
    /// Calculated direction from angle
    /// </summary>
    /// <param name="inputAngle"> provided angle </param>
    /// <param name="inputEulerAngleZ"> provided euler Z angle </param>
    /// <returns> Vector3 value which specifies direction from provided angle </returns>
    private Vector3 CalculateDirectionFromAngle(float inputAngle, float inputEulerAngleZ)
    {
        var newAngle = inputAngle - inputEulerAngleZ;
        var calculatedDirection = new Vector3(Mathf.Sin(newAngle * Mathf.Deg2Rad), Mathf.Cos(newAngle * Mathf.Deg2Rad), 0f);
        
        return calculatedDirection;
    }
}

#endif

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
    public LayerMask obstacleLayerMask; // Layer with all obstacles, which is used during circle cast. Enemy cannot see through obstacles
    public GameObject target; // Target object
    # endregion

    private void Awake() => target = PlayerController.self.gameObject; // set target here

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
        if (Vector2.SqrMagnitude(target.transform.position - transform.position) <= viewOuterRadius * viewOuterRadius) // when target is inside outer view radius
        {
            targetInsideViewOuterRadius = true;
            
            var directionToTarget = (target.transform.position - transform.position).normalized;
            var distanceToTarget = Vector2.Distance(transform.position, target.transform.position);

            if (Vector2.SqrMagnitude(target.transform.position - transform.position) <= viewInnerRadius * viewInnerRadius) // when target is inside inner view radius
            {
                // when raycast doesn't collide with any object from obstacle mask, it means, that target is spotted
                targetSpotted = !Physics2D.Raycast(transform.position, directionToTarget, distanceToTarget, obstacleLayerMask);
            }
            else
            {
                // when the target is inside view angle and raycast doesn't collide with any object from obstacle mask, it means, that target is spotted
                if (Vector3.Angle(transform.right, directionToTarget) < viewAngle * 0.5f)
                    targetSpotted = !Physics2D.Raycast(transform.position, directionToTarget, distanceToTarget, obstacleLayerMask);
                else
                    targetSpotted = false;
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
            var directionToTarget = (target.transform.position - transform.position).normalized;
            var distanceToTarget = Vector3.Distance(transform.position, target.transform.position);
            
            if (!Physics2D.Raycast(transform.position, directionToTarget, distanceToTarget, obstacleMask))
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