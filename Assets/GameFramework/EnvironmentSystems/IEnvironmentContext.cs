namespace GameFramework.EnvironmentSystems {
    /// <summary>
    /// 環境設定インターフェース
    /// </summary>
    public interface IEnvironmentContext {
        /// <summary>
        /// 値の反映
        /// </summary>
        void Apply();

        /// <summary>
        /// 値の線形補間
        /// </summary>
        /// <param name="target">相手</param>
        /// <param name="rate">ブレンド率</param>
        IEnvironmentContext Lerp(IEnvironmentContext target, float rate);
    }
}
