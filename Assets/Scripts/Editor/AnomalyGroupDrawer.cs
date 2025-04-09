using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomPropertyDrawer(typeof(AnomalyGroup))]
public class AnomalyGroupDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var targetsProp = property.FindPropertyRelative("anomalies");
        
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
            newElement.FindPropertyRelative("transitionTime").floatValue = 2;
            newElement.FindPropertyRelative("visible").boolValue = true;
            newElement.FindPropertyRelative("seenDistance").floatValue = 3;
            newElement.FindPropertyRelative("seenAngle").floatValue = 100;

            property.serializedObject.ApplyModifiedProperties();
        };

        targetList.DoList(position);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var targetsProp = property.FindPropertyRelative("anomalies");

        var targetList = new ReorderableList(property.serializedObject, targetsProp, true, true, true, true);

        targetList.elementHeightCallback = index => EditorGUI.GetPropertyHeight(targetsProp.GetArrayElementAtIndex(index), true);

        return targetList.GetHeight();
    }
}