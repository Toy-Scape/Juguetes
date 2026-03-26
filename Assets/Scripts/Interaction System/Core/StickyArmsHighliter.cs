using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InteractionSystem.Core
{
    public class StickyArmsHighlighter : MonoBehaviour
    {
        [Header("Highlight Settings")]
        [SerializeField] private float detectionRadius = 5f;
        [SerializeField] private Color highlightColor = Color.cyan;
        [SerializeField, Range(0f, 10f)] private float rimPower = 3f;

        [Header("Pulse Settings")]
        [SerializeField, Range(0f, 10f)] private float pulseSpeed = 1f;
        [SerializeField, Range(0f, 1f)] private float pulseMin = 0.5f;

        [SerializeField] private LayerMask climbableLayer = ~0;

        [Header("References")]
        [SerializeField] private LimbManager limbManager;

        private Dictionary<Renderer, Material[]> originalMaterials = new();
        private Material runtimeMaterial;
        private Coroutine highlightCoroutine;

        private void Awake()
        {
            if (limbManager == null)
                limbManager = GetComponent<LimbManager>();

            if (limbManager == null)
                limbManager = FindFirstObjectByType<LimbManager>();

            Shader shader = Shader.Find("Custom/RimHighlight");
            if (shader != null)
            {
                runtimeMaterial = new Material(shader);
                UpdateMaterialProperties();
            }
            else
            {
                Debug.LogError("[SuctionArmsHighlighter] Shader not found!");
            }
        }

        private void OnValidate()
        {
            if (runtimeMaterial != null)
                UpdateMaterialProperties();
        }

        private void UpdateMaterialProperties()
        {
            runtimeMaterial.SetColor("_RimColor", highlightColor);

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
            // ⚠️ Cambia esto por tu ScriptableObject real
            if (limb is OctopusTentaclesSO)
                StartHighlighting();
            else
                StopHighlighting();
        }

        private void StartHighlighting()
        {
            if (runtimeMaterial == null) return;

            if (highlightCoroutine != null)
                StopCoroutine(highlightCoroutine);

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
                HighlightNearbySurfaces();
                yield return wait;
            }
        }

        private void HighlightNearbySurfaces()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius, climbableLayer);
            HashSet<Renderer> currentRenderers = new();

            foreach (var hit in hits)
            {
                ClimbableWall climbable = FindClimbableInParents(hit.gameObject);

                if (climbable != null && climbable.CanBeClimbed())
                {
                    GameObject target = climbable.gameObject;

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

            // Quitar highlight a los que ya no están
            List<Renderer> toRemove = new();

            foreach (var r in originalMaterials.Keys)
            {
                if (!currentRenderers.Contains(r))
                    toRemove.Add(r);
            }

            foreach (var r in toRemove)
                RemoveHighlight(r);
        }

        private void ApplyHighlight(Renderer r)
        {
            if (r == null) return;
            if (originalMaterials.ContainsKey(r)) return;

            // Limpieza de residuos
            var mats = new List<Material>(r.sharedMaterials);

            for (int i = mats.Count - 1; i >= 0; i--)
            {
                if (mats[i] != null && mats[i].shader.name == "Custom/RimHighlight")
                    mats.RemoveAt(i);
            }

            r.materials = mats.ToArray();

            // Guardar originales
            originalMaterials[r] = r.sharedMaterials;

            // Añadir highlight
            Material[] newMats = new Material[r.sharedMaterials.Length + 1];

            for (int i = 0; i < r.sharedMaterials.Length; i++)
                newMats[i] = r.sharedMaterials[i];

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

            if (originalMaterials.TryGetValue(r, out var originals))
            {
                r.materials = originals;
                originalMaterials.Remove(r);
            }
        }

        private void ClearAllHighlights()
        {
            var keys = new List<Renderer>(originalMaterials.Keys);

            foreach (var r in keys)
                RemoveHighlight(r);

            originalMaterials.Clear();
        }

        private ClimbableWall FindClimbableInParents(GameObject start)
        {
            Transform t = start.transform;

            while (t != null)
            {
                if (t.TryGetComponent<ClimbableWall>(out var climbable))
                    return climbable;

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