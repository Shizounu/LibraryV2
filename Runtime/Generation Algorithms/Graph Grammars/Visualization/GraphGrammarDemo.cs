using System.Collections;
using UnityEngine;
using Shizounu.Library.RandomSystem;
using UnityEngine.InputSystem;

namespace Shizounu.Library.GenerationAlgorithms.GraphGrammars.Visualization
{
    /// <summary>
    /// Interactive demo of graph grammar generation with step-by-step visualization.
    /// </summary>
    public class GraphGrammarDemo : MonoBehaviour
    {
        [Header("Demo Settings")]
        [Tooltip("Type of demo to run")]
        [SerializeField] private DemoType demoType = DemoType.Branching;

        [Tooltip("Seed for reproducible generation")]
        [SerializeField] private int seed = 42;

        [Tooltip("Start generation automatically on play")]
        [SerializeField] private bool autoStart = true;

        [Tooltip("Delay between generation steps (seconds)")]
        [SerializeField] private float stepDelay = 0.5f;

        [Tooltip("Maximum iterations for generation")]
        [SerializeField] private int maxIterations = 15;

        [Header("References")]
        [SerializeField] private GraphGrammarVisualizer visualizer;

        private GraphGrammarEngine<string> _engine;
        private RngContext _rngContext;
        private RngUser _rngUser;
        private bool _isGenerating = false;

        public enum DemoType
        {
            BasicExpansion,
            Branching,
            Dungeon,
            Custom
        }

        private void Start()
        {
            if (visualizer == null)
                visualizer = GetComponent<GraphGrammarVisualizer>();

            if (visualizer == null)
            {
                Debug.LogError("GraphGrammarVisualizer not found! Adding one...");
                visualizer = gameObject.AddComponent<GraphGrammarVisualizer>();
            }

            if (autoStart)
                StartGeneration();
        }

        private void Update()
        {
            // Space to step, R to reset, G to generate all
            
            if (Keyboard.current.spaceKey.wasPressedThisFrame && !_isGenerating)
            {
                GenerateOneStep();
            }
            else if (Keyboard.current.rKey.wasPressedThisFrame)
            {
                ResetGeneration();
            }
            else if (Keyboard.current.gKey.wasPressedThisFrame && !_isGenerating)
            {
                StartGeneration();
            }
        }

        /// <summary>
        /// Starts the generation process from scratch.
        /// </summary>
        [ContextMenu("Start Generation")]
        public void StartGeneration()
        {
            ResetGeneration();
            StartCoroutine(GenerateCoroutine());
        }

        /// <summary>
        /// Resets to initial state.
        /// </summary>
        [ContextMenu("Reset")]
        public void ResetGeneration()
        {
            StopAllCoroutines();
            _isGenerating = false;

            // Setup RNG
            _rngContext = new RngContext();
            _rngUser = _rngContext.GetOrCreateUser("GraphGrammarDemo", seed: (uint)seed);

            // Create initial graph and engine based on demo type
            Graph<string> initialGraph;
            switch (demoType)
            {
                case DemoType.BasicExpansion:
                    initialGraph = CreateBasicExpansionDemo(out _engine);
                    break;
                case DemoType.Branching:
                    initialGraph = CreateBranchingDemo(out _engine);
                    break;
                case DemoType.Dungeon:
                    initialGraph = CreateDungeonDemo(out _engine);
                    break;
                default:
                    initialGraph = CreateBasicExpansionDemo(out _engine);
                    break;
            }

            visualizer.SetGraph(initialGraph);
            Debug.Log($"Graph Grammar Demo Reset - Type: {demoType}, Seed: {seed}");
        }

        /// <summary>
        /// Generates one step of the grammar.
        /// </summary>
        [ContextMenu("Generate One Step")]
        public void GenerateOneStep()
        {
            if (_engine == null)
            {
                ResetGeneration();
            }

            bool applied = _engine.Step();
            
            if (applied)
            {
                visualizer.RefreshLayout();
                Debug.Log($"Step {_engine.IterationCount}: Graph now has {_engine.Graph.NodeCount} nodes, {_engine.Graph.EdgeCount} edges");
            }
            else
            {
                Debug.Log("No more rules can be applied!");
            }
        }

