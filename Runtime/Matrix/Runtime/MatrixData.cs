using Darklight.UnityExt.Editor;

using UnityEngine;

namespace Darklight.UnityExt.Matrix
{
    public partial class Matrix
    {
        [System.Serializable]
        public class Data
        {
            Matrix _matrix;


            [Header("World Space Values")]
            [ShowOnly] public Vector3 WorldPosition;

            public Node[] nodes;

            public Data(Matrix matrix)
            {

            }

            public void Reset()
            {
                nodes = new Node[0];
            }
        }
    }
}