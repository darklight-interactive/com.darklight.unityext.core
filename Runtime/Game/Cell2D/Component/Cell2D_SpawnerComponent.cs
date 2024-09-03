using UnityEngine;

namespace Darklight.UnityExt.Game.Grid
{
    public class Cell2D_SpawnerComponent : Cell2D.Component
    {
        GameObject _spawnedObj;
        public Cell2D_SpawnerComponent(Cell2D baseObj) : base(baseObj) { }

        public void Spawn(GameObject prefab)
        {
            if (_spawnedObj != null)
            {
                Object.Destroy(_spawnedObj);
            }

            BaseCell.GetTransformData(out Vector3 position, out Vector2 dimensions, out Vector3 normal);
            _spawnedObj = Object.Instantiate(prefab, position, Quaternion.identity);
        }

    }
}