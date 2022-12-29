using GameFramework.EnvironmentSystems;

namespace SampleGame {
    /// <summary>
    /// 環境設定
    /// </summary>
    public interface IEnvironmentContext : GameFramework.EnvironmentSystems.IEnvironmentContext {
        EnvironmentDefaultSettings DefaultSettings { get; }
    }
}