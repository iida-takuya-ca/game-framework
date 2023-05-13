using GameFramework.CameraSystems;
using GameFramework.Core;
using GameFramework.EntitySystems;
using GameFramework.LogicSystems;
using UnityEngine;

namespace SampleGame.Battle {
    /// <summary>
    /// CameraのAttachment更新用Logic
    /// </summary>
    public class CameraTargetPointLogic : Logic {
        private Entity _playerEntity;
        private BattleAngleModel _angleModel;

        private Transform _playerPoint;
        private Transform _rootAnglePoint;
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public CameraTargetPointLogic(Entity playerEntity, BattleAngleModel angleModel) {
            _playerEntity = playerEntity;
            _angleModel = angleModel;
        }

        /// <summary>
        /// アクティブ時処理
        /// </summary>
        /// <param name="scope"></param>
        protected override void ActivateInternal(IScope scope) {
            var cameraManager = Services.Get<CameraManager>();
            _playerPoint = cameraManager.GetTargetPoint("Player");
            _rootAnglePoint = cameraManager.GetTargetPoint("RootAngle");
        }

        /// <summary>
        /// 後更新処理
        /// </summary>
        protected override void LateUpdateInternal() {
            var playerBody = _playerEntity?.GetBody();
            
            // Playerの追従処理
            if (playerBody != null && _playerPoint != null) {
                _playerPoint.position = playerBody.Position;
                _playerPoint.rotation = playerBody.Rotation;
            }
            
            // Angleの追従処理
            if (playerBody != null && _angleModel != null && _rootAnglePoint != null) {
                _rootAnglePoint.position = playerBody.Position;
                _rootAnglePoint.rotation = Quaternion.Euler(0.0f, _angleModel.Angle.Value, 0.0f);
            }
        }
    }
}