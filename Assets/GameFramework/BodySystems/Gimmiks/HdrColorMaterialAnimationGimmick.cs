using GameFramework.RendererSystems;
using UnityEngine;

namespace GameFramework.BodySystems {
    /// <summary>
    /// HdrColor型のMaterialアニメーションギミック
    /// </summary>
    public class HdrColorMaterialAnimationGimmick : MaterialAnimationGimmick {
        [SerializeField, Tooltip("開始値"), ColorUsage(true, true)]
        private Color _start;
        [SerializeField, Tooltip("終了値"), ColorUsage(true, true)]
        private Color _end;
        
        /// <summary>
        /// 値の更新
        /// </summary>
        protected override void SetValue(MaterialHandle handle, int propertyId, float ratio) {
            var val = Color.Lerp(_start, _end, ratio);
            handle.SetColor(propertyId, val);
        }
    }
}