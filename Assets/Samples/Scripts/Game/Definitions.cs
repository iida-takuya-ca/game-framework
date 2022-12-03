
namespace SampleGame {
    /// <summary>
    /// タスクの実行順
    /// </summary>
    public enum TaskOrder {
        PreSystem,
        Input,
        Logic,
        Actor,
        Body,
        Collision,
        Camera,
        UI,
        PostSystem,
    }
    
    /// <summary>
    /// ゲーム用の定義処理
    /// </summary>
    public static class Definitions {
    
    }
}