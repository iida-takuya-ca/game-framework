namespace GameFramework.BodySystems {
    /// <summary>
    /// Body構築用インターフェース
    /// </summary>
    public interface IBodyBuilder {
        /// <summary>
        /// 構築処理
        /// </summary>
        /// <param name="body">構築対象のBody</param>
        void Build(IBody body);
    }
}