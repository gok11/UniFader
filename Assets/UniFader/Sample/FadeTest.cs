using System.Collections;
using UnityEngine;
using MB.UniFader;

public class FadeTest : MonoBehaviour
{
    void Update()
    {
        if (Input.GetButtonDown("Jump"))
        {
            UniFader.Instance.LoadSceneWithFade(0);
        }
    }
}

