using Darklight.UnityExt.Editor;

using NaughtyAttributes;

using UnityEngine;

namespace Darklight.UnityExt.Matrix
{
    public partial class Matrix
    {
        [System.Serializable]
        public class InternalConfig
        {
            readonly DropdownList<Vector3> editor_directions = new DropdownList<Vector3>()
            {
                { "Up", Vector3.up },
                { "Down", Vector3.down },
                { "Left", Vector3.left },
                { "Right", Vector3.right },
                { "Forward", Vector3.forward },
                { "Back", Vector3.back },
            };

            Transform _transform;

            [SerializeField] Alignment _matrixAlignment = Alignment.Center;

            [Space(10)]
            [SerializeField] bool _lockPosToTransform = true;
            [SerializeField, HideIf("_lockPosToTransform"), AllowNesting] Vector3 _matrixPosition = new Vector3(0, 0, 0);
            [SerializeField] bool _lockNormalToTransform = true;
            [SerializeField, HideIf("_lockNormalToTransform"), AllowNesting, Dropdown("editor_directions")] Vector3 _matrixNormal = Vector3.up;

            [Space(10)]
            [SerializeField, ShowOnly] Vector2Int _matrixDimensions = new Vector2Int(3, 3);
            [SerializeField, Range(1, 100)] int _matrixColumns = 3;
            [SerializeField, Range(1, 100)] int _matrixRows = 3;


            [HorizontalLine(4, EColor.Gray)]
            [SerializeField, ShowOnly] Vector2 _nodeDimensions;
            [SerializeField, Range(0.1f, 10)] float _nodeWidth = 1;
            [SerializeField, Range(0.1f, 10)] float _nodeHeight = 1;

            [Space(10)]
            [SerializeField, ShowOnly] Vector2 _nodeSpacing;
            [SerializeField, Range(-1f, 10)] float _nodeSpacingX;
            [SerializeField, Range(-1f, 10)] float _nodeSpacingY;

            [Space(10)]
            [SerializeField, ShowOnly] Vector2 _nodeBonding;
            [SerializeField, Range(-10, 10)] float _nodeBondingX;
            [SerializeField, Range(-10, 10)] float _nodeBondingY;

            // ======== [[ PROPERTIES ]] ============================================================ >>>>
            public bool LockPosToTransform { get => _lockPosToTransform; set => _lockPosToTransform = value; }
            public bool LockNormalToTransform { get => _lockNormalToTransform; set => _lockNormalToTransform = value; }
            public Alignment MatrixAlignment { get => _matrixAlignment; set => _matrixAlignment = value; }
            public Vector3 MatrixPosition => _matrixPosition = CalculatePosition();
            public Vector3 MatrixNormal => _matrixNormal = CalculateNormal();
            public Vector2Int MatrixDimensions
            {
                get
                {
                    _matrixDimensions = new Vector2Int(_matrixColumns, _matrixRows);
                    return _matrixDimensions;
                }
                set
                {
                    _matrixColumns = value.x;
                    _matrixRows = value.y;
                    _matrixDimensions = value;
                }
            }
            public int MatrixWidth { get => _matrixColumns; set => _matrixColumns = value; }
            public int MatrixHeight { get => _matrixRows; set => _matrixRows = value; }

            public Vector2 NodeDimensions
            {
                get
                {
                    _nodeDimensions = new Vector2(_nodeWidth, _nodeHeight);
                    return _nodeDimensions;
                }
                set
                {
                    _nodeWidth = value.x;
                    _nodeHeight = value.y;
                    _nodeDimensions = value;
                }
            }
            public Vector2 NodeSpacing
            {
                get
                {
                    _nodeSpacing = new Vector2(_nodeSpacingX, _nodeSpacingY);
                    return _nodeSpacing;
                }
                set
                {
                    _nodeSpacingX = value.x;
                    _nodeSpacingY = value.y;
                    _nodeSpacing = value;
                }
            }
            public Vector2 NodeBonding
            {
                get
                {
                    _nodeBonding = new Vector2(_nodeBondingX, _nodeBondingY);
                    return _nodeBonding;
                }
                set
                {
                    _nodeBondingX = value.x;
                    _nodeBondingY = value.y;
                    _nodeBonding = value;
                }
            }

            // ======== [[ METHODS ]] ============================================================ >>>>
            public void Copy(InternalConfig config)
            {
                _lockPosToTransform = config._lockPosToTransform;
                _lockNormalToTransform = config._lockNormalToTransform;

                _matrixAlignment = config._matrixAlignment;
                _matrixPosition = config._matrixPosition;
                _matrixNormal = config._matrixNormal;

                MatrixDimensions = config.MatrixDimensions;
                NodeDimensions = config.NodeDimensions;
                NodeSpacing = config.NodeSpacing;
                NodeBonding = config.NodeBonding;
            }

            public void SetToDefaults()
            {
                _lockPosToTransform = true;
                _lockNormalToTransform = true;
                _matrixAlignment = Alignment.Center;
                _matrixPosition = new Vector3(0, 0, 0);
                _matrixNormal = Vector3.up;
                _matrixDimensions = new Vector2Int(3, 3);
                _nodeDimensions = new Vector2(1, 1);
                _nodeSpacing = new Vector2(0, 0);
                _nodeBonding = new Vector2(0, 0);
            }

            public void SetTransform(Transform transform)
            {
                // Set the private transform field
                _transform = transform;
                _matrixPosition = CalculatePosition();
                _matrixNormal = CalculateNormal();
            }

            Vector3 CalculatePosition()
            {
                Vector3 position = _matrixPosition;
                if (_lockPosToTransform && _transform != null)
                {
                    position = _transform.position;
                }
                return position;
            }

            Vector3 CalculateNormal()
            {
                Vector3 normal = _matrixNormal;
                if (_lockNormalToTransform && _transform != null)
                {
                    normal = _transform.forward;
                }
                return normal;
            }

            void OnValidate()
            {
                Debug.Log("OnValidate");
            }
        }
    }
}