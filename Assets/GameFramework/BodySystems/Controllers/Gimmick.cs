using System;
using UnityEngine;

namespace GameFramework.BodySystems {
    /// <summary>
    /// Body内に仕込むGimmickの基底
    /// </summary>
    public abstract class Gimmick : MonoBehaviour {
        /// <summary>
        /// 更新処理
        /// </summary>
        /// <param name="deltaTime">変位時間</param>
        public void UpdateGimmick(float deltaTime) {
            UpdateInternal(deltaTime);
        }
        
        /// <summary>
        /// 後更新処理
        /// </summary>
        /// <param name="deltaTime">変位時間</param>
        public void LateUpdateGimmick(float deltaTime) {
            LateUpdateInternal(deltaTime);
        }
        
        /// <summary>
        /// 初期化処理
        /// </summary>
        protected virtual void InitializeInternal() {}
        
        /// <summary>
        /// 廃棄時処理
        /// </summary>
        protected virtual void DisposeInternal() {}

        /// <summary>
        /// 更新処理
        /// </summary>
        /// <param name="deltaTime">変位時間</param>
        protected virtual void UpdateInternal(float deltaTime) {}

        /// <summary>
        /// 後更新処理
        /// </summary>
        /// <param name="deltaTime">変位時間</param>
        protected virtual void LateUpdateInternal(float deltaTime) {}
        
        /// <summary>
        /// Validate処理
        /// </summary>
        private void OnValidate() {
            // Inspectorに表示しない
            hideFlags |= HideFlags.HideInInspector;
        }

        /// <summary>
        /// 生成時処理
        /// </summary>
        private void Awake() {
            InitializeInternal();
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        private void OnDestroy() {
            DisposeInternal();
        }
    }
}