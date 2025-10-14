using System;
using UnityEditor;
using UnityEngine;
using System.Linq;
using UnityEditor.Rendering;

public abstract class ScopeShaderGUIBase : CustomShaderGUI {


    public enum Expandable : uint {
        ReticleLayers = 1 << 9,
        AdvancedSettings = 1 << 7,
        Custom1 = 1 << 11,
        Custom2 = 1 << 12,
        Custom3 = 1 << 13,
        Custom4 = 1 << 14,
        Custom5 = 1 << 15,
    }



    /// <summary>
    /// Draws the gui of the shader
    /// </summary>
    /// <param name="materialEditor"></param>
    /// <param name="properties"></param>
    /// <param name="material"></param>
    protected override void DrawGUI(MaterialEditor materialEditor, MaterialProperty[] properties, Material material) {
        DrawTitle(materialEditor, properties, material);
        DrawShaderProperties(materialEditor, properties, material);

        DrawFoldoutHeaderGroup((uint) Expandable.AdvancedSettings, "Advanced Settings", materialEditor, () => {
            materialEditor.RenderQueueField();
            materialEditor.EnableInstancingField();
            materialEditor.DoubleSidedGIField();
        });

        ValidateMaterial(material);
    }

    protected abstract void DrawShaderProperties(MaterialEditor materialEditor, MaterialProperty[] properties, Material material);




    /// <summary>
    /// Draws the title section of the shader
    /// </summary>
    /// <param name="materialEditor"></param>
    /// <param name="properties"></param>
    /// <param name="material"></param>
    protected void DrawTitle(MaterialEditor materialEditor, MaterialProperty[] properties, Material material) {
        EditorGUILayout.Space(64);

        GUIStyle headerStyle = new GUIStyle(EditorStyles.label);
        headerStyle.fontSize = 32;
        headerStyle.alignment = TextAnchor.UpperCenter;
        EditorGUILayout.LabelField(material.name, headerStyle, GUILayout.Height(40));

        GUIStyle subHeaderStyle = new GUIStyle(EditorStyles.label);
        subHeaderStyle.alignment = TextAnchor.UpperCenter;
        subHeaderStyle.fontSize = 16;
        EditorGUILayout.LabelField(material.shader.name.Replace("Shader Graphs/", "") + " Shader", subHeaderStyle, GUILayout.Height(20));
        EditorGUILayout.Space(64);
    }

    /// <summary>
    /// Draws a reticle layer
    /// </summary>
    /// <param name="materialEditor"></param>
    /// <param name="properties"></param>
    /// <param name="index"></param>
    protected void DrawReticleLayer(MaterialEditor materialEditor, MaterialProperty[] properties, int index) {

        var boxStyle = GUI.skin.box;
        boxStyle.margin.bottom = 0;
        EditorGUILayout.BeginHorizontal(boxStyle);
        GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"Layer {index}", headerStyle);
        EditorGUILayout.EndHorizontal();


        EditorGUILayout.BeginHorizontal();

        MaterialProperty textureProp = FindProperty($"_Reticle_Layer_{index}", properties);
        textureProp.textureValue = EditorGUILayout.ObjectField(textureProp.textureValue, typeof(Texture2D), false, GUILayout.Width(64), GUILayout.Height(64)) as Texture2D;
        EditorGUILayout.Space(1, false);

        var labelWidth = 90;
        var initialWidth = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = labelWidth;

        EditorGUILayout.BeginVertical();
        DrawSpaceOptimizedSlider(materialEditor, properties, $"_Reticle_Layer_{index}_Scale", "Scale", labelWidth);
        DrawSpaceOptimizedSlider(materialEditor, properties, $"_Reticle_Layer_{index}_Depth", "Depth", labelWidth);
        DrawSpaceOptimizedColorField(materialEditor, properties, $"_Reticle_Layer_{index}_Color", "Color", labelWidth);
        DrawSpaceOptimizedSlider(materialEditor, properties, $"_Reticle_Layer_{index}_Rotation", "Rotation", labelWidth);
        DrawSpaceOptimizedFloatField(materialEditor, properties, $"_Reticle_Layer_{index}_Rotation_Speed", "Rotation Speed", labelWidth);
        EditorGUILayout.EndVertical();

        

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(24);

