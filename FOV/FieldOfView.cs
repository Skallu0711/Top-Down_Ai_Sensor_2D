using System.Collections;
using SkalluUtils.PropertyAttributes; // download package via package manager https://github.com/Skallu0711/Skallu-Utils.git
using UnityEngine;

#if UNITY_EDITOR

using UnityEditor;
using UnityEditorInternal;

[CustomEditor(typeof(FieldOfView)), CanEditMultipleObjects]
public class FieldOfViewEditor : Editor
{
    private FieldOfView fov;

    // use these two variables when character sprite is not centered
    private float offset = 0f;
    private Vector3 fixedFovPos;

    #region GROUP WRAPPING RELATED FIELDS
    private bool showFovParameters = true; // is "FOV Parameters" foldout header group unwrapped
    private bool showFovEditorParameters = true; // is "FOV Editor Parameters" foldout header group unwrapped
    private bool showFovChecks; // is "FOV Zone Checks" foldout header group unwrapped
    private SerializedProperty useSpecialZones;
    #endregion

    private void OnEnable()
    {
        fov = (FieldOfView) target;
        useSpecialZones = serializedObject.FindProperty("useSpecialZones");
    }

    /// <summary>
    /// Creates slider field for serialized property
    /// </summary>
    /// <param name="property"> serialized property </param>
    /// <param name="leftValue"> min value </param>
    /// <param name="rightValue"> max value </param>
    /// <param name="label"> label (name, tooltip) </param>
    private void PropertySliderField(SerializedProperty property, float leftValue, float rightValue, GUIContent label)
    {
        var position = EditorGUILayout.GetControlRect();
        
        label = EditorGUI.BeginProperty(position, label, property);
        
        EditorGUI.BeginChangeCheck();
        var newValue = EditorGUI.Slider(position, label, property.floatValue, leftValue, rightValue);
        
        if (EditorGUI.EndChangeCheck())
            property.floatValue = newValue;
        
        EditorGUI.EndProperty();
    }

    public override void OnInspectorGUI()
    {
        if (fov == null) return;
        
        // default "script" object field
        EditorGUI.BeginDisabledGroup(true); 
        EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((MonoBehaviour)target), GetType(), false);
        EditorGUI.EndDisabledGroup();
        
        serializedObject.Update();
        EditorGUILayout.BeginVertical();
        
        #region FOV PARAMETERS GROUP
        showFovParameters = EditorGUILayout.BeginFoldoutHeaderGroup(showFovParameters, "Main parameters");
        if (showFovParameters)
        {
            PropertySliderField(serializedObject.FindProperty("viewOuterRadius"), 0, 20,
                new GUIContent("Outer view radius", "Area that determines the ability to detect target within it, provided that it is also within the viewing angle cone"));
            
            PropertySliderField(serializedObject.FindProperty("viewInnerRadius"), 0, 10,
                new GUIContent("Inner view radius", "The minimum area that determines the ability to detect target within it"));
            
            PropertySliderField(serializedObject.FindProperty("viewAngle"), 0, 360,
                new GUIContent("View angle", "Angle (in degrees), which determines the ability to spot objects within its area"));

            // shows "Special Zones" main parameters
            EditorGUILayout.PropertyField(useSpecialZones);
            if (EditorGUILayout.BeginFadeGroup(useSpecialZones.boolValue ? 1 : 0))
            {
                PropertySliderField(serializedObject.FindProperty("safeZoneRadius"), 0, 10,
                    new GUIContent("Safe zone radius", "Radius of an optional safe zone area"));

                PropertySliderField(serializedObject.FindProperty("attackRangeRadius"), 0, 10,
                    new GUIContent("Attack range radius", "Radius of an optional attack range area"));
            }
            EditorGUILayout.EndFadeGroup();
            EditorGUILayout.Space();

            PropertySliderField(serializedObject.FindProperty("zoneCheckInterval"), 0.001f, 1,
                new GUIContent("Update interval", "Time interval between zone checks (i.e. fov update)"));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("obstacleLayerMask"));
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        EditorGUILayout.Space();
        #endregion

        #region EDITOR PARAMETERS GROUP
        showFovEditorParameters = EditorGUILayout.BeginFoldoutHeaderGroup(showFovEditorParameters, "Visual Parameters");
        if (showFovEditorParameters)
        {
            PropertySliderField(serializedObject.FindProperty("thickness"), 0.5f, 2,
                new GUIContent("Thickness", "Handles thickness"));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("mainFovColor"));

