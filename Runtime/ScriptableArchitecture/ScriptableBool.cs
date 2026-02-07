using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Shizounu.Library.ScriptableArchitecture
{
    [CreateAssetMenu(fileName = "ScriptableBool", menuName = "Shizounu/ScriptableArchitecture/ScriptableBool", order = 0)]
    public class ScriptableBool : ScriptableVariable<bool>{

    }

    [System.Serializable]public class BoolReference : VariableReference<bool>{
        public static implicit operator bool(BoolReference reference) => reference.Value;
    }
}