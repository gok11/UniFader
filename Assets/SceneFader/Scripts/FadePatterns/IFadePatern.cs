using UnityEngine.UI;

namespace MB.UniFader
{
    public interface IFadePattern
    {
        void Initialize(Image targetImage);
        void ExecFade(float progress, bool fadeOut);
    }
}