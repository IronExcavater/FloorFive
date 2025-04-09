using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomPropertyDrawer(typeof(Anomaly))]
public class AnomalyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var targetsProp = property.FindPropertyRelative("targets");
        
        var targetList = new ReorderableList(property.serializedObject, targetsProp,
            true, true, true, true);
        
        targetList.drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Targets");

        targetList.drawElementCallback = (rect, index, active, focused) =>
        {
            var element = targetsProp.GetArrayElementAtIndex(index);
            EditorGUI.PropertyField(rect, element, GUIContent.none, true);
        };

        targetList.elementHeightCallback = index =>
        {
            return EditorGUI.GetPropertyHeight(targetsProp.GetArrayElementAtIndex(index), true);
        };

        targetList.onAddCallback = list =>
        {
            list.serializedProperty.arraySize++;
            property.serializedObject.ApplyModifiedProperties();

            var newElement = list.serializedProperty.GetArrayElementAtIndex(list.serializedProperty.arraySize - 1);
            newElement.FindPropertyRelative("transitionTime").floatValue = 2f;
            newElement.FindPropertyRelative("visible").boolValue = true;

            property.serializedObject.ApplyModifiedProperties();
        };

        targetList.DoList(position);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var targetsProp = property.FindPropertyRelative("targets");

        var targetList = new ReorderableList(property.serializedObject, targetsProp, true, true, true, true);

        targetList.elementHeightCallback = index => EditorGUI.GetPropertyHeight(targetsProp.GetArrayElementAtIndex(index), true);

        return targetList.GetHeight();
    }
}