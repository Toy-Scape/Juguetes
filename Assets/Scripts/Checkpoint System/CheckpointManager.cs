using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.AntiGravityController;
using Inventory; // Access to PlayerInventory and ItemData
using UnityEngine;

namespace CheckpointSystem
{
    public class CheckpointManager : MonoBehaviour
    {
        public static CheckpointManager Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private bool autoSave = true;
        [SerializeField] private string saveKey = "PlayerSaveData";
        [SerializeField] private DeadZone deadZone;

        private GameData currentGameData;
        private List<Checkpoint> allCheckpoints;
        private List<IResettable> resettables;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            currentGameData = new GameData();
            resettables = new List<IResettable>();
        }

        private void Start()
        {
            // Find all resettables in the scene (expensive, do it once or on scene load)
            FindResettables();

            // Load game if exists
            LoadGame();
        }

        public void RegisterResettable(IResettable resettable)
        {
            if (!resettables.Contains(resettable))
            {
                resettables.Add(resettable);
            }
        }

        public void UnregisterResettable(IResettable resettable)
        {
            if (resettables.Contains(resettable))
            {
                resettables.Remove(resettable);
            }
        }

        private void FindResettables()
        {
            // Note: FindObjectsOfType includes inactive objects only if specified, 
            // but usually we want active ones.
            // For interfaces, we might need to search MonoBehaviours and check implementation.
            // This can be slow, better if objects register themselves in Start().
            // For now, I'll rely on objects registering themselves or manual assignment if needed.
            // But to be safe and robust as requested:
            var monoBehaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            foreach (var mb in monoBehaviours)
            {
                if (mb is IResettable resettable)
                {
                    RegisterResettable(resettable);
                }
            }
        }

        public void SetCheckpoint(Checkpoint checkpoint)
        {
            // Deactivate other checkpoints visually if needed
            if (allCheckpoints == null) allCheckpoints = FindObjectsByType<Checkpoint>(FindObjectsSortMode.None).ToList();

            foreach (var cp in allCheckpoints)
            {
                if (cp != checkpoint) cp.Deactivate();
            }

            currentGameData.lastCheckpointPosition = checkpoint.transform.position;
            currentGameData.lastCheckpointRotation = checkpoint.transform.rotation;

            // Update Dead Zone Position
            if (deadZone != null)
            {
                float newY = checkpoint.transform.position.y - checkpoint.DeadZoneOffset;
                Vector3 pos = deadZone.transform.position;
                pos.y = newY;
                deadZone.transform.position = pos;
            }

            SaveProgress();
        }

        public void SaveProgress()
        {
            // Save Inventory
            var playerInventory = FindFirstObjectByType<PlayerInventory>();
            if (playerInventory != null)
            {
                currentGameData.inventoryItems.Clear();
                foreach (var item in playerInventory.GetAllItems())
                {
                    currentGameData.inventoryItems.Add(new GameData.InventoryEntry
                    {
                        itemName = item.Data.name, // Assuming name is unique key
                        quantity = item.Quantity
                    });
                }

                currentGameData.inventoryLimbs.Clear();
                foreach (var item in playerInventory.GetAllLimbItems())
                {
                    currentGameData.inventoryLimbs.Add(new GameData.InventoryEntry
                    {
                        itemName = item.Data.name,
                        quantity = item.Quantity
                    });
                }
            }

            // Save Puzzles (Assumed handled elsewhere via AddSolvedPuzzle)

            string json = JsonUtility.ToJson(currentGameData);
            PlayerPrefs.SetString(saveKey, json);
            PlayerPrefs.Save();
            Debug.Log("Game Saved.");
        }

        public void LoadGame()
        {
            if (PlayerPrefs.HasKey(saveKey))
            {
                string json = PlayerPrefs.GetString(saveKey);
                currentGameData = JsonUtility.FromJson<GameData>(json);
                Debug.Log("Game Loaded.");
            }
            else
            {
                // Default start
                var player = FindFirstObjectByType<AntiGravityPlayerController>().gameObject;
                if (player != null)
                {
                    currentGameData.lastCheckpointPosition = player.transform.position;
                    currentGameData.lastCheckpointRotation = player.transform.rotation;
                }
            }
        }

