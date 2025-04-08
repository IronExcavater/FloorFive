using UnityEditor;
using UnityEngine;
using Matrix4x4 = UnityEngine.Matrix4x4;
using Quaternion = UnityEngine.Quaternion;

[CustomEditor(typeof(Room))]
public class AnomalyEditor : Editor
{
    private void OnSceneGUI()
    {
        var room = (Room) target;

        foreach (var anomaly in room.anomalies)
        {
            foreach (var target in anomaly.targets)
            {
                var trans = target.gameObject.transform;
                var rend = trans.GetComponent<MeshRenderer>();
                
                var position = trans.position + trans.TransformDirection(target.positionOffset);
                var rotation = trans.rotation * Quaternion.Euler(target.rotationOffset);
                var scale = trans.localScale + target.scaleOffset;
                
                Handles.color = Color.cyan;
                Handles.DrawDottedLine(trans.position, position, 10f);
                if (rend)
                {
                    Handles.matrix = Matrix4x4.TRS(position, rotation, scale);
                    Handles.DrawWireCube(Vector3.zero, rend.bounds.size);
                    Handles.matrix = Matrix4x4.identity;
                }
                
                EditorGUI.BeginChangeCheck();
                
                switch (Tools.current)
                {
                    case Tool.Move:
                        position = Handles.PositionHandle(position, Quaternion.identity);
                        break;
                    case Tool.Rotate:
                        rotation = Handles.RotationHandle(rotation, position);
                        break;
                    case Tool.Scale:
                        scale = Handles.ScaleHandle(scale, position, rotation);
                        break;
                    case Tool.Transform:
                        Handles.TransformHandle(ref position, ref rotation, ref scale);
                        break;
                }
                
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(room, "Edit Anomaly Target");
                    target.positionOffset = trans.InverseTransformDirection(position - trans.position);
                    target.rotationOffset = (Quaternion.Inverse(trans.rotation) * rotation).eulerAngles;
                    target.scaleOffset = scale - trans.localScale;
                }
            }
        }
    }
}
