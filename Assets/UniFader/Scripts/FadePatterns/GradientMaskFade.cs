using System;
using UnityEngine;
using UnityEngine.UI;

namespace MB.UniFader
{
    public class GradientMaskFade : IFadePattern
    {
        [SerializeField] private Material ruleMaterial;
        [SerializeField] private Texture2D ruleTexture = null;
        [SerializeField] private FadeOutInMode fadeOutInMode = FadeOutInMode.Yoyo;

        public enum FadeOutInMode
        {
            Yoyo,
            InvertYoyo,
            RepeatTwice,
            InvertRepeatTwice
        }
        
        private static readonly int AlphaRate = Shader.PropertyToID("_AlphaRate");
        private static readonly int RegularDirection = Shader.PropertyToID("_RegularDirection");
        private static readonly int GradientMaskTex = Shader.PropertyToID("_GradientMaskTex");

        public GradientMaskFade() {}
        
        public GradientMaskFade(Material ruleMaterial)
        {
            this.ruleMaterial = ruleMaterial;
        }

        public void Initialize(Image targetImage)
        {
            if (!ruleMaterial || !ruleTexture) return;
            
            targetImage.material = ruleMaterial;
            ruleMaterial.SetTexture(GradientMaskTex, ruleTexture);
        }

        public void ExecFade(float progress, bool fadeOut)
        {
            if (!ruleMaterial || !ruleTexture)
            {
                throw new NullReferenceException("should set Gradient material and texture in Scene Fader");
            }
            
            var direction = CalcDirection(fadeOut);
            ruleMaterial.SetFloat(RegularDirection, direction);
            ruleMaterial.SetFloat(AlphaRate, progress);
        }

        private int CalcDirection(bool fadeOut)
        {
            var direction = 0;

            switch (fadeOutInMode)
            {
                case FadeOutInMode.Yoyo:
                    direction = 0;
                    break;
                
                case FadeOutInMode.InvertYoyo:
                    direction = 1;
                    break;

                case FadeOutInMode.RepeatTwice:
                    direction = fadeOut ? 0 : 1;
                    break;
                
                case FadeOutInMode.InvertRepeatTwice:
                    direction = fadeOut ? 1 : 0;
                    break;
            }

            return direction;
        }
    }
}