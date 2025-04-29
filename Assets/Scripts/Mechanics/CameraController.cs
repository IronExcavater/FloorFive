using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CameraController : MonoBehaviour
{
    [Header("Camera Settings")]
    public Camera photoCamera;
    public RenderTexture renderTexture;
    public LayerMask normalLayerMask;
    public LayerMask anomaliesLayerMask; // Anomalies 레이어 마스크
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

        // 하이라이트 머티리얼 생성
        highlightMaterial = new Material(Shader.Find("Custom/HighlightShader"));
        highlightMaterial.SetColor("_HighlightColor", anomalyHighlightColor);
        highlightMaterial.SetFloat("_Intensity", highlightIntensity);
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
            StartCoroutine(TakePhotoWithAnomalies());
            lastPhotoTime = Time.time;
        }
    }

    System.Collections.IEnumerator TakePhotoWithAnomalies()
    {
        // 1. 원본 머티리얼 저장을 위한 딕셔너리
        Dictionary<Renderer, Material[]> originalMaterials = new Dictionary<Renderer, Material[]>();

        // 2. Anomalies 레이어 오브젝트 찾아서 하이라이트 머티리얼 적용
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

        // 3. Anomalies 레이어 추가하여 카메라 설정 변경
        photoCamera.cullingMask = normalLayerMask | anomaliesLayerMask;

        // 4. 렌더링을 위해 한 프레임 대기
        yield return new WaitForEndOfFrame();

        // 5. 사진 찍기
        Texture2D photoTexture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);
        RenderTexture.active = renderTexture;
        photoTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        photoTexture.Apply();
        RenderTexture.active = null;

        // 6. 머티리얼 원상복구
        foreach (var kvp in originalMaterials)
        {
            kvp.Key.materials = kvp.Value;
        }

        // 7. 카메라 설정 원래대로 복구
        photoCamera.cullingMask = normalLayerMask;

        // 8. 사진 객체 생성
        GameObject newPhoto = Instantiate(photoPrefab, photoOutputPoint.position, photoOutputPoint.rotation);
        newPhoto.GetComponent<Renderer>().material.mainTexture = photoTexture;

        // 9. 사진 컬렉션 관리
        photoCollection.Add(newPhoto);
        if (photoCollection.Count > maxPhotos)
        {
            Destroy(photoCollection[0]);
            photoCollection.RemoveAt(0);
        }

        // 10. UI 업데이트
        Sprite photoSprite = Sprite.Create(photoTexture,
            new Rect(0, 0, photoTexture.width, photoTexture.height),
            new Vector2(0.5f, 0.5f));
        photoDisplayUI.sprite = photoSprite;
    }

    // 특정 레이어에 속한 게임 오브젝트 찾는 메서드
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
        playerController.ToggleControl(!state);
        Cursor.lockState = state ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = state;
    }
}