using System;
using System.Collections.Generic;
using Darklight.Editor;
using NaughtyAttributes;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Darklight.Core2D.Animation
{
    [ExecuteAlways]
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteSheetAnimator : MonoBehaviour
    {
        const int DEFAULT_FRAMERATE = 4;

        float _timer = 0f;
        bool _animationDone = false;
        int _currentFrame = 0;
        Sprite _currentSprite;

        [SerializeField]
        protected SpriteRenderer sr;

        [SerializeField]
        protected SpriteSheet currentSpriteSheet;

        public virtual void LoadSpriteSheet(SpriteSheet newSpriteSheet)
        {
            if (newSpriteSheet == null)
                return;

            currentSpriteSheet = newSpriteSheet;
            _currentSprite = currentSpriteSheet.GetSpriteAtFrame(0);
            sr.sprite = _currentSprite;

            _animationDone = false;
            _currentFrame = 0;
            _timer = 0f;
        }

        public virtual void Clear()
        {
            currentSpriteSheet = null;
            sr.sprite = null;
            _currentSprite = null;
            _currentFrame = 0;
            _timer = 0f;
            _animationDone = false;
        }

        public virtual bool AnimationIsOver() => _animationDone;

        protected virtual void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
        }

        protected virtual void Update()
        {
            if (sr == null || currentSpriteSheet == null)
                return;
            UpdateFrame();
        }

        void CalculateTimePerFrame(out float timePerFrame)
        {
            timePerFrame = 1f / DEFAULT_FRAMERATE;
            if (currentSpriteSheet != null)
            {
                timePerFrame = 1f / currentSpriteSheet.frameRate;
            }
        }

        protected virtual void UpdateFrame()
        {
            if (currentSpriteSheet == null)
                return;

            if (_currentFrame + 1 == currentSpriteSheet.Length && !currentSpriteSheet.loop)
            {
                _animationDone = true;
                return;
            }

            _timer += Time.deltaTime;

            CalculateTimePerFrame(out float timePerFrame);
            if (_timer >= timePerFrame)
            {
                _currentFrame = (_currentFrame + 1) % currentSpriteSheet.Length;
                _currentSprite = currentSpriteSheet.GetSpriteAtFrame(_currentFrame);
                sr.sprite = _currentSprite;
                _timer -= timePerFrame;
            }
        }

#if UNITY_EDITOR
        [UnityEditor.CustomEditor(typeof(SpriteSheetAnimator), true)]
        public class SpriteAnimatorCustomEditor : UnityEditor.Editor
        {
            SerializedObject _serializedObject;
            SpriteSheetAnimator _script;

            bool _infoFoldout = false;

            private void OnEnable()
            {
                _serializedObject = new SerializedObject(target);
                _script = (SpriteSheetAnimator)target;
            }

            public override void OnInspectorGUI()
            {
                _serializedObject.Update();

                EditorGUI.BeginChangeCheck();

                DrawInfo();
                CustomInspectorGUI.DrawDefaultInspectorWithoutScript(_script);

                if (EditorGUI.EndChangeCheck())
                {
                    _serializedObject.ApplyModifiedProperties();
                }
            }

            void DrawInfo()
            {
                _infoFoldout = CustomInspectorGUI.DrawFoldoutPropertyGroup(
                    "Info",
                    _infoFoldout,
                    () =>
                    {
                        EditorGUI.BeginDisabledGroup(true);
                        EditorGUILayout.LabelField("Timer", _script._timer.ToString());

                        EditorGUILayout.LabelField(
                            "Current Frame",
                            _script._currentFrame.ToString()
                        );
                        EditorGUILayout.LabelField("Current Sprite", _script._currentSprite.name);
                        EditorGUILayout.LabelField(
                            "Current Direction",
                            _script.currentSpriteSheet.startDirection.ToString()
                        );
                        EditorGUILayout.LabelField(
                            "Animation Done",
                            _script._animationDone.ToString()
                        );
                        EditorGUI.EndDisabledGroup();
                    }
                );
            }
        }
#endif
    }
}
