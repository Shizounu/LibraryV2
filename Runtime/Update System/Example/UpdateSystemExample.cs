using UnityEngine;
using Shizounu.Library.UpdateSystem;

namespace Shizounu.Library.UpdateSystem.Example
{
    public class UpdateSystemExample : MonoBehaviour
    {
        void Awake()
        {

        }

        public void OnSecond(float deltaTime, UpdateContext context)
        {
            Debug.Log($"<color=red>1 Second called! DeltaTime: {deltaTime}, ThreadId: {context.ThreadId}, IsMainThread: {context.IsMainThread}</color>");
        }
        public void On10Seconds(float deltaTime, UpdateContext context)
        {
            Debug.Log($"<color=green>10 Seconds called! DeltaTime: {deltaTime}, ThreadId: {context.ThreadId}, IsMainThread: {context.IsMainThread}</color>");
        }
        public void OnTenthSecond(float deltaTime, UpdateContext context)
        {
            Debug.Log($"<color=blue>.1 Seconds called! DeltaTime: {deltaTime}, ThreadId: {context.ThreadId}, IsMainThread: {context.IsMainThread}</color>");
        }

        void OnEnable()
        {
            UpdateSystem.Instance.RegisterCallback(OnSecond, 1.0f, UpdateThreading.MainThread);
            UpdateSystem.Instance.RegisterCallback(On10Seconds, 10.0f, UpdateThreading.MainThread);
            UpdateSystem.Instance.RegisterCallback(OnTenthSecond, 0.1f, UpdateThreading.MainThread);
        }
        void OnDisable()
        {
            UpdateSystem.Instance.UnregisterCallback(OnSecond, 1.0f, UpdateThreading.MainThread);
            UpdateSystem.Instance.UnregisterCallback(On10Seconds, 10.0f, UpdateThreading.MainThread);
            UpdateSystem.Instance.UnregisterCallback(OnTenthSecond, 0.1f, UpdateThreading.MainThread);
        }
    }
}
