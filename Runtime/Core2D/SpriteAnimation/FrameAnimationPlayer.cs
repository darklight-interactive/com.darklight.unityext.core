using System;
using System.Collections.Generic;
using Darklight.UnityExt.Animation;
using Darklight.UnityExt.Editor;
using NaughtyAttributes;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace Darklight.UnityExt.Animation
{
    public enum DefaultFrameState
    {
        IDLE,
        WALK,
        JUMP,
        ATTACK,
        HURT,
        DEAD
    }

    [ExecuteAlways]
    [RequireComponent(typeof(SpriteRenderer))]
    public class FrameAnimationPlayer : MonoBehaviour
    {
        public enum SpriteDirection
        {
            NONE,
            LEFT,
            RIGHT,
            UP,
            DOWN
        }

        protected const string PREFIX = "FrameAnimationPlayer";

        protected int _defaultFrameRate = 4;

        [SerializeField, ShowOnly]
        int _currentFrame = 0;

        [SerializeField, ShowOnly]
        Sprite _currentSprite;

        [SerializeField, Expandable]
        SpriteSheet _curSpriteSheet;

        [SerializeField]
        SpriteDirection _defaultDirection = SpriteDirection.NONE;

        [SerializeField, ShowOnly]
        SpriteDirection _currentDirection = SpriteDirection.NONE;

        protected float _timer = 0f;
        protected bool _animationDone = false;
        protected SpriteRenderer _spriteRenderer;

        protected float TimePerFrame
        {
            get
            {
                if (_curSpriteSheet != null)
                {
                    return 1f / _curSpriteSheet.frameRate;
                }
                else
                {
                    return 1f / _defaultFrameRate;
                }
            }
        }

        protected virtual void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        protected virtual void Update()
        {
            if (_curSpriteSheet == null)
                return;
            UpdateFrame();
        }

        protected virtual void UpdateFrame()
        {
            if (_curSpriteSheet == null)
                return;

            if (_currentFrame + 1 == _curSpriteSheet.Length && !_curSpriteSheet.loop)
            {
                _animationDone = true;
                return;
            }

            _timer += Time.deltaTime;

            if (_timer >= TimePerFrame)
            {
                _currentFrame = (_currentFrame + 1) % _curSpriteSheet.Length;
                _currentSprite = _curSpriteSheet.GetSpriteAtFrame(_currentFrame);
                _spriteRenderer.sprite = _currentSprite;
                _timer -= TimePerFrame;
            }
        }

        public virtual void LoadSpriteSheet(SpriteSheet newSpriteSheet)
        {
            if (newSpriteSheet == null)
            {
                Debug.LogError($"{PREFIX} SpriteSheet is null.", this);
                return;
            }

            _curSpriteSheet = newSpriteSheet;
            _currentSprite = _curSpriteSheet.GetSpriteAtFrame(0);
            _spriteRenderer.sprite = _currentSprite;

            _animationDone = false;
            _currentFrame = 0;
            _timer = 0f;
        }

        public virtual void Clear()
        {
            _curSpriteSheet = null;
            _spriteRenderer.sprite = null;
            _currentSprite = null;
            _currentFrame = 0;
            _timer = 0f;
            _animationDone = false;
        }

        public virtual bool AnimationIsOver() => _animationDone;

        public void SetFacing(SpriteDirection direction)
        {
            _currentDirection = direction;
            if (_curSpriteSheet == null)
                return;

            if (_currentDirection == SpriteDirection.LEFT)
                _spriteRenderer.flipX = false;
            else if (_currentDirection == SpriteDirection.RIGHT)
                _spriteRenderer.flipX = true;
        }
    }
}
