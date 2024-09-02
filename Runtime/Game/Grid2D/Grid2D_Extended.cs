using UnityEngine;
using NaughtyAttributes;
using System.Collections.Generic;


namespace Darklight.UnityExt.Game.Grid
{
    public class Grid2D_Extended : Grid2D
    {
        // ======== [[ FIELDS ]] ======================================================= >>>>
        [SerializeField, Expandable] Grid2D_ConfigDataObject _configObj;
        [SerializeField, Expandable] Grid2D_SerializedDataObject _dataObj;

        // ======== [[ METHODS ]] ======================================================= >>>>

        #region  -- (( UNITY RUNTIME )) ------------------ >>
        public override void Preload()
        {
            // Assign the grid's config from the config object
            base.config = _configObj.CreateGridConfig();
            if (config.LockToTransform)
            {
                // Set the grid's position and normal to the transform's position and forward
                base.config.SetGridPosition(transform.position);
                base.config.SetGridNormal(transform.forward);
            }
        }

        public override void Initialize()
        {
            base.Initialize();

            // Generate the scriptable objects
            GenerateDataObjects();

            // Create a new grid from the config object
            Config config = _configObj.CreateGridConfig();
            SetConfig(config);
        }

        #endregion

        #region -- (( PUBLIC METHODS )) ------------------ >>

        public virtual void SaveGridData()
        {
            if (_dataObj == null) return;
            _dataObj.SaveCells(GetCells());

            Debug.Log($"{CONSOLE_PREFIX} saved {GetCells().Count} cells.", this);
        }

        public virtual void LoadGridData()
        {
            if (_dataObj == null) return;
            List<Cell2D> loadedCells = _dataObj.LoadCells();
            if (loadedCells == null || loadedCells.Count == 0) return;

            SetCells(loadedCells);
            Debug.Log($"{CONSOLE_PREFIX} loaded {loadedCells.Count} cells.", this);
        }

        public virtual void ClearData()
        {
            if (_dataObj == null) return;
            _dataObj.ClearData();
            Clear();
        }
        #endregion

        // -- (( PRIVATE METHODS )) ------------------ >>
        void GenerateDataObjects()
        {
            _configObj = ScriptableObjectUtility.CreateOrLoadScriptableObject<Grid2D_ConfigDataObject>
                (CONFIG_PATH, $"{CONSOLE_PREFIX}_Config");

            _dataObj = ScriptableObjectUtility.CreateOrLoadScriptableObject<Grid2D_SerializedDataObject>
                (DATA_PATH, $"{CONSOLE_PREFIX}_Data");
        }



    }

}