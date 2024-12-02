using UnityEngine;

namespace MMZZ.Spline
{
    public class SplinePoint : MonoBehaviour
    {
        public Vector3 CurrentPos => transform.position;
        public Vector3 LastPos { get; private set; }

        private void Update()
        {
            UpdateMover();
        }

        public void UpdateMover()
        {
            if (CurrentPos != LastPos)
            {
                var mover = transform.parent.GetComponent<SplineMover>();
                mover.ConnectWithSpline(mover.IsLinear);
                LastPos = CurrentPos;
            }
        }
    }
}