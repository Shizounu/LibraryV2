using UnityEngine;


namespace Shizounu.Library.Utility
{

    static class Vector2Extensions
    {
        public static Vector2 Round(this Vector2 vector) => new(Mathf.Round(vector.x), Mathf.Round(vector.y)); 

        public static Vector2 Rotate(this Vector2 vector, float angle, Vector2 pivot = default(Vector2)) 
        {
            Vector2 rotated = Quaternion.Euler(new Vector3(0f, 0f, angle)) * (vector - pivot);
            return rotated + pivot;
        }
    }
}