        EditorGUIUtility.labelWidth = initialWidth;
    }


    /// <summary>
    /// Draws a shader property by it's display name
    /// </summary>
    /// <param name="materialEditor"></param>
    /// <param name="properties"></param>
    /// <param name="propertyName"></param>
    protected void DrawPropertyByName(MaterialEditor materialEditor, MaterialProperty[] properties, string propertyName, string displayNameOverride = null) {
        var prop = FindProperty(propertyName, properties);
        if (prop == null) {
            var material = materialEditor.target as Material;
            Debug.LogError($"Could not draw property '{propertyName}' for shader {material.name}");
            return;
        }

        materialEditor.ShaderProperty(prop, displayNameOverride != null ? displayNameOverride : prop.displayName);
    }
    /// <summary>
    /// Draws a slider where the label is taking up less space to optimize the editing experience
    /// </summary>
    /// <param name="materialEditor"></param>
    /// <param name="properties"></param>
    /// <param name="propertyName"></param>
    /// <param name="labelWidth"></param>
    /// <param name="displayNameOverride"></param>
    protected void DrawSpaceOptimizedSlider(MaterialEditor materialEditor, MaterialProperty[] properties, string propertyName, string displayNameOverride = null, float labelWidth = 80f) {
        MaterialProperty prop = FindProperty(propertyName, properties);

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label(displayNameOverride != null ? displayNameOverride : prop.displayName, GUILayout.Width(labelWidth));
        prop.floatValue = EditorGUILayout.Slider(prop.floatValue, prop.rangeLimits.x, prop.rangeLimits.y, GUILayout.ExpandWidth(true));
        EditorGUILayout.EndHorizontal();
    }
    /// <summary>
    /// Draws a float field where the label is taking up less space to optimize the editing experience
    /// </summary>
    /// <param name="materialEditor"></param>
    /// <param name="properties"></param>
    /// <param name="propertyName"></param>
    /// <param name="labelWidth"></param>
    /// <param name="displayNameOverride"></param>
    protected void DrawSpaceOptimizedFloatField(MaterialEditor materialEditor, MaterialProperty[] properties, string propertyName, string displayNameOverride = null, float labelWidth = 80f) {
        MaterialProperty prop = FindProperty(propertyName, properties);

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label(displayNameOverride != null ? displayNameOverride : prop.displayName, GUILayout.Width(labelWidth));
        prop.floatValue = EditorGUILayout.FloatField(prop.floatValue, GUILayout.ExpandWidth(true));
        EditorGUILayout.EndHorizontal();
    }
    /// <summary>
    /// Draws a float field where the label is taking up less space to optimize the editing experience
    /// </summary>
    /// <param name="materialEditor"></param>
    /// <param name="properties"></param>
    /// <param name="propertyName"></param>
    /// <param name="labelWidth"></param>
    /// <param name="displayNameOverride"></param>
    protected void DrawSpaceOptimizedColorField(MaterialEditor materialEditor, MaterialProperty[] properties, string propertyName, string displayNameOverride = null, float labelWidth = 80f) {
        MaterialProperty prop = FindProperty(propertyName, properties);

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label(displayNameOverride != null ? displayNameOverride : prop.displayName, GUILayout.Width(labelWidth));
        prop.colorValue = EditorGUILayout.ColorField(GUIContent.none, prop.colorValue, true, true, true, GUILayout.ExpandWidth(true));
        EditorGUILayout.EndHorizontal();
    }


    /// <summary>
    /// Draws a foldout header group
    /// </summary>
    /// <param name="foldout"></param>
    /// <param name="title"></param>
    /// <param name="material"></param>
    /// <param name="drawContent"></param>
    protected void DrawFoldoutHeaderGroup(uint expandableBit, string title, MaterialEditor editor, Action drawContent) {

        using (var header = new MaterialHeaderScope(title, expandableBit, editor)) {
            if (header.expanded) {
                drawContent();
            }
        }

    }

}
