using System;

namespace GameFramework.BodySystems {
    /// <summary>
    /// Body制御クラス基底
    /// </summary>
    public abstract class BodyController : IBodyController {
        // 実行優先度
        public virtual int ExecutionOrder => 0;
        // 制御対象のBody
        public Body Body { get; private set; }

        /// <summary>
        /// 初期化処理
        /// </summary>
        void IBodyController.Initialize(Body body) {
            Body = body;
            InitializeInternal();
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        void IDisposable.Dispose() {
            DisposeInternal();
            Body = null;
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        protected virtual void DisposeInternal() {
        }
        
        /// <summary>
        /// 初期化処理
        /// </summary>
        protected virtual void InitializeInternal() {
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        void IBodyController.Update(float deltaTime) {
            UpdateInternal(deltaTime);
        }
        
        /// <summary>
        /// 更新処理
        /// </summary>
        /// <param name="deltaTime">変位時間</param>
        protected virtual void UpdateInternal(float deltaTime) {
        }

        /// <summary>
        /// 後更新処理
        /// </summary>
        /// <param name="deltaTime">変位時間</param>
        void IBodyController.LateUpdate(float deltaTime) {
            LateUpdateInternal(deltaTime);
        }
        
        /// <summary>
        /// 後更新処理
        /// </summary>
        /// <param name="deltaTime">変位時間</param>
        protected virtual void LateUpdateInternal(float deltaTime) {
        }
    }
}