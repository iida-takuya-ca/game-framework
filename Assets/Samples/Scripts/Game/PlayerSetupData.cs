using UnityEngine;

namespace SampleGame {
    /// <summary>
    /// プレイヤー制御用アクター
    /// </summary>
    [CreateAssetMenu(fileName = "dat_player_setup_hoge.asset", menuName = "SampleGame/Player Setup Data")]
    public class PlayerSetupData : ScriptableObject, PlayerActor.ISetupData {
        public RuntimeAnimatorController controller;
        public float velocity = 3.0f;
        public float angularVelocity = 360.0f;

        RuntimeAnimatorController PlayerActor.ISetupData.Controller => controller;
        float PlayerActor.ISetupData.Velocity => velocity;
        float PlayerActor.ISetupData.AngularVelocity => angularVelocity;
    }
}
