using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Shizounu.Library.Editor{
    public static class CustomGizmos
    {
        //Given to me by a friend
        public static void DrawClosedArc(Vector3 position, Vector3 up, Vector3 forward, float radius, float angle)
        {
            up = ((up == Vector3.zero) ? Vector3.up : up).normalized * radius;
            forward = forward.normalized * radius;
            Vector3 right = Vector3.Cross(up, forward).normalized * radius;

            Matrix4x4 matrix = new Matrix4x4();

            matrix[0] = right.x;
            matrix[1] = right.y;
            matrix[2] = right.z;

            matrix[4] = up.x;
            matrix[5] = up.y;
            matrix[6] = up.z;

            matrix[8] = forward.x;
            matrix[9] = forward.y;
            matrix[10] = forward.z;

            angle *= 0.5f;
            float angleStart = (90f - angle) * Mathf.Deg2Rad;

            Vector3 _lastPoint = position + matrix.MultiplyPoint3x4(new Vector3(Mathf.Cos(angleStart), 0, Mathf.Sin(angleStart)));
            Vector3 _nextPoint = Vector3.zero;

            Gizmos.DrawLine(position, _lastPoint);

            for (var i = angleStart; i <= (90f + angle) * Mathf.Deg2Rad; i += Mathf.Deg2Rad)
            {
                _nextPoint.x = Mathf.Cos(i);
                _nextPoint.z = Mathf.Sin(i);
                _nextPoint.y = 0;

                _nextPoint = position + matrix.MultiplyPoint3x4(_nextPoint);

                Gizmos.DrawLine(_lastPoint, _nextPoint);
                _lastPoint = _nextPoint;
            }

            Gizmos.DrawLine(position, _lastPoint);
        }
    }
}