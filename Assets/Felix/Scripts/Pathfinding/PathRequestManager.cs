using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

namespace Pathfinding
{
    public class PathRequestManager : MonoBehaviour
    {
        private static PathRequestManager instance;
        private Pathfinding pathfinding;

        private Queue<PathResult> results = new Queue<PathResult>();

        private Thread t;

        private void Awake()
        {
            instance = this;
            pathfinding = GetComponent<Pathfinding>();
        }

        private void Start()
        {
            ThreadStart threadStart = delegate
            {
                pathfinding.IteratePath();
            };
            t = new Thread(threadStart);
            t.Start();
        }

        private void Update()
        {
            if (results.Count > 0)
            {
                int items = results.Count;
                lock (results)
                {
                    for (int i = 0; i < items; i++)
                    {
                        PathResult result = results.Dequeue();
                        result.callback(result.path, result.success);
                    }
                }
            }
        }

        public static void RequestPath(PathRequest _request, bool _isAStar)
        {
            instance.pathfinding.FindPath(_request, instance.FinishedProcessingPath, _isAStar);
        }

        public void FinishedProcessingPath(PathResult _pathResult)
        {
            lock (results)
            {
                results.Enqueue(_pathResult);
            }
        }

        private void OnApplicationQuit()
        {
            t.Abort();
        }
    }
    
    public struct PathResult
    {
        public Vector3[] path;
        public bool success;
        public Action<Vector3[], bool> callback;

        public PathResult(Vector3[] _path, bool _success, Action<Vector3[], bool> _callback)
        {
            path = _path;
            success = _success;
            callback = _callback;
        }
    }
    
    public struct PathRequest
    {
        public Vector3 startPoint;
        public Vector3 endPoint;
        public Action<Vector3[], bool> callback;

        public PathRequest(Vector3 _startPoint, Vector3 _endPoint, Action<Vector3[], bool> _callback)
        {
            startPoint = _startPoint;
            endPoint = _endPoint;
            callback = _callback;
        }
    }
}