        private IEnumerator GenerateCoroutine()
        {
            _isGenerating = true;

            for (int i = 0; i < maxIterations; i++)
            {
                bool applied = _engine.Step();
                
                if (!applied)
                {
                    Debug.Log($"Generation complete at iteration {i}: No more rules applicable");
                    break;
                }

                visualizer.RefreshLayout();
                Debug.Log($"Step {_engine.IterationCount}: {_engine.Graph.NodeCount} nodes, {_engine.Graph.EdgeCount} edges");

                yield return new WaitForSeconds(stepDelay);
            }

            if (_engine.IterationCount >= maxIterations)
            {
                Debug.Log($"Generation stopped: Max iterations ({maxIterations}) reached");
            }

            _isGenerating = false;
        }

        private Graph<string> CreateBasicExpansionDemo(out GraphGrammarEngine<string> engine)
        {
            var initialGraph = new Graph<string>();
            var startNode = new GraphNode<string>("Start");
            initialGraph.AddNode(startNode);

            // Rule: Start -> A -> B
            var pattern = new Graph<string>();
            var patStart = new GraphNode<string>("Start");
            pattern.AddNode(patStart);

            var replacement = new Graph<string>();
            var repA = new GraphNode<string>("A");
            var repB = new GraphNode<string>("B");
            replacement.AddNode(repA);
            replacement.AddNode(repB);
            replacement.AddEdge(new GraphEdge<string>(repA, repB, "next"));

            var rule = new GraphProductionRule<string>(pattern, replacement, "Start->AB");

            engine = new GraphGrammarEngine<string>(initialGraph, _rngUser.Source);
            engine.AddRule(rule);

            return initialGraph;
        }

        private Graph<string> CreateBranchingDemo(out GraphGrammarEngine<string> engine)
        {
            var initialGraph = new Graph<string>();
            var f = new GraphNode<string>("F");
            initialGraph.AddNode(f);

            // Rule: F -> F with two branches
            var pattern = new Graph<string>();
            var patF = new GraphNode<string>("F");
            pattern.AddNode(patF);

            var replacement = new Graph<string>();
            var center = new GraphNode<string>("F");
            var left = new GraphNode<string>("F");
            var right = new GraphNode<string>("F");
            replacement.AddNode(center);
            replacement.AddNode(left);
            replacement.AddNode(right);
            replacement.AddEdge(new GraphEdge<string>(center, left, "left"));
            replacement.AddEdge(new GraphEdge<string>(center, right, "right"));

            var rule = new RuleBuilder<string>()
                .WithName("Branch")
                .WithPriority(10)
                .WithPattern(pattern)
                .WithReplacement(replacement)
                .WithPredicate((g, m) => g.NodeCount < 20)
                .Build();

            engine = new GraphGrammarEngine<string>(initialGraph, _rngUser.Source);
            engine.AddRule(rule);
            engine.SelectionStrategy = RuleSelectionStrategy.Random;

            return initialGraph;
        }

