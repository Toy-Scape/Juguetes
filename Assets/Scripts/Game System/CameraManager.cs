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
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Start()
    {
        LockCursor();
        UnlockCameraMovement();
    }

    private void OnEnable()
    {
        InputManager.OnActionMapChanged += HandleActionMapChanged;
        RadialMenuController.OnRadialOpen += HandleRadialOpen;
        RadialMenuController.OnRadialClose += HandleRadialClose;
    }

    private void OnDisable()
    {
        InputManager.OnActionMapChanged -= HandleActionMapChanged;
        RadialMenuController.OnRadialOpen -= HandleRadialOpen;
        RadialMenuController.OnRadialClose -= HandleRadialClose;
    }

    private void HandleActionMapChanged(string newMap)
    {
        if (newMap == ActionMaps.UI)
        {
            UnlockCursor();
            LockCameraMovement();
        }
        else
        {
            LockCursor();
            UnlockCameraMovement();
        }
    }

    private void HandleRadialOpen()
    {
        LockCameraMovement();
        UnlockCursor();
    }

    private void HandleRadialClose()
    {
        UnlockCameraMovement();
        LockCursor();
    }

    public void LockCameraMovement()
    {
        if (inputController != null)
            inputController.enabled = false;
    }

    public void UnlockCameraMovement()
    {
        if (inputController != null)
            inputController.enabled = true;
    }

    public void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
