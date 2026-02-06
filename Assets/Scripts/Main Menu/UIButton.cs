using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Image))]
public class UIButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler, IPointerClickHandler
{
    [Header("Hover & Scale")]
    public float hoverScale = 1.05f;
    public float speed = 10f;

    [Header("Shader Colors")]
    public Color topColor = new Color(107f / 255f, 91f / 255f, 1f, 1f);
    public Color bottomColor = new Color(77f / 255f, 163f / 255f, 1f, 1f);
    public Color topHoverColor = new Color(140f / 255f, 127f / 255f, 1f, 1f);
    public Color bottomHoverColor = new Color(92f / 255f, 191f / 255f, 1f, 1f);

    private Vector3 originalScale;
    private Image img;
    private Material matInstance;

    private bool isHovered = false;
    private bool isSelected = false;

    private static UIButton lastSelectedByMouse;

    private void Awake()
    {
        img = GetComponent<Image>();
        originalScale = transform.localScale;

        matInstance = Instantiate(img.material);
        img.material = matInstance;

        matInstance.SetColor("_TopColor", topColor);
        matInstance.SetColor("_BottomColor", bottomColor);
    }

    private void OnEnable()
    {
        if (lastSelectedByMouse == this)
            lastSelectedByMouse = null;
    }

    private void Update()
    {
        bool active = isHovered || isSelected || EventSystem.current?.currentSelectedGameObject == gameObject;

        // Escalado suave
        transform.localScale = Vector3.Lerp(transform.localScale,
            active ? originalScale * hoverScale : originalScale, Time.deltaTime * speed);

        // Cambiar colores
        matInstance.SetColor("_TopColor", active ? topHoverColor : topColor);
        matInstance.SetColor("_BottomColor", active ? bottomHoverColor : bottomColor);
    }

    // Hover del ratón
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;

        if (lastSelectedByMouse != null && lastSelectedByMouse != this)
            lastSelectedByMouse.isSelected = false;

        lastSelectedByMouse = this;

        // Solo selecciona si no es ya el objeto seleccionado
        if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != gameObject)
            EventSystem.current.SetSelectedGameObject(gameObject);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
    }

    // Cuando el EventSystem selecciona este botón
    public void OnSelect(BaseEventData eventData)
    {
        isSelected = true;

        if (lastSelectedByMouse != null && lastSelectedByMouse != this)
            lastSelectedByMouse.isSelected = false;
    }

    public void OnDeselect(BaseEventData eventData)
    {
        isSelected = false;
    }
    private void OnDisable()
    {
        isHovered = false;
        isSelected = false;
        ResetColors();
    }

    private void ResetColors()
    {
        if (matInstance != null)
        {
            matInstance.SetColor("_TopColor", topColor);
            matInstance.SetColor("_BottomColor", bottomColor);
            transform.localScale = originalScale;
        }
    }

    // Desseleccionar automáticamente después de click
    public void OnPointerClick(PointerEventData eventData)
    {
        if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == gameObject)
        {
            // Deselect after click
            EventSystem.current.SetSelectedGameObject(null);
        }
    }
}
