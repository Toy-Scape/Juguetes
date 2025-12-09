using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] private CinemachineInputAxisController inputController;
    public static CameraManager Instance;

    void Awake ()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Start ()
    {
        LockCursor();
        UnlockCameraMovement();
    }

    private void OnEnable ()
    {
        InputMapManager.OnActionMapChanged += HandleActionMapChanged;
    }

    private void OnDisable ()
    {
        InputMapManager.OnActionMapChanged -= HandleActionMapChanged;
    }

    private void HandleActionMapChanged (string newMap)
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

    public void LockCameraMovement ()
    {
        if (inputController != null)
            inputController.enabled = false;
    }

    public void UnlockCameraMovement ()
    {
        if (inputController != null)
            inputController.enabled = true;
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