using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MB.UniFader
{
    public class FadeDatabase : ScriptableObject
    {
        public string databaseName;
        [SerializeField] private FadeInfo[] fadeInfos;

        public FadeInfo[] FadeInfos
        {
            get => fadeInfos;
            set => fadeInfos = value;
        }

        [System.Serializable]
        public class FadeInfo
        {
            [SerializeField] private string fadeName = "";
            [SerializeField] private float fadeDuration = 1f;
            [SerializeField] private AnimationCurve fadeOutCurve = AnimationCurve.Linear(0, 0, 1, 1);
            [SerializeField] private AnimationCurve fadeInCurve = AnimationCurve.Linear(0, 1, 1, 0);
            [SerializeField] private bool ignoreTimeScale = false;
            
            [SerializeField] private IFadePattern fadePattern = new ColorFade();

            public string FadeName
            {
                get => fadeName;
                set => fadeName = value;
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

            public bool IgnoreTimeScale
            {
                get => ignoreTimeScale;
                set => ignoreTimeScale = value;
            }

            public void SetFadePattern(IFadePattern fadePattern, Image fadeTarget)
            {
                this.fadePattern = fadePattern;
                if (fadeTarget)
                {
                    fadePattern.Initialize(fadeTarget);
                }
            }
        }
    }
}
