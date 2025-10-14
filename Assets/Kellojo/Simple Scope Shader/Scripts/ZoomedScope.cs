using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;


namespace Kellojo.ScopeShader {
    public class ZoomedScope : MonoBehaviour {

        [Header("Camera Settings:")]
        [SerializeField, Tooltip("The camera used to capture the scope picture")] private Camera ScopeCamera;
        [SerializeField, Range(0.01f, 180f)] private float _cameraFov = 4f;
        public float CameraFoV {
            get => _cameraFov;
            set {
                _cameraFov = value;
                if (ScopeCamera != null) ScopeCamera.fieldOfView = _cameraFov;
            }
        }
        [SerializeField, Tooltip("The resolution of the render texture used to capture the scope picture")] private Vector2Int RenderTextureSize = new Vector2Int(1024, 1024);

        [Header("Lens Settings:")]
        [SerializeField, Tooltip("The renderer that renders the lens, onto which the scene image is projected.")] private Renderer LensRenderer;

        [Header("Distance Check Settings:")]
        [Tooltip("Should the scope automatically disable it's camera, if a certain istance from the player is reached?")] public bool AutoDisableBasedOnDistance = true;
        [Tooltip("An override for the reference point to meassure the distance from (i.e., the player). If empty, Camera.main will be used.")] public Transform MainCameraTransformOverride;
        [Tooltip("The interval at which the check should be performed"), Range(0.001f, 10f)] public float DistanceCheckInterval = 0.5f;
        [Tooltip("The max distance at which the scope will still be enabled")] public float MaxDistance = 25f;

        Material LensMaterial;
        RenderTexture CameraTarget;
        Coroutine DistanceCheckRoutine;
        Camera MainCamera;



        protected void Awake() {
            MainCamera = Camera.main;
            CameraTarget = RenderTexture.GetTemporary(RenderTextureSize.x, RenderTextureSize.y, 32);
            CameraTarget.format = RenderTextureFormat.DefaultHDR;
            ScopeCamera.targetTexture = CameraTarget;

            LensMaterial = LensRenderer.material;
            LensMaterial.SetTexture("_SceneColor", CameraTarget);

            CameraFoV = CameraFoV;
        }

        protected void OnValidate() {
            CameraFoV = CameraFoV;

            if (MaxDistance < 0f) MaxDistance = 0.01f;
        }

        protected void OnEnable() {
            ScopeCamera.enabled = true;

            if(AutoDisableBasedOnDistance) {
                DistanceCheckRoutine = StartCoroutine(RunDistanceCheck());
            }

        }

        protected void OnDisable() {
            ScopeCamera.enabled = false;

            if (DistanceCheckRoutine != null) {
                StopCoroutine(DistanceCheckRoutine);
                DistanceCheckRoutine = null;
            }
        }

        protected void OnDestroy() {
            RenderTexture.ReleaseTemporary(CameraTarget);
        }


        protected IEnumerator RunDistanceCheck() {

            while (enabled) {
                var target = MainCameraTransformOverride != null ? MainCameraTransformOverride : MainCamera.transform;
                var dist = Vector3.Distance(transform.position, target.position);
                ScopeCamera.enabled = dist < MaxDistance;

                yield return new WaitForSeconds(DistanceCheckInterval);
            }

        }
    }

}

