using System.Collections.Generic;
using System.Linq;
using Unity.Android.Gradle.Manifest;
using UnityEngine;

namespace Darklight.UnityExt.Game.Grid
{
    public class Grid2D_SerializedDataObject : ScriptableObject
    {
        [SerializeField] Cell2D_SerializedData[] _savedData;

        public List<Cell2D_SerializedData> GetData()
        {
            if (_savedData == null || _savedData.Length == 0)
                return new List<Cell2D_SerializedData>();
            return _savedData.ToList();
        }

        public void SetData(List<Cell2D_SerializedData> data)
        {
            List<Cell2D_SerializedData> dataList = new List<Cell2D_SerializedData>(data);
            _savedData = dataList.ToArray();
        }

        public void ClearData()
        {
            _savedData = new Cell2D_SerializedData[0];
        }
    }
}
