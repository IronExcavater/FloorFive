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
        public LayerMask cameraOnlyLayerMask;      // 카메라에서만 보이는 블랙 미스트 레이어

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

        void Start()
        {
            playerController = GetComponent<PlayerController>();
            photoCamera.cullingMask = normalLayerMask; // 기본적으로 일반 레이어만 렌더링
        }

        void Update()
        {
            HandlePhotoInput();
            HandlePhotoInspection();
        }

        void HandlePhotoInput()
        {
            if (Input.GetMouseButtonDown(0) && Time.time > lastPhotoTime + photoCooldown)
            {
                StartCoroutine(CaptureAnomalyPhoto());
                lastPhotoTime = Time.time;
            }
        }

        IEnumerator CaptureAnomalyPhoto()
        {
            // 1. 카메라에 블랙 미스트(이상현상) 레이어 추가
            photoCamera.cullingMask = normalLayerMask | cameraOnlyLayerMask;

            yield return new WaitForEndOfFrame();

            // 2. 사진 텍스처 생성
            Texture2D photoTexture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);
            RenderTexture.active = renderTexture;
            photoTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            photoTexture.Apply();
            RenderTexture.active = null;

            // 3. 사진에 찍힌 블랙 미스트를 현실 세계에 드러나게 처리
            RevealAnomaliesInPhoto();

            // 4. 카메라 레이어 복구
            photoCamera.cullingMask = normalLayerMask;

            // 5. 사진 오브젝트 생성 및 관리
            CreatePhotoObject(photoTexture);
        }

        void RevealAnomaliesInPhoto()
        {
            GameObject[] anomalies = FindGameObjectsWithLayer(cameraOnlyLayerMask);
            foreach (GameObject anomaly in anomalies)
            {
                if (IsVisibleInCamera(anomaly))
                {
                    // 레이어를 Default(현실 세계)로 변경
                    anomaly.layer = LayerMask.NameToLayer("Default");

                    // 파티클 등 추가 효과가 있다면 재생
                    ParticleSystem ps = anomaly.GetComponent<ParticleSystem>();
                    if (ps != null)
                        ps.Play();
                }
            }
        }

        bool IsVisibleInCamera(GameObject target)
        {
            Vector3 viewportPoint = photoCamera.WorldToViewportPoint(target.transform.position);
            return viewportPoint.z > 0 &&
                   viewportPoint.x > 0 && viewportPoint.x < 1 &&
                   viewportPoint.y > 0 && viewportPoint.y < 1;
        }

        void CreatePhotoObject(Texture2D texture)
        {
            GameObject newPhoto = Instantiate(photoPrefab, photoOutputPoint.position, photoOutputPoint.rotation);
            newPhoto.GetComponent<Renderer>().material.mainTexture = texture;

            photoCollection.Add(newPhoto);
            if (photoCollection.Count > maxPhotos)
            {
                Destroy(photoCollection[0]);
                photoCollection.RemoveAt(0);
            }

            // UI 미리보기 업데이트
            UpdatePhotoUI(texture);
        }

        void UpdatePhotoUI(Texture2D texture)
        {
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
                    Texture2D tex = (Texture2D)photo.GetComponent<Renderer>().material.mainTexture;
                    UpdatePhotoUI(tex);
                    TogglePhotoUI(!isViewingPhoto);
                    break;
                }
            }
        }

        public void TogglePhotoUI(bool state)
        {
            isViewingPhoto = state;
            photoUICanvas.SetActive(state);
            // playerController.ToggleControl(!state); // 필요시 주석 해제
            Cursor.lockState = state ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = state;
        }

        // 특정 레이어 오브젝트 찾기
        GameObject[] FindGameObjectsWithLayer(LayerMask layerMask)
        {
            List<GameObject> objects = new List<GameObject>();
            GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (GameObject obj in allObjects)
            {
                if ((layerMask.value & (1 << obj.layer)) != 0)
                {
                    objects.Add(obj);
                }
            }
            return objects.ToArray();
        }
    }
}
