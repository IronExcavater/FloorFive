using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Player;

namespace Tools
{
    public class CameraController : MonoBehaviour
    {
        [Header("Layer Settings")]
        public LayerMask normalLayerMask;          // 플레이어가 보는 일반 레이어
        public LayerMask cameraOnlyLayerMask;      // 카메라에서만 보이는 anomaly(블랙 미스트) 레이어

        [Header("Camera System")]
        public Camera photoCamera;                 // 사진 촬영용 카메라
        public RenderTexture renderTexture;        // 사진 저장용 렌더 텍스처
        public Transform photoOutputPoint;         // 사진 생성 위치

        [Header("Photo Settings")]
        public GameObject photoPrefab;             // 사진 프리팹
        public int maxPhotos = 4;                  // 최대 사진 개수
        public float photoCooldown = 1f;           // 사진 쿨타임
        private float lastPhotoTime;

        [Header("UI System")]
        public Image photoDisplayUI;               // 사진 미리보기 UI
        public GameObject photoUICanvas;           // 사진 UI 캔버스
        private bool isViewingPhoto;

        private List<GameObject> photoCollection = new List<GameObject>();
        private PlayerController playerController;

        [Header("Audio")]
        public AudioSource clickSound;
        public AudioSource errorSound;

        [Header("UI Feedback")]
        public Text errorText;
        public float errorDisplayTime = 1.5f;

        void Start()
        {
            playerController = GetComponent<PlayerController>();
            SetPhotoCameraToNormal();
        }

        void Update()
        {
            HandlePhotoInput();
            HandlePhotoInspection();
        }

        void HandlePhotoInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (Time.time > lastPhotoTime + photoCooldown)
                {
                    clickSound?.Play();
                    StartCoroutine(CaptureAnomalyPhoto());
                    lastPhotoTime = Time.time;
                }
                else
                {
                    errorSound?.Play();
                    StartCoroutine(ShowError("Camera needs to cool-down"));
                }
            }
        }

        IEnumerator ShowError(string message)
        {
            if (errorText != null)
            {
                errorText.text = message;
                errorText.enabled = true;
                yield return new WaitForSeconds(errorDisplayTime);
                errorText.enabled = false;
            }
        }

        IEnumerator CaptureAnomalyPhoto()
        {
            // 1. anomaly 레이어를 카메라에 추가
            SetPhotoCameraToAnomaly();

            yield return new WaitForEndOfFrame();

            // 2. 사진 촬영
            Texture2D photoTexture = CapturePhotoTexture();

            // 3. anomaly 노출
            RevealAnomaliesInPhoto();

            // 4. 카메라 레이어 복구
            SetPhotoCameraToNormal();

            // 5. 사진 오브젝트 생성 및 UI 갱신
            CreatePhotoObject(photoTexture);
        }

        void SetPhotoCameraToNormal()
        {
            photoCamera.cullingMask = normalLayerMask;
        }

        void SetPhotoCameraToAnomaly()
        {
            photoCamera.cullingMask = normalLayerMask | cameraOnlyLayerMask;
        }

        Texture2D CapturePhotoTexture()
        {
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

        void RevealAnomaliesInPhoto()
        {
            // 해당 레이어에 속한 모든 anomaly 찾기
            var anomalies = FindGameObjectsWithLayer(cameraOnlyLayerMask);
            foreach (var anomaly in anomalies)
            {
                if (IsVisibleInCamera(anomaly))
                {
                    var cameraAnomaly = anomaly.GetComponent<Anomaly.CameraAnomaly>();
                    if (cameraAnomaly != null)
                        cameraAnomaly.Reveal();
                }
            }
        }

        // 카메라 뷰포트 내에 anomaly가 실제로 보이는지 체크
        bool IsVisibleInCamera(GameObject target)
        {
            if (target == null) return false;
            var renderer = target.GetComponent<Renderer>();
            if (renderer == null) return false;

            // 카메라의 frustum에 실제로 포함되는지 체크
            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(photoCamera);
            return GeometryUtility.TestPlanesAABB(planes, renderer.bounds);
        }

        void CreatePhotoObject(Texture2D texture)
        {
            if (photoPrefab == null || photoOutputPoint == null) return;
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

        void UpdatePhotoUI(Texture2D texture)
        {
            if (photoDisplayUI == null || texture == null) return;
            Sprite photoSprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f)
            );
            photoDisplayUI.sprite = photoSprite;
        }

        void HandlePhotoInspection()
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
            // playerController.ToggleControl(!state); // 필요시 주석 해제
            Cursor.lockState = state ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = state;
        }

        // 특정 레이어 오브젝트 찾기 (최적화)
        GameObject[] FindGameObjectsWithLayer(LayerMask layerMask)
        {
            var allObjects = GameObject.FindObjectsOfType<GameObject>(true);
            var result = new List<GameObject>();
            int mask = layerMask.value;
            foreach (var obj in allObjects)
            {
                if ((mask & (1 << obj.layer)) != 0)
                    result.Add(obj);
            }
            return result.ToArray();
        }
    }
}
