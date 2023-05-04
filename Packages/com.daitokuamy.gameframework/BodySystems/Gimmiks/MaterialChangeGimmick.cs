using System.Linq;
using GameFramework.RendererSystems;
using UnityEngine;

namespace GameFramework.BodySystems {
    /// <summary>
    /// Materialの値を設定できるギミック基底
    /// </summary>
    public abstract class MaterialChangeGimmick<T> : ChangeGimmick<T> {
        [SerializeField, Tooltip("制御プロパティ名")]
        private string _propertyName = "";
        [SerializeField, Tooltip("対象のMaterial")]
        private RendererMaterial[] _targets;
        [SerializeField, Tooltip("Materialの制御タイプ")]
        private MaterialInstance.ControlType _controlType = MaterialInstance.ControlType.PropertyBlock;

        // マテリアル制御ハンドル
        private MaterialHandle _materialHandle;
        // プロパティのID
        private int _propertyId;

        /// <summary>
        /// 初期化処理
        /// </summary>
        protected override void InitializeInternal() {
            Refresh();
        }

        /// <summary>
        /// Validate処理
        /// </summary>
        protected override void OnValidateInternal() {
            Refresh();
        }

        /// <summary>
        /// 値の更新
        /// </summary>
        /// <param name="val">反映したい値</param>
        /// <param name="rate">反映率</param>
        protected sealed override void SetValue(T val, float rate) {
            SetValue(_materialHandle, _propertyId, val, rate);
        }

        /// <summary>
        /// 値の更新
        /// </summary>
        /// <param name="handle">マテリアル変更用ハンドル</param>
        /// <param name="propertyId">変更対象のプロパティID</param>
        /// <param name="val">反映したい値</param>
        /// <param name="rate">反映率</param>
        protected abstract void SetValue(MaterialHandle handle, int propertyId, T val, float rate);

        /// <summary>
        /// Material情報のリフレッシュ
        /// </summary>
        private void Refresh() {
            var instances = _targets
                .Where(x => x.IsValid)
                .Select(x =>
                    new MaterialInstance(x.renderer, x.materialIndex, _controlType));
            _materialHandle = new MaterialHandle(instances);
            _propertyId = Shader.PropertyToID(_propertyName);
        }
    }
}