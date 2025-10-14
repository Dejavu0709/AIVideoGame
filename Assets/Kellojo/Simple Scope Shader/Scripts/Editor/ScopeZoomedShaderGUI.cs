using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;


public class ScopeZoomedShaderGUI : ScopeShaderGUIBase {

    public const int NUMBER_OF_RETICLE_LAYERS = 10;

    protected override void DrawShaderProperties(MaterialEditor materialEditor, MaterialProperty[] properties, Material material) {
        DrawFoldoutHeaderGroup((uint) Expandable.ReticleLayers, "Reticle Layer Settings", materialEditor, () => {
            for (int i = 1; i <= NUMBER_OF_RETICLE_LAYERS; i++) {
                DrawReticleLayer(materialEditor, properties, i);
            }
        });

        DrawFoldoutHeaderGroup((uint) Expandable.Custom1, "Scene Color Settings", materialEditor, () => {
            DrawPropertyByName(materialEditor, properties, "_SceneColorMultiplier");
            DrawPropertyByName(materialEditor, properties, "_SceneDepth");
        });

        DrawFoldoutHeaderGroup((uint) Expandable.Custom2, "Vignette Settings", materialEditor, () => {
            DrawPropertyByName(materialEditor, properties, "_Vignette_Radius");
            DrawPropertyByName(materialEditor, properties, "_Vignette_Hardness");
            DrawPropertyByName(materialEditor, properties, "_Vignette_Depth_Multiplier");         
        });

        DrawFoldoutHeaderGroup((uint) Expandable.Custom3, "Reflection Settings", materialEditor, () => {
            DrawPropertyByName(materialEditor, properties, "_Reflection_Strength");
            DrawPropertyByName(materialEditor, properties, "_Reflection_Fresnel_Power");
        });
    }

}

