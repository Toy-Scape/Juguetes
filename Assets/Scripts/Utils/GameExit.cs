using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameExit : MonoBehaviour
{
    [SerializeField] private GameObject _firstSelected;
    private void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);

            if (_firstSelected != null)
            {
                EventSystem.current.SetSelectedGameObject(_firstSelected);
            }
            else
            {
                var btn = GetComponent<Button>();
                if (btn != null)
                {
                    EventSystem.current.SetSelectedGameObject(gameObject);
                }
            }
        }
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
