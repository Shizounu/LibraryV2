using System;
using System.Collections.Generic;
using System.Linq;
using Shizounu.Library.RandomSystem;

namespace Shizounu.Library.GenerationAlgorithms.GraphGrammars.Examples
{
    /// <summary>
    /// Comprehensive examples demonstrating graph grammar generation.
    /// </summary>
    public static class GraphGrammarExample
    {
        /// <summary>
        /// Basic example: Expand a single node into a linear chain.
        /// </summary>
        public static void BasicExpansionExample()
        {
            Console.WriteLine("=== Basic Expansion Example ===");

            // Initial graph with a single "Start" node
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

            // Generate
            var engine = new GraphGrammarEngine<string>(initialGraph, seed: 12345);
            engine.AddRule(rule);
            var result = engine.Generate(steps: 1);

            Console.WriteLine($"Result: {result}");
            Console.WriteLine($"Final graph: {engine.Graph.NodeCount} nodes, {engine.Graph.EdgeCount} edges");
            
            foreach (var node in engine.Graph.Nodes)
            {
                Console.WriteLine($"  Node: {node.Data}");
            }
        }

        /// <summary>
        /// L-System-like branching structure.
        /// </summary>
        public static void BranchingExample()
        {
            Console.WriteLine("\n=== Branching Example (L-System Style) ===");

            // Initial: F
            var initialGraph = new Graph<string>();
            var f = new GraphNode<string>("F");
            initialGraph.AddNode(f);

            // Rule 1: F -> F [ + F ] F [ - F ] F
            // Simplified: F -> F with two branches
            var pattern1 = new Graph<string>();
            var patF = new GraphNode<string>("F");
            pattern1.AddNode(patF);

            var replacement1 = new Graph<string>();
            var center = new GraphNode<string>("F");
            var left = new GraphNode<string>("F");
            var right = new GraphNode<string>("F");
            replacement1.AddNode(center);
            replacement1.AddNode(left);
            replacement1.AddNode(right);
            replacement1.AddEdge(new GraphEdge<string>(center, left, "branch_left"));
            replacement1.AddEdge(new GraphEdge<string>(center, right, "branch_right"));

            var rule1 = new RuleBuilder<string>()
                .WithName("Branch")
                .WithPriority(10)
                .WithPattern(pattern1)
                .WithReplacement(replacement1)
                .WithPredicate((g, m) => g.NodeCount < 20) // Limit growth
                .Build();

            // Generate with reproducible RNG
            var context = new RngContext();
            var rngUser = context.GetOrCreateUser("GraphGrammar", seed: 42);
            var engine = new GraphGrammarEngine<string>(initialGraph, rngUser.Source);
            engine.AddRule(rule1);
            engine.SelectionStrategy = RuleSelectionStrategy.Random;

            var result = engine.Generate();

            Console.WriteLine($"Result: {result}");
            Console.WriteLine($"Generated tree: {engine.Graph.NodeCount} nodes, {engine.Graph.EdgeCount} edges");
            Console.WriteLine($"Iterations: {engine.IterationCount}");
        }

        /// <summary>
        /// Dungeon room generation with corridors.
        /// </summary>
        public static void DungeonExample()
        {
            Console.WriteLine("\n=== Dungeon Generation Example ===");

            // Initial: Single room marked as "entrance"
            var initialGraph = new Graph<string>();
            var entrance = new GraphNode<string>("Room");
            entrance.SetAttribute("type", "entrance");
            entrance.SetAttribute("size", "large");
            initialGraph.AddNode(entrance);

            // Rule 1: Entrance room -> Entrance + Corridor + Room (high priority)
            var pat1 = new Graph<string>();
            var patEntrance = new GraphNode<string>("Room");
            patEntrance.SetAttribute("type", "entrance");
            pat1.AddNode(patEntrance);

            var rep1 = new Graph<string>();
            var repEntrance = new GraphNode<string>("Room");
            repEntrance.SetAttribute("type", "entrance");
            repEntrance.SetAttribute("size", "large");
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

            // Rule 2: Normal room -> Room + Corridor + Room (expandable)
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
                .WithPredicate((g, m) => g.Nodes.Count(n => n.Data == "Room") < 10)
                .Build();

            // Rule 3: Add treasure room (low priority, limited)
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
                .WithName("Add Treasure Room")
                .WithPriority(10)
                .WithPattern(pat3)
                .WithReplacement(rep3)
                .Preserve(patNormalRoom, repNormalRoom)
                .WithPredicate((g, m) => g.Nodes.Count(n => n.GetAttribute("type") == "treasure") < 2)
                .Build();

            // Generate dungeon
            var engine = new GraphGrammarEngine<string>(initialGraph, seed: 999);
            engine.AddRule(rule1);
            engine.AddRule(rule2);
            engine.AddRule(rule3);
            engine.SelectionStrategy = RuleSelectionStrategy.HighestPriority;
            engine.MaxIterations = 50;

            var result = engine.Generate();

            Console.WriteLine($"Result: {result}");
            Console.WriteLine($"Dungeon stats:");
            Console.WriteLine($"  Total elements: {engine.Graph.NodeCount}");
            Console.WriteLine($"  Rooms: {engine.Graph.Nodes.Count(n => n.Data == "Room")}");
            Console.WriteLine($"  Corridors: {engine.Graph.Nodes.Count(n => n.Data == "Corridor")}");
            Console.WriteLine($"  Treasure rooms: {engine.Graph.Nodes.Count(n => n.GetAttribute("type") == "treasure")}");
            Console.WriteLine($"  Connections: {engine.Graph.EdgeCount}");
        }

