using UnityEngine;
using System.Collections;
using Player;
using System.Collections.Generic;

namespace Tools
{
    [RequireComponent(typeof(Rigidbody))]
    public class CameraController : ToolBase
    {
        [Header("Layers")]
        public LayerMask normalLayerMask;
        public LayerMask cameraOnlyLayerMask;

        [Header("Cameras")]
        public Camera photoCamera;   // Disabled by default, used only for taking photos
        public Camera playerCamera;  // Main player camera
        public RenderTexture renderTexture;

        [Header("Photo Settings")]
        public float photoCooldown = 1f;
        private float lastPhotoTime;

        [Header("Audio")]
        public AudioClip clickSound;
        public AudioClip errorSound;

        private bool hasCameraTool = false;

        // Call this when the player picks up the camera
        public void AcquireCameraTool() => hasCameraTool = true;

        protected override void Awake()
        {
            base.Awake();
            if (photoCamera != null)
                photoCamera.enabled = false;
            SetPhotoCameraToNormal();
        }

        protected override void Update()
        {
            base.Update();
            // 마우스 왼쪽 버튼 또는 F키로 사진 찍기
            if (hasCameraTool && (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.F)))
                TryTakePhoto();
        }

        private void TryTakePhoto()
        {
            if (Time.time < lastPhotoTime + photoCooldown)
            {
                PlaySound(errorSound);
                return;
            }

            lastPhotoTime = Time.time;
            StartCoroutine(CapturePhotoSequence());
        }

        private IEnumerator CapturePhotoSequence()
        {
            // Sync photo camera to player's view
            SyncPhotoCameraToPlayer();

            // Set culling mask to reveal anomalies
            SetPhotoCameraToAnomaly();
            yield return null; // Wait a frame

            // 사진이 실제로 찍히는 순간 소리 재생!
            PlaySound(clickSound);

            // Capture photo
            Texture2D photo = CapturePhotoTexture();

            // Reveal anomalies in view
            RevealAnomaliesInPhoto();

            // Restore camera mask
            SetPhotoCameraToNormal();

            // 캡처된 사진 Texture2D는 더 이상 사용하지 않으니 바로 파괴
            if (photo != null)
                Destroy(photo);
        }

        private void SyncPhotoCameraToPlayer()
        {
            if (photoCamera == null || playerCamera == null) return;
            photoCamera.transform.SetPositionAndRotation(playerCamera.transform.position, playerCamera.transform.rotation);
            photoCamera.fieldOfView = playerCamera.fieldOfView;
            photoCamera.nearClipPlane = playerCamera.nearClipPlane;
            photoCamera.farClipPlane = playerCamera.farClipPlane;
        }

        private void SetPhotoCameraToNormal()
        {
            if (photoCamera != null)
                photoCamera.cullingMask = normalLayerMask;
        }

        private void SetPhotoCameraToAnomaly()
        {
            if (photoCamera != null)
                photoCamera.cullingMask = normalLayerMask | cameraOnlyLayerMask;
        }

        private Texture2D CapturePhotoTexture()
        {
            if (renderTexture == null || photoCamera == null) return null;

            Texture2D tex = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);
            RenderTexture.active = renderTexture;
            photoCamera.targetTexture = renderTexture;
            photoCamera.Render();
            tex.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            tex.Apply();
            RenderTexture.active = null;
            photoCamera.targetTexture = null;
            return tex;
        }

        private void RevealAnomaliesInPhoto()
        {
            var anomalies = FindGameObjectsWithLayer(cameraOnlyLayerMask);
            foreach (var anomaly in anomalies)
            {
                if (IsVisibleInCamera(anomaly))
                {
                    var cameraAnomaly = anomaly.GetComponent<Anomaly.CameraAnomaly>();
                    cameraAnomaly?.Reveal();
                }
            }
        }

        private bool IsVisibleInCamera(GameObject target)
        {
            if (target == null) return false;
            var renderer = target.GetComponent<Renderer>();
            if (renderer == null) return false;

            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(photoCamera);
            return GeometryUtility.TestPlanesAABB(planes, renderer.bounds);
        }

        // PlaySound 함수는 AudioSource의 PlayOneShot을 사용!
        private void PlaySound(AudioClip clip)
        {
            if (_audioSource != null && clip != null)
            {
                _audioSource.PlayOneShot(clip);
            }
        }

        private GameObject[] FindGameObjectsWithLayer(LayerMask layerMask)
        {
            var allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            var result = new List<GameObject>();
            int mask = layerMask.value;

            foreach (var obj in allObjects)
            {
                if (obj.hideFlags == HideFlags.NotEditable || obj.hideFlags == HideFlags.HideAndDontSave)
                    continue;
                if (!Application.IsPlaying(obj))
                    continue;

                if ((mask & (1 << obj.layer)) != 0)
                    result.Add(obj);
            }

            return result.ToArray();
        }

        // ToolBase의 추상 메서드 구현 (playerCamera 자동 할당)
        protected override void Use(PlayerController player)
        {
            // playerCamera가 비어 있으면 자동 할당
            if (playerCamera == null && player != null && player.cameraTransform != null)
            {
                playerCamera = player.cameraTransform.GetComponent<Camera>();
            }
            if (!hasCameraTool)
                AcquireCameraTool();

            StartCoroutine(CapturePhotoSequence());
        }
    }
}
