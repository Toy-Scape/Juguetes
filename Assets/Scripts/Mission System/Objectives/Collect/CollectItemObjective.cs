using Inventory; // Assuming PlayerInventory is in this namespace or accessible
using MissionSystem.Data;
using UnityEngine;

namespace MissionSystem.Runtime
{
    public class CollectItemObjective : Objective
    {
        private CollectItemObjectiveDefinition _def;
        private int _currentAmount;

        public CollectItemObjective(CollectItemObjectiveDefinition definition, Mission mission)
            : base(definition, mission)
        {
            _def = definition;
        }

        public override void Start()
        {
            base.Start();

            // Initial check
            var playerInventory = GameObject.FindFirstObjectByType<PlayerInventory>();
            if (playerInventory != null)
            {
                _currentAmount = playerInventory.GetItemCount(_def.TargetItem);
                UpdateProgress((float)_currentAmount / _def.TargetQuantity);

                // Listen for updates - using existing UnityEvent from PlayerInventory
                playerInventory.onItemAdded.AddListener(OnItemAdded);
                playerInventory.onItemRemoved.AddListener(OnItemRemoved);
            }
            else
            {
                Debug.LogWarning("[CollectItemObjective] PlayerInventory not found in scene!");
            }

            CheckCompletion();
        }

        private void OnItemAdded(ItemData item, int quantity)
        {
            if (State == ObjectiveState.Completed) return;
            if (item != _def.TargetItem) return;

            _currentAmount = GameObject.FindFirstObjectByType<PlayerInventory>().GetItemCount(_def.TargetItem);
            UpdateProgress((float)_currentAmount / _def.TargetQuantity);
            CheckCompletion();
        }

        private void OnItemRemoved(ItemData item, int quantity)
        {
            if (State == ObjectiveState.Completed) return;
            if (item != _def.TargetItem) return;

            _currentAmount = GameObject.FindFirstObjectByType<PlayerInventory>().GetItemCount(_def.TargetItem);
            UpdateProgress((float)_currentAmount / _def.TargetQuantity);
        }

        private void CheckCompletion()
        {
            if (_currentAmount >= _def.TargetQuantity)
            {
                Complete();
                // Cleanup listeners
                var playerInventory = GameObject.FindFirstObjectByType<PlayerInventory>();
                if (playerInventory != null)
                {
                    playerInventory.onItemAdded.RemoveListener(OnItemAdded);
                    playerInventory.onItemRemoved.RemoveListener(OnItemRemoved);
                }
            }
        }
    }
}
