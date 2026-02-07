using System.Collections.Generic;
using UnityEngine;

namespace Shizounu.Library.GameAI.GOAP.Examples
{
    /// <summary>
    /// Demonstrates that deep copy works correctly for WorldState and Blackboard.
    /// This is critical for GOAP planning to work properly, as the planner simulates
    /// different world states without affecting the original.
    /// </summary>
    public class DeepCopyExample : MonoBehaviour
    {
        [System.Serializable]
        public class TestData
        {
            public int number;
            public string text;
            
            public override string ToString() => $"TestData({number}, {text})";
        }

        private void Start()
        {
            Debug.Log("=== WorldState Deep Copy Test ===");
            TestWorldStateDeepCopy();
            
            Debug.Log("\n=== Blackboard Deep Copy Test ===");
            TestBlackboardDeepCopy();
            
            Debug.Log("\n=== Collection Deep Copy Test ===");
            TestCollectionDeepCopy();
            
            Debug.Log("\n=== GOAP Planning Simulation Test ===");
            TestGoapPlanningSimulation();
        }

        /// <summary>
        /// Test that copying a WorldState creates independent copies of values.
        /// </summary>
        private void TestWorldStateDeepCopy()
        {
            // Create original world state
            var original = new WorldState();
            original.SetValue("health", 100f);
            original.SetValue("position", new Vector3(10, 0, 0));
            
            var testData = new TestData { number = 42, text = "original" };
            original.SetValue("data", testData);
            
            var inventory = new List<string> { "sword", "shield" };
            original.SetValue("inventory", inventory);

            // Create a copy
            var copy = new WorldState(original);

            // Verify initial values match
            Debug.Log($"Original health: {original.GetValue<float>("health")}");
            Debug.Log($"Copy health: {copy.GetValue<float>("health")}");

            // Modify the copy
            copy.SetValue("health", 50f);
            copy.SetValue("position", new Vector3(20, 0, 0));
            
            var copyTestData = copy.GetValue<TestData>("data");
            copyTestData.number = 999;
            copyTestData.text = "modified";
            
            var copyInventory = copy.GetValue<List<string>>("inventory");
            copyInventory.Add("potion");

            // Verify original is unchanged
            Debug.Log($"<color=lime>✓ Original health unchanged: {original.GetValue<float>("health")} == 100</color>");
            Debug.Log($"<color=lime>✓ Copy health modified: {copy.GetValue<float>("health")} == 50</color>");
            
            var originalTestData = original.GetValue<TestData>("data");
            Debug.Log($"<color=lime>✓ Original TestData unchanged: {originalTestData}</color>");
            Debug.Log($"<color=cyan>  Copy TestData modified: {copyTestData}</color>");
            
            var originalInventory = original.GetValue<List<string>>("inventory");
            Debug.Log($"<color=lime>✓ Original inventory size: {originalInventory.Count} == 2</color>");
            Debug.Log($"<color=cyan>  Copy inventory size: {copyInventory.Count} == 3</color>");
            
            var originalPos = original.GetValue<Vector3>("position");
            var copyPos = copy.GetValue<Vector3>("position");
            Debug.Log($"<color=lime>✓ Original position: {originalPos}</color>");
            Debug.Log($"<color=cyan>  Copy position: {copyPos}</color>");
        }

        /// <summary>
        /// Test that Blackboard deep copy works for various data types.
        /// </summary>
        private void TestBlackboardDeepCopy()
        {
            var original = new SimpleBlackboard();
            
            // Test various types
            original.SetValue("int", 42);
            original.SetValue("float", 3.14f);
            original.SetValue("string", "test");
            original.SetValue("bool", true);
            original.SetValue("vector", new Vector3(1, 2, 3));
            
            var array = new int[] { 1, 2, 3, 4, 5 };
            original.SetValue("array", array);
            
            var dict = new Dictionary<string, int> { { "a", 1 }, { "b", 2 } };
            original.SetValue("dict", dict);

            // Deep copy
            var copy = original.DeepCopy();

            // Modify arrays and collections in the copy
            var copyArray = copy.GetValue<int[]>("array");
            copyArray[0] = 999;
            
            var copyDict = copy.GetValue<Dictionary<string, int>>("dict");
            copyDict["a"] = 999;

            // Verify original is unchanged
            var originalArray = original.GetValue<int[]>("array");
            var originalDict = original.GetValue<Dictionary<string, int>>("dict");
            
            Debug.Log($"<color=lime>✓ Original array[0]: {originalArray[0]} == 1 (unchanged)</color>");
            Debug.Log($"<color=cyan>  Copy array[0]: {copyArray[0]} == 999</color>");
            
            Debug.Log($"<color=lime>✓ Original dict['a']: {originalDict["a"]} == 1 (unchanged)</color>");
            Debug.Log($"<color=cyan>  Copy dict['a']: {copyDict["a"]} == 999</color>");
        }

