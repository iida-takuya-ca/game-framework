using UnityEngine;

namespace SampleGame {
    /// <summary>
    /// プレイヤーアクター初期化用データ
    /// </summary>
    [CreateAssetMenu(fileName = "dat_player_actor_setup_pl000.asset", menuName = "SampleGame/Player Actor Setup Data")]
    public class PlayerActorSetupData : ScriptableObject, PlayerActor.ISetupData {
        public RuntimeAnimatorController controller;
        public AnimationClip vibrateClip;
        public float angularVelocity = 360.0f;
        public GameObject bulletPrefab;
        public GameObject auraPrefab;

        RuntimeAnimatorController PlayerActor.ISetupData.Controller => controller;
        AnimationClip PlayerActor.ISetupData.VibrateClip => vibrateClip;
        float PlayerActor.ISetupData.AngularVelocity => angularVelocity;
        GameObject PlayerActor.ISetupData.BulletPrefab => bulletPrefab;
        GameObject PlayerActor.ISetupData.AuraPrefab => auraPrefab;
    }
}