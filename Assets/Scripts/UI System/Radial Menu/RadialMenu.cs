using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class RadialMenu : MonoBehaviour
{
    [SerializeField] private LimbManager limbManager;
    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] private Transform radialContainer;

    private List<Button> buttons = new();
    private Limb currentSelection;
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
        float angleStep = 360f / limbs.Count;
        float radius = 150f;

        for (int i = 0; i < limbs.Count; i++)
        {
            var limb = limbs[i];
            var buttonObj = Instantiate(buttonPrefab, radialContainer);
            var button = buttonObj.GetComponent<Button>();

            button.GetComponentInChildren<Text>().text = limb.LimbName;

            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector2 pos = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            buttonObj.GetComponent<RectTransform>().anchoredPosition = pos;

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
            Hide();
        }
    }

    private Limb GetSelectionFromMouse (Vector2 mousePos, Vector2 center, List<Limb> limbs)
    {
        Vector2 dir = mousePos - center;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360f;

        float angleStep = 360f / limbs.Count;
        int index = Mathf.FloorToInt(angle / angleStep);
        return limbs[index];
    }

    private Limb GetSelectionFromJoystick (Vector2 input, List<Limb> limbs)
    {
        if (input == Vector2.zero) return null;

        float angle = Mathf.Atan2(input.y, input.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360f;

        float angleStep = 360f / limbs.Count;
        int index = Mathf.FloorToInt(angle / angleStep);
        return limbs[index];
    }
}
