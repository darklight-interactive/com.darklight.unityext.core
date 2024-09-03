using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Darklight.UnityExt.Utility
{
    public static class SpriteUtility
    {
        /// <summary>
        /// Adjusts the scale of the sprite's GameObject so that it fits the specified Vector2 size.
        /// </summary>
        /// <param name="spriteRenderer">The SpriteRenderer component of the GameObject.</param>
        /// <param name="targetSize">The desired size in world units as a Vector2 (width, height).</param>
        public static void FitSpriteToSize(SpriteRenderer spriteRenderer, Vector2 targetSize)
        {
            // Get the size of the sprite in units (width and height)
            Vector2 spriteSize = spriteRenderer.sprite.bounds.size;

            // Calculate the scale factor needed to match the target size
            Vector2 scale = new Vector2(targetSize.x / spriteSize.x, targetSize.y / spriteSize.y);

            // Apply the scale to the GameObject
            spriteRenderer.transform.localScale = scale;
        }
    }
}
