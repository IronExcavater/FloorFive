using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(Anomaly))]
public class AnomalyDrawer : PropertyDrawer
{
    private string[] _properties = {"target", "trigger", "mode", "triggerDelay", "transitionTime",
        "visible", "positionOffset", "rotationOffset", "scaleOffset"};
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var numOfProperties = _properties.Length;
        
        var triggerProp = property.FindPropertyRelative("trigger");
        if (triggerProp.intValue == 1 || triggerProp.intValue == 2) numOfProperties += 2;
        
        return numOfProperties * EditorGUIUtility.singleLineHeight 
               + numOfProperties * EditorGUIUtility.standardVerticalSpacing
               + EditorGUIUtility.singleLineHeight / 2;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, GUIContent.none, property);
        
        var lineHeight = EditorGUIUtility.singleLineHeight;
        var spacing = EditorGUIUtility.standardVerticalSpacing;

        var y = position.y;
        foreach (var propName in _properties)
        {
            var rect = new Rect(position.x, y, position.width, lineHeight);
            var prop = property.FindPropertyRelative(propName);
            EditorGUI.PropertyField(rect, prop);
            y += lineHeight + spacing;

            if (propName == "trigger" && (prop.intValue == 1 || prop.intValue == 2))
            {
                EditorGUI.PropertyField(new Rect(position.x, y, position.width, lineHeight),
                    property.FindPropertyRelative("seenDistance"));
                y+= lineHeight + spacing;
                
                EditorGUI.PropertyField(new Rect(position.x, y, position.width, lineHeight),
                    property.FindPropertyRelative("seenAngle"));
                y += lineHeight + spacing;
            }
        }
        
        EditorGUI.EndProperty();
    }
}