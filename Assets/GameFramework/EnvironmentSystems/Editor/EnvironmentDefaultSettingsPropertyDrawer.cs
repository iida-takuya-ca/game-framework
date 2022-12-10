using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.Rendering;
using RenderSettings = UnityEngine.RenderSettings;

namespace GameFramework.EnvironmentSystems.Editor
{
    /// <summary>
    /// 環境設定のPropertyDrawer
    /// </summary>
    [CustomPropertyDrawer(typeof(EnvironmentDefaultSettings))]
    public class EnvironmentDefaultSettingsPropertyDrawer : PropertyDrawer {
        /// <summary>
        /// Style情報
        /// </summary>
        private static class Styles {
            public static readonly GUIContent SkyTitle = EditorGUIUtility.TrTextContent("Sky");
            public static readonly GUIContent AmbientTitle = EditorGUIUtility.TrTextContent("Environment Lighting");
            public static readonly GUIContent Sun = EditorGUIUtility.TrTextContent("Sun Source", "Specifies the directional light that is used to indicate the direction of the sun when a procedural skybox is used. If set to None, the brightest directional light in the Scene is used to represent the sun.");
            public static readonly GUIContent SubtractiveShadowColor = EditorGUIUtility.TrTextContent("Realtime Shadow Color", "The color used for mixing realtime shadows with baked lightmaps in Subtractive lighting mode. The color defines the darkest point of the realtime shadow.");
            public static readonly GUIContent AmbientSkyColor = EditorGUIUtility.TrTextContent("Sky Color", "Controls the color of light emitted from the sky in the Scene.");
            public static readonly GUIContent AmbientEquatorColor = EditorGUIUtility.TrTextContent("Equator Color", "Controls the color of light emitted from the sides of the Scene.");
            public static readonly GUIContent AmbientGroundColor = EditorGUIUtility.TrTextContent("Ground Color", "Controls the color of light emitted from the ground of the Scene.");
            public static readonly GUIContent AmbientColor = EditorGUIUtility.TrTextContent("Ambient Color", "Controls the color of the ambient light contributed to the Scene.");
            public static readonly GUIContent AmbientSource = EditorGUIUtility.TrTextContent("Source", "Specifies whether to use a skybox, gradient, or color for ambient light contributed to the Scene.");
            public static readonly GUIContent AmbientIntensity = EditorGUIUtility.TrTextContent("Intensity Multiplier", "Controls the brightness of the skybox lighting in the Scene.");
            public static readonly GUIContent SkyboxMaterial = EditorGUIUtility.TrTextContent("Skybox Material", "Specifies the material that is used to simulate the sky or other distant background in the Scene.");
            public static readonly GUIContent SkyboxWarning = EditorGUIUtility.TrTextContent("Shader of this material does not support skybox rendering.");
            public static readonly GUIContent ReflectionTitle = EditorGUIUtility.TrTextContent("Environment Reflections");
            public static readonly GUIContent ReflectionMode = EditorGUIUtility.TrTextContent("Source", "Specifies whether to use the skybox or a custom cube map for reflection effects in the Scene.");
            public static readonly GUIContent ReflectionResolution = EditorGUIUtility.TrTextContent("Resolution", "Controls the resolution for the cube map assigned to the skybox material for reflection effects in the Scene.");
            public static readonly GUIContent CustomReflection = EditorGUIUtility.TrTextContent("Cubemap", "Specifies the custom cube map used for reflection effects in the Scene.");
            public static readonly GUIContent ReflectionIntensity = EditorGUIUtility.TrTextContent("Intensity Multiplier", "Controls how much the skybox or custom cubemap affects reflections in the Scene. A value of 1 produces physically correct results.");
            public static readonly GUIContent ReflectionBounces = EditorGUIUtility.TrTextContent("Bounces", "Controls how many times a reflection includes other reflections. A value of 1 results in the Scene being rendered once so mirrored reflections will be black. A value of 2 results in mirrored reflections being visible in the Scene.");

