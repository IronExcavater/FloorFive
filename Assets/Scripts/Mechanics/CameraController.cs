using UnityEngine;
using UnityEngine.UI;

public class CameraController: MonoBehaviour
{
    [Header("Camera Settings")]
    public Camera photoCamera;
    public RenderTexture renderTexture;
    public LayerMask photoLayerMask;
    public Transform photoOutputPoint;

    [Header("Photo Settings")]
    public GameObject photoPrefab;
    public int maxPhotos = 5;
    public float photoCooldown = 1f;
    private float lastPhotoTime;

    [Header("UI Elements")]
    public Image photoDisplayUI;
    public GameObject photoUICanvas;
    private bool isViewingPhoto;

    private GameObject currentPhoto;
    private PlayerController playerController;

    void Start()
    {
        playerController = GetComponent<PlayerController>();
        photoCamera.cullingMask = photoLayerMask;
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
            TakePhoto();
            lastPhotoTime = Time.time;
        }
    }

    void TakePhoto()
    {
        // ���� �ؽ�ó���� �ؽ�ó ����
        Texture2D photoTexture = new Texture2D(renderTexture.width, renderTexture.height);
        RenderTexture.active = renderTexture;
        photoTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        photoTexture.Apply();

        // ���� ���� ����
        if (currentPhoto != null) Destroy(currentPhoto);

        // �� ���� ��ü ����
        currentPhoto = Instantiate(photoPrefab, photoOutputPoint.position, photoOutputPoint.rotation);
        currentPhoto.GetComponent<Renderer>().material.mainTexture = photoTexture;

        // UI�� ��������Ʈ ����
        Sprite photoSprite = Sprite.Create(photoTexture,
            new Rect(0, 0, photoTexture.width, photoTexture.height),
            new Vector2(0.5f, 0.5f));
        photoDisplayUI.sprite = photoSprite;
    }

    void HandlePhotoInspection()
    {
        if (currentPhoto == null) return;

        float distance = Vector3.Distance(transform.position, currentPhoto.transform.position);
        if (distance < 2f && Input.GetKeyDown(KeyCode.E))
        {
            TogglePhotoUI(!isViewingPhoto);
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
