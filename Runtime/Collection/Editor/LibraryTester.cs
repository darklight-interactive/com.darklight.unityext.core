using System.Collections.Generic;
using System.Linq;
using System.Text;
using NaughtyAttributes;
using UnityEngine;

namespace Darklight.UnityExt.Collection.Editor
{
    /// <summary>
    /// Editor utility for testing and monitoring CollectionDictionary implementations.
    /// </summary>
    public class LibraryTester : MonoBehaviour
    {
        #region Serialized Fields
        [SerializeField, Expandable]
        private CollectionDictionary<string, GameObject> _library;

        [SerializeField]
        private CollectionStats _stats;

        [SerializeField]
        private bool _autoUpdateStats = true;

        [SerializeField]
        private float _statsUpdateInterval = 1f;

        [SerializeField, ReadOnly]
        private string _lastOperationResult;

        [SerializeField]
        private List<GameObject> _testObjects = new();

        [SerializeField]
        private int _testItemCount = 100;
        #endregion

        #region Private Fields
        private float _nextStatsUpdate;
        private StringBuilder _stringBuilder = new();
        #endregion

        #region Unity Methods
        private void Awake()
        {
            InitializeLibrary();
        }

        private void Update()
        {
            if (_autoUpdateStats && Time.time >= _nextStatsUpdate)
            {
                UpdateStats();
                _nextStatsUpdate = Time.time + _statsUpdateInterval;
            }
        }
        #endregion

        #region Test Methods
        [Button("Initialize Library")]
        public void InitializeLibrary()
        {
            _library = new CollectionDictionary<string, GameObject>();
            _stats = new CollectionStats();
            RecordOperation("Library initialized", true);
        }

        [Button("Generate Test Data")]
        public void GenerateTestData()
        {
            _testObjects.Clear();
            for (int i = 0; i < _testItemCount; i++)
            {
                var go = new GameObject($"Test_Object_{i}");
                go.transform.SetParent(transform);
                _testObjects.Add(go);
            }
            RecordOperation($"Generated {_testItemCount} test objects", true);
        }

        [Button("Add Test Objects")]
        public void AddTestObjects()
        {
            var startTime = Time.realtimeSinceStartup;
            int successCount = 0;

            foreach (var obj in _testObjects)
            {
                try
                {
                    _library.Add(obj.name, obj);
                    successCount++;
                    _stats.RecordOperation(true);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Failed to add {obj.name}: {e.Message}");
                    _stats.RecordOperation(false);
                }
            }

            var elapsed = (Time.realtimeSinceStartup - startTime) * 1000f;
            _stats.RecordAccessTime(elapsed / _testObjects.Count);

            RecordOperation(
                $"Added {successCount}/{_testObjects.Count} objects in {elapsed:F2}ms",
                successCount == _testObjects.Count
            );
            UpdateStats();
        }

        [Button("Remove Random Items")]
        public void RemoveRandomItems(int count = 10)
        {
            var startTime = Time.realtimeSinceStartup;
            var keys = _library
                .Items.Select(i => ((KeyValueCollectionItem<string, GameObject>)i).Key)
                .ToList();
            int successCount = 0;

            for (int i = 0; i < Mathf.Min(count, keys.Count); i++)
            {
                var randomIndex = Random.Range(0, keys.Count);
                var key = keys[randomIndex];
                keys.RemoveAt(randomIndex);

                try
                {
                    if (
                        _library.Remove(
                            new KeyValueCollectionItem<string, GameObject>(0, key, null)
                        )
                    )
                    {
                        successCount++;
                        _stats.RecordOperation(true);
                    }
                    else
                    {
                        _stats.RecordOperation(false);
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Failed to remove {key}: {e.Message}");
                    _stats.RecordOperation(false);
                }
            }

            var elapsed = (Time.realtimeSinceStartup - startTime) * 1000f;
            _stats.RecordAccessTime(elapsed / count);

            RecordOperation(
                $"Removed {successCount}/{count} items in {elapsed:F2}ms",
                successCount == count
            );
            UpdateStats();
        }

        [Button("Perform Random Access Test")]
        public void PerformRandomAccess(int iterations = 100)
        {
            var startTime = Time.realtimeSinceStartup;
            var keys = _library
                .Items.Select(i => ((KeyValueCollectionItem<string, GameObject>)i).Key)
                .ToList();
            int successCount = 0;

            for (int i = 0; i < iterations; i++)
            {
                if (keys.Count == 0)
                    break;

                var randomKey = keys[Random.Range(0, keys.Count)];
                try
                {
                    if (_library.TryGetValue(randomKey, out GameObject value))
                    {
                        successCount++;
                        _stats.RecordOperation(true);
                    }
                    else
                    {
                        _stats.RecordOperation(false);
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Failed to access {randomKey}: {e.Message}");
                    _stats.RecordOperation(false);
                }
            }

            var elapsed = (Time.realtimeSinceStartup - startTime) * 1000f;
            _stats.RecordAccessTime(elapsed / iterations);

            RecordOperation(
                $"Accessed {successCount}/{iterations} items in {elapsed:F2}ms",
                successCount == iterations
            );
            UpdateStats();
        }

        [Button("Clear Library")]
        public void ClearLibrary()
        {
            var startTime = Time.realtimeSinceStartup;
            int itemCount = _library.Count;

            try
            {
                _library.Clear();
                var elapsed = (Time.realtimeSinceStartup - startTime) * 1000f;
                _stats.RecordOperation(true);
                _stats.RecordAccessTime(elapsed);
                RecordOperation($"Cleared {itemCount} items in {elapsed:F2}ms", true);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to clear library: {e.Message}");
                _stats.RecordOperation(false);
                RecordOperation("Failed to clear library", false);
            }

            UpdateStats();
        }

        [Button("Reset Stats")]
        public void ResetStats()
        {
            _stats.Reset();
            RecordOperation("Statistics reset", true);
        }
        #endregion

        #region Utility Methods
        private void UpdateStats()
        {
            if (_library == null || _stats == null)
                return;

            _stats.UpdateCapacityStats(_library.Capacity, _library.Count);
            _stats.UpdateItemStats(
                _library.Count,
                _library.Items.Count(i => i != null),
                _library.Items.Count(i => i == null)
            );

            // Estimate memory usage (rough approximation)
            long estimatedMemory = _library.Count * (sizeof(int) + sizeof(int) + 16); // Key index + reference + overhead
            _stats.UpdateMemoryStats(estimatedMemory);
        }

        private void RecordOperation(string message, bool success)
        {
            _stringBuilder.Clear();
            _stringBuilder.Append(success ? "<color=green>" : "<color=red>");
            _stringBuilder.Append(message);
            _stringBuilder.Append("</color>");
            _lastOperationResult = _stringBuilder.ToString();
        }
        #endregion
    }
}
