using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace MB.UniFader
{
    [CustomSerializeReferenceDrawer(typeof(GradientMaskFade))]
    public class GradientMaskFadeDrawer : PropertyDrawer
    {
        private static readonly int FadeMode = Shader.PropertyToID("_FadeMode");
        private static readonly int AlphaRate = Shader.PropertyToID("_AlphaRate");
        private static readonly int RegularDirection = Shader.PropertyToID("_RegularDirection");
        private static readonly int GradientMaskTex = Shader.PropertyToID("GradientMaskTex");
        private static readonly int CutoutEdgeFactor = Shader.PropertyToID("_CutoutEdgeFactor");

        private enum TransitionMode
        {
            Fade, Cutout
        }
        
        private float LineHeight => EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var ruleMaterial = property.FindPropertyRelative("ruleMaterial");
            var ruleTexture = property.FindPropertyRelative("ruleTexture");
            var fadeOutInMode = property.FindPropertyRelative("fadeOutInMode");
            var ruleMaterialObj = (ruleMaterial.objectReferenceValue as System.Object) as Material;

            var fieldRect = position;
            fieldRect.height = EditorGUIUtility.singleLineHeight;
            
            // Set material
            fieldRect.y += LineHeight;
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(fieldRect, ruleMaterial, new GUIContent("Transition Material"));
            if (EditorGUI.EndChangeCheck())
            {
                var fadeTarget = property.serializedObject.FindProperty("fadeTarget");
                var image = (fadeTarget.objectReferenceValue as System.Object) as Image;
                image.material = (ruleMaterial.objectReferenceValue as System.Object) as Material;
            }

            // Set texture
            fieldRect.y += LineHeight;
            //var prevTexture = ruleTexture.objectReferenceValue;
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(fieldRect, ruleTexture, new GUIContent("Mask Texture"));
            if (EditorGUI.EndChangeCheck() && ruleMaterialObj != null)
            {
                ruleMaterialObj.SetTexture(GradientMaskTex, ruleTexture.objectReferenceValue as Texture);
            }

            // -- draw transition mode --
            fieldRect.y += LineHeight;
            GUI.enabled = ruleMaterialObj != null;
            
            var transitionModeRect = fieldRect;
            EditorGUI.PrefixLabel(transitionModeRect, new GUIContent("Transition Mode"));
            
            transitionModeRect.x += EditorGUIUtility.labelWidth + 2;
            transitionModeRect.width -= EditorGUIUtility.labelWidth + 2;

            if (ruleMaterialObj != null)
            {
                // set transition mode
                var prevMode = (TransitionMode) ruleMaterialObj.GetInt(FadeMode);
                var newMode =
                    (TransitionMode) EditorGUI.EnumPopup(transitionModeRect, (TransitionMode) ruleMaterialObj.GetInt(FadeMode));
                if (prevMode != newMode)
                {
                    Undo.RecordObject(ruleMaterialObj, "Edit Transition Mode");
                    ruleMaterialObj.SetInt(FadeMode, (int) newMode);

                    // update material keyword
                    var values = Enum.GetValues(typeof(TransitionMode));
                    foreach (var value in values)
                    {
                        if ((int)value == (int) newMode)
                        {
                            ruleMaterialObj.EnableKeyword("_FADEMODE_" + newMode.ToString().ToUpper());
                        }
                        else
                        {
                            var disabledName = Enum.GetName(typeof(TransitionMode), value);
                            ruleMaterialObj.DisableKeyword("_FADEMODE_" + disabledName.ToUpper());
                        }
                    }
                }
                
                // Set cutout edge factor
                if ((TransitionMode) ruleMaterialObj.GetInt(FadeMode) == TransitionMode.Cutout)
                {
                    fieldRect.y += LineHeight;

                    var edgeFactor = ruleMaterialObj.GetFloat(CutoutEdgeFactor);
                    
                    EditorGUI.BeginChangeCheck();
                    var newFactor = EditorGUI.Slider(fieldRect, new GUIContent("Cutout Edge Smoothness"),
                            edgeFactor, 0, 0.1f);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(ruleMaterialObj, "Edit Edge Factor");
                        ruleMaterialObj.SetFloat(CutoutEdgeFactor, newFactor);
                    }
                }
            }
            else
            {
                // only showing button
                GUI.Button(transitionModeRect, "Fade", "MiniPopup");
            }

            // EnumPopup in SerializeReferenced interface doesn't work on 2019.3
            fieldRect.y += LineHeight;
            fadeOutInMode.intValue = (int) (GradientMaskFade.FadeOutInMode) EditorGUI.EnumPopup(
                fieldRect, "Fade Out In Mode",
                (GradientMaskFade.FadeOutInMode) fadeOutInMode.intValue);
            
            // debug function
            fieldRect.y += LineHeight;
            var boxRect = fieldRect;
            boxRect.height *= 2;
            GUI.Box(boxRect, "");

            var alphaRateRect = fieldRect;
            alphaRateRect.x += 2;
            GUI.Label(alphaRateRect, new GUIContent("Debug"), EditorStyles.boldLabel); // EditorGUI.PrefixLabel doesn't work correctly

            alphaRateRect.y += LineHeight;
            alphaRateRect.width -= 4;
            if (ruleMaterialObj != null)
            {
                var currentRate = ruleMaterialObj.GetFloat(AlphaRate);
                EditorGUI.BeginChangeCheck();
                var newRate = EditorGUI.Slider(alphaRateRect, new GUIContent("Alpha Rate"), currentRate, 0, 1f);
                if (currentRate != newRate)
                {
                    Undo.RecordObject(ruleMaterialObj, "Edit Fade Values");
                    var direction = CalcDirection((GradientMaskFade.FadeOutInMode) fadeOutInMode.intValue);
                    ruleMaterialObj.SetFloat(RegularDirection, direction);
                    ruleMaterialObj.SetFloat(AlphaRate, newRate);
                }
            }
            else
            {
                EditorGUI.Slider(alphaRateRect, new GUIContent("Alpha Rate"), 0, 0, 1f);
            }

            GUI.enabled = true;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var height = LineHeight * 4; // 4 props
            height += LineHeight * 2; // debug height
            
            var ruleMaterial = property.FindPropertyRelative("ruleMaterial");
            var ruleMaterialObj = (ruleMaterial.objectReferenceValue as System.Object) as Material;

            if (ruleMaterialObj &&
                (TransitionMode) ruleMaterialObj.GetInt(FadeMode) == TransitionMode.Cutout)
            {
                height += LineHeight;
            }
            
            return height;
        }
        
        private int CalcDirection(GradientMaskFade.FadeOutInMode fadeOutInMode)
        {
            var direction = 0;

            switch (fadeOutInMode)
            {
                case GradientMaskFade.FadeOutInMode.Yoyo:
                case GradientMaskFade.FadeOutInMode.RepeatTwice:
                    direction = 0;
                    break;
                
                case GradientMaskFade.FadeOutInMode.InvertYoyo:
                case GradientMaskFade.FadeOutInMode.InvertRepeatTwice:
                    direction = 1;
                    break;
            }

            return direction;
        }
    }   
}
