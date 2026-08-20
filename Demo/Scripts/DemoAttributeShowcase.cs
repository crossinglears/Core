using UnityEngine;

namespace CrossingLears
{
    public class DemoAttributeShowcase : MonoBehaviour
    {
        [ReadOnly]
        public string readOnlyStatus = "This field is read-only in the Inspector.";

        [SerializeField]
        private int clickCount;

        [Button("Log Demo Message")]
        private void LogDemoMessage()
        {
            clickCount++;
            readOnlyStatus = "Button clicked " + clickCount + " time(s).";
            Debug.Log("[Crossing Lears Core Demo] " + readOnlyStatus);
        }

        [Button]
        private void ResetDemoState()
        {
            clickCount = 0;
            readOnlyStatus = "This field is read-only in the Inspector.";
        }
    }
}
