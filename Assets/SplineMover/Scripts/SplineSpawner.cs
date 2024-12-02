using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Splines;

namespace MMZZ.Spline.Wip
{
    /// <summary>
    /// ** WIP **
    /// Works is in progress and still in try and error
    /// </summary>
    public class SplineSpawner : MonoBehaviour
    {
        [SerializeField] private SplineContainer _spline;
        [SerializeField] private BoxCollider _boxCol;
        [SerializeField] private Bounds _bounds;
        [SerializeField] private GameObject _objectToSpawn;
        [SerializeField] private Transform _spawnedContainer;

        public int rows = 10;             // Number of rows
        public int columns = 10;          // Number of columns


        [ContextMenu("Test")]
        public void Test()
        {
            _bounds = _spline.Spline.GetBounds();
            SpawnObjects(_bounds);
            //var spawned = Instantiate(_objectToSpawn, _spawnedContainer);
        }

        public void SpawnObjects(Bounds bounds)
        {
            var start = bounds.min;
            var end = bounds.max;

            SpawnObjectsAlongLine(start, end, 10, _objectToSpawn);
        }

        void SpawnObjectsAlongLine(Vector3 start, Vector3 end, int count, GameObject prefab)
        {
            // Calculate the step increment for spacing
            float step = 1f / (count - 1);

            // Loop through and spawn objects
            for (int i = 0; i < count; i++)
            {
                // Interpolate position along the line
                Vector3 spawnPosition = Vector3.Lerp(start, end, i * step);

                // Spawn the object
                var spawned = Instantiate(prefab, _spawnedContainer);
                spawned.transform.position = spawnPosition;
            }
        }

        [Header("For testing only")]
        [SerializeField] private GameObject _cube;

        [ContextMenu("SpawnPrefav")]
        public void SpawnPrefab()
        {
            _bounds = _spline.Spline.GetBounds();
            transform.position = _bounds.center + _spline.transform.position;
            transform.localScale = _bounds.size;
            //_boxCol.size = _bounds.size;
            //_boxCol.bounds.SetMinMax(_bounds.min, _bounds.max);

            var cubePos = new Vector2(_cube.transform.position.x, _cube.transform.position.z);
            var polygons = new List<Vector3>();
            var offset = _spline.transform.position;
            _spline.Spline.Knots.ToList().ForEach(k => polygons.Add(new Vector3(k.Position.x, 0, k.Position.z) + offset));
            polygons.Add(polygons[0]);

            bool isInsidePolygon = IsPointInsidePolygon(cubePos, polygons.ToArray());

            Debug.Log(isInsidePolygon ? "Plane is inside the spline area." : "Plane is outside the spline area.");
        }

        private bool IsPointInsidePolygon(Vector2 point, Vector3[] polygon)
        {
            int crossings = 0;

            for (int i = 0; i < polygon.Length; i++)
            {
                Vector2 v1 = new Vector2(polygon[i].x, polygon[i].z);
                Vector2 v2 = new Vector2(polygon[(i + 1) % polygon.Length].x, polygon[(i + 1) % polygon.Length].z);

                // Check if the line segment crosses the ray to the right of the point
                if (((v1.y > point.y) != (v2.y > point.y)) &&
                    (point.x < (v2.x - v1.x) * (point.y - v1.y) / (v2.y - v1.y) + v1.x))
                {
                    crossings++;
                }
            }

            // Odd crossings mean inside
            return (crossings % 2) == 1;
        }

        //private bool IsPointInsidePolygon(Vector2 point, Vector3[] polygon)
        //{
        //    int crossings = 0;

        //    for (int i = 0; i < polygon.Length; i++)
        //    {
        //        Vector2 v1 = new Vector2(polygon[i].x, polygon[i].z);
        //        Vector2 v2 = new Vector2(polygon[(i + 1) % polygon.Length].x, polygon[(i + 1) % polygon.Length].z);

        //        // Check if point is on the line segment
        //        if (IsPointOnLineSegment(point, v1, v2))
        //            return true;

        //        // Check if the line segment crosses the ray to the right of the point
        //        if (((v1.y > point.y) != (v2.y > point.y)) &&
        //            (point.x < (v2.x - v1.x) * (point.y - v1.y) / (v2.y - v1.y) + v1.x))
        //        {
        //            crossings++;
        //        }
        //    }

        //    // Odd crossings mean inside
        //    return (crossings % 2) == 1;
        //}

        //private bool IsPointOnLineSegment(Vector2 point, Vector2 v1, Vector2 v2)
        //{
        //    float cross = (point.y - v1.y) * (v2.x - v1.x) - (point.x - v1.x) * (v2.y - v1.y);
        //    if (Mathf.Abs(cross) > Mathf.Epsilon) return false;

        //    float dot = (point.x - v1.x) * (v2.x - v1.x) + (point.y - v1.y) * (v2.y - v1.y);
        //    if (dot < 0) return false;

        //    float squaredLength = (v2.x - v1.x) * (v2.x - v1.x) + (v2.y - v1.y) * (v2.y - v1.y);
        //    return dot <= squaredLength;
        //}
    }
}