        /// <summary>
        /// Demonstrates different rule selection strategies.
        /// </summary>
        public static void SelectionStrategyExample()
        {
            Console.WriteLine("\n=== Selection Strategy Comparison ===");

            // Create two competing rules with same pattern
            var pattern = new Graph<int>();
            var p = new GraphNode<int>(0);
            pattern.AddNode(p);

            // Rule A: 0 -> 1 (adds small structure)
            var repA = new Graph<int>();
            var ra = new GraphNode<int>(1);
            repA.AddNode(ra);
            var ruleA = new GraphProductionRule<int>(pattern, repA, "Small") { Priority = 10 };

            // Rule B: 0 -> 2 - 2 (adds larger structure)
            var repB = new Graph<int>();
            var rb1 = new GraphNode<int>(2);
            var rb2 = new GraphNode<int>(2);
            repB.AddNode(rb1);
            repB.AddNode(rb2);
            repB.AddEdge(new GraphEdge<int>(rb1, rb2));
            var ruleB = new GraphProductionRule<int>(pattern, repB, "Large") { Priority = 5 };

            var strategies = new[]
            {
                RuleSelectionStrategy.FirstMatch,
                RuleSelectionStrategy.Random,
                RuleSelectionStrategy.HighestPriority,
                RuleSelectionStrategy.MaximizeGrowth,
                RuleSelectionStrategy.MinimizeGrowth
            };

            foreach (var strategy in strategies)
            {
                var initial = new Graph<int>();
                initial.AddNode(new GraphNode<int>(0));

                var engine = new GraphGrammarEngine<int>(initial, seed: 555);
                engine.AddRule(ruleA);
                engine.AddRule(ruleB);
                engine.SelectionStrategy = strategy;

                engine.Generate(steps: 10);

                Console.WriteLine($"{strategy}: {engine.Graph.NodeCount} nodes, {engine.Graph.EdgeCount} edges");
            }
        }

        /// <summary>
        /// Shows integration with the RNG system for reproducibility and snapshots.
        /// </summary>
        public static void ReproducibleGenerationExample()
        {
            Console.WriteLine("\n=== Reproducible Generation with Snapshots ===");

            // Setup RNG context
            var context = new RngContext();
            var rngUser = context.GetOrCreateUser("GraphGrammar", seed: 42);

            // Simple expansion rule
            var pattern = new Graph<string>();
            var p = new GraphNode<string>("X");
            pattern.AddNode(p);

            var replacement = new Graph<string>();
            var r1 = new GraphNode<string>("Y");
            var r2 = new GraphNode<string>("Y");
            replacement.AddNode(r1);
            replacement.AddNode(r2);
            replacement.AddEdge(new GraphEdge<string>(r1, r2));

            var rule = new GraphProductionRule<string>(pattern, replacement, "X->YY");

            // First generation
            var initial1 = new Graph<string>();
            initial1.AddNode(new GraphNode<string>("X"));

            var engine1 = new GraphGrammarEngine<string>(initial1, rngUser.Source);
            engine1.AddRule(rule);
            engine1.SelectionStrategy = RuleSelectionStrategy.Random;

            Console.WriteLine("First run:");
            for (int i = 0; i < 3; i++)
            {
                engine1.Step();
                Console.WriteLine($"  Iteration {i + 1}: {engine1.Graph.NodeCount} nodes");
            }

            // Take snapshot and reset
            var snapshot = context.CreateSnapshot("After 3 iterations");

            // Second generation from snapshot (should match)
            var initial2 = new Graph<string>();
            initial2.AddNode(new GraphNode<string>("X"));

            context.RestoreFromSnapshot(snapshot);
            var engine2 = new GraphGrammarEngine<string>(initial2, rngUser.Source);
            engine2.AddRule(rule);
            engine2.SelectionStrategy = RuleSelectionStrategy.Random;

            Console.WriteLine("\nSecond run from snapshot (should match):");
            for (int i = 0; i < 3; i++)
            {
                engine2.Step();
                Console.WriteLine($"  Iteration {i + 1}: {engine2.Graph.NodeCount} nodes");
            }

            Console.WriteLine($"\nGraphs match: {engine1.Graph.NodeCount == engine2.Graph.NodeCount}");
        }

        /// <summary>
        /// Runs all examples.
        /// </summary>
        public static void RunAllExamples()
        {
            BasicExpansionExample();
            BranchingExample();
            DungeonExample();
            SelectionStrategyExample();
            ReproducibleGenerationExample();

            Console.WriteLine("\n=== All examples completed ===");
        }
    }
}
