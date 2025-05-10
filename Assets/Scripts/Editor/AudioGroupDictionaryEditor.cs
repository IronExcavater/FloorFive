using Audio;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(AudioGroupDictionary))]
    public class AudioGroupDictionaryEditor : UnityEditor.Editor
    {
        private SerializedProperty _entriesProp;

        private void OnEnable()
        {
            _entriesProp = serializedObject.FindProperty("entries");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Audio Groups", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            for (int i = 0; i < _entriesProp.arraySize; i++)
            {
                SerializedProperty element = _entriesProp.GetArrayElementAtIndex(i);
                EditorGUILayout.PropertyField(element, new GUIContent($"Group {i + 1}"), true);
            }

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+", GUILayout.Width(30)))
            {
                _entriesProp.InsertArrayElementAtIndex(_entriesProp.arraySize);
                
                SerializedProperty newElement = _entriesProp.GetArrayElementAtIndex(_entriesProp.arraySize - 1);
                newElement.FindPropertyRelative("Key").stringValue = ""; // Clear Key
                SerializedProperty newClips = newElement.FindPropertyRelative("Value").FindPropertyRelative("clips");
                newClips.arraySize = 0; // Clear Value.clips
                newClips.arraySize = 1;
                newClips.isExpanded = true;
            }
            if (GUILayout.Button("-", GUILayout.Width(30)) && _entriesProp.arraySize > 0)
            {
                _entriesProp.DeleteArrayElementAtIndex(_entriesProp.arraySize - 1);
            }
            EditorGUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }
    }
}