using ActionSequencer;
using UnityEngine;

namespace SampleGame {
    /// <summary>
    /// プレイヤーアクターのアクションデータ
    /// </summary>
    [CreateAssetMenu(fileName = "dat_player_actor_action_pl000_hoge.asset",
        menuName = "SampleGame/Player Actor Action Data")]
    public class PlayerActorActionData : ScriptableObject, PlayerActor.IActionData {
        [Tooltip("アクション用のAnimatorController")]
        public RuntimeAnimatorController controller;
        [Tooltip("アクション再生時に実行するSequenceClip")]
        public SequenceClip sequenceClip;

        public RuntimeAnimatorController Controller => controller;
        public SequenceClip SequenceClip => sequenceClip;
    }
}