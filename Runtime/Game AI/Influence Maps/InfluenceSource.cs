using UnityEngine;

namespace Shizounu.Library.GameAI.InfluenceMaps
{
    /// <summary>
    /// Component that continuously adds influence to an influence map.
    /// </summary>
    public class InfluenceSource : MonoBehaviour
    {
        [SerializeField] private InfluenceMapManager mapManager;
        [SerializeField] private float influenceAmount = 1f;
        [SerializeField] private float influenceRadius = 5f;
        [SerializeField] private bool usePosition = true;
        [SerializeField] private string layerName = "";

        [Header("Update Settings")]
        [SerializeField] private bool updateEveryFrame = false;
        [SerializeField] private float updateInterval = 0.5f;
        
        private float timeSinceLastUpdate;

        private void Update()
        {
            if (mapManager == null)
                return;

            if (updateEveryFrame)
            {
                AddInfluenceToMap();
            }
            else
            {
                timeSinceLastUpdate += Time.deltaTime;
                if (timeSinceLastUpdate >= updateInterval)
                {
                    AddInfluenceToMap();
                    timeSinceLastUpdate = 0f;
                }
            }
        }

        private void AddInfluenceToMap()
        {
            Vector3 position = usePosition ? transform.position : Vector3.zero;
            
            if (string.IsNullOrEmpty(layerName))
            {
                mapManager.Map.AddInfluence(position, influenceAmount * Time.deltaTime, influenceRadius);
            }
            else
            {
                mapManager.Map.AddInfluenceToLayer(layerName, position, influenceAmount * Time.deltaTime, influenceRadius);
            }
        }

        public void SetInfluenceAmount(float amount)
        {
            influenceAmount = amount;
        }

        public void SetInfluenceRadius(float radius)
        {
            influenceRadius = radius;
        }
    }
}
