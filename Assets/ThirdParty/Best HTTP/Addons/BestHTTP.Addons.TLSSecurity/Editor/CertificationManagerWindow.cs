using System;
using System.IO;
using System.Linq;

using BestHTTP.Addons.TLSSecurity.Editor.Utils;

using UnityEditor;
using UnityEditor.UIElements;

using UnityEngine;
using UnityEngine.UIElements;

namespace BestHTTP.Addons.TLSSecurity.Editor
{
    public sealed class CertificationWindowBinding : ScriptableObject
    {
        public string testRequestStatus;
    }

    public sealed class CertificationManagerWindow : EditorWindow
    {
        CertificationWindowBinding windowBinding;
        SerializedObject serializedWindowBinding;
        static CertificationManagerWindow openInstance;

        [MenuItem("Window/Best HTTP/Addons/TLS Security/Certification Manager %&e")]
        static void OpenWindow()
        {
            if (openInstance == null)
            {
                openInstance = GetWindow<CertificationManagerWindow>("Certification Manager");
                openInstance.minSize = new Vector2(1240, 612);
            }

            openInstance.Show();
        }

        [MenuItem("Window/Best HTTP/Addons/TLS Security/Close Certification Manager %&w")]
        static void CloseWindow()
        {
#if UNITY_2019_3_OR_NEWER
            if (HasOpenInstances<CertificationManagerWindow>())
                GetWindow<CertificationManagerWindow>().Close();
#else
            if (openInstance != null)
                openInstance.Close();
#endif
        }

        [MenuItem("Window/Best HTTP/Addons/TLS Security/Unload Databases from Memory %&u")]
        static void UnloadDatabases()
        {
            TLSSecurity.UnloadDatabases();
        }

        public void OnEnable()
        {
            // Show 'splash screen'
            Label label = new Label("Opening databases...");
            label.style.fontSize = 36;
            label.style.unityTextAlign = TextAnchor.MiddleCenter;
            label.style.flexGrow = 1;

            this.rootVisualElement.Add(label);

            if (TLSSecurity.IsSetupCalled)
            {
                OnTLSecuritySetupFinished();
            }
            else
            {
                TLSSecurity.OnSetupFinished += OnTLSecuritySetupFinished;
                TLSSecurity.Setup();
            }
        }

        public void OnDestroy()
        {
            openInstance = null;
        }

        private void OnTLSecuritySetupFinished()
        {
            TLSSecurity.OnSetupFinished -= OnTLSecuritySetupFinished;

            this.rootVisualElement.Clear();
            AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(EditorHelper.GetRelativePluginFolder() + "/Editor/UXML/CertificationManagerWindow.uxml")
                .CloneTree(this.rootVisualElement);

            this.windowBinding = ScriptableObject.CreateInstance<CertificationWindowBinding>();
            this.serializedWindowBinding = new SerializedObject(this.windowBinding);
            this.rootVisualElement.Bind(this.serializedWindowBinding);

            var templateSetup = ScriptableObject.CreateInstance<TemplateBinding>();
            templateSetup.header = "Trusted Root CAs";
            templateSetup.originalURL = "https://ccadb-public.secure.force.com/mozilla/IncludedCACertificateReportPEMCSV";
            templateSetup.URL = templateSetup.originalURL;
            templateSetup.MetadataExtension = SecurityOptions.FolderAndFileOptions.MetadataExtension;
            templateSetup.DatabaseExtension = SecurityOptions.FolderAndFileOptions.DatabaseExtension;
            templateSetup.HashExtension = SecurityOptions.FolderAndFileOptions.HashExtension;
            templateSetup.HelpURL = "https://benedicht.github.io/BestHTTP-Documentation/#8.Addons/TLSSecurity/CertificationManagerWindow/#trusted-root-cas";

            new TemplateHandler(this.rootVisualElement.Q("TrustedRoots"), templateSetup, TLSSecurity.trustedRootCertificates);

            templateSetup = ScriptableObject.CreateInstance<TemplateBinding>();
            templateSetup.header = "Trusted Intermediate Certificates";
            templateSetup.originalURL = "https://ccadb-public.secure.force.com/mozilla/PublicAllIntermediateCertsWithPEMCSV";
            templateSetup.URL = templateSetup.originalURL;
            templateSetup.MetadataExtension = SecurityOptions.FolderAndFileOptions.MetadataExtension;
            templateSetup.DatabaseExtension = SecurityOptions.FolderAndFileOptions.DatabaseExtension;
            templateSetup.HashExtension = SecurityOptions.FolderAndFileOptions.HashExtension;
            templateSetup.HelpURL = "https://benedicht.github.io/BestHTTP-Documentation/#8.Addons/TLSSecurity/CertificationManagerWindow/#trusted-intermediate-certificates";

            new TemplateHandler(this.rootVisualElement.Q("TrustedIntermediates"), templateSetup, TLSSecurity.trustedIntermediateCertificates);

            new ClientCredentialsManager(this.rootVisualElement.Q("ClientCredentialsRoot"), TLSSecurity.ClientCredentials);

#if UNITY_2019_3_OR_NEWER
            this.rootVisualElement.Q<ToolbarButton>("SendButton").clicked += OnTestRequestSendButtonClicked;
#else
            this.rootVisualElement.Q<ToolbarButton>("SendButton").RegisterCallback<MouseUpEvent>(OnTestRequestSendButtonClicked);
#endif

            this.rootVisualElement.Q<TextField>("TestRequestURL").viewDataKey = "BestHTTP_TestRequestURL_ViewDataKey";
        }

        private void OnTestRequestSendButtonClicked(
#if UNITY_2019_3_OR_NEWER
#else
            MouseUpEvent evt
#endif
            )
        {
            var textField = this.rootVisualElement.Q<TextField>("TestRequestURL");
            
            var request = new HTTPRequest(new Uri("https://" + textField.text), (req, resp) =>
            {
                switch (req.State)
                {
                    case HTTPRequestStates.Finished:
                        this.windowBinding.testRequestStatus = "Finished! Status code: " + resp.StatusCode;
                        break;

                    case HTTPRequestStates.Aborted:
                        this.windowBinding.testRequestStatus = "Aborted";
                        break;

                    case HTTPRequestStates.ConnectionTimedOut:
                        this.windowBinding.testRequestStatus = "Connection Timed Out";
                        break;
                    case HTTPRequestStates.TimedOut:
                        this.windowBinding.testRequestStatus = "Download Timed Out";
                        break;

                    case HTTPRequestStates.Error:
                        this.windowBinding.testRequestStatus = "Internal Error: " + req.Exception.Message;
                        if (req.Exception.InnerException != null)
                            this.windowBinding.testRequestStatus += " " + req.Exception.InnerException.Message;
                        break;
                }
            });

            request.Tag = new TLSTagEx { RemoveALPNExtension = true };

            request.IsKeepAlive = false;
#if !BESTHTTP_DISABLE_CACHING
            request.DisableCache = true;
#endif

            request.Send();

            this.windowBinding.testRequestStatus = "Request sent";
        }
    }
}
