using UnityEngine;

namespace Shizounu.Library.GenerationAlgorithms.WaveFunctionCollapse
{
    public sealed class WfcGenerator3DBehaviour : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private WfcTileSetAsset tileSet;
        [SerializeField, Min(1)] private int width = 10;
        [SerializeField, Min(1)] private int height = 10;
        [SerializeField, Min(1)] private int depth = 10;
        [SerializeField] private bool useRandomSeed = true;
        [SerializeField] private int seed;
        [SerializeField] private bool generateOnStart = true;

        [Header("Placement")]
        [SerializeField] private bool instantiatePrefabs = true;
        [SerializeField] private float cellSize = 1f;
        [SerializeField] private Vector3 origin = Vector3.zero;
        [SerializeField] private Transform parent;
        [SerializeField] private bool clearBeforeGenerate = true;

        [Header("Debug")]
        [SerializeField] private bool drawGizmos = true;

        private int[,,] _lastIndices;
        private WfcTileSet<UnityEngine.Object> _runtimeTileSet;

        private void Start()
        {
            if (generateOnStart)
                Generate();
        }

        [ContextMenu("Generate")]
        public void Generate()
        {
            if (tileSet == null)
            {
                Debug.LogWarning("WFC 3D generator is missing a tile set asset", this);
                return;
            }

            if (clearBeforeGenerate && instantiatePrefabs)
                ClearChildren();

            try
            {
                _runtimeTileSet = tileSet.BuildTileSet();
                if (_runtimeTileSet.DirectionCount != 6)
                {
                    Debug.LogError("TileSet must have 6 directions for 3D generation", this);
                    return;
                }
                Debug.Log($"Built 3D tile set with {_runtimeTileSet.Count} tiles", this);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to build tile set: {ex.Message}", this);
                return;
            }

            int? appliedSeed = useRandomSeed ? (int?)null : seed;
            var solver = new WfcSolver3D<UnityEngine.Object>(width, height, depth, _runtimeTileSet, appliedSeed);

            WfcResult result = solver.Run();
            if (result != WfcResult.Success)
            {
                _lastIndices = null;
                Debug.LogWarning($"WFC 3D generation failed: {result}", this);
                return;
            }

            if (!solver.TryGetCollapsedIndices(out _lastIndices))
            {
                Debug.LogWarning("WFC 3D generation did not fully collapse", this);
                return;
            }

            Debug.Log($"WFC 3D generation succeeded, grid: {width}x{height}x{depth}", this);
            if (instantiatePrefabs)
                SpawnPrefabs();
        }

        private void SpawnPrefabs()
        {
            if (_lastIndices == null || _runtimeTileSet == null)
                return;

            Transform root = parent != null ? parent : transform;
            int instantiatedCount = 0;

            for (int z = 0; z < depth; z++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int tileIndex = _lastIndices[x, y, z];
                        UnityEngine.Object tile = _runtimeTileSet.Tiles[tileIndex];

                        if (tile == null)
                            continue;

                        Vector3 position = origin + new Vector3(x * cellSize, y * cellSize, z * cellSize);

                        if (tile is GameObject prefab)
                        {
                            Instantiate(prefab, position, Quaternion.identity, root);
                            instantiatedCount++;
                        }
                        else if (tile is Component component)
                        {
                            Instantiate(component.gameObject, position, Quaternion.identity, root);
                            instantiatedCount++;
                        }
                        else
                        {
                            Debug.LogWarning($"Tile at ({x}, {y}, {z}) is not a GameObject or Component: {tile.GetType().Name}", this);
                        }
                    }
                }
            }

            Debug.Log($"WFC 3D generation complete: instantiated {instantiatedCount} prefabs", this);
        }

        private void ClearChildren()
        {
            Transform root = parent != null ? parent : transform;
            for (int i = root.childCount - 1; i >= 0; i--)
            {
                Transform child = root.GetChild(i);
                if (Application.isPlaying)
                    Destroy(child.gameObject);
                else
                    DestroyImmediate(child.gameObject);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (!drawGizmos || _lastIndices == null || _runtimeTileSet == null)
                return;

            for (int z = 0; z < depth; z++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int tileIndex = _lastIndices[x, y, z];
                        float hue = (tileIndex * 0.6180339f) % 1f;
                        Gizmos.color = Color.HSVToRGB(hue, 0.6f, 0.9f);
                        Vector3 position = origin + new Vector3(x * cellSize, y * cellSize, z * cellSize);
                        Gizmos.DrawCube(position, Vector3.one * (cellSize * 0.9f));
                    }
                }
            }
        }
    }
}
