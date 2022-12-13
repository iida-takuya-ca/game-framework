using GameFramework.EnvironmentSystems;

/// <summary>
/// 環境設定
/// </summary>
public interface IEnvironmentContext : GameFramework.EnvironmentSystems.IEnvironmentContext {
    EnvironmentDefaultSettings DefaultSettings { get; }
}