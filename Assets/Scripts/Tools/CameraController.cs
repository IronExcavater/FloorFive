using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Player;

namespace Tools
{
    public class CameraController : MonoBehaviour
    {
        [Header("Camera Settings")]
        public Camera photoCamera;
        public RenderTexture renderTexture;
        public LayerMask normalLayerMask;
        public LayerMask anomaliesLayerMask; // Anomalies ���̾� ����ũ
        public Transform photoOutputPoint;

        [Header("Highlight Settings")]
        public Color anomalyHighlightColor = Color.red;
        public float highlightIntensity = 2f;

        [Header("Photo Settings")]
        public GameObject photoPrefab;
        public int maxPhotos = 4;
        public float photoCooldown = 1f;
        private float lastPhotoTime;

        [Header("UI Elements")]
        public Image photoDisplayUI;
        public GameObject photoUICanvas;
        private bool isViewingPhoto;

        private List<GameObject> photoCollection = new List<GameObject>();
        private PlayerController playerController;
        private Material highlightMaterial;

        void Start()
        {
            playerController = GetComponent<PlayerController>();
            photoCamera.cullingMask = normalLayerMask;

            //// ���̶���Ʈ ��Ƽ���� ����
            //highlightMaterial = new Material(Shader.Find("Custom/HighlightShader"));
            //highlightMaterial.SetColor("_HighlightColor", anomalyHighlightColor);
            //highlightMaterial.SetFloat("_Intensity", highlightIntensity);
        }

        void Update()
        {
            HandlePhotoInput();
            HandlePhotoInspection();
        }

        void HandlePhotoInput()
        {   //Get a mouse buttons
            if (Input.GetMouseButtonDown(0) && Time.time > lastPhotoTime + photoCooldown)
            {
                StartCoroutine(TakePhotoWithAnomalies());
                lastPhotoTime = Time.time;
            }
        }

        System.Collections.IEnumerator TakePhotoWithAnomalies()
        {
            // 1. ���� ��Ƽ���� ������ ���� ��ųʸ�
            Dictionary<Renderer, Material[]> originalMaterials = new Dictionary<Renderer, Material[]>();

            // 2. Anomalies ���̾� ������Ʈ ã�Ƽ� ���̶���Ʈ ��Ƽ���� ����
            GameObject[] anomalyObjects = FindGameObjectsWithLayer(anomaliesLayerMask);
            foreach (GameObject obj in anomalyObjects)
            {
                Renderer renderer = obj.GetComponent<Renderer>();
                if (renderer != null)
                {
                    originalMaterials[renderer] = renderer.materials;
                    Material[] highlightMaterials = new Material[renderer.materials.Length];
                    for (int i = 0; i < highlightMaterials.Length; i++)
                    {
                        highlightMaterials[i] = highlightMaterial;
                    }
                    renderer.materials = highlightMaterials;
                }
            }

            // 3. Anomalies ���̾� �߰��Ͽ� ī�޶� ���� ����
            photoCamera.cullingMask = normalLayerMask | anomaliesLayerMask;

            // 4. �������� ���� �� ������ ���
            yield return new WaitForEndOfFrame();

            // 5. ���� ���
            Texture2D photoTexture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);
            RenderTexture.active = renderTexture;
            photoTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            photoTexture.Apply();
            RenderTexture.active = null;

            // 6. ��Ƽ���� ���󺹱�
            foreach (var kvp in originalMaterials)
            {
                kvp.Key.materials = kvp.Value;
            }

            // 7. ī�޶� ���� ������� ����
            photoCamera.cullingMask = normalLayerMask;

            // 8. ���� ��ü ����
            GameObject newPhoto = Instantiate(photoPrefab, photoOutputPoint.position, photoOutputPoint.rotation);
            newPhoto.GetComponent<Renderer>().material.mainTexture = photoTexture;

            // 9. ���� �÷��� ����
            photoCollection.Add(newPhoto);
            if (photoCollection.Count > maxPhotos)
            {
                Destroy(photoCollection[0]);
                photoCollection.RemoveAt(0);
            }

            // 10. UI ������Ʈ
            Sprite photoSprite = Sprite.Create(photoTexture,
                new Rect(0, 0, photoTexture.width, photoTexture.height),
                new Vector2(0.5f, 0.5f));
            photoDisplayUI.sprite = photoSprite;
        }

        // Ư�� ���̾ ���� ���� ������Ʈ ã�� �޼���
        GameObject[] FindGameObjectsWithLayer(LayerMask layerMask)
        {
            List<GameObject> objects = new List<GameObject>();
            GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();

            foreach (GameObject obj in allObjects)
            {
                if ((layerMask & (1 << obj.layer)) != 0)
                {
                    objects.Add(obj);
                }
            }

            return objects.ToArray();
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
                    Sprite photoSprite = Sprite.Create(tex,
                        new Rect(0, 0, tex.width, tex.height),
                        new Vector2(0.5f, 0.5f));
                    photoDisplayUI.sprite = photoSprite;

                    TogglePhotoUI(!isViewingPhoto);
                    break;
                }
            }
        }

        public void TogglePhotoUI(bool state)
        {
            isViewingPhoto = state;
            photoUICanvas.SetActive(state);
            //playerController.ToggleControl(!state);
            Cursor.lockState = state ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = state;
        }
    }
}