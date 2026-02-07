using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Shizounu.Library.ScriptableArchitecture
{
    [CreateAssetMenu(fileName = "ScriptableVector3", menuName = "Shizounu/ScriptableArchitecture/ScriptableVector3", order = 0)]
    public class ScriptableVector3 : ScriptableVariable<Vector3>{

    }

    [System.Serializable] public class Vector3Reference : VariableReference<Vector3>{
        public static implicit operator Vector3(Vector3Reference reference) => reference.Value;
    }
}
