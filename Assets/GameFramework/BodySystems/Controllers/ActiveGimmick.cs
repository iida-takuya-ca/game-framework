namespace GameFramework.BodySystems {
    /// <summary>
    /// Active制御するGimmickの基底
    /// </summary>
    public abstract class ActiveGimmick : Gimmick {
        // 現在アクティブ状態か
        public bool IsActive { get; private set; }
        
        /// <summary>
        /// アクティブ化
        /// </summary>
        public void Activate() {
            if (IsActive) {
                return;
            }

            IsActive = true;
            ActivateInternal();
        }

        /// <summary>
        /// 非アクティブ化
        /// </summary>
        public void Deactivate() {
            if (!IsActive) {
                return;
            }

            IsActive = false;
            DeactivateInternal();
        }

        /// <summary>
        /// アクティブ化処理
        /// </summary>
        protected abstract void ActivateInternal();
        
        /// <summary>
        /// 非アクティブ化処理
        /// </summary>
        protected abstract void DeactivateInternal();
    }
}