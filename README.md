# UniFader

UniFader is a transition helper components for Unity.

### Supported Unity versions

Unity 2019.3 or higher.

## Quick Start

1. Download `UniFader.unitypackage` from [releases](https://github.com/gok11/UniFader/releases) page and import it.
2. You can transition scene by calling only `UniFader.Instance.LoadSceneWithFade(0)`.
3. Enjoy!

![ColorFade](https://user-images.githubusercontent.com/8979566/69447773-938f7b00-0d9a-11ea-9217-a984b741e724.gif)

Samples can be seen in `Sample` folder if you want.

## Description

You can use two transition types - `Color Fade` and `Gradient Mask Fade`.

| Fade Pattern | Feature | Screenshot |
|-------------|----------|------------|
| **Color Fade** | Simple color fade effect. You can set base color from `Background Color`.  | ![ColorFade](https://user-images.githubusercontent.com/8979566/69447773-938f7b00-0d9a-11ea-9217-a984b741e724.gif) | 
| **Gradient Mask Fade** | Transition with gradient gray scaled texture. | ![GradientMaskFade](https://user-images.githubusercontent.com/8979566/69445581-f6324800-0d95-11ea-8629-bdcae1df2ea3.gif) |

## Usage for Mask Gradient Fade

1. Download `UniFader.unitypackage` from [releases](https://github.com/gok11/UniFader/releases) page and import it.
2. Create new GameObject and add UniFader component from `Add Component` in inspector.
3. Change Fade Pattern to `MaskGradientFade`<br>
![ChangeToMGF](https://user-images.githubusercontent.com/8979566/69473528-ccf6d380-0df8-11ea-993d-11c56fae9a72.png)

4. Set `Transition Material` from `Assets/UniFader/Materials/UI_GradientMaskTransition.mat`.
5. Set `Mask Texture` from  and `Assets/UniFader/Sample/Textures/SampleGradientMasks`.<br>
![SetElements](https://user-images.githubusercontent.com/8979566/69473529-cff1c400-0df8-11ea-938c-8f5bd93b939c.png)

6. Play!

## Transition options

### Easing

Edit easing from `Fade Out Curve` and `Fade In Curve`.

### Fade Out In Mode *(Gradient Mask Fade only)*

Yoyo<br>
Invert Yoyo<br>
Repeat twice<br>
Repeat invert twice

![FadePatterns](https://user-images.githubusercontent.com/8979566/69473383-1e9e5e80-0df7-11ea-9ec5-9339cd15cb26.gif)


## Tips

### Used As Instance

If this is enabled, the fader will be singleton instance. It is referenced from `UniFader.Instance` and it will be set `DontDestroyOnLoad`.

### Add fade pattern

You can add Fade Pattern by implementing `IFadePattern`.

```c#

using MB.UniFader;
using UnityEngine;
using UnityEngine.UI;

public class TestFade : IFadePattern
{
    [SerializeField] private int hoge;
    [SerializeField] private Color fuga;

    public void Initialize(Image targetImage) { }

    public void ExecFade(float progress, bool fadeOut) { }
}


```

After that, you can select that pattern in `Fade Pattern` in inspector.

![AddPattern](https://user-images.githubusercontent.com/8979566/69443882-9e461200-0d92-11ea-8662-98041dc3d4de.png)

### Add callback

You can add callback from inspector (On Fade Out / In) or script.
If you add callback for FadeOut, you can write as follows.

```c#

using System.Collections;
using UnityEngine;
using MB.UniFader;

public class FadeTest : MonoBehaviour
{
    void Start()
    {
        UniFader.Instance.FadeOut(() =>
        {
            Debug.Log("FadeOut completed!");
        });
    }
}


```

## License

- MIT
- © UTJ/UCL

## Author

Gok
