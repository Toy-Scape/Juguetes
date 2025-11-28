using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] private CinemachineInputAxisController inputController;
    public static CameraManager instance;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            LockCursor();
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    public void LockCameraMovement ()
    {
        if (inputController != null)
        {
            inputController.enabled = false;
        }
    }

    public void UnlockCameraMovement ()
    {
        if (inputController != null)
        {
            inputController.enabled = true;
        }
    }

    public void LockCursor ()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void UnlockCursor ()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
