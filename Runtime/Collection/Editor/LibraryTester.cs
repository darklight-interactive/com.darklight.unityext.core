using System.Collections.Generic;
using System.Linq;
using System.Text;
using NaughtyAttributes;
using UnityEngine;

namespace Darklight.UnityExt.Collection.Editor
{
    /// <summary>
    /// Editor utility for testing and monitoring CollectionDictionary implementations.
    /// </summary>
    public class LibraryTester : MonoBehaviour
    {
        [SerializeField]
        private CollectionLibrary<GameObject> _library;
    }
}
