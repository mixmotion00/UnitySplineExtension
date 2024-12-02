using UnityEngine;
using UnityEditor;

namespace MMZZ.Spline
{
    [CustomEditor(typeof(SplinePoint))]
    public class SplinePointEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            SplinePoint point = (SplinePoint)target;
            point.UpdateMover();
        }
    }
}
