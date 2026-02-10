using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

public class EventSystemSingleton : MonoBehaviour
{
    public static EventSystemSingleton Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (GetComponent<EventSystem>() == null)
            gameObject.AddComponent<EventSystem>();
        if (GetComponent<InputSystemUIInputModule>() == null)
            gameObject.AddComponent<InputSystemUIInputModule>();
    }
}
