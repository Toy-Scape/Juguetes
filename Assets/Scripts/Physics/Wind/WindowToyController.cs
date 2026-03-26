using UnityEngine;
using Domain.StaticNpc;
using CinematicSystem.Core;
using CinematicSystem.Application;

[RequireComponent(typeof(WindAffectedSimpleFall))]
public class WindowToyController : MonoBehaviour
{
    WindAffectedSimpleFall windSimpleFall;

    public FatherNPCState fatherState;
    public StaticNpcActionHandler npcActions;
    public StaticNpcActionHandler stickyActions;
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
        windSimpleFall = GetComponent<WindAffectedSimpleFall>();
        pickup = GetComponent<ToyPickupController>();
        fanStartPos = fan.position;
        fanStartRot = fan.rotation;
    }

    void OnEnable()
    {
        windSimpleFall.OnBlown += HandleBlown;
    }

    void OnDisable()
    {
        windSimpleFall.OnBlown -= HandleBlown;
    }

    void HandleBlown(WindAffectedSimpleFall obj)
    {
        if (fatherState.isDistracted)
        {
            stickyActions.PlayActionsByIdSequence(
            "Fall, Impact, Roll, Lay"
        );
            return;
        }

        if (toyEventTriggered) return;

        toyEventTriggered = true;

        Debug.Log("Toy blown!");
        var cinematicPlayer = FindFirstObjectByType<CinematicPlayer>();

        pickup.SaveWindowPose();

        PrepareCinematic();

        cinematicPlayer.Play(toyFallCinematic);

        npcActions.PlayActionsByIdSequence(
            "stand-up, move-toy, gather-toy, place-toy, move-chair, sit-chair"
        );

        stickyActions.PlayActionsByIdSequence(
            "Fall, Impact, Roll, Lay"
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
        windSimpleFall.ResetBlownState();
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