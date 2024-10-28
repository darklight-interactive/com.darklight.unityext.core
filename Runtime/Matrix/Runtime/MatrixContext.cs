using System;

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

        [Serializable]
        public struct Context
        {
            readonly Vector2 _minSize => new Vector2(0.125f, 0.125f);
            readonly Vector2 _minSpacing => new Vector2(-0.5f, -0.5f);
            readonly DropdownList<Vector3> _vec3directions => new DropdownList<Vector3>()
            {
                { "Up", Vector3.up },
                { "Down", Vector3.down },
                { "Left", Vector3.left },
                { "Right", Vector3.right },
                { "Forward", Vector3.forward },
                { "Back", Vector3.back },
            };

            public Alignment MatrixAlignment;
            [Range(1, 25)] public int MatrixRows;
            [Range(1, 25)] public int MatrixColumns;
            public Vector3 MatrixPosition;
            [Dropdown("_vec3directions"), AllowNesting] public Vector3 MatrixNormal;
            public Vector2 NodeDimensions;
            public Vector2 NodeSpacing;
            public Vector2 NodeBonding;

            public Context(int rows, int columns)
            {
                MatrixRows = rows > 0 ? rows : 1;
                MatrixColumns = columns > 0 ? columns : 1;

                MatrixAlignment = Alignment.MiddleCenter;
                MatrixPosition = Vector3.zero;
                MatrixNormal = Vector2.up;

                NodeDimensions = new Vector2(1, 1);
                NodeSpacing = new Vector2(0, 0);
                NodeBonding = new Vector2(0, 0);
            }
            public Context(Alignment alignment, int rows, int columns) : this(rows, columns) => MatrixAlignment = alignment;
            public Context(Context context) => this = context;
        }
        public class MatrixContextPreset : ScriptableData<Context> { }
    }
}