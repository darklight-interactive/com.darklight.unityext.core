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
        [System.Serializable]
        class Config
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
            public bool LockPosToTransform = true;
            public bool LockNormalToTransform = true;

            public Alignment MatrixAlignment = Alignment.Center;

            [HideIf("LockPosToTransform"), AllowNesting] 
            public Vector3 MatrixPosition = new Vector3(0, 0, 0);

            [HideIf("LockNormalToTransform"),Dropdown("_dropdown_vec3directions"), AllowNesting] 
            public Vector3 MatrixNormal = Vector3.up;

            [Space(5)]
            [Range(1, 25)] public int MatrixColumns = 3;
            [Range(1, 25)] public int MatrixRows = 3;

            [Space(5)]
            [Range(0.125f, 10)]public float NodeWidth = 1.0f;
            [Range(0.125f, 10)]public float NodeHeight = 1.0f;
            public Vector2 NodeSpacing;
            public Vector2 NodeBonding;

            public Vector2 NodeDimensions => new Vector2(NodeWidth, NodeHeight);
        }
    }
}