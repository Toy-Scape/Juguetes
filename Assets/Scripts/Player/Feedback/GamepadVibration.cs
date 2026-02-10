using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class GamepadVibration : MonoBehaviour
{
    private Coroutine vibrationCoroutine;

    // Impulse state
    private bool isImpulseActive = false;
    private float impulseLow;
    private float impulseHigh;

    // Continuous state
    private bool isContinuousActive = false;
    private float continuousLow;
    private float continuousHigh;

    private void Update()
    {
        if (Gamepad.current == null) return;

        // Priority logic: Impulse overrides Continuous
        if (isImpulseActive)
        {
            Gamepad.current.SetMotorSpeeds(impulseLow, impulseHigh);
        }
        else if (isContinuousActive)
        {
            Gamepad.current.SetMotorSpeeds(continuousLow, continuousHigh);
        }
        else
        {
            Gamepad.current.SetMotorSpeeds(0f, 0f);
        }
    }

    /// <summary>
    /// Triggers a one-shot vibration (Impulse). Higher priority than continuous.
    /// </summary>
    public void Vibrate(float lowFrequency, float highFrequency, float duration)
    {
        if (vibrationCoroutine != null) StopCoroutine(vibrationCoroutine);
        vibrationCoroutine = StartCoroutine(ImpulseRoutine(lowFrequency, highFrequency, duration));
    }

    public void SetContinuousVibration(float lowFreq, float highFreq)
    {
        isContinuousActive = true;
        continuousLow = lowFreq;
        continuousHigh = highFreq;
    }

    public void StopContinuousVibration()
    {
        isContinuousActive = false;
        continuousLow = 0f;
        continuousHigh = 0f;
    }

    public void StopAllVibration()
    {
        isImpulseActive = false;
        isContinuousActive = false;
        if (vibrationCoroutine != null) StopCoroutine(vibrationCoroutine);
        if (Gamepad.current != null) Gamepad.current.SetMotorSpeeds(0f, 0f);
    }

    private IEnumerator ImpulseRoutine(float lowFreq, float highFreq, float time)
    {
        isImpulseActive = true;
        impulseLow = lowFreq;
        impulseHigh = highFreq;

        yield return new WaitForSeconds(time);

        isImpulseActive = false;
        vibrationCoroutine = null;
    }

    private void OnDisable()
    {
        StopAllVibration();
    }

    private void OnApplicationQuit()
    {
        StopAllVibration();
    }
}
