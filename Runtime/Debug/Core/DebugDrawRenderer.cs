using UnityEngine;

namespace Shizounu.Library.Utility
{
    /// <summary>
    /// Internal renderer component for DebugDraw system.
    /// </summary>
    internal class DebugDrawRenderer : MonoBehaviour
    {
        private Material _material;
        private Mesh _sphereMesh;
        private Mesh _boxMesh;

        private void OnEnable()
        {
            if (_material == null)
                _material = new Material(Shader.Find("Unlit/Color"));

            if (_sphereMesh == null)
                _sphereMesh = CreateSphereMesh();
            if (_boxMesh == null)
                _boxMesh = CreateBoxMesh();
        }

        private Mesh CreateSphereMesh(int stacks = 6, int slices = 12)
        {
            var mesh = new Mesh { name = "DebugSphere" };
            var vertices = new System.Collections.Generic.List<Vector3>();
            var indices = new System.Collections.Generic.List<int>();

            for (int i = 0; i <= stacks; i++)
            {
                var phi = Mathf.PI * i / stacks;
                for (int j = 0; j <= slices; j++)
                {
                    var theta = 2 * Mathf.PI * j / slices;
                    vertices.Add(new Vector3(
                        Mathf.Sin(phi) * Mathf.Cos(theta),
                        Mathf.Cos(phi),
                        Mathf.Sin(phi) * Mathf.Sin(theta)
                    ) * 0.5f);
                }
            }

            for (int i = 0; i < stacks; i++)
            {
                for (int j = 0; j < slices; j++)
                {
                    int a = i * (slices + 1) + j;
                    int b = a + slices + 1;
                    indices.Add(a);
                    indices.Add(b);
                    indices.Add(a + 1);
                    indices.Add(b);
                    indices.Add(b + 1);
                    indices.Add(a + 1);
                }
            }

            mesh.SetVertices(vertices);
            mesh.SetTriangles(indices, 0);
            mesh.RecalculateNormals();
            return mesh;
        }

        private Mesh CreateBoxMesh()
        {
            var mesh = new Mesh { name = "DebugBox" };
            mesh.vertices = new Vector3[]
            {
                new Vector3(-0.5f, -0.5f, -0.5f),
                new Vector3(0.5f, -0.5f, -0.5f),
                new Vector3(0.5f, 0.5f, -0.5f),
                new Vector3(-0.5f, 0.5f, -0.5f),
                new Vector3(-0.5f, -0.5f, 0.5f),
                new Vector3(0.5f, -0.5f, 0.5f),
                new Vector3(0.5f, 0.5f, 0.5f),
                new Vector3(-0.5f, 0.5f, 0.5f)
            };
            mesh.triangles = new int[]
            {
                0, 1, 2, 0, 2, 3, 4, 6, 5, 4, 7, 6,
                0, 4, 5, 0, 5, 1, 2, 6, 7, 2, 7, 3,
                0, 3, 7, 0, 7, 4, 1, 5, 6, 1, 6, 2
            };
            mesh.RecalculateNormals();
            return mesh;
        }

        private void Update()
        {
            DebugDraw.RemoveExpiredRequests();
            DebugDraw.RenderAllRequests();
        }

        public void DrawLine(Vector3 start, Vector3 end, Color color)
        {
            Debug.DrawLine(start, end, color);
        }

        public void DrawSphere(Vector3 position, float radius, Color color)
        {
            if (_material == null) _material = new Material(Shader.Find("Unlit/Color"));
            var matrix = Matrix4x4.TRS(position, Quaternion.identity, Vector3.one * radius * 2f);
            _material.color = color;
            Graphics.DrawMesh(_sphereMesh, matrix, _material, 0);
        }

        public void DrawBox(Vector3 position, Vector3 size, Color color)
        {
            if (_material == null) _material = new Material(Shader.Find("Unlit/Color"));
            var matrix = Matrix4x4.TRS(position, Quaternion.identity, size);
            _material.color = color;
            Graphics.DrawMesh(_boxMesh, matrix, _material, 0);
        }

        private void OnDestroy()
        {
            if (_material != null)
                Destroy(_material);
            if (_sphereMesh != null)
                Destroy(_sphereMesh);
            if (_boxMesh != null)
                Destroy(_boxMesh);
        }
    }
}
