using Anomaly;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(AnomalyBase))]
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
                anomaly.anomalousPosition = anomaly.transform.localPosition;
                anomaly.anomalousRotation = anomaly.transform.localEulerAngles;
                EditorUtility.SetDirty(anomaly);
            }
        }
        
        private void OnSceneGUI()
        {
            AnomalyBase anomaly = (AnomalyBase)target;
                    
            Transform trans = anomaly.transform;
            Transform parent = trans.parent;
            Collider collider = trans.GetComponent<Collider>();

            Vector3 position = parent.TransformPoint(anomaly.anomalousPosition);
            Quaternion rotation = parent.rotation * Quaternion.Euler(anomaly.anomalousRotation);
            
            Handles.color = Color.cyan;
            Handles.DrawDottedLine(trans.position, position, 10f);
            
            if (collider)
            {
                Vector3 localOffset = trans.InverseTransformPoint(collider.bounds.center);
                var matrix = Matrix4x4.TRS(rotation * localOffset + position, rotation, trans.lossyScale);

                using (new Handles.DrawingScope(Color.cyan, matrix))
                {
                    Handles.DrawWireCube(Vector3.zero, collider.bounds.size);
                }
            }
            
            EditorGUI.BeginChangeCheck();
            
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
