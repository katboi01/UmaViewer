using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RootMotion.Demos
{

    // Custom navigator for Unity Navigation.
    [System.Serializable]
    public class Navigator
    {

        public enum State
        {
            Idle,
            Seeking,
            OnPath,
        }

        [Tooltip("Should this Navigator be actively seeking a path.")]
        public bool activeTargetSeeking;

        [Tooltip("Increase this value if the character starts running in a circle, not able to reach the corner because of a too large turning radius.")]
        public float cornerRadius = 0.5f;

        [Tooltip("Recalculate path if target position has moved by this distance from the position it was at when the path was originally calculated")]
        public float recalculateOnPathDistance = 1f;
        //public float recalculateBadTargetDistance = 1f;

        [Tooltip("Sample within this distance from sourcePosition.")]
        public float maxSampleDistance = 5f;

        [Tooltip("Interval of updating the path")]
        public float nextPathInterval = 3f;

        /// <summary>
        /// Get the move direction vector (normalized). If nowhere to go or path finished, will return Vector3.zero.
        /// </summary>
        public Vector3 normalizedDeltaPosition { get; private set; }

        /// <summary>
        /// Get the current state of this Navigator (Idle, Seeking, OnPath).
        /// </summary>
        public State state { get; private set; }

        private Transform transform;
        private int cornerIndex;
        private Vector3[] corners = new Vector3[0];
        private UnityEngine.AI.NavMeshPath path;
        private Vector3 lastTargetPosition;
        private bool initiated;
        private float nextPathTime;

        public void Initiate(Transform transform)
        {
            this.transform = transform;
            path = new UnityEngine.AI.NavMeshPath();
            initiated = true;
            cornerIndex = 0;
            corners = new Vector3[0];
            state = State.Idle;
            lastTargetPosition = new Vector3(Mathf.Infinity, Mathf.Infinity, Mathf.Infinity);
        }

        public void Update(Vector3 targetPosition)
        {
            if (!initiated)
            {
                Debug.LogError("Trying to update an uninitiated Navigator.");
                return;
            }

            switch (state)
            {
                // When seeking path
                case State.Seeking:
                    normalizedDeltaPosition = Vector3.zero;

                    if (path.status == UnityEngine.AI.NavMeshPathStatus.PathComplete)
                    {
                        corners = path.corners;
                        cornerIndex = 0;

                        if (corners.Length == 0)
                        {
                            Debug.LogWarning("Zero Corner Path", transform);

                            Stop();
                        }
                        else
                        {
                            state = State.OnPath;
                        }
                    }

                    if (path.status == UnityEngine.AI.NavMeshPathStatus.PathPartial)
                    {
                        Debug.LogWarning("Path Partial", transform);
                    }

                    if (path.status == UnityEngine.AI.NavMeshPathStatus.PathInvalid)
                    {
                        Debug.LogWarning("Path Invalid", transform);
                    }
                    break;

                // When already on path
                case State.OnPath:
                    if (activeTargetSeeking && Time.time > nextPathTime && HorDistance(targetPosition, lastTargetPosition) > recalculateOnPathDistance)
                    {
                        CalculatePath(targetPosition);
                        break;
                    }

                    if (cornerIndex < corners.Length)
                    {
                        Vector3 d = corners[cornerIndex] - transform.position;
                        d.y = 0f;
                        float mag = d.magnitude;

                        if (mag > 0f) normalizedDeltaPosition = (d / d.magnitude);
                        else normalizedDeltaPosition = Vector3.zero;

                        if (mag < cornerRadius)
                        {
                            cornerIndex++;

                            if (cornerIndex >= corners.Length) Stop();
                        }
                    }
                    break;

                // Not on path, not seeking
                case State.Idle:
                    if (activeTargetSeeking && Time.time > nextPathTime) CalculatePath(targetPosition);
                    break;
            }
        }

        private void CalculatePath(Vector3 targetPosition)
        {
            if (Find(targetPosition))
            {
                lastTargetPosition = targetPosition;
                state = State.Seeking;
            }
            else
            {
                Stop();
            }

            nextPathTime = Time.time + nextPathInterval;
        }

        private bool Find(Vector3 targetPosition)
        {
            if (HorDistance(transform.position, targetPosition) < cornerRadius * 2f) return false;
            //if (HorDistance(targetPosition, lastTargetPosition) < recalculateBadTargetDistance) return false;
            if (UnityEngine.AI.NavMesh.CalculatePath(transform.position, targetPosition, UnityEngine.AI.NavMesh.AllAreas, path))
            {
                return true;
            }
            else
            {
                UnityEngine.AI.NavMeshHit hit = new UnityEngine.AI.NavMeshHit();

                if (UnityEngine.AI.NavMesh.SamplePosition(targetPosition, out hit, maxSampleDistance, UnityEngine.AI.NavMesh.AllAreas))
                {
                    if (UnityEngine.AI.NavMesh.CalculatePath(transform.position, hit.position, UnityEngine.AI.NavMesh.AllAreas, path))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void Stop()
        {
            state = State.Idle;
            normalizedDeltaPosition = Vector3.zero;
        }

        private float HorDistance(Vector3 p1, Vector3 p2)
        {
            return Vector2.Distance(new Vector2(p1.x, p1.z), new Vector2(p2.x, p2.z));
        }

        public void Visualize()
        {
            if (state == State.Idle) Gizmos.color = Color.gray;
            if (state == State.Seeking) Gizmos.color = Color.red;
            if (state == State.OnPath) Gizmos.color = Color.green;

            if (corners.Length > 0 && state == State.OnPath && cornerIndex == 0)
            {
                Gizmos.DrawLine(transform.position, corners[0]);
            }

            for (int i = 0; i < corners.Length; i++)
            {
                Gizmos.DrawSphere(corners[i], 0.1f);
            }

            if (corners.Length > 1)
            {
                for (int i = 0; i < corners.Length - 1; i++)
                {
                    Gizmos.DrawLine(corners[i], corners[i + 1]);
                }
            }

            Gizmos.color = Color.white;
        }

    }
}
