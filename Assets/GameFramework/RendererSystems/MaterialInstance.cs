using UnityEngine;

namespace GameFramework.RendererSystems {
    /// <summary>
    /// マテリアル制御用インスタンス
    /// </summary>
    public class MaterialInstance {
        /// <summary>
        /// 制御タイプ
        /// </summary>
        public enum ControlType {
            Raw,
            Clone,
            PropertyBlock,
        }

        public Renderer renderer;
        public int materialIndex;
        public Material material;
        public Material clonedMaterial;
        public MaterialPropertyBlock block;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="renderer">Materialを保持するRenderer</param>
        /// <param name="materialIndex">MaterialのIndex</param>
        /// <param name="controlType">制御タイプ</param>
        public MaterialInstance(Renderer renderer, int materialIndex, ControlType controlType) {
            this.renderer = renderer;
            this.materialIndex = materialIndex;
            material = renderer.sharedMaterials[materialIndex];
            switch (controlType) {
                case ControlType.Clone:
                    clonedMaterial = renderer.materials[materialIndex];
                    break;
                case ControlType.PropertyBlock:
                    block = new MaterialPropertyBlock();
                    break;
            }
        }
    }
}