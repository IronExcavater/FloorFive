using UnityEngine;
using System.Collections;
using Player;
using System.Collections.Generic;

namespace Tools
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(AudioSource))]
    public class CameraController : ToolBase
    {
        [Header("Layers")]
        public LayerMask normalLayerMask;
        public LayerMask cameraOnlyLayerMask;

        [Header("Cameras")]
        public Camera photoCamera;
        public Camera playerCamera;
        public RenderTexture renderTexture;

        [Header("Photo Settings")]
        public float photoCooldown = 1f;
        private float lastPhotoTime;

        [Header("Audio")]
        public AudioClip clickSound;
        public AudioClip errorSound;

        // AudioSource는 ToolBase에서 protected _audioSource로 선언되어 있다고 가정
        // 만약 ToolBase에 없다면, 아래와 같이 선언하세요:
        // private AudioSource _audioSource;

        protected override void Awake()
        {
            base.Awake();
            if (photoCamera != null)
                photoCamera.enabled = false;
            SetPhotoCameraToNormal();

            // AudioSource를 반드시 할당
            if (_audioSource == null)
                _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
                _audioSource = gameObject.AddComponent<AudioSource>();
        }

        protected override void Update()
        {
            base.Update();
            // 입력 체크 없음 (F키 입력은 PlayerController에서 처리)
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
            SyncPhotoCameraToPlayer();
            SetPhotoCameraToAnomaly();
            yield return null;

            Texture2D photo = CapturePhotoTexture();
            RevealAnomaliesInPhoto();
            SetPhotoCameraToNormal();
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

        protected override void Use(PlayerController player)
        {
            if (playerCamera == null && player != null && player.cameraTransform != null)
            {
                playerCamera = player.cameraTransform.GetComponent<Camera>();
            }

            PlaySound(clickSound);
            TryTakePhoto();
        }
    }
}
