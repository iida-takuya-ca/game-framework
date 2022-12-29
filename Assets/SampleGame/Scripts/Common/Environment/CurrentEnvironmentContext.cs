using GameFramework.EnvironmentSystems;

namespace SampleGame {
    /// <summary>
    /// 現在の環境設定
    /// </summary>
    public class CurrentEnvironmentContext : IEnvironmentContext {
        public virtual EnvironmentDefaultSettings DefaultSettings => EnvironmentDefaultSettings.GetCurrent();
    }
}