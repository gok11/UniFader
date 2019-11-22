using UnityEngine;
using UnityEngine.UI;

namespace MB.UniFader
{
    public class ColorFade : IFadePattern
    {
        [SerializeField] private Color backgroundColor = Color.white;
        private Image target;
    
        public ColorFade () {}

        public ColorFade(Color backgroundColor)
        {
            this.backgroundColor = backgroundColor;
        }

        public void Initialize(Image targetImage)
        {
            target = targetImage;
            target.color = backgroundColor;
        }

        public void ExecFade(float progress, bool fadeOut)
        {
            target.SetImageAlpha(progress);
        }
    }

    static class ImageExtensions
    {
        public static void SetImageAlpha(this Image target, float alpha)
        {
            var c = target.color;
            c.a = alpha;
            target.color = c;
        }
    }
}