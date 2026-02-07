using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Shizounu.Library.Editor
{
    [CustomEditor(typeof(Shizounu.Library.ScriptableArchitecture.ScriptableEvent), editorForChildClasses:true)]
    public class ScriptableEventEditor : UnityEditor.Editor {
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            GUI.enabled = Application.isPlaying;

            Shizounu.Library.ScriptableArchitecture.ScriptableEvent e = target as Shizounu.Library.ScriptableArchitecture.ScriptableEvent;
            if(GUILayout.Button("Invoke Event"))
                e.Invoke();    
        }
    }
 
   
}
