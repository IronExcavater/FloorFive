using UnityEngine;
using UnityEngine.UI;

public class InteractionController : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float interactDistance = 3f;
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private GameObject promptPanelPrefab;
    [SerializeField] private Text promptText;
    [SerializeField] private Color highlightColor = Color.green;

    private IInteractable currentTarget;
    private IInteractable previousTarget;
    private GameObject promptPanelInstance;
    private CanvasGroup promptCanvasGroup;

    void Update()
    {
        DetectInteractables();
        HandleInteractionInput();
    }

    private void ShowPromptPanel()
    {
        if (promptPanelInstance == null)
        {
            promptPanelInstance = Instantiate(promptPanelPrefab, transform.position, Quaternion.identity);
            promptCanvasGroup = promptPanelInstance.GetComponent<CanvasGroup>();
            if (promptCanvasGroup == null)
            {
                promptCanvasGroup = promptPanelInstance.AddComponent<CanvasGroup>(); // ������ �߰�
            }
        }

        promptPanelInstance.SetActive(true);
    }

    private void HidePromptPanel()
    {
        if (promptPanelInstance != null)
        {
            promptPanelInstance.SetActive(false);
        }
    }

    private void DetectInteractables()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactableLayer))
        {
            var interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != null && interactable.CanInteract)
            {
                float distanceToInteractable = Vector3.Distance(playerCamera.transform.position, hit.point);

                if (distanceToInteractable <= interactDistance)
                {
                    UpdateCurrentTarget(interactable);

                    if (promptCanvasGroup != null)
                    {
                        float alpha = 1f - (distanceToInteractable / interactDistance);
                        promptCanvasGroup.alpha = Mathf.Clamp01(alpha);
                    }

                    return;
                }
            }
        }

        ClearCurrentTarget();
    }

    private void UpdateCurrentTarget(IInteractable newTarget)
    {
        if (currentTarget != newTarget)
        {
            previousTarget?.OnEndHighlight();
            previousTarget = currentTarget;
            currentTarget = newTarget;
            currentTarget.OnStartHighlight(highlightColor);

            ShowPromptPanel();
            if (promptText != null)
            {
                promptText.text = currentTarget.InteractMessage;
            }
        }
    }

    private void ClearCurrentTarget()
    {
        if (currentTarget != null)
        {
            currentTarget.OnEndHighlight();
            previousTarget = currentTarget;
            currentTarget = null;
            HidePromptPanel();
        }
    }

    private void HandleInteractionInput()
    {
        if (currentTarget != null && Input.GetButtonDown("Interact"))
        {
            currentTarget.OnInteract();
        }
    }
}
