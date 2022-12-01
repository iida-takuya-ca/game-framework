using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameFramework.BodySystems {
    /// <summary>
    /// Body用のMaterial制御クラス
    /// </summary>
    public class MaterialController : BodyController {
        /// <summary>
        /// 制御タイプ
        /// </summary>
        public enum ControlType {
            Raw,
            Clone,
            PropertyBlock,
        }

        /// <summary>
        /// マテリアル制御用情報
        /// </summary>
        public class MaterialInfo {
            public Renderer renderer;
            public int materialIndex;
            public Material material;
            public Material clonedMaterial;
            public MaterialPropertyBlock block;

            public MaterialInfo(Renderer renderer, int materialIndex, ControlType controlType) {
                this.renderer = renderer;
                this.materialIndex = materialIndex;
                material = renderer.sharedMaterials[materialIndex];
                switch (controlType) {
                    case ControlType.Clone:
                        clonedMaterial = renderer.materials[materialIndex];
                        break;
                    case ControlType.PropertyBlock:
                        block = new MaterialPropertyBlock();
                        break;
                }
            }
        }
        
        /// <summary>
        /// マテリアル制御ハンドル
        /// </summary>
        public struct MaterialHandle {
            private string _key;
            private List<MaterialInfo> _infos;

            // ハンドルキー
            public string Key => _key ?? "";
            // 有効なハンドルか
            public bool IsValid => _infos != null && _infos.Count > 0;
            
            /// <summary>
            /// コンストラクタ
            /// </summary>
            /// <param name="key">制御用キー</param>
            /// <param name="infos">制御対象のMaterial情報リスト</param>
            public MaterialHandle(string key, List<MaterialInfo> infos) {
                _key = key;
                _infos = infos;
            }
            
            /// <summary>
            /// 各種セッター
            /// </summary>
            public void SetFloat(int nameId, float val) {
                SetValue(nameId, val,
                    (b, n, v) => b.SetFloat(n, v),
                    (m, n, v) => m.SetFloat(n, v));
            }
            public void SetInt(int nameId, int val) {
                SetValue(nameId, val,
                    (b, n, v) => b.SetInt(n, v),
                    (m, n, v) => m.SetInt(n, v));
            }
            public void SetVector(int nameId, Vector4 val) {
                SetValue(nameId, val,
                    (b, n, v) => b.SetVector(n, v),
                    (m, n, v) => m.SetVector(n, v));
            }
            public void SetColor(int nameId, Color val) {
                SetValue(nameId, val,
                    (b, n, v) => b.SetColor(n, v),
                    (m, n, v) => m.SetColor(n, v));
            }
            public void SetMatrix(int nameId, Matrix4x4 val) {
                SetValue(nameId, val,
                    (b, n, v) => b.SetMatrix(n, v),
                    (m, n, v) => m.SetMatrix(n, v));
            }
            public void SetFloatArray(int nameId, float[] val) {
                SetValue(nameId, val,
                    (b, n, v) => b.SetFloatArray(n, v),
                    (m, n, v) => m.SetFloatArray(n, v));
            }
            public void SetVectorArray(int nameId, Vector4[] val) {
                SetValue(nameId, val,
                    (b, n, v) => b.SetVectorArray(n, v),
                    (m, n, v) => m.SetVectorArray(n, v));
            }
            public void SetMatrixArray(int nameId, Matrix4x4[] val) {
                SetValue(nameId, val,
                    (b, n, v) => b.SetMatrixArray(n, v),
                    (m, n, v) => m.SetMatrixArray(n, v));
            }
            public void SetTexture(int nameId, Texture val) {
                SetValue(nameId, val,
                    (b, n, v) => b.SetTexture(n, v),
                    (m, n, v) => m.SetTexture(n, v));
            }
            
            /// <summary>
            /// 各種ゲッター
            /// </summary>
            public float GetFloat(int nameId) {
                return GetValue(nameId,
                    (b, n) => b.GetFloat(n),
                    (m, n) => m.GetFloat(n));
            }
            public int GetInt(int nameId) {
                return GetValue(nameId,
                    (b, n) => b.GetInt(n),
                    (m, n) => m.GetInt(n));
            }
            public Vector4 GetVector(int nameId) {
                return GetValue(nameId,
                    (b, n) => b.GetVector(n),
                    (m, n) => m.GetVector(n));
            }
            public Color GetColor(int nameId) {
                return GetValue(nameId,
                    (b, n) => b.GetColor(n),
                    (m, n) => m.GetColor(n));
            }
            public Matrix4x4 GetMatrix(int nameId) {
                return GetValue(nameId,
                    (b, n) => b.GetMatrix(n),
                    (m, n) => m.GetMatrix(n));
            }
            public float[] GetFloatArray(int nameId) {
                return GetValue(nameId,
                    (b, n) => b.GetFloatArray(n),
                    (m, n) => m.GetFloatArray(n));
            }
            public Vector4[] GetVectorArray(int nameId) {
                return GetValue(nameId,
                    (b, n) => b.GetVectorArray(n),
                    (m, n) => m.GetVectorArray(n));
            }
            public Matrix4x4[] GetMatrixArray(int nameId) {
                return GetValue(nameId,
                    (b, n) => b.GetMatrixArray(n),
                    (m, n) => m.GetMatrixArray(n));
            }
            public Texture GetTexture(int nameId) {
                return GetValue(nameId,
                    (b, n) => b.GetTexture(n),
                    (m, n) => m.GetTexture(n));
            }

            /// <summary>
            /// Materialに適切な方法で値を設定する
            /// </summary>
            private static void SetValue<T>(MaterialInfo info, int nameId, T val, Action<MaterialPropertyBlock, int, T> setPropertyAction, Action<Material, int, T> setMaterialAction) {
                if (info.block != null) {
                    info.renderer.GetPropertyBlock(info.block, info.materialIndex);
                    setPropertyAction.Invoke(info.block, nameId, val);
                    info.renderer.SetPropertyBlock(info.block);
                }
                else if (info.clonedMaterial != null) {
                    setMaterialAction.Invoke(info.clonedMaterial, nameId, val);
                }
                else {
                    setMaterialAction.Invoke(info.material, nameId, val);
                }
            }
            private void SetValue<T>(int nameId, T val, Action<MaterialPropertyBlock, int, T> setPropertyAction, Action<Material, int, T> setMaterialAction) {
                if (!IsValid) {
                    return;
                }
                
                for (var i = 0; i < _infos.Count; i++) {
                    SetValue(_infos[i], nameId, val, setPropertyAction, setMaterialAction);
                }
            }

            /// <summary>
            /// Materialから適切な方法で値を取得する
            /// </summary>
            private static T GetValue<T>(MaterialInfo info, int nameId, Func<MaterialPropertyBlock, int, T> getPropertyAction, Func<Material, int, T> getMaterialAction) {
                if (info.block != null) {
                    info.renderer.GetPropertyBlock(info.block, info.materialIndex);
                    return getPropertyAction.Invoke(info.block, nameId);
                }
                if (info.clonedMaterial != null) {
                    getMaterialAction.Invoke(info.clonedMaterial, nameId);
                }
                return getMaterialAction.Invoke(info.material, nameId);
            }
            private T GetValue<T>(int nameId, Func<MaterialPropertyBlock, int, T> getPropertyAction, Func<Material, int, T> getMaterialAction) {
                if (!IsValid) {
                    return default;
                }

                return GetValue(_infos[0], nameId, getPropertyAction, getMaterialAction);
            }
        }
        
        // キャッシュ用のMaterial情報リスト
        private Dictionary<string, List<MaterialInfo>> _materialInfos = new Dictionary<string, List<MaterialInfo>>();

        // Material情報のリフレッシュ
        public event Action OnRefreshed;

        /// <summary>
        /// 制御キーの一覧を取得
        /// </summary>
        public string[] GetKeys() {
            return _materialInfos.Keys.ToArray();
        }

        /// <summary>
        /// マテリアル制御情報の取得
        /// </summary>
        /// <param name="key">制御用キー</param>
        public MaterialHandle GetHandle(string key) {
            if (_materialInfos.TryGetValue(key, out var result)) {
                return new MaterialHandle();
            }

            return default;
        }
        
        /// <summary>
        /// 初期化処理
        /// </summary>
        protected override void InitializeInternal() {
            var meshController = Body.GetController<MeshController>();
            meshController.OnRefreshed += () => {
                // Material情報の回収
                CreateMaterialInfos();
            };
            CreateMaterialInfos();
        }

        /// <summary>
        /// マテリアル情報の生成
        /// </summary>
        private void CreateMaterialInfos() {
            _materialInfos.Clear();
            
            var partsList = Body.GetComponentsInChildren<MaterialParts>(true);
            for (var i = 0; i < partsList.Length; i++) {
                var parts = partsList[i];
                for (var j = 0; j < parts.infos.Length; j++) {
                    var info = parts.infos[j];
                    if (!_materialInfos.TryGetValue(info.key, out var list)) {
                        list = new List<MaterialInfo>();
                        _materialInfos.Add(info.key, list);
                    }
                    
                    var foundIndex = -1;
                    for (var k = 0; k < info.renderer.sharedMaterials.Length; k++) {
                        if (info.renderer.sharedMaterials[k] != info.material) {
                            break;
                        }
                        foundIndex = k;
                    }

                    if (foundIndex < 0) {
                        Debug.LogWarning($"Not found material. {Body.GameObject.name}:{info.renderer.name}.{info.material.name}");
                        continue;
                    }

                    var materialInfo = new MaterialInfo(info.renderer, foundIndex, ControlType.PropertyBlock);
                    list.Add(materialInfo);
                }
            }
            
            OnRefreshed?.Invoke();
        }
    }
}
