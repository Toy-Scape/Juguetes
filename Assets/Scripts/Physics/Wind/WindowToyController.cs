using UnityEngine;
using Domain.StaticNpc;
using CinematicSystem.Core;
using CinematicSystem.Application;

[RequireComponent(typeof(WindAffected))]
public class WindowToyController : MonoBehaviour
{
    WindAffected wind;

    public FatherNPCState fatherState;
    public StaticNpcActionHandler npcActions;

    public CinematicAsset toyFallCinematic;
    ToyPickupController pickup;
    public GameObject player;
    public Transform fan;
    public Transform cinematicCamera;
    bool toyEventTriggered = false;
    Vector3 fanStartPos;
    Quaternion fanStartRot;
    [SerializeField] Dialogue needDistractionDialogue;

    void Awake()
    {
        wind = GetComponent<WindAffected>();
        pickup = GetComponent<ToyPickupController>();
        fanStartPos = fan.position;
        fanStartRot = fan.rotation;
    }

    void OnEnable()
    {
        wind.OnBlown += HandleBlown;
    }

    void OnDisable()
    {
        wind.OnBlown -= HandleBlown;
    }

    void HandleBlown(WindAffected obj)
    {
        if (fatherState.isDistracted) return;

        if (toyEventTriggered) return;

        toyEventTriggered = true;

        Debug.Log("Toy blown!");
        var cinematicPlayer = FindFirstObjectByType<CinematicPlayer>();

        pickup.SaveWindowPose();

        PrepareCinematic();

        cinematicPlayer.Play(toyFallCinematic);

        npcActions.PlayActionsByIdSequence(
            "stand-up, move-toy, gather-toy, stand-gather, place-toy, move-chair, sit-chair"
        );
    }

    void PrepareCinematic()
    {
        var grabInteractor = player.GetComponent<GrabInteractor>();
        if (grabInteractor != null && grabInteractor.IsGrabbing)
        {
            grabInteractor.ReleaseGrab();
        }

        var controller = player.GetComponent<PlayerController>();
        if (controller != null)
        {
            controller.Context.Velocity = Vector3.zero;
            controller.Context.MoveInput = Vector2.zero;
        }

        player.SetActive(false);

        fan.position = fanStartPos;
        fan.rotation = fanStartRot;

        cinematicCamera.position += Vector3.up * 15f;
    }

    public void RestoreGameplay()
    {
        player.SetActive(true);

        var controller = player.GetComponent<PlayerController>();
        if (controller != null)
        {
            controller.ResetAfterCinematic();
        }
    }

    public void ResetToyEvent()
    {
        toyEventTriggered = false;
        wind.ResetBlownState();
    }

    public void PlayNeedDistractionThought()
    {
        if (needDistractionDialogue == null)
            return;

        if (DialogueBox.Instance != null)
        {
            DialogueBox.Instance.StartDialogue(needDistractionDialogue);
        }
    }
}