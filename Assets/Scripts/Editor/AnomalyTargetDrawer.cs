using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(AnomalyTarget))]
public class AnomalyTargetDrawer : PropertyDrawer
{
    private string[] _properties = {"gameObject", "positionOffset", "rotationOffset", "scaleOffset", "transitionTime", "visible"};
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return _properties.Length * EditorGUIUtility.singleLineHeight
               + (_properties.Length - 1) * EditorGUIUtility.standardVerticalSpacing;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, GUIContent.none, property);
        
        var lineHeight = EditorGUIUtility.singleLineHeight;
        var spacing = EditorGUIUtility.standardVerticalSpacing;
        
        for (var i = 0; i < _properties.Length; i++)
        {
            var rect = new Rect(position.x, position.y + (lineHeight + spacing) * i, position.width, lineHeight);
            EditorGUI.PropertyField(rect, property.FindPropertyRelative(_properties[i]));
        }
        
        EditorGUI.EndProperty();
    }
}