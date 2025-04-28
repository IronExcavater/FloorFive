//using UnityEditor;
//using UnityEngine;

//[CustomEditor(typeof(Room))]
//public class AnomalyEditor : Editor
//{
//    private void OnSceneGUI()
//    {
//        var room = (Room) target;

//        foreach (var group in room.anomalies)
//        {
//            foreach (var anomaly in group.anomalies)
//            {
//                if (!anomaly.target) continue;
                
//                var trans = anomaly.target.transform;
//                var rend = trans.GetComponent<MeshRenderer>();
                
//                var position = trans.position + trans.TransformDirection(anomaly.positionOffset);
//                var rotation = trans.rotation * Quaternion.Euler(anomaly.rotationOffset);
//                var scale = trans.localScale + anomaly.scaleOffset;
                
//                Handles.color = Color.cyan;
//                Handles.DrawDottedLine(trans.position, position, 10f);
//                if (rend)
//                {
//                    var worldScale = Vector3.Scale(trans.lossyScale, Vector3.one + anomaly.scaleOffset);

//                    // Build matrix for drawing the cube in world space
//                    var matrix = Matrix4x4.TRS(position, rotation, worldScale);

//                    using (new Handles.DrawingScope(Color.cyan, matrix))
//                    {
//                        Handles.DrawWireCube(rend.localBounds.center, rend.localBounds.size);
//                    }
//                }
                
//                EditorGUI.BeginChangeCheck();
                
//                switch (Tools.current)
//                {
//                    case Tool.Move:
//                        position = Handles.PositionHandle(position, Quaternion.identity);
//                        break;
//                    case Tool.Rotate:
//                        rotation = Handles.RotationHandle(rotation, position);
//                        break;
//                    case Tool.Scale:
//                        scale = Handles.ScaleHandle(scale, position, rotation);
//                        break;
//                    case Tool.Transform:
//                        Handles.TransformHandle(ref position, ref rotation, ref scale);
//                        break;
//                }
                
//                if (EditorGUI.EndChangeCheck())
//                {
//                    Undo.RecordObject(room, "Edit Anomaly");
//                    anomaly.positionOffset = trans.InverseTransformDirection(position - trans.position);
//                    anomaly.rotationOffset = (Quaternion.Inverse(trans.rotation) * rotation).eulerAngles;
//                    anomaly.scaleOffset = scale - trans.localScale;
//                }
//            }
//        }
//    }
//}
