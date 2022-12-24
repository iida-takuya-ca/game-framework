using GameFramework.RendererSystems;
using UnityEngine;

namespace GameFramework.BodySystems {
    /// <summary>
    /// Float型のMaterialアニメーションギミック
    /// </summary>
    public class FloatMaterialAnimationGimmick : MaterialAnimationGimmick {
        [SerializeField, Tooltip("開始値")]
        private float _start;
        [SerializeField, Tooltip("終了値")]
        private float _end;
        
        /// <summary>
        /// 値の更新
        /// </summary>
        protected override void SetValue(MaterialHandle handle, int propertyId, float ratio) {
            var val = Mathf.Lerp(_start, _end, ratio);
            handle.SetFloat(propertyId, val);
        }
    }
}