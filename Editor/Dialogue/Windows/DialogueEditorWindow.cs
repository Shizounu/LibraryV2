using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

using Shizounu.Library.Dialogue;

using Shizounu.Library.Dialogue.Data;
using Shizounu.Library.Editor.DialogueEditor.Utilities;
using UnityEngine;

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
        private DialogueTemplate selectedTemplate;

        public static DialogueEditorWindow ActiveWindow { get; private set; }
        public DialogueGraphView GraphView => graphView;
        public DialogueData LoadedData => loadedData;
        
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
            ActiveWindow = this;
            AddGraphView();
            AddToolbar();
            ApplyStyles();

            DialogueManager.ElementEntered += HandleRuntimeElementEntered;
            DialogueManager.DialogueEnded += HandleRuntimeDialogueEnded;
        }

        private void OnDisable()
        {
            if (ActiveWindow == this)
                ActiveWindow = null;

            graphView?.Dispose();

            DialogueManager.ElementEntered -= HandleRuntimeElementEntered;
            DialogueManager.DialogueEnded -= HandleRuntimeDialogueEnded;
        }

        private void OnFocus()
        {
            ActiveWindow = this;
        }
        
        #endregion

        #region UI Construction
        
        /// <summary>
        /// Creates and adds the toolbar to the window.
        /// </summary>
        private void AddToolbar()
        {
            Toolbar toolbar = new Toolbar();

            toolbar.AddToClassList("ds-toolbar");

            VisualElement fileSection = CreateToolbarSection("File");
            TextField fileNameField = ElementUtility.CreateTextField(
                currentEventName,
                "Name",
                change => currentEventName = change.newValue);
            fileNameField.AddToClassList("ds-toolbar__field--wide");
            fileSection.Add(fileNameField);

            Button saveButton = ElementUtility.CreateButton("Save", HandleSave);
            saveButton.tooltip = "Save dialogue asset";
            saveButton.AddToClassList("ds-toolbar__button--primary");
            fileSection.Add(saveButton);

            ObjectField loadField = ElementUtility.CreateSOField<DialogueData>(
                "Load",
                null,
                change => loadedData = (DialogueData)change.newValue);
            loadField.AddToClassList("ds-toolbar__field--wide");
            fileSection.Add(loadField);

            Button loadButton = ElementUtility.CreateButton("Open", HandleLoad);
            loadButton.tooltip = "Load selected dialogue asset";
            fileSection.Add(loadButton);

            toolbar.Add(fileSection);

            toolbar.Add(CreateToolbarSeparator());

            VisualElement viewSection = CreateToolbarSection("View");
            TextField searchField = ElementUtility.CreateTextField(
                string.Empty,
                "Search",
                change => graphView.ApplySearchFilter(change.newValue));
            searchField.AddToClassList("ds-toolbar__field--wide");
            viewSection.Add(searchField);

            Button clearSearchButton = ElementUtility.CreateButton("Clear", () =>
            {
                searchField.value = string.Empty;
                graphView.ClearSearchFilter();
            });
            clearSearchButton.tooltip = "Clear search filter";
            viewSection.Add(clearSearchButton);

            Button layoutButton = ElementUtility.CreateButton("Auto Layout", () => graphView.AutoLayout());
            layoutButton.tooltip = "Auto layout nodes";
            viewSection.Add(layoutButton);

            toolbar.Add(viewSection);

            toolbar.Add(CreateToolbarSeparator());

            VisualElement templateSection = CreateToolbarSection("Templates");
            ObjectField templateField = ElementUtility.CreateSOField<DialogueTemplate>(
                "Template",
                null,
                change => selectedTemplate = (DialogueTemplate)change.newValue);
            templateField.AddToClassList("ds-toolbar__field--wide");
            templateSection.Add(templateField);

            Button saveTemplateButton = ElementUtility.CreateButton("Save", HandleSaveTemplate);
            saveTemplateButton.tooltip = "Save selected nodes as template";
            templateSection.Add(saveTemplateButton);

            Button instantiateTemplateButton = ElementUtility.CreateButton("Instantiate", HandleInstantiateTemplate);
            instantiateTemplateButton.tooltip = "Instantiate selected template";
            templateSection.Add(instantiateTemplateButton);

            toolbar.Add(templateSection);

            ToolbarSpacer spacer = new ToolbarSpacer();
            spacer.style.flexGrow = 1;
            toolbar.Add(spacer);

            VisualElement toolsSection = CreateToolbarSection("Tools");
            toolsSection.Add(ElementUtility.CreateButton("Validate", () => DialogueValidationWindow.Open()));
            toolsSection.Add(ElementUtility.CreateButton("Debugger", () => DialogueDebuggerWindow.Open()));
            toolsSection.Add(ElementUtility.CreateButton("Diff", () => DialogueDiffWindow.Open()));
            toolbar.Add(toolsSection);

            // Apply toolbar styles
            toolbar.AddStyleSheets("ToolbarStyle.uss");

            rootVisualElement.Add(toolbar);
        }

        private static VisualElement CreateToolbarSection(string title)
        {
            VisualElement section = new VisualElement();
            section.AddToClassList("ds-toolbar__section");

            Label label = new Label(title);
            label.AddToClassList("ds-toolbar__section-label");
            section.Add(label);

            return section;
        }

        private static VisualElement CreateToolbarSeparator()
        {
            VisualElement separator = new VisualElement();
            separator.AddToClassList("ds-toolbar__separator");
            return separator;
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

            bool didSave = SavingUtility.Save(currentEventName, graphView);
            if (!didSave)
                UnityEngine.Debug.LogWarning("[DialogueEditor] Save was blocked due to validation errors.");
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

            graphView.BeginGraphUpdate();
            SavingUtility.Load(loadedData, graphView);
            graphView.EndGraphUpdate();
            currentEventName = loadedData.name;
        }

        private void HandleSaveTemplate()
        {
            DialogueTemplate template = DialogueTemplateUtility.CreateTemplateFromSelection(graphView);
            if (template == null)
            {
                UnityEngine.Debug.LogWarning("[DialogueEditor] No nodes selected to create a template.");
                return;
            }

            string path = EditorUtility.SaveFilePanelInProject(
                "Save Dialogue Template",
                "DialogueTemplate",
                "asset",
                "Choose a location for the template asset.");

            if (string.IsNullOrWhiteSpace(path))
                return;

            AssetDatabase.CreateAsset(template, path);
            AssetDatabase.SaveAssets();
            selectedTemplate = template;
        }

        private void HandleInstantiateTemplate()
        {
            if (selectedTemplate == null)
            {
                UnityEngine.Debug.LogWarning("[DialogueEditor] No template selected.");
                return;
            }

            Vector2 viewCenter = graphView.contentViewContainer.WorldToLocal(graphView.layout.center);
            DialogueTemplateUtility.InstantiateTemplate(selectedTemplate, graphView, viewCenter);
        }

        private void HandleRuntimeElementEntered(Shizounu.Library.Dialogue.Data.DialogueElement element)
        {
            if (graphView == null || element == null)
                return;

            if (loadedData != null && DialogueManager.ActiveDialogue == loadedData)
            {
                graphView.SetRuntimeActiveNode(element.ID);
            }
        }

        private void HandleRuntimeDialogueEnded()
        {
            graphView?.SetRuntimeActiveNode(string.Empty);
        }
        
        #endregion
    }
}
