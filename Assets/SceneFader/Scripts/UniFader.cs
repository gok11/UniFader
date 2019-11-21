using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace MB.UniFader
{
    public enum FadeMode
    {
        FadeIn,
        FadeOut,
        FadeInOut,
        FadeOutIn
    }

    [ExecuteAlways]
    public class UniFader : MonoBehaviour
    {
        private Canvas canvas;
        
        [Tooltip("If this is enabled, this fader is used by static functions")]
        [SerializeField] private bool usedByDefault = true;
        [SerializeField] private bool dontDestroyOnLoad = false;
        
        [SerializeField] private Image fadeTarget;
        [SerializeField] private float defaultDuration = 1f;
        [SerializeField] private AnimationCurve fadeOutCurve = AnimationCurve.Linear(0, 0, 1, 1);
        [SerializeField] private AnimationCurve fadeInCurve = AnimationCurve.Linear(0, 1, 1, 0);
        [SerializeField] private int sortingOrder = 10000;
        [SerializeField] private bool ignoreTimeScale = false;

        [SerializeField] private UnityEvent onFadeOut = new UnityEvent();
        [SerializeField] private UnityEvent onFadeIn = new UnityEvent();

        [SerializeReference] private IFadePattern fadePattern = new ColorFade();

        public bool UsedByDefault => usedByDefault;

        public Image FadeTarget
        {
            get => fadeTarget;
            set => fadeTarget = value;
        }

        public Sprite FadeContentSprite
        {
            set => fadeTarget.sprite = value;
        }

        public AnimationCurve FadeOutCurve
        {
            get => fadeOutCurve;
            set => fadeOutCurve = value;
        }

        public AnimationCurve FadeInCurve
        {
            get => fadeInCurve;
            set => fadeInCurve = value;
        }

        public int SortingOrder
        {
            get => sortingOrder;
            set => sortingOrder = value;
        }

        public bool IgnoreTimeScale
        {
            get => ignoreTimeScale;
            set => ignoreTimeScale = value;
        }

        public UnityEvent OnFadeOut
        {
            get => onFadeOut;
            set => onFadeOut = value;
        }

        public UnityEvent OnFadeIn
        {
            get => onFadeIn;
            set => onFadeIn = value;
        }

        public IFadePattern FadePattern
        {
            set
            {
                fadePattern = value;
                if (fadeTarget)
                    fadePattern.Initialize(fadeTarget);
            }
        }

        public static UniFader Instance
        {
            get
            {
                if (_instance) return _instance;

                _instance = FindObjectsOfType<UniFader>()
                    .FirstOrDefault(t => t.fadeTarget && t.usedByDefault); 
                if (_instance) return _instance;
                
                _instance = new GameObject("SceneFader").AddComponent<UniFader>();
                return _instance;
            }
        }
        private static UniFader _instance;
        private bool _isFading;

        void Awake()
        {
            InitializeFadeTarget();
        }

        void OnValidate()
        {
            if (canvas) canvas.sortingOrder = sortingOrder;
            if (fadePattern != null && fadeTarget)
                fadePattern.Initialize(fadeTarget);
        }

        /// <summary>
        /// Initialization.
        /// Create fade target panel and set color.
        /// </summary>
        void InitializeFadeTarget()
        {

            if (!fadeTarget)
            {
                canvas = new GameObject("FadeCanvas").AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = sortingOrder;
            
                var canvasScaler = canvas.gameObject.AddComponent<CanvasScaler>();
                canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                canvasScaler.referenceResolution = 
                    new Vector2(Screen.currentResolution.width, Screen.currentResolution.height);;
                canvas.gameObject.AddComponent<GraphicRaycaster>();
            
                var panel = new GameObject("FadePanel").AddComponent<Image>();
                fadeTarget = panel;
            
                var panelRt = panel.GetComponent<RectTransform>();
                panelRt.SetParent(canvas.transform);

                var vector2Zero = Vector2.zero;
                panelRt.anchorMin = vector2Zero;
                panelRt.anchorMax = Vector2.one;
                panelRt.sizeDelta = vector2Zero;
                panelRt.anchoredPosition = vector2Zero;
            }
            else
            {
                canvas = fadeTarget.GetComponentInParent<Canvas>();
                canvas.sortingOrder = sortingOrder;
            }

            // initialize fade pattern
            fadePattern.Initialize(fadeTarget);

            if (Application.isPlaying && dontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
                DontDestroyOnLoad(fadeTarget.GetComponentInParent<Canvas>().gameObject);
            }
        }
        
        // ----- Exec Loading Scene -----
        
        public void LoadSceneWithFade(string sceneName, FadeMode fadeMode = FadeMode.FadeOutIn)
        {
            var index = SceneManager.GetSceneByName(sceneName).buildIndex;
            StartCoroutine(LoadSceneWithFadeCor(index, fadeMode, defaultDuration));
        }
        
        public void LoadSceneWithFade(string sceneName, float fadeTime, FadeMode fadeMode = FadeMode.FadeOutIn)
        {
            var index = SceneManager.GetSceneByName(sceneName).buildIndex;
            StartCoroutine(LoadSceneWithFadeCor(index, fadeMode, fadeTime));
        }
        
        public void LoadSceneWithFade(int sceneBuildIndex, FadeMode fadeMode = FadeMode.FadeOutIn)
        {
            StartCoroutine(LoadSceneWithFadeCor(sceneBuildIndex, fadeMode, defaultDuration));
        }
        
        public void LoadSceneWithFade(int sceneBuildIndex, float fadeTime, FadeMode fadeMode = FadeMode.FadeOutIn)
        {
            StartCoroutine(LoadSceneWithFadeCor(sceneBuildIndex, fadeMode, fadeTime));
        }

        public IEnumerator LoadSceneWithFadeCor(int sceneBuildIndex, FadeMode fadeMode, float fadeTime)
        {
            if (_isFading) yield break;
            _isFading = true;
            
            // Fade Out
            if (fadeMode == FadeMode.FadeOut || fadeMode == FadeMode.FadeOutIn)
                yield return ExecFade(true, fadeTime, null);
            
            SceneManager.LoadScene(sceneBuildIndex);
            
            // Fade In
            if (fadeMode == FadeMode.FadeIn || fadeMode == FadeMode.FadeOutIn)
                yield return ExecFade(false, fadeTime, null);

            _isFading = false;
        }
        
        // ----- Exec Fading -----

        public void FadeOut(Action onFadeCompleted = null)
        {
            StartCoroutine(ExecFade(true, defaultDuration, onFadeCompleted));
        }
        
        public void FadeOut(float fadeTime, Action onFadeCompleted = null)
        {
            StartCoroutine(ExecFade(true, fadeTime, onFadeCompleted));
        }

        public void FadeIn(Action onFadeCompleted = null)
        {
            StartCoroutine(ExecFade(false, defaultDuration, onFadeCompleted));
        }
        
        public void FadeIn(float fadeTime, Action onFadeCompleted = null)
        {
            StartCoroutine(ExecFade(false, fadeTime, onFadeCompleted));
        }
        
        IEnumerator ExecFade(bool fadeOut, float fadeTime, Action onCompleted)
        {
            if (_isFading) yield break;
            _isFading = true;
            
            var end = fadeOut ? fadeOutCurve.Evaluate(1) : fadeInCurve.Evaluate(1);

            if (canvas && !canvas.enabled)
                canvas.enabled = true;

            fadeTime = Mathf.Max(fadeTime, 0f);
            if (fadeTime <= float.Epsilon)
            {
                
                fadePattern.ExecFade(end, fadeOut);
                yield break;
            }

            for (var t = 0f; t < fadeTime; t += ignoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime)
            {
                var alpha = fadeOut ?
                    fadeOutCurve.Evaluate(t / fadeTime) :
                    fadeInCurve.Evaluate(t / fadeTime);
                
                fadePattern.ExecFade(alpha, fadeOut);
                yield return null;
            }
            
            fadePattern.ExecFade(end, fadeOut);

            if (canvas && !fadeOut)
                canvas.enabled = false;
            
            _isFading = false;
            
            if (fadeOut) onFadeOut?.Invoke();
            if (!fadeOut) onFadeIn?.Invoke();

            onCompleted?.Invoke();
        }
    }
}