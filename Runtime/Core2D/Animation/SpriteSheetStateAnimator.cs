using System;
using Darklight.UnityExt.Collection;
using Darklight.UnityExt.Library;
using NaughtyAttributes;
using UnityEngine;

namespace Darklight.UnityExt.Core2D.Animation
{
    public class SpriteSheetStateAnimator<TState> : SpriteSheetAnimator
    {
        [Space(10), HorizontalLine]
        [Header("State Dictionary")]
        CollectionDictionary<TState, SpriteSheet> _spriteSheetDict;
        public CollectionDictionary<TState, SpriteSheet> SpriteSheetDict;

        public void SetState(TState state, Spatial2D.Direction direction = Spatial2D.Direction.NONE)
        {
            SpriteSheetDict.TryGetValue(state, out var spriteSheet);
            if (spriteSheet == null)
                return;

            LoadSpriteSheet(spriteSheet);
        }
    }
}
