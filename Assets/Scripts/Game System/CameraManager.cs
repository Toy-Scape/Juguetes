using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    private CinemachineInputAxisController inputController;

    void Awake ()
    {
        inputController = FindFirstObjectByType<CinemachineInputAxisController>();
        LockCursor();
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
