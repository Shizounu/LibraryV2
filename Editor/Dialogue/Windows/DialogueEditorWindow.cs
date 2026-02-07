using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

using Shizounu.Library.Dialogue.Data;
using Shizounu.Library.Editor.DialogueEditor.Utilities;

namespace Shizounu.Library.Editor.DialogueEditor.Windows
{
    /// <summary>
    /// Main editor window for creating and editing dialogue graphs.
    /// </summary>
    public class DialogueEditorWindow : EditorWindow
    {
        #region Constants
        
        private const string DefaultEventName = "new Dialogue";
        private const string WindowTitle = "Dialogue Editor";
        private const string MenuPath = "Shizounu/Dialogue Editor";
        
        #endregion

        #region Fields
        
        private string currentEventName = DefaultEventName;
        private DialogueGraphView graphView;
        private DialogueData loadedData;
        
        #endregion

        #region Unity Menu
        
        [MenuItem(MenuPath)]
        public static void Open()
        {
            GetWindow<DialogueEditorWindow>(WindowTitle);
        }
        
        #endregion

        #region Unity Lifecycle
        
        private void OnEnable()
        {
            AddGraphView();
            AddToolbar();
            ApplyStyles();
        }
        
        #endregion

        #region UI Construction
        
        /// <summary>
        /// Creates and adds the toolbar to the window.
        /// </summary>
        private void AddToolbar()
        {
            Toolbar toolbar = new Toolbar();

            // File name field
            toolbar.Add(ElementUtility.CreateTextField(
                currentEventName, 
                "FileName:", 
                change => currentEventName = change.newValue));

            // Save button
            toolbar.Add(ElementUtility.CreateButton("Save", HandleSave));

            // Load file field
            toolbar.Add(ElementUtility.CreateSOField<DialogueData>(
                "File to Load", 
                null,
                change => loadedData = (DialogueData)change.newValue));

            // Load button
            toolbar.Add(ElementUtility.CreateButton("Load", HandleLoad));

            // Apply toolbar styles
            toolbar.AddStyleSheets("ToolbarStyle.uss");

            rootVisualElement.Add(toolbar);
        }

        /// <summary>
        /// Applies stylesheets to the root visual element.
        /// </summary>
        private void ApplyStyles()
        {
            rootVisualElement.AddStyleSheets("Variables.uss");
        }

        /// <summary>
        /// Creates and adds the graph view to the window.
        /// </summary>
        private void AddGraphView()
        {
            graphView = new DialogueGraphView(this);
            graphView.StretchToParentSize();
            rootVisualElement.Add(graphView);
        }
        
        #endregion

        #region Event Handlers
        
        /// <summary>
        /// Handles the save button click event.
        /// </summary>
        private void HandleSave()
        {
            if (string.IsNullOrWhiteSpace(currentEventName))
            {
                UnityEngine.Debug.LogWarning("[DialogueEditor] Cannot save: File name is empty.");
                return;
            }

            SavingUtility.Save(currentEventName, graphView);
        }

        /// <summary>
        /// Handles the load button click event.
        /// </summary>
        private void HandleLoad()
        {
            if (loadedData == null)
            {
                UnityEngine.Debug.LogWarning("[DialogueEditor] Cannot load: No dialogue data selected.");
                return;
            }

            SavingUtility.Load(loadedData, graphView);
            currentEventName = loadedData.name;
        }
        
        #endregion
    }
}
