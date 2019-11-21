using System.Collections;
using UnityEngine;
using MB.UniFader;

public class FadeTest : MonoBehaviour
{
    IEnumerator Start()
    {
        yield return new WaitForSeconds(1.5f);
        
        UniFader.Instance.FadeOut(1, () =>
        {
            UniFader.Instance.FadeIn(1);
        });
    }
}

