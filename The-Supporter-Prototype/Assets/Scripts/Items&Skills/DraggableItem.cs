using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Cooldown Settings")]
    [SerializeField] protected float coolDownTime;
    [SerializeField] protected Image coolDownOverlay;

    protected float currentCooldownTime;
    private GameObject dragObject;
    private Canvas canvas;

    protected virtual void Start()
    {
        canvas = GetComponentInParent<Canvas>();
        
        // Fallback: try to find an image in children if not assigned
        if (coolDownOverlay == null)
        {
            // Note: This might pick up the main icon if it's an Image component on the same object
            // It is recommended to assign the overlay manually in the inspector
            var images = GetComponentsInChildren<Image>();
            foreach (var img in images)
            {
                if (img.gameObject != this.gameObject)
                {
                    coolDownOverlay = img;
                    break;
                }
            }
        }

        if (coolDownOverlay != null)
        {
            coolDownOverlay.fillAmount = 0;
        }
    }

    protected virtual void Update()
    {
        if (currentCooldownTime > 0)
        {
            currentCooldownTime -= Time.deltaTime;
            if (coolDownOverlay != null)
            {
                coolDownOverlay.fillAmount = currentCooldownTime / coolDownTime;
            }
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (currentCooldownTime > 0) return;

        CreateDragVisual(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragObject != null)
        {
            dragObject.transform.position = eventData.position;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (dragObject != null)
        {
            Destroy(dragObject);
            TryApplyEffect();
        }
    }

    private void CreateDragVisual(PointerEventData eventData)
    {
        dragObject = new GameObject("DragIcon");
        if (canvas != null)
        {
            dragObject.transform.SetParent(canvas.transform, false);
        }
        else
        {
            dragObject.transform.SetParent(transform.parent, false);
        }
        
        dragObject.transform.SetAsLastSibling();
        
        Image originalImage = GetComponent<Image>();
        Image dragImage = dragObject.AddComponent<Image>();
        if (originalImage != null) 
        {
            dragImage.sprite = originalImage.sprite;
            dragImage.color = originalImage.color;
            // Preserve aspect ratio if needed, or just copy settings
            dragImage.preserveAspect = originalImage.preserveAspect;
        }
        
        // Match size
        RectTransform rt = dragObject.GetComponent<RectTransform>();
        RectTransform origRt = GetComponent<RectTransform>();
        if (rt != null && origRt != null)
        {
            rt.sizeDelta = origRt.sizeDelta;
        }
        
        dragObject.transform.position = eventData.position;
        
        // Make sure it doesn't block raycasts
        dragImage.raycastTarget = false;
    }

    private void TryApplyEffect()
    {
        Vector2 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero);

        if (hit.collider != null)
        {
            if (ApplyEffect(hit.collider.gameObject))
            {
                StartCooldown();
            }
        }
    }

    protected void StartCooldown()
    {
        currentCooldownTime = coolDownTime;
        if (coolDownOverlay != null)
        {
            coolDownOverlay.fillAmount = 1;
        }
    }

    public bool IsReady()
    {
        return currentCooldownTime <= 0;
    }

    // Abstract method to be implemented by specific items
    // Returns true if the effect was successfully applied (consuming the item/triggering cooldown)
    protected abstract bool ApplyEffect(GameObject target);
}
