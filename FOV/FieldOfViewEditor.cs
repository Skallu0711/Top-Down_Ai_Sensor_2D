#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

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
            
            fov.castRadius = EditorGUILayout.Slider(
                new GUIContent("CircleCast radius", "CircleCast radius value"), 
                fov.castRadius, 0, 2);

            fov.obstacleLayerMask = EditorGUILayout.LayerField(
                new GUIContent("Obstacle layer mask", "Layer with all obstacles, which is used during circle cast. Enemy cannot see through obstacles"),
                fov.obstacleLayerMask);
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
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
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
        if (fov.targetInsideViewOuterRadius)
        {
            Handles.color = fov.targetSpotted ? targetSpottedColor : targetHiddenColor;
            Handles.DrawLine(fovPos, fov.target.transform.position, thickness);
        }
    }

    /// <summary>
    /// Draws Field of View zone wire arc in certain color
    /// </summary>
    /// <param name="center"> center position </param>
    /// <param name="angle"> angle </param>
    /// <param name="radius"> radius </param>
    /// <param name="thickness"> handles thickness </param>
    /// <param name="color"> handles color </param>
    private void DrawFovZone(Vector3 center, float angle, float radius, float thickness, Color color)
    {
        Handles.color = color;
        Handles.DrawWireArc(center, Vector3.forward, Vector3.up, angle, radius, thickness);
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