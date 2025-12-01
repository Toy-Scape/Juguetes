using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RadialMenu : MonoBehaviour
{
    [SerializeField] private LimbManager limbManager;
    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] private Transform radialContainer;
    [SerializeField] private TMP_Text centerLabel;


    [Header("Radial Settings")]
    [SerializeField] private float innerRadius = 100f;
    [SerializeField] private float outerRadius = 180f;
    [SerializeField] private int segments = 32;
    [SerializeField] private float paddingAngle = 0.4f;
    [SerializeField] private float paddingRadial = 10f;
    [SerializeField] private float globalRotationOffset = 90f;

    private List<GameObject> buttons = new();
    private LimbSO currentSelection;
    private bool isOpen = false;
    private LimbSO lastSelection;

    void Start ()
    {
        PopulateMenu();
        Hide();
    }

    public bool CanBeOpened ()
    {
        var limbs = limbManager.GetAvailableLimbs();
        return limbs.Count > 1;
    }

    public void Show ()
    {
        PopulateMenu();

        isOpen = true;
        gameObject.SetActive(true);
    }

    public void Hide ()
    {
        isOpen = false;
        gameObject.SetActive(false);
    }

    public void PopulateMenu ()
    {
        foreach (var obj in buttons)
            Destroy(obj);
        buttons.Clear();

        var limbs = limbManager.GetAvailableLimbs();
        if (limbs.Count == 0) return;

        float angleStep = 360f / limbs.Count;

        for (int i = 0; i < limbs.Count; i++)
        {
            var limb = limbs[i];
            var prefabObj = Instantiate(buttonPrefab, radialContainer);
            var arcGraphic = prefabObj.GetComponent<CurvedSegmentGraphic>();
            var button = prefabObj.GetComponentInChildren<Button>();
            var image = button.GetComponentInChildren<Image>(true);
            var text = button.GetComponentInChildren<TMP_Text>(true);
            image.sprite = limb.LimbIcon;
            text.text = "";

            float startAngle = i * angleStep + paddingAngle + globalRotationOffset;
            float endAngle = (i + 1) * angleStep - paddingAngle + globalRotationOffset;

            arcGraphic.Configure(innerRadius + paddingRadial, outerRadius - paddingRadial, startAngle, endAngle, segments, Vector2.zero, 0);

            float midRad = ((startAngle + endAngle) * 0.5f) * Mathf.Deg2Rad;
            float effectiveInner = innerRadius + paddingRadial;
            float effectiveOuter = outerRadius - paddingRadial;
            float midRadius = (effectiveInner + effectiveOuter) * 0.5f;
            Vector2 centerPoint = new Vector2(Mathf.Cos(midRad), Mathf.Sin(midRad)) * midRadius;

            var rt = button.GetComponent<RectTransform>();
            rt.anchoredPosition = centerPoint;
            rt.rotation = Quaternion.identity;

            //text.rectTransform.anchoredPosition = Vector2.zero;
            //text.rectTransform.rotation = Quaternion.identity;

            button.onClick.AddListener(() => limbManager.EquipLimb(limb));
            buttons.Add(prefabObj);
        }
    }

    public void SelectWithMouse (Vector2 mousePos, Vector2 center)
    {
        var limbs = limbManager.GetAvailableLimbs();
        currentSelection = GetSelectionFromMouse(mousePos, center, limbs);
        HighlightSelection(currentSelection);
    }

    public void SelectWithJoystick (Vector2 input)
    {
        var limbs = limbManager.GetAvailableLimbs();
        currentSelection = GetSelectionFromJoystick(input, limbs);
        HighlightSelection(currentSelection);
    }

    public void ConfirmSelection ()
    {
        if (isOpen && currentSelection != null)
        {
            limbManager.EquipLimb(currentSelection);
        }
    }

    private LimbSO GetSelectionFromMouse (Vector2 mousePos, Vector2 center, List<LimbSO> limbs)
    {
        if (limbs.Count == 0) return null;

        Vector2 dir = mousePos - center;
        float dist = dir.magnitude;

        if (dist < innerRadius)
            return null;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360f;
        angle = (angle - globalRotationOffset + 360f) % 360f;

        float angleStep = 360f / limbs.Count;
        int index = Mathf.FloorToInt(angle / angleStep);
        return limbs[index];
    }

    private LimbSO GetSelectionFromJoystick (Vector2 input, List<LimbSO> limbs, float deadzone = 0.2f)
    {
        if (limbs.Count == 0) return null;

        if (input.magnitude < deadzone)
            return lastSelection;

        float angle = Mathf.Atan2(input.y, input.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360f;
        angle = (angle - globalRotationOffset + 360f) % 360f;

        float angleStep = 360f / limbs.Count;
        int index = Mathf.FloorToInt(angle / angleStep);

        lastSelection = limbs[index];
        return lastSelection;
    }

    private void HighlightSelection (LimbSO selection)
    {
        var limbs = limbManager.GetAvailableLimbs();
        for (int i = 0; i < buttons.Count; i++)
        {
            var arc = buttons[i].GetComponent<CurvedSegmentGraphic>();
            if (arc != null)
            {
                bool isSelected = limbs[i] == selection;
                arc.SetHover(isSelected);
            }
        }

        if (centerLabel != null)
            centerLabel.text = selection != null ? selection.LimbName : "";
    }
}
