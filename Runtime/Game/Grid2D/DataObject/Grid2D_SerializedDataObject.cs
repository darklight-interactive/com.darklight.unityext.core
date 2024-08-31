using System.Collections.Generic;
using System.Linq;
using Unity.Android.Gradle.Manifest;
using UnityEngine;

namespace Darklight.UnityExt.Game.Grid
{
    public class Grid2D_SerializedDataObject : ScriptableObject
    {
        [SerializeField] Cell2D_Data[] _savedData;

        public List<Cell2D_Data> GetData()
        {
            if (_savedData == null || _savedData.Length == 0)
                return new List<Cell2D_Data>();
            return _savedData.ToList();
        }

        public void SetData(List<Cell2D_Data> data)
        {
            List<Cell2D_Data> dataList = new List<Cell2D_Data>(data);
            _savedData = dataList.ToArray();
        }

        public void ClearData()
        {
            _savedData = new Cell2D_Data[0];
        }
    }
}
