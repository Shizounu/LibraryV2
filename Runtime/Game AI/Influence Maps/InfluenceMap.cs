using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shizounu.Library.GameAI.InfluenceMaps
{
    /// <summary>
    /// Grid-based influence map for spatial AI decision making.
    /// Supports multiple influence layers, propagation, and decay.
    /// </summary>
    public class InfluenceMap
    {
        private readonly int _width;
        private readonly int _height;
        private readonly float _cellSize;
        private readonly Vector3 _origin;
        
        private float[,] _influenceGrid;
        private bool[,] _blocked;
        private Dictionary<string, float[,]> _layers = new Dictionary<string, float[,]>();
        
        public int Width => _width;
        public int Height => _height;
        public float CellSize => _cellSize;
        public Vector3 Origin => _origin;

        /// <summary>
        /// Create a new influence map.
        /// </summary>
        /// <param name="width">Grid width</param>
        /// <param name="height">Grid height</param>
        /// <param name="cellSize">Size of each cell in world units</param>
        /// <param name="origin">World position of grid origin (bottom-left)</param>
        public InfluenceMap(int width, int height, float cellSize, Vector3 origin)
        {
            _width = width;
            _height = height;
            _cellSize = cellSize;
            _origin = origin;
            
            _influenceGrid = new float[width, height];
            _blocked = new bool[width, height];
        }

        /// <summary>
        /// Add influence at a world position.
        /// </summary>
        public void AddInfluence(Vector3 worldPosition, float amount, float radius = 0)
        {
            if (!WorldToGrid(worldPosition, out int x, out int y))
                return;

            if (radius <= 0)
            {
                AddInfluenceAtCell(x, y, amount);
            }
            else
            {
                AddInfluenceRadial(x, y, amount, radius);
            }
        }

        /// <summary>
        /// Set influence at a specific cell.
        /// </summary>
        public void SetInfluenceAtCell(int x, int y, float value)
        {
            if (IsValidCell(x, y) && !IsBlocked(x, y))
            {
                _influenceGrid[x, y] = value;
            }
        }

        /// <summary>
        /// Add influence at a specific cell.
        /// </summary>
        public void AddInfluenceAtCell(int x, int y, float amount)
        {
            if (IsValidCell(x, y) && !IsBlocked(x, y))
            {
                _influenceGrid[x, y] += amount;
            }
        }

        /// <summary>
        /// Add influence in a radius around a cell with falloff.
        /// </summary>
        public void AddInfluenceRadial(int centerX, int centerY, float amount, float radius)
        {
            int radiusCells = Mathf.CeilToInt(radius / _cellSize);
            
            for (int x = centerX - radiusCells; x <= centerX + radiusCells; x++)
            {
                for (int y = centerY - radiusCells; y <= centerY + radiusCells; y++)
                {
                    if (!IsValidCell(x, y) || IsBlocked(x, y))
                        continue;

                    float distance = Mathf.Sqrt((x - centerX) * (x - centerX) + (y - centerY) * (y - centerY)) * _cellSize;
                    
                    if (distance <= radius)
                    {
                        float falloff = 1f - (distance / radius);
                        _influenceGrid[x, y] += amount * falloff;
                    }
                }
            }
        }

        /// <summary>
        /// Get influence value at a world position.
        /// </summary>
        public float GetInfluence(Vector3 worldPosition)
        {
            if (!WorldToGrid(worldPosition, out int x, out int y))
                return 0f;
            
            return GetInfluenceAtCell(x, y);
        }

        /// <summary>
        /// Get influence value at a specific cell.
        /// </summary>
        public float GetInfluenceAtCell(int x, int y)
        {
            return IsValidCell(x, y) ? _influenceGrid[x, y] : 0f;
        }

        /// <summary>
        /// Apply decay to all influence values.
        /// </summary>
        public void ApplyDecay(float decayRate, float deltaTime)
        {
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    _influenceGrid[x, y] *= Mathf.Pow(1f - decayRate, deltaTime);
                    
                    // Clamp to zero if very small
                    if (Mathf.Abs(_influenceGrid[x, y]) < 0.001f)
                        _influenceGrid[x, y] = 0f;
                }
            }
        }

        /// <summary>
        /// Propagate influence to neighboring cells.
        /// </summary>
        public void Propagate(float propagationAmount, bool includeDiagonals = false)
        {
            float[,] newGrid = new float[_width, _height];
            
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    if (IsBlocked(x, y))
                    {
                        newGrid[x, y] = 0f;
                        continue;
                    }

                    newGrid[x, y] = _influenceGrid[x, y];
                    
                    // Get average of neighbors
                    float neighborSum = 0f;
                    int neighborCount = 0;
                    
                    // Cardinal directions
                    AddNeighborInfluence(x - 1, y, ref neighborSum, ref neighborCount);
                    AddNeighborInfluence(x + 1, y, ref neighborSum, ref neighborCount);
                    AddNeighborInfluence(x, y - 1, ref neighborSum, ref neighborCount);
                    AddNeighborInfluence(x, y + 1, ref neighborSum, ref neighborCount);
                    
                    if (includeDiagonals)
                    {
                        AddNeighborInfluence(x - 1, y - 1, ref neighborSum, ref neighborCount);
                        AddNeighborInfluence(x - 1, y + 1, ref neighborSum, ref neighborCount);
                        AddNeighborInfluence(x + 1, y - 1, ref neighborSum, ref neighborCount);
                        AddNeighborInfluence(x + 1, y + 1, ref neighborSum, ref neighborCount);
                    }
                    
                    if (neighborCount > 0)
                    {
                        float neighborAverage = neighborSum / neighborCount;
                        newGrid[x, y] += (neighborAverage - _influenceGrid[x, y]) * propagationAmount;
                    }
                }
            }
            
            _influenceGrid = newGrid;
        }

        private void AddNeighborInfluence(int x, int y, ref float sum, ref int count)
        {
            if (IsValidCell(x, y) && !IsBlocked(x, y))
            {
                sum += _influenceGrid[x, y];
                count++;
            }
        }

        /// <summary>
        /// Clear all influence values.
        /// </summary>
        public void Clear()
        {
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    _influenceGrid[x, y] = 0f;
                }
            }
        }

        /// <summary>
        /// Find the position with the highest influence.
        /// </summary>
        public Vector3 FindMaxInfluence()
        {
            float maxInfluence = float.MinValue;
            int maxX = 0, maxY = 0;
            
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    if (_influenceGrid[x, y] > maxInfluence)
                    {
                        maxInfluence = _influenceGrid[x, y];
                        maxX = x;
                        maxY = y;
                    }
                }
            }
            
            return GridToWorld(maxX, maxY);
        }

        /// <summary>
        /// Find the position with the lowest influence.
        /// </summary>
        public Vector3 FindMinInfluence()
        {
            float minInfluence = float.MaxValue;
            int minX = 0, minY = 0;
            
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    if (_influenceGrid[x, y] < minInfluence)
                    {
                        minInfluence = _influenceGrid[x, y];
                        minX = x;
                        minY = y;
                    }
                }
            }
            
            return GridToWorld(minX, minY);
        }

        /// <summary>
        /// Convert world position to grid coordinates.
        /// </summary>
        public bool WorldToGrid(Vector3 worldPosition, out int x, out int y)
        {
            Vector3 localPos = worldPosition - _origin;
            x = Mathf.FloorToInt(localPos.x / _cellSize);
            y = Mathf.FloorToInt(localPos.z / _cellSize);
            
            return IsValidCell(x, y);
        }

        /// <summary>
        /// Convert grid coordinates to world position (center of cell).
        /// </summary>
        public Vector3 GridToWorld(int x, int y)
        {
            return _origin + new Vector3(
                (x + 0.5f) * _cellSize,
                0f,
                (y + 0.5f) * _cellSize
            );
        }

        /// <summary>
        /// Check if grid coordinates are valid.
        /// </summary>
        public bool IsValidCell(int x, int y)
        {
            return x >= 0 && x < _width && y >= 0 && y < _height;
        }

        /// <summary>
        /// Check if a grid cell is blocked from influence.
        /// </summary>
        public bool IsBlocked(int x, int y)
        {
            return IsValidCell(x, y) && _blocked[x, y];
        }

        /// <summary>
        /// Block or unblock a specific cell.
        /// </summary>
        public void SetBlockedAtCell(int x, int y, bool isBlocked = true)
        {
            if (!IsValidCell(x, y))
                return;

            _blocked[x, y] = isBlocked;
            if (isBlocked)
                _influenceGrid[x, y] = 0f;
        }

        /// <summary>
        /// Block or unblock a cell at a world position.
        /// </summary>
        public void SetBlocked(Vector3 worldPosition, bool isBlocked = true)
        {
            if (WorldToGrid(worldPosition, out int x, out int y))
            {
                SetBlockedAtCell(x, y, isBlocked);
            }
        }

        /// <summary>
        /// Block or unblock cells covered by a bounds volume.
        /// </summary>
        public void SetBlockedBounds(Bounds bounds, bool isBlocked = true)
        {
            int minX = WorldToGridIndex(bounds.min.x, _origin.x);
            int maxX = WorldToGridIndex(bounds.max.x, _origin.x);
            int minY = WorldToGridIndex(bounds.min.z, _origin.z);
            int maxY = WorldToGridIndex(bounds.max.z, _origin.z);

            minX = Mathf.Clamp(minX, 0, _width - 1);
            maxX = Mathf.Clamp(maxX, 0, _width - 1);
            minY = Mathf.Clamp(minY, 0, _height - 1);
            maxY = Mathf.Clamp(maxY, 0, _height - 1);

            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    SetBlockedAtCell(x, y, isBlocked);
                }
            }
        }

        /// <summary>
        /// Clear all blocked cells.
        /// </summary>
        public void ClearBlocked()
        {
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    _blocked[x, y] = false;
                }
            }
        }

        private int WorldToGridIndex(float worldCoord, float originCoord)
        {
            return Mathf.FloorToInt((worldCoord - originCoord) / _cellSize);
        }

        #region Layer Management
        
        /// <summary>
        /// Create a named influence layer.
        /// </summary>
        public void CreateLayer(string layerName)
        {
            if (!_layers.ContainsKey(layerName))
            {
                _layers[layerName] = new float[_width, _height];
            }
        }

        /// <summary>
        /// Add influence to a specific layer.
        /// </summary>
        public void AddInfluenceToLayer(string layerName, Vector3 worldPosition, float amount, float radius = 0)
        {
            if (!_layers.ContainsKey(layerName))
                CreateLayer(layerName);

            if (!WorldToGrid(worldPosition, out int x, out int y))
                return;

            if (radius <= 0)
            {
                if (!IsBlocked(x, y))
                    _layers[layerName][x, y] += amount;
            }
            else
            {
                AddInfluenceRadialToLayer(layerName, x, y, amount, radius);
            }
        }

        private void AddInfluenceRadialToLayer(string layerName, int centerX, int centerY, float amount, float radius)
        {
            var layer = _layers[layerName];
            int radiusCells = Mathf.CeilToInt(radius / _cellSize);
            
            for (int x = centerX - radiusCells; x <= centerX + radiusCells; x++)
            {
                for (int y = centerY - radiusCells; y <= centerY + radiusCells; y++)
                {
                    if (!IsValidCell(x, y) || IsBlocked(x, y))
                        continue;

                    float distance = Mathf.Sqrt((x - centerX) * (x - centerX) + (y - centerY) * (y - centerY)) * _cellSize;
                    
                    if (distance <= radius)
                    {
                        float falloff = 1f - (distance / radius);
                        layer[x, y] += amount * falloff;
                    }
                }
            }
        }

        /// <summary>
        /// Get influence from a specific layer.
        /// </summary>
        public float GetInfluenceFromLayer(string layerName, Vector3 worldPosition)
        {
            if (!_layers.ContainsKey(layerName))
                return 0f;

            if (!WorldToGrid(worldPosition, out int x, out int y))
                return 0f;

            return _layers[layerName][x, y];
        }

        /// <summary>
        /// Combine multiple layers with weights.
        /// </summary>
        public void CombineLayers(Dictionary<string, float> layerWeights)
        {
            Clear();
            
            foreach (var kvp in layerWeights)
            {
                string layerName = kvp.Key;
                float weight = kvp.Value;
                
                if (!_layers.ContainsKey(layerName))
                    continue;
                
                var layer = _layers[layerName];
                
                for (int x = 0; x < _width; x++)
                {
                    for (int y = 0; y < _height; y++)
                    {
                        if (!IsBlocked(x, y))
                            _influenceGrid[x, y] += layer[x, y] * weight;
                    }
                }
            }
        }

        /// <summary>
        /// Clear a specific layer.
        /// </summary>
        public void ClearLayer(string layerName)
        {
            if (!_layers.ContainsKey(layerName))
                return;

            var layer = _layers[layerName];
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    layer[x, y] = 0f;
                }
            }
        }

        #endregion
    }
}
