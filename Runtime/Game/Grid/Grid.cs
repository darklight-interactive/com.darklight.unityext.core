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
    public abstract class BaseGrid
    {
        // ===================== >> PROTECTED DATA << ===================== //
        [SerializeField] protected GridConfig config;
        [SerializeField] protected BaseGridMap map;
        public GridConfig Config => config;
        public BaseGridMap Map => map;

        // ===================== >> INITIALIZATION << ===================== //
        public BaseGrid() { }
        public BaseGrid(GridConfig config) => Initialize(config);

        // ===================== >> ABSTRACT METHODS << ===================== //
        public abstract void Initialize(GridConfig config);
        public abstract void UpdateConfig(GridConfig config);
        public abstract void DrawGizmos(bool editMode);
    }
    #endregion

    #region -- << GENERIC CLASS >> : GRID2D ------------------------------------ >>
    [System.Serializable]
    public class GenericGrid<TCell, TData> : BaseGrid
        where TCell : BaseCell
        where TData : BaseCellData
    {

        // (( Override GridMap )) ------------------------------ >>
        protected new GenericGridMap<TCell, TData> map
        {
            get => base.map as GenericGridMap<TCell, TData>;
            set => base.map = value;
        }
        public new GenericGridMap<TCell, TData> Map => map;

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
            map.ApplyConfigToMap(config);
        }

        public override void DrawGizmos(bool editMode)
        {
            if (!config.showGizmos) return;
            map.MapFunction(cell =>
            {
                cell.DrawGizmos(editMode);
                return cell;
            });
        }
        #endregion
    }
}
