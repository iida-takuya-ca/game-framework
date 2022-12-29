using GameFramework.EnvironmentSystems;

namespace SampleGame {
    /// <summary>
    /// 環境設定
    /// </summary>
    public class EnvironmentContext : IEnvironmentContext {
        public EnvironmentDefaultSettings DefaultSettings { get; set; }
    }
}