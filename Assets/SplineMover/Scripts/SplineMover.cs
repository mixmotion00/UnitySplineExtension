using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;
using System.Linq;

namespace MMZZ.Spline
{
    [RequireComponent(typeof(SplineContainer), typeof(SplineInstantiate))]
    public class SplineMover : MonoBehaviour
    {
        [SerializeField] public bool AutoRefresh;
        [SerializeField] private GameObject _pointPrefab;

        public bool IsLinear { get; set; } = false;

        private SplineContainer _splineCont;

        private int _childCount
        {
            get
            {
                var childCount = 0;

                for (int i = 0; i < transform.childCount; i++)
                {
                    var child = transform.GetChild(i);
                    if (child.gameObject.hideFlags != HideFlags.None || !child.gameObject.activeInHierarchy) continue;
                    childCount++;
                }

                return childCount;
            }
        }

        public void AutoCreateNextPoint()
        {
            EnableSplineInst(false);

            if (_childCount == 0)
            {
                var entry = Instantiate(_pointPrefab, transform);
                entry.transform.position = transform.position;
            }

            var point = Instantiate(_pointPrefab, transform);
            var lastPos = _splineCont.Spline.Knots.ToList().Last().Position;
            point.transform.position = new Vector3(10, 0, 0) + new Vector3(lastPos.x, 0, lastPos.z);
        }

        public void CreateNextPoint(Vector3 nextPos)
        {
            EnableSplineInst(false);

            if (_childCount == 0)
            {
                var entry = Instantiate(_pointPrefab, transform);
                entry.transform.position = transform.position;
            }

            var point = Instantiate(_pointPrefab, transform);
            point.transform.position = nextPos + transform.position;

            EnableSplineInst(true);
        }

        public void ConnectWithSpline(bool isLinear)
        {
            EnableSplineInst(false);

            if (_childCount == 0) return;

            //var entry = new BezierKnot(new float3(transform.position.x, transform.position.y, transform.position.z));

            ClearSplineInst();
            ClearSplineContainer();

            //_splineCont.Spline.Add(entry);

            for (int i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                //Debug.Log($"{child.gameObject.hideFlags}");
                if (child.gameObject.hideFlags == HideFlags.HideInHierarchy || !child.gameObject.activeInHierarchy) continue;
                var point = new BezierKnot(new float3(child.localPosition.x, child.localPosition.y, child.localPosition.z));
                _splineCont.Spline.Add(point);
            }

            if (!isLinear)
                _splineCont.Spline.SetTangentMode(TangentMode.AutoSmooth);
            else
                _splineCont.Spline.SetTangentMode(TangentMode.Linear);

            //foreach(Transform child in transform)
            //{
            //    var point = new BezierKnot(new float3(child.position.x, child.position.y, child.position.z));
            //    _splineCont.Spline.Add(point);
            //}

            var splineInst = GetComponent<SplineInstantiate>();
            splineInst.UpdateInstances();

            IsLinear = isLinear;
        }

        public void RemoveLastPoint()
        {
            var child = transform.GetChild(_childCount - 1);
#if UNITY_EDITOR
            DestroyImmediate(child.gameObject);
#else
            Destroy(child.gameObject);
#endif
        }

        public void ClearPoints()
        {
            while (transform.childCount > 0)
            {
#if UNITY_EDITOR
                DestroyImmediate(transform.GetChild(0).gameObject);
#else
                Destroy(transform.GetChild(0).gameObject);
#endif
            }

            //        foreach (Transform item in transform)
            //        {
            //#if UNITY_EDITOR
            //            DestroyImmediate(item.gameObject);
            //#else
            //            Destroy(item.gameObject);
            //#endif
            //        }

            var splineInst = GetComponent<SplineInstantiate>();
            splineInst.enabled = false;
            splineInst.Clear();
            ClearSplineInst();
            ClearSplineContainer();
        }

        private void ClearSplineContainer()
        {
            _splineCont = GetComponent<SplineContainer>();
            _splineCont.Spline.Clear();
        }

        private void EnableSplineInst(bool enable)
        {
            var splineInst = GetComponent<SplineInstantiate>();
            splineInst.enabled = enable;
        }

        private void ClearSplineInst()
        {
            var splineInst = GetComponent<SplineInstantiate>();
            splineInst.Clear();
        }
    }
}