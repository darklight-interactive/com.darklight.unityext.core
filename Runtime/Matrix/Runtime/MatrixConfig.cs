using Darklight.UnityExt.Behaviour;
using Darklight.UnityExt.Editor;

using NaughtyAttributes;

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

#endif

namespace Darklight.UnityExt.Matrix
{
    public partial class Matrix
    {

        public class MatrixContextPreset : ScriptableData<Context> { }

        [System.Serializable]
        public class SerializedConfig
        {
            readonly DropdownList<Vector3> _dropdown_vec3directions = new DropdownList<Vector3>()
            {
                { "Up", Vector3.up },
                { "Down", Vector3.down },
                { "Left", Vector3.left },
                { "Right", Vector3.right },
                { "Forward", Vector3.forward },
                { "Back", Vector3.back },
            };

            [SerializeField] Alignment _matrixAlignment = Alignment.MiddleCenter;
            [SerializeField, Range(1, 25)] int _matrixColumns = 3;
            [SerializeField, Range(1, 25)] int _matrixRows = 3;
            [SerializeField, Range(0.125f, 10.0f)] float _nodeSize = 1.0f;

            /*
                        [SerializeField] bool _lockPosToTransform = true;
                        [SerializeField] bool _lockNormalToTransform = true;

                        [Header("Matrix Config")]
                        [SerializeField, HideIf("_lockPosToTransform"), AllowNesting]
                        Vector3 _matrixLocalPosition = new Vector3(0, 0, 0);
                        [SerializeField, Dropdown("_dropdown_vec3directions"), HideIf("_lockNormalToTransform"), AllowNesting]
                        Vector3 _matrixNormal = Vector3.up;


                        [Header("Node Config")]
                        [SerializeField, Range(0.125f, 10.0f)] float _nodeWidth = 1.0f;
                        [SerializeField, Range(0.125f, 10.0f)] float _nodeHeight = 1.0f;
                        [SerializeField] Vector2 _nodeSpacing = Vector2.zero;
                        [SerializeField] Vector2 _nodeBonding = Vector2.zero;

                        public bool LockPosToTransform => _lockPosToTransform;
                        public bool LockNormalToTransform => _lockNormalToTransform;

                        public Alignment MatrixAlignment => _matrixAlignment;
                        public Vector3 MatrixLocalPosition => _matrixLocalPosition;
                        public Vector3 MatrixNormal => _matrixNormal;
                        public int MatrixColumns => _matrixColumns;
                        public int MatrixRows => _matrixRows;

                        public float NodeWidth => _nodeWidth;
                        public float NodeHeight => _nodeHeight;
                        public Vector2 NodeDimensions => new Vector2(_nodeWidth, _nodeHeight);
                        public float NodeSize => (_nodeWidth + _nodeHeight) * 0.5f;
                        public Vector2 NodeSpacing => _nodeSpacing;
                        public Vector2 NodeBonding => _nodeBonding;

                        public Config()
                        {
                            _lockPosToTransform = true;
                            _lockNormalToTransform = true;

                            _matrixAlignment = Alignment.Center;

                            _matrixLocalPosition = new Vector3(0, 0, 0);
                            _matrixNormal = Vector3.up;

                            _matrixColumns = 3;
                            _matrixRows = 3;

                            _nodeWidth = 1.0f;
                            _nodeHeight = 1.0f;

                            _nodeSpacing = Vector2.zero;
                            _nodeBonding = Vector2.zero;
                        }
                                */

        }

    }
}   