using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace MB.UniFader
{
    [CustomEditor(typeof(UniFader))]
    public class UniFaderEditor : Editor
    {
        private SerializedProperty m_Script;
        private SerializedProperty usedAsInstance;
        private SerializedProperty fadeTarget;
        private SerializedProperty defaultDuration;
        private SerializedProperty fadeOutCurve;
        private SerializedProperty fadeInCurve;
        private SerializedProperty sortingOrder;
        private SerializedProperty ignoreTimeScale;
        private SerializedProperty onFadeOut;
        private SerializedProperty onFadeIn;
        private SerializedProperty fadePattern;

        private FadeMode fadeMode = FadeMode.FadeOutIn;
        private Dictionary<string, Type> serializeDrawerDictionary;

        void OnEnable()
        {
            m_Script = serializedObject.FindProperty("m_Script");
            usedAsInstance = serializedObject.FindProperty("usedAsInstance");
            fadeTarget = serializedObject.FindProperty("fadeTarget");
            defaultDuration = serializedObject.FindProperty("defaultDuration");
            fadeOutCurve = serializedObject.FindProperty("fadeOutCurve");
            fadeInCurve = serializedObject.FindProperty("fadeInCurve");
            sortingOrder = serializedObject.FindProperty("sortingOrder");
            ignoreTimeScale = serializedObject.FindProperty("ignoreTimeScale");
            onFadeOut = serializedObject.FindProperty("onFadeOut");
            onFadeIn = serializedObject.FindProperty("onFadeIn");
            fadePattern = serializedObject.FindProperty("fadePattern");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var uniFader = serializedObject.targetObject as UniFader;

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(m_Script);   
            }

            EditorGUILayout.PropertyField(usedAsInstance);

            if (CheckIfMultiFaderHasDefaultSettings())
            {
                EditorGUILayout.HelpBox("There are some UniFader marked as \"Used By Default\". One of them is used by static functions at random.", MessageType.Warning);
            }

            EditorGUILayout.PropertyField(fadeTarget);
            EditorGUILayout.PropertyField(defaultDuration);

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.PropertyField(fadeOutCurve);
                ShowContextButton("Copy Mirror Curve To Fade In", fadeOutCurve);
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.PropertyField(fadeInCurve);
                ShowContextButton("Copy Mirror Curve To Fade Out", fadeInCurve);
            }

            EditorGUILayout.PropertyField(sortingOrder);
            EditorGUILayout.PropertyField(ignoreTimeScale);
            
            DrawUtility.DrawSeparator(3);
            
            // create fade pattern menu
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.PrefixLabel("Fade Pattern");
                if (GUILayout.Button(fadePattern.type, "MiniPopup"))
                {
                    var menu = new GenericMenu();
                    var names = TypeUtility.GetAllFadePatternNames<IFadePattern>();

                    for (var i = 0; i < names.Length; ++i)
                    {
                        var idx = i;
                        menu.AddItem(new GUIContent(names[i]), false,
                            () =>
                            {
                                if (fadePattern.type == names[idx]) return;

                                // Type.GetType doesn't work
                                Undo.RegisterCompleteObjectUndo(uniFader, "Change Fade Pattern");
                                var fadeType = TypeUtility.GetTypeByName(names[idx]);
                                var fadeInstance = (IFadePattern)Activator.CreateInstance(fadeType);
                                uniFader.FadePattern = fadeInstance;
                                
                                var image = (fadeTarget.objectReferenceValue as System.Object) as Image;
                                image.material = null;
                            });
                    }
                    menu.ShowAsContext();
                }    
            }

            DrawFaderItem(fadePattern);

            // Draw preview button
            using (new EditorGUILayout.VerticalScope("box"))
            {
                var titleLabel = Application.isPlaying ? "Debug" : "Debug (Runtime Only)";
                GUILayout.Label(titleLabel, EditorStyles.boldLabel);
                
                EditorGUI.BeginDisabledGroup(!Application.isPlaying);
                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button("Preview:", GUILayout.Width(EditorGUIUtility.labelWidth)))
                {
                    switch (fadeMode)
                    {
                        case FadeMode.FadeIn:    uniFader.FadeIn();    break;
                        case FadeMode.FadeOut:   uniFader.FadeOut();   break;
                        case FadeMode.FadeInOut: uniFader.FadeIn(() => uniFader.FadeOut()); break;
                        case FadeMode.FadeOutIn: uniFader.FadeOut(() => uniFader.FadeIn()); break;
                    }
                }
                
                fadeMode = (FadeMode)EditorGUILayout.EnumPopup(fadeMode);
                
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.PropertyField(onFadeOut);
            EditorGUILayout.PropertyField(onFadeIn);

            serializedObject.ApplyModifiedProperties();
        }

        bool CheckIfMultiFaderHasDefaultSettings()
        {
            var faderHasUsedByDefaultCount =
                FindObjectsOfType<UniFader>().Count(t => t.UsedAsInstance);
            return faderHasUsedByDefaultCount > 1;
        }

        void ShowContextButton(string itemContent, SerializedProperty baseCurve)
        {
            if (GUILayout.Button("…", GUILayout.Width(30)))
            {
                var uniFader = serializedObject.targetObject as UniFader;
                
                var menu = new GenericMenu();

                // add reversed copy button
                menu.AddItem(new GUIContent(itemContent), false, () =>
                {
                    // https://issuetracker.unity3d.com/issues/animationcurve-value-cannot-be-changed-in-custom-inspector-when-it-is-accessed-as-serialized-propertys-animationcurvevalue
                    var fader = serializedObject.targetObject as UniFader;

                    var baseKeys = baseCurve.animationCurveValue.keys;
                    var keyLength = baseKeys.Length;
                    var newKeys = new Keyframe[keyLength];

                    for (int i = 0; i < keyLength; ++i)
                    {
                        newKeys[i].time = baseKeys[i].time;
                        newKeys[i].value = baseKeys[keyLength - 1 - i].value;

                        newKeys[i].inWeight = baseKeys[keyLength - 1 - i].outWeight;
                        newKeys[i].outWeight = baseKeys[keyLength - 1 - i].inWeight;

                        newKeys[i].inTangent = baseKeys[keyLength - 1 - i].outTangent * -1;
                        newKeys[i].outTangent = baseKeys[keyLength - 1 - i].inTangent * -1;
                    }

                    var newCurve = new AnimationCurve(newKeys);

                    if (baseCurve.name == "fadeOutCurve")
                        uniFader.FadeInCurve = newCurve;
                    else
                        uniFader.FadeOutCurve = newCurve;

                    // force update animation curve
                    AssetDatabase.Refresh();
                });

                // add reset button
                menu.AddItem(new GUIContent("Reset"), false, () =>
                {
                    // https://issuetracker.unity3d.com/issues/animationcurve-value-cannot-be-changed-in-custom-inspector-when-it-is-accessed-as-serialized-propertys-animationcurvevalue
                    var fader = serializedObject.targetObject as UniFader;

                    if (baseCurve.name == "fadeOutCurve")
                        uniFader.FadeOutCurve = AnimationCurve.Linear(0, 0, 1, 1);
                    else
                        uniFader.FadeInCurve = AnimationCurve.Linear(0, 1, 1, 0);

                    // force update animation curve
                    AssetDatabase.Refresh();
                });

                menu.ShowAsContext();
            }
        }

        void DrawFaderItem(SerializedProperty prop)
        {
            // CustomPropertyDrawer in SerializeReferenced interface seems not worked on 2019.3...(?)
            if (serializeDrawerDictionary == null)
            {
                serializeDrawerDictionary = TypeUtility.GetTypeDictWithCustomDrawerAttribute();
            }

            var drawerType = serializeDrawerDictionary.ContainsKey(fadePattern.type)
                ? serializeDrawerDictionary[fadePattern.type] : null;

            if (drawerType != null)
            {
                var drawer = (PropertyDrawer)Activator.CreateInstance(drawerType);
                drawer.OnGUI(GUILayoutUtility.GetLastRect(), prop, null);
                EditorGUILayout.Space(drawer.GetPropertyHeight(prop, null));
            }
            else
            {
                var fadeContent = prop.Copy();
                while (fadeContent.NextVisible(true))
                {
                    EditorGUILayout.PropertyField(fadeContent, true);
                }
            }
        }
    }
}
