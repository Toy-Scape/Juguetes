using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CinematicSystem.Transitions
{
    public class SceneTransitionManager : MonoBehaviour
    {
        public static SceneTransitionManager Instance { get; private set; }

        // No longer needed as we use SceneIntroTrigger component
        // [SerializeField] private string _targetTag = "Player"; 
        // [SerializeField] private string _triggerName = "init";

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void CrossfadeToScene(string sceneName, float duration)
        {
            StartCoroutine(CrossfadeRoutine(sceneName, duration));
        }

        private IEnumerator CrossfadeRoutine(string sceneName, float duration)
        {
            // Store current scene reference (we might be in Menu or Table scene)
            Scene currentScene = SceneManager.GetActiveScene();

            Debug.Log($"[SceneTransitionManager] Loading scene '{sceneName}' additively.");
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            yield return asyncLoad;

            Scene newScene = SceneManager.GetSceneByName(sceneName);

            // Find the new scene's camera (usually MainCamera tag or first enable cam)
            // We search roots for safety
            Camera newSceneCamera = null;
            foreach (var root in newScene.GetRootGameObjects())
            {
                newSceneCamera = root.GetComponentInChildren<Camera>(true);
                if (newSceneCamera != null) break;
            }

            if (newSceneCamera != null)
            {
                // Ensure camera is active but redirected to RT
                bool wasEnabled = newSceneCamera.enabled;
                newSceneCamera.enabled = false;
                Debug.Log($"[SceneTransitionManager] Found camera '{newSceneCamera.name}' in new scene. Setting up RenderTexture.");

                // Create a RenderTexture matchng screen res
                RenderTexture rt = new RenderTexture(Screen.width, Screen.height, 24);
                newSceneCamera.targetTexture = rt;
                newSceneCamera.enabled = true;

                // Create overlay showing the RenderTexture
                Debug.Log("[SceneTransitionManager] Creating crossfade overlay.");
                GameObject overlayObj = new GameObject("CrossfadeOverlay");
                DontDestroyOnLoad(overlayObj);

                Canvas canvas = overlayObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 32767;

                CanvasScaler scaler = overlayObj.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);

                overlayObj.AddComponent<GraphicRaycaster>();

                GameObject imageObj = new GameObject("NewSceneImage");
                imageObj.transform.SetParent(overlayObj.transform, false);

                RawImage rawImage = imageObj.AddComponent<RawImage>();
                rawImage.texture = rt;

                RectTransform imageRt = imageObj.GetComponent<RectTransform>();
                imageRt.anchorMin = Vector2.zero;
                imageRt.anchorMax = Vector2.one;
                imageRt.sizeDelta = Vector2.zero;

                CanvasGroup overlayCG = imageObj.AddComponent<CanvasGroup>();
                overlayCG.alpha = 0f; // Starts invisible (old scene is fully visible)
                overlayCG.blocksRaycasts = true; // Block raycasts to old scene

                // Crossfade: Fade IN the new scene overlay (old scene fades away underneath)
                Debug.Log("[SceneTransitionManager] Crossfading to new scene.");
                Tween crossfade = overlayCG.DOFade(1f, duration).SetEase(Ease.InOutQuad).SetUpdate(true);
                yield return crossfade.WaitForCompletion();

                // Swap: New camera renders to screen, remove overlay
                Debug.Log("[SceneTransitionManager] Crossfade complete. Swapping cameras.");
                newSceneCamera.targetTexture = null; // Render to screen now

                SceneManager.SetActiveScene(newScene);

                // SceneIntroTrigger will handle itself in its Start() method.
                // No need to look for it here.

                // Cleanup
                rt.Release();
                Destroy(rt);
                Destroy(overlayObj);
            }
            else
            {
                // Fallback if no camera found in new scene
                Debug.LogWarning("[SceneTransitionManager] No camera found in new scene, switching directly.");
                SceneManager.SetActiveScene(newScene);
            }

            // Unload old scene
            Debug.Log($"[SceneTransitionManager] Unloading old scene '{currentScene.name}'.");
            SceneManager.UnloadSceneAsync(currentScene);
        }
    }
}