        private Graph<string> CreateDungeonDemo(out GraphGrammarEngine<string> engine)
        {
            var initialGraph = new Graph<string>();
            var entrance = new GraphNode<string>("Room");
            entrance.SetAttribute("type", "entrance");
            entrance.SetAttribute("size", "large");
            initialGraph.AddNode(entrance);

            // Rule 1: Entrance expansion
            var pat1 = new Graph<string>();
            var patEntrance = new GraphNode<string>("Room");
            patEntrance.SetAttribute("type", "entrance");
            pat1.AddNode(patEntrance);

            var rep1 = new Graph<string>();
            var repEntrance = new GraphNode<string>("Room");
            repEntrance.SetAttribute("type", "entrance");
            var corridor1 = new GraphNode<string>("Corridor");
            var room1 = new GraphNode<string>("Room");
            room1.SetAttribute("type", "normal");
            rep1.AddNode(repEntrance);
            rep1.AddNode(corridor1);
            rep1.AddNode(room1);
            rep1.AddEdge(new GraphEdge<string>(repEntrance, corridor1, "connects"));
            rep1.AddEdge(new GraphEdge<string>(corridor1, room1, "connects"));

            var rule1 = new RuleBuilder<string>()
                .WithName("Expand Entrance")
                .WithPriority(100)
                .WithPattern(pat1)
                .WithReplacement(rep1)
                .Preserve(patEntrance, repEntrance)
                .Build();

            // Rule 2: Normal room expansion
            var pat2 = new Graph<string>();
            var patRoom = new GraphNode<string>("Room");
            patRoom.SetAttribute("type", "normal");
            pat2.AddNode(patRoom);

            var rep2 = new Graph<string>();
            var repRoom = new GraphNode<string>("Room");
            repRoom.SetAttribute("type", "normal");
            var corridor2 = new GraphNode<string>("Corridor");
            var newRoom = new GraphNode<string>("Room");
            newRoom.SetAttribute("type", "normal");
            rep2.AddNode(repRoom);
            rep2.AddNode(corridor2);
            rep2.AddNode(newRoom);
            rep2.AddEdge(new GraphEdge<string>(repRoom, corridor2, "connects"));
            rep2.AddEdge(new GraphEdge<string>(corridor2, newRoom, "connects"));

            var rule2 = new RuleBuilder<string>()
                .WithName("Expand Room")
                .WithPriority(50)
                .WithPattern(pat2)
                .WithReplacement(rep2)
                .Preserve(patRoom, repRoom)
                .WithPredicate((g, m) => 
                {
                    int roomCount = 0;
                    foreach (var node in g.Nodes)
                        if (node.Data == "Room") roomCount++;
                    return roomCount < 10;
                })
                .Build();

            // Rule 3: Add treasure room
            var pat3 = new Graph<string>();
            var patNormalRoom = new GraphNode<string>("Room");
            patNormalRoom.SetAttribute("type", "normal");
            pat3.AddNode(patNormalRoom);

            var rep3 = new Graph<string>();
            var repNormalRoom = new GraphNode<string>("Room");
            repNormalRoom.SetAttribute("type", "normal");
            var corridor3 = new GraphNode<string>("Corridor");
            var treasureRoom = new GraphNode<string>("Room");
            treasureRoom.SetAttribute("type", "treasure");
            rep3.AddNode(repNormalRoom);
            rep3.AddNode(corridor3);
            rep3.AddNode(treasureRoom);
            rep3.AddEdge(new GraphEdge<string>(repNormalRoom, corridor3, "connects"));
            rep3.AddEdge(new GraphEdge<string>(corridor3, treasureRoom, "connects"));

            var rule3 = new RuleBuilder<string>()
                .WithName("Add Treasure")
                .WithPriority(10)
                .WithPattern(pat3)
                .WithReplacement(rep3)
                .Preserve(patNormalRoom, repNormalRoom)
                .WithPredicate((g, m) =>
                {
                    int treasureCount = 0;
                    foreach (var node in g.Nodes)
                        if (node.GetAttribute("type") == "treasure") treasureCount++;
                    return treasureCount < 2;
                })
                .Build();

            engine = new GraphGrammarEngine<string>(initialGraph, _rngUser.Source);
            engine.AddRule(rule1);
            engine.AddRule(rule2);
            engine.AddRule(rule3);
            engine.SelectionStrategy = RuleSelectionStrategy.HighestPriority;

            return initialGraph;
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Label($"Graph Grammar Demo - {demoType}", GUI.skin.box);
            GUILayout.Label($"Seed: {seed}");
            
            if (_engine != null)
            {
                GUILayout.Label($"Iteration: {_engine.IterationCount}");
                GUILayout.Label($"Nodes: {_engine.Graph.NodeCount}");
                GUILayout.Label($"Edges: {_engine.Graph.EdgeCount}");
            }

            GUILayout.Space(10);
            GUILayout.Label("Controls:", EditorStyles.boldLabel);
            GUILayout.Label("SPACE - Generate one step");
            GUILayout.Label("G - Generate all steps");
            GUILayout.Label("R - Reset");
            
            GUILayout.EndArea();
        }

        // EditorStyles fallback for runtime
        private static class EditorStyles
        {
            public static GUIStyle boldLabel = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold };
        }
    }
}
