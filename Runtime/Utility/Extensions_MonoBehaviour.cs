using UnityEngine;
using System.Collections;

namespace Shizounu.Library.Utility
{
    public static class MonoBehaviourExtensions
    {
        public static void InvokeSafe(this MonoBehaviour behavior, System.Action method, float delayInSeconds)
        {
            behavior.StartCoroutine(InvokeSafeRoutine(method, delayInSeconds));
        }

        public static void InvokeRepeatingSafe(this MonoBehaviour behavior, System.Action method, float delayInSeconds, float repeatRateInSeconds)
        {
            behavior.StartCoroutine(InvokeSafeRepeatingRoutine(method, delayInSeconds, repeatRateInSeconds));
        }

        private static IEnumerator InvokeSafeRepeatingRoutine(System.Action method, float delayInSeconds, float repeatRateInSeconds)
        {
            yield return new WaitForSeconds(delayInSeconds);

            while (true)
            {
                if (method != null) method.Invoke();
                yield return new WaitForSeconds(repeatRateInSeconds);
            }
        }

        private static IEnumerator InvokeSafeRoutine(System.Action method, float delayInSeconds)
        {
            yield return new WaitForSeconds(delayInSeconds);
            if (method != null) method.Invoke();
        }
    }
}
