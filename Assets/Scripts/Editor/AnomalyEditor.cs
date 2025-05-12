using Anomaly;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(AnomalyBase), true)]
    public class AnomalyEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            AnomalyBase anomaly = (AnomalyBase)target;

            EditorGUILayout.Space();
            if (GUILayout.Button("Set Anomalous Transform to Current"))
            {
                Undo.RecordObject(anomaly, "Set Anomalous Transform to Current");
                Vector3 center = Utils.Utils.GetLocalBounds(anomaly.gameObject).center;
                anomaly.anomalousPosition = anomaly.transform.localPosition + center;
                anomaly.anomalousRotation = anomaly.transform.localEulerAngles;
                EditorUtility.SetDirty(anomaly);
            }
        }
        
        private void OnSceneGUI()
        {
            AnomalyBase anomaly = (AnomalyBase)target;
                    
            Transform trans = anomaly.transform;
            Transform parent = trans.parent;

            Vector3 position = parent.TransformPoint(anomaly.anomalousPosition);
            Quaternion rotation = parent.rotation * Quaternion.Euler(anomaly.anomalousRotation);
            
            Bounds bounds = Utils.Utils.GetLocalBounds(anomaly.gameObject);
            
            Handles.color = Color.cyan;
            Handles.DrawDottedLine(trans.position + bounds.center, position, 10f);

            Matrix4x4 matrix = Matrix4x4.TRS(position, rotation, Vector3.one);
            using (new Handles.DrawingScope(matrix))
            {
                Handles.DrawWireCube(Vector3.zero, bounds.size);
            }
            
            EditorGUI.BeginChangeCheck();
            
            Handles.DrawLine(trans.position, trans.position + bounds.center);
            
            switch (UnityEditor.Tools.current)
            {
                case Tool.Move:
                    position = Handles.PositionHandle(position, Quaternion.identity);
                    break;
                case Tool.Rotate:
                    rotation = Handles.RotationHandle(rotation, position);
                    break;
            }
            
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(anomaly, "Edit Anomaly");
                anomaly.anomalousPosition = parent.InverseTransformPoint(position);
                anomaly.anomalousRotation = (Quaternion.Inverse(parent.rotation) * rotation).eulerAngles;
                EditorUtility.SetDirty(anomaly);
            }
        }
    }
}
