using UnityEngine;

namespace Shizounu.Library.Dialogue.Data
{
    [CreateAssetMenu(fileName = "new Speaker", menuName = "Dialogue/Speaker")]
    public class Speaker : ScriptableObject
    {
        public string Name = "new Speaker";
        public Color NameColor = Color.white;
        public float SpeechSpeed = 1.5f;
        [Range(0f, 2f)] public float SpeechSpeedWiggle = 0.25f; 
    }
}
