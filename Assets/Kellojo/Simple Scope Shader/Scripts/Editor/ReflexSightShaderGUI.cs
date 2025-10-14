using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;


public class ReflexSightShaderGUI : ScopeShaderGUIBase {

    public const int NUMBER_OF_RETICLE_LAYERS = 10;



    protected override void DrawShaderProperties(MaterialEditor materialEditor, MaterialProperty[] properties, Material material) {

        if (!IsTransparentSurfaceType(properties)) {
            EditorGUILayout.HelpBox("The material surface type is currently set to 'Opaque', which is going to cause issues rendering the sight. Please change the type to 'Transparent' instead.", MessageType.Error);
            EditorGUILayout.Space(16);
        }
        
        DrawFoldoutHeaderGroup((uint) Expandable.ReticleLayers, "Reticle Layer Settings", materialEditor, () => {
            for (int i = 1; i <= NUMBER_OF_RETICLE_LAYERS; i++) {
                DrawReticleLayer(materialEditor, properties, i);
            }
        });


        DrawFoldoutHeaderGroup((uint) Expandable.Custom1, "Glass Settings", materialEditor, () => {
            DrawPropertyByName(materialEditor, properties, "_Glass");
            DrawPropertyByName(materialEditor, properties, "_Glass_Opacity_Multiplier");
            EditorGUILayout.Space(32);

            DrawPropertyByName(materialEditor, properties, "_Smoothness");
            DrawPropertyByName(materialEditor, properties, "_Smoothness_Multiplier");
            EditorGUILayout.Space(32);

            DrawPropertyByName(materialEditor, properties, "_Metallic");
            DrawPropertyByName(materialEditor, properties, "_Metallic_Multiplier");
            EditorGUILayout.Space(32);

            DrawPropertyByName(materialEditor, properties, "_Ambient_Occlusion");
        });
    }
}

