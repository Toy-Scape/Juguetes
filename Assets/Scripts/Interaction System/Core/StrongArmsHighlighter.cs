using System.Collections;
using System.Collections.Generic;
using InteractionSystem.Interfaces;
using UnityEngine;

namespace InteractionSystem.Core
{
    public class StrongArmsHighlighter : MonoBehaviour
    {
        [Header("Highlight Settings")]
        [SerializeField] private float detectionRadius = 5f;
        [SerializeField] private Color highlightColor = Color.green;
        [Tooltip("Controls the Fill Intensity")]
        [SerializeField, Range(0f, 10f)] private float rimPower = 3f;
        [Tooltip("If true, turns off highlighting when holding an object")]
        [SerializeField] private bool disableHighlightWhenGrabbing = true;
        [Header("Pulse Settings")]
        [SerializeField, Range(0f, 10f)] private float pulseSpeed = 1f;
        [SerializeField, Range(0f, 1f)] private float pulseMin = 0.5f;
        [SerializeField] private LayerMask interactableLayer = ~0;

        [Header("References")]
        [SerializeField] private LimbManager limbManager;
        [SerializeField] private GrabInteractor grabInteractor;

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

            if (grabInteractor == null)
                grabInteractor = GetComponent<GrabInteractor>();
            if (grabInteractor == null)
                grabInteractor = FindFirstObjectByType<GrabInteractor>();

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

            // Check if we are holding something
            bool isHolding = false;
            if (grabInteractor != null)
            {
                isHolding = grabInteractor.IsGrabbing || grabInteractor.IsPicking;
            }

            // Setup context for hack if needed
            LimbContext ctx = (limbManager != null) ? limbManager.GetContext() : null;
            GameObject realHeldObject = null;
            bool didHack = false;

            // If we are holding something...
            if (isHolding)
            {
                // If option is ENABLED (check on), we stop highlighting.
                if (disableHighlightWhenGrabbing)
                {
                    ClearAllHighlights();
                    return;
                }

                // If option is DISABLED (check off), we want to KEEP highlighting.
                // We mock empty hands so CanBeGrabbed() passes its "Hands Full" check.
                if (ctx != null)
                {
                    realHeldObject = ctx.HeldObject;
                    ctx.HeldObject = null; // MOCK EMPTY HANDS
                    didHack = true;
                }
            }

            Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius, interactableLayer);
            HashSet<Renderer> currentRenderers = new HashSet<Renderer>();

            try
            {
                foreach (var hit in hits)
                {
                    GameObject target = null;
                    bool shouldHighlight = false;

                    // 1. Check for Grabbable
                    IGrabbable grabbable = FindGrabbableInParents(hit.gameObject);
                    if (grabbable != null)
                    {
                        if (grabbable.CanBeGrabbed())
                        {
                            target = (grabbable as Component)?.gameObject;
                            shouldHighlight = true;
                        }
                    }

                    // 2. Check for Pickable if not Grabbable
                    if (!shouldHighlight)
                    {
                        IPickable pickable = FindPickableInParents(hit.gameObject);
                        if (pickable != null)
                        {
                            if (pickable.CanBePicked())
                            {
                                target = (pickable as Component)?.gameObject;
                                shouldHighlight = true;
                            }
                        }
                    }

                    if (shouldHighlight && target != null)
                    {
                        if (target == null) target = hit.gameObject;

                        // Don't highlight the object we are currently holding (if any)
                        // We rely on checking if it is the grabbed object from interactor
                        if (grabInteractor != null)
                        {
                            if (isHolding && grabInteractor.GrabbedObjectTransform != null &&
                                (target.transform == grabInteractor.GrabbedObjectTransform || target.transform.IsChildOf(grabInteractor.GrabbedObjectTransform)))
                                continue;

                            // Pickables might be parented to hand, so check hierarchy
                            if (target.transform.IsChildOf(transform))
                                continue;
                        }

                        // Apply to all renderers in the target
                        Renderer[] renderers = target.GetComponentsInChildren<Renderer>();
                        foreach (var r in renderers)
                        {
                            if (r is ParticleSystemRenderer) continue;
                            if (r is TrailRenderer) continue;

                            currentRenderers.Add(r);
                            ApplyHighlight(r);
                        }
                    }
                }
            }
            finally
            {
                // Restore real usage
                if (didHack && ctx != null)
                    ctx.HeldObject = realHeldObject;
            }

            // Remove highlight from objects that are no longer in range
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

        private void ApplyHighlight(Renderer r)
        {
            if (r == null) return;
            if (originalMaterials.ContainsKey(r)) return; // Already highlighted

            // Clean up any residual highlights from previous sessions or reloads
            var currentMats = new List<Material>(r.sharedMaterials);
            bool cleaned = false;
            for (int i = currentMats.Count - 1; i >= 0; i--)
            {
                if (currentMats[i] != null && currentMats[i].shader.name == "Custom/RimHighlight")
                {
                    currentMats.RemoveAt(i);
                    cleaned = true;
                }
            }

            if (cleaned)
            {
                // If we found residuals, apply the clean list to the renderer first
                // preventing us from baking the highlight into the "original" state
                r.materials = currentMats.ToArray();
            }

            // Save original (now guaranteed clean)
            originalMaterials[r] = r.sharedMaterials;

            // Add new material
            Material[] mats = r.sharedMaterials;
            Material[] newMats = new Material[mats.Length + 1];
            for (int i = 0; i < mats.Length; i++) newMats[i] = mats[i];
            newMats[newMats.Length - 1] = runtimeMaterial;

            r.materials = newMats;
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

        private IPickable FindPickableInParents(GameObject start)
        {
            Transform t = start.transform;
            while (t != null)
            {
                if (t.TryGetComponent<IPickable>(out var pickable)) return pickable;
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
