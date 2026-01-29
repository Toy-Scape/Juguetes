using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InteractionSystem.Interfaces;

namespace InteractionSystem.Core
{
    public class StrongArmsHighlighter : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float detectionRadius = 5f;
        [SerializeField] private Color highlightColor = Color.green;
        [Tooltip("Controls the Fill Intensity")]
        [SerializeField, Range(0f, 10f)] private float rimPower = 3f;
        [Header("Pulse Settings")]
        [SerializeField, Range(0f, 10f)] private float pulseSpeed = 1f;
        [SerializeField, Range(0f, 1f)] private float pulseMin = 0.5f;
        [SerializeField] private LayerMask interactableLayer = ~0; 

        [Header("References")]
        [SerializeField] private LimbManager limbManager;

        // Cache of original materials to restore them accurately
        // Key: Renderer, Value: Original Material Array
        private Dictionary<Renderer, Material[]> originalMaterials = new Dictionary<Renderer, Material[]>();
        
        // The material instance we use for highlighting
        private Material runtimeMaterial;
        private Coroutine highlightCoroutine;

        private void Awake()
        {
            if (limbManager == null)
                limbManager = GetComponent<LimbManager>();
            
            if (limbManager == null)
                limbManager = FindFirstObjectByType<LimbManager>();

            // Create material at runtime using the shader
            Shader shader = Shader.Find("Custom/RimHighlight");
            if (shader != null)
            {
                runtimeMaterial = new Material(shader);
                UpdateMaterialProperties();
            }
            else
            {
                Debug.LogError("[StrongArmsHighlighter] Shader 'Custom/RimHighlight' not found!");
            }
        }

        private void OnValidate()
        {
            if (runtimeMaterial != null)
            {
                UpdateMaterialProperties();
            }
        }

        private void UpdateMaterialProperties()
        {
            if (runtimeMaterial == null) return;
            runtimeMaterial.SetColor("_RimColor", highlightColor);
            
            // Map inspector value 0-10 to Fill Intensity
            float fill = Mathf.Clamp01(rimPower / 20f); 
            runtimeMaterial.SetFloat("_FillIntensity", 0.1f + fill);

            runtimeMaterial.SetFloat("_PulseSpeed", pulseSpeed);
            runtimeMaterial.SetFloat("_PulseMin", pulseMin);
        }

        private void OnEnable()
        {
            if (limbManager != null)
            {
                limbManager.OnLimbChanged += HandleLimbChanged;
                HandleLimbChanged(limbManager.GetEquippedLimb());
            }
        }

        private void OnDisable()
        {
            if (limbManager != null)
                limbManager.OnLimbChanged -= HandleLimbChanged;
            
            StopHighlighting();
        }

        private void HandleLimbChanged(LimbSO limb)
        {
            if (limb is StrongArmSO)
            {
                StartHighlighting();
            }
            else
            {
                StopHighlighting();
            }
        }

        private void StartHighlighting()
        {
            if (runtimeMaterial == null) return;
            if (highlightCoroutine != null) StopCoroutine(highlightCoroutine);
            highlightCoroutine = StartCoroutine(HighlightRoutine());
        }

        private void StopHighlighting()
        {
            if (highlightCoroutine != null)
            {
                StopCoroutine(highlightCoroutine);
                highlightCoroutine = null;
            }
            ClearAllHighlights();
        }

        private IEnumerator HighlightRoutine()
        {
            WaitForSeconds wait = new WaitForSeconds(0.2f);

            while (true)
            {
                HighlightNearbyObjects();
                yield return wait;
            }
        }

        private void HighlightNearbyObjects()
        {
            if (runtimeMaterial == null) return;

            Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius, interactableLayer);
            HashSet<Renderer> currentRenderers = new HashSet<Renderer>();

            foreach (var hit in hits)
            {
                IGrabbable grabbable = FindGrabbableInParents(hit.gameObject);
                
                if (grabbable != null && grabbable.CanBeGrabbed())
                {
                    GameObject target = (grabbable as Component)?.gameObject;
                    if (target == null) target = hit.gameObject;

                    // Apply to all renderers in the target
                    Renderer[] renderers = target.GetComponentsInChildren<Renderer>();
                    foreach (var r in renderers)
                    {
                        if (r is ParticleSystemRenderer) continue; // Skip particles

                        currentRenderers.Add(r);
                        
                        if (!originalMaterials.ContainsKey(r))
                        {
                            AddHighlight(r);
                        }
                    }
                }
            }

            // Cleanup renderers that are no longer in range
            List<Renderer> toRemove = new List<Renderer>();
            foreach (var r in originalMaterials.Keys)
            {
                if (!currentRenderers.Contains(r))
                {
                    toRemove.Add(r);
                }
            }

            foreach (var r in toRemove)
            {
                RemoveHighlight(r);
            }
        }

        private void AddHighlight(Renderer r)
        {
            if (r == null) return;
            
            // Store original
            originalMaterials[r] = r.sharedMaterials;

            // Append highlight material
            Material[] newMats = new Material[r.sharedMaterials.Length + 1];
            System.Array.Copy(r.sharedMaterials, newMats, r.sharedMaterials.Length);
            newMats[newMats.Length - 1] = runtimeMaterial;
            
            r.materials = newMats; // Use .materials to instantiate if needed, or .sharedMaterials if we want to modify all instances (probably bad). 
            // Using .materials creates instance. Good for not affecting assets.
        }

        private void RemoveHighlight(Renderer r)
        {
            if (r == null) 
            {
                originalMaterials.Remove(r);
                return;
            }

            if (originalMaterials.TryGetValue(r, out Material[] originals))
            {
                r.materials = originals; // Restore
                originalMaterials.Remove(r);
            }
        }

        private void ClearAllHighlights()
        {
            // Create a list copy to avoid modification errors during iteration
            List<Renderer> allKeys = new List<Renderer>(originalMaterials.Keys);
            foreach (var r in allKeys)
            {
                RemoveHighlight(r);
            }
            originalMaterials.Clear();
        }

        private IGrabbable FindGrabbableInParents(GameObject start)
        {
            Transform t = start.transform;
            while (t != null)
            {
                if (t.TryGetComponent<IGrabbable>(out var grabbable)) return grabbable;
                t = t.parent;
            }
            return null;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = highlightColor;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
        }
    }
}
