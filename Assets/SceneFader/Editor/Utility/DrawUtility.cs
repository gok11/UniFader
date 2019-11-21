using UnityEngine;

namespace MB.UniFader
{
    public class DrawUtility
    {
        public static void DrawSeparator(float height)
        {
            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(height));
        }
    }
}
