using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Kellojo.ScopeShader.Editor
{
    [CustomEditor(typeof(ZoomedScope))]
    [CanEditMultipleObjects]
    public class ZoomedScopeEditor : UnityEditor.Editor {


        public override void OnInspectorGUI() {
            serializedObject.Update();


            EditorGUILayout.PropertyField(serializedObject.FindProperty("ScopeCamera"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_cameraFov"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("RenderTextureSize"));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("LensRenderer"));

            var enabledProp = serializedObject.FindProperty("AutoDisableBasedOnDistance");
            EditorGUILayout.PropertyField(enabledProp);
            EditorGUI.BeginDisabledGroup(!enabledProp.boolValue);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("MainCameraTransformOverride"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("DistanceCheckInterval"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("MaxDistance"));
            EditorGUI.EndDisabledGroup();


            serializedObject.ApplyModifiedProperties();
        }

    }
}
