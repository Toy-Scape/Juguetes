using Inventory; // Assuming ItemData is here
using MissionSystem.Runtime;
using UnityEngine;

namespace MissionSystem.Data
{
    [CreateAssetMenu(fileName = "CollectItemObjective", menuName = "Mission/Objectives/Collect Item")]
    public class CollectItemObjectiveDefinition : ObjectiveDefinition
    {
        [Header("Collection Config")]
        [SerializeField] private ItemData targetItem;
        [SerializeField] private int targetQuantity = 1;

        public ItemData TargetItem => targetItem;
        public int TargetQuantity => targetQuantity;

        public void Init(string desc, ItemData item, int quantity)
        {
            this.description = desc;
            this.targetItem = item;
            this.targetQuantity = quantity;
        }

        public override Objective CreateRuntimeInstance(Mission mission)
        {
            return new CollectItemObjective(this, mission);
        }
    }
}
