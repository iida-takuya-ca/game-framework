using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Experimental.Animations;
using UnityEngine.Playables;

namespace GameFramework.PlayableSystems {
    /// <summary>
    /// AnimationJobを再生させるためのクラス
    /// </summary>
    public class AnimationJobPlayer : IDisposable {
        // 再生中のProvider情報
        private class PlayingInfo {
            public int order;
            public IAnimationJobProvider provider;
        }
        
        // Playable情報
        private PlayableGraph _graph;
        // Output
        private AnimationPlayableOutput _output;
        
        // 再生中のProvider情報
        private List<PlayingInfo> _sortedPlayingInfos = new List<PlayingInfo>();
        private HashSet<IAnimationJobProvider> _providers = new HashSet<IAnimationJobProvider>();
        
        // Graph更新フラグ
        private bool _dirtyGraph;

        // 再生速度
        private float _speed = 1.0f;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="animator">Outputを反映させるAnimator</param>
        /// <param name="updateMode">更新モード</param>
        /// <param name="outputSortingOrder">Outputの出力オーダー</param>
        public AnimationJobPlayer(Animator animator, DirectorUpdateMode updateMode = DirectorUpdateMode.GameTime,
            ushort outputSortingOrder = 100) {
            _graph = PlayableGraph.Create($"{nameof(PlayablePlayer)}({animator.name})");
            _output = AnimationPlayableOutput.Create(_graph, "Output", animator);

            _graph.SetTimeUpdateMode(updateMode);
            _output.SetSortingOrder(outputSortingOrder);
            _graph.Play();
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        public void Dispose() {
            // 登録されているProviderをDisposeする
            foreach (var info in _sortedPlayingInfos) {
                info.provider.Dispose();
            }
            _sortedPlayingInfos.Clear();
            _providers.Clear();
            
            // Graphを削除
            _graph.Destroy();
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        public void Update() {
            // 時間の更新
            var updateMode = _graph.GetTimeUpdateMode();
            var deltaTime = (updateMode == DirectorUpdateMode.UnscaledGameTime ? Time.unscaledDeltaTime : Time.deltaTime) * _speed;

            // 無効なProviderがいたら削除
            for (var i = _sortedPlayingInfos.Count - 1; i >= 0; i--) {
                var info = _sortedPlayingInfos[i];
                if (info.provider != null && !info.provider.IsDisposed) {
                    continue;
                }

                _providers.Remove(info.provider);
                _sortedPlayingInfos.RemoveAt(i);
                _dirtyGraph = true;
            }

            // Graphの更新
            if (_dirtyGraph) {
                RefreshGraph();
            }
            
            // Providerの更新
            for (var i = 0; i < _sortedPlayingInfos.Count; i++) {
                _sortedPlayingInfos[i].provider.Update(deltaTime);
            }

            // Manualモードの場合、ここで骨の更新を行う
            if (updateMode == DirectorUpdateMode.Manual) {
                _graph.Evaluate(deltaTime);
            }
        }

        /// <summary>
        /// 更新モードの変更
        /// </summary>
        public void SetUpdateMode(DirectorUpdateMode updateMode) {
            _graph.SetTimeUpdateMode(updateMode);
        }

        /// <summary>
        /// Providerの設定
        /// </summary>
        public void SetProvider(IAnimationJobProvider provider, int order = 0) {
            if (provider == null || provider.IsDisposed) {
                Debug.LogError($"Failed provider. {provider}");
                return;
            }

            // 既に設定済み
            if (_providers.Contains(provider)) {
                return;
            }

            // 要素の追加
            _providers.Add(provider);
            _sortedPlayingInfos.Add(new PlayingInfo {
                provider = provider,
                order = order
            });

            _dirtyGraph = true;
        }

        /// <summary>
        /// グラフの更新
        /// </summary>
        private void RefreshGraph() {
            // Listを整理
            _sortedPlayingInfos.Sort((a, b) => a.order.CompareTo(b.order));

            if (_sortedPlayingInfos.Count > 0) {
                // Outputに直列に並べなおす
                var outputPlayable = _sortedPlayingInfos[_sortedPlayingInfos.Count - 1].provider.GetPlayable();
                _output.SetSourcePlayable(outputPlayable);

                for (var i = _sortedPlayingInfos.Count - 2; i >= 0; i--) {
                    var inputPlayable = _sortedPlayingInfos[i].provider.GetPlayable();
                    outputPlayable.DisconnectInput(0);
                    outputPlayable.ConnectInput(0, inputPlayable, 0);
                    outputPlayable = inputPlayable;
                }

                outputPlayable.DisconnectInput(0);
            }
            else {
                // 対象なければ何もしない
                _output.SetSourcePlayable(Playable.Null);
            }

            _dirtyGraph = false;
        }
    }
}