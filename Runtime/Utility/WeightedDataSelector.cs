using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

public interface IWeightedData
{
    int Weight { get; }
}

/// <summary>
/// A utility class for selecting random items based on weight.
/// Uses a cumulative weight approach to select items.
/// </summary>
public static class WeightedDataSelector
{
    private static Dictionary<IList, float> cachedTotalWeights = new Dictionary<IList, float>();

    /// <summary>
    /// Clear the cache for total weights. Useful if the item list changes frequently.
    /// </summary>
    public static void ClearCache()
    {
        cachedTotalWeights.Clear();
    }

    /// <summary>
    /// Selects a random item based on weight.
    /// </summary>
    /// <typeparam name="T">The type of the item, which must implement IWeightedData.</typeparam>
    /// <typeparam name="TResult">The type of the result item.</typeparam>
    /// <param name="items">A list of items to choose from.</param>
    /// <param name="itemSelector">A function to select the item from the list element.</param>
    /// <param name="defaultItem">A default item to return if the selection fails.</param>
    /// <returns>A randomly selected item based on its weight.</returns>
    public static TResult SelectRandomWeightedItem<T, TResult>
        (IList<T> items, Func<T, TResult> itemSelector, TResult defaultItem = default)
        where T : IWeightedData
    {
        if (items == null || items.Count == 0)
        {
            return defaultItem;
        }

        float totalWeight;

        // Cache the total weight to avoid recalculating if the list hasn't changed
        if (!cachedTotalWeights.TryGetValue((IList)items, out totalWeight))
        {
            totalWeight = items.Sum(item => item.Weight);
            cachedTotalWeights[(IList)items] = totalWeight;
        }

        // Generate a random number between 0 and totalWeight
        float randomWeight = UnityEngine.Random.Range(0, totalWeight);
        float cumulativeWeight = 0f;

        // Select the item based on the random weight
        foreach (var item in items)
        {
            cumulativeWeight += item.Weight;
            if (randomWeight <= cumulativeWeight)
            {
                return itemSelector(item);
            }
        }

        // Fallback to the default item if something goes wrong
        return defaultItem;
    }


    /// <summary>
    /// Simplified method for selecting a random item from a list of WeightedData.
    /// </summary>
    /// <typeparam name="T">The type of the item.</typeparam>
    /// <param name="items">A list of WeightedData items to choose from.</param>
    /// <returns>A randomly selected item based on its weight.</returns>
    public static T SelectRandomWeightedItem<T>(IList<IWeightedData> items)
    {
        return SelectRandomWeightedItem(items, item => (T)item);
    }

    /// <summary>
    /// Selects multiple random items based on weight, optionally with replacement.
    /// </summary>
    /// <typeparam name="T">The type of the item, which must implement IWeightedData.</typeparam>
    /// <param name="items">A list of items to choose from.</param>
    /// <param name="count">The number of items to select.</param>
    /// <param name="withReplacement">If true, items can be selected multiple times.</param>
    /// <returns>A list of randomly selected items based on their weights.</returns>
    public static List<T> SelectMultipleRandomWeightedItems<T>(IList<T> items, int count, bool withReplacement = false)
        where T : IWeightedData
    {
        List<T> selectedItems = new List<T>();
        List<T> workingList = withReplacement ? new List<T>(items) : items.ToList();

        for (int i = 0; i < count; i++)
        {
            if (workingList.Count == 0)
            {
                break;
            }

            // Use the reworked SelectRandomWeightedItem method to select an item
            T selectedItem = SelectRandomWeightedItem(workingList, item => item, default(T));

            if (!withReplacement)
            {
                // Remove the selected item from the working list
                workingList.Remove(selectedItem);
            }

            selectedItems.Add(selectedItem);
        }

        return selectedItems;
    }

    /// <summary>
    /// Selects the item with the highest weight.
    /// </summary>
    /// <typeparam name="T">The type of the item, which must implement IWeightedData.</typeparam>
    /// <param name="items">A list of items to choose from.</param>
    /// <returns>The item with the highest weight.</returns>
    public static T SelectHighestWeightedItem<T>(IList<T> items)
        where T : IWeightedData
    {
        if (items == null || items.Count == 0)
        {
            return default;
        }

        return items.Aggregate((maxItem, nextItem) => nextItem.Weight > maxItem.Weight ? nextItem : maxItem);
    }

    /// <summary>
    /// Selects the item with the lowest weight.
    /// </summary>
    /// <typeparam name="T">The type of the item, which must implement IWeightedData.</typeparam>
    /// <param name="items">A list of items to choose from.</param>
    /// <returns>The item with the lowest weight.</returns>
    public static T SelectLowestWeightedItem<T>(IList<T> items)
        where T : IWeightedData
    {
        if (items == null || items.Count == 0)
        {
            return default;
        }

        return items.Aggregate((minItem, nextItem) => nextItem.Weight < minItem.Weight ? nextItem : minItem);
    }
}