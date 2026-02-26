using System;
using Unity.Cinemachine;
using UnityEngine;

public class VentilatorGrabEvents : MonoBehaviour
{
    [SerializeField] private CinemachineCamera VentilatorCamera;
    private void OnEnable()
    {
        var grabbable = GetComponent<Grabbable>();
        grabbable.OnObjectGrabbed += HandleGrabbed;
        grabbable.OnObjectReleased += HandleReleased;
    }

    private void HandleGrabbed()
    {
        VentilatorCamera.Priority = 99;
    }
    private void HandleReleased()
    {
        VentilatorCamera.Priority = -1;
    }

}
