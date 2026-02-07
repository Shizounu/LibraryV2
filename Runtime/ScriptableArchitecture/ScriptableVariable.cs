using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Shizounu.Library.ScriptableArchitecture
{
    public abstract class ScriptableVariable<T> : ScriptableObject 
    {
        [SerializeField] private T _initialValue;
        private T _runtimeValue;
        
        public T RuntimeValue
        {
            get => _runtimeValue;
            set 
            {
                _runtimeValue = value;
                if (OnRuntimeValueChange != null)
                    OnRuntimeValueChange.Invoke();
            }
        }

        public ScriptableEvent OnRuntimeValueChange;
        
        public void OnEnable()
        {
            RuntimeValue = _initialValue;
        }
        
        public void OnDisable()
        {
        }
    }

    [System.Serializable] 
    public abstract class VariableReference<T>
    {
        public bool UseConstant;
        public T ConstantValue;
        public ScriptableVariable<T> Variable;

        public VariableReference() { }
        
        public VariableReference(T value)
        {
            UseConstant = true;
            ConstantValue = value;
        }

        public T Value
        {
            get { return UseConstant ? ConstantValue : Variable.RuntimeValue; }
            set 
            { 
                if (UseConstant)
                {
                    Debug.LogError("Cannot change constant value");
                    return;
                }

                Variable.RuntimeValue = value;
            }
        }
    }
    
}

