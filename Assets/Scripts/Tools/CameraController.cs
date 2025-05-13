using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Player;
using TMPro;
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

        [Header("UI")]
        public Image photoDisplayUI;
        public GameObject photoUICanvas;
        public float photoDisplayTime = 2f;
        private Coroutine photoDisplayCoroutine;

        [Header("Audio")]
        public AudioClip clickSound;
        public AudioClip errorSound;

        [Header("UI Feedback")]
        public TextMeshProUGUI errorText;
        public float errorDisplayTime = 1.5f;

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
            if (hasCameraTool && Input.GetMouseButtonDown(0))
                TryTakePhoto();
        }

        private void TryTakePhoto()
        {
            if (Time.time < lastPhotoTime + photoCooldown)
            {
                PlaySound(errorSound);
                StartCoroutine(ShowError("Camera needs to cool-down"));
                return;
            }

            PlaySound(clickSound);
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

            // Capture photo
            Texture2D photo = CapturePhotoTexture();

            // Reveal anomalies in view
            RevealAnomaliesInPhoto();

            // Restore camera mask
            SetPhotoCameraToNormal();

            // Show photo on UI
            ShowPhotoOnUI(photo);
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

        private void ShowPhotoOnUI(Texture2D texture)
        {
            if (photoDisplayUI == null || texture == null) return;
            Sprite photoSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            photoDisplayUI.sprite = photoSprite;

            if (photoDisplayCoroutine != null)
                StopCoroutine(photoDisplayCoroutine);

            photoDisplayCoroutine = StartCoroutine(ShowPhotoTemporarily(texture));
        }

        private IEnumerator ShowPhotoTemporarily(Texture2D texture)
        {
            TogglePhotoUI(true);
            yield return new WaitForSeconds(photoDisplayTime);
            TogglePhotoUI(false);
            if (photoDisplayUI != null) photoDisplayUI.sprite = null;
            if (texture != null) Destroy(texture);
        }

        public void TogglePhotoUI(bool state)
        {
            if (photoUICanvas != null)
                photoUICanvas.SetActive(state);
            Cursor.lockState = state ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = state;
        }

        private void PlaySound(AudioClip clip)
        {
            if (clip != null)
                AudioSource.PlayClipAtPoint(clip, transform.position);
        }

        private IEnumerator ShowError(string message)
        {
            if (errorText != null)
            {
                errorText.text = message;
                errorText.enabled = true;
                yield return new WaitForSeconds(errorDisplayTime);
                errorText.enabled = false;
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

        // Optional: ToolBase override, if needed for interaction
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
