using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Kellojo.ScopeShader.Editor
{
    public static class MenuItems
    {
        
        [MenuItem("Assets/Create/Simple Scope Shader/Reflex Sight Material")]
        public static void CreateReflexSightMaterial() {
            CreateNewMaterialAtCurrentPath("Shader Graphs/Reflex Sight", "New Reflex Sight");
        }

        [MenuItem("Assets/Create/Simple Scope Shader/Zoomed Scope Material")]
        public static void CreateZoomedScopeMaterial() {
            CreateNewMaterialAtCurrentPath("Shader Graphs/Zoomed Scope", "New Zoomed Scope");
        }

        public static void CreateNewMaterialAtCurrentPath(string shaderName, string assetName) {
            Material material = new Material(Shader.Find(shaderName));
            AssetDatabase.CreateAsset(material, $"{GetCurrentlyOpenedProjectPath()}/{assetName}.mat");
            AssetDatabase.SaveAssets();
        }

        public static string GetCurrentlyOpenedProjectPath() {
            if (Selection.assetGUIDs.Length > 0) return AssetDatabase.GUIDToAssetPath(Selection.assetGUIDs.First());
            return "Assets";
        }

    }
}
