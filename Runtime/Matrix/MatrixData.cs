using Darklight.UnityExt.Editor;

using UnityEngine;

namespace Darklight.UnityExt.Matrix
{
    public partial class Matrix
    {
        [System.Serializable]
        public class Data
        {
            [ShowOnly] public bool isPreloaded;
            [ShowOnly] public bool isInitialized;
            [ShowOnly] public Vector2Int originKey;
            public Node[] nodes;

            public void Reset()
            {
                isPreloaded = false;
                isInitialized = false;
                nodes = new Node[0];
            }
        }
    }
}