            public static readonly int[] AmbientSourceValues = {
                (int)AmbientMode.Skybox,
                (int)AmbientMode.Trilight,
                (int)AmbientMode.Flat
            };
            
            public static readonly GUIContent[] AmbientSourceLabels =
            {
                EditorGUIUtility.TrTextContent("Skybox"),
                EditorGUIUtility.TrTextContent("Gradient"),
                EditorGUIUtility.TrTextContent("Color"),
            };

            public static readonly int[] ReflectionResolutionValues = {
                16,
                32,
                64,
                128,
                256,
                512,
                1024,
                2048,
            };

            public static readonly GUIContent[] ReflectionResolutionLabels =
                ReflectionResolutionValues.Select(x => new GUIContent(x.ToString())).ToArray();
        }
        
        private SerializedProperty _sun;
        private SerializedProperty _subtractiveShadowColor;
        private SerializedProperty _ambientMode;
        private SerializedProperty _ambientSkyColor;
        private SerializedProperty _ambientEquatorColor;
        private SerializedProperty _ambientGroundColor;
        private SerializedProperty _ambientIntensity;
        private SerializedProperty _skyboxMaterial;
        private SerializedProperty _defaultReflectionMode;
        private SerializedProperty _defaultReflectionResolution;
        private SerializedProperty _customReflection;
        private SerializedProperty _reflectionIntensity;
        private SerializedProperty _reflectionBounces;
        
        /// <summary>
        /// GUI描画
        /// </summary>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            GetProperties(property);
            
            var skyboxMaterial = _skyboxMaterial.objectReferenceValue as Material;
            var lineHeight = EditorGUIUtility.singleLineHeight;
            var lineOffsetUnit = lineHeight + EditorGUIUtility.standardVerticalSpacing;
            var rect = position;
            rect.height = lineHeight;

            // Title
            EditorGUI.LabelField(rect, Styles.SkyTitle, EditorStyles.boldLabel);
            rect.y += lineOffsetUnit;
            EditorGUI.indentLevel++;
            
            // Skybox
            EditorGUI.PropertyField(rect, _skyboxMaterial, Styles.SkyboxMaterial);
            rect.y += lineOffsetUnit;
            if (skyboxMaterial != null && !EditorMaterialUtility.IsBackgroundMaterial(skyboxMaterial)) {
                rect.height = lineHeight * 2;
                EditorGUI.HelpBox(rect, Styles.SkyboxWarning.text, MessageType.Warning);
                rect.y += lineOffsetUnit + lineHeight;
                rect.height = lineHeight;
            }
            
            // Sun
            EditorGUI.PropertyField(rect, _sun, Styles.Sun);
            rect.y += lineOffsetUnit;
            EditorGUI.PropertyField(rect, _subtractiveShadowColor, Styles.SubtractiveShadowColor);
            rect.y += lineOffsetUnit;
            EditorGUI.indentLevel--;

            // Ambient
            EditorGUI.LabelField(rect, Styles.AmbientTitle, EditorStyles.boldLabel);
            rect.y += lineOffsetUnit;
            EditorGUI.indentLevel++;

