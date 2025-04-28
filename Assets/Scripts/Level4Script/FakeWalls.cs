using System.Collections;
using UnityEngine;

public class FakeWalls : MonoBehaviour, IInteractable
{
    [Header("Settings")]
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private Color highlightColor = new(0, 1, 0, 0.5f);

    private Renderer _renderer;
    private MaterialPropertyBlock _propBlock;
    private Color _originalColor;

    public bool CanInteract => true;
    public string InteractMessage => "E키 - 가짜 벽 통과"; // 메시지 구현

    private void Awake()
    {
        _renderer = GetComponent<Renderer>(); // 타입 명시
        _propBlock = new MaterialPropertyBlock();
        _originalColor = _renderer.material.color;
        gameObject.layer = LayerMask.NameToLayer("Interactable");
    }

    public void OnStartHighlight(Color highlightColor)
    {
        _renderer.GetPropertyBlock(_propBlock);
        _propBlock.SetColor("_Color", highlightColor);
        _renderer.SetPropertyBlock(_propBlock);
    }

    public void OnEndHighlight()
    {
        _renderer.GetPropertyBlock(_propBlock);
        _propBlock.SetColor("_Color", _originalColor);
        _renderer.SetPropertyBlock(_propBlock);
    }

    public void OnInteract()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        GetComponent<Collider>().isTrigger = true; // 타입 명시
        _renderer.material.color = new Color(1, 1, 1, 0.3f);
        StartCoroutine(ReLockCursor());
    }

    private IEnumerator ReLockCursor()
    {
        yield return new WaitForSeconds(2f);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
