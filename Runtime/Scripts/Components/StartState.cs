using UnityEngine;

namespace CrossingLears
{

    public class StartState : MonoBehaviour
    {
        public enum StartStateEnum
        {
            Close,
            Open,
            Destroy,
        }

        public StartStateEnum startState;
    }
}
