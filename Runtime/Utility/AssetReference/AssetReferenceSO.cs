using System;
using UnityEngine;
using AYellowpaper.SerializedCollections;
namespace Darklight
{
    /// <summary>
    /// A scriptable object that stores a dictionary reference to a group of assets.
    /// </summary>
    /// <typeparam name="TEnum">
    /// The enum type that represents the keys of the dictionary.
    /// </typeparam>
    /// <typeparam name="TAsset">
    /// The type of the asset that the dictionary stores.
    /// </typeparam>
    public abstract class AssetReferenceSO<TEnum, TAsset> : ScriptableObject
        where TEnum : Enum
        where TAsset : UnityEngine.Object
    {
        [SerializeField] private SerializedDictionary<TEnum, TAsset> _assetDictionary = new();
        
        protected SerializedDictionary<TEnum, TAsset> assetDictionary => _assetDictionary;
        
        public bool TryGetValue(TEnum key, out TAsset asset)
        {
            return assetDictionary.TryGetValue(key, out asset);
        }
    }
}