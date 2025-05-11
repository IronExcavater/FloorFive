using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Player;
using TMPro;
using Tools;

namespace Tools
{
    [RequireComponent(typeof(Rigidbody))]
    public class CameraController : ToolBase
    {
        [Header("Layer Settings")]
        public LayerMask normalLayerMask;
        public LayerMask cameraOnlyLayerMask;

        [Header("Camera System")]
        public Camera photoCamera;
        public RenderTexture renderTexture;
        public Transform photoOutputPoint;

        [Header("Photo Settings")]
        public GameObject photoPrefab;
        public int maxPhotos = 4;
        public float photoCooldown = 1f;
        private float lastPhotoTime;

        [Header("UI System")]
        public Image photoDisplayUI;
        public GameObject photoUICanvas;
        private bool isViewingPhoto;

        private readonly List<GameObject> photoCollection = new List<GameObject>();
        private PlayerController playerController;

        [Header("Audio")]
        public AudioClip clickSound;
        public AudioClip errorSound;

        [Header("UI Feedback")]
        public TextMeshProUGUI errorText;
        public float errorDisplayTime = 1.5f;

        protected override void Awake()
        {
            base.Awake();
            playerController = GetComponent<PlayerController>();
            SetPhotoCameraToNormal();
        }

        protected override void Update()
        {
            base.Update();
            ProcessPhotoInput();
            ProcessPhotoInspection();
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
            SetPhotoCameraToAnomaly();
            yield return new WaitForEndOfFrame();

            Texture2D photoTexture = CapturePhotoTexture();
            RevealAnomaliesInPhoto();
            SetPhotoCameraToNormal();
            CreatePhotoObject(photoTexture);
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

        private void CreatePhotoObject(Texture2D texture)
        {
            if (photoPrefab == null || photoOutputPoint == null || texture == null) return;

            GameObject newPhoto = Instantiate(photoPrefab, photoOutputPoint.position, photoOutputPoint.rotation);
            var renderer = newPhoto.GetComponent<Renderer>();
            if (renderer != null)
                renderer.material.mainTexture = texture;

            photoCollection.Add(newPhoto);
            if (photoCollection.Count > maxPhotos)
            {
                Destroy(photoCollection[0]);
                photoCollection.RemoveAt(0);
            }

            UpdatePhotoUI(texture);
        }

        private void UpdatePhotoUI(Texture2D texture)
        {
            if (photoDisplayUI == null || texture == null) return;
            Sprite photoSprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f)
            );
            photoDisplayUI.sprite = photoSprite;
        }

        private void ProcessPhotoInspection()
        {
            if (photoCollection.Count == 0) return;

            foreach (GameObject photo in photoCollection)
            {
                float distance = Vector3.Distance(transform.position, photo.transform.position);
                if (distance < 2f && Input.GetKeyDown(KeyCode.E))
                {
                    var renderer = photo.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        Texture2D tex = renderer.material.mainTexture as Texture2D;
                        UpdatePhotoUI(tex);
                        TogglePhotoUI(!isViewingPhoto);
                        break;
                    }
                }
            }
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

        // ToolBase의 추상 메서드 구현
        protected override void Use(PlayerController player)
        {
            StartCoroutine(CaptureAnomalyPhoto());
        }
    }
}
