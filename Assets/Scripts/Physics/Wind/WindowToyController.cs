using UnityEngine;
using Domain.StaticNpc;

[RequireComponent(typeof(WindAffected))]
public class WindowToyController : MonoBehaviour
{
    WindAffected wind;
    
    public FatherNPCState fatherState;
    public StaticNpcActionHandler npcActions;

    void Awake()
    {
        wind = GetComponent<WindAffected>();
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
        Debug.Log("Toy blown!");

        if (!fatherState.isDistracted)
        {
            npcActions.PlayActionsByIdSequence("stand-up, move-toy, Gather, stand-gather, place-toy, move-chair, sit-chair");
        }
    }
}