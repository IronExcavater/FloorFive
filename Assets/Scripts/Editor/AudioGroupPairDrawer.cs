using Audio;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomPropertyDrawer(typeof(AudioGroupPair))]
    public class AudioGroupPairDrawer : PropertyDrawer
    {
        private const float FoldoutIconOffset = 15;
        private const float FoldoutWidth = 20;
        private const float ButtonWidth = 20;
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            float lineHeight = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;
            float y = position.y;
            
            SerializedProperty keyProp = property.FindPropertyRelative("Key");
            SerializedProperty clipsProp = property.FindPropertyRelative("Value").FindPropertyRelative("clips");
            
            // Line 1
            Rect foldoutRect = new Rect(position.width + FoldoutIconOffset, y, 0, lineHeight);
            Rect keyRect = new Rect(position.x, y, position.width - FoldoutWidth - spacing, lineHeight);
            y += lineHeight + spacing;
            
            clipsProp.isExpanded = EditorGUI.Foldout(foldoutRect, clipsProp.isExpanded, GUIContent.none, false);
            EditorGUI.PropertyField(keyRect, keyProp, GUIContent.none);
            
            // Line 2
            if (clipsProp.isExpanded)
            {
                for (int i = 0; i < clipsProp.arraySize; i++)
                {
                    SerializedProperty elementProp = clipsProp.GetArrayElementAtIndex(i);
                    Rect elementRect = new Rect(position.x, y, position.width, lineHeight);
                    EditorGUI.PropertyField(elementRect, elementProp, GUIContent.none);
                    y += lineHeight + spacing;
                }

                float buttonX = position.x + position.width - ButtonWidth * 2;
                Rect plusRect = new Rect(buttonX, y, ButtonWidth, lineHeight);
                Rect minusRect = new Rect(buttonX + ButtonWidth + spacing, y, ButtonWidth, lineHeight);
                
                if (GUI.Button(plusRect, "+")) clipsProp.InsertArrayElementAtIndex(clipsProp.arraySize);
                if (GUI.Button(minusRect, "-")) clipsProp.DeleteArrayElementAtIndex(clipsProp.arraySize - 1);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float lineHeight = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;
            float height = 0;
            
            SerializedProperty clipsProp = property.FindPropertyRelative("Value").FindPropertyRelative("clips");

            // Line 1
            height += lineHeight + spacing;
            
            // Line 2
            if (clipsProp.isExpanded)
            {
                for (int i = 0; i < clipsProp.arraySize; i++) height += lineHeight + spacing;
                height += lineHeight + spacing;
            }
            
            return height;
        }
    }
}