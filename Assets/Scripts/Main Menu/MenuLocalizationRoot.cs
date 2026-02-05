using UnityEngine;

public class MenuLocalizationRoot : MonoBehaviour
{
    private void Start()
    {
        foreach (var l in GetComponentsInChildren<ILocalizable>(true))
            l.Localize();
    }
}
