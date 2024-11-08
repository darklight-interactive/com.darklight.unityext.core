using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

namespace Darklight.UnityExt.Collection.Editor
{
    /// <summary>
    /// Editor utility for testing and monitoring CollectionDictionary implementations.
    /// </summary>
    public class LibraryTester : MonoBehaviour
    {
        [Header("ScriptableObject Libraries")]
        [SerializeField] private CollectionLibrary<ScriptableObject> _scriptableObjectLibrary = new();
        [SerializeField] private CollectionDictionary<string, ScriptableObject> _scriptableObjectDictionary = new();
        
        [Header("String Libraries")]
        [SerializeField] private CollectionLibrary<string> _stringLibrary = new();
        [SerializeField] private CollectionDictionary<int, string> _indexedStringDictionary = new();
        
        [Header("Primitive Libraries")]
        [SerializeField] private CollectionLibrary<int> _intLibrary = new();
        [SerializeField] private CollectionLibrary<float> _floatLibrary = new();
        [SerializeField] private CollectionLibrary<bool> _boolLibrary = new();
        
        [Header("Vector Libraries")]
        [SerializeField] private CollectionLibrary<Vector2> _vector2Library = new();
        [SerializeField] private CollectionLibrary<Vector3> _vector3Library = new();
        [SerializeField] private CollectionDictionary<string, Vector3> _namedVector3Dictionary = new();
        
        [Header("Unity Type Libraries")]
        [SerializeField] private CollectionLibrary<Color> _colorLibrary = new();
        [SerializeField] private CollectionLibrary<Quaternion> _rotationLibrary = new();
        [SerializeField] private CollectionLibrary<AnimationCurve> _curveLibrary = new();
        
        [Header("Component Libraries")]
        [SerializeField] private CollectionLibrary<Transform> _transformLibrary = new();
        [SerializeField] private CollectionLibrary<Rigidbody> _rigidbodyLibrary = new();
        [SerializeField] private CollectionDictionary<string, MonoBehaviour> _behaviorDictionary = new();
        
        [Header("Array Libraries")]
        [SerializeField] private CollectionLibrary<string[]> _stringArrayLibrary = new();
        [SerializeField] private CollectionLibrary<int[]> _intArrayLibrary = new();
        [SerializeField] private CollectionLibrary<Vector3[]> _vector3ArrayLibrary = new();
        
        [Header("List Libraries")]
        [SerializeField] private CollectionLibrary<List<string>> _stringListLibrary = new();
        [SerializeField] private CollectionLibrary<List<ScriptableObject>> _scriptableObjectListLibrary = new();
        [SerializeField] private CollectionDictionary<string, List<string>> _stringListDictionary = new();
        
        [Header("Complex Type Libraries")]
        [SerializeField] private CollectionLibrary<UnityEvent> _eventLibrary = new();
        [SerializeField] private CollectionDictionary<string, Dictionary<string, object>> _metadataDictionary = new();

        private void OnEnable()
        {
            PopulateTestData();
        }

