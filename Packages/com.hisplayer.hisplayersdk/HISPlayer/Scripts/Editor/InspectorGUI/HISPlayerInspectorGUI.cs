using HISPlayerAPI;
using UnityEditor;
using UnityEngine;
using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEditorInternal;
using System.Diagnostics;

[CustomEditor(typeof(HISPlayerManager), true)]
public class HISPlayerInspectorGUI : Editor
{
    private ReorderableList multiStreamReorderableList;

    private Dictionary<string, bool> urlExpandedStates = new Dictionary<string, bool>();
    private Dictionary<string, bool> urlMimeTypesExpandedStates = new Dictionary<string, bool>();
    private Dictionary<string, bool> keyServerURIExpandedStates = new Dictionary<string, bool>();
    private Dictionary<string, bool> DRMTokensExpandedStates = new Dictionary<string, bool>();
    private Dictionary<string, bool> webglFoldoutStates = new Dictionary<string, bool>();

    private void OnEnable()
    {
        var manager = (HISPlayerManager)target;

        multiStreamReorderableList = new ReorderableList(serializedObject, serializedObject.FindProperty("multiStreamProperties"), true, true, true, true);

        multiStreamReorderableList.drawHeaderCallback = (Rect rect) =>
        {
            EditorGUI.LabelField(rect, "Multi Stream Properties");
        };

        multiStreamReorderableList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            SerializedProperty element = multiStreamReorderableList.serializedProperty.GetArrayElementAtIndex(index);
            rect.y += 2;

            string idValue = element.FindPropertyRelative("id").stringValue;

            Rect foldoutRect = new Rect(rect.x + 10, rect.y, rect.width - 10, EditorGUIUtility.singleLineHeight);
            element.isExpanded = EditorGUI.Foldout(foldoutRect, element.isExpanded, $"Element {index}", true);
            
            if (element.isExpanded)
            {
                float yOffset = rect.y + EditorGUIUtility.singleLineHeight + 4;
                float lineHeight = EditorGUIUtility.singleLineHeight + 2;

                var renderMode = element.FindPropertyRelative("renderMode");
                var material = element.FindPropertyRelative("material");
                var rawImage = element.FindPropertyRelative("rawImage");
                var renderTexture = element.FindPropertyRelative("renderTexture");

                EditorGUI.PropertyField(new Rect(rect.x, yOffset, rect.width, lineHeight), renderMode);
                yOffset += lineHeight;

                int modeValue = renderMode.enumValueIndex;
                switch ((HISPlayerRenderMode)modeValue)
                {
                    case HISPlayerRenderMode.RenderTexture:
                        EditorGUI.PropertyField(new Rect(rect.x, yOffset, rect.width, lineHeight), renderTexture);
                        yOffset += lineHeight + 6;
                        break;
                    case HISPlayerRenderMode.Material:
                        EditorGUI.PropertyField(new Rect(rect.x, yOffset, rect.width, lineHeight), material);
                        yOffset += lineHeight + 6;
                        break;
                    case HISPlayerRenderMode.RawImage:
                        EditorGUI.PropertyField(new Rect(rect.x, yOffset, rect.width, lineHeight), rawImage);
                        yOffset += lineHeight + 6;
                        break;
                    case HISPlayerRenderMode.NONE:
                        yOffset += 4;
                        break;
                }

                var url = element.FindPropertyRelative("url");
                if (url != null)
                {
                    string urlKey = idValue + ".url";

                    if (!urlExpandedStates.ContainsKey(urlKey))
                        urlExpandedStates[urlKey] = url.isExpanded;

                    url.isExpanded = urlExpandedStates[urlKey];

                    float urlHeight = EditorGUI.GetPropertyHeight(url, url.isExpanded);
                    EditorGUI.PropertyField(new Rect(rect.x, yOffset, rect.width, urlHeight), url, true);
                    yOffset += urlHeight + 2;

                    urlExpandedStates[urlKey] = url.isExpanded;
                }

                var urlMimeTypes = element.FindPropertyRelative("urlMimeTypes");
                if (urlMimeTypes != null)
                {
                    string urlMimeKey = idValue + ".urlMimeTypes";

                    if (!urlMimeTypesExpandedStates.ContainsKey(urlMimeKey))
                        urlMimeTypesExpandedStates[urlMimeKey] = urlMimeTypes.isExpanded;

                    urlMimeTypes.isExpanded = urlMimeTypesExpandedStates[urlMimeKey];

                    float urlMimeHeight = EditorGUI.GetPropertyHeight(urlMimeTypes, urlMimeTypes.isExpanded);
                    EditorGUI.PropertyField(new Rect(rect.x, yOffset, rect.width, urlMimeHeight), urlMimeTypes, true);
                    yOffset += urlMimeHeight + 2;

                    urlMimeTypesExpandedStates[urlMimeKey] = urlMimeTypes.isExpanded;
                }

                var autoPlay = element.FindPropertyRelative("autoPlay");
                if (autoPlay != null)
                {
                    EditorGUI.PropertyField(new Rect(rect.x, yOffset, rect.width, lineHeight), autoPlay);
                    yOffset += lineHeight;
                }

                var loopPlayback = element.FindPropertyRelative("loopPlayback");
                if (loopPlayback != null)
                {
                    EditorGUI.PropertyField(new Rect(rect.x, yOffset, rect.width, lineHeight), loopPlayback);
                    yOffset += lineHeight;
                }

                var autoTransition = element.FindPropertyRelative("autoTransition");
                if (autoTransition != null)
                {
                    EditorGUI.PropertyField(new Rect(rect.x, yOffset, rect.width, lineHeight), autoTransition);
                    yOffset += lineHeight;
                }

                EditorGUI.LabelField(new Rect(rect.x, yOffset, rect.width, lineHeight), "Unity Audio Output (Platform: Android)", EditorStyles.boldLabel);
                yOffset += lineHeight + 1;

                var unityAudio = element.FindPropertyRelative("unityAudio");
                if (unityAudio != null)
                {
                    EditorGUI.PropertyField(new Rect(rect.x, yOffset, rect.width, lineHeight), unityAudio);
                    yOffset += lineHeight;
                }

                var enableDRM = element.FindPropertyRelative("enableDRM");
                if (enableDRM != null)
                {
                    EditorGUI.LabelField(new Rect(rect.x, yOffset, rect.width, lineHeight), "DRM (Digital Rights Management) (Platforms: Android, iOS)", EditorStyles.boldLabel);
                    yOffset += lineHeight + 1;

                    EditorGUI.PropertyField(new Rect(rect.x, yOffset, rect.width, lineHeight), enableDRM);
                    yOffset += lineHeight + 2;

                    if (enableDRM.boolValue)
                    {
                        var keyServerURI = element.FindPropertyRelative("keyServerURI");
                        if (keyServerURI != null)
                        {
                            string keyServerKey = idValue + ".keyServerURI";

                            if (!keyServerURIExpandedStates.ContainsKey(keyServerKey))
                                keyServerURIExpandedStates[keyServerKey] = keyServerURI.isExpanded;

                            keyServerURI.isExpanded = keyServerURIExpandedStates[keyServerKey];

                            float keyServerHeight = EditorGUI.GetPropertyHeight(keyServerURI, keyServerURI.isExpanded);
                            EditorGUI.PropertyField(new Rect(rect.x, yOffset, rect.width, keyServerHeight), keyServerURI, true);
                            yOffset += keyServerHeight + 2;

                            keyServerURIExpandedStates[keyServerKey] = keyServerURI.isExpanded;
                        }

                        var DRMTokens = element.FindPropertyRelative("DRMTokens");
                        if (DRMTokens != null)
                        {
                            string drmTokensKey = idValue + ".DRMTokens";

                            if (!DRMTokensExpandedStates.ContainsKey(drmTokensKey))
                                DRMTokensExpandedStates[drmTokensKey] = DRMTokens.isExpanded;

                            DRMTokens.isExpanded = DRMTokensExpandedStates[drmTokensKey];

                            float drmTokensHeight = EditorGUI.GetPropertyHeight(DRMTokens, DRMTokens.isExpanded);
                            EditorGUI.PropertyField(new Rect(rect.x, yOffset, rect.width, drmTokensHeight), DRMTokens, true);
                            yOffset += drmTokensHeight + 6;

                            DRMTokensExpandedStates[drmTokensKey] = DRMTokens.isExpanded;
                        }
                    }
                }

                string webglKey = idValue + ".webgl";

                if (!webglFoldoutStates.ContainsKey(webglKey))
                    webglFoldoutStates[webglKey] = false;

                bool isExpandedWebGL = webglFoldoutStates[webglKey];
                isExpandedWebGL = HISPlayerInspectorGUI_WebGL.DrawWebGL(rect.x, yOffset, rect.width, element, 2, isExpandedWebGL, out float newYOffset);
                webglFoldoutStates[webglKey] = isExpandedWebGL;
            }
        };

