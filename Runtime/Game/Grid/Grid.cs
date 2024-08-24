using System;
using System.Collections.Generic;

using Darklight.UnityExt.Editor;

using UnityEngine;
using System.Linq;

namespace Darklight.UnityExt.Game.Grid
{

    #region -- << ABSTRACT CLASS >> : BaseGrid ------------------------------------ >>
    /// <summary>
    ///     Abstract class for a 2D grid. Creates a Grid2D with Cell2D objects.
    /// </summary>
    [System.Serializable]
    public abstract class AbstractGrid
    {
        // ===================== >> PROTECTED DATA << ===================== //
        protected BaseGridMap map;

        // ===================== >> INITIALIZATION << ===================== //
        public AbstractGrid() { }
        public AbstractGrid(GridMapConfig config) => Initialize(config);

        // ===================== >> ABSTRACT METHODS << ===================== //
        public abstract void Initialize(GridMapConfig config);
        public abstract void Update();

        public abstract void SetConfig(GridMapConfig config);
        public abstract void GetData<TData>(out List<TData> data) where TData : BaseCellData;
        public abstract void SetData<TData>(List<TData> data) where TData : BaseCellData;
        public abstract void ClearData();
        public abstract void DrawGizmos(bool editMode);
    }
    #endregion

    #region -- << GENERIC CLASS >> : GRID2D ------------------------------------ >>
    [System.Serializable]
    public class GenericGrid<TCell, TData> : AbstractGrid
        where TCell : BaseCell
        where TData : BaseCellData
    {
        // -- Protected Data ---- >>
        [SerializeField] protected new GenericGridMap<TCell, TData> map;

        // -- Constructor ---- >>
        public GenericGrid(GridMapConfig config)
        {
            Initialize(config);
        }

        public override void Initialize(GridMapConfig config)
        {
            map = new GenericGridMap<TCell, TData>(config);
        }

        public override void Update()
        {
            if (map == null) return;
            map.Update();
        }

        public override void DrawGizmos(bool editMode)
        {
            if (map == null) return;
            map.DrawGizmos(editMode);
        }

        public override void SetConfig(GridMapConfig config)
        {
            if (map == null) return;
            map.SetConfig(config);
        }

        public override void GetData<T>(out List<T> data)
        {
            data = map.GetData() as List<T>;
        }

        public override void SetData<T>(List<T> data)
        {
            map.SetData(data as List<TData>);
        }

        public override void ClearData()
        {
            map.Clear();
        }

        #endregion
    }
}
