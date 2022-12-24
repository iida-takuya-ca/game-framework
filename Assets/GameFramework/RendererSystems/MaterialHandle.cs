using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameFramework.RendererSystems {
    /// <summary>
    /// マテリアル制御ハンドル
    /// </summary>
    public struct MaterialHandle {
        private MaterialInstance[] _infos;
        
        // 有効なハンドルか
        public bool IsValid => _infos != null && _infos.Length > 0;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="infos">制御対象のMaterial情報リスト</param>
        public MaterialHandle(IEnumerable<MaterialInstance> infos) {
            _infos = infos.ToArray();
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
        private static void SetValue<T>(MaterialInstance instance, int nameId, T val,
            Action<MaterialPropertyBlock, int, T> setPropertyAction, Action<Material, int, T> setMaterialAction) {
            if (instance.block != null) {
                instance.renderer.GetPropertyBlock(instance.block, instance.materialIndex);
                setPropertyAction.Invoke(instance.block, nameId, val);
                instance.renderer.SetPropertyBlock(instance.block);
            }
            else if (instance.clonedMaterial != null) {
                setMaterialAction.Invoke(instance.clonedMaterial, nameId, val);
            }
            else {
                setMaterialAction.Invoke(instance.material, nameId, val);
            }
        }

        /// <summary>
        /// Materialに適切な方法で値を設定する
        /// </summary>
        private void SetValue<T>(int nameId, T val, Action<MaterialPropertyBlock, int, T> setPropertyAction,
            Action<Material, int, T> setMaterialAction) {
            if (!IsValid) {
                return;
            }

            for (var i = 0; i < _infos.Length; i++) {
                SetValue(_infos[i], nameId, val, setPropertyAction, setMaterialAction);
            }
        }

        /// <summary>
        /// Materialから適切な方法で値を取得する
        /// </summary>
        private static T GetValue<T>(MaterialInstance instance, int nameId,
            Func<MaterialPropertyBlock, int, T> getPropertyAction, Func<Material, int, T> getMaterialAction) {
            if (instance.block != null) {
                instance.renderer.GetPropertyBlock(instance.block, instance.materialIndex);
                return getPropertyAction.Invoke(instance.block, nameId);
            }

            if (instance.clonedMaterial != null) {
                getMaterialAction.Invoke(instance.clonedMaterial, nameId);
            }

            return getMaterialAction.Invoke(instance.material, nameId);
        }

        /// <summary>
        /// Materialから適切な方法で値を取得する
        /// </summary>
        private T GetValue<T>(int nameId, Func<MaterialPropertyBlock, int, T> getPropertyAction,
            Func<Material, int, T> getMaterialAction) {
            if (!IsValid) {
                return default;
            }

            return GetValue(_infos[0], nameId, getPropertyAction, getMaterialAction);
        }
    }
}