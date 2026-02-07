using UnityEngine;
using System.Collections.Generic;

namespace Shizounu.Library.Utility
{
    /// <summary>
    /// Persistent debug drawing system that works in both editor and builds.
    /// Provides visualization for lines, spheres, boxes, and arrows with category-based coloring.
    /// </summary>
    public static class DebugDraw
    {
        public enum Category
        {
            Default,
            AI,
            Physics,
            Pathfinding,
            Events,
            UI,
            Network,
            Custom
        }

        private class DrawRequest
        {
            public float CreationTime;
            public float Duration;
            public Color Color;
            public Category Category;
            public DrawType Type;
            public Vector3 Position;
            public Vector3 Position2;
            public Vector3 Scale;
            public string Text;
            public int FontSize;

            public bool IsExpired(float currentTime) => currentTime - CreationTime > Duration;
        }

        private enum DrawType
        {
            Line,
            Sphere,
            Box,
            Arrow,
            Text
        }

        private static List<DrawRequest> _drawRequests = new();
        private static DebugDrawRenderer _renderer;
        private static bool _isInitialized = false;
        private static Dictionary<Category, Color> _categoryColors = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            if (_isInitialized) return;

            var rendererGO = new GameObject("_DebugDrawRenderer");
            Object.DontDestroyOnLoad(rendererGO);
            _renderer = rendererGO.AddComponent<DebugDrawRenderer>();

            InitializeDefaultCategoryColors();
            _isInitialized = true;
        }

        private static void InitializeDefaultCategoryColors()
        {
            _categoryColors = new Dictionary<Category, Color>
            {
                { Category.Default, Color.white },
                { Category.AI, Color.yellow },
                { Category.Physics, Color.cyan },
                { Category.Pathfinding, Color.green },
                { Category.Events, Color.magenta },
                { Category.UI, Color.blue },
                { Category.Network, Color.red },
                { Category.Custom, Color.gray }
            };
        }

        public static void SetCategoryColor(Category category, Color color)
        {
            _categoryColors[category] = color;
        }

        public static void DrawLine(Vector3 start, Vector3 end, Color? color = null, float duration = 0f, Category category = Category.Default)
        {
            if (!_isInitialized) Initialize();

            var request = new DrawRequest
            {
                CreationTime = Time.time,
                Duration = duration,
                Color = color ?? GetCategoryColor(category),
                Category = category,
                Type = DrawType.Line,
                Position = start,
                Position2 = end
            };

            _drawRequests.Add(request);
        }

        public static void DrawSphere(Vector3 position, float radius = 0.1f, Color? color = null, float duration = 0f, Category category = Category.Default)
        {
            if (!_isInitialized) Initialize();

            var request = new DrawRequest
            {
                CreationTime = Time.time,
                Duration = duration,
                Color = color ?? GetCategoryColor(category),
                Category = category,
                Type = DrawType.Sphere,
                Position = position,
                Scale = new Vector3(radius, radius, radius)
            };

            _drawRequests.Add(request);
        }

        public static void DrawBox(Vector3 position, Vector3 size, Color? color = null, float duration = 0f, Category category = Category.Default)
        {
            if (!_isInitialized) Initialize();

            var request = new DrawRequest
            {
                CreationTime = Time.time,
                Duration = duration,
                Color = color ?? GetCategoryColor(category),
                Category = category,
                Type = DrawType.Box,
                Position = position,
                Scale = size
            };

            _drawRequests.Add(request);
        }

        public static void DrawArrow(Vector3 position, Vector3 direction, float length = 1f, Color? color = null, float duration = 0f, Category category = Category.Default)
        {
            if (!_isInitialized) Initialize();

            var normalizedDir = direction.normalized;
            var endPosition = position + normalizedDir * length;
            var arrowHeadSize = length * 0.2f;

            // Draw line
            var request = new DrawRequest
            {
                CreationTime = Time.time,
                Duration = duration,
                Color = color ?? GetCategoryColor(category),
                Category = category,
                Type = DrawType.Line,
                Position = position,
                Position2 = endPosition
            };
            _drawRequests.Add(request);

            // Draw arrow head
            var right = Vector3.Cross(normalizedDir, Vector3.up).normalized;
            if (right.sqrMagnitude < 0.01f)
                right = Vector3.Cross(normalizedDir, Vector3.right).normalized;

            var headStart1 = endPosition - normalizedDir * arrowHeadSize + right * arrowHeadSize * 0.5f;
            var headStart2 = endPosition - normalizedDir * arrowHeadSize - right * arrowHeadSize * 0.5f;

            var request1 = new DrawRequest
            {
                CreationTime = Time.time,
                Duration = duration,
                Color = color ?? GetCategoryColor(category),
                Category = category,
                Type = DrawType.Line,
                Position = headStart1,
                Position2 = endPosition
            };
            _drawRequests.Add(request1);

            var request2 = new DrawRequest
            {
                CreationTime = Time.time,
                Duration = duration,
                Color = color ?? GetCategoryColor(category),
                Category = category,
                Type = DrawType.Line,
                Position = headStart2,
                Position2 = endPosition
            };
            _drawRequests.Add(request2);
        }

        public static void DrawText(Vector3 position, string text, Color? color = null, float duration = 0f, int fontSize = 20, Category category = Category.Default)
        {
            if (!_isInitialized) Initialize();

            var request = new DrawRequest
            {
                CreationTime = Time.time,
                Duration = duration,
                Color = color ?? GetCategoryColor(category),
                Category = category,
                Type = DrawType.Text,
                Position = position,
                Text = text,
                FontSize = fontSize
            };

            _drawRequests.Add(request);
        }

        public static void Clear()
        {
            _drawRequests.Clear();
        }

        public static void ClearCategory(Category category)
        {
            _drawRequests.RemoveAll(r => r.Category == category);
        }

        public static int GetActiveDrawCount() => _drawRequests.Count;

        private static Color GetCategoryColor(Category category)
        {
            return _categoryColors.TryGetValue(category, out var color) ? color : Color.white;
        }

        internal static void RemoveExpiredRequests()
        {
            _drawRequests.RemoveAll(r => r.IsExpired(Time.time));
        }

        internal static void RenderAllRequests()
        {
            if (_renderer == null) return;

            foreach (var request in _drawRequests)
            {
                switch (request.Type)
                {
                    case DrawType.Line:
                        _renderer.DrawLine(request.Position, request.Position2, request.Color);
                        break;
                    case DrawType.Sphere:
                        _renderer.DrawSphere(request.Position, request.Scale.x, request.Color);
                        break;
                    case DrawType.Box:
                        _renderer.DrawBox(request.Position, request.Scale, request.Color);
                        break;
                }
            }
        }
    }
}
