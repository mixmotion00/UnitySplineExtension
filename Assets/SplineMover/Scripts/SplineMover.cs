using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;
using System.Linq;
using Unity.VisualScripting;

namespace MMZZ.Spline
{
    public enum MouseState 
    {
        OnSelect,
        OnBeginDrag,
        OnDrag,
        OnDragEnded
    }

    public class MouseInputEx
    {
        public MouseState State { get; private set; }
        public float ClickSensitivity { get; set; } // time need for mouse to start dragging

        private bool _isStart;
        private bool _isDrag;
        private float _dragTime;

        public MouseInputEx(float clickSensitivity = 0.2f)
        {
            _isStart = false;
            _isDrag = false;
            _dragTime = 0f;
            ClickSensitivity = clickSensitivity;
        }

        public void Tick(float deltaTime)
        {
            if (Input.GetMouseButtonDown(0) && _isStart == false)
            {
                _dragTime = 0f;
                _isDrag = false;
                _isStart = true;

                State = MouseState.OnSelect;
            }
            if (Input.GetMouseButtonUp(0) || !Input.GetMouseButton(0))
            {
                _isStart = false;
                _isDrag = false;

                State = MouseState.OnDragEnded;
            }

            if (Input.GetMouseButton(0) && _isStart)
            {
                _dragTime += Time.deltaTime;

                if ((_dragTime > ClickSensitivity))
                {
                    _isDrag = true;
                    State = MouseState.OnDrag;
                }
                else 
                {
                    _isDrag = false;
                    State = MouseState.OnBeginDrag;
                }
            }
        }
    }

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

        private MouseInputEx _mouseInputEx;
        private SplinePoint _draggedSplinePt;
        private Camera _mainCam;

        private void Start()
        {
            _mainCam = Camera.main;
            _mouseInputEx = new MouseInputEx(0.1f);
        }

        private void Update()
        {
            _mouseInputEx.Tick(Time.deltaTime);

            if(_mouseInputEx.State == MouseState.OnBeginDrag) 
            {
                // check any spline point is selected
                _draggedSplinePt = DetectSplinePoint();
            }
            else if(_mouseInputEx.State == MouseState.OnDrag) 
            {
                // if dragged spline exist
                if (_draggedSplinePt != null) 
                {
                    // Create a ray from the mouse position
                    Ray ray = _mainCam.ScreenPointToRay(Input.mousePosition);

                    // Raycast to detect the clicked position
                    if (Physics.Raycast(ray, out RaycastHit hitInfo))
                    {
                        // Get the clicked position and maintain sphere's Y position
                        Vector3 targetPosition = hitInfo.point;
                        targetPosition.y = _draggedSplinePt.transform.position.y;

                        // Move the sphere to the target position
                        _draggedSplinePt.transform.position = targetPosition;
                    }
                }
            }
        }

        private SplinePoint DetectSplinePoint()
        {
            // Cast a ray from the mouse position
            Ray ray = _mainCam.ScreenPointToRay(Input.mousePosition);
            RaycastHit[] hits;

            // Perform the raycast
            hits = Physics.RaycastAll(ray);

            var splPoints = hits.Where(r => r.collider.gameObject.GetComponent<SplinePoint>() != null).ToList();

            // Check if the ray hits any objects
            if (splPoints.Count() > 0)
            {
                return splPoints.First().collider.gameObject.GetComponent<SplinePoint>();

                //// Iterate through all the hits
                //foreach (RaycastHit hit in hits)
                //{
                //    // Print the name of the hit object
                //    Debug.Log("Hit object name: " + hit.collider.gameObject.name);
                //}
            }

            return null;
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