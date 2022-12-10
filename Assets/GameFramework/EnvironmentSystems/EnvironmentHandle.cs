namespace GameFramework.EnvironmentSystems {
    /// <summary>
    /// 環境設定適用後のハンドル
    /// </summary>
    public struct EnvironmentHandle {
        // 制御情報
        private EnvironmentManager.EnvironmentInfo _environmentInfo;

        // 有効なハンドルか
        public bool IsValid => _environmentInfo != null;

        // 完了したか
        public bool IsDone => !IsValid || _environmentInfo.timer <= 0.0f;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="info">制御情報</param>
        public EnvironmentHandle(EnvironmentManager.EnvironmentInfo info) {
            _environmentInfo = info;
        }

        /// <summary>
        /// ハッシュ値の取得
        /// </summary>
        public override int GetHashCode() {
            return _environmentInfo != null ? _environmentInfo.GetHashCode() : 0;
        }
    }
}