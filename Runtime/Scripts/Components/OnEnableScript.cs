using UnityEngine;
using UnityEngine.Events;

namespace CrossingLears
{
    public class OnEnableScript : MonoBehaviour 
    {
        public UnityEvent onEnable;
        public UnityEvent onDisable;

        void OnEnable()
        {
            onEnable.Invoke();
        }
        void OnDisable()
        {
            onDisable.Invoke();
        }
    }
}