using System.Collections.Generic;
using System.Linq;
using Darklight.UnityExt.Editor;
using Unity.Android.Gradle.Manifest;
using UnityEngine;
namespace Darklight.UnityExt.Game.Grid
{
    using ComponentType = ICell2DComponent.TypeKey;

    [System.Serializable]
    public class Cell2D
    {
        // ======== [[ SERIALIZED FIELDS ]] ======================================================= >>>>
        [SerializeField, ShowOnly] string _name = "Cell2D";
        [SerializeField] Cell2D_Config _config;
        [SerializeField] Cell2D_Data _data;
        [SerializeField] Cell2D_Composite _composite;

        // ======== [[ PROPERTIES ]] ======================================================= >>>>
        public Cell2D_Config Config { get => _config; }
        public Cell2D_Data Data { get => _data; }
        public Cell2D_Composite Composite { get => _composite; }

        // ======== [[ CONSTRUCTORS ]] ======================================================= >>>>
        public Cell2D(Vector2Int key) => Initialize(key, null);
        public Cell2D(Vector2Int key, Cell2D_Config config) => Initialize(key, config);

        // ======== [[ METHODS ]] ============================================================ >>>>
        // (( RUNTIME )) -------- )))
        public void Initialize(Vector2Int key, Cell2D_Config config)
        {
            // Initialize the configuration
            if (config == null)
                config = new Cell2D_Config();
            _config = config;

            // Create the data
            _data = new Cell2D_Data(key);

            // Create the composite
            _composite = new Cell2D_Composite(this);

            // Set the name
            _name = $"Cell2D ({key.x},{key.y})";
        }

        public void Update()
        {
            if (_data == null) return;
            if (_composite == null) return;

            _composite.UpdateComponents(_config);
        }

        public Cell2D Clone()
        {
            Cell2D clone = new Cell2D(Data.Key);
            Cell2D_Data newData = new Cell2D_Data(Data);
            Cell2D_Config newConfig = new Cell2D_Config(Config);
            Cell2D_Composite newComposite = new Cell2D_Composite(Composite);
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
        public float GetMinDimension() => Mathf.Min(Data.Dimensions.x, Data.Dimensions.y);

        public void GetTransformData(out Vector3 position, out Vector3 normal, out Vector2 dimensions)
        {
            position = Data.Position;
            normal = Data.Normal;
            dimensions = Data.Dimensions;
        }

        public void GetTransformData(out Vector3 position, out float radius, out Vector3 normal)
        {
            position = Data.Position;
            radius = GetMinDimension() / 2;
            normal = Data.Normal;
        }

        // (( SETTERS )) -------- ))
        public void SetData(Cell2D_Data data) => _data = data;
        public void SetConfig(Cell2D_Config config) => _config = config;
        public void SetComposite(Cell2D_Composite composite) => _composite = composite;

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
    }
}
