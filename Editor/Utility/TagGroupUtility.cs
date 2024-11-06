using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Darklight.UnityExt.Editor
{
    /// <summary>
    /// Utility class for handling tag grouping operations.
    /// </summary>
    public static class TagGroupUtility
    {
        public const string BUILT_IN_TAGS =
            "Untagged,Respawn,Finish,EditorOnly,MainCamera,Player,GameController";

        /// <summary>
        /// Finds all tags that share the same prefix as the given tag.
        /// </summary>
        /// <param name="tag">The tag to find related tags for</param>
        /// <param name="allTags">Collection of all available tags</param>
        /// <param name="builtInTags">Collection of built-in tags to exclude from grouping</param>
        /// <returns>Collection of tags that share the same prefix, or empty if no matches found</returns>
        public static IEnumerable<string> FindTagsWithCommonPrefix(
            string tag,
            IEnumerable<string> allTags,
            ISet<string> builtInTags
        )
        {
            // Get all possible prefixes for the current tag (minimum 3 characters)
            var possiblePrefixes = Enumerable
                .Range(3, tag.Length - 2)
                .Select(length => tag.Substring(0, length))
                .OrderByDescending(p => p.Length);

            foreach (var prefix in possiblePrefixes)
            {
                var matchingTags = allTags
                    .Where(t => !builtInTags.Contains(t))
                    .Where(t => t.StartsWith(prefix))
                    .ToList();

                // If we found at least two tags (including the current one) with this prefix
                if (matchingTags.Count > 1)
                {
                    return matchingTags;
                }
            }

            return Enumerable.Empty<string>();
        }

        /// <summary>
        /// Groups tags based on common prefixes.
        /// </summary>
        /// <param name="tagProperty">SerializedProperty containing the tags</param>
        /// <param name="builtInTags">Set of built-in tags to exclude from grouping</param>
        /// <param name="defaultGroup">The default group name for ungrouped tags</param>
        /// <returns>Dictionary of group names to TagGroup objects</returns>
        public static Dictionary<string, TagGroup> GroupTags(
            SerializedProperty tagProperty,
            ISet<string> builtInTags,
            string defaultGroup
        )
        {
            var tagGroups = new Dictionary<string, TagGroup>();
            var allTags = GetAllTags(tagProperty).ToList();
            var processedTags = new HashSet<string>();

            for (int i = 0; i < tagProperty.arraySize; i++)
            {
                string tag = tagProperty.GetArrayElementAtIndex(i).stringValue;
                if (builtInTags.Contains(tag) || processedTags.Contains(tag))
                    continue;

                var relatedTags = FindTagsWithCommonPrefix(tag, allTags, builtInTags);

                if (relatedTags.Any())
                {
                    // Use the longest common prefix as the group name
                    string commonPrefix = FindLongestCommonPrefix(relatedTags);

                    if (!tagGroups.ContainsKey(commonPrefix))
                    {
                        tagGroups[commonPrefix] = new TagGroup { Prefix = commonPrefix };
                    }

                    // Add all related tags to the group
                    foreach (var relatedTag in relatedTags)
                    {
                        int tagIndex = FindTagIndex(tagProperty, relatedTag);
                        if (tagIndex != -1)
                        {
                            tagGroups[commonPrefix].Tags.Add((relatedTag, tagIndex));
                            processedTags.Add(relatedTag);
                        }
                    }
                }
                else
                {
                    // Add to default group if no related tags found
                    if (!tagGroups.ContainsKey(defaultGroup))
                    {
                        tagGroups[defaultGroup] = new TagGroup { Prefix = defaultGroup };
                    }
                    tagGroups[defaultGroup].Tags.Add((tag, i));
                    processedTags.Add(tag);
                }
            }

            return tagGroups;
        }

        /// <summary>
        /// Finds the longest common prefix among a collection of strings.
        /// </summary>
        private static string FindLongestCommonPrefix(IEnumerable<string> strings)
        {
            if (!strings.Any())
                return string.Empty;

            var sortedStrings = strings.OrderBy(s => s.Length);
            var firstString = sortedStrings.First();
            var maxLength = firstString.Length;

            for (int i = maxLength; i >= 3; i--)
            {
                string prefix = firstString.Substring(0, i);
                if (strings.All(s => s.StartsWith(prefix)))
                {
                    return prefix;
                }
            }

            return string.Empty;
        }

        private static int FindTagIndex(SerializedProperty tagProperty, string tag)
        {
            for (int i = 0; i < tagProperty.arraySize; i++)
            {
                if (tagProperty.GetArrayElementAtIndex(i).stringValue == tag)
                {
                    return i;
                }
            }
            return -1;
        }

        private static IEnumerable<string> GetAllTags(SerializedProperty tagProperty)
        {
            for (int i = 0; i < tagProperty.arraySize; i++)
            {
                yield return tagProperty.GetArrayElementAtIndex(i).stringValue;
            }
        }
    }
}
