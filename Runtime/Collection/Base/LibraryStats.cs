using System;
using UnityEngine;

namespace Darklight.UnityExt.Collection
{
    /// <summary>
    /// Provides detailed statistics for collection libraries.
    /// </summary>
    [Serializable]
    public class CollectionStats
    {
        #region Capacity Statistics
        [SerializeField] private int _totalCapacity;
        [SerializeField] private int _usedCapacity;
        [SerializeField] private int _freeCapacity;
        [SerializeField] private float _capacityUtilization;
        #endregion

        #region Item Statistics
        [SerializeField] private int _totalItems;
        [SerializeField] private int _activeItems;
        [SerializeField] private int _inactiveItems;
        [SerializeField] private int _nullItems;
        [SerializeField] private float _itemUtilization;
        #endregion

        #region Operation Statistics
        [SerializeField] private int _totalOperations;
        [SerializeField] private int _successfulOperations;
        [SerializeField] private int _failedOperations;
        [SerializeField] private float _operationSuccessRate;
        #endregion

        #region Memory Statistics
        [SerializeField] private long _estimatedMemoryUsage;
        [SerializeField] private long _peakMemoryUsage;
        [SerializeField] private float _memoryUtilization;
        #endregion

        #region Performance Statistics
        [SerializeField] private float _averageAccessTime;
        [SerializeField] private float _peakAccessTime;
        [SerializeField] private int _collisions;
        [SerializeField] private float _collisionRate;
        #endregion

        #region Properties
        public int TotalCapacity => _totalCapacity;
        public int UsedCapacity => _usedCapacity;
        public int FreeCapacity => _freeCapacity;
        public float CapacityUtilization => _capacityUtilization;

        public int TotalItems => _totalItems;
        public int ActiveItems => _activeItems;
        public int InactiveItems => _inactiveItems;
        public int NullItems => _nullItems;
        public float ItemUtilization => _itemUtilization;

        public int TotalOperations => _totalOperations;
        public int SuccessfulOperations => _successfulOperations;
        public int FailedOperations => _failedOperations;
        public float OperationSuccessRate => _operationSuccessRate;

        public long EstimatedMemoryUsage => _estimatedMemoryUsage;
        public long PeakMemoryUsage => _peakMemoryUsage;
        public float MemoryUtilization => _memoryUtilization;

        public float AverageAccessTime => _averageAccessTime;
        public float PeakAccessTime => _peakAccessTime;
        public int Collisions => _collisions;
        public float CollisionRate => _collisionRate;
        #endregion

        /// <summary>
        /// Updates capacity-related statistics.
        /// </summary>
        public void UpdateCapacityStats(int totalCapacity, int usedCapacity)
        {
            _totalCapacity = totalCapacity;
            _usedCapacity = usedCapacity;
            _freeCapacity = totalCapacity - usedCapacity;
            _capacityUtilization = totalCapacity > 0 ? (float)usedCapacity / totalCapacity : 0f;
        }

        /// <summary>
        /// Updates item-related statistics.
        /// </summary>
        public void UpdateItemStats(int totalItems, int activeItems, int nullItems)
        {
            _totalItems = totalItems;
            _activeItems = activeItems;
            _inactiveItems = totalItems - activeItems;
            _nullItems = nullItems;
            _itemUtilization = totalItems > 0 ? (float)activeItems / totalItems : 0f;
        }

        /// <summary>
        /// Records an operation and its result.
        /// </summary>
        public void RecordOperation(bool success)
        {
            _totalOperations++;
            if (success)
                _successfulOperations++;
            else
                _failedOperations++;
            _operationSuccessRate = (float)_successfulOperations / _totalOperations;
        }

        /// <summary>
        /// Updates memory usage statistics.
        /// </summary>
        public void UpdateMemoryStats(long currentMemoryUsage)
        {
            _estimatedMemoryUsage = currentMemoryUsage;
            _peakMemoryUsage = Math.Max(_peakMemoryUsage, currentMemoryUsage);
            _memoryUtilization = _peakMemoryUsage > 0 ? (float)currentMemoryUsage / _peakMemoryUsage : 0f;
        }

        /// <summary>
        /// Records access time for performance tracking.
        /// </summary>
        public void RecordAccessTime(float accessTime)
        {
            if (_totalOperations == 0)
            {
                _averageAccessTime = accessTime;
            }
            else
            {
                _averageAccessTime = (_averageAccessTime * _totalOperations + accessTime) / (_totalOperations + 1);
            }
            _peakAccessTime = Math.Max(_peakAccessTime, accessTime);
        }

        /// <summary>
        /// Records a collision in the collection.
        /// </summary>
        public void RecordCollision()
        {
            _collisions++;
            _collisionRate = _totalOperations > 0 ? (float)_collisions / _totalOperations : 0f;
        }

        /// <summary>
        /// Resets all statistics to their default values.
        /// </summary>
        public void Reset()
        {
            _totalCapacity = 0;
            _usedCapacity = 0;
            _freeCapacity = 0;
            _capacityUtilization = 0f;

            _totalItems = 0;
            _activeItems = 0;
            _inactiveItems = 0;
            _nullItems = 0;
            _itemUtilization = 0f;

            _totalOperations = 0;
            _successfulOperations = 0;
            _failedOperations = 0;
            _operationSuccessRate = 0f;

            _estimatedMemoryUsage = 0;
            _peakMemoryUsage = 0;
            _memoryUtilization = 0f;

            _averageAccessTime = 0f;
            _peakAccessTime = 0f;
            _collisions = 0;
            _collisionRate = 0f;
        }

        /// <summary>
        /// Creates a snapshot of the current statistics.
        /// </summary>
        public CollectionStats CreateSnapshot()
        {
            return new CollectionStats
            {
                _totalCapacity = this._totalCapacity,
                _usedCapacity = this._usedCapacity,
                _freeCapacity = this._freeCapacity,
                _capacityUtilization = this._capacityUtilization,
                _totalItems = this._totalItems,
                _activeItems = this._activeItems,
                _inactiveItems = this._inactiveItems,
                _nullItems = this._nullItems,
                _itemUtilization = this._itemUtilization,
                _totalOperations = this._totalOperations,
                _successfulOperations = this._successfulOperations,
                _failedOperations = this._failedOperations,
                _operationSuccessRate = this._operationSuccessRate,
                _estimatedMemoryUsage = this._estimatedMemoryUsage,
                _peakMemoryUsage = this._peakMemoryUsage,
                _memoryUtilization = this._memoryUtilization,
                _averageAccessTime = this._averageAccessTime,
                _peakAccessTime = this._peakAccessTime,
                _collisions = this._collisions,
                _collisionRate = this._collisionRate
            };
        }

        /// <summary>
        /// Returns a formatted string containing all statistics.
        /// </summary>
        public override string ToString()
        {
            return $"Collection Statistics:\n" +
                   $"Capacity: {_usedCapacity}/{_totalCapacity} ({_capacityUtilization:P2})\n" +
                   $"Items: {_activeItems}/{_totalItems} active, {_nullItems} null ({_itemUtilization:P2})\n" +
                   $"Operations: {_successfulOperations}/{_totalOperations} successful ({_operationSuccessRate:P2})\n" +
                   $"Memory: {_estimatedMemoryUsage:N0} bytes (Peak: {_peakMemoryUsage:N0})\n" +
                   $"Performance: Avg {_averageAccessTime:F6}ms, Peak {_peakAccessTime:F6}ms\n" +
                   $"Collisions: {_collisions} ({_collisionRate:P2})";
        }
    }
}
