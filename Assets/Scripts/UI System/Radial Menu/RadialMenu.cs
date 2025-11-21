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
        // limpiar botones previos
        foreach (var btn in buttons)
            Destroy(btn.gameObject);
        buttons.Clear();

        var limbs = limbManager.GetAvailableLimbs();
        if (limbs.Count == 0) return;

        float angleStep = 360f / limbs.Count;
        float radius = 150f;

        for (int i = 0; i < limbs.Count; i++)
        {
            var limb = limbs[i];
            var buttonObj = Instantiate(buttonPrefab, radialContainer);
            var button = buttonObj.GetComponent<Button>();

            // mostrar nombre de la extremidad
            button.GetComponentInChildren<TMP_Text>().text = limb.LimbName;

            // calcular posición radial
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector2 pos = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            buttonObj.GetComponent<RectTransform>().anchoredPosition = pos;

            // asignar acción de equipar
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
