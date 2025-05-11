using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Player;
using TMPro;
using Tools;
using System.Collections.Generic;

namespace Tools
{
    [RequireComponent(typeof(Rigidbody))]
    public class CameraController : ToolBase
    {
        [Header("Layer Settings")]
        public LayerMask normalLayerMask;
        public LayerMask cameraOnlyLayerMask;

        [Header("Camera System")]
        public Camera photoCamera;         // 사진 촬영용 카메라 (별도 오브젝트, 평소엔 꺼짐)
        public Camera playerCamera;        // 플레이어 시점 카메라 (MainCamera)
        public RenderTexture renderTexture;

        [Header("Photo Settings")]
        public float photoCooldown = 1f;
        private float lastPhotoTime;

        [Header("UI System")]
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

        private PlayerController playerController;
        private bool isViewingPhoto;

        protected override void Awake()
        {
            base.Awake();
            playerController = GetComponent<PlayerController>();
            SetPhotoCameraToNormal();
            if (photoCamera != null)
                photoCamera.enabled = false; // 사진 카메라는 평소엔 꺼둠
        }

        protected override void Update()
        {
            base.Update();
            ProcessPhotoInput();
        }

        private void ProcessPhotoInput()
        {
            if (!Input.GetMouseButtonDown(0)) return;

            if (Time.time > lastPhotoTime + photoCooldown)
            {
                PlaySound(clickSound);
                StartCoroutine(CaptureAnomalyPhoto());
                lastPhotoTime = Time.time;
            }
            else
            {
                PlaySound(errorSound);
                StartCoroutine(ShowError("Camera needs to cool-down"));
            }
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

        private IEnumerator CaptureAnomalyPhoto()
        {
            // 1. 사진 카메라 위치/회전 플레이어 카메라에 맞추기
            if (photoCamera != null && playerCamera != null)
            {
                photoCamera.transform.position = playerCamera.transform.position;
                photoCamera.transform.rotation = playerCamera.transform.rotation;
                photoCamera.fieldOfView = playerCamera.fieldOfView;
                photoCamera.enabled = true;
            }

            SetPhotoCameraToAnomaly();
            yield return new WaitForEndOfFrame();

            Texture2D photoTexture = CapturePhotoTexture();
            RevealAnomaliesInPhoto();
            SetPhotoCameraToNormal();

            // 2. 사진 카메라 끄기
            if (photoCamera != null)
                photoCamera.enabled = false;

            ShowPhotoOnUI(photoTexture);
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
            if (renderTexture == null || photoCamera == null)
                return null;

            Texture2D texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);
            RenderTexture.active = renderTexture;
            photoCamera.targetTexture = renderTexture;
            photoCamera.Render();
            texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            texture.Apply();
            RenderTexture.active = null;
            photoCamera.targetTexture = null;
            return texture;
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
            Sprite photoSprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f)
            );
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
            if (photoDisplayUI != null)
                photoDisplayUI.sprite = null;
            if (texture != null)
                Destroy(texture);
        }

        public void TogglePhotoUI(bool state)
        {
            isViewingPhoto = state;
            if (photoUICanvas != null)
                photoUICanvas.SetActive(state);
            Cursor.lockState = state ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = state;
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
            StartCoroutine(CaptureAnomalyPhoto());
        }
    }
}
