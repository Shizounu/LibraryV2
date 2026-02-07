using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shizounu.Library.Editor.DialogueEditor.Utilities
{
    /// <summary>
    /// Utility class for managing visual element styles and stylesheets in the Dialogue Editor.
    /// </summary>
    public static class StyleUtility
    {
        private static readonly Dictionary<string, string> StyleSheetPathCache = new Dictionary<string, string>();
        
        /// <summary>
        /// Adds multiple CSS class names to a visual element.
        /// </summary>
        /// <param name="element">The visual element to add classes to.</param>
        /// <param name="classNames">The class names to add.</param>
        /// <returns>The visual element for method chaining.</returns>
        public static VisualElement AddClasses(this VisualElement element, params string[] classNames)
        {
            if (element == null)
            {
                Debug.LogError("[StyleUtility] Cannot add classes to null element.");
                return null;
            }

            foreach (string className in classNames)
            {
                if (!string.IsNullOrWhiteSpace(className))
                {
                    element.AddToClassList(className);
                }
            }

            return element;
        }

        /// <summary>
        /// Adds multiple stylesheets to a visual element. Automatically locates stylesheets 
        /// regardless of whether they're in Packages or Assets folder.
        /// </summary>
        /// <param name="element">The visual element to add stylesheets to.</param>
        /// <param name="styleSheetNames">The names or relative paths of the stylesheets to add.</param>
        /// <returns>The visual element for method chaining.</returns>
        public static VisualElement AddStyleSheets(this VisualElement element, params string[] styleSheetNames)
        {
            if (element == null)
            {
                Debug.LogError("[StyleUtility] Cannot add stylesheets to null element.");
                return null;
            }

            foreach (string styleSheetName in styleSheetNames)
            {
                if (string.IsNullOrWhiteSpace(styleSheetName))
                {
                    Debug.LogWarning("[StyleUtility] Skipping null or empty stylesheet name.");
                    continue;
                }

                StyleSheet styleSheet = LoadStyleSheet(styleSheetName);
                
                if (styleSheet != null)
                {
                    element.styleSheets.Add(styleSheet);
                }
                else
                {
                    Debug.LogWarning($"[StyleUtility] Failed to load stylesheet: {styleSheetName}");
                }
            }

            return element;
        }

        /// <summary>
        /// Loads a stylesheet by name, searching in both Packages and Assets folders.
        /// Results are cached for improved performance on subsequent loads.
        /// </summary>
        /// <param name="styleSheetName">The name or path of the stylesheet.</param>
        /// <returns>The loaded StyleSheet, or null if not found.</returns>
        private static StyleSheet LoadStyleSheet(string styleSheetName)
        {
            // Check cache first
            if (StyleSheetPathCache.TryGetValue(styleSheetName, out string cachedPath))
            {
                StyleSheet cachedSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(cachedPath);
                if (cachedSheet != null)
                    return cachedSheet;
                
                // Path is stale, remove from cache
                StyleSheetPathCache.Remove(styleSheetName);
            }

            // Try to load using EditorGUIUtility first (handles Packages/ paths)
            StyleSheet styleSheet = EditorGUIUtility.Load(styleSheetName) as StyleSheet;
            if (styleSheet != null)
            {
                StyleSheetPathCache[styleSheetName] = AssetDatabase.GetAssetPath(styleSheet);
                return styleSheet;
            }

            // Extract just the filename if a path was provided
            string fileName = Path.GetFileName(styleSheetName);
            
            // Search for the stylesheet in the project
            string[] guids = AssetDatabase.FindAssets($"{Path.GetFileNameWithoutExtension(fileName)} t:StyleSheet");
            
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                
                // Check if this is the file we're looking for
                if (Path.GetFileName(assetPath) == fileName || 
                    assetPath.Replace("\\", "/").EndsWith(styleSheetName.Replace("\\", "/")))
                {
                    styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(assetPath);
                    if (styleSheet != null)
                    {
                        StyleSheetPathCache[styleSheetName] = assetPath;
                        return styleSheet;
                    }
                }
            }

            // If still not found, try loading from common locations
            string[] commonPaths = new[]
            {
                $"Packages/Library/Editor/Dialogue/Style Sheets/{fileName}",
                $"Assets/Library/Editor/Dialogue/Style Sheets/{fileName}",
                styleSheetName
            };

            foreach (string path in commonPaths)
            {
                styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(path);
                if (styleSheet != null)
                {
                    StyleSheetPathCache[styleSheetName] = path;
                    return styleSheet;
                }
            }

            return null;
        }

        /// <summary>
        /// Clears the stylesheet path cache. Useful if stylesheets are moved or renamed.
        /// </summary>
        public static void ClearStyleSheetCache()
        {
            StyleSheetPathCache.Clear();
        }
    }
}
