using System.Collections.Generic;
using System.Linq;
using Inventory;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class LimbManager : MonoBehaviour
{
    public event System.Action<LimbSO> OnLimbChanged;

    [System.Serializable]
    public struct LimbSocketDefinition
    {
        public LimbSlot Slot;
        public Transform SocketTransform;
        public LimbSO DefaultLimb;
    }

    [SerializeField] private LimbSO DefaultLimb;
    [SerializeField] private ItemData DefaultLimbData;
    [SerializeField] private PlayerInventory inventory;
    [SerializeField] private List<LimbSocketDefinition> limbSockets;
    [SerializeField] private GameObject PoofParticlesPrefab;
    [SerializeField] private AudioSource PoofSoundEffect;

    private LimbContext context;
    private LimbSO equippedLimb;

    private void Awake()
    {
        context = GetComponent<LimbContext>();
        equippedLimb = DefaultLimb;
        this.transform.FindDeep(DefaultLimb.LimbNameOnModel)?.gameObject.SetActive(true);

        if (GetComponent<InteractionSystem.Core.StrongArmsHighlighter>() == null)
        {
            gameObject.AddComponent<InteractionSystem.Core.StrongArmsHighlighter>();
        }
    }

    public void EquipLimb(LimbSO newLimb)
    {
        if (newLimb == null || newLimb == equippedLimb) return;

        if (equippedLimb != null)
        {
            equippedLimb.OnUnequip(context);
            this.transform.FindDeep(equippedLimb.LimbNameOnModel)?.gameObject.SetActive(false);
        }

        Instantiate(PoofParticlesPrefab, transform.position, Quaternion.identity);
        if (PoofSoundEffect != null)
        {
            PoofSoundEffect.time = 0.12f;
            PoofSoundEffect.Play();
        }

        equippedLimb = newLimb;
        equippedLimb.OnEquip(context);
        this.transform.FindDeep(equippedLimb.LimbNameOnModel)?.gameObject.SetActive(true);

        OnLimbChanged?.Invoke(equippedLimb);
    }

    public void UseActive() => equippedLimb?.UseActive(context);
    public void UseSecondary() => equippedLimb?.UseSecondary(context);

    public LimbContext GetContext() => context;
    public LimbSO GetEquippedLimb() => equippedLimb;
    public ItemData GetDefaultLimbData() => DefaultLimbData;

    public List<LimbSO> GetAvailableLimbs() =>
        inventory.GetAllLimbs()
                 .Select(p => p.LimbSO)
                 .Prepend(DefaultLimb)
                 .ToList();
}