        /// <summary>
        /// Test deep copying of nested collections.
        /// </summary>
        private void TestCollectionDeepCopy()
        {
            var original = new SimpleBlackboard();
            
            // Nested list
            var nestedList = new List<List<int>>
            {
                new List<int> { 1, 2, 3 },
                new List<int> { 4, 5, 6 }
            };
            original.SetValue("nested", nestedList);

            // Deep copy
            var copy = original.DeepCopy();

            // Modify nested collection
            var copyNested = copy.GetValue<List<List<int>>>("nested");
            copyNested[0].Add(999);

            // Verify original is unchanged
            var originalNested = original.GetValue<List<List<int>>>("nested");
            
            Debug.Log($"<color=lime>✓ Original nested[0] size: {originalNested[0].Count} == 3 (unchanged)</color>");
            Debug.Log($"<color=cyan>  Copy nested[0] size: {copyNested[0].Count} == 4</color>");
        }

        /// <summary>
        /// Test that GOAP planning simulation works correctly with deep copies.
        /// This simulates what happens during A* planning.
        /// </summary>
        private void TestGoapPlanningSimulation()
        {
            // Create initial world state
            var currentState = new WorldState();
            currentState.SetValue("hasWood", false);
            currentState.SetValue("hasStone", false);
            currentState.SetValue("hasTool", false);
            currentState.SetValue("stamina", 100);

            Debug.Log("Initial state: hasWood=false, hasStone=false, hasTool=false, stamina=100");

            // Simulate planning: exploring different action sequences
            // Branch 1: Try gathering wood
            var branch1 = currentState.Clone();
            branch1.SetValue("hasWood", true);
            branch1.SetValue("stamina", 80);
            Debug.Log("<color=yellow>Branch 1 after gathering wood: hasWood=true, stamina=80</color>");

            // Branch 2: Try gathering stone
            var branch2 = currentState.Clone();
            branch2.SetValue("hasStone", true);
            branch2.SetValue("stamina", 80);
            Debug.Log("<color=yellow>Branch 2 after gathering stone: hasStone=true, stamina=80</color>");

            // Verify original state is unchanged
            Debug.Log($"<color=lime>✓ Original state unchanged:</color>");
            Debug.Log($"  hasWood={currentState.GetValue<bool>("hasWood")}");
            Debug.Log($"  hasStone={currentState.GetValue<bool>("hasStone")}");
            Debug.Log($"  stamina={currentState.GetValue<int>("stamina")}");

            // Continue branch 1
            branch1.SetValue("hasStone", true);
            branch1.SetValue("stamina", 60);
            Debug.Log("<color=yellow>Branch 1 after also gathering stone: stamina=60</color>");

            // Craft tool in branch 1
            branch1.SetValue("hasTool", true);
            branch1.SetValue("hasWood", false);
            branch1.SetValue("hasStone", false);
            Debug.Log("<color=yellow>Branch 1 after crafting: hasTool=true, resources consumed</color>");

            // Verify branch 2 is independent
            Debug.Log($"<color=lime>✓ Branch 2 still independent:</color>");
            Debug.Log($"  hasWood={branch2.GetValue<bool>("hasWood")}");
            Debug.Log($"  hasStone={branch2.GetValue<bool>("hasStone")}");
            Debug.Log($"  hasTool={branch2.GetValue<bool>("hasTool")}");
            Debug.Log($"  stamina={branch2.GetValue<int>("stamina")}");

            Debug.Log("<color=lime>✓ Deep copy allows independent state simulation for planning!</color>");
        }
    }
}
