using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Image))]
public class UIButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Hover & Scale")]
    public float hoverScale = 1.05f;
    public float speed = 10f;

    [Header("Shader Colors")]
    public Color topColor = new Color(107f / 255f, 91f / 255f, 1f, 1f);      // Primary top
    public Color bottomColor = new Color(77f / 255f, 163f / 255f, 1f, 1f);   // Primary bottom
    public Color topHoverColor = new Color(140f / 255f, 127f / 255f, 1f, 1f);
    public Color bottomHoverColor = new Color(92f / 255f, 191f / 255f, 1f, 1f);

    private Vector3 originalScale;
    private Image img;
    private Material matInstance;
    private bool isHovered = false;

    void Awake()
    {
        img = GetComponent<Image>();
        originalScale = transform.localScale;

        // Instanciamos el material para este botón
        matInstance = Instantiate(img.material);
        img.material = matInstance;

        // Inicializamos los colores normales
        matInstance.SetColor("_TopColor", topColor);
        matInstance.SetColor("_BottomColor", bottomColor);
    }

    void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale,
            isHovered ? originalScale * hoverScale : originalScale, Time.deltaTime * speed);

        matInstance.SetColor("_TopColor", isHovered ? topHoverColor : topColor);
        matInstance.SetColor("_BottomColor", isHovered ? bottomHoverColor : bottomColor);
    }

    public void OnPointerEnter(PointerEventData eventData) => isHovered = true;
    public void OnPointerExit(PointerEventData eventData) => isHovered = false;
}
