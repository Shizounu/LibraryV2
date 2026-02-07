using UnityEngine;

namespace Shizounu.Library.GameAI.InfluenceMaps
{
    /// <summary>
    /// MonoBehaviour wrapper for InfluenceMap with automatic updates.
    /// </summary>
    public class InfluenceMapManager : MonoBehaviour
    {
        [Header("Grid Settings")]
        [SerializeField] private int width = 50;
        [SerializeField] private int height = 50;
        [SerializeField] private float cellSize = 1f;
        [SerializeField] private bool centerOnTransform = true;
        [SerializeField] private Vector3 origin = Vector3.zero;

        [Header("Update Settings")]
        [SerializeField] private bool autoDecay = true;
        [SerializeField] private float decayRate = 0.1f;
        [SerializeField] private bool autoPropagate = false;
        [SerializeField] private float propagationAmount = 0.1f;
        [SerializeField] private bool includeDiagonals = false;

        [Header("Visualization")]
        [SerializeField] private bool drawGizmos = true;
        [SerializeField] private float gizmoHeight = 0.1f;
        [SerializeField] private Color positiveColor = Color.red;
        [SerializeField] private Color negativeColor = Color.blue;
        [SerializeField] private Color neutralColor = Color.gray;
        [SerializeField] private float maxVisualizationValue = 10f;

        public InfluenceMap Map;


        private void Awake()
        {
            Map = new InfluenceMap(width, height, cellSize, GetOrigin());
        }

        private Vector3 GetOrigin()
        {
            if (!centerOnTransform)
                return origin;

            float halfWidth = (width * cellSize) * 0.5f;
            float halfHeight = (height * cellSize) * 0.5f;
            return transform.position - new Vector3(halfWidth, 0f, halfHeight);
        }

        private void Update()
        {
            if (autoDecay)
            {
                Map.ApplyDecay(decayRate, Time.deltaTime);
            }

            if (autoPropagate)
            {
                Map.Propagate(propagationAmount, includeDiagonals);
            }
        }

        public void AddInfluence(Vector3 position, float amount, float radius = 0)
        {
            Map.AddInfluence(position, amount, radius);
        }

        public float GetInfluence(Vector3 position)
        {
            return Map.GetInfluence(position);
        }
        
        public void Clear()
        {
            Map.Clear();
        }

        private void OnDrawGizmos()
        {
            if (!drawGizmos || Map == null)
                return;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    float influence = Map.GetInfluenceAtCell(x, y);
                    
                    if (Mathf.Abs(influence) < 0.01f)
                        continue;

                    Vector3 worldPos = Map.GridToWorld(x, y);
                    worldPos.y = gizmoHeight;

                    Color color;
                    if (influence > 0)
                    {
                        float t = Mathf.Clamp01(influence / maxVisualizationValue);
                        color = Color.Lerp(neutralColor, positiveColor, t);
                    }
                    else
                    {
                        float t = Mathf.Clamp01(-influence / maxVisualizationValue);
                        color = Color.Lerp(neutralColor, negativeColor, t);
                    }

                    Gizmos.color = color;
                    Gizmos.DrawCube(worldPos, new Vector3(cellSize * 0.9f, 0.1f, cellSize * 0.9f));
                }
            }
        }
    }
}
