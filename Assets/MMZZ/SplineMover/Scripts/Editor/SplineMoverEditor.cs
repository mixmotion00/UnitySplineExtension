using UnityEngine;
using UnityEditor;
using UnityEngine.Splines;

namespace MMZZ.Spline
{
    [ExecuteAlways]
    [CustomEditor(typeof(SplineMover))]
    public class SplineMoverEditor : Editor
    {
        private Vector3 nextPosOffset; // A field for user input

        private void OnEnable()
        {
            SplineMover splineMover = (SplineMover)target;
            var splineInst = splineMover.GetComponent<SplineInstantiate>();
            if (splineInst != null)
            {
                splineInst.enabled = false;
            }
        }

        public override void OnInspectorGUI()
        {
            SplineMover splineMover = (SplineMover)target;

            EditorGUILayout.Space();

            base.OnInspectorGUI();

            // Draw a field for specifying the next position
            nextPosOffset = EditorGUILayout.Vector3Field("Next Point Position", nextPosOffset);

            if (GUILayout.Button("Create Next Point"))
            {
                splineMover.CreateNextPoint(nextPosOffset);
            }

            if (GUILayout.Button("Remove Last Point"))
            {
                splineMover.RemoveLastPoint();
            }

            if (GUILayout.Button("Clear Points"))
            {
                splineMover.ClearPoints();
            }

            if (GUILayout.Button("Connect With Splines Linear"))
            {
                splineMover.ConnectWithSpline(true);
            }

            if (GUILayout.Button("Connect With Splines Auto"))
            {
                splineMover.ConnectWithSpline(false);
            }

            if (splineMover.AutoRefresh)
            {
                splineMover.ConnectWithSpline(splineMover.IsLinear);
            }
        }
    }
}