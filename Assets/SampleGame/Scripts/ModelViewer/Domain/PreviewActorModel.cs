using GameFramework.ModelSystems;
using UniRx;
using UnityEngine;

namespace SampleGame.ModelViewer {
    /// <summary>
    /// 表示用アクターモデル
    /// </summary>
    public class PreviewActorModel : AutoIdModel<PreviewActorModel> {
        private ReactiveProperty<PreviewActorSetupData> _setupData = new();
        private ReactiveProperty<AnimationClip> _currentAnimationClip = new();
        private ReactiveProperty<AnimationClip> _currentAdditiveAnimationClip = new();

        /// <summary>初期化データID</summary>
        public string SetupDataId { get; private set; }
        /// <summary>Actor初期化用データ</summary>
        public IReadOnlyReactiveProperty<PreviewActorSetupData> SetupData => _setupData;
        /// <summary>現在再生中のClip</summary>
        public IReadOnlyReactiveProperty<AnimationClip> CurrentAnimationClip => _currentAnimationClip;
        /// <summary>現在再生中の加算Clip</summary>
        public IReadOnlyReactiveProperty<AnimationClip> CurrentAdditiveAnimationClip => _currentAdditiveAnimationClip;

        /// <summary>
        /// データの設定
        /// </summary>
        public void Setup(string setupDataId, PreviewActorSetupData actorSetupData) {
            // 現在のクリップをクリアする
            _currentAdditiveAnimationClip.Value = null;
            _currentAnimationClip.Value = null;
            
            // ID記憶
            SetupDataId = setupDataId;

            // ActorData更新
            _setupData.Value = actorSetupData;

            // 初期状態のクリップを設定
            ChangeClip(0);
            ChangeAdditiveClip(-1);
        }

        /// <summary>
        /// アニメーションクリップの変更
        /// ※同じClipを設定したら再度再生
        /// </summary>
        public void ChangeClip(int clipIndex) {
            _currentAnimationClip.Value = null;
            _currentAnimationClip.Value = GetAnimationClip(clipIndex);
        }

        /// <summary>
        /// 加算用アニメーションクリップの変更
        /// ※同じClipを設定したらトグル
        /// </summary>
        public void ChangeAdditiveClip(int clipIndex) {
            var currentClip = _currentAdditiveAnimationClip.Value;
            var nextClip = GetAnimationClip(clipIndex);
            _currentAdditiveAnimationClip.Value = null;
            if (currentClip != nextClip) {
                _currentAdditiveAnimationClip.Value = nextClip;
            }
        }

        /// <summary>
        /// AnimationClipの取得
        /// </summary>
        private AnimationClip GetAnimationClip(int index) {
            if (_setupData.Value == null) {
                return null;
            }

            var clips = _setupData.Value.animationClips;

            if (index < 0 || index >= clips.Length) {
                return null;
            }

            return clips[index];
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        private PreviewActorModel(int id)
            : base(id) {
        }
    }
}