using System.Collections;
using System.Collections.Generic;
using Darklight.UnityExt.Editor;
using UnityEngine;


namespace Darklight.UnityExt.Editor.Example
{
    public class ShowOnlyExample : MonoBehaviour, IUnityEditorListener
    {
        [ShowOnly] public int exampleInt = 42;
        [ShowOnly] public bool exampleBool = true;
        [ShowOnly] public float exampleFloat = 3.14159f;
        [ShowOnly] public string exampleString = "Hello, World!";

        [Space(10)]
        [ShowOnly] public Vector2 exampleVector2 = new Vector2(1.1f, 2.2f);
        [ShowOnly] public Vector3 exampleVector3 = new Vector3(1.1f, 2.2f, 3.3f);
        [ShowOnly] public Vector2Int exampleVector2Int = new Vector2Int(1, 2);
        [ShowOnly] public Vector3Int exampleVector3Int = new Vector3Int(1, 2, 3);
        [ShowOnly] public Quaternion exampleQuaternion = Quaternion.Euler(45, 45, 45);

        [Space(10)]
        [ShowOnly] public Color exampleColor = Color.red;
        [ShowOnly] public Bounds exampleBounds = new Bounds(Vector3.zero, Vector3.one);
        [ShowOnly] public Rect exampleRect = new Rect(0, 0, 100, 100);

        [Space(10)]
        [ShowOnly] public GameObject exampleObjectReference;
        [ShowOnly] public AnimationCurve exampleCurve = AnimationCurve.Linear(0, 0, 1, 1);
        [ShowOnly] public LayerMask exampleLayerMask = 1;
        [ShowOnly] public Gradient exampleGradient;
        [ShowOnly] public Transform exampleTransform;

        [Space(10)]
        [ShowOnly] public Sprite exampleSprite;

        public void OnEditorReloaded()
        {
            Reset();
        }

        private void Reset()
        {
            // Assign a default value for the gradient since it's not easy to set inline.
            exampleGradient = new Gradient
            {
                colorKeys = new GradientColorKey[]
                {
                new GradientColorKey(Color.red, 0.0f),
                new GradientColorKey(Color.green, 0.5f),
                new GradientColorKey(Color.blue, 1.0f)
                }
            };

            // Assign a GameObject to the object reference field for demonstration.
            exampleObjectReference = this.gameObject;

            // Assign the default LayerMask value (0 = Nothing, 1 = Everything).
            exampleLayerMask = LayerMask.GetMask("Default");

            exampleTransform = this.transform;
        }
    }
}
