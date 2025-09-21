using System;

using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

using BestHTTP.Addons.cURLParser.Editor.Utils;

namespace BestHTTP.Addons.cURLParser.Editor
{
    public enum RequestUsageTypes : int
    {
        Callback,
        Coroutine,
        AsyncAwait
    }

    public class cURLParserWindow : EditorWindow
    {
        static cURLParserWindow openInstance;

        [MenuItem("Window/Best HTTP/Addons/cURL Parser/Parser Window %&r")]
        public static void ShowParserWindow()
        {
            if (openInstance == null)
            {
                openInstance = GetWindow<cURLParserWindow>("cURL Parser Addon");
                openInstance.Show();
            }
            else
                openInstance.Close();
        }

        public void OnEnable()
        {
            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;

            root.Clear();

            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(System.IO.Path.Combine(EditorHelper.GetRelativePluginFolder(), "Editor", "cURLParserWindow.uxml"));
            visualTree.CloneTree(root);

            root.Q<Button>("ParseButton")
#if UNITY_2019_3_OR_NEWER
                .clicked += OnParseButtonClicked;
#else
                .RegisterCallback<MouseUpEvent>(evt => OnParseButtonClicked());
#endif

            root.Q<Button>("copyToClipboard")
#if UNITY_2019_3_OR_NEWER
                .clicked += CopyToClipboard;
#else
                .RegisterCallback<MouseUpEvent>(evt => CopyToClipboard());
#endif

            root.Q<EnumField>("RequestUsageType").Init((RequestUsageTypes)EditorPrefs.GetInt("cURLParserWindow_RequestUsageType", (int)RequestUsageTypes.Callback));
            root.Q<TextField>("cURLCommand").value = EditorPrefs.GetString("cURLParserWindow_cURLCommand", "curl --data-urlencode \"name = I am Daniel\" http://www.example.com");

            string url = "https://benedicht.github.io/BestHTTP-Documentation/#8.Addons/cURLParser/";
            root.Q<Button>("HelpButton")
#if UNITY_2019_3_OR_NEWER
                .clicked += () => Application.OpenURL(url);
#else
                .RegisterCallback<MouseUpEvent>(evt => Application.OpenURL(url));
#endif
        }

        public void CopyToClipboard()
        {
            GUIUtility.systemCopyBuffer = this.rootVisualElement.Q<Label>("CSharpOutput").text;
        }

        public void OnDestroy()
        {
            openInstance = null;
        }

        public void OnParseButtonClicked()
        {
            // https://curl.se/docs/manpage.html
            // https://curl.se/docs/httpscripting.html

            // --trace-time

            try
            {
                var requestUsageType = (RequestUsageTypes)base.rootVisualElement.Q<EnumField>("RequestUsageType").value;
                var command = this.rootVisualElement.Q<TextField>("cURLCommand").value;

                EditorPrefs.SetInt("cURLParserWindow_RequestUsageType", (int)requestUsageType);
                EditorPrefs.SetString("cURLParserWindow_cURLCommand", command);

                this.rootVisualElement.Q<Label>("CSharpOutput").text = Generator.Generate(command, requestUsageType);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex, this);
            }
        }
    }
}
