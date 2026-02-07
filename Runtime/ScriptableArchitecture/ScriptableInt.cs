using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Shizounu.Library.ScriptableArchitecture
{
    [CreateAssetMenu(fileName = "ScriptableInt", menuName = "Shizounu/ScriptableArchitecture/ScriptableInt", order = 0)]
    public class ScriptableInt : ScriptableVariable<int>{

    }

    [System.Serializable]public class IntReference : VariableReference<int>{
        public static implicit operator int(IntReference reference) => reference.Value;
    }
}
