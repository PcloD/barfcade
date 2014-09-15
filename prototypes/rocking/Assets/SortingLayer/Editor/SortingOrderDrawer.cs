using UnityEngine;
using UnityEditor;
using System;

namespace UnityToolbag
{
    [CustomPropertyDrawer(typeof(SortingOrderAttribute))]
    public class SortingOrderDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // var sortingLayerNames = SortingLayerHelper.sortingLayerNames;
            if (property.propertyType != SerializedPropertyType.Integer) {
                EditorGUI.HelpBox(position, string.Format("{0} is not an integer but has [SortingOrder].", property.name), MessageType.Error);
            }
            // Expose the manual sorting order
            EditorGUI.BeginProperty(position, label, property);
            int newValue = EditorGUI.IntField(position, label.text, property.intValue);
            if (newValue != property.intValue) {
                // renderer.sortingOrder = newValue;
                property.intValue = newValue;
            }
            EditorGUI.EndProperty();

            // int newSortingLayerOrder = EditorGUILayout.IntField("Sorting Layer Order", renderer.sortingOrder);
            // if (newSortingLayerOrder != renderer.sortingOrder) {
            //     Undo.RecordObject(renderer, "Edit Sorting Order");
            //     renderer.sortingOrder = newSortingLayerOrder;
            //     EditorUtility.SetDirty(renderer);
            // }
            // else if (sortingLayerNames != null) {
            //     EditorGUI.BeginProperty(position, label, property);

            //     // Look up the layer name using the current layer ID
            //     string oldName = SortingLayerHelper.GetSortingLayerNameFromID(property.intValue);

            //     // Use the name to look up our array index into the names list
            //     int oldLayerIndex = Array.IndexOf(sortingLayerNames, oldName);

            //     // Show the popup for the names
            //     int newLayerIndex = EditorGUI.Popup(position, label.text, oldLayerIndex, sortingLayerNames);

            //     // If the index changes, look up the ID for the new index to store as the new ID
            //     if (newLayerIndex != oldLayerIndex) {
            //         property.intValue = SortingLayerHelper.GetSortingLayerIDForIndex(newLayerIndex);
            //     }

            //     EditorGUI.EndProperty();
            // }
            // else {
            //     EditorGUI.BeginProperty(position, label, property);
            //     int newValue = EditorGUI.IntField(position, label.text, property.intValue);
            //     if (newValue != property.intValue) {
            //         property.intValue = newValue;
            //     }
            //     EditorGUI.EndProperty();
            // }

        }
    }
}
