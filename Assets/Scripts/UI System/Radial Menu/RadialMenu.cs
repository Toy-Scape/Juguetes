using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RadialMenu : MonoBehaviour
{
    [SerializeField] private LimbManager limbManager;
    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] private Transform radialContainer;

    private List<Button> buttons = new();
    private LimbSO currentSelection;
    private bool isOpen = false;

    void Start ()
    {
        PopulateMenu();
        Hide();
    }

    public void Show ()
    {
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
        foreach (var btn in buttons)
            Destroy(btn.gameObject);
        buttons.Clear();

        var limbs = limbManager.GetAvailableLimbs();
        if (limbs.Count == 0) return;

        float angleStep = 360f / limbs.Count;
        float innerRadius = 100f;
        float outerRadius = 180f;
        int segments = 32;

        float paddingAngle = 0.4f;
        float paddingRadial = 10f;

        for (int i = 0; i < limbs.Count; i++)
        {
            var limb = limbs[i];
            var buttonObj = Instantiate(buttonPrefab, radialContainer);
            var button = buttonObj.GetComponent<Button>();

            var text = button.GetComponentInChildren<TMP_Text>(true);
            text.text = limb.LimbName;

            float angleRad = i * angleStep * Mathf.Deg2Rad;
            Vector2 pos = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad)) * ((innerRadius + outerRadius) * 0.5f);

            var rt = buttonObj.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.rotation = Quaternion.identity;

            var arcGraphic = buttonObj.GetComponentInChildren<CurvedSegmentGraphic>(true);
            if (arcGraphic == null) arcGraphic = buttonObj.AddComponent<CurvedSegmentGraphic>();

            float startAngle = i * angleStep + paddingAngle;
            float endAngle = (i + 1) * angleStep - paddingAngle;
            Vector2 centerOffset = -pos;

            arcGraphic.Configure(innerRadius + paddingRadial, outerRadius - paddingRadial, startAngle, endAngle, segments, centerOffset, 0);

            float midRad = ((startAngle + endAngle) * 0.5f) * Mathf.Deg2Rad;
            float labelRadius = (innerRadius + paddingRadial) + ((outerRadius - paddingRadial) - (innerRadius + paddingRadial)) * 0.35f;
            Vector2 labelLocalPos = new Vector2(Mathf.Cos(midRad), Mathf.Sin(midRad)) * labelRadius;
            text.rectTransform.anchoredPosition = labelLocalPos;
            text.rectTransform.rotation = Quaternion.identity;

            button.onClick.AddListener(() => limbManager.EquipLimb(limb));
            buttons.Add(button);
        }
    }

    public void SelectWithMouse (Vector2 mousePos, Vector2 center)
    {
        var limbs = limbManager.GetAvailableLimbs();
        currentSelection = GetSelectionFromMouse(mousePos, center, limbs);
    }

    public void SelectWithJoystick (Vector2 input)
    {
        var limbs = limbManager.GetAvailableLimbs();
        currentSelection = GetSelectionFromJoystick(input, limbs);
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
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360f;

        float angleStep = 360f / limbs.Count;
        int index = Mathf.FloorToInt(angle / angleStep);
        return limbs[index];
    }

    private LimbSO GetSelectionFromJoystick (Vector2 input, List<LimbSO> limbs)
    {
        if (input == Vector2.zero || limbs.Count == 0) return null;

        float angle = Mathf.Atan2(input.y, input.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360f;

        float angleStep = 360f / limbs.Count;
        int index = Mathf.FloorToInt(angle / angleStep);
        return limbs[index];
    }
}