            // shows "Special Zones" visual parameters
            if (EditorGUILayout.BeginFadeGroup(useSpecialZones.boolValue ? 1 : 0))
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("safeZoneColor"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("attackRangeColor"));
            }
            EditorGUILayout.EndFadeGroup();
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        EditorGUILayout.Space();
        #endregion

        #region ZONE CHECKS GROUP
        showFovChecks = EditorGUILayout.BeginFoldoutHeaderGroup(showFovChecks, "Zones Check Debug");
        if (showFovChecks)
        {
            EditorGUI.BeginDisabledGroup(true);
        
            fov.targetInsideViewOuterRadius = EditorGUILayout.Toggle("Target inside outer view radius", fov.targetInsideViewOuterRadius);
            fov.targetSpotted = EditorGUILayout.Toggle("Target spotted", fov.targetSpotted);
            
            // shows "Special Zones" debug checks
            if (EditorGUILayout.BeginFadeGroup(useSpecialZones.boolValue ? 1 : 0))
            {
                fov.targetInsideSafeZone = EditorGUILayout.Toggle("Target inside safe zone", fov.targetInsideSafeZone);
                fov.targetInsideAttackRange = EditorGUILayout.Toggle("Target inside attack range", fov.targetInsideAttackRange);
            }
            EditorGUILayout.EndFadeGroup();
            EditorGUI.EndDisabledGroup();
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        #endregion

        // apply modified properties and repaint
        EditorGUILayout.EndVertical();
        serializedObject.ApplyModifiedProperties();
        
        if (GUI.changed)
            InternalEditorUtility.RepaintAllViews();
    }

    private void OnSceneGUI()
    {
        if (fov == null) return;

        var fovPos = fov.transform.position;
        var thickness = fov.thickness;

        /*
        // use this variable as center of wire arc (instead of fovPos) when sprite is not centered, otherwise keep it commented and use fovPos
        fixedFovPos = new Vector3(fovPos.x, fovPos.y, 0f) + fov.transform.TransformDirection(new Vector3(0f, offset, 0f));
        */
        
        // draws outer and inner view radius
        DrawFovZone(fovPos, 360f, fov.viewOuterRadius, thickness, fov.mainFovColor);
        DrawFovZone(fovPos, 360f, fov.viewInnerRadius, thickness, fov.mainFovColor);

        // calculates and draws view angle cone
        Vector3 viewAngleLeft = CalculateDirectionFromAngle((-fov.viewAngle + 180) * 0.5f, fov.transform.eulerAngles.z); // left view angle: \|
        Vector3 viewAngleRight = CalculateDirectionFromAngle((fov.viewAngle + 180) * 0.5f, fov.transform.eulerAngles.z); // right view angle: |/
        
        Handles.color = fov.mainFovColor;
        Handles.DrawLine(fovPos, fovPos + viewAngleLeft * fov.viewOuterRadius, fov.thickness);
        Handles.DrawLine(fovPos, fovPos + viewAngleRight * fov.viewOuterRadius, fov.thickness);
        
        // draws special zones if used
        if (useSpecialZones.boolValue is true)
        {
            DrawFovZone(fovPos, 360f, fov.safeZoneRadius, thickness, fov.safeZoneColor); // draws safe zone radius
            DrawFovZone(fovPos, 360f, fov.attackRangeRadius, thickness, fov.attackRangeColor); // draws attack range radius
        }
        
        // draws line from character to spotted target
        if (fov.targetInsideViewOuterRadius)
        {
            Handles.color = fov.targetSpotted ? fov.targetSpottedColor : fov.targetHiddenColor;
            Handles.DrawLine(fovPos, fov.target.transform.position, thickness);
        }
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
    private Vector3 CalculateDirectionFromAngle(float inputAngle, float inputEulerAngleZ)
    {
        var newAngle = inputAngle - inputEulerAngleZ;
        return new Vector3(Mathf.Sin(newAngle * Mathf.Deg2Rad), Mathf.Cos(newAngle * Mathf.Deg2Rad), 0f);
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

    public bool useSpecialZones;
    public float safeZoneRadius; // Radius of an optional safe zone area
    public float attackRangeRadius; // Radius of an optional attack range area
    
    public float zoneCheckInterval = 0.02f; // Time interval between zone checks (i.e. fov update)
    public LayerMask obstacleLayerMask; // Layer with all obstacles, which is used during circle cast. Enemy cannot see through obstacles
    public GameObject target; // Target object
    # endregion
    
    #region FOV EDITOR VISUAL PARAMETERS
    public Color mainFovColor = Color.white; // view radius and view angle handles color
    public Color safeZoneColor = Color.yellow; // safe zone handles color
    public Color attackRangeColor = Color.red; // attack range handles color
    public float thickness = 0.5f; // handles thickness

    public readonly Color targetSpottedColor = Color.green;
    public readonly Color targetHiddenColor = Color.red;
    #endregion

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
            if (useSpecialZones)
            {
                if (safeZoneRadius > 0) 
                    CheckSafeZone();
                if (attackRangeRadius > 0)
                    CheckAttackRange();
            }
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