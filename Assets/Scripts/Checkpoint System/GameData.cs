using System.Collections.Generic;
using UnityEngine;

namespace CheckpointSystem
{
    [System.Serializable]
    public class GameData
    {
        public Vector3 lastCheckpointPosition;
        public Quaternion lastCheckpointRotation;

        // Inventory serialization
        // We store item names and quantities. 
        // Assumption: ItemData names are unique.
        [System.Serializable]
        public class InventoryEntry
        {
            public string itemName;
            public int quantity;
        }
        public List<InventoryEntry> inventoryItems = new List<InventoryEntry>();
        public List<InventoryEntry> inventoryLimbs = new List<InventoryEntry>();

        // Puzzle progress
        // Store IDs of solved puzzles or collected unique items
        public List<string> solvedPuzzles = new List<string>();

        public GameData()
        {
            // Default values
            lastCheckpointPosition = Vector3.zero;
            lastCheckpointRotation = Quaternion.identity;
        }
    }
}
