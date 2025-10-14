using UnityEditor;
using UnityEngine;
using UnityEditor.Rendering;

public abstract class CustomShaderGUI : ShaderGUI {

    public CustomShaderGUI() {

    }

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties) {
        Material targetMat = materialEditor.target as Material;
        
        EditorGUI.BeginChangeCheck();
        DrawGUI(materialEditor, properties, targetMat);
        if (EditorGUI.EndChangeCheck()) {
            EditorUtility.SetDirty(targetMat);
        }
    }

    protected abstract void DrawGUI(MaterialEditor materialEditor, MaterialProperty[] properties, Material material);

    protected bool IsTransparentSurfaceType( MaterialProperty[] properties) {
        return true;
    }

}
