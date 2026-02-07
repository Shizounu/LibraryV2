using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Shizounu.Library.ScriptableArchitecture
{
    [CreateAssetMenu(fileName = "ScriptableFloat", menuName = "Shizounu/ScriptableArchitecture/ScriptableFloat", order = 0)]
    public class ScriptableFloat : ScriptableVariable<float>{

    }

    [System.Serializable]public class FloatReference : VariableReference<float>{
        public static implicit operator float(FloatReference reference) => reference.Value;
    }
}
