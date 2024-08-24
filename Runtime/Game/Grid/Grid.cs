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
        protected GridConfig config;
        protected BaseGridMap map;
        public GridConfig Config => config;
        public BaseGridMap Map => map;

        // ===================== >> INITIALIZATION << ===================== //
        public AbstractGrid() { }
        public AbstractGrid(GridConfig config) => Initialize(config);

        // ===================== >> ABSTRACT METHODS << ===================== //
        public abstract void Initialize(GridConfig config);
        public abstract void UpdateConfig(GridConfig config);
        public abstract void DrawGizmos(bool editMode);
    }
    #endregion

    #region -- << GENERIC CLASS >> : GRID2D ------------------------------------ >>
    [System.Serializable]
    public class GenericGrid<TCell, TData> : AbstractGrid
        where TCell : AbstractCell
        where TData : BaseCellData
    {
        // (( Override GridMap )) ------------------------------ >>
        public new GenericGridMap<TCell, TData> map;

        // -- Constructor ---- >>
        public GenericGrid(GridConfig config)
        {
            Initialize(config);
        }

        public override void Initialize(GridConfig config)
        {
            // ( Set the config )
            this.config = config;

            // ( Rebuild the cell map )
            map = new GenericGridMap<TCell, TData>(config);
            map.ApplyConfigToMap(config);
        }

        public override void UpdateConfig(GridConfig config)
        {
            // ( Set the config )
            this.config = config;

            // ( Update the cell map )
            if (map == null) return;
            map.ApplyConfigToMap(config);
            map.RefreshData();
        }

        public override void DrawGizmos(bool editMode)
        {
            if (!config.showGizmos) return;
            if (map == null) return;
            map.MapFunction(cell =>
            {
                cell.DrawGizmos(editMode);
                return cell;
            });
        }
        #endregion
    }
}
