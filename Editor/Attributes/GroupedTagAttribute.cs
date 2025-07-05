using System;
using UnityEngine;

namespace Darklight.Editor
{
    /// <summary>
    /// Attribute that allows selecting a Unity tag from a grouped dropdown list.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Field | AttributeTargets.Property,
        Inherited = true,
        AllowMultiple = false
    )]
    public class GroupedTagAttribute : PropertyAttribute { }
}