        multiStreamReorderableList.elementHeightCallback = (int index) =>
        {
            SerializedProperty element = multiStreamReorderableList.serializedProperty.GetArrayElementAtIndex(index);
            if (!element.isExpanded)
                return EditorGUIUtility.singleLineHeight + 4;

            string idValue = element.FindPropertyRelative("id").stringValue;

            SerializedProperty renderMode = element.FindPropertyRelative("renderMode");
            int modeValue = renderMode.enumValueIndex;

            float height = EditorGUIUtility.singleLineHeight * 2 + 6;

            if ((HISPlayerRenderMode)modeValue != HISPlayerRenderMode.NONE)
                height += EditorGUIUtility.singleLineHeight + 2 + 6;

            var url = element.FindPropertyRelative("url");
            if (url != null)
            {
                string urlKey = idValue + ".url"; 
                bool isExpanded = urlExpandedStates.ContainsKey(urlKey) ? urlExpandedStates[urlKey] : url.isExpanded;
                height += EditorGUI.GetPropertyHeight(url, isExpanded) + 2;
            }

            var urlMimeTypes = element.FindPropertyRelative("urlMimeTypes");
            if (urlMimeTypes != null)
            {
                string urlMimeKey = idValue + ".urlMimeTypes";
                bool isExpanded = urlMimeTypesExpandedStates.ContainsKey(urlMimeKey) ? urlMimeTypesExpandedStates[urlMimeKey] : urlMimeTypes.isExpanded;
                height += EditorGUI.GetPropertyHeight(urlMimeTypes, isExpanded) + 2;
            }

            var autoPlay = element.FindPropertyRelative("autoPlay");
            if (autoPlay != null)
                height += EditorGUIUtility.singleLineHeight + 2;

            var loopPlayback = element.FindPropertyRelative("loopPlayback");
            if (loopPlayback != null)
                height += EditorGUIUtility.singleLineHeight + 2;

            var autoTransition = element.FindPropertyRelative("autoTransition");
            if (autoTransition != null)
                height += EditorGUIUtility.singleLineHeight + 2;

            var unityAudio = element.FindPropertyRelative("unityAudio");
            if (unityAudio != null)
                height += EditorGUIUtility.singleLineHeight + 2;

            var enableDRM = element.FindPropertyRelative("enableDRM");
            if (enableDRM != null)
            {
                height += EditorGUIUtility.singleLineHeight + 4;

                if (enableDRM.boolValue)
                {
                    var keyServerURI = element.FindPropertyRelative("keyServerURI");
                    if (keyServerURI != null)
                    {
                        string keyServerKey = idValue + ".keyServerURI";
                        bool isExpanded = keyServerURIExpandedStates.ContainsKey(keyServerKey) ? keyServerURIExpandedStates[keyServerKey] : keyServerURI.isExpanded;
                        height += EditorGUI.GetPropertyHeight(keyServerURI, isExpanded) + 2;
                    }

                    var DRMTokens = element.FindPropertyRelative("DRMTokens");
                    if (DRMTokens != null)
                    {
                        string drmTokensKey = idValue + ".DRMTokens";
                        bool isExpanded = DRMTokensExpandedStates.ContainsKey(drmTokensKey) ? DRMTokensExpandedStates[drmTokensKey] : DRMTokens.isExpanded;
                        height += EditorGUI.GetPropertyHeight(DRMTokens, isExpanded) + 6;
                    }
                }
            }

            string webglKey = idValue + ".webgl";

            if (!webglFoldoutStates.ContainsKey(webglKey))
                webglFoldoutStates[webglKey] = false;

            bool isExpandedWebGL = webglFoldoutStates[webglKey];

            height += EditorGUIUtility.singleLineHeight + 25;

            if (isExpandedWebGL)
            {
                height += HISPlayerInspectorGUI_WebGL.GetWebGLHeight(element, 2);
            }

            return height;
        };
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        System.Type baseType = typeof(HISPlayerManager);

        List<SerializedProperty> baseProperties = new();
        List<SerializedProperty> derivedProperties = new();

        SerializedProperty iterator = serializedObject.GetIterator();
        bool enterChildren = true;

        while (iterator.NextVisible(enterChildren))
        {
            enterChildren = false;

            if (iterator.name == "m_Script")
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.PropertyField(iterator, true);
                EditorGUI.EndDisabledGroup();
                continue;
            }

            if (iterator.name == "multiStreamProperties")
                continue;

            var fieldInfo = baseType.GetField(iterator.name, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            if (fieldInfo != null && fieldInfo.DeclaringType == baseType)
            {
                baseProperties.Add(iterator.Copy());
            }
            else
            {
                derivedProperties.Add(iterator.Copy());
            }
        }

        foreach (var prop in baseProperties)
        {
            EditorGUILayout.PropertyField(prop, true);
        }

        multiStreamReorderableList.DoLayoutList();

        foreach (var prop in derivedProperties)
        {
            EditorGUILayout.PropertyField(prop, true);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
