using System.Collections.Generic;
using GameFramework.BodySystems;
using GameFramework.Core;
using UniRx;
using UnityEditor;
using UnityEngine;

namespace SampleGame.ModelViewer.Editor {
    /// <summary>
    /// ModelViewerのComponentパネル
    /// </summary>
    partial class ModelViewerWindow {
        /// <summary>
        /// ComponentPanel
        /// </summary>
        private class BodyPanel : PanelBase {
            public override string Title => "Body";
            
            private const float FoldoutHeight = 300;
            
            private Body _body;
            private FoldoutList<string> _locatorFoldoutList;
            private FoldoutList<string> _materialFoldoutList;
            private FoldoutList<string> _gimmickFoldoutList;

            private Dictionary<string, Color> _gimmickColorParams = new();
            private Dictionary<string, float> _gimmickFloatParams = new();
            private Dictionary<string, Vector2> _gimmickVector2Params = new();
            private Dictionary<string, Vector3> _gimmickVector3Params = new();
            private Dictionary<string, Vector4> _gimmickVector4Params = new();

            /// <summary>
            /// 初期化処理
            /// </summary>
            protected override void InitializeInternal(IScope scope) {
                _locatorFoldoutList = new FoldoutList<string>("Locator Controller");
                _materialFoldoutList = new FoldoutList<string>("Material Controller");
                _gimmickFoldoutList = new FoldoutList<string>("Gimmick Controller");
                
                var entityManager = Services.Get<ActorManager>();
                entityManager.PreviewActor
                    .TakeUntil(scope)
                    .Subscribe(x => _body = x?.Body);
            }

            /// <summary>
            /// GUI描画
            /// </summary>
            protected override void OnGUIInternal() {
                if (_body == null) {
                    return;
                }

                // Locator
                var locatorController = _body.GetController<LocatorController>();
                if (locatorController != null) {
                    _locatorFoldoutList.OnGUI(locatorController.GetKeys(), (key, index) => {
                        var locator = locatorController[key];
                        EditorGUILayout.ObjectField(key, locator, typeof(Transform), true);
                        EditorGUI.indentLevel++;
                        EditorGUILayout.LabelField("Position", locator.position.ToString());
                        EditorGUILayout.LabelField("Angles", locator.transform.eulerAngles.ToString());
                        EditorGUI.indentLevel--;
                    }, GUILayout.Height(FoldoutHeight));
                }

                // Material
                var materialController = _body.GetController<MaterialController>();
                if (materialController != null) {
                    _materialFoldoutList.OnGUI(materialController.GetKeys(), (key, index) => {
                        var handle = materialController.GetHandle(key);
                        EditorGUILayout.LabelField(key);
                        EditorGUI.indentLevel++;
                        var instances = handle.GetMaterialInstances();
                        for (var i = 0; i < instances.Length; i++) {
                            var instance = instances[i];
                            EditorGUILayout.ObjectField($"[{i}]", instance.Renderer, typeof(Renderer), true);
                            EditorGUILayout.ObjectField(" ", instance.Material, typeof(Material), false);
                        }
                        EditorGUI.indentLevel--;
                    }, GUILayout.Height(FoldoutHeight));
                }
                
                // Gimmick
                var gimmickController = _body.GetController<GimmickController>();
                if (gimmickController != null) {
                    void DrawActivateGimmick(string key) {
                        var gimmicks = gimmickController.GetActiveGimmicks(key);
                        if (gimmicks.Length <= 0) {
                            return;
                        }

                        var activate = gimmicks[0].IsActive;
                        if (GUILayout.Button(activate ? "Deactivate" : "Activate")) {
                            if (activate) {
                                gimmicks.Deactivate();
                            }
                            else {
                                gimmicks.Activate();
                            }
                        }
                    }
                    
                    void DrawAnimationGimmick(string key) {
                        var gimmicks = gimmickController.GetAnimationGimmicks(key);
                        if (gimmicks.Length <= 0) {
                            return;
                        }

                        if (GUILayout.Button("Play")) {
                            gimmicks.Play();
                        }
                        if (GUILayout.Button("Reverse Play")) {
                            gimmicks.Play(true);
                        }
                    }
                    
                    void DrawInvokeGimmick(string key) {
                        var gimmicks = gimmickController.GetInvokeGimmicks(key);
                        if (gimmicks.Length <= 0) {
                            return;
                        }

                        if (GUILayout.Button("Invoke")) {
                            gimmicks.Invoke();
                        }
                    }
                    
                    void DrawChangeGimmick<T>(string key) {
                        var gimmicks = gimmickController.GetChangeGimmicks<T>(key);
                        if (gimmicks.Length <= 0) {
                            return;
                        }

                        var target = default(object);
                        if (typeof(T) == typeof(Color)) {
                            if (!_gimmickColorParams.TryGetValue(key, out var val)) {
                                val = Color.white;
                                _gimmickColorParams[key] = val;
                            }

                            val = EditorGUILayout.ColorField(GUIContent.none, val);
                            target = val;
                        }
                        if (typeof(T) == typeof(float)) {
                            if (!_gimmickFloatParams.TryGetValue(key, out var val)) {
                                val = 0.0f;
                                _gimmickFloatParams[key] = val;
                            }

                            val = EditorGUILayout.FloatField(GUIContent.none, val);
                            target = val;
                        }
                        if (typeof(T) == typeof(Vector2)) {
                            if (!_gimmickVector2Params.TryGetValue(key, out var val)) {
                                val = Vector2.zero;
                                _gimmickVector2Params[key] = val;
                            }

                            val = EditorGUILayout.Vector2Field(GUIContent.none, val);
                            target = val;
                        }
                        if (typeof(T) == typeof(Vector3)) {
                            if (!_gimmickVector3Params.TryGetValue(key, out var val)) {
                                val = Vector3.zero;
                                _gimmickVector3Params[key] = val;
                            }

                            val = EditorGUILayout.Vector3Field(GUIContent.none, val);
                            target = val;
                        }
                        if (typeof(T) == typeof(Vector4)) {
                            if (!_gimmickVector4Params.TryGetValue(key, out var val)) {
                                val = Vector4.zero;
                                _gimmickVector4Params[key] = val;
                            }

                            val = EditorGUILayout.Vector4Field(GUIContent.none, val);
                            target = val;
                        }
                        
                        if (GUILayout.Button("Change")) {
                            gimmicks.Change((T)target);
                        }
                    }
                    
                    _gimmickFoldoutList.OnGUI(gimmickController.GetKeys(), (key, index) => {
                        EditorGUILayout.LabelField(key);
                        EditorGUI.indentLevel++;
                        DrawActivateGimmick(key);
                        DrawAnimationGimmick(key);
                        DrawInvokeGimmick(key);
                        DrawChangeGimmick<Color>(key);
                        DrawChangeGimmick<float>(key);
                        DrawChangeGimmick<Vector2>(key);
                        DrawChangeGimmick<Vector3>(key);
                        DrawChangeGimmick<Vector4>(key);
                        EditorGUI.indentLevel--;
                    }, GUILayout.Height(FoldoutHeight));
                }
            }
        }
    }
}