            EditorGUI.IntPopup(rect, _ambientMode, Styles.AmbientSourceLabels, Styles.AmbientSourceValues, Styles.AmbientSource);
            rect.y += lineOffsetUnit;
            switch ((AmbientMode)_ambientMode.intValue)
            {
                case AmbientMode.Trilight:
                {
                    EditorGUI.BeginChangeCheck();
                    var newSkyColor = EditorGUI.ColorField(rect, Styles.AmbientSkyColor, _ambientSkyColor.colorValue, true, false, true);
                    rect.y += lineOffsetUnit;
                    var newEquatorColor = EditorGUI.ColorField(rect, Styles.AmbientEquatorColor, _ambientEquatorColor.colorValue, true, false, true);
                    rect.y += lineOffsetUnit;
                    var newGroundColor = EditorGUI.ColorField(rect, Styles.AmbientGroundColor, _ambientGroundColor.colorValue, true, false, true);
                    rect.y += lineOffsetUnit;
                    if (EditorGUI.EndChangeCheck())
                    {
                        _ambientSkyColor.colorValue = newSkyColor;
                        _ambientEquatorColor.colorValue = newEquatorColor;
                        _ambientGroundColor.colorValue = newGroundColor;
                    }
                }
                break;

                case AmbientMode.Flat:
                {
                    EditorGUI.BeginChangeCheck();
                    var newColor = EditorGUI.ColorField(rect, Styles.AmbientColor, _ambientSkyColor.colorValue, true, false, true);
                    rect.y += lineOffsetUnit;
                    if (EditorGUI.EndChangeCheck()) {
                        _ambientSkyColor.colorValue = newColor;
                    }
                }
                break;

                case AmbientMode.Skybox:
                    if (skyboxMaterial == null)
                    {
                        EditorGUI.BeginChangeCheck();
                        var newColor = EditorGUI.ColorField(rect, Styles.AmbientColor, _ambientSkyColor.colorValue, true, false, true);
                        rect.y += lineOffsetUnit;
                        if (EditorGUI.EndChangeCheck()) {
                            _ambientSkyColor.colorValue = newColor;
                        }
                    }
                    else
                    {
                        EditorGUI.Slider(rect, _ambientIntensity, 0.0f, 8.0f, Styles.AmbientIntensity);
                        rect.y += lineOffsetUnit;
                    }
                    break;
            }

            EditorGUI.indentLevel--;
            
            // Reflection
            EditorGUI.LabelField(rect, Styles.ReflectionTitle, EditorStyles.boldLabel);
            rect.y += lineOffsetUnit;
            EditorGUI.indentLevel++;

            EditorGUI.PropertyField(rect, _defaultReflectionMode, Styles.ReflectionMode);
            rect.y += lineOffsetUnit;

            var defReflectionMode = (DefaultReflectionMode)_defaultReflectionMode.intValue;
            switch (defReflectionMode)
            {
                case DefaultReflectionMode.Skybox:
                    EditorGUI.IntPopup(rect, _defaultReflectionResolution, Styles.ReflectionResolutionLabels, Styles.ReflectionResolutionValues, Styles.ReflectionResolution);
                    rect.y += lineOffsetUnit;
                    break;
                
                case DefaultReflectionMode.Custom:
                    EditorGUI.PropertyField(rect, _customReflection, Styles.CustomReflection);
                    rect.y += lineOffsetUnit;
                    break;
            }

            EditorGUI.Slider(rect, _reflectionIntensity, 0.0f, 1.0f, Styles.ReflectionIntensity);
            rect.y += lineOffsetUnit;
            EditorGUI.IntSlider(rect, _reflectionBounces, 1, 5, Styles.ReflectionBounces);
            rect.y += lineOffsetUnit;

            EditorGUI.indentLevel--;
            
            // 値の取得/反映ボタン
            rect.width = position.width * 0.5f;
            if (GUI.Button(rect, "Get Settings")) {
                _sun.objectReferenceValue = RenderSettings.sun;
                _subtractiveShadowColor.colorValue = RenderSettings.subtractiveShadowColor;
                _ambientMode.intValue = (int)RenderSettings.ambientMode;
                _ambientSkyColor.colorValue = RenderSettings.ambientSkyColor;
                _ambientEquatorColor.colorValue = RenderSettings.ambientEquatorColor;
                _ambientGroundColor.colorValue = RenderSettings.ambientGroundColor;
                _ambientIntensity.floatValue = RenderSettings.ambientIntensity;
                _skyboxMaterial.objectReferenceValue = RenderSettings.skybox;
                _defaultReflectionMode.intValue = (int)RenderSettings.defaultReflectionMode;
                _defaultReflectionResolution.intValue = RenderSettings.defaultReflectionResolution;
                _customReflection.objectReferenceValue = RenderSettings.customReflection;
                _reflectionIntensity.floatValue = RenderSettings.reflectionIntensity;
                _reflectionBounces.intValue = RenderSettings.reflectionBounces;
            }