        [Button]
        private void PopulateTestData()
        {
            // Populate ScriptableObject Libraries
            var testSO1 = ScriptableObject.CreateInstance<TestScriptableObject>();
            var testSO2 = ScriptableObject.CreateInstance<TestScriptableObject>();
            var testSO3 = ScriptableObject.CreateInstance<TestScriptableObject>();
            
            _scriptableObjectLibrary.Add(new CollectionItem<ScriptableObject>(0, testSO1));
            _scriptableObjectLibrary.Add(new CollectionItem<ScriptableObject>(1, testSO2));
            _scriptableObjectLibrary.Add(new CollectionItem<ScriptableObject>(2, testSO3));

            _scriptableObjectDictionary.Add("SO1", testSO1);
            _scriptableObjectDictionary.Add("SO2", testSO2);
            _scriptableObjectDictionary.Add("SO3", testSO3);

            // Populate String Libraries
            _stringLibrary.Add(new CollectionItem<string>(0, "Test String 1"));
            _stringLibrary.Add(new CollectionItem<string>(1, "Test String 2"));
            _stringLibrary.Add(new CollectionItem<string>(2, "Test String 3"));

            _indexedStringDictionary.Add(1, "First String");
            _indexedStringDictionary.Add(2, "Second String");
            _indexedStringDictionary.Add(3, "Third String");

            // Populate Primitive Libraries
            for (int i = 0; i < 3; i++)
            {
                _intLibrary.Add(new CollectionItem<int>(i, i * 100));
                _floatLibrary.Add(new CollectionItem<float>(i, i * 1.5f));
                _boolLibrary.Add(new CollectionItem<bool>(i, i % 2 == 0));
            }

            // Populate Vector Libraries
            _vector2Library.Add(new CollectionItem<Vector2>(0, Vector2.one));
            _vector2Library.Add(new CollectionItem<Vector2>(1, Vector2.right));
            _vector2Library.Add(new CollectionItem<Vector2>(2, Vector2.up));

            _vector3Library.Add(new CollectionItem<Vector3>(0, Vector3.one));
            _vector3Library.Add(new CollectionItem<Vector3>(1, Vector3.forward));
            _vector3Library.Add(new CollectionItem<Vector3>(2, Vector3.up));

            _namedVector3Dictionary.Add("Position1", new Vector3(1, 2, 3));
            _namedVector3Dictionary.Add("Position2", new Vector3(4, 5, 6));
            _namedVector3Dictionary.Add("Position3", new Vector3(7, 8, 9));

            // Populate Unity Type Libraries
            _colorLibrary.Add(new CollectionItem<Color>(0, Color.red));
            _colorLibrary.Add(new CollectionItem<Color>(1, Color.green));
            _colorLibrary.Add(new CollectionItem<Color>(2, Color.blue));

            _rotationLibrary.Add(new CollectionItem<Quaternion>(0, Quaternion.identity));
            _rotationLibrary.Add(new CollectionItem<Quaternion>(1, Quaternion.Euler(90, 0, 0)));
            _rotationLibrary.Add(new CollectionItem<Quaternion>(2, Quaternion.Euler(0, 90, 0)));

            // Populate Array Libraries
            _stringArrayLibrary.Add(new CollectionItem<string[]>(0, new[] { "A1", "A2", "A3" }));
            _stringArrayLibrary.Add(new CollectionItem<string[]>(1, new[] { "B1", "B2", "B3" }));
            _stringArrayLibrary.Add(new CollectionItem<string[]>(2, new[] { "C1", "C2", "C3" }));

            _intArrayLibrary.Add(new CollectionItem<int[]>(0, new[] { 1, 2, 3 }));
            _intArrayLibrary.Add(new CollectionItem<int[]>(1, new[] { 4, 5, 6 }));
            _intArrayLibrary.Add(new CollectionItem<int[]>(2, new[] { 7, 8, 9 }));

            // Populate List Libraries
            _stringListLibrary.Add(new CollectionItem<List<string>>(0, new List<string> { "List1-A", "List1-B", "List1-C" }));
            _stringListLibrary.Add(new CollectionItem<List<string>>(1, new List<string> { "List2-A", "List2-B", "List2-C" }));
            _stringListLibrary.Add(new CollectionItem<List<string>>(2, new List<string> { "List3-A", "List3-B", "List3-C" }));

            _stringListDictionary.Add("List1", new List<string> { "Dict1-A", "Dict1-B", "Dict1-C" });
            _stringListDictionary.Add("List2", new List<string> { "Dict2-A", "Dict2-B", "Dict2-C" });
            _stringListDictionary.Add("List3", new List<string> { "Dict3-A", "Dict3-B", "Dict3-C" });

            // Populate Complex Type Libraries
            _metadataDictionary.Add("Meta1", new Dictionary<string, object> { 
                { "name", "Test1" }, 
                { "value", 42 }, 
                { "active", true } 
            });
            _metadataDictionary.Add("Meta2", new Dictionary<string, object> { 
                { "name", "Test2" }, 
                { "value", 84 }, 
                { "active", false } 
            });
            _metadataDictionary.Add("Meta3", new Dictionary<string, object> { 
                { "name", "Test3" }, 
                { "value", 126 }, 
                { "active", true } 
            });
        }
    }

    public class TestScriptableObject : ScriptableObject
    {
        public string testString = "Test";
        public int testInt = 42;
        public float testFloat = 3.14f;
        public bool testBool = true;
        public Vector3 testVector = Vector3.one;
    }
}
