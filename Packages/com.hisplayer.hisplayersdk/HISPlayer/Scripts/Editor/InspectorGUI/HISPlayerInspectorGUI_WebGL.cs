using UnityEditor;
using UnityEngine;

public static class HISPlayerInspectorGUI_WebGL
{
    public static bool DrawWebGL(float x, float y, float width, SerializedProperty element, float spacing, bool isExpanded, out float newY)
    {
        Rect foldoutRect = new(x, y, width, EditorGUIUtility.singleLineHeight);
        isExpanded = EditorGUI.Foldout(foldoutRect, isExpanded, "Platform WebGL", true);
        y += EditorGUIUtility.singleLineHeight + 6;

        if (isExpanded)
        {
            float height;
            var ads = element.FindPropertyRelative("adsProperties");
            height = EditorGUI.GetPropertyHeight(ads, true);
            EditorGUI.PropertyField(new Rect(x, y, width, height), ads);
            y += height + spacing;

            var sb = element.FindPropertyRelative("startingBitrate");
            height = EditorGUI.GetPropertyHeight(sb, true);
            EditorGUI.PropertyField(new Rect(x, y, width, height), sb);
            y += height + spacing;

            var cb = element.FindPropertyRelative("customBitrate");
            height = EditorGUI.GetPropertyHeight(cb, true);
            cb.boolValue = EditorGUI.Toggle(new Rect(x, y, width, height), new GUIContent("Custom Bitrate"), cb.boolValue);
            y += height + spacing;

            if (cb.boolValue)
            {
                var br = element.FindPropertyRelative("trackBitrateRange");
                height = EditorGUI.GetPropertyHeight(br, true);
                EditorGUI.PropertyField(new Rect(x, y, width, height), br);
                y += height + spacing;
            }

            var cms = element.FindPropertyRelative("customMaxSize");
            height = EditorGUI.GetPropertyHeight(cms, true);
            cms.boolValue = EditorGUI.Toggle(new Rect(x, y, width, height), new GUIContent("Custom Max Size"), cms.boolValue);
            y += height + spacing;

            if (cms.boolValue)
            {
                var ms = element.FindPropertyRelative("resolutionMaxSize");
                height = EditorGUI.GetPropertyHeight(ms, true);
                EditorGUI.PropertyField(new Rect(x, y, width, height), ms);
                y += height + spacing;
            }

            var cmin = element.FindPropertyRelative("customMinSize");
            height = EditorGUI.GetPropertyHeight(cmin, true);
            cmin.boolValue = EditorGUI.Toggle(new Rect(x, y, width, height), new GUIContent("Custom Min Size"), cmin.boolValue);
            y += height + spacing;

            if (cmin.boolValue)
            {
                var min = element.FindPropertyRelative("resolutionMinSize");
                height = EditorGUI.GetPropertyHeight(min, true);
                EditorGUI.PropertyField(new Rect(x, y, width, height), min);
                y += height + spacing;
            }
        }

        newY = y;
        return isExpanded;
    }

    public static float GetWebGLHeight(SerializedProperty element, float spacing)
    {
        float total = 4;
        total += EditorGUI.GetPropertyHeight(element.FindPropertyRelative("adsProperties"), true) + spacing;
        total += EditorGUI.GetPropertyHeight(element.FindPropertyRelative("startingBitrate"), true) + spacing;

        var cb = element.FindPropertyRelative("customBitrate");
        total += EditorGUI.GetPropertyHeight(cb, true) + spacing;
        if (cb.boolValue)
            total += EditorGUI.GetPropertyHeight(element.FindPropertyRelative("trackBitrateRange"), true) + spacing;

        var cms = element.FindPropertyRelative("customMaxSize");
        total += EditorGUI.GetPropertyHeight(cms, true) + spacing;
        if (cms.boolValue)
            total += EditorGUI.GetPropertyHeight(element.FindPropertyRelative("resolutionMaxSize"), true) + spacing;

        var cmin = element.FindPropertyRelative("customMinSize");
        total += EditorGUI.GetPropertyHeight(cmin, true) + spacing;
        if (cmin.boolValue)
            total += EditorGUI.GetPropertyHeight(element.FindPropertyRelative("resolutionMinSize"), true) + spacing;

        return total;
    }
}
