using System.Collections.Generic;
using System.Linq;
using Darklight.UnityExt.Editor;
using Unity.Android.Gradle.Manifest;
using UnityEngine;
namespace Darklight.UnityExt.Game.Grid
{

    [System.Serializable]
    public partial class Cell2D
    {
        // ======== [[ SERIALIZED FIELDS ]] ======================================================= >>>>
        [SerializeField, ShowOnly] string _name = "Cell2D";
        [SerializeField] Config _config;
        [SerializeField] SerializedData _data;
        [SerializeField] Composite _composite;

        // ======== [[ PROPERTIES ]] ======================================================= >>>>
        public Vector2Int Key { get => _data.Key; }
        protected Config config { get => _config; }
        protected SerializedData data { get => _data; }
        protected Composite composite { get => _composite; }

        // ======== [[ CONSTRUCTORS ]] ======================================================= >>>>
        public Cell2D(Vector2Int key) => Initialize(key, null);
        public Cell2D(Vector2Int key, Config config) => Initialize(key, config);

        // ======== [[ METHODS ]] ============================================================ >>>>
        // (( RUNTIME )) -------- )))
        public void Initialize(Vector2Int key, Config config)
        {
            // Initialize the configuration
            if (config == null)
                config = new Config();
            _config = config;

            // Create the data
            _data = new SerializedData(key);

            // Create the composite
            _composite = new Composite(this);

            // Set the name
            _name = $"Cell2D ({key.x},{key.y})";
        }

        public void Update()
        {
            if (_data == null) return;
            if (_composite == null) return;

            _composite.UpdateComponents(_config);
        }

        public void RecalculateDataFromGrid(Grid2D grid)
        {
            if (_data == null) return;
            if (_composite == null) return;

            // Calculate the cell's transform
            Grid2D_SpatialUtility.CalculateCellTransform(
                out Vector3 position, out Vector2Int coordinate,
                out Vector3 normal, out Vector2 dimensions,
                this, grid.Config);

            // Assign the calculated values to the cell
            data.SetPosition(position);
            data.SetCoordinate(coordinate);
            data.SetNormal(normal);
            data.SetDimensions(dimensions);
        }

        public Cell2D Clone()
        {
            Cell2D clone = new Cell2D(data.Key);
            SerializedData newData = new SerializedData(data);
            Config newConfig = new Config(config);
            Composite newComposite = new Composite(composite);
            clone.SetData(newData);
            clone.SetConfig(newConfig);
            clone.SetComposite(newComposite);
            return clone;
        }

        // (( VISITOR PATTERN )) -------- ))
        public void Accept(ICell2DVisitor visitor)
        {
            visitor.VisitCell(this);
        }

        // (( GETTERS )) -------- ))
        public float GetMinDimension() => Mathf.Min(data.Dimensions.x, data.Dimensions.y);
        public void GetTransformData(out Vector3 position, out Vector3 normal, out Vector2 dimensions)
        {
            position = data.Position;
            normal = data.Normal;
            dimensions = data.Dimensions;
        }
        public void GetTransformData(out Vector3 position, out float radius, out Vector3 normal)
        {
            position = data.Position;
            radius = GetMinDimension() / 2;
            normal = data.Normal;
        }

        // (( SETTERS )) -------- ))
        protected void SetData(SerializedData data) => _data = data;
        protected void SetConfig(Config config) => _config = config;
        protected void SetComposite(Composite composite) => _composite = composite;


        // (( GIZMOS )) -------- ))
        public void DrawGizmos()
        {
            if (_data == null) return;

            GetTransformData(out Vector3 position, out float radius, out Vector3 normal);
            CustomGizmos.DrawWireSquare(position, radius, normal, Color.gray);

            _composite.MapFunction(component =>
            {
                component.DrawGizmos();
            });
        }

        public void DrawEditorGizmos()
        {
            if (_data == null) return;

            _composite.MapFunction(component =>
            {
                component.DrawEditorGizmos();
            });
        }

        // ========= [[ NESTED TYPES ]] ======================================================= >>>>
        /// <summary>
        /// Enum to represent the different types of components that can be attached to a cell.
        /// Intended to be used as a bit mask to determine which components are present on a cell.
        /// </summary>
        public enum ComponentFlags
        {
            Base = 0,
            Overlap = 1 << 0,
            Shape = 1 << 1,
            Weight = 1 << 2
        }
    }
}