            rect.x += rect.width;
            if (GUI.Button(rect, "Set Settings")) {
                RenderSettings.sun = _sun.objectReferenceValue as Light;
                RenderSettings.subtractiveShadowColor = _subtractiveShadowColor.colorValue;
                RenderSettings.ambientMode = (AmbientMode)_ambientMode.intValue;
                RenderSettings.ambientSkyColor = _ambientSkyColor.colorValue;
                RenderSettings.ambientEquatorColor = _ambientEquatorColor.colorValue;
                RenderSettings.ambientGroundColor = _ambientGroundColor.colorValue;
                RenderSettings.ambientIntensity = _ambientIntensity.floatValue;
                RenderSettings.skybox = _skyboxMaterial.objectReferenceValue as Material;
                RenderSettings.defaultReflectionMode = (DefaultReflectionMode)_defaultReflectionMode.intValue;
                RenderSettings.defaultReflectionResolution = _defaultReflectionResolution.intValue;
                RenderSettings.customReflection = _customReflection.objectReferenceValue as Texture;
                RenderSettings.reflectionIntensity = _reflectionIntensity.floatValue;
                RenderSettings.reflectionBounces = _reflectionBounces.intValue;
            }

            rect.x = position.x;
            rect.width = position.width;
            rect.y += lineOffsetUnit;
        }

        /// <summary>
        /// プロパティの高さ取得
        /// </summary>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            GetProperties(property);
            
            var skyboxMaterial = _skyboxMaterial.objectReferenceValue as Material;
            var lineHeight = EditorGUIUtility.singleLineHeight;
            var lineOffsetUnit = lineHeight + EditorGUIUtility.standardVerticalSpacing;
            var totalHeight = 0.0f;

            // Sky
            totalHeight += lineOffsetUnit;
            
            // Skybox
            totalHeight += lineOffsetUnit;
            if (skyboxMaterial != null && !EditorMaterialUtility.IsBackgroundMaterial(skyboxMaterial)) {
                totalHeight += lineOffsetUnit + lineHeight;
            }
            
            // Sun
            totalHeight += lineOffsetUnit * 2;
            
            // Ambient
            totalHeight += lineOffsetUnit * 2;
            switch ((AmbientMode)_ambientMode.intValue) {
                case AmbientMode.Trilight:
                    totalHeight += lineOffsetUnit * 3;
                    break;

                case AmbientMode.Flat:
                    totalHeight += lineOffsetUnit;
                    break;

                case AmbientMode.Skybox:
                    totalHeight += lineOffsetUnit;
                    break;
            }
            
            // Reflection
            totalHeight += lineOffsetUnit * 5;
            
            // Button
            totalHeight += lineOffsetUnit;

            return totalHeight;
        }

        /// <summary>
        /// プロパティの取得
        /// </summary>
        private void GetProperties(SerializedProperty property) {
            _sun = property.FindPropertyRelative("sun");
            _subtractiveShadowColor = property.FindPropertyRelative("subtractiveShadowColor");
            _ambientMode = property.FindPropertyRelative("ambientMode");
            _ambientSkyColor = property.FindPropertyRelative("ambientSkyColor");
            _ambientEquatorColor = property.FindPropertyRelative("ambientEquatorColor");
            _ambientGroundColor = property.FindPropertyRelative("ambientGroundColor");
            _ambientIntensity = property.FindPropertyRelative("ambientIntensity");
            _skyboxMaterial = property.FindPropertyRelative("skyboxMaterial");
            _defaultReflectionMode = property.FindPropertyRelative("defaultReflectionMode");
            _defaultReflectionResolution = property.FindPropertyRelative("defaultReflectionResolution");
            _customReflection = property.FindPropertyRelative("customReflection");
            _reflectionIntensity = property.FindPropertyRelative("reflectionIntensity");
            _reflectionBounces = property.FindPropertyRelative("reflectionBounces");
        }
    }
}