        public void RespawnPlayer()
        {
            // 1. Reset Player Position
            var player = FindFirstObjectByType<AntiGravityPlayerController>().gameObject;
            if (player != null)
            {
                var respawnComponent = player.GetComponent<PlayerRespawn>();
                if (respawnComponent != null)
                {
                    respawnComponent.Respawn(currentGameData.lastCheckpointPosition, currentGameData.lastCheckpointRotation);
                }
                else
                {
                    // Fallback if no specific component
                    var cc = player.GetComponent<CharacterController>();
                    if (cc != null) cc.enabled = false;
                    player.transform.position = currentGameData.lastCheckpointPosition;
                    player.transform.rotation = currentGameData.lastCheckpointRotation;
                    if (cc != null) cc.enabled = true;
                }
            }

            // 2. Reset Enemies/Objects
            foreach (var resettable in resettables)
            {
                resettable.ResetState();
            }

            // 3. Restore Inventory
            RestoreInventory();
        }

        private void RestoreInventory()
        {
            var playerInventory = FindFirstObjectByType<PlayerInventory>();
            if (playerInventory == null) return;

            playerInventory.ClearInventory();

            if (currentGameData.inventoryItems.Count > 0)
            {
                foreach (var itemEntry in currentGameData.inventoryItems)
                {
                    // Try to load ItemData from Resources
                    // Assumption: ItemData assets are located in a "Resources/Items" folder
                    var itemData = Resources.Load<ItemData>($"Items/{itemEntry.itemName}");

                    if (itemData != null)
                    {
                        playerInventory.AddItem(itemData, itemEntry.quantity);
                    }
                    else
                    {
                        Debug.LogWarning($"Could not find ItemData for '{itemEntry.itemName}'. Make sure it is in a 'Resources/Items' folder.");
                    }
                }
            }

            if (currentGameData.inventoryLimbs.Count > 0)
            {
                foreach (var limbEntry in currentGameData.inventoryLimbs)
                {
                    // Try to load Limb ItemData from Resources. Assumes same structure or unique names findable.
                    // IMPORTANT: The user said they moved things to Resources. We assume path "Items/" works or we might need "Limbs/"
                    // We will try "Items/" first, and if not found, maybe just "Limbs/" or empty path if they are at root.
                    // Simplest first attempt: Try same path as items, or root if unique.

                    var itemData = Resources.Load<ItemData>($"Items/{limbEntry.itemName}");
                    if (itemData == null)
                    {
                        // Try Limbs specific folder just in case
                        itemData = Resources.Load<ItemData>($"Limbs/{limbEntry.itemName}");
                    }
                    if (itemData == null)
                    {
                        // Basic load by name if in root or unknown path structure but unique name
                        itemData = Resources.Load<ItemData>(limbEntry.itemName);
                    }

                    if (itemData != null)
                    {
                        playerInventory.AddItem(itemData, limbEntry.quantity);
                    }
                    else
                    {
                        Debug.LogWarning($"Could not find Limb ItemData for '{limbEntry.itemName}'. Checked 'Items/', 'Limbs/' and root in Resources.");
                    }
                }
            }
        }

        public void AddSolvedPuzzle(string puzzleId)
        {
            if (!currentGameData.solvedPuzzles.Contains(puzzleId))
            {
                currentGameData.solvedPuzzles.Add(puzzleId);
                SaveProgress();
            }
        }

        public bool IsPuzzleSolved(string puzzleId)
        {
            return currentGameData.solvedPuzzles.Contains(puzzleId);
        }

        public Vector3 GetLastCheckpointPosition()
        {
            return currentGameData.lastCheckpointPosition;
        }
    }
}
